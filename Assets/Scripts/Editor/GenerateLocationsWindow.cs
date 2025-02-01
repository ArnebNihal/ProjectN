using System;
using System.Collections.Generic;
using System.IO;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop;
using System.Linq;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Serialization;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

namespace MapEditor
{
    public class GenerateLocationsWindow : EditorWindow
    {
        static GenerateLocationsWindow generateLocationsWindow;
        const string windowTitle = "Locations Generator";
        public static GenOptions regionOptions;
        public const float distTolerance = 1.5f;
        public static List<ushort> locationIdList = new List<ushort>();

        // How much the major regionalism weigh on the total name pool, as a percentage;
        // minor regionalisms are then (100 - majorRegionalismChance).
        public const int majorRegionalismChance = 90;
        public const int hammerfellTownVowel = 7; // number of "vowels" used in generating the second part of Hammerfell town names
        public const int genericDungeonEpithets = 19;
        public static int[] foolRegions = { 2, 3, 4, 6, 7, 8, 10, 12, 13, 14, 15, 24, 25, 27, 28, 29, 30, 31 };
        public static string[] regionalismAreas;

        #region Dungeon Exteriors

        public static string[][] castleRMB = { 
            new string[] { "CASTAA09", "CASTAA07" },              // small
            new string[] { "CASTAA00", "CASTAA01" },              // medium
            new string[] { "CASTAA03", "CASTAA04", "CASTAA05" },  // large
            new string[] { "CUSTAA09", "CUSTAA13" } };            // special rules
        public static string[][] towerRMB = {
            new string[] { "CASTAA29", "CASTAA34" },
            new string[] { "CASTAA22", "CARNAA15" },
            new string[] { "CASTAA23" }
        };
        public static string[][] harpyRMB = {
            new string[] { "DUNGAA01" }
        };
        public static string[][] caveRMB = {
            new string[] { "DUNGAA02", "DUNGAA03", "DUNGAA05", "DUNGAA09", "DUNGAA12", "DUNGAA14", "DUNGAA17" },
            new string[] { "DUNGAA06", "DUNGAA08", "DUNGAA10", "DUNGAA13", "DUNGAA15", "DUNGAA16", "DUNGAA18" },
            new string[] { "DUNGAA01", "DUNGAA04", "DUNGAA07", "DUNGAA11" }
        };
        public static string[][] hauntRMB = { 
            new string[] { "CARNAA02", "CARNAA04", "CARNAA05", "CARNAA06", "CARNAA16" },
            new string[] { "CARNAA01" },
            new string[] { "CARNAA14", "CARNAA15", "CARNAA24" }
        };
        public static string[][] assemblyRMB = {
            new string[] { "DUNGAA02", "DUNGAA03", "DUNGAA04", "DUNGAA05", "DUNGAA06", "DUNGAA07", "DUNGAA08", "DUNGAA09", "DUNGAA10", "DUNGAA11", "DUNGAA12", "DUNGAA13", "DUNGAA14", "DUNGAA15", "DUNGAA16", "DUNGAA17", "DUNGAA18", "RUINAA01", "RUINAA02", "RUINAA03", "RUINAA04", "RUINAA05", "RUINAA06", "RUINAA07", "RUINAA08", "RUINAA09", "RUINAA10", "RUINAA11", "RUINAA12", "RUINAA13", "RUINAA14", "RUINAA15", "RUINAA16", "RUINAA17", "RUINAA18", "RUINAA19", "RUINAA20", "RUINAA21", "RUINAA22", "RUINAA23", "RUINAA24", "RUINAA25", "RUINAA26", "RUINAA27", "RUINAA28" },
            new string[] { "CARNAA01", "CARNAA02", "CARNAA04", "CARNAA05", "CARNAA06", "CARNAA14", "CARNAA15", "CARNAA16", "CARNAA24" },
            new string[] { "CASTAA06", "CASTAA08", "CASTAA22", "CASTAA24", "CASTAA25", "CASTAA29", "CASTAA33", "CASTAA34" }
        };
        public static string[][] covenRMB = {
            new string[] { "DUNGAA02", "DUNGAA03", "DUNGAA04", "DUNGAA05", "DUNGAA06", "DUNGAA07", "DUNGAA08", "DUNGAA09", "DUNGAA10", "DUNGAA11", "DUNGAA12", "DUNGAA13", "DUNGAA14", "DUNGAA15", "DUNGAA16", "DUNGAA17", "DUNGAA18" },
            new string[] { "RUINAA01", "RUINAA02", "RUINAA03", "RUINAA04", "RUINAA05", "RUINAA06", "RUINAA07", "RUINAA08", "RUINAA09", "RUINAA10", "RUINAA11", "RUINAA12", "RUINAA13", "RUINAA14", "RUINAA15", "RUINAA16", "RUINAA17", "RUINAA18", "RUINAA19", "RUINAA20", "RUINAA21", "RUINAA22", "RUINAA23", "RUINAA24", "RUINAA25", "RUINAA26", "RUINAA27", "RUINAA28" }
        };
        public static string[][] laboratoryRMB = {
            new string[] { "DUNGAA02", "DUNGAA03", "DUNGAA04", "DUNGAA05", "DUNGAA06", "DUNGAA07", "DUNGAA08", "DUNGAA09", "DUNGAA10", "DUNGAA11", "DUNGAA12", "DUNGAA13", "DUNGAA14", "DUNGAA15", "DUNGAA16", "DUNGAA17", "DUNGAA18", "RUINAA01", "RUINAA02", "RUINAA03", "RUINAA04", "RUINAA05", "RUINAA06", "RUINAA07", "RUINAA08", "RUINAA09", "RUINAA10", "RUINAA11", "RUINAA12", "RUINAA13", "RUINAA14", "RUINAA15", "RUINAA16", "RUINAA17", "RUINAA18", "RUINAA19", "RUINAA20", "RUINAA21", "RUINAA22", "RUINAA23", "RUINAA24", "RUINAA25", "RUINAA26", "RUINAA27", "RUINAA28" },
            new string[] { "CARNAA01", "CARNAA02", "CARNAA04", "CARNAA05", "CARNAA06", "CARNAA14", "CARNAA15", "CARNAA16", "CARNAA24" },
            new string[] { "CASTAA07", "CASTAA09" }
        };
        public static string[][] cryptRMB = {
            new string[] { "GRVEAS00", "GRVEAS02", "GRVEAS03", "GRVEAS04", "GRVEAS09", "GRVEAS11", "GRVEAS13", "GRVEAS14", "GRVEAS19", "GRVEAS21", "GRVEAS26", "GRVEAS27", "GRVEAS30", "GRVEAS31", "GRVEAS32", "GRVEAS33", "GRVEAS34", "GRVEAS35", "GRVEAS37", "GRVEAS40" },
            new string[] { "GRVEAS01", "GRVEAS05", "GRVEAS08", "GRVEAS10", "GRVEAS12", "GRVEAS15", "GRVEAS16", "GRVEAS17", "GRVEAS18", "GRVEAS20", "GRVEAS23", "GRVEAS24", "GRVEAS25", "GRVEAS28", "GRVEAS29", "GRVEAS36", "GRVEAS38", "GRVEAS39", "GRVEAS41" },
            new string[] { "GRVEAS06", "GRVEAS07", "GRVEAS22", "GRVEAS42" }
        };
        public static string[][] prisonRMB = {
            new string[] { "CASTAA09" },
            new string[] { "CASTAA00", "CASTAA01" },
            new string[] { "CASTAA05" }
        };
        public static string[][] ruinsRMB = {
            new string[] { "CASTAA06", "CASTAA08", "CASTAA10", "CASTAA18", "CASTAA19", "CASTAA29", "CASTAA32", "CASTAA33" },
            new string[] { "CASTAA02", "CASTAA13", "CASTAA17", "CARNAA02", "CARNAA04", "CARNAA05", "CARNAA14" },
            new string[] { "CARNAA05" }
        };
        public static string[][] towerRuinRMB = {
            new string[] { "CASTAA21", "CASTAA22", "CASTAA27", "CASTAA28", "CASTAA29", "CASTAA30", "CASTAA34", "CARNAA15" },
            new string[] { "CASTAA20", "CASTAA21", "CASTAA22", "CASTAA28", "CARNAA01", "CARNAA06", "CARNAA15", "CARNAA16" },
            new string[] { "CASTAA23", "CARNAA06" }
        };
        public static string[][] villageRuinRMB = {
            new string[] { "RUINAA00", "RUINAA01", "RUINAA02", "RUINAA03", "RUINAA05", "RUINAA06", "RUINAA10", "RUINAA13", "RUINAA16", "RUINAA17", "RUINAA19", "RUINAA20", "RUINAA25", "RUINAA26" },
            new string[] { "RUINAA04", "RUINAA07", "RUINAA08", "RUINAA09", "RUINAA11", "RUINAA12", "RUINAA15", "RUINAA18", "RUINAA21", "RUINAA23", "RUINAA24", "RUINAA28" },
            new string[] { "RUINAA14", "RUINAA22", "RUINAA27" }
        };
        public static string[][] monsterRMB = {
            new string[] { "CASTAA07", "CASTAA09" },
            new string[] { "CASTAA00", "CASTAA01" },
            new string[] { "CASTAA03", "CASTAA04", "CASTAA05" }
        };

        #endregion


        // locationDensity lists density for [climate, locationType] of a Kingdom region.
        // Other government types reduce/increase these values by a percentage.
        public static float[,] locationDensity = { 
            { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f },                  // Ocean
            { 0.35f, 0.45f, 0.60f, 0.30f, 0.40f, 0.45f, 0.60f, 0.25f, 0.30f, 0.35f, 0.05f, 0.30f, 0.20f, 0.0f },      // Desert
            { 0.30f, 0.40f, 0.55f, 0.25f, 0.45f, 0.40f, 0.55f, 0.30f, 0.25f, 0.30f, 0.10f, 0.25f, 0.15f, 0.0f },      // Desert2
            { 0.10f, 0.20f, 0.60f, 0.40f, 0.45f, 0.25f, 0.70f, 0.30f, 0.55f, 0.15f, 0.55f, 0.30f, 0.25f, 0.0f },      // Mountain
            { 0.25f, 0.55f, 0.80f, 0.70f, 0.40f, 0.45f, 1.15f, 0.65f, 0.85f, 0.30f, 0.40f, 0.40f, 0.20f, 0.0f },      // Rainforest
            { 0.10f, 0.15f, 0.35f, 0.25f, 0.35f, 0.20f, 0.80f, 0.25f, 0.40f, 0.10f, 0.20f, 0.20f, 0.15f, 0.0f },      // Swamp
            { 0.25f, 0.50f, 0.65f, 0.65f, 0.70f, 0.65f, 1.25f, 0.40f, 1.10f, 0.40f, 0.20f, 0.90f, 0.15f, 0.0f },      // Subtropical
            { 0.20f, 0.45f, 0.90f, 1.10f, 0.55f, 0.40f, 0.35f, 0.15f, 0.70f, 0.30f, 0.70f, 0.45f, 0.45f, 0.0f },      // MountainWood
            { 0.25f, 0.90f, 0.95f, 0.95f, 0.80f, 0.60f, 1.30f, 0.65f, 1.00f, 0.95f, 0.60f, 1.10f, 0.60f, 0.0f },      // Woodland
            { 0.20f, 0.75f, 0.80f, 0.75f, 0.70f, 0.50f, 1.10f, 0.65f, 0.70f, 0.95f, 0.80f, 0.90f, 0.50f, 0.0f }       // HauntedWoodland
        };                                                                                                        // TODO: Maquis

        // governmentModifier list reduction/increase of densities in a [government, locationType] format.
        // Kingdom doesn't make any change, but it's listed nonetheless for better understanding of future-me.
        public static float[,] governmentModifier = { 
            { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f},                // Kingdom
            { 0.95f, 0.90f, 0.85f, 0.95f, 1.05f, 0.90f, 0.95f, 1.10f, 0.90f, 0.95f, 1.15f, 0.90f, 0.95f, 1.0f },  // Duchy
            { 0.90f, 0.85f, 0.80f, 0.90f, 1.10f, 0.85f, 0.90f, 1.15f, 0.85f, 0.90f, 1.20f, 0.85f, 0.90f, 1.0f },  // March
            { 0.85f, 0.80f, 0.75f, 0.85f, 1.15f, 0.80f, 0.85f, 1.20f, 0.80f, 0.85f, 1.25f, 0.80f, 0.85f, 1.0f },  // County
            { 0.80f, 0.75f, 0.70f, 0.80f, 1.20f, 0.75f, 0.80f, 1.25f, 0.75f, 0.80f, 1.30f, 0.75f, 0.80f, 1.0f },  // Barony
            { 0.75f, 0.70f, 0.65f, 0.75f, 1.25f, 0.70f, 0.75f, 1.30f, 0.70f, 0.75f, 1.35f, 0.70f, 0.75f, 1.0f },  // Fiefdom
            { 1.05f, 1.10f, 1.15f, 1.05f, 0.95f, 1.10f, 1.05f, 0.90f, 1.10f, 1.05f, 0.85f, 1.10f, 1.05f, 1.0f }   // Empire
        };
        
                                                                                                                                                          
        public class GenOptions
        {
            public float[] locationDensity = new float[Enum.GetNames(typeof(DFRegion.LocationTypes)).Length];
            public bool includeCapital;
            public string capitalName;
            public (int, int) capitalPosition;
            public (int, int) capitalSize;
            public bool capitalWalled;
            public bool capitalPort;
            public bool[] majorRegionalism = new bool[Enum.GetNames(typeof(LocationNamesList.NameTypes)).Length];
            public bool[] minorRegionalism = new bool[Enum.GetNames(typeof(LocationNamesList.NameTypes)).Length];

            // static GenOptions()
            // {
            //     locationDensity = new float[(int)(DFRegion.LocationTypes.TownCity)];
            //     includeCapital = false;
            // }
        }

        public class Farm
        {
            public int worldX;
            public int worldY;
            public long locationID;
            public string region;
            public int climate;

        }

        public enum TownShapes
        {
            Regular = 0,
            LShape = 1,
            FourOnSix = 2,
            FiveOnSix = 3,
            SixOnEight = 4,
            MantaShape = 5,
            EightOnNine = 6,
            NineOnTwelve = 7,
            TwelveOnSixteen = 8,
        }

        void Awake()
        {
            regionOptions = new GenOptions();
            regionalismAreas = new string[Enum.GetNames(typeof(LocationNamesList.NameTypes)).Length];
            int counter = 0;
            foreach (string regionalism in Enum.GetNames(typeof(LocationNamesList.NameTypes)))
            {
                regionalismAreas[counter] = regionalism;
                counter++;
            }

            InitializeLocationDensity();

            locationIdList = InitializeLocationIdList();
        }

        void OnGUI()
        {
            GUILayout.Label(WorldInfo.WorldSetting.RegionNames[RegionManager.currentRegionIndex], EditorStyles.boldLabel);
            EditorGUILayout.Space(50.0f);

            GUILayout.BeginHorizontal();
            regionOptions.locationDensity[(int)(DFRegion.LocationTypes.TownCity)] = EditorGUILayout.FloatField("Cities: ", regionOptions.locationDensity[(int)(DFRegion.LocationTypes.TownCity)]);
            EditorGUILayout.LabelField("", (CalculateInteger(RegionManager.currentRegionData.surface, regionOptions.locationDensity[(int)(DFRegion.LocationTypes.TownCity)])).ToString());
            GUILayout.EndHorizontal();
            
            GUILayout.BeginHorizontal();
            regionOptions.locationDensity[(int)(DFRegion.LocationTypes.TownHamlet)] = EditorGUILayout.FloatField("Hamlets: ", regionOptions.locationDensity[(int)(DFRegion.LocationTypes.TownHamlet)]);
            EditorGUILayout.LabelField("", (CalculateInteger(RegionManager.currentRegionData.surface, regionOptions.locationDensity[(int)(DFRegion.LocationTypes.TownHamlet)])).ToString());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            regionOptions.locationDensity[(int)(DFRegion.LocationTypes.TownVillage)] = EditorGUILayout.FloatField("Villages: ", regionOptions.locationDensity[(int)(DFRegion.LocationTypes.TownVillage)]);
            EditorGUILayout.LabelField("", (CalculateInteger(RegionManager.currentRegionData.surface, regionOptions.locationDensity[(int)(DFRegion.LocationTypes.TownVillage)])).ToString());
            GUILayout.EndHorizontal();

            GUILayout.Space(20.0f);

            GUILayout.BeginHorizontal();
            regionOptions.locationDensity[(int)(DFRegion.LocationTypes.HomeWealthy)] = EditorGUILayout.FloatField("Wealthy Homes: ", regionOptions.locationDensity[(int)(DFRegion.LocationTypes.HomeWealthy)]);
            EditorGUILayout.LabelField("", (CalculateInteger(RegionManager.currentRegionData.surface, regionOptions.locationDensity[(int)(DFRegion.LocationTypes.HomeWealthy)])).ToString());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            regionOptions.locationDensity[(int)(DFRegion.LocationTypes.HomePoor)] = EditorGUILayout.FloatField("Hovels: ", regionOptions.locationDensity[(int)(DFRegion.LocationTypes.HomePoor)]);
            EditorGUILayout.LabelField("", (CalculateInteger(RegionManager.currentRegionData.surface, regionOptions.locationDensity[(int)(DFRegion.LocationTypes.HomePoor)])).ToString());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            regionOptions.locationDensity[(int)(DFRegion.LocationTypes.Tavern)] = EditorGUILayout.FloatField("Taverns: ", regionOptions.locationDensity[(int)(DFRegion.LocationTypes.Tavern)]);
            EditorGUILayout.LabelField("", (CalculateInteger(RegionManager.currentRegionData.surface, regionOptions.locationDensity[(int)(DFRegion.LocationTypes.Tavern)])).ToString());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            regionOptions.locationDensity[(int)(DFRegion.LocationTypes.HomeFarms)] = EditorGUILayout.FloatField("Farms: ", regionOptions.locationDensity[(int)(DFRegion.LocationTypes.HomeFarms)]);
            EditorGUILayout.LabelField("", (CalculateInteger(RegionManager.currentRegionData.surface, regionOptions.locationDensity[(int)(DFRegion.LocationTypes.HomeFarms)])).ToString());
            GUILayout.EndHorizontal();
    
            GUILayout.Space(20.0f);

            GUILayout.BeginHorizontal();
            regionOptions.locationDensity[(int)(DFRegion.LocationTypes.ReligionTemple)] = EditorGUILayout.FloatField("Temples: ", regionOptions.locationDensity[(int)(DFRegion.LocationTypes.ReligionTemple)]);
            EditorGUILayout.LabelField("", (CalculateInteger(RegionManager.currentRegionData.surface, regionOptions.locationDensity[(int)(DFRegion.LocationTypes.ReligionTemple)])).ToString());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            regionOptions.locationDensity[(int)(DFRegion.LocationTypes.ReligionCult)] = EditorGUILayout.FloatField("Cult Sites: ", regionOptions.locationDensity[(int)(DFRegion.LocationTypes.ReligionCult)]);
            EditorGUILayout.LabelField("", (CalculateInteger(RegionManager.currentRegionData.surface, regionOptions.locationDensity[(int)(DFRegion.LocationTypes.ReligionCult)])).ToString());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            regionOptions.locationDensity[(int)(DFRegion.LocationTypes.Coven)] = EditorGUILayout.FloatField("Witch Covens: ", regionOptions.locationDensity[(int)(DFRegion.LocationTypes.Coven)]);
            EditorGUILayout.LabelField("", (CalculateInteger(RegionManager.currentRegionData.surface, regionOptions.locationDensity[(int)(DFRegion.LocationTypes.Coven)])).ToString());
            GUILayout.EndHorizontal();

            GUILayout.Space(20.0f);

            GUILayout.BeginHorizontal();
            regionOptions.locationDensity[(int)(DFRegion.LocationTypes.DungeonLabyrinth)] = EditorGUILayout.FloatField("Large Dungeons: ", regionOptions.locationDensity[(int)(DFRegion.LocationTypes.DungeonLabyrinth)]);
            EditorGUILayout.LabelField("", (CalculateInteger(RegionManager.currentRegionData.surface, regionOptions.locationDensity[(int)(DFRegion.LocationTypes.DungeonLabyrinth)])).ToString());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            regionOptions.locationDensity[(int)(DFRegion.LocationTypes.DungeonKeep)] = EditorGUILayout.FloatField("Medium Dungeons: ", regionOptions.locationDensity[(int)(DFRegion.LocationTypes.DungeonKeep)]);
            EditorGUILayout.LabelField("", (CalculateInteger(RegionManager.currentRegionData.surface, regionOptions.locationDensity[(int)(DFRegion.LocationTypes.DungeonKeep)])).ToString());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            regionOptions.locationDensity[(int)(DFRegion.LocationTypes.DungeonRuin)] = EditorGUILayout.FloatField("Small Dungeons: ", regionOptions.locationDensity[(int)(DFRegion.LocationTypes.DungeonRuin)]);
            EditorGUILayout.LabelField("", (CalculateInteger(RegionManager.currentRegionData.surface, regionOptions.locationDensity[(int)(DFRegion.LocationTypes.DungeonRuin)])).ToString());
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            regionOptions.locationDensity[(int)(DFRegion.LocationTypes.Graveyard)] = EditorGUILayout.FloatField("Graveyards: ", regionOptions.locationDensity[(int)(DFRegion.LocationTypes.Graveyard)]);
            EditorGUILayout.LabelField("", (CalculateInteger(RegionManager.currentRegionData.surface, regionOptions.locationDensity[(int)(DFRegion.LocationTypes.Graveyard)])).ToString());
            GUILayout.EndHorizontal();

            GUILayout.Space(20.0f);

            GUILayout.BeginHorizontal();
            regionOptions.locationDensity[(int)(DFRegion.LocationTypes.HiddenLocation)] = EditorGUILayout.FloatField("Hidden Locations: ", regionOptions.locationDensity[(int)(DFRegion.LocationTypes.HiddenLocation)]);
            EditorGUILayout.LabelField("", (CalculateInteger(RegionManager.currentRegionData.surface, regionOptions.locationDensity[(int)(DFRegion.LocationTypes.HiddenLocation)])).ToString());
            GUILayout.EndHorizontal();

            GUILayout.Space(50.0f);

            regionOptions.includeCapital = EditorGUILayout.ToggleLeft("Generate Capital", regionOptions.includeCapital);
            GUILayout.BeginHorizontal();
            regionOptions.capitalName = EditorGUILayout.TextField("Name: ", regionOptions.capitalName, GUILayout.MaxWidth(300.0f));
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            regionOptions.capitalPosition.Item1 = EditorGUILayout.IntField("Position: ", regionOptions.capitalPosition.Item1, GUILayout.MaxWidth(300.0f));
            GUILayout.Space(20.0f);
            regionOptions.capitalPosition.Item2 = EditorGUILayout.IntField(", ", regionOptions.capitalPosition.Item2, GUILayout.MaxWidth(300.0f));
            // GUILayout.Space(30.0f);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            regionOptions.capitalSize.Item1 = EditorGUILayout.IntField("Size: X = ", regionOptions.capitalSize.Item1, GUILayout.MaxWidth(300.0f));
            GUILayout.Space(20.0f);
            regionOptions.capitalSize.Item2 = EditorGUILayout.IntField(", Y = ", regionOptions.capitalSize.Item2, GUILayout.MaxWidth(300.0f));
            GUILayout.Space(30.0f);
            regionOptions.capitalWalled = EditorGUILayout.ToggleLeft("Walled", regionOptions.capitalWalled);
            regionOptions.capitalPort = EditorGUILayout.ToggleLeft("Port", regionOptions.capitalPort);
            GUILayout.EndHorizontal();

            GUILayout.Space(50.0f);

            GUILayout.BeginHorizontal();
            GUILayout.Label("Major Regionalisms");
            for (int i = 0; i < Enum.GetNames(typeof(LocationNamesList.NameTypes)).Length; i++)
            {
                if (i % 4 == 0)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.EndHorizontal();
                }
                regionOptions.majorRegionalism[i] = EditorGUILayout.ToggleLeft(((LocationNamesList.NameTypes)i).ToString(), regionOptions.majorRegionalism[i]);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(30.0f);

            GUILayout.Label("Minor Regionalisms");
            GUILayout.BeginHorizontal();
            for (int j = 0; j < Enum.GetNames(typeof(LocationNamesList.NameTypes)).Length; j++)
            {
                if (j % 4 == 0)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.EndHorizontal();
                }
                regionOptions.minorRegionalism[j] = EditorGUILayout.ToggleLeft(((LocationNamesList.NameTypes)j).ToString(), regionOptions.minorRegionalism[j]);
            }
            GUILayout.EndHorizontal();


            if (GUILayout.Button("Generate Locations", GUILayout.MaxWidth(200.0f)))
            {
                GenerateLocations(regionOptions);
            }

            // if (GUILayout.Button("Generate Routes", GUILayout.MaxWidth(200.0f)))
            // {
            //     GenerateRoutes(regionOptions);
            // }

            // if (GUILayout.Button("Generate farms", GUILayout.MaxWidth(200.0f)))
            // {
            //     GenerateFarms();
            // }
        }

        protected int CalculateInteger(int surface, float density)
        {
            float result = density * (float)surface / 100.0f;

            if ((result - (int)result) < 0.5f)
                return (int)result;
            else return (int)result + 1;
        }

        protected void GenerateFarms()
        {
            string text = File.ReadAllText(MapEditor.testPath + "/RoadsideFarms.json");

            List<Farm> farms = new List<Farm>();
            int cursorPos = 0;

            while(cursorPos < text.Length)
            {
                Farm farm = new Farm();
                string textPart;
                int index = 0;
                cursorPos = text.IndexOf("Farm,", cursorPos);
                cursorPos += 5;
                index = text.IndexOf(",", cursorPos);
                textPart = text.Substring(cursorPos, index - cursorPos);
                // Debug.Log("cursorPos (farm.worldX): " + cursorPos + ", textPart: " + textPart);

                farm.worldX = int.Parse(textPart);

                
                if(farm.worldX < 0)
                {
                    Debug.Log("Error with cursor position while reading worldX! (cursorPos == " + cursorPos + ", textPart: " + textPart + ")");
                }
                // if(!int.TryParse(textPart, out farm.worldX));
                // {
                //     Debug.Log("Error with cursor position while reading worldX! (cursorPos == " + cursorPos + ", textPart: " + textPart + ")");
                //     break;
                // }

                cursorPos += index - cursorPos + 1;
                index = text.IndexOf(",", cursorPos);
                textPart = text.Substring(cursorPos, index - cursorPos);
                // Debug.Log("cursorPos (farm.worldY): " + cursorPos + ", textPart: " + textPart);
                farm.worldY = int.Parse(textPart);
                
                if(farm.worldY < 0)
                {
                    Debug.Log("Error with cursor position while reading worldY! (cursorPos == " + cursorPos + ", textPart: " + textPart + ")");
                }
                // if(!int.TryParse(textPart, out farm.worldY));
                // {
                //     Debug.Log("Error with cursor position while reading worldY! (cursorPos == " + cursorPos + ", textPart: " + textPart + ")");
                //     break;
                // }

                cursorPos += index - cursorPos + 1;
                index = text.IndexOf(",", cursorPos);
                textPart = text.Substring(cursorPos, index - cursorPos);
                Debug.Log("cursorPos (farm.locationID): " + cursorPos + ", textPart: " + textPart);
                farm.locationID = long.Parse(textPart);
                
                if(farm.locationID < 0)
                {
                    Debug.Log("Error with cursor position while reading locationID! (cursorPos == " + cursorPos + ", textPart: " + textPart + ")");
                }
                // if(!int.TryParse(textPart, out farm.locationID));
                // {
                //     Debug.Log("Error with cursor position while reading locationID! (cursorPos == " + cursorPos + ", textPart: " + textPart + ")");
                //     break;
                // }

                cursorPos += index - cursorPos + 1;
                index = text.IndexOf(",", cursorPos);
                textPart = text.Substring(cursorPos, index - cursorPos);
                farm.region = textPart;

                cursorPos += index - cursorPos + 1;
                index = text.IndexOf(",", cursorPos);
                textPart = text.Substring(cursorPos, index - cursorPos);
                // Debug.Log("cursorPos (farm.climate): " + cursorPos + ", textPart: " + textPart);
                farm.climate = int.Parse(textPart);
                
                if(farm.climate < 0)
                {
                    Debug.Log("Error with cursor position while reading climate! (cursorPos == " + cursorPos + ", textPart: " + textPart + ")");
                }
                // if(!int.TryParse(textPart, out farm.climate));
                // {
                //     Debug.Log("Error with cursor position while reading climate! (cursorPos == " + cursorPos + ", textPart: " + textPart + ")");
                //     break;
                // }

                cursorPos += 2;

                farms.Add(farm);
            }

            int[] progressive = new int[62];
            int counter = 0;
            string[][] vanillaLocationNames = new string[62][];
            string[][] randomFarmNames = new string[62][];
            List<string> generatedFarmNames = new List<string>();

            foreach (Farm randomFarm in farms)
            {
                int regionIndex = Array.IndexOf(WorldInfo.WorldSetting.RegionNames, randomFarm.region);

                generatedFarmNames = new List<string>();
                if (randomFarmNames[regionIndex] != null)
                {
                    generatedFarmNames = randomFarmNames[regionIndex].ToList();
                }

                if (vanillaLocationNames[regionIndex] == null)
                {
                    int count = 0;
                    vanillaLocationNames[regionIndex] = new string[Worldmaps.Worldmap[regionIndex].LocationCount];

                    foreach (DFLocation location in Worldmaps.Worldmap[regionIndex].Locations)
                    {
                        vanillaLocationNames[regionIndex][count] = Worldmaps.Worldmap[regionIndex].Locations[count].Name;
                        count++;
                    }
                }

                int regionalism = GetRegionalism(randomFarm.region);
                string farmName;

                do{
                    farmName = GenerateHomeName((int)DFRegion.LocationTypes.HomeFarms, regionalism);
                }
                while (vanillaLocationNames[regionIndex].Contains(farmName) || generatedFarmNames.Contains(farmName));

                generatedFarmNames.Add(farmName);
                randomFarmNames[regionIndex] = new string[generatedFarmNames.Count];
                randomFarmNames[regionIndex] = generatedFarmNames.ToArray();

                DFLocation generatedFarm = GenerateFarm((int)DFRegion.LocationTypes.HomeFarms, randomFarm, farmName, regionalism);

                string fileDataPath = Path.Combine(MapEditor.testPath, "Farms", "locationnew-randomfarm" + progressive[regionIndex] + "-" + regionIndex + ".json");

                var json = JsonConvert.SerializeObject(generatedFarm, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                File.WriteAllText(fileDataPath, json);

                progressive[regionIndex]++;
                counter++;

                // if (counter > 30)
                //     break;
            }

            Debug.Log(counter + " farms were generated.");
            // string fileDataPath = Path.Combine(MapEditor.testPath, "RoadsideFarms.json");
            // var json = JsonConvert.SerializeObject(text, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            // File.WriteAllText(fileDataPath, json);
        }

        protected int GetRegionalism(string regionName)
        {
            switch (regionName)
            {
                case "Dwynnen":
                case "Isle of Balfiera":
                case "Wrothgarian Mountains":
                case "Daggerfall":
                case "Glenpoint":
                case "Betony":
                case "Anticlere":
                case "Wayrest":
                case "Orsinium Area":
                case "Northmoor":
                case "Menevia":
                case "Alcaire":
                case "Koegria":
                case "Bhoriane":
                case "Kambria":
                case "Phrygias":
                case "Urvaius":
                case "Ykalon":
                case "Daenia":
                case "Shalgora":
                case "Tulune":
                case "Glenumbra Moors":
                case "Ilessan Hills":
                    return 0;

                case "Alik'r Desert":
                case "DragonTail Mountains":
                case "Dak'fron":
                case "Sentinel":
                case "Lainlyn":
                case "Abibon-Gora":
                case "Kairou":
                case "Pothago":
                case "Myrkwasa":
                case "Ayasofya":
                case "Tigonus":
                case "Kozanset":
                case "Satakalaam":
                case "Totambu":
                case "Mournoth":
                case "Ephesus":
                case "Santaki":
                case "Antiphyllos":
                case "Bergama":
                case "Gavaudon":
                case "Cybiades":
                default:
                    return 2;
            }
        }

        /// <summary>Main locations generator function.</summary>
        protected void GenerateLocations(GenOptions currentOptions)
        {
            bool capitalToBeGenerated = regionOptions.includeCapital;
            DFPosition capitalPosition = new DFPosition(-1, -1);
            DFPosition[] surfaceArray = new DFPosition[RegionManager.currentRegionData.surface];
            DFPosition[][] locationMatrix = new DFPosition[Enum.GetNames(typeof(DFRegion.LocationTypes)).Length][];
            surfaceArray = GenerateSurfaceArray(RegionManager.currentRegionIndex, surfaceArray.Length);

            for (int k = 0; k < Enum.GetNames(typeof(DFRegion.LocationTypes)).Length; k++)
            {
                if (k == (int)DFRegion.LocationTypes.HomeYourShips || k == (int)DFRegion.LocationTypes.None)
                    locationMatrix[k] = new DFPosition[0];
                else locationMatrix[k] = new DFPosition[CalculateInteger(surfaceArray.Length, currentOptions.locationDensity[k])];
            }

            List<int> locationAlreadyRandomised = new List<int>();
            locationAlreadyRandomised.Add((int)DFRegion.LocationTypes.HomeYourShips);
            locationAlreadyRandomised.Add((int)DFRegion.LocationTypes.None);

            locationMatrix = InitializeLocationMatrix(locationMatrix);

            for (int i = 0; i < Enum.GetNames(typeof(DFRegion.LocationTypes)).Length; i++)
            {
                if (i == (int)DFRegion.LocationTypes.HomeYourShips || i == (int)DFRegion.LocationTypes.None)
                    continue;
                
                int randomType;
                int randomIndex;

                do{
                    randomType = UnityEngine.Random.Range(0, Enum.GetNames(typeof(DFRegion.LocationTypes)).Length);
                }
                while (locationAlreadyRandomised.Contains(randomType) || randomType == (int)DFRegion.LocationTypes.HomeYourShips || randomType == (int)DFRegion.LocationTypes.None);

                locationAlreadyRandomised.Add(randomType);

                if (capitalToBeGenerated)
                {
                    if (regionOptions.capitalPosition.Item1 <= 0 || regionOptions.capitalPosition.Item2 <= 0)
                    {
                        Debug.Log("Position for capital city was not provided: will generate it randomly");
                        randomIndex = UnityEngine.Random.Range(0, surfaceArray.Length);
                        locationMatrix[(int)DFRegion.LocationTypes.TownCity][0] = capitalPosition = surfaceArray[randomIndex];
                        surfaceArray[randomIndex] = new DFPosition(-1, -1);
                    }
                    else
                    {
                        capitalPosition = new DFPosition(regionOptions.capitalPosition.Item1, regionOptions.capitalPosition.Item2);
                        if (surfaceArray.Contains(capitalPosition))
                        {
                            locationMatrix[(int)DFRegion.LocationTypes.TownCity][0] = surfaceArray[Array.IndexOf(surfaceArray, capitalPosition)];
                            surfaceArray[Array.IndexOf(surfaceArray, capitalPosition)] = new DFPosition(-1, -1);
                        }
                        else
                        {
                            locationMatrix[(int)DFRegion.LocationTypes.TownCity][0] = new DFPosition(capitalPosition.X, capitalPosition.Y);
                            // surfaceArray[Array.IndexOf(surfaceArray, capitalPosition)] = new DFPosition(-1, -1);
                        }
                    }
                }
                capitalToBeGenerated = false;

                for (int j = 0; j < locationMatrix[randomType].Length; j++)
                {

                    bool posOK = false;
                    DFPosition assignedPosition;

                    if (randomType == (int)DFRegion.LocationTypes.TownCity && j == 0 && locationMatrix[(int)DFRegion.LocationTypes.TownCity][j] != null)
                        j++;

                    do{
                        randomIndex = UnityEngine.Random.Range(0, surfaceArray.Length);

                        if (surfaceArray[randomIndex].X == -1)
                            continue;

                        locationMatrix[randomType][j] = surfaceArray[randomIndex];

                        posOK = CheckPosition(surfaceArray[randomIndex], randomType, ref currentOptions, locationMatrix);
                    }
                    while (!posOK);

                    assignedPosition = surfaceArray[randomIndex];
                    surfaceArray[randomIndex] = new DFPosition(-1, -1);
                }
            }

            // Initialising region
            Worldmap modifiedRegion = new Worldmap();

            // Converting what is needed to List, for ease of use
            List<string> mapNames = new List<string>();
            List<DFRegion.RegionMapTable> mapTable = new List<DFRegion.RegionMapTable>();
            List<DFLocation> locations = new List<DFLocation>();
            List<Worldmap> worldList = new List<Worldmap>();

            if (Worldmaps.Worldmap.Length > RegionManager.currentRegionIndex &&
                !foolRegions.Contains(RegionManager.currentRegionIndex))
            {
                modifiedRegion = Worldmaps.Worldmap[RegionManager.currentRegionIndex];
                mapNames = modifiedRegion.MapNames.ToList();
                mapTable = modifiedRegion.MapTable.ToList();
                locations = modifiedRegion.Locations.ToList();
            }
            else{
                modifiedRegion = new Worldmap();
            }

            worldList = Worldmaps.Worldmap.ToList();
            Worldmaps.Worldmap = new Worldmap[WorldInfo.WorldSetting.Regions];
            int regionCounter = 0;

            for (int z = 0; z < WorldInfo.WorldSetting.Regions; z++)
            {
                string regionName = WorldInfo.WorldSetting.RegionNames[z];

                if (foolRegions.Contains(z) || z >= worldList.Count)
                {
                    Worldmaps.Worldmap[z] = new Worldmap();
                    Worldmaps.Worldmap[z] = new Worldmap();
                    Worldmaps.Worldmap[z].MapNames = new string[0];
                }
                else{
                    Worldmaps.Worldmap[z] = worldList[z];
                }      
            }

            string locationName;
            DFLocation location = new DFLocation();

            for (int l = 0; l < Enum.GetNames(typeof(DFRegion.LocationTypes)).Length; l++)
            {
                for (int m = 0; m < locationMatrix[l].Length; m++)
                {
                    if (l == (int)DFRegion.LocationTypes.DungeonLabyrinth || l == (int)DFRegion.LocationTypes.DungeonKeep || l == (int)DFRegion.LocationTypes.DungeonRuin)
                    {
                        int dungeonType;
                        
                        do{
                            dungeonType = UnityEngine.Random.Range(0, Enum.GetNames(typeof(DFRegion.DungeonTypes)).Length);
                        }
                        while (dungeonType == (int)DFRegion.DungeonTypes.Cemetery || dungeonType == (int)DFRegion.DungeonTypes.NoDungeon);

                        location = CreateLocation(l, locationMatrix[l][m], out locationName, ref currentOptions, false, dungeonType);
                        locations.Add(location);
                        // mapTable.Add(location.MapTableData);
                    }
                    else if (l == (int)DFRegion.LocationTypes.Graveyard)
                    {
                        location = CreateLocation(l, locationMatrix[l][m], out locationName, ref currentOptions, false, (int)DFRegion.DungeonTypes.Cemetery);
                        locations.Add(location);
                        // mapTable.Add(location.MapTableData);
                    }
                    else if (locationMatrix[l][m] == capitalPosition)
                    {
                        location = CreateLocation(l, locationMatrix[l][m], out locationName, ref currentOptions, true);
                        locations.Add(location);
                    }
                    else{ 
                        location = CreateLocation(l, locationMatrix[l][m], out locationName, ref currentOptions);
                        locations.Add(location);
                        // mapTable.Add(location.MapTableData);
                    }

                    // mapNames.Add(location.Name);
                }
            }

            bool everythingDone = true;
            int index;

            do
            {
                everythingDone = true;

                for (int n = 0; n < locations.Count; n++)
                {
                    if (n == 0)
                        continue;

                    if (locations[n].MapTableData.MapId < locations[n - 1].MapTableData.MapId)
                    {
                        DFLocation swap = new DFLocation();
                        swap = locations[n - 1];
                        locations.RemoveAt(n - 1);
                        locations.Insert(n, swap);
                        everythingDone = false;
                    }
                }
            }
            while (!everythingDone);

            for (int p = 0; p < locations.Count; p++)
            {
                DFLocation correctIndex = locations[p];
                correctIndex.LocationIndex = p;
                locations.RemoveAt(p);
                locations.Insert(p, correctIndex);
                mapTable.Add(correctIndex.MapTableData);
                mapNames.Add(correctIndex.Name);
            }

            Worldmaps.Worldmap[RegionManager.currentRegionIndex].Name = WorldInfo.WorldSetting.RegionNames[RegionManager.currentRegionIndex];
            Worldmaps.Worldmap[RegionManager.currentRegionIndex].LocationCount = locations.Count;
            Worldmaps.Worldmap[RegionManager.currentRegionIndex].MapNames = mapNames.ToArray();
            Worldmaps.Worldmap[RegionManager.currentRegionIndex].MapTable = mapTable.ToArray();
            Worldmaps.Worldmap[RegionManager.currentRegionIndex].Locations = locations.ToArray();

            string fileDataPath = Path.Combine(MapEditor.testPath, "MapsTest.json");
            var json = JsonConvert.SerializeObject(Worldmaps.Worldmap, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            File.WriteAllText(fileDataPath, json);
        }

        protected DFPosition[][] InitializeLocationMatrix(DFPosition[][] locationMatrix)
        {
            for (int i = 0; i < Enum.GetNames(typeof(DFRegion.LocationTypes)).Length; i++)
            {
                for (int j = 0; j < locationMatrix[i].Length; j++)
                {
                    locationMatrix[i][j] = new DFPosition(-1, -1);
                }
            }

            return locationMatrix;
        }

        protected DFLocation CreateLocation(int locationType, DFPosition position, out string locationName, ref GenOptions currentOptions, bool isCapital = false, int dungeonType = -1)
        {
            // Generate a location name based on LocationType, and major and minor regionalisms
            List<int> majorRegionalism = new List<int>();
            List<int> minorRegionalism = new List<int>();
            DFLocation generatedLocation = new DFLocation();
            
            for (int i = 0; i < currentOptions.majorRegionalism.Length; i++)
            {
                if (currentOptions.majorRegionalism[i])
                    majorRegionalism.Add(i);
            }

            for (int j = 0; j < currentOptions.minorRegionalism.Length; j++)
            {
                if (currentOptions.minorRegionalism[j])
                    minorRegionalism.Add(j);
            }

            do
            {
                if (isCapital)
                {
                    locationName = regionOptions.capitalName;
                }
                else
                    locationName = GenerateLocationName(locationType, majorRegionalism, minorRegionalism, dungeonType);
            }
            while (Worldmaps.Worldmap[RegionManager.currentRegionIndex].MapNames.Contains(locationName));

            generatedLocation = GenerateLocation(locationType, position, dungeonType, locationName, isCapital);

            return generatedLocation;
        }

        protected string GenerateLocationName(int locationType, List<int> majorReg, List<int> minorReg, int dungeonType)
        {
            int majRegNumber = majorReg.Count;
            int minRegNumber = minorReg.Count;
            int minorRegionalismChance = 100 - majorRegionalismChance;
            int majChance = majRegNumber * majorRegionalismChance;
            int minChance = minRegNumber * minorRegionalismChance;
            int totalChance = majChance + minChance;
            int selectedReg = 0;

            int randomPick = (UnityEngine.Random.Range(0, totalChance) + 1);

            if (randomPick <= majChance)
                selectedReg = majorReg[(randomPick - 1 ) / majorRegionalismChance];
            else selectedReg = minorReg[(randomPick - majChance - 1) / minorRegionalismChance];

            string resultingName;

            switch (locationType)
            {
                case (int)DFRegion.LocationTypes.TownCity:
                case (int)DFRegion.LocationTypes.TownHamlet:
                case (int)DFRegion.LocationTypes.TownVillage:
                    resultingName = GenerateTownName(locationType, selectedReg);
                    break;

                case (int)DFRegion.LocationTypes.HomeFarms:
                case (int)DFRegion.LocationTypes.HomeWealthy:
                case (int)DFRegion.LocationTypes.HomePoor:
                    resultingName = GenerateHomeName(locationType, selectedReg);
                    break;

                case (int)DFRegion.LocationTypes.Tavern:
                    resultingName = "Waiting for RMB selection";
                    break;

                case (int)DFRegion.LocationTypes.DungeonLabyrinth:
                case (int)DFRegion.LocationTypes.DungeonKeep:
                case (int)DFRegion.LocationTypes.DungeonRuin:
                case (int)DFRegion.LocationTypes.Graveyard:
                    resultingName = GenerateDungeonName(locationType, selectedReg, dungeonType);
                    break;

                case (int)DFRegion.LocationTypes.ReligionTemple:
                case (int)DFRegion.LocationTypes.ReligionCult:
                    resultingName = GenerateTempleName(locationType, selectedReg);
                    break;

                default:
                    resultingName = "error";
                    break;
            }

            return resultingName;
        }

        protected string GenerateTownName(int locationType, int regionalism)
        {
            int numPrefix;
            int numSuffix1;
            bool uniqueName = false;
            string tempName = "";
            int numExtra;

            Debug.Log("Regionalism picked: " + (LocationNamesList.NameTypes)regionalism);

            switch (regionalism)
            {
                case (int)LocationNamesList.NameTypes.HighRockVanilla:
                    do
                    {
                        numPrefix = UnityEngine.Random.Range(0, LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Prefix].Length);
                        numSuffix1 = UnityEngine.Random.Range(0, LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Suffix1].Length);

                        tempName = string.Concat(LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Prefix][numPrefix],
                                            LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Suffix1][numSuffix1]);
                    }
                    while (tempName.Equals("Daggerfall") || tempName.Equals("Wayrest"));

                    if ((UnityEngine.Random.Range(0, 10) + 1) > 9)
                    {
                        numExtra = UnityEngine.Random.Range(0, LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Extra].Length);
                        tempName = string.Concat(tempName, " ", LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Extra][numExtra]);
                    }

                    return tempName;

                case (int)LocationNamesList.NameTypes.HighRockModern:
                    do
                    {
                        numPrefix = UnityEngine.Random.Range(0, LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Prefix].Length);
                        numSuffix1 = UnityEngine.Random.Range(0, LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Suffix1].Length);

                        tempName = string.Concat(LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Prefix][numPrefix],
                                            LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Suffix1][numSuffix1]);
                    }
                    while (tempName.Equals("Daggerfall") || tempName.Equals("Wayrest"));

                    if ((UnityEngine.Random.Range(0, 10) + 1) > 9)
                    {
                        numExtra = UnityEngine.Random.Range(0, LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Extra].Length);
                        tempName = string.Concat(LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Extra][numExtra], " de ", tempName);
                    }

                    return tempName;
                break;

                case (int)LocationNamesList.NameTypes.Hammerfell:
                    
                        numPrefix = UnityEngine.Random.Range(0, LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Prefix].Length);
                        numSuffix1 = UnityEngine.Random.Range(0, hammerfellTownVowel);
                        string suffix1 = "";
                        string[] vowelSuffix = new string[LocationNamesList.NamesList[(int)LocationNamesList.NameTypes.Hammerfell][(int)LocationNamesList.NameParts.Suffix1].Length];
                        vowelSuffix = LocationNamesList.NamesList[(int)LocationNamesList.NameTypes.Hammerfell][(int)LocationNamesList.NameParts.Suffix1];
                        string[][] vowelArray = GenerateVowelArray(vowelSuffix);
                        int nameType;

                        if (numSuffix1 != 3)
                            nameType = UnityEngine.Random.Range(0, 4);
                        else nameType = UnityEngine.Random.Range(0, 2) + 2;
                        int numSuffix2 = UnityEngine.Random.Range(0, LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Suffix2].Length);
                        numExtra = UnityEngine.Random.Range(0, LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Extra].Length);

                        Debug.Log("numSuffix1: " + numSuffix1);

                        switch (nameType)
                        {
                            case 0:     // Prefix + vowel + Suffix1 + Suffix2
                            case 1:     //  Prefix + vowel + Suffix1 + Extra
                                suffix1 = PickVowel(numSuffix1, vowelSuffix, vowelArray, true);
                                break;

                            case 2:     // Prefix + vowel + Suffix2
                            case 3:     // Prefix + vowel + Extra
                                suffix1 = PickVowel(numSuffix1, vowelSuffix, vowelArray, false);
                                break;                                
                        }

                        switch (nameType)
                        {
                            case 0:
                            case 2:
                                tempName = string.Concat(LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Prefix][numPrefix], 
                                            suffix1,
                                            LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Suffix2][numSuffix2]);
                                break;

                            case 1:
                            case 3:
                                tempName = string.Concat(LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Prefix][numPrefix],
                                            suffix1,
                                            LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Extra][numExtra]);
                                break;

                            default:
                                break;
                        }

                    return tempName;

                case (int)LocationNamesList.NameTypes.Skyrim:
                        numPrefix = UnityEngine.Random.Range(0, LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Prefix].Length);
                        numSuffix1 = UnityEngine.Random.Range(0, LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Suffix1].Length);

                        if ((UnityEngine.Random.Range(0, 10) + 1) > 8)
                        {
                            int extra = UnityEngine.Random.Range(0, LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Extra].Length);
                            if (!LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Extra][extra].Equals(LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Suffix1][numSuffix1]))
                            {
                                string extraUpper = LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Extra][extra];

                                return LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Prefix][numPrefix] +
                                       LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Suffix1][numSuffix1] +
                                       " " +
                                       extraUpper;
                            }
                        }

                        tempName = string.Concat(LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Prefix][numPrefix] +
                               LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Suffix1][numSuffix1]);

                    return tempName;

                case (int)LocationNamesList.NameTypes.Reachmen:
                        numPrefix = UnityEngine.Random.Range(0, LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Prefix].Length);
                        numSuffix1 = UnityEngine.Random.Range(0, LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Suffix1].Length);

                        if ((UnityEngine.Random.Range(0, 10) + 1) > 4)
                        {
                            int extra = UnityEngine.Random.Range(0, LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Extra].Length);
                            string extraUpper = LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Extra][extra];

                            return LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Prefix][numPrefix] +
                                   extraUpper +
                                   LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Suffix1][numSuffix1];
                        }

                        tempName = string.Concat(LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Prefix][numPrefix] +
                               LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Suffix1][numSuffix1]);

                    return tempName;

                case (int)LocationNamesList.NameTypes.Morrowind:
                        numPrefix = UnityEngine.Random.Range(0, LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Prefix].Length);
                        numSuffix1 = UnityEngine.Random.Range(0, LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Suffix1].Length);

                        if ((UnityEngine.Random.Range(0, 10) + 1) > 8)
                        {
                            int extra = UnityEngine.Random.Range(0, LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Suffix2].Length);
                            string extraUpper = LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Suffix2][extra];

                            return LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Prefix][numPrefix] +
                                   extraUpper +
                                   LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Suffix1][numSuffix1];

                        }

                        tempName = string.Concat(LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Prefix][numPrefix] +
                               LocationNamesList.NamesList[regionalism][(int)LocationNamesList.NameParts.Suffix1][numSuffix1]);

                    return tempName;

                default:
                    break;
            }

            return "Error";
        }

        protected string PickVowel(int vowel, string[] vowelSuffix, string[][] vowelArray, bool suffixPresent = true)
        {
            int counter = 0;
            int startingPoint = -1;
            string resultingString = "";
            int suffix1;
            int vowelCounter = vowel;

            do{
                if (counter == vowelSuffix.Length || vowelSuffix[counter].StartsWith("*"))
                {
                    if (vowelCounter == 0)
                    {
                        resultingString = vowelSuffix[counter].Substring(1);
                        if (!suffixPresent)
                        {
                            return resultingString;
                        }
                        else{
                            suffix1 = UnityEngine.Random.Range(0, vowelArray[vowel].Length);
                            resultingString = string.Concat(resultingString, vowelArray[vowel][suffix1]);
                            break;
                        }
                    }

                    vowelCounter--;
                }
                counter++;
            }
            while (startingPoint < 0);

            return resultingString;
        }

        protected string[][] GenerateVowelArray(string[] vowels)
        {
            string[][] vowelArray = new string[hammerfellTownVowel][];
            int vowelCounter = -1;
            List<string> vowelList = new List<string>();
            for (int i = 0; i <= vowels.Length; i++)
            {
                if (i == vowels.Length || vowels[i].StartsWith("*"))
                {
                    if (i != 0)
                    {
                        vowelArray[vowelCounter] = vowelList.ToArray();
                        vowelList = new List<string>();
                    }
                    vowelCounter++;
                }
                else{
                    vowelList.Add(vowels[i]);
                }
            }

            return vowelArray;
        }

        protected string ConvertToUppercase(string input)
        {
            char[] charExtra = input.ToCharArray();
            Char.ToUpper(charExtra[0]);
            input = charExtra.ToString();

            return input;
        }

        /// <summary>
        /// Checks if an assigned position is too close to a conflicting location,
        /// or already taken.
        /// </summary>
        protected bool CheckPosition(DFPosition position, int locationType, ref GenOptions currentOptions, DFPosition[][] locationMatrix)
        {
            if (3 > SmallHeightmap.GetHeightMapValue(position.X, position.Y))
                return false;

            List<int> conflictingLocations = new List<int>();
            conflictingLocations = GetConflictingLocations(locationType);

            for (int i = 0; i < locationType; i++)
            {
                if (i == (int)DFRegion.LocationTypes.HomeYourShips || i == (int)DFRegion.LocationTypes.None)
                    continue;

                for (int j = 0; j < (CalculateInteger(RegionManager.currentRegionData.surface, currentOptions.locationDensity[i])); j++)
                {
                    if (locationMatrix[i][j].X == position.X && locationMatrix[i][j].Y == position.Y)
                        return false;

                    if (!conflictingLocations.Contains(i))
                        break;

                    float distance = CalculateDistance(position, locationMatrix[i][j]);
                    if (distance <= distTolerance)
                        return false;
                }
            }

            return true;
        }

        public static float CalculateDistance(DFPosition position1, DFPosition position2)
        {
            double absX = (double)(Math.Abs(position1.X - position2.X));
            double absY = (double)(Math.Abs(position1.Y - position2.Y));

            return (float)(Math.Sqrt((Math.Pow(absX, 2.0f) + Math.Pow(absY, 2.0f))));
        }

        protected float CalculateDistance((int, int) position1, (int, int) position2)
        {
            double absX = (double)(Math.Abs(position1.Item1 - position2.Item1));
            double absY = (double)(Math.Abs(position1.Item2 - position2.Item2));

            return (float)(Math.Sqrt((Math.Pow(absX, 2.0f) + Math.Pow(absY, 2.0f))));
        }

        /// <summary>
        /// Create a list of those location types that can't be positioned near
        /// the location that's being created.
        /// </summary>
        protected List<int> GetConflictingLocations(int locationType)
        {
            List<int> conflictingLocations = new List<int>();

            switch (locationType)
            {
                case (int)DFRegion.LocationTypes.TownCity:
                    conflictingLocations.Add((int)DFRegion.LocationTypes.TownCity);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.TownHamlet);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.Coven);
                    break;

                case (int)DFRegion.LocationTypes.TownHamlet:
                    conflictingLocations.Add((int)DFRegion.LocationTypes.TownCity);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.TownHamlet);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.TownVillage);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.DungeonLabyrinth);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.Coven);
                    break;

                case (int)DFRegion.LocationTypes.TownVillage:
                    conflictingLocations.Add((int)DFRegion.LocationTypes.TownCity);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.TownHamlet);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.TownVillage);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.DungeonLabyrinth);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.ReligionTemple);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.Tavern);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.DungeonKeep);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.HomeWealthy);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.Coven);
                    break;

                case (int)DFRegion.LocationTypes.HomeFarms:
                    conflictingLocations.Add((int)DFRegion.LocationTypes.DungeonLabyrinth);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.DungeonKeep);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.DungeonRuin);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.Graveyard);
                    break;

                case (int)DFRegion.LocationTypes.DungeonLabyrinth:
                    conflictingLocations.Add((int)DFRegion.LocationTypes.TownHamlet);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.TownVillage);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.HomeFarms);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.DungeonLabyrinth);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.ReligionTemple);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.Tavern);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.DungeonKeep);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.HomeWealthy);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.DungeonRuin);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.HomePoor);
                    break;

                case (int)DFRegion.LocationTypes.ReligionTemple:
                    conflictingLocations.Add((int)DFRegion.LocationTypes.TownVillage);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.DungeonLabyrinth);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.ReligionTemple);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.DungeonKeep);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.ReligionCult);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.Coven);
                    break;

                case (int)DFRegion.LocationTypes.Tavern:
                    conflictingLocations.Add((int)DFRegion.LocationTypes.TownVillage);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.DungeonLabyrinth);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.Tavern);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.DungeonKeep);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.Graveyard);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.Coven);
                    break;

                case (int)DFRegion.LocationTypes.DungeonKeep:
                    conflictingLocations.Add((int)DFRegion.LocationTypes.TownVillage);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.HomeFarms);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.DungeonLabyrinth);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.ReligionTemple);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.Tavern);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.DungeonKeep);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.HomeWealthy);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.DungeonRuin);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.HomePoor);
                    break;

                case (int)DFRegion.LocationTypes.HomeWealthy:
                    conflictingLocations.Add((int)DFRegion.LocationTypes.TownVillage);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.DungeonLabyrinth);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.DungeonKeep);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.HomeWealthy);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.DungeonRuin);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.Coven);
                    break;

                case (int)DFRegion.LocationTypes.ReligionCult:
                    conflictingLocations.Add((int)DFRegion.LocationTypes.ReligionTemple);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.ReligionCult);
                    break;

                case (int)DFRegion.LocationTypes.DungeonRuin:
                    conflictingLocations.Add((int)DFRegion.LocationTypes.HomeFarms);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.DungeonLabyrinth);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.DungeonKeep);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.HomeWealthy);
                    break;

                case (int)DFRegion.LocationTypes.HomePoor:
                    conflictingLocations.Add((int)DFRegion.LocationTypes.DungeonLabyrinth);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.DungeonKeep);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.HomePoor);
                    break;

                case (int)DFRegion.LocationTypes.Graveyard:
                    conflictingLocations.Add((int)DFRegion.LocationTypes.HomeFarms);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.Tavern);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.Graveyard);
                    break;

                case (int)DFRegion.LocationTypes.Coven:
                    conflictingLocations.Add((int)DFRegion.LocationTypes.TownCity);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.TownHamlet);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.TownVillage);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.ReligionTemple);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.Tavern);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.HomeWealthy);
                    conflictingLocations.Add((int)DFRegion.LocationTypes.Coven);
                    break;

                default:
                    break;
            }

            return conflictingLocations;
        }

        protected DFPosition[] GenerateSurfaceArray(int index, int surface)
        {
            List<DFPosition> surfaceList = new List<DFPosition>();

            for (int x = 0; x < MapsFile.MaxMapPixelX; x++)
            {
                for (int y = 0; y < MapsFile.MaxMapPixelY; y++)
                {
                    if (index == PoliticInfo.ConvertMapPixelToRegionIndex(x, y))
                    {
                        DFPosition position = new DFPosition(x, y);
                        surfaceList.Add(position);
                    }
                }
            }

            return surfaceList.ToArray();
        }

        protected string GenerateHomeName(int locationType, int regionalism)
        {
            int selectedEpithet;
            int homeNameStructure;
            string epithet;
            string resultingName;

            int bankType = ConvertRegionalismToBankType(regionalism);
            int gender = UnityEngine.Random.Range(0, 2);

            switch (locationType)
            {
                case (int)DFRegion.LocationTypes.HomePoor:
                    selectedEpithet = UnityEngine.Random.Range(0, Enum.GetNames(typeof(LocationNamesList.PoorHomeEpithets)).Length);
                    epithet = ((LocationNamesList.PoorHomeEpithets)selectedEpithet).ToString();
                    break;

                case (int)DFRegion.LocationTypes.HomeWealthy:
                    selectedEpithet = UnityEngine.Random.Range(0, Enum.GetNames(typeof(LocationNamesList.WealthyHomeEpithets)).Length);
                    epithet = ((LocationNamesList.WealthyHomeEpithets)selectedEpithet).ToString();
                    break;

                case (int)DFRegion.LocationTypes.HomeFarms:
                    selectedEpithet = UnityEngine.Random.Range(0, Enum.GetNames(typeof(LocationNamesList.FarmsteadEpithets)).Length);
                    epithet = ((LocationNamesList.FarmsteadEpithets)selectedEpithet).ToString();
                    break;

                default:
                    epithet = "error";
                    break;
            }

            homeNameStructure = UnityEngine.Random.Range(0, 4);
            string name;
            string surname;

            switch (homeNameStructure)
            {
                case 0:     // The + (sur)name + epithet
                    if (regionalism == (int)LocationNamesList.NameTypes.Hammerfell)
                        surname = DaggerfallUnity.Instance.NameHelper.FirstName(NameHelper.BankTypes.Redguard, (DaggerfallWorkshop.Game.Entity.Genders)gender);
                    else surname = DaggerfallUnity.Instance.NameHelper.Surname((NameHelper.BankTypes)bankType, UnityEngine.Random.Range(0, 2));

                    resultingName = string.Concat("The ", surname, " ", epithet);
                    break;

                case 1:     // (sur)name + epithet
                    if (regionalism == (int)LocationNamesList.NameTypes.Hammerfell)
                        surname = DaggerfallUnity.Instance.NameHelper.FirstName(NameHelper.BankTypes.Redguard, (DaggerfallWorkshop.Game.Entity.Genders)gender);
                    else surname = DaggerfallUnity.Instance.NameHelper.Surname((NameHelper.BankTypes)bankType, UnityEngine.Random.Range(0, 2));

                    resultingName = string.Concat(surname, " ", epithet);
                    break;

                case 2:     // Old + name + 's + epithet
                    name = DaggerfallUnity.Instance.NameHelper.FirstName((NameHelper.BankTypes)bankType, (DaggerfallWorkshop.Game.Entity.Genders)gender);

                    resultingName = string.Concat("Old ", name, "'s ", epithet);
                    break;

                case 3:     // The Old + (sur)name + epithet
                    if (regionalism == (int)LocationNamesList.NameTypes.Hammerfell)
                        surname = DaggerfallUnity.Instance.NameHelper.FirstName(NameHelper.BankTypes.Redguard, (DaggerfallWorkshop.Game.Entity.Genders)gender);
                    else surname = DaggerfallUnity.Instance.NameHelper.Surname((NameHelper.BankTypes)bankType, UnityEngine.Random.Range(0, 2));

                    resultingName = string.Concat("The Old ", surname, " ", epithet);
                    break;

                default:
                    resultingName = "error";
                    break;
            }

            return resultingName;
        }

        protected string GenerateDungeonName(int locationType, int regionalism, int dungeonType)
        {
            int selectedEpithet;
            int dungeonNameStructure;
            int title;
            int gender;
            int bankType = ConvertRegionalismToBankType(regionalism);
            int nameType;
            string epithet;
            string resultingName;
            int dungeonNameType;

            if (dungeonType == (int)DFRegion.DungeonTypes.Cemetery || (UnityEngine.Random.Range(0, 10) + 1 < 8))
                dungeonNameType = dungeonType;
            else dungeonNameType = Enum.GetNames(typeof(DFRegion.DungeonTypes)).Length;

            // 0: The (name)'s (epithet);
            // 1: (title) (name)'s (epithet);
            // 2: The (epithet) of (name);
            // 3: The (epithet) of (title) (name);
            dungeonNameStructure = UnityEngine.Random.Range(0, 4);

            // In case it is needed, choose a title (Lord, Baron, etc...);
            title = UnityEngine.Random.Range(0, Enum.GetNames(typeof(RegionManager.Ruler)).Length);

            // In case it is left to chance, choose a name type (First Name, Surname or Monster Name);
            nameType = UnityEngine.Random.Range(0, 3);

            // This should generate a random gender and, at the same time, make it coordinated with title
            if (title % 2 == 0)
                gender = 1;
            else gender = 0;

            switch (dungeonNameType)
            {
                case (int)DFRegion.DungeonTypes.Crypt:

                    selectedEpithet = UnityEngine.Random.Range(0, LocationNamesList.DungeonNamesList[dungeonType].Length);

                    // I'll change this so first names and surnames can be used too.

                    switch (dungeonNameStructure)
                    {
                        case 0:     // The (name)'s (epithet);
                            resultingName = string.Concat(GetName(nameType, gender, regionalism), "'s ", LocationNamesList.DungeonNamesList[(int)DFRegion.DungeonTypes.Crypt][selectedEpithet]);
                            return resultingName;

                        case 1:     // (title) (name)'s (epithet);
                            resultingName = string.Concat(((RegionManager.Ruler)title).ToString(), " ", GetName(nameType, gender, regionalism), "'s ", LocationNamesList.DungeonNamesList[(int)DFRegion.DungeonTypes.Crypt][selectedEpithet]);
                            return resultingName;

                        case 2:     // The (epithet) of (name);
                            resultingName = string.Concat("The ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet], " of ", GetName(nameType, gender, regionalism));
                            return resultingName;

                        case 3:     // The (epithet) of (title) (name);
                        default:
                            resultingName = string.Concat("The ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet], " of ", ((RegionManager.Ruler)title).ToString(), " ", GetName(nameType, gender, regionalism));
                            return resultingName;
                    }
                    // "(monster name)'s Barrow";
                    // "(title) (monster name)'s Barrow";
                    // "The Barrow of (monster name)";
                    // "The Barrow of (title) (monster name)"; // Not present, but could be added?
                    // "The Cairn of (monster name)";
                    // "The Cairn of (title) (monster name)";
                    // "Castle (family name)";     // If by crypt we just consider it a place full of undeads, it should be ok
                    // "The Citadel of (family name)";
                    // "The Crypt of (monster name)";
                    // "The Fortress of (family name)";
                    // "The Grave of (monster name)";
                    // "The Grave of (title) (monster name)";
                    // "(family name)'s Guard";
                    // "(family name)'s Hold";
                    // "The Hold of (family name)";
                    // "The Masoleum of (monster name)";
                    // "Ruins of The (Cabin)";
                    // "Ruins of (Castle)";
                    // "Ruins of The (Citadel)";
                    // "Ruins of (Court)";
                    // "Ruins of (Old Farm)";
                    // "Ruins of The (Farmstead)";
                    // "Ruins of (Grange)";
                    // "Ruins of (Guard)";
                    // "Ruins of (Hall)";
                    // "Ruins of The (Hold)";
                    // "Ruins of The (Old Hovel)";
                    // "Ruins of (Manor)";
                    // "Ruins of (Orchard)";
                    // "Ruins of (Old Place)";
                    // "Ruins of The (Old Place)";
                    // "Ruins of (Old Shack)";
                    // "Ruins of The (Old Shack)";
                    // "Ruins of (Tower)";
                    // "The Sepulcher of (monster name)";
                    // "The Stronghold of (family name)";
                    // "(monster name)'s Tomb";
                    // "The Tomb of (monster name)";
                    // "Tower";
                    // "The Vault of (monster name)";
                    // "The Vault of (title) (monster name)";

                case (int)DFRegion.DungeonTypes.HumanStronghold:
                    selectedEpithet = UnityEngine.Random.Range(0, LocationNamesList.DungeonNamesList[dungeonType].Length);

                    // "Guard" can use only the last structure;
                    if ((LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet].ToString()).Equals("Guard"))
                        dungeonNameStructure += 3;

                    // "Hold" and "Stronghold" can only use the last two structures;
                    else if (((LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]).ToString()).Equals("Hold") ||
                             ((LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]).ToString()).Equals("Stronghold"))
                        dungeonNameStructure += 2;

                    switch (dungeonNameStructure)
                    {
                        case 0:
                        case 1:     // (epithet) (male name)
                            resultingName = string.Concat((LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]).ToString(), " ", GetName(0, gender, regionalism));
                            return resultingName;

                        case 2:     // The (epithet) of (male name)
                            resultingName = string.Concat("The ", (LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]).ToString(), " of ", GetName(0, gender, regionalism));
                            return resultingName;

                        case 3:     // (male name)'s (epithet)
                        default:
                            resultingName = string.Concat(GetName(0, gender, regionalism), "'s ", LocationNamesList.DungeonNamesList[(int)DFRegion.DungeonTypes.Crypt][selectedEpithet]);
                            return resultingName;
                    }                  
                    // "Castle (family name)";
                    // "Castle (male name)";
                    // "The Citadel of (family name)";
                    // "The Fortress of (family name)";
                    // "(male name)'s Guard";
                    // "(family name)'s Guard";
                    // *"(family name)'s Hall";
                    // "(family name)'s Hold";
                    // "(male name)'s Hold";
                    // "The Hold of (male name)";
                    // "Ruins of The (Cabin)";
                    // "Ruins of (Castle)";
                    // "Ruins of The (Citadel)";
                    // "Ruins of (Court)";
                    // "Ruins of (Old Farm)";
                    // "Ruins of The (Farmstead)";
                    // "Ruins of The (Fortress)";
                    // "Ruins of (Grange)";
                    // "Ruins of (Guard)";
                    // "Ruins of (Hall)";
                    // "Ruins of (Hold)";
                    // "Ruins of (Manor)";
                    // "Ruins of (Orchard)";
                    // "Ruins of (Old Place)";
                    // "Ruins of The (Plantation)";
                    // "Ruins of (Old Shack)";
                    // "Ruins of The (Old Shack)";
                    // "Ruins of (Tower)";
                    // "Ruins of The (Tower)";
                    // "The Stronghold of (male name)";
                    // "The Stronghold of (family name)";
                    // "Tower";

                case (int)DFRegion.DungeonTypes.Prison:

                    selectedEpithet = UnityEngine.Random.Range(0, LocationNamesList.DungeonNamesList[dungeonType].Length);

                    switch (dungeonNameStructure)
                    {
                        case 0:
                        case 1:     // The (name) (epithet);
                            resultingName = string.Concat("The ", GetName(nameType, gender, regionalism), " ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;

                        case 2:     // The (epithet) of (name);
                            resultingName = string.Concat("The ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet], " of ", GetName(nameType, gender, regionalism));
                            return resultingName;

                        case 3:     // (name)'s (epithet);
                        default:
                            resultingName = string.Concat(GetName(nameType, gender, regionalism), "'s ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;
                    }
                    // "The (male name) Asylum";
                    // "Castle (family name)";
                    // "The Citadel of (family name)";
                    // "The Dungeon of (male name)";
                    // "The Fortress of (family name)";
                    // "(male name)'s Gaol";
                    // "The Gaol of (male name)";
                    // "(family name)'s Guard";
                    // "(family name)'s Hold";
                    // "The Hold of (family name)";
                    // "The (male name) House of Correction";
                    // "The Penitentiary of (male name)";
                    // "The (male name) Prison";
                    // "The Prison of (male name)";
                    // "The (male name) Reformatory";
                    // "Ruins of The (Cabin)";
                    // "Ruins of (Castle)";
                    // "Ruins of (Court)";
                    // "Ruins of (Old Farm)";
                    // "Ruins of The (Farm)";
                    // "Ruins of The (Farmstead)";
                    // "Ruins of (Grange)";
                    // "Ruins of (Guard)";
                    // "Ruins of (Hall)";
                    // "Ruins of (Hold)";
                    // "Ruins of The (Hold)";
                    // "Ruins of (Old Hovel)";
                    // "Ruins of The (Old Hovel)";
                    // "Ruins of (Manor)";
                    // "Ruins of (Orchard)";
                    // "Ruins of (Palace)";
                    // "Ruins of (Old Place)";
                    // "Ruins of The (Old Place)";
                    // "Ruins of The (Plantation)";
                    // "Ruins of (Old Shack)";
                    // "Ruins of The (Old Shack)";
                    // "Ruins of (Tower)";
                    // "Ruins of The (Tower)";
                    // "The Stronghold of (family name)";
                    // "Tower";
                
                case (int)DFRegion.DungeonTypes.DesecratedTemple:

                    selectedEpithet = UnityEngine.Random.Range(0, LocationNamesList.DungeonNamesList[dungeonType].Length);

                    switch (dungeonNameStructure)
                    {
                        case 0:     // (name) (epithet);
                            resultingName = string.Concat(GetName(nameType, gender, regionalism), " ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;

                        case 1:     // The (name) (epithet);
                            resultingName = string.Concat("The ", GetName(nameType, gender, regionalism), " ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;

                        case 2:     // The (epithet) of (name);
                            resultingName = string.Concat("The ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet], " of ", GetName(nameType, gender, regionalism));
                            return resultingName;

                        case 3:     // (male name)'s (epithet);
                        default:
                            resultingName = string.Concat(GetName(nameType, gender, regionalism), "'s ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;
                    }
                    // "The Abbey of (monster name)";
                    // "Castle (family name)";     // Not sure this should be kept, doesn't make much sense
                    // "The Cathedral of (monster name)";
                    // "The Citadel of (family name)";
                    // "The (monster name) Cloister";
                    // "The Convent of (female name)";
                    // "The Fortress of (family name)";
                    // "The Friary of (monster name)";
                    // "(family name)'s Guard";
                    // "The (monster name) Hermitage";
                    // "(family name)'s Hold";
                    // "The Hold of (family name)";
                    // "The (monster name) Manse";
                    // "(monster name) Minster";
                    // "The (monster name) Monastery";
                    // "The Monastery of (monster name)";
                    // "The (monster name) Rectory";
                    // "Ruins of The (Cabin)";
                    // "Ruins of (Castle)";
                    // "Ruins of (Court)";
                    // "Ruins of (Old Farm)";
                    // "Ruins of The (Farmstead)";
                    // "Ruins of The (Fortress)";
                    // "Ruins of (Grange)";
                    // "Ruins of (Hall)";
                    // "Ruins of (Hold)";
                    // "Ruins of The (Hold)";
                    // "Ruins of (Old Hovel)";
                    // "Ruins of The (Old Hovel)";
                    // "Ruins of (Manor)";
                    // "Ruins of (Orchard)";
                    // "Ruins of (Palace)";
                    // "Ruins of (Old Place)";
                    // "Ruins of The (Old Place)";
                    // "Ruins of The (Plantation)";
                    // "Ruins of (Old Shack)";
                    // "Ruins of The (Old Shack)";
                    // "Ruins of (Tower)";
                    // "Ruins of The (Tower)";
                    // "The (monster name) Shrine";
                    // "The Shrine of (monster name)";
                    // "The Stronghold of (family name)";
                    // "The Temple of (monster name)";
                    // "Tower";

                case (int)DFRegion.DungeonTypes.Mine:

                    selectedEpithet = UnityEngine.Random.Range(0, LocationNamesList.DungeonNamesList[dungeonType].Length);

                    if ((LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet].Equals("Excavation") ||
                         LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet].Equals("Lode")) &&
                         nameType == 2)
                        nameType = 0;

                    switch (dungeonNameStructure)
                    {
                        case 0:
                        case 1:     // The (name) (epithet);
                            resultingName = string.Concat("The ", GetName(nameType, gender, regionalism), " ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;

                        case 2:     // The (epithet) of (name);
                            resultingName = string.Concat("The ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet], " of ", GetName(nameType, gender, regionalism));
                            return resultingName;

                        case 3:     // (male name)'s (epithet);
                        default:
                            resultingName = string.Concat(GetName(nameType, gender, regionalism), "'s ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;
                    }
                    // "Castle (family name)";         // Not sure this should be kept, doesn't make much sense
                    // "The Citadel of (family name)";
                    // "The (male name) Excavation";
                    // "The Fortress of (family name)";
                    // "(family name)'s Guard";
                    // "(family name)'s Hold";
                    // "The Hold of (family name)";
                    // "The (male name) Lode";
                    // "The (male name) Mine";
                    // "The Mines of (male name)";
                    // "The (male name) Pit";
                    // "The (male name) Quarry";
                    // "The Quarry of (male name)";
                    // "Ruins of The (Cabin)";
                    // "Ruins of (Castle)";
                    // "Ruins of (Court)";
                    // "Ruins of (Old Farm)";
                    // "Ruins of The (Farmstead)";
                    // "Ruins of (Grange)";
                    // "Ruins of (Hall)";
                    // "Ruins of The (Hold)";
                    // "Ruins of (Old Hovel)";
                    // "Ruins of (Manor)";
                    // "Ruins of The (Old Place)";
                    // "Ruins of The (Plantation)";
                    // "Ruins of (Old Shack)";
                    // "Ruins of The (Old Shack)";
                    // "Ruins of The (Tower)";
                    // "The Stronghold of (family name)";
                    // "Tower";
                    // "The (male name) Tunnel";

                case (int)DFRegion.DungeonTypes.NaturalCave:

                    selectedEpithet = UnityEngine.Random.Range(0, LocationNamesList.DungeonNamesList[dungeonType].Length);

                    switch (dungeonNameStructure)
                    {
                        case 0:     // (name) (epithet);
                            resultingName = string.Concat(GetName(nameType, gender, regionalism), " ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;

                        case 1:     // The (name) (epithet);
                            resultingName = string.Concat("The ", GetName(nameType, gender, regionalism), " ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;

                        case 2:     // The (epithet) of (name);
                        case 3:
                        default:
                            resultingName = string.Concat("The ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet], " of ", GetName(nameType, gender, regionalism));
                            return resultingName;
                    }
                    // "Castle (family name)";             // Not sure this should be kept, doesn't make much sense
                    // "The (monster name) Cave";
                    // "The Cave of (monster name)";
                    // "The (monster name) Cavern";
                    // "The Cavern of (monster name)";
                    // "The Citadel of (family name)";     // Not sure this should be kept, doesn't make much sense
                    // "The Fortress of (family name)";    // Not sure this should be kept, doesn't make much sense
                    // "The (monster name) Grotto";
                    // "The Grotto of (monster name)";
                    // "(family name)'s Guard";            // Not sure this should be kept, doesn't make much sense
                    // "(family name)'s Hold";
                    // "The Hold of (family name)";
                    // "The (monster name) Hole";
                    // "The Hole of (monster name)";
                    // "(monster name) Hollow";
                    // "The Lair of (monster name)";
                    // "The Pit of (monster name)";
                    // "Ruins of The (Cabin)";
                    // "Ruins of (Castle)";
                    // "Ruins of (Court)";
                    // "Ruins of (Old Farm)";
                    // "Ruins of The (Farmstead)";
                    // "Ruins of (Grange)";
                    // "Ruins of (Hall)";
                    // "ruins of (Hold)";
                    // "Ruins of The (Hold)";
                    // "Ruins of (Old Hovel)";
                    // "Ruins of (Manor)";
                    // "Ruins of (Orchard)";
                    // "Ruins of (Palace)";
                    // "Ruins of The (Old Place)";
                    // "Ruins of (Old Shack)";
                    // "Ruins of (Tower)";
                    // "Ruins of The (Tower)";
                    // "The Stronghold of (family name)";
                    // "Tower";

                case (int)DFRegion.DungeonTypes.Coven:

                    selectedEpithet = UnityEngine.Random.Range(0, LocationNamesList.DungeonNamesList[dungeonType].Length);

                    // For this type of dungeon, names are always first names and feminine.
                    nameType = 0;
                    gender = 0;

                    switch (dungeonNameStructure)
                    {
                        case 0:
                        case 1:     // The (name) (epithet);
                            resultingName = string.Concat("The ", GetName(nameType, gender, regionalism), " ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;

                        case 2:     // The (epithet) of (name);
                            resultingName = string.Concat("The ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet], " of ", GetName(nameType, gender, regionalism));
                            return resultingName;

                        case 3:     // (name)'s (epithet);
                        default:
                            resultingName = string.Concat(GetName(nameType, gender, regionalism), "'s ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;
                    }
                    // "The (female name) Cabal";
                    // "The Cabal of (female name)";
                    // "Castle (family name)";
                    // "The Citadel of (family name)";
                    // "The Coven of (female name)";
                    // "The (female name) Coven";
                    // "The (male name?) Coven";
                    // "The Cult of (female name)";
                    // "(female name) Cultus";
                    // "The Fortress of (family name)";
                    // "(female name?)'s Guard";
                    // "(family name)'s Hold";
                    // "The Hold of (family name)";
                    // "Ruins of The (Cabin)";
                    // "Ruins of (Court)";
                    // "Ruins of The (Farmstead)";
                    // "Ruins of (Grange)";
                    // "Ruins of (Guard)";
                    // "Ruins of (Hall)";
                    // "Ruins of (Old Hovel)";
                    // "Ruins of The (Old Hovel)";
                    // "Ruins of (Manor)";
                    // "Ruins of (Palace)";
                    // "Ruins of (Old Place)";
                    // "Ruins of The (Old Place)";
                    // "Ruins of (Old Shack)";
                    // "Ruins of (Tower)";
                    // "The Stronghold of (family name)";
                    // "Tower";
                    // "The (female name) Coven";

                case (int)DFRegion.DungeonTypes.VampireHaunt:

                    selectedEpithet = UnityEngine.Random.Range(0, LocationNamesList.DungeonNamesList[dungeonType].Length);

                    string eventualTitle = "";
                    if (UnityEngine.Random.Range(0, 10) + 1 > 8)
                        eventualTitle = string.Concat(((RegionManager.Ruler)title).ToString(), " ");

                    switch (dungeonNameStructure)
                    {
                        case 0:     // [title] (name) (epithet);
                            resultingName = string.Concat(eventualTitle, GetName(nameType, gender, regionalism), " ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;

                        case 1:     // The (name) (epithet);
                            resultingName = string.Concat("The ", GetName(nameType, gender, regionalism), " ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;

                        case 2:     // The (epithet) of [title] (name);
                            resultingName = string.Concat("The ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet], " of ", eventualTitle, GetName(nameType, gender, regionalism));
                            return resultingName;

                        case 3:     // [title] (name)'s (epithet);
                        default:
                            resultingName = string.Concat(eventualTitle, GetName(nameType, gender, regionalism), "'s ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;
                    }
                    // "Castle (family name)";
                    // "Castle (monster name)";
                    // "Castle (title) (monster name)";
                    // "The Citadel of (family name)";
                    // "The Fortress of (family name)";
                    // "(family name)'s Guard";
                    // "(monster name) Hall";
                    // "(title) (monster name) Hall";
                    // "The Haunt of (monster name)";
                    // "The Haunt of (title) (monster name)";
                    // "(family name)'s Hold";
                    // "The Hold of (family name)";
                    // "The House of (monster name)";
                    // "The House of (title) (monster name)";
                    // "Ruins of The (Cabin)";
                    // "Ruins of (Castle)";
                    // "Ruins of (Court)";
                    // "Ruins of (Old Farm)";
                    // "Ruins of The (Farmstead)";
                    // "Ruins of The (Fortress)";
                    // "Ruins of (Hall)";
                    // "Ruins of (Hold)";
                    // "Ruins of The (Hold)";
                    // "Ruins of (Old Hovel)";
                    // "Ruins of The (Old Hovel)";
                    // "Ruins of (Manor)";
                    // "Ruins of (Orchard)";
                    // "Ruins of (Palace)";
                    // "Ruins of (Old Place)";
                    // "Ruins of The (Old Shack)";
                    // "Ruins of (Tower)";
                    // "The Stronghold of (family name)";
                    // "Tower";

                case (int)DFRegion.DungeonTypes.Laboratory:

                    selectedEpithet = UnityEngine.Random.Range(0, LocationNamesList.DungeonNamesList[dungeonType].Length);

                    switch (dungeonNameStructure)
                    {
                        case 0:     // (name) (epithet);
                            resultingName = string.Concat(GetName(nameType, gender, regionalism), " ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;

                        case 1:     // The (name) (epithet);
                            resultingName = string.Concat("The ", GetName(nameType, gender, regionalism), " ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;

                        case 2:     // The (epithet) of (name);
                            resultingName = string.Concat("The ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet], " of ", GetName(nameType, gender, regionalism));
                            return resultingName;

                        case 3:     // (name)'s (epithet);
                        default:
                            resultingName = string.Concat(GetName(nameType, gender, regionalism), "'s ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;
                    }
                    // "The (male name) Academy";
                    // "Castle (family name)";
                    // "The Citadel of (family name)";
                    // "The Fortress of (family name)";
                    // "(family name)'s Guard";
                    // "(family name)'s Hold";
                    // "The Hold of (family name)";
                    // "(male name) Laboratory";
                    // "The Laboratory of (male name)";
                    // "Ruins of The (Cabin)";
                    // "Ruins of (Castle)";
                    // "Ruins of The (Farmstead)";
                    // "Ruins of (Grange)";
                    // "Ruins of (Hall)";
                    // "Ruins of (Hold)";
                    // "Ruins of (Old Hovel)";
                    // "Ruins of The (Old Hovel)";
                    // "Ruins of (Manor)";
                    // "Ruins of (Palace)";
                    // "Ruins of (Old Place)";
                    // "Ruins of (Old Shack)";
                    // "Ruins of (Tower)";
                    // "The Stronghold of (family name)";
                    // "Tower";

                case (int)DFRegion.DungeonTypes.HarpyNest:

                    selectedEpithet = UnityEngine.Random.Range(0, LocationNamesList.DungeonNamesList[dungeonType].Length);

                    switch (dungeonNameStructure)
                    {
                        case 0:     // (name) (epithet);
                            resultingName = string.Concat(GetName(nameType, gender, regionalism), " ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;

                        case 1:     // The (name) (epithet);
                            resultingName = string.Concat("The ", GetName(nameType, gender, regionalism), " ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;

                        case 2:     // The (epithet) of (name);
                            resultingName = string.Concat("The ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet], " of ", GetName(nameType, gender, regionalism));
                            return resultingName;

                        case 3:     // (name)'s (epithet);
                        default:
                            resultingName = string.Concat(GetName(nameType, gender, regionalism), "'s ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;
                    }
                    // "The (monster name) Aerie";
                    // "The (monster name) Aviary";
                    // "The Aviary of (monster name)";
                    // "Castle (family name)";
                    // "The Citadel of (family name)";
                    // "The (monster name) Coop";
                    // "The Fortress of (family name)";
                    // "(family name)'s Guard";
                    // "(family name)'s Hold";
                    // "The Hold of (family name)";
                    // "The (monster name) Mews";
                    // "The Nest of (monster name)";
                    // "The (monster name) Nest";
                    // "The Roost of (monster name)";
                    // "Ruins of The (Citadel)";
                    // "Ruins of (Old Farm)";
                    // "Ruins of The (Farmstead)";
                    // "Ruins of (Grange)";
                    // "Ruins of (Guard)";
                    // "Ruins of (Hall)";
                    // "Ruins of (Old Hovel)";
                    // "Ruins of The (Old Hovel)";
                    // "Ruins of (Manor)";
                    // "Ruins of (Orchard)";
                    // "Ruins of (Palace)";
                    // "Ruins of (Old Place)";
                    // "Ruins of (Tower)";
                    // "The Stronghold of (family name)";
                    // "Tower";

                case (int)DFRegion.DungeonTypes.RuinedCastle:

                    selectedEpithet = UnityEngine.Random.Range(0, LocationNamesList.DungeonNamesList[dungeonType].Length);

                    switch (dungeonNameStructure)
                    {
                        case 0:     // (name) (epithet);
                            resultingName = string.Concat(GetName(nameType, gender, regionalism), " ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;

                        case 1:     // The (name) (epithet);
                            resultingName = string.Concat("The ", GetName(nameType, gender, regionalism), " ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;

                        case 2:     // The (epithet) of (name);
                            resultingName = string.Concat("The ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet], " of ", GetName(nameType, gender, regionalism));
                            return resultingName;

                        case 3:     // (male name)'s (epithet);
                        default:
                            resultingName = string.Concat(GetName(nameType, gender, regionalism), "'s ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;
                    }
                    // "Castle (family name)";
                    // "The Castle of (family name)";
                    // "The Citadel of (family name)";
                    // "The Fortress of (family name)";
                    // "(family name)'s Guard";
                    // "(family name) Hall";
                    // "(family name)'s Hold";
                    // "The Hold of (family name)";
                    // "The House of (family name)";
                    // "Ruins of The (Cabin)";
                    // "Ruins of (Old Farm)";
                    // "Ruins of The (Farmstead)";
                    // "Ruins of (Grange)";
                    // "Ruins of (Guard)";
                    // "Ruins of (Hall)";
                    // "Ruins of (Hold)";
                    // "Ruins of The (Hold)";
                    // "Ruins of (Old Hovel)";
                    // "Ruins of The (Old Hovel)";
                    // "Ruins of (Manor)";
                    // "Ruins of (Orchard)";
                    // "Ruins of (Palace)";
                    // "Ruins of (Old Place)";
                    // "Ruins of The (Plantation)";
                    // "Ruins of (Old Shack)";
                    // "Ruins of The (Old Shack)";
                    // "Ruins of (Tower)";
                    // "Ruins of The (Tower)";
                    // "The (family name) Ruins";
                    // "The Stronghold of (family name)";
                    // "Tower";

                case (int)DFRegion.DungeonTypes.SpiderNest:

                    selectedEpithet = UnityEngine.Random.Range(0, LocationNamesList.DungeonNamesList[dungeonType].Length);

                    switch (dungeonNameStructure)
                    {
                        case 0:     // (name) (epithet);
                            resultingName = string.Concat(GetName(2, gender, regionalism), " ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;

                        case 1:     // The (name) (epithet);
                            resultingName = string.Concat("The ", GetName(2, gender, regionalism), " ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;

                        case 2:     // The (epithet) of (name);
                            resultingName = string.Concat("The ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet], " of ", GetName(2, gender, regionalism));
                            return resultingName;

                        case 3:     // (male name)'s (epithet);
                        default:
                            resultingName = string.Concat(GetName(2, gender, regionalism), "'s ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;
                    }
                    // "Castle (family name)";
                    // "The Citadel of (family name)";
                    // "The Fortress of (family name)";
                    // "(family name)'s Guard";
                    // "(family name)'s Hold";
                    // "The Hold of (family name)";
                    // "The Lair of (monster name)";
                    // "The (monster name) Nest";
                    // "(monster name)'s Nest";
                    // "The Nest of (monster name)";
                    // "Ruins of The (Cabin)";
                    // "Ruins of (Castle)";
                    // "Ruins of (Old Farm)";
                    // "Ruins of (Grange)";
                    // "Ruins of (Hall)";
                    // "Ruins of (Hold)";
                    // "Ruins of (Old Hovel)";
                    // "Ruins of (Manor)";
                    // "Ruins of (Orchard)";
                    // "Ruins of (Old Place)";
                    // "Ruins of The (Old Place)";
                    // "Ruins of (Tower)";
                    // "Ruins of The (Tower)";
                    // "The Stronghold of (family name)";
                    // "Tower";
                    // "The (monster name) Web";
                    // "The Web of (monster name)";

                case (int)DFRegion.DungeonTypes.OrcStronghold:  // Temporarily Orc Stronghold generates the same names as Giant Stronghold                 
                    // "Castle (family name)";             // Should this be changed to Orc family name?
                    // "The Citadel of (family name)";     // Should this be changed to Orc family name? Maybe add the chance? Maybe only near Orsinium?
                    // "The Fortress of (family name)";    // Should this be changed to Orc family name?
                    // "(family name)'s Guard";            // Should this be changed to Orc family name?
                    // "(family name)'s Hold";
                    // "The Hold of (family name)";
                    // "Ruins of The (Cabin)";
                    // "Ruins of (Castle)";
                    // "Ruins of (Court)";
                    // "Ruins of (Hall)";
                    // "Ruins of (Hold)";
                    // "Ruins of The (Hold)";
                    // "Ruins of (Old Hovel)";
                    // "Ruins of (Manor)";
                    // "Ruins of (Orchard)";
                    // "Ruins of (Palace)";
                    // "Ruins of The (Old Shack)";
                    // "Ruins of (Tower)";
                    // "The Stronghold of (family name)";
                    // "Tower";
                    // "(monster name)";

                case (int)DFRegion.DungeonTypes.GiantStronghold:

                    // selectedEpithet = UnityEngine.Random.Range(0, LocationNamesList.DungeonNamesList[dungeonType].Length);

                    switch (dungeonNameStructure)
                    {
                        default:
                            resultingName = GetName(2, gender, regionalism);
                            return resultingName;
                    }
                    // "Castle (family name)";
                    // "The Citadel of (family name)";
                    // "The Fortress of (family name)";
                    // "(family name)'s Hold";
                    // "The Hold of (family name)";
                    // "Ruins of The (Cabin)";
                    // "Ruins of (Castle)";
                    // "Ruins of (Old Farm)";
                    // "Ruins of The (Farmstead)";
                    // "Ruins of (Grange)";
                    // "Ruins of (Guard)";
                    // "Ruins of (Hall)";
                    // "Ruins of (Hold)";
                    // "Ruins of The (Hold)";
                    // "Ruins of (Manor)";
                    // "Ruins of (Orchard)";
                    // "Ruins of (Palace)";
                    // "Ruins of (Tower)";
                    // "Ruins of The (Tower)";
                    // "The Stronghold of (family name)";
                    // "Tower";
                    // "(monster name)";

                case (int)DFRegion.DungeonTypes.DragonsDen:

                    selectedEpithet = UnityEngine.Random.Range(0, LocationNamesList.DungeonNamesList[dungeonType].Length);

                    switch (dungeonNameStructure)
                    {
                        case 0:     // (name) (epithet);
                            resultingName = string.Concat(GetName(nameType, gender, regionalism), " ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;

                        case 1:     // The (name) (epithet);
                            resultingName = string.Concat("The ", GetName(nameType, gender, regionalism), " ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;

                        case 2:     // The (epithet) of (name);
                            resultingName = string.Concat("The ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet], " of ", GetName(nameType, gender, regionalism));
                            return resultingName;

                        case 3:     // (male name)'s (epithet);
                        default:
                            resultingName = string.Concat(GetName(2, gender, regionalism), "'s ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;
                    }
                    // "Castle (family name)";
                    // "The Citadel of (family name)";
                    // "(monster name)'s Den";
                    // "The Den of (male name)";
                    // "The Fortress of (family name)";
                    // "The Lair of (male name)";
                    // "Ruins of (Castle)";
                    // "Ruins of (Grange)";
                    // "Ruins of (Hall)";
                    // "Ruins of The (Hold)";
                    // "Ruins of The (Old Hovel)";
                    // "Ruins of (Manor)";
                    // "Ruins of (Palace)";
                    // "Ruins of (Old Place)";
                    // "Ruins of The (Old Place)";
                    // "Ruins of (Tower)";
                    // "Ruins of The (Tower)";
                    // "The Stronghold of (family name)";
                    // "Tower";

                case (int)DFRegion.DungeonTypes.BarbarianStronghold:

                    selectedEpithet = UnityEngine.Random.Range(0, LocationNamesList.DungeonNamesList[dungeonType].Length);

                    switch (dungeonNameStructure)
                    {
                        case 0:     // (name) (epithet);
                            resultingName = string.Concat(GetName(nameType, gender, regionalism), " ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;

                        case 1:     // The (name) (epithet);
                            resultingName = string.Concat("The ", GetName(nameType, gender, regionalism), " ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;

                        case 2:     // The (epithet) of (name);
                            resultingName = string.Concat("The ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet], " of ", GetName(nameType, gender, regionalism));
                            return resultingName;

                        case 3:     // (male name)'s (epithet);
                        default:
                            resultingName = string.Concat(GetName(nameType, gender, regionalism), "'s ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;
                    }
                    // "The (female name) Assembly";
                    // "The Assembly of (female name)";
                    // "Castle (family name)";
                    // "The Circle of (female name)";
                    // "The Citadel of (family name)";
                    // "The Community of (female name)";
                    // "The Conclave of (female name)";
                    // "The (female name) Convergence";
                    // "The Convocation of (female name)";
                    // "The (female name) Council";
                    // "The Fortress of (family name)";
                    // "The Gathering of (female name)";
                    // "(family name)'s Hold";
                    // "The Hold of (family name)";
                    // "Ruins of The (Cabin)";
                    // "Ruins of (Castle)";
                    // "Ruins of (Old Farm)";
                    // "Ruins of The (Farmstead)";
                    // "Ruins of (Hall)";
                    // "Ruins of (Hold)";
                    // "Ruins of (Old Hovel)";
                    // "Ruins of The (Old Hovel)";
                    // "Ruins of (Manor)";
                    // "Ruins of (Orchard)";
                    // "Ruins of (Palace)";
                    // "Ruins of The (Old Place)";
                    // "Ruins of The (Old Shack)";
                    // "Ruins of (Tower)";
                    // "The Stronghold of (family name)";
                    // "Tower";

                case (int)DFRegion.DungeonTypes.VolcanicCaves:

                    selectedEpithet = UnityEngine.Random.Range(0, LocationNamesList.DungeonNamesList[dungeonType].Length);

                    switch (dungeonNameStructure)
                    {
                        case 0:     // (name) (epithet);
                            resultingName = string.Concat(GetName(2, gender, regionalism), " ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;

                        case 1:     // The (name) (epithet);
                            resultingName = string.Concat("The ", GetName(2, gender, regionalism), " ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;

                        case 2:     // The (epithet) of (name);
                            resultingName = string.Concat("The ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet], " of ", GetName(2, gender, regionalism));
                            return resultingName;

                        case 3:     // (male name)'s (epithet);
                        default:
                            resultingName = string.Concat(GetName(2, gender, regionalism), "'s ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;
                    }
                    // "Castle (family name)";     // Not sure this should be kept, doesn't make much sense
                    // "The (monster name) Cave";
                    // "The Cave of (monster name)";
                    // "The (monster name) Cavern";
                    // "The Cavern of (monster name)";
                    // "The Citadel of (family name)";     // Not sure this should be kept, doesn't make much sense
                    // "The Fortress of (family name)";    // Not sure this should be kept, doesn't make much sense
                    // "The (monster name) Grotto";
                    // "The Grotto of (monster name)";
                    // "(family name)'s Guard";
                    // "The Hold of (family name)";
                    // "The (monster name) Hole";
                    // "The Hole of (monster name)";
                    // "The Lair of (monster name)";
                    // "Ruins of The (Cabin)";
                    // "Ruins of (Old Farm)";
                    // "Ruins of The (Farmstead)";
                    // "Ruins of (Old Hovel)";
                    // "Ruins of (Palace)";
                    // "Ruins of (Old Shack)";
                    // "Tower";

                case (int)DFRegion.DungeonTypes.ScorpionNest:

                    selectedEpithet = UnityEngine.Random.Range(0, LocationNamesList.DungeonNamesList[dungeonType].Length);

                    switch (dungeonNameStructure)
                    {
                        case 0:     // (name) (epithet);
                            resultingName = string.Concat(GetName(2, gender, regionalism), " ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;

                        case 1:     // The (name) (epithet);
                            resultingName = string.Concat("The ", GetName(2, gender, regionalism), " ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;

                        case 2:     // The (epithet) of (name);
                            resultingName = string.Concat("The ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet], " of ", GetName(2, gender, regionalism));
                            return resultingName;

                        case 3:     // (male name)'s (epithet);
                        default:
                            resultingName = string.Concat(GetName(2, gender, regionalism), "'s ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;
                    }
                    // "Castle (family name)";
                    // "The Fortress of (family name)";
                    // "(family name)'s Guard";
                    // "(family name)'s Hold";
                    // "The Hold of (family name)";
                    // "The Lair of (monster name)";
                    // "(monster name)'s Nest";
                    // "The Nest of (monster name)";
                    // "The (monster name) Nest";
                    // "Ruins of The (Cabin)";
                    // "Ruins of (Castle)";
                    // "Ruins of The (Citadel)";
                    // "Ruins of (Court)";
                    // "Ruins of (Old Farm)";
                    // "Ruins of The (Farmstead)";
                    // "Ruins of (Grange)";
                    // "Ruins of (Hall)";
                    // "Ruins of The (Hold)";
                    // "Ruins of (Old Hovel)";
                    // "Ruins of The (Old Hovel)";
                    // "Ruins of (Manor)";
                    // "Ruins of (Old Place)";
                    // "Ruins of (Old Shack)";
                    // "Ruins of The (Old Shack)";
                    // "Ruins of (Tower)";
                    // "The Stronghold of (family name)";
                    // "Tower";
                    // "The Web of (monster name)";
                    // "(monster name)'s Web";

                case (int)DFRegion.DungeonTypes.Cemetery:

                    selectedEpithet = UnityEngine.Random.Range(0, LocationNamesList.DungeonNamesList[dungeonType].Length);

                    if (LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet].Equals("Graves"))
                        dungeonNameStructure = 1;

                    else if (LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet].Equals("Crypts") ||
                             LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet].Equals("Tombs"))
                        dungeonNameStructure = UnityEngine.Random.Range(0, 2);

                    else dungeonNameStructure = 0;

                    switch (dungeonNameStructure)
                    {
                        case 0:     // The (family name) (epithet) - BG, Cemetery, Crypts, Graveyard, Tombs, Vaults
                            resultingName = string.Concat("The ", GetName(1, gender, regionalism), " ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet]);
                            return resultingName;

                        case 1:     // The (epithet) of (family name) - Crypts, Graves, Tombs
                        default:
                            resultingName = string.Concat("The ", LocationNamesList.DungeonNamesList[dungeonType][selectedEpithet], " of ", GetName(1, gender, regionalism));
                            return resultingName;                        
                    }


                default:
                    selectedEpithet = UnityEngine.Random.Range(0, LocationNamesList.DungeonNamesList[DFRegion.DungeonTypeNames.Length].Length);
                    if (((LocationNamesList.DungeonNamesList[DFRegion.DungeonTypeNames.Length][selectedEpithet]).ToString()).Equals("Ruins"))
                    {
                        resultingName = GenerateRuinsName(regionalism, gender,dungeonType);
                        return resultingName;
                    }
                    else
                    {
                        resultingName = GenerateGenericDungeonName(regionalism, gender, dungeonType);
                        return resultingName;
                    }
            }
        }

        protected string GenerateRuinsName(int regionalism, int gender, int dungeonType)
        {
            // 0: Ruins of (home name);
            // 1: Ruins of (generic dungeon);
            int nameStructure = UnityEngine.Random.Range(0, 2);

            switch (nameStructure)
            {
                case 0:
                    int homeType = UnityEngine.Random.Range(0, 3);

                    switch (homeType)
                    {
                        case 0:
                            return string.Concat("Ruins of ", GenerateHomeName((int)DFRegion.LocationTypes.HomeFarms, regionalism));

                        case 1:
                            return string.Concat("Ruins of ", GenerateHomeName((int)DFRegion.LocationTypes.HomeWealthy, regionalism));

                        case 2:
                        default:
                            return string.Concat("Ruins of ", GenerateHomeName((int)DFRegion.LocationTypes.HomePoor, regionalism));
                    }

                case 1:
                default:
                    return string.Concat("Ruins of ", GenerateGenericDungeonName(regionalism, gender, dungeonType));
            }
        }

        protected string GenerateGenericDungeonName(int regionalism, int gender, int dungeonType)
        {
            int selectedEpithet = UnityEngine.Random.Range(0, LocationNamesList.DungeonNamesList[genericDungeonEpithets].Length);
            int nameStructure = UnityEngine.Random.Range(0, 4);
            string resultingName;

            if ((LocationNamesList.DungeonNamesList[genericDungeonEpithets][selectedEpithet].ToString()).Equals("Tower") &&
                 nameStructure > 3)
                nameStructure = 3;

            else if ((LocationNamesList.DungeonNamesList[genericDungeonEpithets][selectedEpithet].ToString()).Equals("Citadel") ||
                (LocationNamesList.DungeonNamesList[genericDungeonEpithets][selectedEpithet].ToString()).Equals("Fortress") ||
                (LocationNamesList.DungeonNamesList[genericDungeonEpithets][selectedEpithet].ToString()).Equals("Stronghold"))
                nameStructure =2;

            else if ((LocationNamesList.DungeonNamesList[genericDungeonEpithets][selectedEpithet].ToString()).Equals("Guard"))
                nameStructure = 3;

            else if ((LocationNamesList.DungeonNamesList[genericDungeonEpithets][selectedEpithet].ToString()).Equals("Hold") &&
                      nameStructure < 2)
                nameStructure++;

            switch (nameStructure)
            {
                case 0:     // (family name) (epithet) - tower
                    resultingName = string.Concat((LocationNamesList.DungeonNamesList[genericDungeonEpithets][selectedEpithet]), " ", GetName(1, gender, regionalism));
                    return resultingName;

                case 1:     // (epithet) (family name) - castle, tower
                    resultingName = string.Concat(LocationNamesList.DungeonNamesList[genericDungeonEpithets][selectedEpithet], " ", GetName(1, gender, regionalism));
                    return resultingName;
                
                case 2:     // The (epithet) of (family name) - castle, citadel, fortress, hold, stronghold, tower
                    resultingName = string.Concat("The ", LocationNamesList.DungeonNamesList[genericDungeonEpithets][selectedEpithet], " of ", GetName(1, gender, regionalism));
                    return resultingName;

                case 3:     // (family name)'s (epithet) - guard, hold
                default:
                    resultingName = string.Concat(GetName(1, gender, regionalism), "'s ", LocationNamesList.DungeonNamesList[genericDungeonEpithets][selectedEpithet]);
                    return resultingName;
            }
        }

        /// <summary>
        /// Provides a random name.
        /// </summary>
        /// <param name="nameType">The kind of name needed:
        /// 0: First Name;
        /// 1: Surname;
        /// 2: Monster Name.</param>
        /// <param name="gender">Gender selected for the name, when relevant.</param>
        /// <param name="regionalism">The name origin.</param>
        protected string GetName(int nameType, int gender = 0, int regionalism = 0)
        {
            int bankType = ConvertRegionalismToBankType(regionalism);
            Debug.Log("bankType: " + bankType);

            switch (nameType)
            {
                case 0:
                    return DaggerfallUnity.Instance.NameHelper.FirstName((NameHelper.BankTypes)bankType, (DaggerfallWorkshop.Game.Entity.Genders)gender);

                case 1:
                    if (bankType != (int)NameHelper.BankTypes.Redguard)
                        return DaggerfallUnity.Instance.NameHelper.Surname((NameHelper.BankTypes)bankType, UnityEngine.Random.Range(0, 2));
                    else return DaggerfallUnity.Instance.NameHelper.FirstName((NameHelper.BankTypes)bankType, (DaggerfallWorkshop.Game.Entity.Genders)(UnityEngine.Random.Range(0, 2)));

                case 2:
                    return DaggerfallUnity.Instance.NameHelper.MonsterName((DaggerfallWorkshop.Game.Entity.Genders)gender);

                default:
                    return "GetName() error";
            }
        }

        protected string GenerateTempleName(int locationType, int regionalism)
        {
            int relativeDeity = UnityEngine.Random.Range(0, 10) + 1;
            int adjectif = UnityEngine.Random.Range(0, LocationNamesList.TempleNamesList[(int)LocationNamesList.TempleParts.Noun].Length);
            int noun = UnityEngine.Random.Range(0, LocationNamesList.TempleNamesList[(int)LocationNamesList.TempleParts.Noun].Length);
            int deity = 0;;

            // Two out of ten chances that a temple won't be dedicated to the official region deity
            if (relativeDeity < 10) // I would have made it "< 9", Jehuty suggested to make it less frequent
                deity = ConvertReligionToDeity(WorldInfo.WorldSetting.regionTemples[RegionManager.currentRegionIndex]);
            else
            {
                do{
                    deity = UnityEngine.Random.Range(0, LocationNamesList.TempleNamesList[(int)LocationNamesList.TempleParts.Deity].Length);
                }
                while (deity == ConvertReligionToDeity(WorldInfo.WorldSetting.regionTemples[RegionManager.currentRegionIndex]));
            }

            switch (locationType)
            {
                case (int)DFRegion.LocationTypes.ReligionTemple:
                    return string.Concat(LocationNamesList.TempleNamesList[(int)LocationNamesList.TempleParts.Adjectif][adjectif], " ",
                                         LocationNamesList.TempleNamesList[(int)LocationNamesList.TempleParts.Noun][noun], " of ",
                                         LocationNamesList.TempleNamesList[(int)LocationNamesList.TempleParts.Deity][deity]);

                case (int)DFRegion.LocationTypes.ReligionCult:
                    return LocationNamesList.TempleNamesList[(int)LocationNamesList.TempleParts.Shrine][UnityEngine.Random.Range(0, LocationNamesList.TempleNamesList[(int)LocationNamesList.TempleParts.Shrine].Length)];

                default:
                    return "Error while generating temple name";
            }
        }

        protected int ConvertRegionalismToBankType(int regionalism)
        {
            switch (regionalism)
            {
                case (int)LocationNamesList.NameTypes.HighRockVanilla:
                    return (int)NameHelper.BankTypes.Breton;

                case (int)LocationNamesList.NameTypes.HighRockModern:
                    return (int)NameHelper.BankTypes.BretonModern;

                case (int)LocationNamesList.NameTypes.Hammerfell:
                    return (int)NameHelper.BankTypes.Redguard;

                case (int)LocationNamesList.NameTypes.Skyrim:
                case (int)LocationNamesList.NameTypes.Reachmen:
                    return (int)NameHelper.BankTypes.Nord;

                case (int)LocationNamesList.NameTypes.Morrowind:
                    return (int)NameHelper.BankTypes.DarkElf;

                case (int)LocationNamesList.NameTypes.Cyrodiil:
                    return (int)NameHelper.BankTypes.Imperial;

                case (int)LocationNamesList.NameTypes.SumursetIsle:
                    return (int)NameHelper.BankTypes.HighElf;

                case (int)LocationNamesList.NameTypes.Valenwood:
                    return (int)NameHelper.BankTypes.WoodElf;

                case (int)LocationNamesList.NameTypes.Elsweyr:
                    return (int)NameHelper.BankTypes.Khajiit;

                case (int)LocationNamesList.NameTypes.BlackMarsh:
                    return (int)NameHelper.BankTypes.Argonian;

                case (int)LocationNamesList.NameTypes.Orsinium:
                    return (int)NameHelper.BankTypes.Orc;

                default:
                    return (int)NameHelper.BankTypes.Breton;
            }
        }

        protected int ConvertReligionToDeity(int religion)
        {
            switch (religion)
            {
                case 36:        // Kynareth
                    return 4;

                case 82:        // Arkay
                    return 1;

                case 84:        // Zenithar
                    return 7;

                case 88:        // Mara
                    return 5;

                case 92:        // Akatosh
                    return 0;

                case 94:        // Julianos
                    return 3;

                case 98:        // Dibella
                    return 2;

                case 106:       // Stendarr
                    return 6;

                default:
                    return -1;
            }
        }

        protected DFLocation GenerateLocation(int locationType, DFPosition position, int dungeonType, string locationName, bool isCapital = false)
        {
            DFRegion.RegionMapTable generatedMapTable = new DFRegion.RegionMapTable();
            DFLocation generatedLocation = new DFLocation();

            Debug.Log("Generating location type " + (DFRegion.LocationTypes)locationType + " with coordinates " + position.X + ", " + position.Y + " (isCapital: " + isCapital + ")");

            switch (locationType)
            {
                case (int)DFRegion.LocationTypes.TownCity:
                case (int)DFRegion.LocationTypes.TownHamlet:
                case (int)DFRegion.LocationTypes.TownVillage:
                    generatedLocation = GenerateTown(locationType, position, locationName, isCapital);
                    break;

                case (int)DFRegion.LocationTypes.DungeonLabyrinth:
                case (int)DFRegion.LocationTypes.DungeonKeep:
                case (int)DFRegion.LocationTypes.DungeonRuin:
                case (int)DFRegion.LocationTypes.Graveyard:
                    generatedLocation = GenerateDungeon(locationType, position, dungeonType, locationName);
                    break;

                case (int)DFRegion.LocationTypes.HomeFarms:
                case (int)DFRegion.LocationTypes.Tavern:
                case (int)DFRegion.LocationTypes.HomeWealthy:
                case (int)DFRegion.LocationTypes.HomePoor:
                    generatedLocation = GenerateHome(locationType, position, locationName);
                    break;

                case (int)DFRegion.LocationTypes.ReligionCult:
                case (int)DFRegion.LocationTypes.ReligionTemple:
                    generatedLocation = GenerateTemple(locationType, position, locationName);
                    break;
            }

            return generatedLocation;
        }

        protected DFLocation GenerateTown(int locationType, DFPosition position, string townName, bool isCapital = false)
        {
            DFLocation generatedLocation = new DFLocation();

            generatedLocation.Loaded = true;
            generatedLocation.RegionName = WorldInfo.WorldSetting.RegionNames[RegionManager.currentRegionIndex];
            generatedLocation.HasDungeon = false;

            (int, int, int) townShape;

            GenerateTownSize(locationType, out townShape, isCapital);
            
            generatedLocation.MapTableData.MapId = (ulong)(position.Y * MapsFile.MaxMapPixelX + position.X);
            generatedLocation.MapTableData.Latitude = SetLocationLongitudeLatitude(position.X, position.Y, townShape.Item1, townShape.Item2, false);
            generatedLocation.MapTableData.Longitude = SetLocationLongitudeLatitude(position.X, position.Y, townShape.Item1, townShape.Item2, true);
            generatedLocation.MapTableData.LocationType = (DFRegion.LocationTypes)locationType;
            generatedLocation.MapTableData.DungeonType = DFRegion.DungeonTypes.NoDungeon;
            generatedLocation.MapTableData.Discovered = false;
            generatedLocation.MapTableData.Key = 0;
            generatedLocation.MapTableData.LocationId = GenerateNewLocationId();

            generatedLocation.Exterior.RecordElement.Header.X = SetPixelXY(position.X, position.Y, townShape.Item1, townShape.Item2, true);
            generatedLocation.Exterior.RecordElement.Header.Y = SetPixelXY(position.X, position.Y, townShape.Item1, townShape.Item2, false);
            generatedLocation.Exterior.RecordElement.Header.IsExterior = 32768;
            generatedLocation.Exterior.RecordElement.Header.Unknown2 = 0;
            generatedLocation.Exterior.RecordElement.Header.LocationId = generatedLocation.MapTableData.LocationId;
            generatedLocation.Exterior.RecordElement.Header.IsInterior = 0;
            generatedLocation.Exterior.RecordElement.Header.ExteriorLocationId = generatedLocation.Exterior.RecordElement.Header.LocationId;
            generatedLocation.Name = generatedLocation.Exterior.RecordElement.Header.LocationName = generatedLocation.Exterior.ExteriorData.AnotherName = townName;

            generatedLocation.Exterior.ExteriorData.MapId = generatedLocation.MapTableData.MapId;
            generatedLocation.Exterior.ExteriorData.LocationId = generatedLocation.MapTableData.LocationId;
            generatedLocation.Exterior.ExteriorData.Width = (byte)townShape.Item1;
            generatedLocation.Exterior.ExteriorData.Height = (byte)townShape.Item2;

            if (isCapital)
                if (regionOptions.capitalPort)
                {
                    generatedLocation.Exterior.ExteriorData.PortTownAndUnknown = 1;
                }
                else generatedLocation.Exterior.ExteriorData.PortTownAndUnknown = 0;
            else if (locationType != (int)DFRegion.LocationTypes.TownVillage)
                generatedLocation.Exterior.ExteriorData.PortTownAndUnknown = DetermineIfPortTown(position.X, position.Y);
            else generatedLocation.Exterior.ExteriorData.PortTownAndUnknown = 0;

            generatedLocation.Exterior.ExteriorData.BlockNames = new string[townShape.Item1 * townShape.Item2];

            if (locationType == (int)DFRegion.LocationTypes.TownCity && (DetermineIfWalled(townShape.Item1, townShape.Item2)) || (isCapital && regionOptions.capitalWalled))
            {
                string specialCondition = "";
                if (isCapital)
                    specialCondition = "capital";
                generatedLocation.Exterior.ExteriorData.Width += 2;
                generatedLocation.Exterior.ExteriorData.Height += 2;
                townShape.Item1 += 2;
                townShape.Item2 += 2;
                generatedLocation.Exterior.ExteriorData.BlockNames = new string[generatedLocation.Exterior.ExteriorData.Width * generatedLocation.Exterior.ExteriorData.Height];
                generatedLocation.Exterior.ExteriorData.BlockNames = PickBlockNames(locationType, townShape, generatedLocation.Exterior.ExteriorData.BlockNames.Length, position, true, specialCondition);
            }
            else generatedLocation.Exterior.ExteriorData.BlockNames = PickBlockNames(locationType, townShape, generatedLocation.Exterior.ExteriorData.BlockNames.Length, position);

            generatedLocation.Exterior.BuildingCount = (ushort)CountBuildings(generatedLocation.Exterior.ExteriorData.BlockNames);

            generatedLocation.Exterior.Buildings = new DFLocation.BuildingData[generatedLocation.Exterior.BuildingCount];

            int buildingCount = 0;

            for (int j = 0; j < townShape.Item1 * townShape.Item2; j++)
            {
                string blockReplacementJson = File.ReadAllText(Path.Combine(MapEditor.testPath, "RMB", string.Concat(generatedLocation.Exterior.ExteriorData.BlockNames[j], ".json")));
                DFBlock block = (DFBlock)SaveLoadManager.Deserialize(typeof(DFBlock), blockReplacementJson);
                // DFBlock block = JsonConvert.DeserializeObject<DFBlock>(File.ReadAllText(Path.Combine(MapEditor.testPath, "RMB", generatedLocation.Exterior.ExteriorData.BlockNames[j])));
                int partialBuildingCount = 0;

                do{
                    generatedLocation.Exterior.Buildings[buildingCount].NameSeed = (ushort)UnityEngine.Random.Range(0, ushort.MaxValue);
                    generatedLocation.Exterior.Buildings[buildingCount].FactionId = block.RmbBlock.FldHeader.BuildingDataList[partialBuildingCount].FactionId;
                    generatedLocation.Exterior.Buildings[buildingCount].Sector = block.RmbBlock.FldHeader.BuildingDataList[partialBuildingCount].Sector;
                    generatedLocation.Exterior.Buildings[buildingCount].BuildingType = block.RmbBlock.FldHeader.BuildingDataList[partialBuildingCount].BuildingType;
                    generatedLocation.Exterior.Buildings[buildingCount].Quality = (byte)UnityEngine.Random.Range(1, 21);

                    buildingCount++;
                    partialBuildingCount++;
                }
                while (partialBuildingCount < block.RmbBlock.FldHeader.BuildingDataList.Length);                
            }

            if (buildingCount != generatedLocation.Exterior.BuildingCount)
                Debug.Log("Buildings set and BuildingCount for location " + generatedLocation.Exterior.RecordElement.Header.LocationName + " don't correspond");

            generatedLocation.Dungeon = new DFLocation.LocationDungeon();
            generatedLocation.Climate = MapsFile.GetWorldClimateSettings(ClimateInfo.Climate[position.X, position.Y]);
            generatedLocation.Politic = PoliticInfo.Politic[position.X, position.Y];
            generatedLocation.RegionIndex = RegionManager.currentRegionIndex;
            generatedLocation.LocationIndex = 0; // TODO

            return generatedLocation;
        }

        protected void GenerateTownSize(int locationType, out (int, int, int) refinedSize, bool isCapital = false)
        {
            int scalableWidth;
            int scalableHeight;
            int width;
            int height;

            scalableWidth = UnityEngine.Random.Range(0, 100) + 1;
            scalableHeight = UnityEngine.Random.Range(0, 100) + 1;

            if (isCapital)
            {
                if (regionOptions.capitalSize.Item1 == null || regionOptions.capitalSize.Item2 == null)
                {
                    locationType = -1;
                    width = ElaborateScalableSize(locationType, scalableWidth);
                    height = ElaborateScalableSize(locationType, scalableHeight);
                }
                else{
                    width = regionOptions.capitalSize.Item1;
                    height = regionOptions.capitalSize.Item2;
                }
            }
            else{
                width = ElaborateScalableSize(locationType, scalableWidth);
                height = ElaborateScalableSize(locationType, scalableHeight);
            }

            switch (locationType)
            {
                case (int)DFRegion.LocationTypes.TownVillage:
                    refinedSize = RefineVillageSize(width, height);
                    break;

                 case (int)DFRegion.LocationTypes.TownHamlet:
                    refinedSize = RefineHamletSize(width, height);
                    break;

                case (int)DFRegion.LocationTypes.TownCity:
                default:
                    refinedSize = (width, height, (int)TownShapes.Regular);
                    break;
            }
        }

        protected (int, int, int) RefineHamletSize(int width, int height)
        {
            int randomChance = UnityEngine.Random.Range(0, 100) + 1;
            switch (width * height)
            {
                case 4:     // 5 blocks is the smallest hamlet possible
                    if (randomChance <= 50)
                        width++;
                    else height++;

                    return (width, height, (int)TownShapes.FiveOnSix);

                case 6:     // There's a small chance a 6 blocks hamlet will have a peculiar shape
                    if (randomChance > 95)
                    {
                        if (UnityEngine.Random.Range(0, 2) == 0)
                            width++;
                        else height++;

                        if (Math.Abs(width - height) == 2)
                            return (width, height, (int)TownShapes.SixOnEight);
                        else return (width, height, (int)TownShapes.MantaShape);
                    }
                    else return (width, height, (int)TownShapes.Regular);                    

                case 8:     // 4 x 2 blocks hamlet should be rare, 4 out of 5 times it's a 3 x 3 with a FILL
                    if (randomChance <= 80)
                    {
                        return (3, 3, (int)TownShapes.EightOnNine);
                    }
                    else return (width, height, (int)TownShapes.Regular);

                case 9:     // 3 x 3 blocks hamlet are usually just square towns, but there's a small chance it will be a 4 x 3 with 3 FILL
                    if (randomChance <= 80)
                    {
                        return (3, 3, (int)TownShapes.Regular);
                    }
                    else{
                        if (UnityEngine.Random.Range(0, 2) == 0)
                            width++;
                        else height++;

                        return (width, height, (int)TownShapes.NineOnTwelve);
                    }
                
                case 12:    // 3 x 4 blocks hamlet have a small chance to be wider, but with 4 FILL
                    if (randomChance <= 80)
                    {
                        return (width, height, (int)TownShapes.Regular);
                    }
                    else return (4, 4, (int)TownShapes.TwelveOnSixteen);

                case 16:    // max blocks for hamlets are 12, therefore a 4 x 4 hamlet has always 4 FILL
                    return (width, height, (int)TownShapes.TwelveOnSixteen);

                default:
                    return (width, height, (int)TownShapes.Regular);
            }
        }

        protected (int, int, int) RefineVillageSize(int width, int height)
        {
            switch (width * height)
            {
                case 1:     // 1 x 1 settlements are only homes
                    if (0 == UnityEngine.Random.Range(0, 2))
                        width++;
                    else height++;

                    return (width, height, (int)TownShapes.Regular);

                case 3:     // 3 x 1 settlements should be rare, 4 out of 5 times it's a 2 x 2 with a FILL
                    if (UnityEngine.Random.Range(0, 5) < 4)
                    {
                        return (2, 2, (int)TownShapes.LShape);
                    }
                    else return (width, height, (int)TownShapes.Regular);

                case 6:     // 4 should be the top limit blocks for villages
                    return (width, height, (int)TownShapes.FourOnSix);

                case 9:     // 3 x 3 is reduced to a 3 x 2, as above
                    if (UnityEngine.Random.Range(0, 2) == 0)
                        width--;
                    else height --;

                    return (width, height, (int)TownShapes.FourOnSix);

                default:
                    return (width, height, (int)TownShapes.Regular);
            }
        }

        protected int ElaborateScalableSize(int locationType, int scalableValue)
        {
            switch (locationType)
            {
                case (int)DFRegion.LocationTypes.TownCity:
                    if (scalableValue <= 80)
                        return 4;
                    else if (scalableValue <= 95)
                        return 5;
                    else return 6;

                case (int)DFRegion.LocationTypes.TownHamlet:
                    if (scalableValue <= 60)
                        return 2;
                    else if (scalableValue <= 90)
                        return 3;
                    else return 4;

                case (int)DFRegion.LocationTypes.TownVillage:
                    if (scalableValue <= 60)
                        return 1;
                    else if (scalableValue <= 90)
                        return 2;
                    else return 3;

                default:
                    return 0;                    
            }
        }

        protected DFLocation GenerateDungeon(int locationType, DFPosition position, int dungeonType, string dungeonName)
        {
            DFLocation generatedLocation = new DFLocation();

            generatedLocation.Loaded = true;
            generatedLocation.RegionName = WorldInfo.WorldSetting.RegionNames[RegionManager.currentRegionIndex];
            generatedLocation.HasDungeon = true;

            generatedLocation.MapTableData.MapId = (ulong)(position.Y * MapsFile.MaxMapPixelX + position.X);
            generatedLocation.MapTableData.Latitude = SetLocationLongitudeLatitude(position.X, position.Y, 1, 1, false);
            generatedLocation.MapTableData.Longitude = SetLocationLongitudeLatitude(position.X, position.Y, 1, 1, true);
            generatedLocation.MapTableData.LocationType = (DFRegion.LocationTypes)locationType;
            generatedLocation.MapTableData.DungeonType = (DFRegion.DungeonTypes)dungeonType;
            generatedLocation.MapTableData.Discovered = false;
            generatedLocation.MapTableData.Key = 0;
            generatedLocation.MapTableData.LocationId = GenerateNewLocationId();

            generatedLocation.Exterior.RecordElement.Header.X = SetPixelXY(position.X, position.Y, 1, 1, true);
            generatedLocation.Exterior.RecordElement.Header.Y = SetPixelXY(position.X, position.Y, 1, 1, false);
            generatedLocation.Exterior.RecordElement.Header.IsExterior = 32768;
            generatedLocation.Exterior.RecordElement.Header.Unknown2 = 0;
            generatedLocation.Exterior.RecordElement.Header.LocationId = generatedLocation.MapTableData.LocationId;
            generatedLocation.Exterior.RecordElement.Header.IsInterior = 0;
            generatedLocation.Exterior.RecordElement.Header.ExteriorLocationId = generatedLocation.Exterior.RecordElement.Header.LocationId;
            generatedLocation.Name = generatedLocation.Exterior.RecordElement.Header.LocationName = generatedLocation.Exterior.ExteriorData.AnotherName = dungeonName;

            generatedLocation.Exterior.ExteriorData.MapId = generatedLocation.MapTableData.MapId;
            generatedLocation.Exterior.ExteriorData.LocationId = generatedLocation.MapTableData.LocationId;
            generatedLocation.Exterior.ExteriorData.Width = 1;
            generatedLocation.Exterior.ExteriorData.Height = 1;

            generatedLocation.Exterior.ExteriorData.PortTownAndUnknown = 0;

            generatedLocation.Exterior.ExteriorData.BlockNames = new string[1];
            generatedLocation.Exterior.ExteriorData.BlockNames = PickDungeonExterior(locationType, position, dungeonType, dungeonName);

            generatedLocation.Dungeon.RecordElement.Header.X = generatedLocation.Exterior.RecordElement.Header.X;
            generatedLocation.Dungeon.RecordElement.Header.Y = generatedLocation.Exterior.RecordElement.Header.Y;
            generatedLocation.Dungeon.RecordElement.Header.IsExterior = 0;
            generatedLocation.Dungeon.RecordElement.Header.Unknown2 = 0;
            generatedLocation.Dungeon.RecordElement.Header.LocationId = (ushort)(generatedLocation.Exterior.RecordElement.Header.LocationId + 1);
            generatedLocation.Dungeon.RecordElement.Header.ExteriorLocationId = generatedLocation.Exterior.RecordElement.Header.LocationId;
            generatedLocation.Dungeon.RecordElement.Header.LocationName = generatedLocation.Name;            

            generatedLocation.Dungeon.Blocks = GenerateDungeonInterior(locationType, dungeonType, dungeonName);
            Debug.Log("Dungeon: " + dungeonName + "; Blocks generated: " + generatedLocation.Dungeon.Blocks.Length);
            generatedLocation.Dungeon.Header.BlockCount = (ushort)generatedLocation.Dungeon.Blocks.Length;

            generatedLocation.Climate = MapsFile.GetWorldClimateSettings(ClimateInfo.Climate[position.X, position.Y]);
            generatedLocation.Politic = PoliticInfo.Politic[position.X, position.Y];
            generatedLocation.RegionIndex = RegionManager.currentRegionIndex;
            generatedLocation.LocationIndex = 0; // TODO

            return generatedLocation;
        }

        protected DFLocation GenerateFarm(int locationType, Farm randomFarm, string homeName, int climateNumb)
        {
            DFLocation generatedLocation = new DFLocation();

            generatedLocation.Loaded = true;
            generatedLocation.RegionName = randomFarm.region;
            generatedLocation.HasDungeon = false;

            int farmSize = UnityEngine.Random.Range(1, 11);
            if (farmSize < 6)
                farmSize = 1;
            else if (farmSize < 9)
                farmSize = 3;
            else farmSize = 5;

            generatedLocation.MapTableData.MapId = (ulong)(randomFarm.worldY * 1000 + randomFarm.worldX);
            generatedLocation.MapTableData.Latitude = (499 - randomFarm.worldY) * 128;
            generatedLocation.MapTableData.Longitude = randomFarm.worldX * 128;
            generatedLocation.MapTableData.LocationType = DFRegion.LocationTypes.HomeFarms;
            generatedLocation.MapTableData.DungeonType = DFRegion.DungeonTypes.NoDungeon;
            generatedLocation.MapTableData.Discovered = false;
            generatedLocation.MapTableData.Key = 0;
            generatedLocation.MapTableData.LocationId = GenerateNewLocationId();

            generatedLocation.Exterior.RecordElement.Header.X = SetPixelXY(randomFarm.worldX, randomFarm.worldY, farmSize, farmSize, true);
            generatedLocation.Exterior.RecordElement.Header.Y = SetPixelXY(randomFarm.worldX, randomFarm.worldY, farmSize, farmSize, false);
            generatedLocation.Exterior.RecordElement.Header.IsExterior = 32768;
            generatedLocation.Exterior.RecordElement.Header.Unknown2 = 0;
            generatedLocation.Exterior.RecordElement.Header.LocationId = generatedLocation.MapTableData.LocationId;
            generatedLocation.Exterior.RecordElement.Header.IsInterior = 0;
            generatedLocation.Exterior.RecordElement.Header.ExteriorLocationId = generatedLocation.Exterior.RecordElement.Header.LocationId;
            generatedLocation.Name = generatedLocation.Exterior.RecordElement.Header.LocationName = generatedLocation.Exterior.ExteriorData.AnotherName = homeName;

            generatedLocation.Exterior.ExteriorData.MapId = generatedLocation.MapTableData.MapId;
            generatedLocation.Exterior.ExteriorData.LocationId = generatedLocation.MapTableData.LocationId;
            generatedLocation.Exterior.ExteriorData.Width = (byte)farmSize;
            generatedLocation.Exterior.ExteriorData.Height = (byte)farmSize;

            generatedLocation.Exterior.ExteriorData.PortTownAndUnknown = 0;

            generatedLocation.Exterior.ExteriorData.BlockNames = new string[farmSize * farmSize];
            generatedLocation.Exterior.ExteriorData.BlockNames = PickFarmExterior((farmSize * farmSize), new DFPosition(randomFarm.worldX, randomFarm.worldY), randomFarm.climate);

            generatedLocation.Exterior.BuildingCount = (ushort)CountBuildings(generatedLocation.Exterior.ExteriorData.BlockNames);
            generatedLocation.Exterior.Buildings = new DFLocation.BuildingData[generatedLocation.Exterior.BuildingCount];

            int buildingCount = 0;

            for (int j = 0; j < farmSize * farmSize; j++)
            {
                string blockReplacementJson = File.ReadAllText(Path.Combine(MapEditor.testPath, "RMB", string.Concat(generatedLocation.Exterior.ExteriorData.BlockNames[j], ".json")));
                DFBlock block = (DFBlock)SaveLoadManager.Deserialize(typeof(DFBlock), blockReplacementJson);
                // DFBlock block = JsonConvert.DeserializeObject<DFBlock>(File.ReadAllText(Path.Combine(MapEditor.testPath, "RMB", generatedLocation.Exterior.ExteriorData.BlockNames[0])));
                int partialBuildingCount = 0;

                do
                {
                    generatedLocation.Exterior.Buildings[buildingCount].NameSeed = (ushort)UnityEngine.Random.Range(0, ushort.MaxValue);
                    generatedLocation.Exterior.Buildings[buildingCount].FactionId = block.RmbBlock.FldHeader.BuildingDataList[partialBuildingCount].FactionId;
                    generatedLocation.Exterior.Buildings[buildingCount].Sector = block.RmbBlock.FldHeader.BuildingDataList[partialBuildingCount].Sector;
                    generatedLocation.Exterior.Buildings[buildingCount].BuildingType = block.RmbBlock.FldHeader.BuildingDataList[partialBuildingCount].BuildingType;
                    generatedLocation.Exterior.Buildings[buildingCount].Quality = (byte)UnityEngine.Random.Range(1, 21);

                    buildingCount++;
                    partialBuildingCount++;
                }
                while (partialBuildingCount < block.RmbBlock.FldHeader.BuildingDataList.Length);
            }

            if (buildingCount != generatedLocation.Exterior.BuildingCount)
                Debug.Log("Buildings set and BuildingCount for location " + generatedLocation.Exterior.RecordElement.Header.LocationName + " don't correspond: buildingCount: " + buildingCount + ", generatedLocation.Exterior.BuildingCount: " + generatedLocation.Exterior.BuildingCount);

            generatedLocation.Dungeon = new DFLocation.LocationDungeon();
            generatedLocation.Climate = MapsFile.GetWorldClimateSettings(randomFarm.climate + 223);
            generatedLocation.Politic = Array.IndexOf(WorldInfo.WorldSetting.RegionNames, randomFarm.region);
            generatedLocation.RegionIndex = generatedLocation.Politic + 128;
            generatedLocation.LocationIndex = 0; // TODO

            return generatedLocation;
        }

        protected DFLocation GenerateHome(int locationType, DFPosition position, string homeName)
        {
            DFLocation generatedLocation = new DFLocation();

            generatedLocation.Loaded = true;
            generatedLocation.RegionName = WorldInfo.WorldSetting.RegionNames[RegionManager.currentRegionIndex];
            generatedLocation.HasDungeon = false;

            generatedLocation.MapTableData.MapId = (ulong)(position.Y * MapsFile.MaxMapPixelX + position.X);
            generatedLocation.MapTableData.Latitude = SetLocationLongitudeLatitude(position.X, position.Y, 1, 1, false);
            generatedLocation.MapTableData.Longitude = SetLocationLongitudeLatitude(position.X, position.Y, 1, 1, true);
            generatedLocation.MapTableData.LocationType = (DFRegion.LocationTypes)locationType;
            generatedLocation.MapTableData.DungeonType = DFRegion.DungeonTypes.NoDungeon;
            generatedLocation.MapTableData.Discovered = false;
            generatedLocation.MapTableData.Key = 0;
            generatedLocation.MapTableData.LocationId = GenerateNewLocationId();

            generatedLocation.Exterior.RecordElement.Header.X = SetPixelXY(position.X, position.Y, 1, 1, true);
            generatedLocation.Exterior.RecordElement.Header.Y = SetPixelXY(position.X, position.Y, 1, 1, false);
            generatedLocation.Exterior.RecordElement.Header.IsExterior = 32768;
            generatedLocation.Exterior.RecordElement.Header.Unknown2 = 0;
            generatedLocation.Exterior.RecordElement.Header.LocationId = generatedLocation.MapTableData.LocationId;
            generatedLocation.Exterior.RecordElement.Header.IsInterior = 0;
            generatedLocation.Exterior.RecordElement.Header.ExteriorLocationId = generatedLocation.Exterior.RecordElement.Header.LocationId;
            generatedLocation.Name = generatedLocation.Exterior.RecordElement.Header.LocationName = generatedLocation.Exterior.ExteriorData.AnotherName = homeName;

            generatedLocation.Exterior.ExteriorData.MapId = generatedLocation.MapTableData.MapId;
            generatedLocation.Exterior.ExteriorData.LocationId = generatedLocation.MapTableData.LocationId;
            generatedLocation.Exterior.ExteriorData.Width = 1;
            generatedLocation.Exterior.ExteriorData.Height = 1;

            generatedLocation.Exterior.ExteriorData.PortTownAndUnknown = 0;

            generatedLocation.Exterior.ExteriorData.BlockNames = new string[1];
            generatedLocation.Exterior.ExteriorData.BlockNames = PickHomeExterior(locationType, position, homeName);

            generatedLocation.Exterior.BuildingCount = (ushort)CountBuildings(generatedLocation.Exterior.ExteriorData.BlockNames);
            generatedLocation.Exterior.Buildings = new DFLocation.BuildingData[generatedLocation.Exterior.BuildingCount];

            string blockReplacementJson = File.ReadAllText(Path.Combine(MapEditor.testPath, "RMB", string.Concat(generatedLocation.Exterior.ExteriorData.BlockNames[0], ".json")));
            DFBlock block = (DFBlock)SaveLoadManager.Deserialize(typeof(DFBlock), blockReplacementJson);
            // DFBlock block = JsonConvert.DeserializeObject<DFBlock>(File.ReadAllText(Path.Combine(MapEditor.testPath, "RMB", generatedLocation.Exterior.ExteriorData.BlockNames[0])));
            int buildingCount = 0;

            do
            {
                generatedLocation.Exterior.Buildings[buildingCount].NameSeed = (ushort)UnityEngine.Random.Range(0, ushort.MaxValue);
                generatedLocation.Exterior.Buildings[buildingCount].FactionId = block.RmbBlock.FldHeader.BuildingDataList[buildingCount].FactionId;
                generatedLocation.Exterior.Buildings[buildingCount].Sector = block.RmbBlock.FldHeader.BuildingDataList[buildingCount].Sector;
                generatedLocation.Exterior.Buildings[buildingCount].BuildingType = block.RmbBlock.FldHeader.BuildingDataList[buildingCount].BuildingType;
                generatedLocation.Exterior.Buildings[buildingCount].Quality = (byte)UnityEngine.Random.Range(1, 21);

                buildingCount++;
            }
            while (buildingCount < block.RmbBlock.FldHeader.BuildingDataList.Length);

            if (buildingCount != generatedLocation.Exterior.BuildingCount)
                Debug.Log("Buildings set and BuildingCount for location " + generatedLocation.Exterior.RecordElement.Header.LocationName + " don't correspond");

            generatedLocation.Dungeon = new DFLocation.LocationDungeon();
            generatedLocation.Climate = MapsFile.GetWorldClimateSettings(ClimateInfo.Climate[position.X, position.Y]);
            generatedLocation.Politic = PoliticInfo.Politic[position.X, position.Y];
            generatedLocation.RegionIndex = RegionManager.currentRegionIndex;
            generatedLocation.LocationIndex = 0; // TODO

            if (locationType == (int)DFRegion.LocationTypes.Tavern)
            {
                bool nameFound = false;
                int buildCount = 0;
                string tavernName;

                do{
                    if (generatedLocation.Exterior.Buildings[buildCount].BuildingType == DFLocation.BuildingTypes.Tavern)
                    {
                        tavernName = TavernNames.GetName(generatedLocation.Exterior.Buildings[buildCount].NameSeed, DFLocation.BuildingTypes.Tavern, generatedLocation.Exterior.Buildings[buildCount].FactionId, homeName, generatedLocation.RegionName);
                        int pickedSuffix = UnityEngine.Random.Range(0, Enum.GetNames(typeof(LocationNamesList.TavernEpithets)).Length);
                        tavernName = string.Concat(tavernName, " ", ((LocationNamesList.TavernEpithets)pickedSuffix).ToString());

                        generatedLocation.Name = generatedLocation.Exterior.RecordElement.Header.LocationName = generatedLocation.Exterior.ExteriorData.AnotherName = tavernName;
                        nameFound = true;
                    }

                    buildCount++;
                }
                while (!nameFound);
            }

            return generatedLocation;
        }

        protected DFLocation GenerateTemple(int locationType, DFPosition position, string templeName)
        {
            DFLocation generatedLocation = new DFLocation();

            generatedLocation.Loaded = true;
            generatedLocation.RegionName = WorldInfo.WorldSetting.RegionNames[RegionManager.currentRegionIndex];
            generatedLocation.HasDungeon = false;
            
            generatedLocation.MapTableData.MapId = (ulong)(position.Y * MapsFile.MaxMapPixelX + position.X);
            generatedLocation.MapTableData.Latitude = SetLocationLongitudeLatitude(position.X, position.Y, 1, 1, false);
            generatedLocation.MapTableData.Longitude = SetLocationLongitudeLatitude(position.X, position.Y, 1, 1, true);
            generatedLocation.MapTableData.LocationType = (DFRegion.LocationTypes)locationType;
            generatedLocation.MapTableData.DungeonType = DFRegion.DungeonTypes.NoDungeon;
            generatedLocation.MapTableData.Discovered = false;
            generatedLocation.MapTableData.Key = 0;
            generatedLocation.MapTableData.LocationId = GenerateNewLocationId();

            generatedLocation.Exterior.RecordElement.Header.X = SetPixelXY(position.X, position.Y, 1, 1, true);
            generatedLocation.Exterior.RecordElement.Header.Y = SetPixelXY(position.X, position.Y, 1, 1, false);
            generatedLocation.Exterior.RecordElement.Header.IsExterior = 32768;
            generatedLocation.Exterior.RecordElement.Header.Unknown2 = 0;
            generatedLocation.Exterior.RecordElement.Header.LocationId = generatedLocation.MapTableData.LocationId;
            generatedLocation.Exterior.RecordElement.Header.IsInterior = 0;
            generatedLocation.Exterior.RecordElement.Header.ExteriorLocationId = generatedLocation.Exterior.RecordElement.Header.LocationId;
            generatedLocation.Name = generatedLocation.Exterior.RecordElement.Header.LocationName = generatedLocation.Exterior.ExteriorData.AnotherName = templeName;

            generatedLocation.Exterior.ExteriorData.MapId = generatedLocation.MapTableData.MapId;
            generatedLocation.Exterior.ExteriorData.LocationId = generatedLocation.MapTableData.LocationId;
            generatedLocation.Exterior.ExteriorData.Width = 1;
            generatedLocation.Exterior.ExteriorData.Height = 1;

            generatedLocation.Exterior.ExteriorData.PortTownAndUnknown = 0;
            generatedLocation.Exterior.ExteriorData.BlockNames = new string[1];

            generatedLocation.Exterior.ExteriorData.BlockNames = PickTempleExterior(locationType, position, templeName);

            generatedLocation.Exterior.BuildingCount = (ushort)CountBuildings(generatedLocation.Exterior.ExteriorData.BlockNames);

            generatedLocation.Exterior.Buildings = new DFLocation.BuildingData[generatedLocation.Exterior.BuildingCount];

            string blockReplacementJson = File.ReadAllText(Path.Combine(MapEditor.testPath, "RMB", string.Concat(generatedLocation.Exterior.ExteriorData.BlockNames[0], ".json")));
            DFBlock block = (DFBlock)SaveLoadManager.Deserialize(typeof(DFBlock), blockReplacementJson);
            // DFBlock block = JsonConvert.DeserializeObject<DFBlock>(File.ReadAllText(Path.Combine(MapEditor.testPath, "RMB", string.Concat(generatedLocation.Exterior.ExteriorData.BlockNames[0], ".json"))));
            int buildingCount = 0;

            do
            {
                generatedLocation.Exterior.Buildings[buildingCount].NameSeed = (ushort)UnityEngine.Random.Range(0, ushort.MaxValue);
                generatedLocation.Exterior.Buildings[buildingCount].FactionId = block.RmbBlock.FldHeader.BuildingDataList[buildingCount].FactionId;
                generatedLocation.Exterior.Buildings[buildingCount].Sector = block.RmbBlock.FldHeader.BuildingDataList[buildingCount].Sector;
                generatedLocation.Exterior.Buildings[buildingCount].BuildingType = block.RmbBlock.FldHeader.BuildingDataList[buildingCount].BuildingType;
                generatedLocation.Exterior.Buildings[buildingCount].Quality = (byte)UnityEngine.Random.Range(1, 21);

                buildingCount++;
            }
            while (buildingCount < block.RmbBlock.FldHeader.BuildingDataList.Length);

            if (buildingCount != generatedLocation.Exterior.BuildingCount)
                Debug.Log("Buildings set and BuildingCount for location " + generatedLocation.Exterior.RecordElement.Header.LocationName + " don't correspond");

            generatedLocation.Dungeon = new DFLocation.LocationDungeon();
            generatedLocation.Climate = MapsFile.GetWorldClimateSettings(ClimateInfo.Climate[position.X, position.Y]);
            generatedLocation.Politic = PoliticInfo.Politic[position.X, position.Y];
            generatedLocation.RegionIndex = RegionManager.currentRegionIndex;
            generatedLocation.LocationIndex = 0; // TODO

            return generatedLocation;
        }

        protected int SetLocationLongitudeLatitude(int x, int y, int width, int height, bool isLongitude)
        {
            int size;
            int pixelSize;

            if (isLongitude)
            {
                size = x * MapsFile.WorldMapTileDim;
                pixelSize = width;
            }
            else{
                size = (MapsFile.MaxMapPixelY - y) * MapsFile.WorldMapTileDim;
                pixelSize = height;
            }

            switch (pixelSize)
            {
                case 1:
                    size += 40;
                    break;

                case 2:
                    size += 32;
                    break;

                case 3:
                case 5:
                    size += 24;
                    break;

                case 4:
                case 6:
                    size += 16;
                    break;

                case 7:
                    size += 8;
                    break;

                default:
                    break;
            }
            return size;
        }

        protected int SetPixelXY(int x, int y, int width, int height, bool isX)
        {
            int size;

            if (isX)
            {
                size = x * MapsFile.WorldMapTerrainDim + (8 - width) * 2048;
            }
            else{
                size = (y * MapsFile.WorldMapTerrainDim + (8 - height) * 2048) + 1;
            }
            return size;
        }

        protected byte DetermineIfPortTown(int x, int y)
        {
            for (int i = -2; i < 3; i++)
            {
                for (int j = -2; j < 3; j++)
                {
                    if (SmallHeightmap.GetHeightMapValue(x + i, y + j) < 3)
                        return 1;
                }
            }

            return 0;
        }

        protected bool DetermineIfWalled(int width, int height)
        {
            int randomChance = UnityEngine.Random.Range(1, 101);

            if (randomChance <= (width * height * 2 + 18))
                return true;
            
            return false;
        }

        protected bool DetermineIfPalaced(int width, int height, bool walled)
        {
            int randomChance = UnityEngine.Random.Range(1, 101);
            int palaced = width * height * 2 + 18;

            if (walled)
                palaced += 10;

            if (randomChance <= palaced)
                return true;

            return false;
        }

        protected string[] PickBlockNames(int locationTypes, (int, int, int) townShape, int locationSize, DFPosition position, bool walled = false, string specialCondition = "")
        {
            List<string> blocks = new List<string>();
            List<(int, int, string)> cityCorners = new List<(int, int, string)>();
            List<(int, int, string)> specialBlocks = new List<(int, int, string)>();
            List<string> blockToNormalize = new List<string>();
            if (locationTypes == (int)DFRegion.LocationTypes.TownCity && walled)
                cityCorners = CalculateWallPosition(townShape.Item1, townShape.Item2);
            specialBlocks = DetermineSpecialBlocksSetting(townShape);

            string climateChar = GetClimateChar(position);
            string sizeChar;
            int blockVariants;
            string partialRMB;
            bool anymoreRMB;
            int multipleCheck;

            bool palacePresent = false;
            (int, int, string) palaceBlock = (0, 0, "");

            if (specialCondition.Equals("capital"))
                palacePresent = true;
            else if (locationTypes == (int)DFRegion.LocationTypes.TownCity)
                palacePresent = DetermineIfPalaced(townShape.Item1, townShape.Item2, walled);

            if (palacePresent)
            {
                int x = UnityEngine.Random.Range(1, (townShape.Item1 - 1));
                int y = UnityEngine.Random.Range(1, (townShape.Item2 - 1));
                int counter = 0;
                blockVariants = 0;
                anymoreRMB = true;

                do
                {
                    string countingRMB = string.Concat("PALA", climateChar, "A", counter.ToString("00"), ".RMB.json");

                    if (File.Exists(Path.Combine(MapEditor.testPath, "RMB", countingRMB)))
                        blockVariants++;
                    else anymoreRMB = false;

                    counter++;
                }
                while (anymoreRMB);

                int pickedVariant = UnityEngine.Random.Range(0, blockVariants);
                palaceBlock = (x, y, (string.Concat("PALA", climateChar, "A", pickedVariant.ToString("00"), ".RMB")));
            }

            for (int i = 0; i < locationSize; i++)
            {
                bool blockPicked = false;
                blockVariants = 0;
                anymoreRMB = true;

                switch (locationTypes)
                {
                    case (int)DFRegion.LocationTypes.TownCity:
                        if (walled)
                        {
                            foreach ((int, int, string) wallPiece in cityCorners)
                            {
                                if ((wallPiece.Item2 * townShape.Item1 + wallPiece.Item1) == i)
                                {
                                    blocks.Add(wallPiece.Item3);
                                    blockPicked = true;
                                    break;
                                }
                            }
                        }

                        if (palacePresent)
                        {
                            if ((palaceBlock.Item2 * townShape.Item1 + palaceBlock.Item1) == i)
                            {
                                blocks.Add(palaceBlock.Item3);
                                blockPicked = true;
                            }
                        }

                        if (!blockPicked)
                        {
                            int cityPrefix;
                            multipleCheck = 0;

                            do{
                                cityPrefix = UnityEngine.Random.Range(0, LocationNamesList.RMBNames[(int)LocationNamesList.RMBTypes.Town].Length);
                                foreach (string block in blocks)
                                {
                                    if (block.StartsWith(LocationNamesList.RMBNames[(int)LocationNamesList.RMBTypes.Town][cityPrefix]))
                                        multipleCheck++;
                                }

                                if (!CheckBlockCompatibility(cityPrefix, multipleCheck, DFRegion.LocationTypes.TownCity))
                                    cityPrefix = -1;
                            }
                            while (cityPrefix < 0);

                            if (walled)
                            {
                                if (UnityEngine.Random.Range(0, 2) == 0)
                                    sizeChar = "L";
                                else sizeChar = "M";
                            }
                            else sizeChar = "M";

                            partialRMB = string.Concat(LocationNamesList.RMBNames[(int)LocationNamesList.RMBTypes.Town][cityPrefix], climateChar, sizeChar);
                            blockToNormalize = new List<string>();
                            blockToNormalize.Add(partialRMB);
                            blockToNormalize = NormalizeTownRMBs(blockToNormalize);
                            partialRMB = blockToNormalize[0];

                            int cityCounter = 0;
                            do{
                                // string tens;
                                // string units;

                                // if (cityCounter < 10)
                                // {
                                //     tens = "0";
                                //     units = cityCounter.ToString();
                                // }
                                // else{
                                //     tens = (cityCounter / 10).ToString();
                                //     units = (cityCounter % 10).ToString();
                                // }

                                string countingRMB = string.Concat(partialRMB, cityCounter.ToString("00"), ".RMB.json");

                                if (File.Exists(Path.Combine(MapEditor.testPath, "RMB", countingRMB)))
                                    blockVariants++;
                                else anymoreRMB = false;

                                cityCounter++;
                            }
                            while (anymoreRMB);

                            int pickedCityVariant = UnityEngine.Random.Range(0, blockVariants);

                            blocks.Add(string.Concat(partialRMB, pickedCityVariant.ToString("00"), ".RMB"));
                        }
                        break;

                    case (int)DFRegion.LocationTypes.TownHamlet:
                    case (int)DFRegion.LocationTypes.TownVillage:
                        if (townShape.Item3 != (int)TownShapes.Regular)
                        {
                            foreach ((int, int, string) fillPiece in specialBlocks)
                            {
                                if ((fillPiece.Item2 * townShape.Item1 + fillPiece.Item1) == i)
                                {
                                    blocks.Add(fillPiece.Item3);
                                    blockPicked = true;
                                    break;
                                }
                            }
                        }

                        if (!blockPicked)
                        {

                            int townPrefix;
                            multipleCheck = 0;

                            do
                            {
                                townPrefix = UnityEngine.Random.Range(0, LocationNamesList.RMBNames[(int)LocationNamesList.RMBTypes.Town].Length);
                                foreach (string block in blocks)
                                {
                                    if (block.StartsWith(LocationNamesList.RMBNames[(int)LocationNamesList.RMBTypes.Town][townPrefix]))
                                        multipleCheck++;
                                }

                                if (!CheckBlockCompatibility(townPrefix, multipleCheck, (DFRegion.LocationTypes)locationTypes))
                                    townPrefix = -1;
                            }
                            while (townPrefix < 0);

                            sizeChar = "M";

                            partialRMB = string.Concat(LocationNamesList.RMBNames[(int)LocationNamesList.RMBTypes.Town][townPrefix], climateChar, sizeChar);
                            blockToNormalize = new List<string>();
                            blockToNormalize.Add(partialRMB);
                            blockToNormalize = NormalizeTownRMBs(blockToNormalize);
                            partialRMB = blockToNormalize[0];

                            int townCounter = 0;
                            do
                            {
                                // string tens;
                                // string units;

                                // if (townCounter < 10)
                                // {
                                //     tens = "0";
                                //     units = townCounter.ToString();
                                // }
                                // else
                                // {
                                //     tens = (townCounter / 10).ToString();
                                //     units = (townCounter % 10).ToString();
                                // }

                                string countingRMB = string.Concat(partialRMB, townCounter.ToString("00"), ".RMB.json");

                                if (File.Exists(Path.Combine(MapEditor.testPath, "RMB", countingRMB)))
                                    blockVariants++;
                                else anymoreRMB = false;

                                townCounter++;
                            }
                            while (anymoreRMB);

                            int pickedTownVariant = UnityEngine.Random.Range(0, blockVariants);
                            if (!partialRMB.StartsWith("TEMP"))
                                blocks.Add(string.Concat(partialRMB, pickedTownVariant.ToString("00"), ".RMB"));
                            else
                            {
                                string templeType;
                                if (UnityEngine.Random.Range(0, 10) + 1 < 6)
                                {
                                    templeType = GetTempleCode(ConvertReligionToDeity(RegionManager.currentRegionData.deity));
                                }
                                else templeType = GetTempleCode(UnityEngine.Random.Range(0, 8));

                                blocks.Add(string.Concat(partialRMB, templeType, ".RMB"));
                            }
                        }
                        break;
                }
            }

            Debug.Log("Normalizing Town RMBs");
            blocks = NormalizeTownRMBs(blocks);

            return blocks.ToArray();
        }

        protected bool CheckBlockCompatibility(int prefix, int multipleCheck, DFRegion.LocationTypes townType)
        {
            string checkedBlock = LocationNamesList.RMBNames[(int)LocationNamesList.RMBTypes.Town][prefix];
            int maxRareTolerated;
            int maxUncommonTolerated;

            if (townType.Equals(DFRegion.LocationTypes.TownCity))
            {
                maxRareTolerated = 1;
                maxUncommonTolerated = 2;
            }
            else if (townType.Equals(DFRegion.LocationTypes.TownHamlet))
            {
                maxRareTolerated = 1;
                maxUncommonTolerated = 1;
            }
            else{
                maxRareTolerated = 0;
                maxUncommonTolerated = 0;
            }

            if ((checkedBlock.Equals("DARK") ||
                 checkedBlock.Equals("FIGH") ||
                 checkedBlock.Equals("MAGE") ||
                 // checkedBlock.Equals("PALA") ||
                 checkedBlock.Equals("THIE")) &&
                 multipleCheck >= maxRareTolerated)
                return false;

            if ((checkedBlock.Equals("BANK") ||
                 // checkedBlock.Equals("FILL") ||
                 checkedBlock.Equals("GRVE") ||
                 checkedBlock.Equals("MARK") ||
                 checkedBlock.Equals("TEMP")) &&
                 multipleCheck >= maxUncommonTolerated)
                return false;

            return true;
        }

        protected string GetTempleCode(int deity)
        {
            switch (deity)
            {
                case 0:
                    return "A0";

                case 1:
                    return "B0";

                case 2:
                    return "C0";

                case 3:
                    return "D0";

                case 4:
                    return "E0";
                
                case 5:
                    return "F0";

                case 6:
                    return "G0";

                case 7:
                    return "H0";

                default:
                    return "Error!!!";
            }
        }

        protected List<string> NormalizeTownRMBs(List<string> RMBList)
        {
            List<string> normalizedRMBList = new List<string>();
            foreach (string block in RMBList)
            {
                string fixedBlock = "notFixed";

                if (!File.Exists(Path.Combine(MapEditor.testPath, "RMB", string.Concat(block, ".json"))))
                {
                    Debug.Log(block + " isn't correct. Going to fix it");

                    fixedBlock = block;

                    if (block.StartsWith("BANK"))       // -g
                        fixedBlock = fixedBlock.Replace('G', 'B');        
                    else if (block.StartsWith("BOOK"))  // -g
                        fixedBlock = fixedBlock.Replace('G', 'B');
                    else if (block.StartsWith("DARK"))  // -g -size
                    {
                        fixedBlock = fixedBlock.Replace("AL", "AA");
                        fixedBlock = fixedBlock.Replace("AM", "AA");
                        fixedBlock = fixedBlock.Replace("AS", "AA");
                        fixedBlock = fixedBlock.Replace("BL", "BA");
                        fixedBlock = fixedBlock.Replace("BM", "BA");
                        fixedBlock = fixedBlock.Replace("BS", "BA");
                        fixedBlock = fixedBlock.Replace("GL", "BA");
                        fixedBlock = fixedBlock.Replace("GM", "BA");
                        fixedBlock = fixedBlock.Replace("GS", "BA");
                    }
                    else if (block.StartsWith("FIGH"))  // a: -size; 
                    {
                        fixedBlock = fixedBlock.Replace("AL", "AA");
                        fixedBlock = fixedBlock.Replace("AM", "AA");
                        fixedBlock = fixedBlock.Replace("AS", "AA");
                    }
                    // else if (block.StartsWith("FILL"))  // -b -g -size
                    // {
                    //     fixedBlock = fixedBlock.Replace("AL", "AA");
                    //     fixedBlock = fixedBlock.Replace("AM", "AA");
                    //     fixedBlock = fixedBlock.Replace("AS", "AA");
                    //     fixedBlock = fixedBlock.Replace("BL", "AA");
                    //     fixedBlock = fixedBlock.Replace("BM", "AA");
                    //     fixedBlock = fixedBlock.Replace("BS", "AA");
                    //     fixedBlock = fixedBlock.Replace("GL", "AA");
                    //     fixedBlock = fixedBlock.Replace("GM", "AA");
                    //     fixedBlock = fixedBlock.Replace("GS", "AA");
                    // }
                    else if (block.StartsWith("GRVE"))  // -b -g
                    {
                        fixedBlock = fixedBlock.Replace('S', 'M');    // no "S" blocks for town graveyards (they have entrances)!
                        if (fixedBlock.Contains("VEBL") && UnityEngine.Random.Range(0, 10) != 0) fixedBlock = fixedBlock.Replace("VEBL", "VEAL");
                        if (!fixedBlock.Contains("VEBL")) fixedBlock = fixedBlock.Replace("VEB", "VEA");
                        fixedBlock = fixedBlock.Replace("VEG", "VEA");
                    }
                    else if (block.StartsWith("LIBR"))  // -g
                        fixedBlock = fixedBlock.Replace('G', 'B');
                    else if (block.StartsWith("MAGE"))  // -size
                    {
                        fixedBlock = fixedBlock.Replace("AL", "AA");
                        fixedBlock = fixedBlock.Replace("AM", "AA");
                        fixedBlock = fixedBlock.Replace("AS", "AA");
                        fixedBlock = fixedBlock.Replace("BL", "BA");
                        fixedBlock = fixedBlock.Replace("BM", "BA");
                        fixedBlock = fixedBlock.Replace("BS", "BA");
                        fixedBlock = fixedBlock.Replace("GL", "GA");
                        fixedBlock = fixedBlock.Replace("GM", "GA");
                        fixedBlock = fixedBlock.Replace("GS", "GA");
                    }
                    else if (block.StartsWith("MARK"))  // a: -size; b: +-size
                    {
                        fixedBlock = fixedBlock.Replace("AL", "AA");
                        fixedBlock = fixedBlock.Replace("AM", "AA");
                        fixedBlock = fixedBlock.Replace("AS", "AA");

                        if (UnityEngine.Random.Range(0, 2) == 0)
                            fixedBlock = fixedBlock.Replace("BM", "BA");
                    }
                    // else if (block.StartsWith("PALA"))  // -size
                    // {
                    //     fixedBlock = fixedBlock.Replace("LAAL", "LAAA");
                    //     fixedBlock = fixedBlock.Replace("LAAM", "LAAA");
                    //     fixedBlock = fixedBlock.Replace("LAAS", "LAAA");
                    //     fixedBlock = fixedBlock.Replace("BL", "BA");
                    //     fixedBlock = fixedBlock.Replace("BM", "BA");
                    //     fixedBlock = fixedBlock.Replace("BS", "BA");
                    //     fixedBlock = fixedBlock.Replace("GL", "GA");
                    //     fixedBlock = fixedBlock.Replace("GM", "GA");
                    //     fixedBlock = fixedBlock.Replace("GS", "GA");
                    // }
                    else if (block.StartsWith("TEMP"))  // -size
                    {
                        fixedBlock = fixedBlock.Replace("AL", "AA");
                        fixedBlock = fixedBlock.Replace("AM", "AA");
                        fixedBlock = fixedBlock.Replace("AS", "AA");
                        fixedBlock = fixedBlock.Replace("BL", "BA");
                        fixedBlock = fixedBlock.Replace("BM", "BA");
                        fixedBlock = fixedBlock.Replace("BS", "BA");
                        fixedBlock = fixedBlock.Replace("GL", "GA");
                        fixedBlock = fixedBlock.Replace("GM", "GA");
                        fixedBlock = fixedBlock.Replace("GS", "GA");
                    }
                    else if (block.StartsWith("THIE"))  // b: -s; -g
                    {
                        fixedBlock = fixedBlock.Replace('G', 'B');
                        fixedBlock = fixedBlock.Replace("BS", "BM");
                    }

                    Debug.Log("RMB name changed to " + fixedBlock);
                }

                if (fixedBlock.Equals("notFixed"))
                    normalizedRMBList.Add(block);
                else normalizedRMBList.Add(fixedBlock);
            }

            return normalizedRMBList;
        }

        protected string[] PickFarmExterior(int farmSize, DFPosition position, int climateNumb)
        {
            string[] resultingName = new string[farmSize];
            int pickedRMB;
            string climateChar;
            string sizeChar;

            for (int i = 0; i < farmSize; i++)
            {
                if (climateNumb + 223 == (int)MapsFile.Climates.Desert ||
                    climateNumb + 223 == (int)MapsFile.Climates.Desert2 ||
                    climateNumb + 223 == (int)MapsFile.Climates.Subtropical)
                    climateChar = "B";
                else climateChar = "A";

                pickedRMB = UnityEngine.Random.Range(0, 10);
                resultingName[i] = string.Concat("FARM", climateChar, "A", pickedRMB.ToString("00"), ".RMB");
            }

            return resultingName;
        }

        protected string[] PickHomeExterior(int locationType, DFPosition position, string homeName)
        {
            string[] resultingName = new string[1];
            int pickedRMB;
            string climateChar;
            string sizeChar;

            switch (locationType)
            {
                case (int)DFRegion.LocationTypes.HomeFarms:
                    climateChar = GetClimateChar(position);
                    if (climateChar.Equals("G"))
                        climateChar = "B";

                    pickedRMB = UnityEngine.Random.Range(0, 10);
                    resultingName[0] = string.Concat("FARMA", climateChar, pickedRMB.ToString("00"), ".RMB");
                    break;

                case (int)DFRegion.LocationTypes.Tavern:
                    climateChar = GetClimateChar(position);

                    if (climateChar.Equals("A"))
                        pickedRMB = UnityEngine.Random.Range(0, 10);
                    else pickedRMB = UnityEngine.Random.Range(0, 5);

                    resultingName[0] = string.Concat("TVRN", climateChar, "S", pickedRMB.ToString("00"), ".RMB");
                    break;

                case (int)DFRegion.LocationTypes.HomeWealthy:
                    climateChar = GetClimateChar(position);

                    if (homeName.Contains("Palace"))
                        sizeChar = "L";
                    else if (homeName.Contains("Manor"))
                        sizeChar = "M";
                    else sizeChar = "S";

                    resultingName[0] = string.Concat("MANR", climateChar, sizeChar);

                    if (resultingName[0].Equals("MANRAL") || resultingName[0].Equals("MANRAM"))
                        pickedRMB = UnityEngine.Random.Range(0, 4);
                    else if (resultingName[0].Equals("MANRAS"))
                        pickedRMB = UnityEngine.Random.Range(0, 3);
                    else pickedRMB = 0;

                    resultingName[0] = string.Concat(resultingName[0], pickedRMB.ToString("00"), ".RMB");
                    break;

                case (int)DFRegion.LocationTypes.HomePoor:
                default:
                    resultingName[0] = "SHCKAA00.RMB";
                    break;                
            }

            return resultingName;
        }

        protected string[] PickTempleExterior(int locationType, DFPosition position, string templeName)
        {
            string[] resultingName = new string[1];
            int pickedRMB;
            string climateChar;
            string deityChar;

            climateChar = GetClimateChar(position);
            if (climateChar.Equals("G"))
                climateChar = "B";

            // A0: Arkay
            // B0: Zenithar
            // C0: Mara
            // D0: Akatosh
            // E0: Dibella
            // F0: Julianos
            // G0: Stendarr
            // H0: Kynareth

            if (locationType == (int)DFRegion.LocationTypes.ReligionCult)
            {
                resultingName[0] = "SHRIAA00.RMB";
            }

            else
            {
                if (templeName.Contains("Arkay"))
                    deityChar = "A0";
                else if (templeName.Contains("Zenithar"))
                    deityChar = "B0";
                else if (templeName.Contains("Mara"))
                    deityChar = "C0";
                else if (templeName.Contains("Akatosh"))
                    deityChar = "D0";
                else if (templeName.Contains("Dibella"))
                    deityChar = "E0";
                else if (templeName.Contains("Julianos"))
                    deityChar = "F0";
                else if (templeName.Contains("Stendarr"))
                    deityChar = "G0";
                else if (templeName.Contains("Kynareth"))
                    deityChar = "H0";
                else deityChar = "AA";

                resultingName[0] = string.Concat("TEMP", climateChar, "A", deityChar, ".RMB");
            }

            return resultingName;
        }

        protected List<(int, int, string)> CalculateWallPosition(int width, int height)
        {
            List<(int, int, string)> wallPosition = new List<(int, int, string)>();

            int westGate = UnityEngine.Random.Range(1, height - 1);
            int eastGate = UnityEngine.Random.Range(1, height - 1);
            int northGate = UnityEngine.Random.Range(1, width - 1);
            int southGate = UnityEngine.Random.Range(1, width - 1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (x == 0 && y == 0)
                        wallPosition.Add((x, y, GetRandom("WALLAA02.RMB", "WALLAA14.RMB", 9)));
                    else if (x == 0 && y == height - 1)
                        wallPosition.Add((x, y, GetRandom("WALLAA00.RMB", "WALLAA12.RMB", 9)));
                    else if (x == width - 1 && y == 0)
                        wallPosition.Add((x, y, GetRandom("WALLAA03.RMB", "WALLAA15.RMB", 9)));
                    else if (x == width - 1 && y == height - 1)
                        wallPosition.Add((x, y, GetRandom("WALLAA01.RMB", "WALLAA13.RMB", 9)));
                    else if (x == 0 && y == westGate)
                        wallPosition.Add((x, y, GetRandom("WALLAA11.RMB", "WALLAA23.RMB", 9)));
                    else if (x == width - 1 && y == eastGate)
                        wallPosition.Add((x, y, GetRandom("WALLAA09.RMB", "WALLAA21.RMB", 9)));
                    else if (x == northGate && y == 0)
                        wallPosition.Add((x, y, GetRandom("WALLAA10.RMB", "WALLAA22.RMB", 9)));      
                    else if (x == southGate && y == height - 1)
                        wallPosition.Add((x, y, GetRandom("WALLAA08.RMB", "WALLAA20.RMB", 9)));
                    else if (x == 0)
                        wallPosition.Add((x, y, GetRandom("WALLAA07.RMB", "WALLAA19.RMB", 9)));
                    else if (x == width - 1)
                        wallPosition.Add((x, y, GetRandom("WALLAA05.RMB", "WALLAA17.RMB", 9)));
                    else if (y == 0)
                        wallPosition.Add((x, y, GetRandom("WALLAA06.RMB", "WALLAA18.RMB", 9)));
                    else if (y == height - 1)
                        wallPosition.Add((x, y, GetRandom("WALLAA04.RMB", "WALLAA16.RMB", 9)));
                }
            }

            return wallPosition;
        }

        protected List<(int, int, string)> DetermineSpecialBlocksSetting((int, int, int) townShape)
        {
            List<(int, int, string)> specialBlocks = new List<(int, int, string)>();
            int x, y;
            int fillIndex;
            string fillBlock;
            int blockVariants = 0;
            bool anymoreRMB = true;

            int fillCounter = 0;
            do
            {
                // string tens;
                // string units;

                // if (cityCounter < 10)
                // {
                //     tens = "0";
                //     units = cityCounter.ToString();
                // }
                // else{
                //     tens = (cityCounter / 10).ToString();
                //     units = (cityCounter % 10).ToString();
                // }

                string countingFILL = string.Concat("FILLAA", fillCounter.ToString("00"), ".RMB.json");

                if (File.Exists(Path.Combine(MapEditor.testPath, "RMB", countingFILL)))
                    blockVariants++;
                else anymoreRMB = false;

                fillCounter++;
            }
            while (anymoreRMB);

            switch (townShape.Item3)
            {
                case (int)TownShapes.LShape:
                    specialBlocks = PutSomeFillerHereAndThere(townShape.Item1, townShape.Item2, blockVariants, 1);
                    break;

                case (int)TownShapes.FourOnSix:
                    specialBlocks = PutSomeFillerHereAndThere(townShape.Item1, townShape.Item2, blockVariants, 2);
                    break;

                case (int)TownShapes.FiveOnSix:
                    specialBlocks = PutSomeFillerHereAndThere(townShape.Item1, townShape.Item2, blockVariants, 1);
                    break;

                case (int)TownShapes.SixOnEight:
                    specialBlocks = PutSomeFillerHereAndThere(townShape.Item1, townShape.Item2, blockVariants, 2);
                    break;

                case (int)TownShapes.MantaShape:    // This is a bit tricky compared to the other shapes
                    x = UnityEngine.Random.Range(0, 2) * 2;
                    y = UnityEngine.Random.Range(0, 2) * 2;
                    fillIndex = UnityEngine.Random.Range(0, blockVariants);
                    fillBlock = string.Concat("FILLAA", fillIndex.ToString("00"), ".RMB");
                    specialBlocks.Add((x, y, fillBlock));
                    Debug.Log("Block " + fillBlock + " added at position " + x + ", " + y);

                    int x1;
                    int y1;

                    x1 = x;
                    do{
                        y1 = UnityEngine.Random.Range(0, 3);
                    }
                    while (y1 == y);

                    fillIndex = UnityEngine.Random.Range(0, blockVariants);
                    fillBlock = string.Concat("FILLAA", fillIndex.ToString("00"), ".RMB");
                    specialBlocks.Add((x1, y1, fillBlock));
                    Debug.Log("Block " + fillBlock + " added at position " + x1 + ", " + y1);

                    int x2;
                    int y2 = y;

                    do{
                        x2 = UnityEngine.Random.Range(0, 3);
                    }
                    while (x2 == x);

                    fillIndex = UnityEngine.Random.Range(0, blockVariants);
                    fillBlock = string.Concat("FILLAA", fillIndex.ToString("00"), ".RMB");
                    specialBlocks.Add((x2, y2, fillBlock));
                    Debug.Log("Block " + fillBlock + " added at position " + x2 + ", " + y2);
                    break;

                case (int)TownShapes.EightOnNine:
                    specialBlocks = PutSomeFillerHereAndThere(townShape.Item1, townShape.Item2, blockVariants, 1);
                    break;

                case (int)TownShapes.NineOnTwelve:
                    specialBlocks = PutSomeFillerHereAndThere(townShape.Item1, townShape.Item2, blockVariants, 3);
                    break;

                case (int)TownShapes.TwelveOnSixteen:
                    specialBlocks = PutSomeFillerHereAndThere(townShape.Item1, townShape.Item2, blockVariants, 4);
                    break;

                default:
                    break;
            }

            return specialBlocks;
        }

        protected List<(int, int, string)> PutSomeFillerHereAndThere(int width, int height, int blockVariants, int fillToPutIn)
        {
            List<(int, int, string)> specialBlocks = new List<(int, int, string)>();
            int fillIndex;
            string fillBlock;

            do{
                int x = UnityEngine.Random.Range(0, width);
                int y = UnityEngine.Random.Range(0, height);
                fillIndex = UnityEngine.Random.Range(0, blockVariants);
                fillBlock = string.Concat("FILLAA", fillIndex.ToString("00"), ".RMB");

                if (!specialBlocks.Contains((x, y, "FILL*")))
                {
                    specialBlocks.Add((x, y, fillBlock));
                    Debug.Log("Block " + fillBlock + " added at position " + x + ", " + y);
                }
            }
            while (specialBlocks.Count < fillToPutIn);

            return specialBlocks;
        }

        protected string GetRandom(string element1, string element2, int firstElementChance = 5)
        {
            if (UnityEngine.Random.Range(1, 11) <= firstElementChance)
                return element1;

            else return element2;
        }

        protected string GetClimateChar(DFPosition position)
        {
            if (ClimateInfo.Climate[position.X, position.Y] == (int)MapsFile.Climates.Desert ||
                ClimateInfo.Climate[position.X, position.Y] == (int)MapsFile.Climates.Desert2 ||
                ClimateInfo.Climate[position.X, position.Y] == (int)MapsFile.Climates.Subtropical)
            {
                if (UnityEngine.Random.Range(0, 2) == 0)
                    return "B";
                else return "G";
            }
            else return "A";
        }

        protected int CountBuildings(string[] blockList)
        {
            int buildingCount = 0;
            for (int i = 0; i < blockList.Length; i++)
            {
                string blockReplacementJson = File.ReadAllText(Path.Combine(MapEditor.testPath, "RMB", string.Concat(blockList[i], ".json")));
                DFBlock block = (DFBlock)SaveLoadManager.Deserialize(typeof(DFBlock), blockReplacementJson);
                // DFBlock block = JsonConvert.DeserializeObject<DFBlock>(File.ReadAllText(Path.Combine(MapEditor.testPath, "RMB", string.Concat(blockList[i], ".json"))));
                buildingCount += block.RmbBlock.FldHeader.BuildingDataList.Length;
            }

            return buildingCount;
        }

        protected string[] PickDungeonExterior(int locationType, DFPosition position, int dungeonType, string dungeonName)
        {
            string[] resultingName = new string[1];
            int progressive = UnityEngine.Random.Range(1, 101);
            int div = 0;
            int size = 0;
            int selectedExterior = 0;

            if (locationType == ((int)DFRegion.LocationTypes.DungeonRuin))
                size = 0;
            else if (locationType == ((int)DFRegion.LocationTypes.DungeonKeep))
                size = 1;
            else if (locationType == ((int)DFRegion.LocationTypes.DungeonLabyrinth))
                size = 2;

            if ((dungeonName.Contains("Castle") || 
                 dungeonName.Contains("Citadel") || 
                 dungeonName.Contains("Hold") || 
                 dungeonName.Contains("Guard") ||
                 dungeonName.Contains("Fortress") ||
                 dungeonName.Contains("Stronghold")) &&
                !dungeonName.Contains("Ruins"))
            {
                if (size == 2 && progressive == 100 && (dungeonType == (int)DFRegion.DungeonTypes.HumanStronghold || dungeonType == (int)DFRegion.DungeonTypes.OrcStronghold))    // special cases
                {
                    if (dungeonType == (int)DFRegion.DungeonTypes.HumanStronghold)
                        resultingName[0] = string.Concat(castleRMB[3][1], ".RMB");
                    if (dungeonType == (int)DFRegion.DungeonTypes.OrcStronghold)
                        resultingName[0] = string.Concat(castleRMB[3][0], ".RMB");
                }
                else
                {
                    div = 100 / castleRMB[size].Length;
                    progressive /= div;
                    if (progressive >= castleRMB[size].Length)
                        progressive = castleRMB[size].Length - 1;
                    resultingName[0] = string.Concat(castleRMB[size][progressive], ".RMB");
                }
                
                return resultingName;
            }

            else if (dungeonName.Contains("Tower") &&
                     !dungeonName.Contains("Ruins"))
            {
                div = 100 / towerRMB[size].Length;
                progressive /= div;
                if (progressive >= towerRMB[size].Length)
                    progressive = towerRMB[size].Length - 1;
                resultingName[0] = string.Concat(towerRMB[size][progressive], ".RMB");
                return resultingName;
            }

            else if ((dungeonName.Contains("Aerie") ||
                      dungeonName.Contains("Aviary") ||
                      dungeonName.Contains("Coop") ||
                      dungeonName.Contains("Mews") ||
                      dungeonName.Contains("Nest") ||
                      dungeonName.Contains("Roost")) &&
                      dungeonType == (int)DFRegion.DungeonTypes.HarpyNest)
            {
                if (size >= harpyRMB.Length) size = harpyRMB.Length - 1; // while we don't have a full set of RMBs for harpy nests, this should stay here
                div = 100 / harpyRMB[size].Length;
                progressive /= div;
                if (progressive >= harpyRMB[size].Length)
                    progressive = harpyRMB[size].Length - 1;
                resultingName[0] = string.Concat(harpyRMB[size][progressive], ".RMB");
                return resultingName;
            }

            else if ((dungeonName.Contains("Nest") ||
                      dungeonName.Contains("Web")) && 
                      dungeonType != (int)DFRegion.DungeonTypes.HarpyNest)
            {
                div = 100 / caveRMB[size].Length;
                progressive /= div;
                if (progressive >= caveRMB[size].Length)
                    progressive = caveRMB[size].Length - 1;
                resultingName[0] = string.Concat(caveRMB[size][progressive], ".RMB");
                return resultingName;
            }

            else if (dungeonName.Contains("Den") ||
                     dungeonName.Contains("Excavation") ||
                     dungeonName.Contains("Mine") ||
                     dungeonName.Contains("Lode") ||
                     dungeonName.Contains("Barrow") ||
                     dungeonName.Contains("Cavern") ||
                     dungeonName.Contains("Cave") ||
                     dungeonName.Contains("Tunnel") ||
                     dungeonName.Contains("Hole") ||
                     dungeonName.Contains("Pit") ||
                     dungeonName.Contains("Quarry") ||
                     dungeonName.Contains("Hollow") ||
                     dungeonName.Contains("Grotto") ||
                     dungeonName.Contains("Lair"))
            {
                div = 100 / caveRMB[size].Length;
                progressive /= div;
                if (progressive >= caveRMB[size].Length)
                    progressive = caveRMB[size].Length - 1;
                if (size == 2 && progressive == 0) progressive = 1; // the tree isn't adequate for this dungeon type
                resultingName[0] = string.Concat(caveRMB[size][progressive], ".RMB");
                return resultingName;
            }

            else if ((dungeonName.Contains("Haunt") ||
                      dungeonName.Contains("House")) &&
                      dungeonType != (int)DFRegion.DungeonTypes.Prison) // So this isn't picked when there's a House of Correction
            {
                if (size == 2 && progressive == 100)    // special cases
                {
                    size = UnityEngine.Random.Range(0, 2);
                    div = 100 / castleRMB[size].Length;
                    progressive /= div;
                    if (progressive >= castleRMB[size].Length)
                        progressive = castleRMB[size].Length - 1;
                    resultingName[0] = string.Concat(castleRMB[size][progressive], ".RMB");
                }
                else
                {
                    div = 100 / hauntRMB[size].Length;
                    progressive /= div;
                    if (progressive >= hauntRMB[size].Length)
                        progressive = hauntRMB[size].Length - 1;
                    resultingName[0] = string.Concat(hauntRMB[size][progressive], ".RMB");
                }
                return resultingName;
            }

            else if (dungeonName.Contains("Assembly") ||
                     dungeonName.Contains("Circle") ||
                     dungeonName.Contains("Community") ||
                     dungeonName.Contains("Conclave") ||
                     dungeonName.Contains("Convergence") ||
                     dungeonName.Contains("Convocation") ||
                     dungeonName.Contains("Council") ||
                     dungeonName.Contains("Gathering"))
            {
                // selectedExterior = UnityEngine.Random.Range(0, 4);

                div = 100 / assemblyRMB[size].Length;
                progressive /= div;
                if (progressive >= assemblyRMB[size].Length)
                    progressive = assemblyRMB[size].Length - 1;
                resultingName[0] = string.Concat(assemblyRMB[size][progressive], ".RMB");
                return resultingName;
            }

            else if (dungeonName.Contains("Cabal") ||
                     dungeonName.Contains("Coven") ||
                     dungeonName.Contains("Cult") ||
                     dungeonName.Contains("Cultus") ||
                     dungeonName.Contains("Tradition"))
            {
                if (size >= 1) size -= 1; // while we don't have a full set of RMBs for covens, this should stay here
                div = 100 / covenRMB[size].Length;
                progressive /= div;
                if (progressive >= covenRMB[size].Length)
                    progressive = covenRMB[size].Length - 1;
                resultingName[0] = string.Concat(covenRMB[size][progressive], ".RMB");
                return resultingName;
            }

            else if (dungeonName.Contains("Laboratory") ||
                     dungeonName.Contains("Academy") ||
                     dungeonName.Contains("Abbey") ||
                     dungeonName.Contains("Cathedral") ||
                     dungeonName.Contains("Cloister") ||
                     dungeonName.Contains("Convent") ||
                     dungeonName.Contains("Friary") ||
                     dungeonName.Contains("Hermitage") ||
                     dungeonName.Contains("Manse") ||
                     dungeonName.Contains("Minster") ||
                     dungeonName.Contains("Monastery") ||
                     dungeonName.Contains("Rectory") ||
                     dungeonName.Contains("Shrine") ||
                     dungeonName.Contains("Temple"))
            {
                div = 100 / laboratoryRMB[size].Length;
                progressive /= div;
                if (progressive >= laboratoryRMB[size].Length)
                    progressive = laboratoryRMB[size].Length - 1;
                resultingName[0] = string.Concat(laboratoryRMB[size][progressive], ".RMB");
                return resultingName;
            }

            else if ((dungeonName.Contains("Crypt") ||
                     dungeonName.Contains("Grave") ||
                     dungeonName.Contains("Mausoleum") ||
                     dungeonName.Contains("Sepulcher") ||
                     dungeonName.Contains("Tomb") ||
                     dungeonName.Contains("Vault")) &&
                     dungeonType != (int)DFRegion.DungeonTypes.Cemetery)
            {
                div = 100 / cryptRMB[size].Length;
                progressive /= div;
                if (progressive >= cryptRMB[size].Length)
                    progressive = cryptRMB[size].Length - 1;
                resultingName[0] = string.Concat(cryptRMB[size][progressive], ".RMB");
                return resultingName;
            }

            else if (dungeonName.Contains("Cairn"))
            {
                resultingName[0] = string.Concat("DUNGAA00", ".RMB");
                return resultingName;
            }

            else if (dungeonName.Contains("Asylum") ||
                     dungeonName.Contains("Dungeon") ||
                     dungeonName.Contains("Gaol") ||
                     dungeonName.Contains("House of Correction") ||
                     dungeonName.Contains("Penitentiary") ||
                     dungeonName.Contains("Prison") ||
                     dungeonName.Contains("Reformatory"))
            {
                // Potential RMBs are selected based on the dungeon size
                div = 100 / prisonRMB[size].Length;
                progressive /= div;
                if (progressive >= prisonRMB[size].Length)
                    progressive = prisonRMB[size].Length - 1;
                resultingName[0] = string.Concat(prisonRMB[size][progressive], ".RMB");
                return resultingName;
            }

            else if (dungeonName.Contains("Ruins") &&
                    (dungeonName.Contains("Castle") ||
                     dungeonName.Contains("Citadel") ||
                     dungeonName.Contains("Fortress") ||
                     dungeonName.Contains("Guard") ||
                     dungeonName.Contains("Hold")))
            {
                // Potential RMBs are selected based on the dungeon size
                div = 100 / ruinsRMB[size].Length;
                progressive /= div;
                if (progressive >= ruinsRMB[size].Length)
                    progressive = ruinsRMB[size].Length - 1;
                resultingName[0] = string.Concat(ruinsRMB[size][progressive], ".RMB");
                return resultingName;
            }

            else if (dungeonName.Contains("Ruins") &&
                    (dungeonName.Contains("Tower")))
            {
                // Potential RMBs are selected based on the dungeon size
                div = 100 / towerRuinRMB[size].Length;
                progressive /= div;
                if (progressive >= towerRuinRMB[size].Length)
                    progressive = towerRuinRMB[size].Length - 1;
                resultingName[0] = string.Concat(towerRuinRMB[size][progressive], ".RMB");
                return resultingName;
            }

            else if (dungeonName.Contains("Ruins") &&
                    (dungeonName.Contains("Cabin") ||
                     dungeonName.Contains("Court") ||
                     dungeonName.Contains("Farm") ||
                     dungeonName.Contains("Farmstead") ||
                     dungeonName.Contains("Grange") ||
                     dungeonName.Contains("Hall") ||
                     dungeonName.Contains("Hovel") ||
                     dungeonName.Contains("Manor") ||
                     dungeonName.Contains("Orchard") ||
                     dungeonName.Contains("Palace") ||
                     dungeonName.Contains("Place") ||
                     dungeonName.Contains("Plantation") ||
                     dungeonName.Contains("Shack")))
            {
                div = 100 / villageRuinRMB[size].Length;
                progressive /= div;
                if (progressive >= villageRuinRMB[size].Length)
                    progressive = villageRuinRMB[size].Length - 1;
                resultingName[0] = string.Concat(villageRuinRMB[size][progressive], ".RMB");
                return resultingName;
            }

            else if (dungeonName.Contains("Burial Ground") ||
                     dungeonName.Contains("Cemetery") ||
                     dungeonName.Contains("Crypts") ||
                     dungeonName.Contains("Graveyard") ||
                     dungeonName.Contains("Graves") ||
                     dungeonName.Contains("Tombs") ||
                     dungeonName.Contains("Vaults"))
            {
                progressive = UnityEngine.Random.Range(0, 43);
                resultingName[0] = string.Concat("GRVEAS", progressive.ToString("00"), ".RMB");
                return resultingName;
            }

            else    // Only Orc-Giant Strongholds that have monster names should be left out
            {
                if (size == 2 && progressive == 100 && dungeonType == (int)DFRegion.DungeonTypes.OrcStronghold)    // special case
                    resultingName[0] = string.Concat(castleRMB[3][0], ".RMB");
                else{
                    div = 100 / monsterRMB[size].Length;
                    progressive /= div;
                    if (progressive >= monsterRMB[size].Length)
                        progressive = monsterRMB[size].Length - 1;
                    resultingName[0] = string.Concat(monsterRMB[size][progressive], ".RMB");
                }                
                return resultingName;
            }
        }

        protected DFLocation.DungeonBlock[] GenerateDungeonInterior(int locationType, int dungeonType, string dungeonName)
        {
            bool[,] diagramProject;
            (int, int)[] dungeonDiagram = GenerateDungeonDiagram(locationType, out diagramProject);
            int startingBlock = UnityEngine.Random.Range(0, dungeonDiagram.Length);
            DFLocation.DungeonBlock[] dungeon = new DFLocation.DungeonBlock[dungeonDiagram.Length];
            bool dungeonTypeBlockPicked = false;
            bool startingBlockPicked = false;
            (int, int) startingPosition;
            int farthestBlock = 0;
            float distanceFromEntrance = 0.0f;

            int nameRDBType = GetRDBType(dungeonType, dungeonName);
            int dungeonRDBType = GetRDBType(dungeonType);

            // Now we pick appropriate Dungeon Blocks based on name (4/5) and type (1/5); entrance is always 
            // of the name-type; no matter the chances, there's always at least one block for name and one for
            // type (exception: when the dungeon has only one true block, in which case the block is selected 
            // based on dungeon name).
            // As a sort of Easter Egg, or just to make things more varied, I'll put one chance out of 5 
            // that big dungeons (10 or more true blocks) have the farthest block(s) from the entrance as
            // a totally randomly-themed block.

            for (int i = 0; i < dungeonDiagram.Length; i++)
            {
                int randomChance = UnityEngine.Random.Range(0, 10) + 1;
                string pickedBlock;

                if (startingBlock == i)
                {
                    pickedBlock = LocationNamesList.RDBNames[nameRDBType][UnityEngine.Random.Range(0, LocationNamesList.RDBNames[nameRDBType].Length)];
                    dungeon[i] = GetBlockData(pickedBlock, dungeonDiagram[i]);
                    dungeon[i].IsStartingBlock = true;
                    startingBlockPicked = true;
                    startingPosition = dungeonDiagram[i];
                }

                else if (randomChance > 8 || 
                        (dungeonDiagram.Length - 2 == i && !dungeonTypeBlockPicked && !startingBlockPicked) ||
                        (dungeonDiagram.Length - 1 == i && !dungeonTypeBlockPicked))
                {
                    pickedBlock = LocationNamesList.RDBNames[dungeonRDBType][UnityEngine.Random.Range(0, LocationNamesList.RDBNames[dungeonRDBType].Length)];
                    dungeon[i] = GetBlockData(pickedBlock, dungeonDiagram[i]);
                    dungeonTypeBlockPicked = true;

                    float distance = CalculateDistance(dungeonDiagram[i], dungeonDiagram[startingBlock]);
                    if (distanceFromEntrance < (distance))
                    {
                        distanceFromEntrance = distance;
                        farthestBlock = i;
                    }
                }

                else if (randomChance < 9)
                {
                    pickedBlock = LocationNamesList.RDBNames[nameRDBType][UnityEngine.Random.Range(0, LocationNamesList.RDBNames[nameRDBType].Length)];
                    dungeon[i] = GetBlockData(pickedBlock, dungeonDiagram[i]);

                    float distance = CalculateDistance(dungeonDiagram[i], dungeonDiagram[startingBlock]);
                    if (distanceFromEntrance < (distance))
                    {
                        distanceFromEntrance = distance;
                        farthestBlock = i;
                    }
                }
            }

            if (dungeonDiagram.Length >= 10)
            {
                int randomType = 0;
                string randomBlock;
                do{
                    randomType = UnityEngine.Random.Range(0, Enum.GetNames(typeof(LocationNamesList.RDBTypes)).Length);
                }
                while (randomType == dungeonRDBType || randomType == nameRDBType || randomType == (int)LocationNamesList.RDBTypes.Cemetery);

                randomBlock = LocationNamesList.RDBNames[randomType][UnityEngine.Random.Range(0, LocationNamesList.RDBNames[randomType].Length)];
                dungeon[farthestBlock] = GetBlockData(randomBlock, dungeonDiagram[farthestBlock]);
            }

            List<DFLocation.DungeonBlock> dungeonList = new List<DFLocation.DungeonBlock>();
            dungeonList = dungeon.ToList();
            for (int x = 0; x < 6; x++)
            {
                for (int y = 0; y < 6; y++)
                {
                    string borderBlock;
                    DFLocation.DungeonBlock block;
                    // Corners are excluded
                    if ((x == 0 || x == 5) && (y == 0 || y == 5))
                        continue;

                    if (!diagramProject[x, y] && CheckAdjacentBlocks(diagramProject, (x, y)))
                    {
                        borderBlock = LocationNamesList.RDBNames[(int)LocationNamesList.RDBTypes.Border][UnityEngine.Random.Range(0, LocationNamesList.RDBNames[(int)LocationNamesList.RDBTypes.Border].Length)];
                        block = GetBlockData(borderBlock, (x - 2, y - 2));
                        dungeonList.Add(block);
                    }
                }
            }

            dungeon = dungeonList.ToArray();

            return dungeon;
        }

        protected (int, int)[] GenerateDungeonDiagram(int locationType, out bool[,] diagramProject)
        {
            int blocksNumber = 0;
            int scalableSize = UnityEngine.Random.Range(0, 100) + 1;
            int[,] dungeonDiagram = new int[6, 6];
            diagramProject = new bool[6, 6];
            (int, int) coordinates;

            // Setting a new diagramProject
            for (int x = 0; x < 6; x++)
                for (int y = 0; y < 6; y++)
                    diagramProject[x, y] = false;

            switch (locationType)
            {
                case (int)DFRegion.LocationTypes.DungeonRuin:
                    if (scalableSize <= 10 )
                        blocksNumber = 1;
                    else if (scalableSize <= 40)
                        blocksNumber = 2;
                    else if (scalableSize <= 70)
                        blocksNumber = 3;
                    else blocksNumber = 4;
                    break;

                case (int)DFRegion.LocationTypes.DungeonKeep:
                    blocksNumber = ((scalableSize - 1) / 25) + 5;
                    break;

                case (int)DFRegion.LocationTypes.DungeonLabyrinth:
                    if (scalableSize < 58)          // 1-57
                        blocksNumber = 9;
                    else if (scalableSize < 81)     // 58-69
                        blocksNumber = 10;
                    else if (scalableSize < 86)     // 70-79
                        blocksNumber = 11;
                    else if (scalableSize < 91)     // 80-87
                        blocksNumber = 12;
                    else if (scalableSize < 95)     // 88-93
                        blocksNumber = 13;
                    else if (scalableSize < 98)     // 94-97
                        blocksNumber = 14;
                    else if (scalableSize < 100)    // 98-99
                        blocksNumber = 15;
                    else if (scalableSize == 100)   // 100
                        blocksNumber = 16;

                    break;

                case (int)DFRegion.LocationTypes.Graveyard:
                    blocksNumber = 1;
                    break;
                    // (int, int)[] cemetery = new (int, int)[1];
                    // diagramProject[0, 0] = true;
                    // cemetery[0] = (0, 0);
                    // return cemetery;
            }

            (int, int)[] diagram = new (int, int)[blocksNumber];

            for (int i = 0; i < blocksNumber; i++)
            {
                if (i == 0)
                {
                    coordinates = (UnityEngine.Random.Range( -1, 2), UnityEngine.Random.Range( -1, 2));
                    diagram[i] = coordinates;
                    diagramProject[(coordinates.Item1 + 2), (coordinates.Item2 + 2)] = true;
                }

                else{
                    bool positionFound = false;

                    do{
                        coordinates = diagram[UnityEngine.Random.Range(0, i)];
                        int[] freeSectors = GetFreeSectors(coordinates, diagramProject);

                        if (freeSectors.Length > 0)
                        {
                            int direction = freeSectors[UnityEngine.Random.Range(0, freeSectors.Length)];
                            (int, int) newSector;

                            switch (direction)
                            {
                                default:
                                case 0:     // North
                                    newSector.Item1 = coordinates.Item1;
                                    newSector.Item2 = coordinates.Item2 - 1;
                                    break;

                                case 1:     // East
                                    newSector.Item1 = coordinates.Item1 + 1;
                                    newSector.Item2 = coordinates.Item2;
                                    break;

                                case 2:     // South
                                    newSector.Item1 = coordinates.Item1;
                                    newSector.Item2 = coordinates.Item2 + 1;
                                    break;

                                case 3:     // West
                                    newSector.Item1 = coordinates.Item1 - 1;
                                    newSector.Item2 = coordinates.Item2;
                                    break;
                            }

                            diagram[i] = (newSector);
                            diagramProject[newSector.Item1 + 2, newSector.Item2 + 2] = true;
                            positionFound = true;
                        }
                    }
                    while(!positionFound);
                } 
            }

            return diagram;
        }

        protected int[] GetFreeSectors((int, int) coordinates, bool[,] diagramProject)
        {
            List<int> availableSectors = new List<int>();
            (int, int) modifiedCoordinates;

            for (int i = 0; i < 4; i++)
            {
                switch (i)
                {
                    default:
                    case 0:     // North
                        modifiedCoordinates.Item1 = coordinates.Item1;
                        modifiedCoordinates.Item2 = coordinates.Item2 - 1;
                        break;

                    case 1:     // East
                        modifiedCoordinates.Item1 = coordinates.Item1 + 1;
                        modifiedCoordinates.Item2 = coordinates.Item2;
                        break;

                    case 2:     // South
                        modifiedCoordinates.Item1 = coordinates.Item1;
                        modifiedCoordinates.Item2 = coordinates.Item2 + 1;
                        break;

                    case 3:     // West
                        modifiedCoordinates.Item1 = coordinates.Item1 - 1;
                        modifiedCoordinates.Item2 = coordinates.Item2;
                        break;
                }

                if (modifiedCoordinates.Item1 > -2 && modifiedCoordinates.Item1 < 3 &&
                    modifiedCoordinates.Item2 > -2 && modifiedCoordinates.Item2 < 3 &&
                    !diagramProject[modifiedCoordinates.Item1 + 2, modifiedCoordinates.Item2 + 2])
                    availableSectors.Add(i);
            }

            return availableSectors.ToArray();
        }

        protected int GetRDBType(int dungeonType, string dungeonName)
        {
            if ((dungeonName.Contains("Castle") || 
                 dungeonName.Contains("Citadel") || 
                 dungeonName.Contains("Hold") || 
                 dungeonName.Contains("Guard") ||
                 dungeonName.Contains("Fortress") ||
                 dungeonName.Contains("Stronghold")) &&
                !dungeonName.Contains("Ruins"))
                return (int)LocationNamesList.RDBTypes.Castle;

            else if (dungeonName.Contains("Tower") &&
                    !dungeonName.Contains("Ruins"))
                return (int)LocationNamesList.RDBTypes.Tower;

            else if ((dungeonName.Contains("Aerie") ||
                      dungeonName.Contains("Aviary") ||
                      dungeonName.Contains("Coop") ||
                      dungeonName.Contains("Mews") ||
                      dungeonName.Contains("Nest") ||
                      dungeonName.Contains("Roost")) &&
                      dungeonType == (int)DFRegion.DungeonTypes.HarpyNest)
                return (int)LocationNamesList.RDBTypes.HarpyNest;

            else if ((dungeonName.Contains("Nest") ||
                      dungeonName.Contains("Web")) && 
                      dungeonType != (int)DFRegion.DungeonTypes.HarpyNest)
                return (int)LocationNamesList.RDBTypes.BugsNest;

            else if (dungeonName.Contains("Den") ||
                     dungeonName.Contains("Barrow") ||
                     dungeonName.Contains("Cavern") ||
                     dungeonName.Contains("Cave") ||
                     dungeonName.Contains("Tunnel") ||
                     dungeonName.Contains("Hole") ||
                     dungeonName.Contains("Pit") ||
                     dungeonName.Contains("Hollow") ||
                     dungeonName.Contains("Grotto") ||
                     dungeonName.Contains("Lair"))
                return (int)LocationNamesList.RDBTypes.Cave;

            else if (dungeonName.Contains("Excavation") ||
                     dungeonName.Contains("Mine") ||
                     dungeonName.Contains("Lode") ||
                     dungeonName.Contains("Quarry"))
                return (int)LocationNamesList.RDBTypes.Mine;

            else if ((dungeonName.Contains("Haunt") ||
                      dungeonName.Contains("House")) &&
                      dungeonType != (int)DFRegion.DungeonTypes.Prison)
                return (int)LocationNamesList.RDBTypes.Haunt;

            else if (dungeonName.Contains("Cabal") ||
                     dungeonName.Contains("Coven") ||
                     dungeonName.Contains("Cult") ||
                     dungeonName.Contains("Cultus") ||
                     dungeonName.Contains("Tradition"))
                return (int)LocationNamesList.RDBTypes.Coven;

            else if (dungeonName.Contains("Laboratory") ||
                     dungeonName.Contains("Academy"))
                return (int)LocationNamesList.RDBTypes.Laboratory;

            else if (dungeonName.Contains("Abbey") ||
                     dungeonName.Contains("Cathedral") ||
                     dungeonName.Contains("Cloister") ||
                     dungeonName.Contains("Convent") ||
                     dungeonName.Contains("Friary") ||
                     dungeonName.Contains("Hermitage") ||
                     dungeonName.Contains("Manse") ||
                     dungeonName.Contains("Minster") ||
                     dungeonName.Contains("Monastery") ||
                     dungeonName.Contains("Rectory") ||
                     dungeonName.Contains("Shrine") ||
                     dungeonName.Contains("Temple"))
                return (int)LocationNamesList.RDBTypes.Temple;

            else if ((dungeonName.Contains("Crypt") ||
                     dungeonName.Contains("Grave") ||
                     dungeonName.Contains("Mausoleum") ||
                     dungeonName.Contains("Sepulcher") ||
                     dungeonName.Contains("Tomb") ||
                     dungeonName.Contains("Vault")) &&
                     dungeonType != (int)DFRegion.DungeonTypes.Cemetery)
                return (int)LocationNamesList.RDBTypes.Crypt;

            else if (dungeonName.Contains("Asylum") ||
                     dungeonName.Contains("Dungeon") ||
                     dungeonName.Contains("Gaol") ||
                     dungeonName.Contains("House of Correction") ||
                     dungeonName.Contains("Penitentiary") ||
                     dungeonName.Contains("Prison") ||
                     dungeonName.Contains("Reformatory"))
                return (int)LocationNamesList.RDBTypes.Prison;

            else if (dungeonName.Contains("Ruins"))
                return (int)LocationNamesList.RDBTypes.Ruins;

            else if (dungeonName.Contains("Burial Ground") ||
                     dungeonName.Contains("Cemetery") ||
                     dungeonName.Contains("Crypts") ||
                     dungeonName.Contains("Graveyard") ||
                     dungeonName.Contains("Graves") ||
                     dungeonName.Contains("Tombs") ||
                     dungeonName.Contains("Vaults"))
                return (int)LocationNamesList.RDBTypes.Cemetery;

            else return (int)LocationNamesList.RDBTypes.Castle;
        }

        protected int GetRDBType(int dungeonType)
        {
            switch (dungeonType)
            {
                case (int)DFRegion.DungeonTypes.Crypt:
                    return (int)LocationNamesList.RDBTypes.Crypt;

                default:
                case (int)DFRegion.DungeonTypes.OrcStronghold:
                case (int)DFRegion.DungeonTypes.HumanStronghold:
                case (int)DFRegion.DungeonTypes.GiantStronghold:
                case (int)DFRegion.DungeonTypes.BarbarianStronghold:
                    return (int)LocationNamesList.RDBTypes.Castle;

                case (int)DFRegion.DungeonTypes.Prison:
                    return (int)LocationNamesList.RDBTypes.Prison;

                case (int)DFRegion.DungeonTypes.DesecratedTemple:
                    return (int)LocationNamesList.RDBTypes.Temple;

                case (int)DFRegion.DungeonTypes.Mine:
                    return (int)LocationNamesList.RDBTypes.Mine;

                case (int)DFRegion.DungeonTypes.NaturalCave:
                    return (int)LocationNamesList.RDBTypes.Cave;

                case (int)DFRegion.DungeonTypes.Coven:
                    return (int)LocationNamesList.RDBTypes.Coven;

                case (int)DFRegion.DungeonTypes.VampireHaunt:
                    return (int)LocationNamesList.RDBTypes.Haunt;

                case (int)DFRegion.DungeonTypes.Laboratory:
                    return (int)LocationNamesList.RDBTypes.Laboratory;

                case (int)DFRegion.DungeonTypes.HarpyNest:
                    return (int)LocationNamesList.RDBTypes.HarpyNest;

                case (int)DFRegion.DungeonTypes.RuinedCastle:
                    return (int)LocationNamesList.RDBTypes.Ruins;

                case (int)DFRegion.DungeonTypes.SpiderNest:
                case (int)DFRegion.DungeonTypes.ScorpionNest:
                    return (int)LocationNamesList.RDBTypes.BugsNest;

                case (int)DFRegion.DungeonTypes.DragonsDen:
                    return (int)LocationNamesList.RDBTypes.DragonsDen;

                case (int)DFRegion.DungeonTypes.VolcanicCaves:
                    return (int)LocationNamesList.RDBTypes.VolcanicCaves;

                case (int)DFRegion.DungeonTypes.Cemetery:
                    return (int)LocationNamesList.RDBTypes.Cemetery;
            }
        }

        protected DFLocation.DungeonBlock GetBlockData(string blockName, (int, int) position)
        {
            DFLocation.DungeonBlock block = new DFLocation.DungeonBlock();
            DFBlock rdBlock = new DFBlock();
            string blockReplacementJson = File.ReadAllText(Path.Combine(MapEditor.testPath, "RDB", string.Concat(blockName, ".json")));
            block = (DFLocation.DungeonBlock)SaveLoadManager.Deserialize(typeof(DFLocation.DungeonBlock), blockReplacementJson);
            // block = JsonConvert.DeserializeObject<DFLocation.DungeonBlock>(File.ReadAllText(Path.Combine(MapEditor.testPath, "RDB", string.Concat(blockName, ".json"))));
            rdBlock = (DFBlock)SaveLoadManager.Deserialize(typeof(DFBlock), blockReplacementJson);
            block.X = (sbyte)position.Item1;
            block.Z = (sbyte)position.Item2;
            block.IsStartingBlock = false;
            block.BlockName = blockName;
            block.WaterLevel = 10000;

            // for (int i = 0; i < rdBlock.RdbBlock.ObjectRootList.Length; i++)
            // {
            //     for (int j = 0; j < rdBlock.RdbBlock.ObjectRootList[i].RdbObjects.Length; j++)
            //     {
            //         if (rdBlock.RdbBlock.ObjectRootList[i].RdbObjects[j].Resources.FlatResource.TextureArchive == 199 &&
            //             rdBlock.RdbBlock.ObjectRootList[i].RdbObjects[j].Resources.FlatResource.TextureRecord == 10)
            //         {
            //             block.WaterLevel = rdBlock.RdbBlock.ObjectRootList[i].RdbObjects[j].Resources.FlatResource.SoundIndex;
            //             break;
            //         }
            //     }
            // }
            block.CastleBlock = false;

            return block;
        }

        protected bool CheckAdjacentBlocks(bool[,] dungeonDiagram, (int, int) position)
        {
            if (position.Item2 > 0 && dungeonDiagram[position.Item1, position.Item2 - 1])
                return true;
            if (position.Item1 < 5 && dungeonDiagram[position.Item1 + 1, position.Item2])
                return true;
            if (position.Item2 < 5 && dungeonDiagram[position.Item1, position.Item2 + 1])
                return true;
            if (position.Item1 > 0 && dungeonDiagram[position.Item1 - 1, position.Item2])
                return true;

            return false;
        }

        protected List<ushort> InitializeLocationIdList()
        {
            List<ushort> initializedList = new List<ushort>();
            bool lowestCatched = false;

            ushort lowestValue = 0;
            ushort highestValue = 0;

            int counter = 0;

            while (!lowestCatched)
            {
                if (Worldmaps.mapDict.ContainsKey((ulong)counter))
                {
                    highestValue = lowestValue = (ushort)Worldmaps.mapDict[(ulong)counter].MapID;
                    Debug.Log("Lowest Value set to " + lowestValue + " (at counter" + counter + ")");
                    lowestCatched = true;
                    initializedList.Add((ushort)Worldmaps.mapDict[(ulong)counter].MapID);
                }

                counter++;
            }

            for (int i = counter; i < int.MaxValue; i++)
            {
                if (Worldmaps.mapDict.ContainsKey((ulong)i))
                {
                    if (lowestValue > (ushort)Worldmaps.mapDict[(ulong)i].MapID)
                        lowestValue = (ushort)Worldmaps.mapDict[(ulong)i].MapID;

                    if (highestValue < (ushort)Worldmaps.mapDict[(ulong)i].MapID)
                        highestValue = (ushort)Worldmaps.mapDict[(ulong)i].MapID;

                    initializedList.Add((ushort)Worldmaps.mapDict[(ulong)i].MapID);
                }
            }

            initializedList.Sort();

            Debug.Log("Lowest MapID: " + lowestValue + "(" + initializedList[0] + "); Highest MapID: " + highestValue + "(" + initializedList[initializedList.Count - 1] + ")");

            return initializedList;
        }

        protected void InitializeLocationDensity()
        {
            for (int i = 0; i < (int)DFRegion.LocationTypes.HomeYourShips; i++)
            {
                regionOptions.locationDensity[i] = 0;

                for (int j = 0; j <= ((int)MapsFile.Climates.HauntedWoodlands - (int)MapsFile.Climates.Ocean); j++)
                {
                    regionOptions.locationDensity[i] += (locationDensity[j, i] * governmentModifier[(RegionManager.currentRegionData.governType + 1) / 2 - 1, i] * ((float)RegionManager.currentRegionData.climates[j] * 100.0f / (float)RegionManager.currentRegionData.surface)) / 100.0f;
                }

                regionOptions.locationDensity[i] += (UnityEngine.Random.Range(-0.10f, 0.10f) * regionOptions.locationDensity[i]);
            }
        }

        protected ushort GenerateNewLocationId()
        {
            bool found = false;
            ushort counter = locationIdList[0];

            do{
                counter += 2;

                if (!locationIdList.Contains(counter) && !locationIdList.Contains((ushort)(counter + 1)))
                {
                    found = true;
                }
            }
            while (!found);

            locationIdList.Add(counter);

            return counter;
        }        
    }
}