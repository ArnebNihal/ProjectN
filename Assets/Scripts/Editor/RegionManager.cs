using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.InternalTypes;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using Unity.Mathematics.Editor;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEditor.Build.Pipeline.Tasks;
using UnityEngine.EventSystems;
using Newtonsoft.Json;
using DaggerfallWorkshop.Game.Player;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Utility.AssetInjection;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game;
using UnityEditor.Build.Pipeline.Utilities;
using System.Text;

namespace MapEditor
{
    public class RegionManager : EditorWindow
    {
        static RegionManager regionManagerWindow;
        const string windowTitle = "Region Manager";
        public static int currentRegionIndex = 0;
        public string[] regionNames;
        public static RegionData currentRegionData;
        public bool geographyStats = false;
        public bool politicalStats = false;
        public bool climateStats = false;
        public bool diplomaticStats = false;
        // FactionFile factionFile;
        public Dictionary<int, FactionFile.FactionData> factions;
        public Dictionary<string, int> factionToIdDict;
        FactionFile.FactionData newFaction;
        public static string modifiedRegionName;
        public static int factionId = 0;
        public const int oldBorderLimit = 11;
        public const int borderLimit = 20;

        public class RegionData
        {
            public string regionName;
            public (int, int) capitalPosition;
            public ushort governType;
            public int population;
            public ushort deity = 0;
            public int surface;
            public int elevationSum;
            public int highestPoint;
            public int lowestPoint = 4;
            public ulong highestLocationId = 0;
            public ulong lowestLocationId = 999999;
            public ushort[] locations; // contains count of each different kind of location, ordered as LocationTypes
            public int[] climates; // same as locations, but for climates
            public int[] regionBorders;

            public void SaveRegionChanges(int regionIndex)
            {
                WorldInfo.WorldSetting.regionRaces[regionIndex] = population;
                WorldInfo.WorldSetting.regionTemples[regionIndex] = deity;

                bool missing = true;
                bool room = false;
                int roomIndex = -1;

                for (int i = 0; i < borderLimit; i++)
                {
                    missing = true;
                    room = false;
                    roomIndex = -1;

                    WorldInfo.WorldSetting.regionBorders[regionIndex * borderLimit + i] = currentRegionData.regionBorders[i];

                    for (int j = 0; j < borderLimit; j++)
                    {
                        if (currentRegionData.regionBorders[i] == WorldInfo.WorldSetting.Regions)
                            break;

                        if (!room && WorldInfo.WorldSetting.regionBorders[currentRegionData.regionBorders[i] * borderLimit + j] == WorldInfo.WorldSetting.Regions)
                        {
                            roomIndex = currentRegionData.regionBorders[i] * borderLimit + j;
                            room = true;
                        }

                        if (missing && WorldInfo.WorldSetting.regionBorders[currentRegionData.regionBorders[i] * borderLimit + j] == regionIndex)
                        {
                            missing = false;
                        }
                    }

                    if (room && missing)
                    {
                        Debug.Log("roomIndex: " + roomIndex + ", regionIndex: " + regionIndex);
                        WorldInfo.WorldSetting.regionBorders[roomIndex] = regionIndex;
                    }
                }

                // for (int k = 0; k < borderLimit; k++)
                // {
                //     WorldInfo.WorldSetting.regionBorders[currentRegionIndex * borderLimit + k] = currentRegionData.regionBorders[k];
                // }

                string fileDataPath = Path.Combine(MapEditor.testPath, "WorldData.json");
                var json = JsonConvert.SerializeObject(WorldInfo.WorldSetting, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                File.WriteAllText(fileDataPath, json);

                WorldInfo.WorldSetting = new WorldStats();
                WorldInfo.WorldSetting = JsonConvert.DeserializeObject<WorldStats>(File.ReadAllText(Path.Combine(MapEditor.testPath, "WorldData.json")));
            }
        }

        public enum Ruler
        {
            None = 0,
            King = 1,
            Queen = 2,
            Duke = 3,
            Duchess = 4,
            Marquis = 5,
            Marquise = 6,
            Count = 7,
            Countess = 8,
            Baron = 9,
            Baroness = 10,
            Lord = 11,
            Lady = 12,
            Emperor = 13,
            Empress = 14,
        }

        // GovernType right now is used only for display in the different menus
        // It is equivalent to (Ruler + 1) / 2
        public enum GovernType
        {
            None = 0,
            Kingdom = 1,
            Duchy = 2,
            March = 3,
            County = 4,
            Barony = 5,
            Fiefdom = 6,
            Empire = 7,
        }

        void Update()
        {

        }

        void Awake()
        {
            // factionFile = DaggerfallUnity.Instance.ContentReader.FactionFileReader;
            factions = new Dictionary<int, FactionFile.FactionData>();
            factionToIdDict = new Dictionary<string, int>();
            factions = FactionAtlas.FactionDictionary;
            currentRegionIndex = 17;
            foreach (KeyValuePair<int, FactionFile.FactionData> faction in factions)
            {
                if (factionToIdDict.ContainsKey(faction.Value.name))
                {
                    string postfix = "(" + (factions[faction.Key].name.TrimStart(' ') + ")");
                    factionToIdDict.Add(faction.Value.name + " " + postfix, faction.Key);
                }
                else factionToIdDict.Add(faction.Value.name, faction.Key);
            }
            // modifiedRegionName = "Alik'ra";
            SetRegionNames();
            AnalyzeSelectedRegion();
        }

        void OnGUI()
        {
            int oldRegionIndex = currentRegionIndex;

            // modifiedRegionName = ConvertRegionName(regionNames[currentRegionIndex]);

            EditorGUILayout.BeginHorizontal();
            currentRegionIndex = EditorGUILayout.Popup("Region: ", currentRegionIndex, regionNames, GUILayout.MaxWidth(600.0f));
            GUILayout.Space(50.0f);
            if (GUILayout.Button("Create new region", GUILayout.MaxWidth(200.0f)))
            {
                OpenNewRegionWindow();
            }
            GUILayout.Space(50.0f);
            if (GUILayout.Button("Create stats table", GUILayout.MaxWidth(200.0f)))
            {
                StatsTable();
            }
            EditorGUILayout.EndHorizontal();
            if (currentRegionIndex != oldRegionIndex)
            {
                modifiedRegionName = ConvertRegionName(regionNames[currentRegionIndex]);
                AnalyzeSelectedRegion(currentRegionIndex);
                if (factionToIdDict.ContainsKey(modifiedRegionName))
                {
                    factionId = factionToIdDict[modifiedRegionName];
                    FactionAtlas.GetFactionData(factionId, out newFaction);
                }
            }

            GUILayout.Label(regionNames[currentRegionIndex], EditorStyles.boldLabel);
            GUILayout.Space(50.0f);
            currentRegionData.regionName = EditorGUILayout.TextField("Name", currentRegionData.regionName);
            EditorGUILayout.LabelField("Capital city: ", "");
            EditorGUILayout.LabelField("Govern Type: ", ((GovernType)((newFaction.ruler + 1) / 2)).ToString());
            EditorGUILayout.BeginHorizontal();
            currentRegionData.population = EditorGUILayout.IntField("Population", currentRegionData.population, GUILayout.MaxWidth(200.0f));
            EditorGUILayout.LabelField(" : ", ((DaggerfallWorkshop.Game.Entity.Races)currentRegionData.population + 1).ToString());
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            currentRegionData.deity = (ushort)EditorGUILayout.IntField("Deity: ", currentRegionData.deity, GUILayout.MaxWidth(200.0f));
            EditorGUILayout.LabelField(" : ", FactionAtlas.FactionDictionary[currentRegionData.deity].name);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(20.0f);

            geographyStats = EditorGUILayout.Foldout(geographyStats, "Geography");
            if (geographyStats)
            {
                EditorGUILayout.LabelField("Surface: ", currentRegionData.surface + " units");
                GUILayout.Space(20.0f);

                if (currentRegionData.surface != 0)
                {
                    EditorGUILayout.LabelField("Average Elevation: ", (currentRegionData.elevationSum / currentRegionData.surface).ToString());
                    EditorGUILayout.LabelField("Highest Point: ", (currentRegionData.highestPoint).ToString());
                    EditorGUILayout.LabelField("Lowest Point: ", (currentRegionData.lowestPoint).ToString());
                }
                else
                {
                    EditorGUILayout.LabelField("Average Elevation: ", "0");
                    EditorGUILayout.LabelField("Highest Point: ", "N/A");
                    EditorGUILayout.LabelField("Lowest Point: ", "N/A");
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Region Borders: ", "");

                for (int i = 0; i < borderLimit; i++)
                {
                    int regionBorder = -1;

                    if (i % 5 == 0)
                    {
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.BeginHorizontal();
                    }

                    if (currentRegionData.regionBorders[i] > -1)
                        regionBorder = (currentRegionData.regionBorders[i]);
                    else regionBorder = WorldInfo.WorldSetting.Regions;

                    currentRegionData.regionBorders[i] = (EditorGUILayout.Popup("", regionBorder, regionNames, GUILayout.MaxWidth(100.0f)));
                }
                EditorGUILayout.EndHorizontal();
            }            
            GUILayout.Space(30.0f);

            politicalStats = EditorGUILayout.Foldout(politicalStats, "Politics");
            if (politicalStats)
            {
                if (currentRegionData.surface != 0)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Cities: ", currentRegionData.locations[(int)DFRegion.LocationTypes.TownCity].ToString());
                    EditorGUILayout.LabelField(((float)currentRegionData.locations[(int)DFRegion.LocationTypes.TownCity] * 100.0f / (float)currentRegionData.surface).ToString(), "%");
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Hamlets: ", currentRegionData.locations[(int)DFRegion.LocationTypes.TownHamlet].ToString());
                    EditorGUILayout.LabelField(((float)currentRegionData.locations[(int)DFRegion.LocationTypes.TownHamlet] * 100.0f / (float)currentRegionData.surface).ToString(), "%");
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Villages: ", currentRegionData.locations[(int)DFRegion.LocationTypes.TownVillage].ToString());
                    EditorGUILayout.LabelField(((float)currentRegionData.locations[(int)DFRegion.LocationTypes.TownVillage] * 100.0f / (float)currentRegionData.surface).ToString(), "%");
                    GUILayout.EndHorizontal();
                    GUILayout.Space(20.0f);
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Wealthy Homes: ", currentRegionData.locations[(int)DFRegion.LocationTypes.HomeWealthy].ToString());
                    EditorGUILayout.LabelField(((float)currentRegionData.locations[(int)DFRegion.LocationTypes.HomeWealthy] * 100.0f / (float)currentRegionData.surface).ToString(), "%");
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Hovels: ", currentRegionData.locations[(int)DFRegion.LocationTypes.HomePoor].ToString());
                    EditorGUILayout.LabelField(((float)currentRegionData.locations[(int)DFRegion.LocationTypes.HomePoor] * 100.0f / (float)currentRegionData.surface).ToString(), "%");
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Taverns: ", currentRegionData.locations[(int)DFRegion.LocationTypes.Tavern].ToString());
                    EditorGUILayout.LabelField(((float)currentRegionData.locations[(int)DFRegion.LocationTypes.Tavern] * 100.0f / (float)currentRegionData.surface).ToString(), "%");
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Farms: ", currentRegionData.locations[(int)DFRegion.LocationTypes.HomeFarms].ToString());
                    EditorGUILayout.LabelField(((float)currentRegionData.locations[(int)DFRegion.LocationTypes.HomeFarms] * 100.0f / (float)currentRegionData.surface).ToString(), "%");
                    GUILayout.EndHorizontal();
                    GUILayout.Space(20.0f);
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Temples: ", currentRegionData.locations[(int)DFRegion.LocationTypes.ReligionTemple].ToString());
                    EditorGUILayout.LabelField(((float)currentRegionData.locations[(int)DFRegion.LocationTypes.ReligionTemple] * 100.0f / (float)currentRegionData.surface).ToString(), "%");
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Cult Sites: ", currentRegionData.locations[(int)DFRegion.LocationTypes.ReligionCult].ToString());
                    EditorGUILayout.LabelField(((float)currentRegionData.locations[(int)DFRegion.LocationTypes.ReligionCult] * 100.0f / (float)currentRegionData.surface).ToString(), "%");
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Witch Covens: ", currentRegionData.locations[(int)DFRegion.LocationTypes.Coven].ToString());
                    EditorGUILayout.LabelField(((float)currentRegionData.locations[(int)DFRegion.LocationTypes.Coven] * 100.0f / (float)currentRegionData.surface).ToString(), "%");
                    GUILayout.EndHorizontal();
                    GUILayout.Space(20.0f);
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Large Dungeons: ", currentRegionData.locations[(int)DFRegion.LocationTypes.DungeonLabyrinth].ToString());
                    EditorGUILayout.LabelField(((float)currentRegionData.locations[(int)DFRegion.LocationTypes.DungeonLabyrinth] * 100.0f / (float)currentRegionData.surface).ToString(), "%");
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Medium Dungeons: ", currentRegionData.locations[(int)DFRegion.LocationTypes.DungeonKeep].ToString());
                    EditorGUILayout.LabelField(((float)currentRegionData.locations[(int)DFRegion.LocationTypes.DungeonKeep] * 100.0f / (float)currentRegionData.surface).ToString(), "%");
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Small Dungeons: ", currentRegionData.locations[(int)DFRegion.LocationTypes.DungeonRuin].ToString());
                    EditorGUILayout.LabelField(((float)currentRegionData.locations[(int)DFRegion.LocationTypes.DungeonRuin] * 100.0f / (float)currentRegionData.surface).ToString(), "%");
                    GUILayout.EndHorizontal();
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("Graveyards: ", currentRegionData.locations[(int)DFRegion.LocationTypes.Graveyard].ToString());
                    EditorGUILayout.LabelField(((float)currentRegionData.locations[(int)DFRegion.LocationTypes.Graveyard] * 100.0f / (float)currentRegionData.surface).ToString(), "%");
                    GUILayout.EndHorizontal();
                    GUILayout.Space(20.0f);
                    int allLocationsCount = 0;
                    for (int i = 0; i < Enum.GetNames(typeof(DFRegion.LocationTypes)).Length; i++)
                    {
                        allLocationsCount += currentRegionData.locations[i];
                    }
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("All Locations: ", allLocationsCount.ToString());
                    EditorGUILayout.LabelField(((float)allLocationsCount * 100.0f / (float)currentRegionData.surface).ToString(), "%");
                    GUILayout.EndHorizontal();
                    EditorGUILayout.LabelField("Location Id range: ", currentRegionData.lowestLocationId + " - " + currentRegionData.highestLocationId);
                }
                else
                {
                    EditorGUILayout.LabelField("Nothing to show", "");
                }
            }
            GUILayout.Space(30.0f);

            climateStats = EditorGUILayout.Foldout(climateStats, "Climate");
            if (climateStats)
            {
                if (currentRegionData.surface != 0)
                {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Ocean: ", "");
                EditorGUILayout.LabelField(((float)currentRegionData.climates[0] * 100.0f / (float)currentRegionData.surface).ToString(), "%");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Desert: ", "");
                EditorGUILayout.LabelField(((float)currentRegionData.climates[(int)MapsFile.Climates.Desert - (int)MapsFile.Climates.Ocean] * 100.0f / (float)currentRegionData.surface).ToString(), "%");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Desert2: ", "");
                EditorGUILayout.LabelField(((float)currentRegionData.climates[(int)MapsFile.Climates.Desert2 - (int)MapsFile.Climates.Ocean] * 100.0f / (float)currentRegionData.surface).ToString(), "%");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Mountain: ", "");
                EditorGUILayout.LabelField(((float)currentRegionData.climates[(int)MapsFile.Climates.Mountain - (int)MapsFile.Climates.Ocean] * 100.0f / (float)currentRegionData.surface).ToString(), "%");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Rainforest: ", "");
                EditorGUILayout.LabelField(((float)currentRegionData.climates[(int)MapsFile.Climates.Rainforest - (int)MapsFile.Climates.Ocean] * 100.0f / (float)currentRegionData.surface).ToString(), "%");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Swamp: ", "");
                EditorGUILayout.LabelField(((float)currentRegionData.climates[(int)MapsFile.Climates.Swamp - (int)MapsFile.Climates.Ocean] * 100.0f / (float)currentRegionData.surface).ToString(), "%");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Subtropical: ", "");
                EditorGUILayout.LabelField(((float)currentRegionData.climates[(int)MapsFile.Climates.Subtropical - (int)MapsFile.Climates.Ocean] * 100.0f / (float)currentRegionData.surface).ToString(), "%");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Mountain Woods: ", "");
                EditorGUILayout.LabelField(((float)currentRegionData.climates[(int)MapsFile.Climates.MountainWoods - (int)MapsFile.Climates.Ocean] * 100.0f / (float)currentRegionData.surface).ToString(), "%");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Woodlands: ", "");
                EditorGUILayout.LabelField(((float)currentRegionData.climates[(int)MapsFile.Climates.Woodlands - (int)MapsFile.Climates.Ocean] * 100.0f / (float)currentRegionData.surface).ToString(), "%");
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Haunted Woodlands: ", "");
                EditorGUILayout.LabelField(((float)currentRegionData.climates[(int)MapsFile.Climates.HauntedWoodlands - (int)MapsFile.Climates.Ocean] * 100.0f / (float)currentRegionData.surface).ToString(), "%");
                GUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.LabelField("Nothing to show", "");
                }
            }
            GUILayout.Space(30.0f);

            if (GUILayout.Button("Generate Locations", GUILayout.MaxWidth(200.0f)))
            {
                OpenGenerateLocationsWindow();
            }

            // diplomaticStats = EditorGUILayout.Foldout(diplomaticStats, "Diplomatic status");
            // if (diplomaticStats)
            // {
            //     if (factionToIdDict.ContainsKey(modifiedRegionName))
            //     {
            //         EditorGUILayout.LabelField("Faction Id: ", factionId.ToString());
            //         EditorGUILayout.LabelField("Parent: ", ((FactionFile.FactionIDs)newFaction.parent).ToString());
            //         EditorGUILayout.LabelField("Type: ", ((FactionFile.FactionTypes)newFaction.type).ToString());
            //         EditorGUILayout.LabelField("Name: ", newFaction.name);
            //         EditorGUILayout.LabelField("Reputation: ", newFaction.rep.ToString());
            //         newFaction.summon = EditorGUILayout.IntField("Summon: ", newFaction.summon);
            //         EditorGUILayout.LabelField("Region: ", (WorldInfo.WorldSetting.RegionNames[newFaction.region]));
            //         newFaction.power = EditorGUILayout.IntField("Power: ", newFaction.power);
            //         EditorGUILayout.LabelField("Flags: ", newFaction.flags.ToString());
            //         EditorGUILayout.LabelField("Ruler: ", ((Ruler)newFaction.ruler).ToString());
            //         EditorGUILayout.LabelField("Ally1: ", ((FactionFile.FactionIDs)newFaction.ally1).ToString());
            //         EditorGUILayout.LabelField("Ally2: ", ((FactionFile.FactionIDs)newFaction.ally2).ToString());
            //         EditorGUILayout.LabelField("Ally3: ", ((FactionFile.FactionIDs)newFaction.ally3).ToString());
            //         EditorGUILayout.LabelField("Enemy1: ", ((FactionFile.FactionIDs)newFaction.enemy1).ToString());
            //         EditorGUILayout.LabelField("Enemy2: ", ((FactionFile.FactionIDs)newFaction.enemy2).ToString());
            //         EditorGUILayout.LabelField("Enemy3: ", ((FactionFile.FactionIDs)newFaction.enemy3).ToString());
            //         EditorGUILayout.LabelField("Face: ", (newFaction.face).ToString());
            //         EditorGUILayout.LabelField("Race: ", (newFaction.race).ToString());
            //         EditorGUILayout.LabelField("Flat1: ", (newFaction.flat1).ToString());
            //         EditorGUILayout.LabelField("Flat2: ", (newFaction.flat2).ToString());
            //         EditorGUILayout.LabelField("Sgroup: ", ((FactionFile.SocialGroups)newFaction.sgroup).ToString());
            //         EditorGUILayout.LabelField("Ggroup: ", ((FactionFile.GuildGroups)newFaction.ggroup).ToString());
            //         EditorGUILayout.LabelField("Minf: ", (newFaction.minf).ToString());
            //         EditorGUILayout.LabelField("Maxf: ", (newFaction.maxf).ToString());
            //         EditorGUILayout.LabelField("Vam: ", ((FactionFile.FactionIDs)newFaction.vam).ToString());
            //         EditorGUILayout.LabelField("Rank: ", (newFaction.rank).ToString());
            //         EditorGUILayout.LabelField("Ruler name seed: ", (newFaction.rulerNameSeed).ToString());
            //         EditorGUILayout.LabelField("Ruler power bonus: ", (newFaction.rulerPowerBonus).ToString());
            //         EditorGUILayout.LabelField("Next faction: ", (newFaction.ptrToNextFactionAtSameHierarchyLevel).ToString());
            //         EditorGUILayout.LabelField("First child faction: ", (newFaction.ptrToFirstChildFaction).ToString());
            //         EditorGUILayout.LabelField("Parent faction: ", (newFaction.ptrToParentFaction).ToString());
            //     }
            //     else
            //     {
            //         EditorGUILayout.LabelField("Nothing to show", "");
            //     }
            // }

            if (GUILayout.Button("Save Changes", GUILayout.MaxWidth(100)))
            {
                currentRegionData.SaveRegionChanges(currentRegionIndex);
            }
        }

        protected void CreateFactionJson()
        {
            string fileDataPath = Path.Combine(MapEditor.testPath, "Faction.json");
            var json = JsonConvert.SerializeObject(factions, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            File.WriteAllText(fileDataPath, json);
            // Dictionary<int, FactionFile.FactionData> factionJson = new Dictionary<int, FactionFile.FactionData>();
            // for (int i = 0; i < 1000; i++)
            // {
            //     if (!factions.ContainsKey(i))
            //         continue;

            //     factionFile.GetFactionData(i, out )

            //     if (factionToIdDict.ContainsKey(modifiedRegionName))
            //     {
            //         factionId = factionToIdDict[modifiedRegionName];
            //         factionFile.GetFactionData(factionId, out newFaction);
            //     }
            //     else{
            //         newFaction.id = i + 1;
            //         newFaction.parent = 0;
            //         newFaction.type = 
            //     }
            // }
        }

        protected void SetRegionNames()
        {
            regionNames = new string[WorldInfo.WorldSetting.Regions + 1];

            for (int i = 0; i < WorldInfo.WorldSetting.Regions; i++)
            {
                regionNames[i] = WorldInfo.WorldSetting.RegionNames[i];
            }

            regionNames[WorldInfo.WorldSetting.Regions] = "None";
        }

        public static string ConvertRegionName(string name)
        {
            switch (name)
            {
                case "Alik'r Desert":
                    return "Alik'ra";
                    break;

                case "Dragontail Mountains":
                    return "Dragontail";
                    break;

                case "Wrothgarian Mountains":
                    return "Wrothgaria";
                    break;

                case "Orsinium Area":
                    return "Orsinium";
                    break;

                default:
                    break;
            }
            return name;
        }

        protected void AnalyzeSelectedRegion(int regionIndex = 17)
        {
            Debug.Log("Performing region analysis on regionIndex " + regionIndex);

            currentRegionData = new RegionData();
            currentRegionData.regionName = regionNames[regionIndex];
            currentRegionData.governType = (ushort)factions[factionToIdDict[modifiedRegionName]].ruler;
            currentRegionData.population = WorldInfo.WorldSetting.regionRaces[regionIndex];
            currentRegionData.deity = (ushort)WorldInfo.WorldSetting.regionTemples[regionIndex];
            currentRegionData.locations = new ushort[Enum.GetNames(typeof(DFRegion.LocationTypes)).Length];
            currentRegionData.climates = new int[Enum.GetNames(typeof(MapsFile.Climates)).Length];

            for (int x = 0; x < MapsFile.MaxMapPixelX; x++)
            {
                for (int y = 0; y < MapsFile.MaxMapPixelY; y++)
                {
                    int index = PoliticInfo.ConvertMapPixelToRegionIndex(x, y);

                    if (index == regionIndex)
                    {
                        currentRegionData.surface++;
                        currentRegionData.elevationSum += (int)SmallHeightmap.GetHeightMapValue(x, y);
                        if ((int)SmallHeightmap.GetHeightMapValue(x, y) > currentRegionData.highestPoint)
                            currentRegionData.highestPoint = (int)SmallHeightmap.GetHeightMapValue(x, y);
                        if ((int)SmallHeightmap.GetHeightMapValue(x, y) < currentRegionData.lowestPoint)
                            currentRegionData.lowestPoint = (int)SmallHeightmap.GetHeightMapValue(x, y);

                        if (Worldmaps.HasLocation(x, y))
                        {
                            MapSummary summary = new MapSummary();
                            Worldmaps.HasLocation(x, y, out summary);

                            currentRegionData.locations[(int)summary.LocationType]++;

                            if (summary.MapID > currentRegionData.highestLocationId)
                                currentRegionData.highestLocationId = summary.MapID;
                            if (summary.MapID < currentRegionData.lowestLocationId)
                                currentRegionData.lowestLocationId = summary.MapID;
                        }
                        currentRegionData.climates[ClimateInfo.Climate[x, y] - (int)MapsFile.Climates.Ocean]++;
                        // if (ClimateInfo.Climate[x, y] - (int)MapsFile.Climates.Ocean == 1)
                        //     Debug.Log("Desert: " + currentRegionData.climates[1] + ", surface: " + currentRegionData.surface);
                    }
                }
            }

            currentRegionData.regionBorders = new int[borderLimit];
            for (int i = 0; i < borderLimit; i++)
            {
                currentRegionData.regionBorders[i] = WorldInfo.WorldSetting.regionBorders[borderLimit * regionIndex + i];
            }
        }

        protected void OpenNewRegionWindow()
        {
            NewRegionWindow newRegionWindow = (NewRegionWindow) EditorWindow.GetWindow(typeof(NewRegionWindow), false, "New Region");
            newRegionWindow.Show();
        }

        protected void OpenGenerateLocationsWindow()
        {
            GenerateLocationsWindow generateLocationsWindow = (GenerateLocationsWindow) EditorWindow.GetWindow(typeof(GenerateLocationsWindow), false, "Locations Generator");
            generateLocationsWindow.Show();
        }

        protected void StatsTable()
        {
            RegionData[] dataTable = new RegionData[WorldInfo.WorldSetting.Regions];

            float[] regionData = new float[25];
            for (int i = 0; i < 62; i++)
            {
                AnalyzeSelectedRegion(i);

                if (currentRegionData.surface == 0)
                    continue;

                string dataPath = Path.Combine(MapEditor.testPath, "Data", "region" + i.ToString() + "A.json");
                var json = JsonConvert.SerializeObject(currentRegionData, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                File.WriteAllText(dataPath, json);

                int allLocationsCount = 0;

                for (int j = 0; j < 14; j++)
                {
                    regionData[j] = (float)currentRegionData.locations[j] * 100.0f / (float)currentRegionData.surface;
                    allLocationsCount += currentRegionData.locations[j];
                }

                regionData[14] = (float)allLocationsCount * 100.0f / (float)currentRegionData.surface;

                for (int k = 15; k < 25; k++)
                {
                    regionData[k] = (float)currentRegionData.climates[k - 15] * 100.0f / (float)currentRegionData.surface;
                }
                
                dataPath = Path.Combine(MapEditor.testPath, "Data", "region" + i.ToString() + "B.json");
                json = JsonConvert.SerializeObject(regionData, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                File.WriteAllText(dataPath, json);
            }
        }
    }

    public class NewRegionWindow : EditorWindow
    {
        static NewRegionWindow newRegionWindow;
        const string windowTitle = "New Region";
        RegionManager.RegionData newRegionData;
        static int newRegionIndex;
        static int tempDeity;

        void Awake()
        {
            newRegionData = new RegionManager.RegionData();
            tempDeity = 0;
        }

        void OnGUI()
        {
            newRegionIndex = GetNextRegionIndex();
            EditorGUILayout.LabelField("Region Index: ", newRegionIndex.ToString());
            newRegionData.regionName = EditorGUILayout.TextField("Name: ", newRegionData.regionName);
            EditorGUILayout.LabelField("Capital city: ", "");
            newRegionData.governType = (ushort)EditorGUILayout.Popup("Govern type: ", newRegionData.governType, Enum.GetNames(typeof(RegionManager.Ruler)), GUILayout.MaxWidth(200.0f)); // refer to https://en.uesp.net/wiki/Daggerfall_Mod:FACTION.TXT/Ruler
            newRegionData.population = EditorGUILayout.IntField("Population: ", newRegionData.population); // right now this is simply used with each digit pointing to a certain race, from the most to the least common
            tempDeity = (ushort)EditorGUILayout.Popup("Deity: ", tempDeity, FactionManager.factionNames[(int)FactionFile.FactionTypes.Temple], GUILayout.MaxWidth(250.0f)); // int points to the faction Id of the main deity for this region
            newRegionData.deity = (ushort)FactionAtlas.FactionToId[FactionManager.factionNames[(int)FactionFile.FactionTypes.Temple][tempDeity]];
            EditorGUILayout.Space(); 



            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Region", GUILayout.MaxWidth(100)))
            {
                AddNewRegion(newRegionData);
            }

            if (GUILayout.Button("Cancel", GUILayout.MaxWidth(100)))
            {
                newRegionWindow.Close();
            }
            EditorGUILayout.EndHorizontal();
        }

        protected int GetNextRegionIndex()
        {
            for (int i = 61; i < WorldInfo.WorldSetting.Regions; i++)
            {
                if (!FactionAtlas.FactionToId.ContainsKey(RegionManager.ConvertRegionName(WorldInfo.WorldSetting.RegionNames[i])))
                    return i;
            }

            return WorldInfo.WorldSetting.Regions;
        }

        protected void AddNewRegion(RegionManager.RegionData region)
        {
            WorldStats modifiedWorldInfo = new WorldStats();

            if (newRegionIndex >= WorldInfo.WorldSetting.Regions)
                modifiedWorldInfo.Regions = WorldInfo.WorldSetting.Regions + 1;
            else modifiedWorldInfo.Regions = WorldInfo.WorldSetting.Regions;

            modifiedWorldInfo.RegionNames = new string[modifiedWorldInfo.Regions];

            for (int i = 0; i < modifiedWorldInfo.Regions; i++)
            {
                if (i == newRegionIndex)
                    modifiedWorldInfo.RegionNames[i] = region.regionName;
                else modifiedWorldInfo.RegionNames[i] = WorldInfo.WorldSetting.RegionNames[i];
            }

            modifiedWorldInfo.regionRaces = new int[modifiedWorldInfo.Regions];

            for (int j = 0; j < modifiedWorldInfo.Regions; j++)
            {
                if (j == newRegionIndex)
                    modifiedWorldInfo.regionRaces[j] = region.population;
                else modifiedWorldInfo.regionRaces[j] = WorldInfo.WorldSetting.regionRaces[j];
            }

            modifiedWorldInfo.regionTemples = new int[modifiedWorldInfo.Regions];

            for (int k = 0; k < modifiedWorldInfo.Regions; k++)
            {
                if (k == newRegionIndex)
                    modifiedWorldInfo.regionTemples[k] = region.deity;
                else modifiedWorldInfo.regionTemples[k] = WorldInfo.WorldSetting.regionTemples[k];
            }

            modifiedWorldInfo.regionBorders = new int[modifiedWorldInfo.Regions * RegionManager.borderLimit];

            for (int m = 0; m < modifiedWorldInfo.Regions; m++)
            {
                if (m == newRegionIndex)
                {
                    for (int n = 0; n < RegionManager.borderLimit; n++)
                    {
                        modifiedWorldInfo.regionBorders[newRegionIndex * RegionManager.borderLimit + n] = 0;
                    }                    
                }
                else
                {
                    for (int o = 0; o < RegionManager.borderLimit; o++)
                    {
                        modifiedWorldInfo.regionBorders[m * RegionManager.borderLimit + o] = WorldInfo.WorldSetting.regionBorders[m * RegionManager.borderLimit + o];
                    }
                }
            }

            string fileDataPath = Path.Combine(MapEditor.testPath, "WorldData.json");
            var json = JsonConvert.SerializeObject(modifiedWorldInfo, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            File.WriteAllText(fileDataPath, json);

            // Dictionary<int, FactionFile.FactionData> factions = new Dictionary<int, FactionFile.FactionData>();
            // Dictionary<string, int> factionsToId = new Dictionary<string, int>();

            // foreach (KeyValuePair<int, FactionFile.FactionData> faction in FactionAtlas.FactionDictionary)
            // {
            //     factions.Add(faction);
            //     factionsToId.Add(faction.Value.name, faction.Key);
            // }

            // Automatically adds 3 factions: the region (0), its court (1) and its people (2); finer details about
            // these factions should be set through the faction editor.
            for (int l = 0; l <= 2; l++)
            {
                FactionFile.FactionData newFaction = new FactionFile.FactionData();
                newFaction = GetGenericRegionFaction((ushort)l, region);
                FactionAtlas.FactionDictionary.Add(newFaction.id, newFaction);
                FactionAtlas.FactionToId.Add(newFaction.name, newFaction.id);
            }

            fileDataPath = Path.Combine(MapEditor.testPath, "Faction.json");
            json = JsonConvert.SerializeObject(FactionAtlas.FactionDictionary, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            File.WriteAllText(fileDataPath, json);

            fileDataPath = Path.Combine(MapEditor.testPath, "FactionToId.json");
            json = JsonConvert.SerializeObject(FactionAtlas.FactionToId, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            File.WriteAllText(fileDataPath, json);

            WorldInfo.WorldSetting = new WorldStats();
            WorldInfo.WorldSetting = JsonConvert.DeserializeObject<WorldStats>(File.ReadAllText(Path.Combine(MapEditor.testPath, "WorldData.json")));
        }

        protected FactionFile.FactionData GetGenericRegionFaction(ushort typeOfFaction, RegionManager.RegionData region)
        {
            FactionFile.FactionData newFaction = new FactionFile.FactionData();

            newFaction.id = GetAvailableFactionId(FactionAtlas.FactionDictionary);

            if (typeOfFaction == 0)
            {
                newFaction.parent = 0;
                newFaction.type = (int)FactionFile.FactionTypes.Province;
                newFaction.name = region.regionName;
                newFaction.ruler = (int)region.governType;
                newFaction.flat1 = 23342;
                newFaction.flat2 = 23435;
                newFaction.sgroup = 3;
                newFaction.ggroup = 15;
                newFaction.minf = 20;
                newFaction.maxf = 60;
                newFaction.vam = 150;
                newFaction.children = new List<int>();
                newFaction.children.Add(GetAvailableFactionId(FactionAtlas.FactionDictionary, newFaction.id + 1));
                newFaction.children.Add(GetAvailableFactionId(FactionAtlas.FactionDictionary, newFaction.children[newFaction.children.Count - 1] + 1));
            }

            else if (typeOfFaction == 1 || typeOfFaction == 2)
            {
                newFaction.parent = FactionAtlas.FactionToId[region.regionName];
                newFaction.type = 13 + typeOfFaction;

                if (typeOfFaction == 1)
                {
                    newFaction.name = "Court of " + region.regionName;
                    newFaction.flat1 = 23427;
                    newFaction.flat2 = 23324;
                    newFaction.sgroup = 3;
                    newFaction.ggroup = 15;
                    newFaction.minf = 20;
                    newFaction.maxf = 80;
                }
                else 
                {
                    newFaction.name = "People of " + region.regionName;
                    newFaction.flat1 = 23316;
                    newFaction.flat2 = 23324;
                    newFaction.sgroup = 0;
                    newFaction.ggroup = 4;
                    newFaction.minf = 5;
                    newFaction.maxf = 30;
                }

                newFaction.ruler = 0;
                newFaction.vam = 0;
                newFaction.children = null;
            }
            
            newFaction.rep = 0;
            newFaction.summon = -1;
            newFaction.region = newRegionIndex;
            newFaction.power = 0;
            newFaction.flags = 0;

            newFaction.ally1 = newFaction.ally2 = newFaction.ally3 = 0;
            newFaction.enemy1 = newFaction.enemy2 = newFaction.enemy3 = 0;
            newFaction.face = -1;
            newFaction.race = region.population;            
            
            newFaction.rank = 0;
            newFaction.rulerNameSeed = 0;
            newFaction.rulerPowerBonus = 0;
            newFaction.ptrToNextFactionAtSameHierarchyLevel = 0;
            newFaction.ptrToFirstChildFaction = 0;
            newFaction.ptrToParentFaction = 0;           

            return newFaction;
        }

        protected int GetAvailableFactionId(Dictionary<int, FactionFile.FactionData> factions, int startingFrom = 201)
        {
            for (int i = startingFrom; i < int.MaxValue; i ++)
            {
                if (!factions.ContainsKey(i))
                    return i;
            }

            return -1;
        }
    }
}