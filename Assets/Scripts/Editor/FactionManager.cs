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
using DaggerfallConnect.Utility;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using Unity.Mathematics.Editor;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEditor.Build.Pipeline.Tasks;
using UnityEngine.EventSystems;
using Newtonsoft.Json;
using System.Linq;
using DaggerfallWorkshop.Game.Player;

namespace MapEditor
{
    public class FactionManager : EditorWindow
    {
        static FactionManager factionManagerWindow;
        const string windowTitle = "Faction Manager";
        public static int currentFactionIndex;
        public static int factionType = 0;
        FactionFile.FactionData factionData;
        public static int factionTypeCount = Enum.GetNames(typeof(FactionFile.FactionTypes)).Length;
        public string[] factionTypeNames = Enum.GetNames(typeof(FactionFile.FactionTypes));
        public static string[][] factionNames = new string[factionTypeCount][];
        public static int factionId = 0;
        public int parentSwapType = 0, ally1SwapType = 0, ally2SwapType = 0, ally3SwapType = 0, enemy1SwapType = 0, enemy2SwapType = 0, enemy3SwapType = 0;
        public int parentSwapIndex = 0, regionSwapIndex = 0, rulerSwapIndex = 0, ally1SwapIndex = 0, ally2SwapIndex = 0, ally3SwapIndex = 0, enemy1SwapIndex = 0, enemy2SwapIndex = 0, enemy3SwapIndex = 0;
        public Vector2 childrenFactionScroll;

        void Awake()
        {
            SetFactionNames();
        }

        void OnGUI()
        {
            int oldFactionIndex = currentFactionIndex;
            int oldFactionType = factionType;

            factionType = EditorGUILayout.Popup("Type: ", factionType, factionTypeNames, GUILayout.MaxWidth(300.0f));
            currentFactionIndex = EditorGUILayout.Popup("Faction: ", currentFactionIndex, factionNames[factionType], GUILayout.MaxWidth(400.0f));

            if (factionType != oldFactionType)
            {
                currentFactionIndex = 0;
            }
            
            if (currentFactionIndex != oldFactionIndex)
            {
                factionId = FactionAtlas.FactionToId[factionNames[factionType][currentFactionIndex]];
                factionData = FactionAtlas.FactionDictionary[factionId];
            }

            GUILayout.Space(20.0f);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Faction Id: ", factionData.id.ToString());

            EditorGUILayout.BeginHorizontal();
            if (factionData.parent > 0)
            {
                EditorGUILayout.LabelField("Parent: ", "", GUILayout.MaxWidth(120.0f));
                if (GUILayout.Button(FactionAtlas.FactionDictionary[factionData.parent].name, GUILayout.MaxWidth(250.0f)))
                {
                    factionData = FactionAtlas.FactionDictionary[factionData.parent];
                }
                GUILayout.Space(30.0f);
            }
            else EditorGUILayout.LabelField("Parent: ", "None", GUILayout.MaxWidth(200.0f));

            parentSwapType = EditorGUILayout.Popup("Change: ", parentSwapType, factionTypeNames, GUILayout.MaxWidth(300.0f));
            parentSwapIndex = EditorGUILayout.Popup("", parentSwapIndex, factionNames[parentSwapType], GUILayout.MaxWidth(300.0f));
            if (parentSwapType >= 0 && parentSwapIndex >= 0 && GUILayout.Button("Set", GUILayout.MaxWidth(30.0f)))
                factionData.parent = FactionAtlas.FactionToId[ConvertFactionName(factionNames[parentSwapType][parentSwapIndex])];
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Type: ", ((FactionFile.FactionTypes)factionData.type).ToString());
            EditorGUILayout.LabelField("Name: ", factionData.name);
            EditorGUILayout.LabelField("Reputation: ", factionData.rep.ToString());
            factionData.summon = EditorGUILayout.IntField("Summon: ", factionData.summon, GUILayout.MaxWidth(200.0f));

            EditorGUILayout.BeginHorizontal();
            if (factionData.region >= 0 && factionData.region < WorldInfo.WorldSetting.Regions)
            {                
                EditorGUILayout.LabelField("Region: ", "", GUILayout.MaxWidth(120.0f));
                if (GUILayout.Button(WorldInfo.WorldSetting.RegionNames[factionData.region], GUILayout.MaxWidth(250.0f)))
                {
                    factionData = FactionAtlas.FactionDictionary[FactionAtlas.FactionToId[WorldInfo.WorldSetting.RegionNames[factionData.region]]];
                }
            }
            else 
            {
                EditorGUILayout.LabelField("Region: ", "None", GUILayout.MaxWidth(200.0f));
                // if (GUILayout.Button(WorldData.WorldSetting.RegionNames[factionData.region], GUILayout.MaxWidth(250.0f)))
                // {
                //     factionData = FactionAtlas.FactionDictionary[FactionAtlas.FactionToId[WorldData.WorldSetting.RegionNames[factionData.region]]];
                // }
            }

            regionSwapIndex = EditorGUILayout.Popup("Change: ", regionSwapIndex, factionNames[(int)FactionFile.FactionTypes.Province], GUILayout.MaxWidth(300.0f));
            if (regionSwapIndex >= 0 && GUILayout.Button("Set", GUILayout.MaxWidth(30.0f)))
                factionData.region = FactionAtlas.FactionDictionary[FactionAtlas.FactionToId[ConvertFactionName(factionNames[(int)FactionFile.FactionTypes.Province][regionSwapIndex])]].region;
            EditorGUILayout.EndHorizontal();

            factionData.power = EditorGUILayout.IntField("Power: ", factionData.power, GUILayout.MaxWidth(200.0f));
            EditorGUILayout.LabelField("Flags: ", factionData.flags.ToString());

            EditorGUILayout.BeginHorizontal();
            if (factionData.ruler > 0)
            {
                EditorGUILayout.LabelField("Ruler: ", ((RegionManager.Ruler)factionData.ruler).ToString(), GUILayout.MaxWidth(200.0f));
            }
            else EditorGUILayout.LabelField("Ruler: ", "None", GUILayout.MaxWidth(200.0f));

            if (factionData.type == (int)FactionFile.FactionTypes.Province)
            {
                rulerSwapIndex = EditorGUILayout.Popup("Change: ", rulerSwapIndex, Enum.GetNames(typeof(RegionManager.Ruler)), GUILayout.MaxWidth(300.0f));
                if (rulerSwapIndex >= 0 && GUILayout.Button("Set", GUILayout.MaxWidth(30.0f)))
                    factionData.ruler = rulerSwapIndex + 1;
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (factionData.ally1 > 0)
            {
                EditorGUILayout.LabelField("Ally1: ", "", GUILayout.MaxWidth(120.0f));
                if (GUILayout.Button(FactionAtlas.FactionDictionary[factionData.ally1].name, GUILayout.MaxWidth(250.0f)))
                {
                    factionData = FactionAtlas.FactionDictionary[factionData.ally1];
                }
                GUILayout.Space(30.0f);
            }
            else EditorGUILayout.LabelField("Ally1: ", "None", GUILayout.MaxWidth(200.0f));

            ally1SwapType = EditorGUILayout.Popup("Change: ", ally1SwapType, factionTypeNames, GUILayout.MaxWidth(300.0f));
            ally1SwapIndex = EditorGUILayout.Popup("", ally1SwapIndex, factionNames[ally1SwapType], GUILayout.MaxWidth(300.0f));
            if (ally1SwapType >= 0 && ally1SwapIndex >= 0 && GUILayout.Button("Set", GUILayout.MaxWidth(30.0f)))
                factionData.ally1 = FactionAtlas.FactionToId[ConvertFactionName(factionNames[ally1SwapType][ally1SwapIndex])];
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (factionData.ally2 > 0)
            {
                EditorGUILayout.LabelField("Ally2: ", "", GUILayout.MaxWidth(120.0f));
                if (GUILayout.Button(FactionAtlas.FactionDictionary[factionData.ally2].name, GUILayout.MaxWidth(250.0f)))
                {
                    factionData = FactionAtlas.FactionDictionary[factionData.ally2];
                }
            }
            else EditorGUILayout.LabelField("Ally2: ", "None", GUILayout.MaxWidth(200.0f));

            ally2SwapType = EditorGUILayout.Popup("Change: ", ally2SwapType, factionTypeNames, GUILayout.MaxWidth(300.0f));
            ally2SwapIndex = EditorGUILayout.Popup("", ally2SwapIndex, factionNames[ally2SwapType], GUILayout.MaxWidth(300.0f));
            if (ally2SwapType >= 0 && ally2SwapIndex >= 0 && GUILayout.Button("Set", GUILayout.MaxWidth(30.0f)))
                factionData.ally2 = FactionAtlas.FactionToId[ConvertFactionName(factionNames[ally2SwapType][ally2SwapIndex])];
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (factionData.ally3 > 0)
            {
                EditorGUILayout.LabelField("Ally3: ", "", GUILayout.MaxWidth(120.0f));
                if (GUILayout.Button(FactionAtlas.FactionDictionary[factionData.ally3].name, GUILayout.MaxWidth(250.0f)))
                {
                    factionData = FactionAtlas.FactionDictionary[factionData.ally3];
                }
            }
            else EditorGUILayout.LabelField("Ally3: ", "None", GUILayout.MaxWidth(200.0f));

            ally3SwapType = EditorGUILayout.Popup("Change: ", ally3SwapType, factionTypeNames, GUILayout.MaxWidth(300.0f));
            ally3SwapIndex = EditorGUILayout.Popup("", ally3SwapIndex, factionNames[ally3SwapType], GUILayout.MaxWidth(300.0f));
            if (ally3SwapType >= 0 && ally3SwapIndex >= 0 && GUILayout.Button("Set", GUILayout.MaxWidth(30.0f)))
                factionData.ally3 = FactionAtlas.FactionToId[ConvertFactionName(factionNames[ally3SwapType][ally3SwapIndex])];
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (factionData.enemy1 > 0)
            {
                EditorGUILayout.LabelField("Enemy1: ", "", GUILayout.MaxWidth(120.0f));
                if (GUILayout.Button(FactionAtlas.FactionDictionary[factionData.enemy1].name, GUILayout.MaxWidth(250.0f)))
                {
                    factionData = FactionAtlas.FactionDictionary[factionData.enemy1];
                }
            }
            else EditorGUILayout.LabelField("Enemy1: ", "None", GUILayout.MaxWidth(200.0f));

            enemy1SwapType = EditorGUILayout.Popup("Change: ", enemy1SwapType, factionTypeNames, GUILayout.MaxWidth(300.0f));
            enemy1SwapIndex = EditorGUILayout.Popup("", enemy1SwapIndex, factionNames[enemy1SwapType], GUILayout.MaxWidth(300.0f));
            if (enemy1SwapType >= 0 && enemy1SwapIndex >= 0 && GUILayout.Button("Set", GUILayout.MaxWidth(30.0f)))
                factionData.enemy1 = FactionAtlas.FactionToId[ConvertFactionName(factionNames[enemy1SwapType][enemy1SwapIndex])];
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (factionData.enemy2 > 0)
            {
                EditorGUILayout.LabelField("Enemy2: ", "", GUILayout.MaxWidth(120.0f));
                if (GUILayout.Button(FactionAtlas.FactionDictionary[factionData.enemy2].name, GUILayout.MaxWidth(250.0f)))
                {
                    factionData = FactionAtlas.FactionDictionary[factionData.enemy2];
                }
            }
            else EditorGUILayout.LabelField("Enemy2: ", "None", GUILayout.MaxWidth(200.0f));

            enemy2SwapType = EditorGUILayout.Popup("Change: ", enemy2SwapType, factionTypeNames, GUILayout.MaxWidth(300.0f));
            enemy2SwapIndex = EditorGUILayout.Popup("", enemy2SwapIndex, factionNames[enemy2SwapType], GUILayout.MaxWidth(300.0f));
            if (enemy2SwapType >= 0 && enemy2SwapIndex >= 0 && GUILayout.Button("Set", GUILayout.MaxWidth(30.0f)))
                factionData.enemy2 = FactionAtlas.FactionToId[ConvertFactionName(factionNames[enemy2SwapType][enemy2SwapIndex])];
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (factionData.enemy3 > 0)
            {
                EditorGUILayout.LabelField("Enemy3: ", "", GUILayout.MaxWidth(120.0f));
                if (GUILayout.Button(FactionAtlas.FactionDictionary[factionData.enemy3].name, GUILayout.MaxWidth(250.0f)))
                {
                    factionData = FactionAtlas.FactionDictionary[factionData.enemy3];
                }
            }
            else EditorGUILayout.LabelField("Enemy3: ", "None", GUILayout.MaxWidth(200.0f));

            enemy3SwapType = EditorGUILayout.Popup("Change: ", enemy3SwapType, factionTypeNames, GUILayout.MaxWidth(300.0f));
            enemy3SwapIndex = EditorGUILayout.Popup("", enemy3SwapIndex, factionNames[enemy3SwapType], GUILayout.MaxWidth(300.0f));
            if (enemy3SwapType >= 0 && enemy3SwapIndex >= 0 && GUILayout.Button("Set", GUILayout.MaxWidth(30.0f)))
                factionData.enemy3 = FactionAtlas.FactionToId[ConvertFactionName(factionNames[enemy3SwapType][enemy3SwapIndex])];
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Face: ", (factionData.face).ToString());
            EditorGUILayout.LabelField("Race: ", ((FactionFile.FactionRaces)factionData.race).ToString());
            EditorGUILayout.LabelField("Flat1: ", (factionData.flat1).ToString());
            EditorGUILayout.LabelField("Flat2: ", (factionData.flat2).ToString());
            EditorGUILayout.LabelField("Sgroup: ", ((FactionFile.SocialGroups)factionData.sgroup).ToString());
            EditorGUILayout.LabelField("Ggroup: ", ((FactionFile.GuildGroups)factionData.ggroup).ToString());
            EditorGUILayout.LabelField("Minf: ", (factionData.minf).ToString());
            EditorGUILayout.LabelField("Maxf: ", (factionData.maxf).ToString());

            EditorGUILayout.BeginHorizontal();
            if (factionData.vam > 0)
            {
                EditorGUILayout.LabelField("Vampire clan: ", "", GUILayout.MaxWidth(120.0f));
                if (GUILayout.Button(FactionAtlas.FactionDictionary[factionData.vam].name, GUILayout.MaxWidth(250.0f)))
                {
                    factionData = FactionAtlas.FactionDictionary[factionData.vam];
                }
            }
            else EditorGUILayout.LabelField("Vampire clan: ", "None");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.LabelField("Rank: ", (factionData.rank).ToString());
            EditorGUILayout.LabelField("Ruler name seed: ", (factionData.rulerNameSeed).ToString());
            EditorGUILayout.LabelField("Ruler power bonus: ", (factionData.rulerPowerBonus).ToString());
            EditorGUILayout.LabelField("Next faction: ", (factionData.ptrToNextFactionAtSameHierarchyLevel).ToString());
            EditorGUILayout.LabelField("First child faction: ", (factionData.ptrToFirstChildFaction).ToString());
            EditorGUILayout.LabelField("Parent faction: ", (factionData.ptrToParentFaction).ToString());

            if (factionData.children != null)
            {
                EditorGUILayout.LabelField("Children factions: ", "");

                childrenFactionScroll = EditorGUILayout.BeginScrollView(childrenFactionScroll);
                foreach (int fact in factionData.children)
                {
                    if (GUILayout.Button(FactionAtlas.FactionDictionary[fact].name, GUILayout.MaxWidth(250.0f)))
                    {
                        factionData = FactionAtlas.FactionDictionary[fact];
                    }
                }

                EditorGUILayout.EndScrollView();
            }
            else EditorGUILayout.LabelField("Children factions: ", "None");
            EditorGUILayout.EndVertical();            

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Save Changes", GUILayout.MaxWidth(100)))
            {
                FactionAtlas.FactionDictionary.Remove(factionData.id);
                FactionAtlas.FactionDictionary.Add(factionData.id, factionData);
                SaveFactionChanges();
            }
        }

        protected void SaveFactionChanges()
        {
            string fileDataPath = Path.Combine(MapEditor.testPath, "Faction.json");
            var json = JsonConvert.SerializeObject(FactionAtlas.FactionDictionary, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            File.WriteAllText(fileDataPath, json);

            fileDataPath = Path.Combine(MapEditor.testPath, "FactionToId.json");
            json = JsonConvert.SerializeObject(FactionAtlas.FactionToId, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            File.WriteAllText(fileDataPath, json);
        }

        protected void SetFactionNames()
        {
            List<string> nameList = new List<string>();
            List<string> daedraList = new List<string>();
            List<string> godList = new List<string>();
            List<string> groupList = new List<string>();
            List<string> subgroupList = new List<string>();
            List<string> individualList = new List<string>();
            List<string> officialList = new List<string>();
            List<string> vampireClanList = new List<string>();
            List<string> provinceList = new List<string>();
            List<string> witchesCovenList = new List<string>();
            List<string> templeList = new List<string>();
            List<string> knightlyGuardList = new List<string>();
            List<string> magicUserList = new List<string>();
            List<string> genericList = new List<string>();
            List<string> thievesList = new List<string>();
            List<string> courtsList = new List<string>();
            List<string> peopleList = new List<string>();

            factionTypeNames[factionTypeCount - 1] = "All";
            factionNames[factionTypeCount - 1] = new string[FactionAtlas.FactionDictionary.Count];
            int counter = 0;
            nameList.Add("None");

            while (nameList.Count <= FactionAtlas.FactionDictionary.Count)
            {
                if (FactionAtlas.FactionDictionary.ContainsKey(counter))
                {
                    factionData = FactionAtlas.FactionDictionary[counter];
                    nameList.Add(factionData.name);
                }

                switch (factionData.type)
                {
                    case 0:
                        daedraList.Add(factionData.name);
                        break;

                    case 1:
                        godList.Add(factionData.name);
                        break;

                    case 2:
                        groupList.Add(factionData.name);
                        break;

                    case 3:
                        subgroupList.Add(factionData.name);
                        break;

                    case 4:
                        individualList.Add(factionData.name);
                        break;

                    case 5:
                        officialList.Add(factionData.name);
                        break;

                    case 6:
                        vampireClanList.Add(factionData.name);
                        break;

                    case 7:
                        provinceList.Add(factionData.name);
                        break;

                    case 8:
                        witchesCovenList.Add(factionData.name);
                        break;

                    case 9:
                        templeList.Add(factionData.name);
                        break;

                    case 10:
                        knightlyGuardList.Add(factionData.name);
                        break;

                    case 11:
                        magicUserList.Add(factionData.name);
                        break;

                    case 12:
                        genericList.Add(factionData.name);
                        break;

                    case 13:
                        thievesList.Add(factionData.name);
                        break;

                    case 14:
                        courtsList.Add(factionData.name);
                        break;

                    case 15:
                        peopleList.Add(factionData.name);
                        break;

                    default:
                        break;
                }

                counter++;
            }

            factionNames[(int)FactionFile.FactionTypes.Daedra] = new string[daedraList.Count];
            if (daedraList.Count > 0)
            {
                factionNames[(int)FactionFile.FactionTypes.Daedra] = daedraList.ToArray();
            }

            factionNames[(int)FactionFile.FactionTypes.God] = new string[godList.Count];
            if (godList.Count > 0)
            {
                factionNames[(int)FactionFile.FactionTypes.God] = godList.ToArray();
            }

            factionNames[(int)FactionFile.FactionTypes.Group] = new string[groupList.Count];
            if (groupList.Count > 0)
            {
                factionNames[(int)FactionFile.FactionTypes.Group] = groupList.ToArray();
            }

            factionNames[(int)FactionFile.FactionTypes.Subgroup] = new string[subgroupList.Count];
            if (subgroupList.Count > 0)
            {
                factionNames[(int)FactionFile.FactionTypes.Subgroup] = subgroupList.ToArray();
            }

            factionNames[(int)FactionFile.FactionTypes.Individual] = new string[individualList.Count];
            if (individualList.Count > 0)
            {
                factionNames[(int)FactionFile.FactionTypes.Individual] = individualList.ToArray();
            }

            factionNames[(int)FactionFile.FactionTypes.Official] = new string[officialList.Count];
            if (officialList.Count > 0)
            {
                factionNames[(int)FactionFile.FactionTypes.Official] = officialList.ToArray();
            }

            factionNames[(int)FactionFile.FactionTypes.VampireClan] = new string[vampireClanList.Count];
            if (vampireClanList.Count > 0)
            {
                factionNames[(int)FactionFile.FactionTypes.VampireClan] = vampireClanList.ToArray();
            }

            factionNames[(int)FactionFile.FactionTypes.Province] = new string[provinceList.Count];
            if (provinceList.Count > 0)
            {
                factionNames[(int)FactionFile.FactionTypes.Province] = provinceList.ToArray();
            }

            factionNames[(int)FactionFile.FactionTypes.WitchesCoven] = new string[witchesCovenList.Count];
            if (witchesCovenList.Count > 0)
            {
                factionNames[(int)FactionFile.FactionTypes.WitchesCoven] = witchesCovenList.ToArray();
            }

            factionNames[(int)FactionFile.FactionTypes.Temple] = new string[templeList.Count];
            if (templeList.Count > 0)
            {
                factionNames[(int)FactionFile.FactionTypes.Temple] = templeList.ToArray();
            }

            factionNames[(int)FactionFile.FactionTypes.KnightlyGuard] = new string[knightlyGuardList.Count];
            if (knightlyGuardList.Count > 0)
            {
                factionNames[(int)FactionFile.FactionTypes.KnightlyGuard] = knightlyGuardList.ToArray();
            }

            factionNames[(int)FactionFile.FactionTypes.MagicUser] = new string[magicUserList.Count];
            if (magicUserList.Count > 0)
            {
                factionNames[(int)FactionFile.FactionTypes.MagicUser] = magicUserList.ToArray();
            }

            factionNames[(int)FactionFile.FactionTypes.Generic] = new string[genericList.Count];
            if (genericList.Count > 0)
            {
                factionNames[(int)FactionFile.FactionTypes.Generic] = genericList.ToArray();
            }

            factionNames[(int)FactionFile.FactionTypes.Thieves] = new string[thievesList.Count];
            if (thievesList.Count > 0)
            {
                factionNames[(int)FactionFile.FactionTypes.Thieves] = thievesList.ToArray();
            }

            factionNames[(int)FactionFile.FactionTypes.Courts] = new string[courtsList.Count];
            if (courtsList.Count > 0)
            {
                factionNames[(int)FactionFile.FactionTypes.Courts] = courtsList.ToArray();
            }

            factionNames[(int)FactionFile.FactionTypes.People] = new string[peopleList.Count];
            if (peopleList.Count > 0)
            {
                factionNames[(int)FactionFile.FactionTypes.People] = peopleList.ToArray();
            }
            
            factionNames[factionTypeCount - 1] = nameList.ToArray();            

            // string fileDataPath = Path.Combine(MapEditor.testPath, "FactionToId.json");
            // var json = JsonConvert.SerializeObject(FactionAtlas.FactionToId, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            // File.WriteAllText(fileDataPath, json);
        }

        protected string ConvertFactionName(string name)
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
    }
}