using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop;
using DaggerfallConnect.Utility;
using Newtonsoft.Json;
using MapEditor;

namespace BlockRotator
{
    public class BlockRotatorWindow : EditorWindow
    {
        static BlockRotatorWindow blockRotatorWindow;
        const string windowTitle = "Block Rotator";
        const string menuPath = "Daggerfall Tools/Block Rotator";
        public const string testPath = "/home/arneb/Games/daggerfall/DaggerfallGameFiles/arena2/Maps/";

        public static string[] blockPrefixes = {
            "TVR", "GEN", "RES", "WEA", "ARM", "ALC", "BAN", "BOO",
            "CLO", "FUR", "GEM", "LIB", "PAW", "TEM", "PAL", "FAR", 
            "DUN", "CAS", "CAR", "MAN", "SHR", "RUI", "SHC", "GRV",
            "FIL", "KRA", "KDR", "KOW", "KMO", "KCA", "KFL", "KHO",
            "KRO", "KWH", "KSC", "KHA", "MAG", "THI", "DAR", "FIG",
            "CUS", "WAL", "MAR", "SHI", "WIT",
            "B", "N", "W", "S", "M"
        };

        public static string[] blockNotToRotate = {
            "CUS", "WAL", "SHI"
        };

        public static string[] rmbBlockClimate = {
            "AA", "AL", "AM", "AS",     // High Rock
            "BA", "BL", "BM", "BS",     // Hammerfell
            "GA", "GL", "GM", "GS"      // To reassign, since it's a copy/paste of the "B" group
        };

        public static string[] deityCode = {
            "A", "B", "C", "D", "E", "F", "G", "H"
        };

        public static readonly string[] rotationNames = { "90", "180", "270" };

        public static readonly DFLocation.BuildingTypes[] merchantBuilding = {
            DFLocation.BuildingTypes.Alchemist,
            DFLocation.BuildingTypes.Armorer,
            DFLocation.BuildingTypes.Bank,
            DFLocation.BuildingTypes.Bookseller,
            DFLocation.BuildingTypes.ClothingStore,
            // DFLocation.BuildingTypes.FurnitureStore,
            DFLocation.BuildingTypes.GemStore,
            DFLocation.BuildingTypes.GeneralStore,
            DFLocation.BuildingTypes.Library,
            DFLocation.BuildingTypes.PawnShop,
            DFLocation.BuildingTypes.Tavern,
            DFLocation.BuildingTypes.WeaponSmith
        };

        public static bool typeToggle;
        public static bool blockToggle;
        public static bool rotationToggle;

        public static int blockType = 0;
        public static int climateType = 0;
        public static int selectedBlock = 0;
        public static int selectedRotation = 0;
        public static string[][] availableBlocks;

        [MenuItem(menuPath)]
        static void Init()
        {
            blockRotatorWindow = (BlockRotatorWindow)EditorWindow.GetWindow(typeof(BlockRotatorWindow));
            blockRotatorWindow.titleContent = new GUIContent(windowTitle);
        }

        void Awake()
        {
            typeToggle = false;
            blockToggle = false;
            rotationToggle = false;

            availableBlocks = new string[blockPrefixes.Length][];

            for (int i = 0; i < blockPrefixes.Length; i++)
            {
                List<string> blockList = new List<string>();

                if (blockPrefixes[i].Equals("N") || blockPrefixes[i].Equals("W") || blockPrefixes[i].Equals("B") || blockPrefixes[i].Equals("S") || blockPrefixes[i].Equals("M"))
                {
                    int counter = 0;
                    string blockName = ElaborateBlockName(blockPrefixes[i], string.Empty, string.Empty, "Z", counter);
                    while (File.Exists(Path.Combine(testPath, "RDB", blockName + ".RDB.json")))
                    {
                        blockList.Add(blockName);
                        counter++;
                        blockName = ElaborateBlockName(blockPrefixes[i], string.Empty, string.Empty, "Z", counter);
                    }
                }
                else
                {
                    for (int j = 0; j < rmbBlockClimate.Length; j++)
                    {
                        int counter = 0;
                        if (blockPrefixes[i].Equals("TEM"))
                        {
                            for (int p = 0; p < 4; p++)
                            {
                                for (int k = 0; k < deityCode.Length; k++)
                                {
                                    counter = 0;
                                    string blockName = ElaborateBlockName(blockPrefixes[i], p.ToString(), rmbBlockClimate[j], deityCode[k], counter);
                                    while (File.Exists(Path.Combine(testPath, "RMB", blockName + ".RMB.json")))
                                    {
                                        blockList.Add(blockName);
                                        counter++;
                                        blockName = ElaborateBlockName(blockPrefixes[i], p.ToString(), rmbBlockClimate[j], deityCode[k], counter);
                                    }
                                }
                            }
                        }
                        else
                        {
                            for (int o = 0; o < 4; o++)
                            {
                                string blockName = ElaborateBlockName(blockPrefixes[i], o.ToString(), rmbBlockClimate[j], "Z", counter);
                                while (File.Exists(Path.Combine(testPath, "RMB", blockName + ".RMB.json")))
                                {
                                    blockList.Add(blockName);
                                    counter++;
                                    blockName = ElaborateBlockName(blockPrefixes[i], o.ToString(), rmbBlockClimate[j], "Z", counter);
                                }
                            }
                        }
                    }
                }
                availableBlocks[i] = blockList.ToArray();
            }
        }

        void OnGUI()
        {
            typeToggle = EditorGUILayout.BeginToggleGroup("Choose Block Type", typeToggle);  
             
            blockType = EditorGUILayout.Popup("Block Type: ", blockType, blockPrefixes, GUILayout.MaxWidth(600.0f));            
            blockToggle = EditorGUILayout.BeginToggleGroup("Choose Specific Block", blockToggle);   
            selectedBlock = EditorGUILayout.Popup("Selected: ", selectedBlock, availableBlocks[blockType], GUILayout.MaxWidth(600.0f));
            EditorGUILayout.EndToggleGroup();
            EditorGUILayout.EndToggleGroup();

            rotationToggle = EditorGUILayout.BeginToggleGroup("Choose Rotation", rotationToggle);
            selectedRotation = EditorGUILayout.Popup("Rotation: ", selectedRotation, rotationNames, GUILayout.MaxWidth(600.0f));
            EditorGUILayout.EndToggleGroup();           

            if (GUILayout.Button("ROTATE", GUILayout.MaxWidth(200.0f)))
            {
                ActivateRotation();
            }

            if (GUILayout.Button("ConvertToClassic", GUILayout.MaxWidth(200.0f)))
            {
                ConvertToClassic(blockType, selectedBlock);
            }

            if (GUILayout.Button("ConvertToPN", GUILayout.MaxWidth(200.0f)))
            {
                ConvertToPN();
            }

            if (GUILayout.Button("Create BLOCKS.BSA", GUILayout.MaxWidth(200.0f)))
            {
                CreateBlocksBsa();
            }

            if (GUILayout.Button("Insert New Blocks", GUILayout.MaxWidth(200.0f)))
            {
                InsertRotatedAndNewBlocks();
            }

            if (GUILayout.Button("Calculate Block Limits", GUILayout.MaxWidth(200.0f)))
            {
                CalculateBlockLimits();
            }
        }

        public static string ElaborateBlockName(string prefix, string rotation, string climate, string deity, int progressive)
        {
            if (prefix.Equals("TEM"))
                return prefix + rotation + climate + deity + progressive.ToString();
            else if (prefix.Equals("N") || prefix.Equals("W") || prefix.Equals("B") || prefix.Equals("S") || prefix.Equals("M"))
                return prefix + progressive.ToString("0000000");
            else
                return prefix + rotation + climate + progressive.ToString("00");
        }

        public static void ActivateRotation()
        {
            if (!typeToggle)
            {
                for (int i = 0; i < availableBlocks.Length; i++)
                {
                    for (int j = 0; j < availableBlocks[i].Length; j++)
                    {
                        if (!rotationToggle)
                        {
                            for (int k = 0; k < 3; k++)
                            {
                                RotateBlock(availableBlocks[i][j], k);
                            }                                
                        }
                        else
                        {
                            RotateBlock(availableBlocks[i][j], selectedRotation);
                        }                        
                    }                        
                }
            }
            else
            {
                if (!blockToggle)
                {
                    for (int l = 0; l < availableBlocks[blockType].Length; l++)
                    {
                        if (!rotationToggle)
                        {
                            for (int m = 0; m < 3; m++)
                            {
                                RotateBlock(availableBlocks[blockType][l], m);
                            }
                        }
                        else
                        {
                            RotateBlock(availableBlocks[blockType][l], selectedRotation);
                        }
                    }
                }
                else
                {
                    if (!rotationToggle)
                    {
                        for (int n = 0; n < 3; n++)
                        {
                            RotateBlock(availableBlocks[blockType][selectedBlock], n);
                        }
                    }
                    else
                        RotateBlock(availableBlocks[blockType][selectedBlock], selectedRotation);
                }
            }
        }

        public static void ConvertToClassic(int type, int selected)
        {
            if (!typeToggle)
                return;

            DFBlock block = new DFBlock();
            DFBlockRotation convBlock = new DFBlockRotation();
            bool isRmb = false;
            string path = string.Empty;
            // string correctName;

            if (blockToggle)
            {
                path = Path.Combine(testPath, "RMB", availableBlocks[type][selected] + ".RMB.json");
                if (File.Exists(path))
                    isRmb = true;
                else
                    path = Path.Combine(testPath, "RDB", availableBlocks[type][selected] + ".RDB.json");

                // if (isRmb)
                //     correctName = availableBlocks[type][selected] + ".RMB";
                // else
                //     correctName = availableBlocks[type][selected] + ".RDB";

                block = JsonConvert.DeserializeObject<DFBlock>(File.ReadAllText(path));

                convBlock = ConvertBlock(block);

                if (isRmb)
                    path = Path.Combine(testPath, "RMB", "converted", convBlock.Name + ".json");
                else
                    path = Path.Combine(testPath, "RDB", "converted", convBlock.Name + ".json");
                var jsonBlock = JsonConvert.SerializeObject(convBlock, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                File.WriteAllText(path, jsonBlock);
            }
            else
            {
                for (int i = 0; i < availableBlocks[type].Length; i++)
                {
                    path = Path.Combine(testPath, "RMB", availableBlocks[type][i] + ".RMB.json");
                    if (File.Exists(path))
                        isRmb = true;
                    else
                        path = Path.Combine(testPath, "RDB", availableBlocks[type][selected] + ".RDB.json");
                    block = JsonConvert.DeserializeObject<DFBlock>(File.ReadAllText(path));

                    convBlock = ConvertBlock(block);

                    if (isRmb)
                        path = Path.Combine(testPath, "RMB", "converted", convBlock.Name + ".json");
                    else
                        path = Path.Combine(testPath, "RDB", "converted", convBlock.Name + ".json");
                    var jsonBlock = JsonConvert.SerializeObject(convBlock, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    File.WriteAllText(path, jsonBlock);
                }
            }            
        }

        public static void ConvertToPN()
        {
            List<string> blockList = new List<string>();
            for (int i = 0; i < blockPrefixes.Length; i++)
            {
                if (blockPrefixes[i].Equals("N") || blockPrefixes[i].Equals("W") || blockPrefixes[i].Equals("B") || blockPrefixes[i].Equals("S") || blockPrefixes[i].Equals("M"))
                {
                    for (int a = 0; a < availableBlocks[i].Length; a++)
                    {
                        string blockName = availableBlocks[i][a];
                        if (File.Exists(Path.Combine(testPath, "RDB", "converted", blockName + ".RDB.json")))
                            blockList.Add(blockName);
                    }
                }
                else
                {
                    for (int j = 0; j < rmbBlockClimate.Length; j++)
                    {
                        if (blockPrefixes[i].Equals("TEMP"))
                        {
                            for (int b = 0; b < availableBlocks[i].Length; b++)
                            {
                                string blockName = availableBlocks[i][b];
                                if (File.Exists(Path.Combine(testPath, "RMB", "converted", blockName + ".RMB.json")))
                                    blockList.Add(blockName);
                            }
                        }
                        else
                        {
                            for (int c = 0; c < availableBlocks[i].Length; c++)
                            {
                                string blockName = availableBlocks[i][c];
                                if (File.Exists(Path.Combine(testPath, "RMB", "converted", blockName + ".RMB.json")))
                                    blockList.Add(blockName);
                            }
                        }
                    }
                }
            }

            foreach (string blockName in blockList)
            {
                Debug.Log("Converting " + blockName);
                DFBlockRotation block = new DFBlockRotation();
                DFBlock convBlock = new DFBlock();
                bool isRmb = false;
                string path = Path.Combine(testPath, "RMB", "converted", blockName + ".RMB.json");
                if (File.Exists(path))
                    isRmb = true;
                else
                    path = Path.Combine(testPath, "RDB", "converted", blockName + ".RDB.json");

                block = JsonConvert.DeserializeObject<DFBlockRotation>(File.ReadAllText(path));                

                convBlock = ConvertBlock(block);

                if (convBlock.Type == DFBlock.BlockTypes.Rmb)
                    path = Path.Combine(testPath, "RMB", convBlock.Name + ".json");
                else if (convBlock.Type == DFBlock.BlockTypes.Rdb)
                    path = Path.Combine(testPath, "RDB", convBlock.Name + ".json");
                else break;
                var jsonBlock = JsonConvert.SerializeObject(convBlock, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                File.WriteAllText(path, jsonBlock);
            }
        }

        public static void CreateBlocksBsa()
        {
            List<long> positionList = new List<long>();
            List<int> indexList = new List<int>();
            List<(string, int)> blocksBsa = new List<(string, int)>();

            for (int i = 0; i < availableBlocks.Length; i++)
            {
                for (int j = 0; j < availableBlocks[i].Length; j++)
                {
                    bool blockModified = false;
                    bool positionModified = false;
                    bool indexModified = false;

                    long positionCounter;
                    int indexCounter;

                    DFBlockRotation block = new DFBlockRotation();
                    Debug.Log("Working on " + availableBlocks[i][j] + " (i: " + i + ", j: " + j + ")");

                    string path;
                    string correctName;
                    bool isRmb = true;

                    if (i >= 44)
                    {
                        path = Path.Combine(testPath, "RDB", availableBlocks[i][j] + ".RDB.json");
                        block = JsonConvert.DeserializeObject<DFBlockRotation>(File.ReadAllText(path));
                        correctName = availableBlocks[i][j] + ".RDB";
                        isRmb = false;
                    }
                    else
                    {
                        path = Path.Combine(testPath, "RMB", availableBlocks[i][j] + ".RMB.json");
                        block = JsonConvert.DeserializeObject<DFBlockRotation>(File.ReadAllText(path));
                        correctName = availableBlocks[i][j] + ".RMB";
                    }

                    if (correctName != block.Name)
                    {
                        block.Name = correctName;
                        blockModified = true;
                    }
                    if (correctName != block.RmbBlock.FldHeader.Name)
                    {
                        block.RmbBlock.FldHeader.Name = correctName;
                        blockModified = true;
                    }

                    positionCounter = block.Position;
                    while (positionList.Contains(positionCounter))
                    {
                        positionCounter++;
                        positionModified = true;
                    }
                    if (positionModified)
                    {
                        block.Position = positionCounter;
                        blockModified = true;
                    }

                    indexCounter = block.Index;
                    while (indexList.Contains(indexCounter))
                    {
                        indexCounter++;
                        indexModified = true;
                    }
                    if (indexModified)
                    {
                        block.Index = indexCounter;
                        blockModified = true;
                    }

                    positionList.Add(block.Position);
                    indexList.Add(block.Index);
                    blocksBsa.Add((block.Name, block.Index));

                    DFBlock convBlock = ConvertBlock(block);

                    if (blockModified)
                    {
                        if (isRmb)
                            path = Path.Combine(testPath, "RMB", availableBlocks[i][j] + ".RMB.json");
                        else
                            path = Path.Combine(testPath, "RDB", availableBlocks[i][j] + ".RDB.json");
                        var jsonBlock = JsonConvert.SerializeObject(convBlock, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                        File.WriteAllText(path, jsonBlock);
                    }
                }
            }

            string resultingPath = Path.Combine(testPath, "BLOCKS.BSA");
            var json = JsonConvert.SerializeObject(blocksBsa, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            File.WriteAllText(resultingPath, json);
        }

        public static DFBlock ConvertBlock(DFBlockRotation block)
        {
            DFBlock convBlock = new DFBlock();

            convBlock.Position = block.Position;
            convBlock.Index = block.Index;
            if (int.TryParse(block.Name.Substring(3, 1), out int rotIndex))
            {
                convBlock.Name = block.Name;
                convBlock.RmbBlock.FldHeader.Name = block.Name;
            }
            else
            {
                convBlock.Name = block.Name.Substring(0, 3) + "0" + block.Name.Substring(4, 8);
                convBlock.RmbBlock.FldHeader.Name = convBlock.Name;
            }
            convBlock.Type = (DFBlock.BlockTypes)block.Type;

            convBlock.RmbBlock = GetRmbBlockDesc(convBlock.Type, block.RmbBlock);
            convBlock.RdbBlock = GetRdbBlockDesc(convBlock.Type, block.RdbBlock);

            convBlock.RdiBlock = new DFBlock.RdiBlockDesc();

            return convBlock;
        }

        public static DFBlockRotation ConvertBlock(DFBlock block)
        {
            DFBlockRotation convBlock = new DFBlockRotation();

            convBlock.Position = block.Position;
            convBlock.Index = block.Index;
            if (int.TryParse(block.Name.Substring(3, 1), out int rotIndex))
            {
                convBlock.Name = block.Name;
                convBlock.RmbBlock.FldHeader.Name = block.Name;
            }
            else
            {
                convBlock.Name = block.Name.Substring(0, 3) + "0" + block.Name.Substring(4, 8);
                convBlock.RmbBlock.FldHeader.Name = convBlock.Name;
            }
            convBlock.Type = (DFBlockRotation.BlockTypes)block.Type;

            convBlock.RmbBlock = GetRmbBlockDesc(convBlock.Type, block.RmbBlock);
            convBlock.RdbBlock = GetRdbBlockDesc(convBlock.Type, block.RdbBlock);

            convBlock.RdiBlock = new DFBlockRotation.RdiBlockDesc();

            return convBlock;
        }

        public static DFBlock.RmbBlockDesc GetRmbBlockDesc(DFBlock.BlockTypes type, DFBlockRotation.RmbBlockDesc block)
        {
            DFBlock.RmbBlockDesc convBlock = new DFBlock.RmbBlockDesc();

            switch (type)
            {
                case DFBlock.BlockTypes.Rmb:
                    convBlock = GetRmbBDForRmb(block);
                    break;
                case DFBlock.BlockTypes.Rdb:
                    convBlock = new DFBlock.RmbBlockDesc();
                    // convBlock = GetRmbBDForRdb(block);
                    break;
                default:
                    Debug.Log("GetRmbBlockDesc = Unknown block type");
                    break;
            }

            return convBlock;
        }

        public static DFBlockRotation.RmbBlockDesc GetRmbBlockDesc(DFBlockRotation.BlockTypes type, DFBlock.RmbBlockDesc block)
        {
            DFBlockRotation.RmbBlockDesc convBlock = new DFBlockRotation.RmbBlockDesc();

            switch (type)
            {
                case DFBlockRotation.BlockTypes.Rmb:
                    convBlock = GetRmbBDForRmb(block);
                    break;
                case DFBlockRotation.BlockTypes.Rdb:
                    convBlock = new DFBlockRotation.RmbBlockDesc();
                    // convBlock = GetRmbBDForRdb(block);
                    break;
                default:
                    Debug.Log("GetRmbBlockDesc = Unknown block type");
                    break;
            }

            return convBlock;
        }

        public static DFBlock.RdbBlockDesc GetRdbBlockDesc(DFBlock.BlockTypes type, DFBlockRotation.RdbBlockDesc block)
        {
            DFBlock.RdbBlockDesc convBlock = new DFBlock.RdbBlockDesc();

            switch (type)
            {
                case DFBlock.BlockTypes.Rmb:
                    convBlock = new DFBlock.RdbBlockDesc();
                    // convBlock = GetRdbBDForRmb(block);
                    break;
                case DFBlock.BlockTypes.Rdb:
                    convBlock = GetRdbBDForRdb(block);
                    break;
                default:
                    Debug.Log("GetRdbBlockDesc = Unknown block type");
                    break;
            }

            return convBlock;
        }

        public static DFBlockRotation.RdbBlockDesc GetRdbBlockDesc(DFBlockRotation.BlockTypes type, DFBlock.RdbBlockDesc block)
        {
            DFBlockRotation.RdbBlockDesc convBlock = new DFBlockRotation.RdbBlockDesc();

            switch (type)
            {
                case DFBlockRotation.BlockTypes.Rmb:
                    convBlock = new DFBlockRotation.RdbBlockDesc();
                    // convBlock = GetRdbBDForRmb(block);
                    break;
                case DFBlockRotation.BlockTypes.Rdb:
                    convBlock = GetRdbBDForRdb(block);
                    break;
                default:
                    Debug.Log("GetRdbBlockDesc = Unknown block type");
                    break;
            }

            return convBlock;
        }

        public static DFBlock.RmbBlockDesc GetRmbBDForRmb(DFBlockRotation.RmbBlockDesc block)
        {
            DFBlock.RmbBlockDesc convBlock = new DFBlock.RmbBlockDesc();

            convBlock.FldHeader.NumBlockDataRecords = block.FldHeader.NumBlockDataRecords;
            convBlock.FldHeader.NumMisc3dObjectRecords = block.FldHeader.NumMisc3dObjectRecords;
            convBlock.FldHeader.NumMiscFlatObjectRecords = block.FldHeader.NumMiscFlatObjectRecords;

            convBlock.FldHeader.BlockPositions = new DFBlock.RmbFldBlockPositions[block.FldHeader.BlockPositions.Length];
            for (int i = 0; i < block.FldHeader.BlockPositions.Length; i++)
            {
                convBlock.FldHeader.BlockPositions[i].XPos = block.FldHeader.BlockPositions[i].XPos;
                convBlock.FldHeader.BlockPositions[i].ZPos = block.FldHeader.BlockPositions[i].ZPos;
                convBlock.FldHeader.BlockPositions[i].YRotation = block.FldHeader.BlockPositions[i].YRotation;
            }

            convBlock.FldHeader.BuildingDataList = new DFLocation.BuildingData[block.FldHeader.BuildingDataList.Length];
            for (int j = 0; j < block.FldHeader.BuildingDataList.Length; j++)
            {
                convBlock.FldHeader.BuildingDataList[j].NameSeed = block.FldHeader.BuildingDataList[j].NameSeed;
                convBlock.FldHeader.BuildingDataList[j].FactionId = block.FldHeader.BuildingDataList[j].FactionId;
                convBlock.FldHeader.BuildingDataList[j].Sector = block.FldHeader.BuildingDataList[j].Sector;
                convBlock.FldHeader.BuildingDataList[j].LocationId = block.FldHeader.BuildingDataList[j].LocationId;
                convBlock.FldHeader.BuildingDataList[j].BuildingType = block.FldHeader.BuildingDataList[j].BuildingType;
                convBlock.FldHeader.BuildingDataList[j].Quality = block.FldHeader.BuildingDataList[j].Quality;
            }

            convBlock.FldHeader.BlockDataSizes = block.FldHeader.BlockDataSizes;
            
            convBlock.FldHeader.GroundData.Header = block.FldHeader.GroundData.Header;
            convBlock.FldHeader.GroundData.GroundTiles = null;
            convBlock.FldHeader.GroundData.GroundTiles1d = new DFBlock.RmbGroundTiles[16 * 16];
            for (int a = 0; a < block.FldHeader.GroundData.GroundTiles.Length; a++)
            {
                convBlock.FldHeader.GroundData.GroundTiles1d[a].TileBitfield = block.FldHeader.GroundData.GroundTiles[a].TileBitfield;
                convBlock.FldHeader.GroundData.GroundTiles1d[a].TextureRecord = block.FldHeader.GroundData.GroundTiles[a].TextureRecord;
                convBlock.FldHeader.GroundData.GroundTiles1d[a].IsRotated = block.FldHeader.GroundData.GroundTiles[a].IsRotated;
                convBlock.FldHeader.GroundData.GroundTiles1d[a].IsFlipped = block.FldHeader.GroundData.GroundTiles[a].IsFlipped;
            }
                
            convBlock.FldHeader.GroundData.GroundScenery = null;
            convBlock.FldHeader.GroundData.GroundScenery1d = new DFBlock.RmbGroundScenery[16 * 16];
            for (int b = 0; b < block.FldHeader.GroundData.GroundScenery.Length; b++)
            {
                convBlock.FldHeader.GroundData.GroundScenery1d[b].TextureRecord = block.FldHeader.GroundData.GroundScenery[b].TextureRecord;
            }

            convBlock.FldHeader.AutoMapData = block.FldHeader.AutoMapData;
            convBlock.FldHeader.Name = block.FldHeader.Name;
            convBlock.FldHeader.OtherNames = block.FldHeader.OtherNames;

            convBlock.SubRecords = new DFBlock.RmbSubRecord[block.SubRecords.Length];
            for (int k = 0; k < block.SubRecords.Length; k++)
            {
                convBlock.SubRecords[k].XPos = block.SubRecords[k].XPos;
                convBlock.SubRecords[k].ZPos = block.SubRecords[k].ZPos;
                convBlock.SubRecords[k].YRotation = block.SubRecords[k].YRotation;
                convBlock.SubRecords[k].Exterior.Header.Position = block.SubRecords[k].Exterior.Header.Position;
                convBlock.SubRecords[k].Exterior.Header.Num3dObjectRecords = block.SubRecords[k].Exterior.Header.Num3dObjectRecords;
                convBlock.SubRecords[k].Exterior.Header.NumFlatObjectRecords = block.SubRecords[k].Exterior.Header.NumFlatObjectRecords;
                convBlock.SubRecords[k].Exterior.Header.NumSection3Records = block.SubRecords[k].Exterior.Header.NumSection3Records;
                convBlock.SubRecords[k].Exterior.Header.NumPeopleRecords = block.SubRecords[k].Exterior.Header.NumPeopleRecords;
                convBlock.SubRecords[k].Exterior.Header.NumDoorRecords = block.SubRecords[k].Exterior.Header.NumDoorRecords;

                convBlock.SubRecords[k].Exterior.Block3dObjectRecords = new DFBlock.RmbBlock3dObjectRecord[block.SubRecords[k].Exterior.Block3dObjectRecords.Length];
                for (int l = 0; l < block.SubRecords[k].Exterior.Block3dObjectRecords.Length; l++)
                {
                    convBlock.SubRecords[k].Exterior.Block3dObjectRecords[l].ModelId = block.SubRecords[k].Exterior.Block3dObjectRecords[l].ModelId;
                    convBlock.SubRecords[k].Exterior.Block3dObjectRecords[l].ModelIdNum = block.SubRecords[k].Exterior.Block3dObjectRecords[l].ModelIdNum;
                    convBlock.SubRecords[k].Exterior.Block3dObjectRecords[l].ObjectType = block.SubRecords[k].Exterior.Block3dObjectRecords[l].ObjectType;
                    convBlock.SubRecords[k].Exterior.Block3dObjectRecords[l].XPos = block.SubRecords[k].Exterior.Block3dObjectRecords[l].XPos;
                    convBlock.SubRecords[k].Exterior.Block3dObjectRecords[l].YPos = block.SubRecords[k].Exterior.Block3dObjectRecords[l].YPos;
                    convBlock.SubRecords[k].Exterior.Block3dObjectRecords[l].ZPos = block.SubRecords[k].Exterior.Block3dObjectRecords[l].ZPos;
                    convBlock.SubRecords[k].Exterior.Block3dObjectRecords[l].XRotation = block.SubRecords[k].Exterior.Block3dObjectRecords[l].XRotation;
                    convBlock.SubRecords[k].Exterior.Block3dObjectRecords[l].YRotation = block.SubRecords[k].Exterior.Block3dObjectRecords[l].YRotation;
                    convBlock.SubRecords[k].Exterior.Block3dObjectRecords[l].ZRotation = block.SubRecords[k].Exterior.Block3dObjectRecords[l].ZRotation;
                }

                convBlock.SubRecords[k].Exterior.BlockFlatObjectRecords = new DFBlock.RmbBlockFlatObjectRecord[block.SubRecords[k].Exterior.BlockFlatObjectRecords.Length];
                for (int m = 0; m < block.SubRecords[k].Exterior.BlockFlatObjectRecords.Length; m++)
                {
                    convBlock.SubRecords[k].Exterior.BlockFlatObjectRecords[m].Position = block.SubRecords[k].Exterior.BlockFlatObjectRecords[m].Position;
                    convBlock.SubRecords[k].Exterior.BlockFlatObjectRecords[m].XPos = block.SubRecords[k].Exterior.BlockFlatObjectRecords[m].XPos;
                    convBlock.SubRecords[k].Exterior.BlockFlatObjectRecords[m].YPos = block.SubRecords[k].Exterior.BlockFlatObjectRecords[m].YPos;
                    convBlock.SubRecords[k].Exterior.BlockFlatObjectRecords[m].ZPos = block.SubRecords[k].Exterior.BlockFlatObjectRecords[m].ZPos;
                    convBlock.SubRecords[k].Exterior.BlockFlatObjectRecords[m].TextureArchive = block.SubRecords[k].Exterior.BlockFlatObjectRecords[m].TextureArchive;
                    convBlock.SubRecords[k].Exterior.BlockFlatObjectRecords[m].TextureRecord = block.SubRecords[k].Exterior.BlockFlatObjectRecords[m].TextureRecord;
                    convBlock.SubRecords[k].Exterior.BlockFlatObjectRecords[m].FactionID = block.SubRecords[k].Exterior.BlockFlatObjectRecords[m].FactionID;
                    convBlock.SubRecords[k].Exterior.BlockFlatObjectRecords[m].Flags = block.SubRecords[k].Exterior.BlockFlatObjectRecords[m].Flags;
                }

                convBlock.SubRecords[k].Exterior.BlockSection3Records = new DFBlock.RmbBlockSection3Record[block.SubRecords[k].Exterior.BlockSection3Records.Length];
                for (int n = 0; n < block.SubRecords[k].Exterior.BlockSection3Records.Length; n++)
                {
                    convBlock.SubRecords[k].Exterior.BlockSection3Records[n].XPos = block.SubRecords[k].Exterior.BlockSection3Records[n].XPos;
                    convBlock.SubRecords[k].Exterior.BlockSection3Records[n].YPos = block.SubRecords[k].Exterior.BlockSection3Records[n].YPos;
                    convBlock.SubRecords[k].Exterior.BlockSection3Records[n].ZPos = block.SubRecords[k].Exterior.BlockSection3Records[n].ZPos;
                }

                convBlock.SubRecords[k].Exterior.BlockPeopleRecords = new DFBlock.RmbBlockPeopleRecord[block.SubRecords[k].Exterior.BlockPeopleRecords.Length];
                for (int o = 0; o < block.SubRecords[k].Exterior.BlockPeopleRecords.Length; o++)
                {
                    convBlock.SubRecords[k].Exterior.BlockPeopleRecords[o].Position = block.SubRecords[k].Exterior.BlockPeopleRecords[o].Position;
                    convBlock.SubRecords[k].Exterior.BlockPeopleRecords[o].XPos = block.SubRecords[k].Exterior.BlockPeopleRecords[o].XPos;
                    convBlock.SubRecords[k].Exterior.BlockPeopleRecords[o].YPos = block.SubRecords[k].Exterior.BlockPeopleRecords[o].YPos;
                    convBlock.SubRecords[k].Exterior.BlockPeopleRecords[o].ZPos = block.SubRecords[k].Exterior.BlockPeopleRecords[o].ZPos;
                    convBlock.SubRecords[k].Exterior.BlockPeopleRecords[o].TextureArchive = block.SubRecords[k].Exterior.BlockPeopleRecords[o].TextureArchive;
                    convBlock.SubRecords[k].Exterior.BlockPeopleRecords[o].TextureRecord = block.SubRecords[k].Exterior.BlockPeopleRecords[o].TextureRecord;
                    convBlock.SubRecords[k].Exterior.BlockPeopleRecords[o].FactionID = block.SubRecords[k].Exterior.BlockPeopleRecords[o].FactionID;
                    convBlock.SubRecords[k].Exterior.BlockPeopleRecords[o].Flags = block.SubRecords[k].Exterior.BlockPeopleRecords[o].Flags;
                }

                convBlock.SubRecords[k].Exterior.BlockDoorRecords = new DFBlock.RmbBlockDoorRecord[block.SubRecords[k].Exterior.BlockDoorRecords.Length];
                for (int p = 0; p < block.SubRecords[k].Exterior.BlockDoorRecords.Length; p++)
                {
                    convBlock.SubRecords[k].Exterior.BlockDoorRecords[p].Position = block.SubRecords[k].Exterior.BlockDoorRecords[p].Position;
                    convBlock.SubRecords[k].Exterior.BlockDoorRecords[p].XPos = block.SubRecords[k].Exterior.BlockDoorRecords[p].XPos;
                    convBlock.SubRecords[k].Exterior.BlockDoorRecords[p].YPos = block.SubRecords[k].Exterior.BlockDoorRecords[p].YPos;
                    convBlock.SubRecords[k].Exterior.BlockDoorRecords[p].ZPos = block.SubRecords[k].Exterior.BlockDoorRecords[p].ZPos;
                    convBlock.SubRecords[k].Exterior.BlockDoorRecords[p].YRotation = block.SubRecords[k].Exterior.BlockDoorRecords[p].YRotation;
                    convBlock.SubRecords[k].Exterior.BlockDoorRecords[p].OpenRotation = block.SubRecords[k].Exterior.BlockDoorRecords[p].OpenRotation;
                    convBlock.SubRecords[k].Exterior.BlockDoorRecords[p].DoorModelIndex = block.SubRecords[k].Exterior.BlockDoorRecords[p].DoorModelIndex;
                }

                convBlock.SubRecords[k].Interior.Header.Position = block.SubRecords[k].Interior.Header.Position;
                convBlock.SubRecords[k].Interior.Header.Num3dObjectRecords = block.SubRecords[k].Interior.Header.Num3dObjectRecords;
                convBlock.SubRecords[k].Interior.Header.NumFlatObjectRecords = block.SubRecords[k].Interior.Header.NumFlatObjectRecords;
                convBlock.SubRecords[k].Interior.Header.NumSection3Records = block.SubRecords[k].Interior.Header.NumSection3Records;
                convBlock.SubRecords[k].Interior.Header.NumPeopleRecords = block.SubRecords[k].Interior.Header.NumPeopleRecords;
                convBlock.SubRecords[k].Interior.Header.NumDoorRecords = block.SubRecords[k].Interior.Header.NumDoorRecords;

                convBlock.SubRecords[k].Interior.Block3dObjectRecords = new DFBlock.RmbBlock3dObjectRecord[block.SubRecords[k].Interior.Block3dObjectRecords.Length];
                for (int q = 0; q < block.SubRecords[k].Interior.Block3dObjectRecords.Length; q++)
                {
                    convBlock.SubRecords[k].Interior.Block3dObjectRecords[q].ModelId = block.SubRecords[k].Interior.Block3dObjectRecords[q].ModelId;
                    convBlock.SubRecords[k].Interior.Block3dObjectRecords[q].ModelIdNum = block.SubRecords[k].Interior.Block3dObjectRecords[q].ModelIdNum;
                    convBlock.SubRecords[k].Interior.Block3dObjectRecords[q].ObjectType = block.SubRecords[k].Interior.Block3dObjectRecords[q].ObjectType;
                    convBlock.SubRecords[k].Interior.Block3dObjectRecords[q].XPos = block.SubRecords[k].Interior.Block3dObjectRecords[q].XPos;
                    convBlock.SubRecords[k].Interior.Block3dObjectRecords[q].YPos = block.SubRecords[k].Interior.Block3dObjectRecords[q].YPos;
                    convBlock.SubRecords[k].Interior.Block3dObjectRecords[q].ZPos = block.SubRecords[k].Interior.Block3dObjectRecords[q].ZPos;
                    convBlock.SubRecords[k].Interior.Block3dObjectRecords[q].XRotation = block.SubRecords[k].Interior.Block3dObjectRecords[q].XRotation;
                    convBlock.SubRecords[k].Interior.Block3dObjectRecords[q].YRotation = block.SubRecords[k].Interior.Block3dObjectRecords[q].YRotation;
                    convBlock.SubRecords[k].Interior.Block3dObjectRecords[q].ZRotation = block.SubRecords[k].Interior.Block3dObjectRecords[q].ZRotation;
                }

                convBlock.SubRecords[k].Interior.BlockFlatObjectRecords = new DFBlock.RmbBlockFlatObjectRecord[block.SubRecords[k].Interior.BlockFlatObjectRecords.Length];
                for (int r = 0; r < block.SubRecords[k].Interior.BlockFlatObjectRecords.Length; r++)
                {
                    convBlock.SubRecords[k].Interior.BlockFlatObjectRecords[r].Position = block.SubRecords[k].Interior.BlockFlatObjectRecords[r].Position;
                    convBlock.SubRecords[k].Interior.BlockFlatObjectRecords[r].XPos = block.SubRecords[k].Interior.BlockFlatObjectRecords[r].XPos;
                    convBlock.SubRecords[k].Interior.BlockFlatObjectRecords[r].YPos = block.SubRecords[k].Interior.BlockFlatObjectRecords[r].YPos;
                    convBlock.SubRecords[k].Interior.BlockFlatObjectRecords[r].ZPos = block.SubRecords[k].Interior.BlockFlatObjectRecords[r].ZPos;
                    convBlock.SubRecords[k].Interior.BlockFlatObjectRecords[r].TextureArchive = block.SubRecords[k].Interior.BlockFlatObjectRecords[r].TextureArchive;
                    convBlock.SubRecords[k].Interior.BlockFlatObjectRecords[r].TextureRecord = block.SubRecords[k].Interior.BlockFlatObjectRecords[r].TextureRecord;
                    convBlock.SubRecords[k].Interior.BlockFlatObjectRecords[r].FactionID = block.SubRecords[k].Interior.BlockFlatObjectRecords[r].FactionID;
                    convBlock.SubRecords[k].Interior.BlockFlatObjectRecords[r].Flags = block.SubRecords[k].Interior.BlockFlatObjectRecords[r].Flags;
                }

                convBlock.SubRecords[k].Interior.BlockSection3Records = new DFBlock.RmbBlockSection3Record[block.SubRecords[k].Interior.BlockSection3Records.Length];
                for (int s = 0; s < block.SubRecords[k].Interior.BlockSection3Records.Length; s++)
                {
                    convBlock.SubRecords[k].Interior.BlockSection3Records[s].XPos = block.SubRecords[k].Interior.BlockSection3Records[s].XPos;
                    convBlock.SubRecords[k].Interior.BlockSection3Records[s].YPos = block.SubRecords[k].Interior.BlockSection3Records[s].YPos;
                    convBlock.SubRecords[k].Interior.BlockSection3Records[s].ZPos = block.SubRecords[k].Interior.BlockSection3Records[s].ZPos;
                }

                convBlock.SubRecords[k].Interior.BlockPeopleRecords = new DFBlock.RmbBlockPeopleRecord[block.SubRecords[k].Interior.BlockPeopleRecords.Length];
                for (int t = 0; t < block.SubRecords[k].Interior.BlockPeopleRecords.Length; t++)
                {
                    convBlock.SubRecords[k].Interior.BlockPeopleRecords[t].Position = block.SubRecords[k].Interior.BlockPeopleRecords[t].Position;
                    convBlock.SubRecords[k].Interior.BlockPeopleRecords[t].XPos = block.SubRecords[k].Interior.BlockPeopleRecords[t].XPos;
                    convBlock.SubRecords[k].Interior.BlockPeopleRecords[t].YPos = block.SubRecords[k].Interior.BlockPeopleRecords[t].YPos;
                    convBlock.SubRecords[k].Interior.BlockPeopleRecords[t].ZPos = block.SubRecords[k].Interior.BlockPeopleRecords[t].ZPos;
                    convBlock.SubRecords[k].Interior.BlockPeopleRecords[t].TextureArchive = block.SubRecords[k].Interior.BlockPeopleRecords[t].TextureArchive;
                    convBlock.SubRecords[k].Interior.BlockPeopleRecords[t].TextureRecord = block.SubRecords[k].Interior.BlockPeopleRecords[t].TextureRecord;
                    convBlock.SubRecords[k].Interior.BlockPeopleRecords[t].FactionID = block.SubRecords[k].Interior.BlockPeopleRecords[t].FactionID;
                    convBlock.SubRecords[k].Interior.BlockPeopleRecords[t].Flags = block.SubRecords[k].Interior.BlockPeopleRecords[t].Flags;
                }

                convBlock.SubRecords[k].Interior.BlockDoorRecords = new DFBlock.RmbBlockDoorRecord[block.SubRecords[k].Interior.BlockDoorRecords.Length];
                for (int u = 0; u < block.SubRecords[k].Interior.BlockDoorRecords.Length; u++)
                {
                    convBlock.SubRecords[k].Interior.BlockDoorRecords[u].Position = block.SubRecords[k].Interior.BlockDoorRecords[u].Position;
                    convBlock.SubRecords[k].Interior.BlockDoorRecords[u].XPos = block.SubRecords[k].Interior.BlockDoorRecords[u].XPos;
                    convBlock.SubRecords[k].Interior.BlockDoorRecords[u].YPos = block.SubRecords[k].Interior.BlockDoorRecords[u].YPos;
                    convBlock.SubRecords[k].Interior.BlockDoorRecords[u].ZPos = block.SubRecords[k].Interior.BlockDoorRecords[u].ZPos;
                    convBlock.SubRecords[k].Interior.BlockDoorRecords[u].YRotation = block.SubRecords[k].Interior.BlockDoorRecords[u].YRotation;
                    convBlock.SubRecords[k].Interior.BlockDoorRecords[u].OpenRotation = block.SubRecords[k].Interior.BlockDoorRecords[u].OpenRotation;
                    convBlock.SubRecords[k].Interior.BlockDoorRecords[u].DoorModelIndex = block.SubRecords[k].Interior.BlockDoorRecords[u].DoorModelIndex;
                }
            }

            convBlock.Misc3dObjectRecords = new DFBlock.RmbBlock3dObjectRecord[block.Misc3dObjectRecords.Length];
            for (int v = 0; v < block.Misc3dObjectRecords.Length; v++)
            {
                convBlock.Misc3dObjectRecords[v].ModelId = block.Misc3dObjectRecords[v].ModelId;
                convBlock.Misc3dObjectRecords[v].ModelIdNum = block.Misc3dObjectRecords[v].ModelIdNum;
                convBlock.Misc3dObjectRecords[v].ObjectType = block.Misc3dObjectRecords[v].ObjectType;
                convBlock.Misc3dObjectRecords[v].XPos = block.Misc3dObjectRecords[v].XPos;
                convBlock.Misc3dObjectRecords[v].YPos = block.Misc3dObjectRecords[v].YPos;
                convBlock.Misc3dObjectRecords[v].ZPos = block.Misc3dObjectRecords[v].ZPos;
                convBlock.Misc3dObjectRecords[v].XRotation = block.Misc3dObjectRecords[v].XRotation;
                convBlock.Misc3dObjectRecords[v].YRotation = block.Misc3dObjectRecords[v].YRotation;
                convBlock.Misc3dObjectRecords[v].ZRotation = block.Misc3dObjectRecords[v].ZRotation;
            }

            convBlock.MiscFlatObjectRecords = new DFBlock.RmbBlockFlatObjectRecord[block.MiscFlatObjectRecords.Length];
            for (int w = 0; w < block.MiscFlatObjectRecords.Length; w++)
            {
                convBlock.MiscFlatObjectRecords[w].Position = block.MiscFlatObjectRecords[w].Position;
                convBlock.MiscFlatObjectRecords[w].XPos = block.MiscFlatObjectRecords[w].XPos;
                convBlock.MiscFlatObjectRecords[w].YPos = block.MiscFlatObjectRecords[w].YPos;
                convBlock.MiscFlatObjectRecords[w].ZPos = block.MiscFlatObjectRecords[w].ZPos;
                convBlock.MiscFlatObjectRecords[w].TextureArchive = block.MiscFlatObjectRecords[w].TextureArchive;
                convBlock.MiscFlatObjectRecords[w].TextureRecord = block.MiscFlatObjectRecords[w].TextureRecord;
                convBlock.MiscFlatObjectRecords[w].FactionID = block.MiscFlatObjectRecords[w].FactionID;
                convBlock.MiscFlatObjectRecords[w].Flags = block.MiscFlatObjectRecords[w].Flags;
            }

            return convBlock;
        }

        public static DFBlockRotation.RmbBlockDesc GetRmbBDForRmb(DFBlock.RmbBlockDesc block)
        {
            DFBlockRotation.RmbBlockDesc convBlock = new DFBlockRotation.RmbBlockDesc();

            convBlock.FldHeader.NumBlockDataRecords = block.FldHeader.NumBlockDataRecords;
            convBlock.FldHeader.NumMisc3dObjectRecords = block.FldHeader.NumMisc3dObjectRecords;
            convBlock.FldHeader.NumMiscFlatObjectRecords = block.FldHeader.NumMiscFlatObjectRecords;

            convBlock.FldHeader.BlockPositions = new DFBlockRotation.RmbFldBlockPositions[block.FldHeader.BlockPositions.Length];
            for (int i = 0; i < block.FldHeader.BlockPositions.Length; i++)
            {
                convBlock.FldHeader.BlockPositions[i].XPos = block.FldHeader.BlockPositions[i].XPos;
                convBlock.FldHeader.BlockPositions[i].ZPos = block.FldHeader.BlockPositions[i].ZPos;
                convBlock.FldHeader.BlockPositions[i].YRotation = block.FldHeader.BlockPositions[i].YRotation;
            }

            convBlock.FldHeader.BuildingDataList = new DFLocation.BuildingData[block.FldHeader.BuildingDataList.Length];
            for (int j = 0; j < block.FldHeader.BuildingDataList.Length; j++)
            {
                convBlock.FldHeader.BuildingDataList[j].NameSeed = block.FldHeader.BuildingDataList[j].NameSeed;
                if (merchantBuilding.Contains(block.FldHeader.BuildingDataList[j].BuildingType))
                {
                    convBlock.FldHeader.BuildingDataList[j].FactionId = 510;
                }
                else convBlock.FldHeader.BuildingDataList[j].FactionId = block.FldHeader.BuildingDataList[j].FactionId;
                convBlock.FldHeader.BuildingDataList[j].Sector = block.FldHeader.BuildingDataList[j].Sector;
                convBlock.FldHeader.BuildingDataList[j].LocationId = block.FldHeader.BuildingDataList[j].LocationId;
                convBlock.FldHeader.BuildingDataList[j].BuildingType = block.FldHeader.BuildingDataList[j].BuildingType;
                convBlock.FldHeader.BuildingDataList[j].Quality = block.FldHeader.BuildingDataList[j].Quality;
            }

            convBlock.FldHeader.BlockDataSizes = block.FldHeader.BlockDataSizes;

            convBlock.FldHeader.GroundData.Header = block.FldHeader.GroundData.Header;
            convBlock.FldHeader.GroundData.GroundTiles = new DFBlockRotation.RmbGroundTiles[16 * 16];
            for (int a = 0; a < block.FldHeader.GroundData.GroundTiles1d.Length; a++)
            {
                convBlock.FldHeader.GroundData.GroundTiles[a].TileBitfield = block.FldHeader.GroundData.GroundTiles1d[a].TileBitfield;
                convBlock.FldHeader.GroundData.GroundTiles[a].TextureRecord = block.FldHeader.GroundData.GroundTiles1d[a].TextureRecord;
                convBlock.FldHeader.GroundData.GroundTiles[a].IsRotated = block.FldHeader.GroundData.GroundTiles1d[a].IsRotated;
                convBlock.FldHeader.GroundData.GroundTiles[a].IsFlipped = block.FldHeader.GroundData.GroundTiles1d[a].IsFlipped;
            }
                
            convBlock.FldHeader.GroundData.GroundScenery = new DFBlockRotation.RmbGroundScenery[16 * 16];
            for (int b = 0; b < block.FldHeader.GroundData.GroundScenery1d.Length; b++)
            {
                convBlock.FldHeader.GroundData.GroundScenery[b].TextureRecord = block.FldHeader.GroundData.GroundScenery1d[b].TextureRecord;
            }

            convBlock.FldHeader.AutoMapData = block.FldHeader.AutoMapData;
            convBlock.FldHeader.Name = block.FldHeader.Name;
            convBlock.FldHeader.OtherNames = block.FldHeader.OtherNames;

            convBlock.SubRecords = new DFBlockRotation.RmbSubRecord[block.SubRecords.Length];
            for (int k = 0; k < block.SubRecords.Length; k++)
            {
                convBlock.SubRecords[k].XPos = block.SubRecords[k].XPos;
                convBlock.SubRecords[k].ZPos = block.SubRecords[k].ZPos;
                convBlock.SubRecords[k].YRotation = block.SubRecords[k].YRotation;
                convBlock.SubRecords[k].Exterior.Header.Position = block.SubRecords[k].Exterior.Header.Position;
                convBlock.SubRecords[k].Exterior.Header.Num3dObjectRecords = block.SubRecords[k].Exterior.Header.Num3dObjectRecords;
                convBlock.SubRecords[k].Exterior.Header.NumFlatObjectRecords = block.SubRecords[k].Exterior.Header.NumFlatObjectRecords;
                convBlock.SubRecords[k].Exterior.Header.NumSection3Records = block.SubRecords[k].Exterior.Header.NumSection3Records;
                convBlock.SubRecords[k].Exterior.Header.NumPeopleRecords = block.SubRecords[k].Exterior.Header.NumPeopleRecords;
                convBlock.SubRecords[k].Exterior.Header.NumDoorRecords = block.SubRecords[k].Exterior.Header.NumDoorRecords;

                convBlock.SubRecords[k].Exterior.Block3dObjectRecords = new DFBlockRotation.RmbBlock3dObjectRecord[block.SubRecords[k].Exterior.Block3dObjectRecords.Length];
                for (int l = 0; l < block.SubRecords[k].Exterior.Block3dObjectRecords.Length; l++)
                {
                    convBlock.SubRecords[k].Exterior.Block3dObjectRecords[l].ModelId = block.SubRecords[k].Exterior.Block3dObjectRecords[l].ModelId;
                    convBlock.SubRecords[k].Exterior.Block3dObjectRecords[l].ModelIdNum = block.SubRecords[k].Exterior.Block3dObjectRecords[l].ModelIdNum;
                    convBlock.SubRecords[k].Exterior.Block3dObjectRecords[l].ObjectType = block.SubRecords[k].Exterior.Block3dObjectRecords[l].ObjectType;
                    convBlock.SubRecords[k].Exterior.Block3dObjectRecords[l].XPos = block.SubRecords[k].Exterior.Block3dObjectRecords[l].XPos;
                    convBlock.SubRecords[k].Exterior.Block3dObjectRecords[l].YPos = block.SubRecords[k].Exterior.Block3dObjectRecords[l].YPos;
                    convBlock.SubRecords[k].Exterior.Block3dObjectRecords[l].ZPos = block.SubRecords[k].Exterior.Block3dObjectRecords[l].ZPos;
                    convBlock.SubRecords[k].Exterior.Block3dObjectRecords[l].XRotation = block.SubRecords[k].Exterior.Block3dObjectRecords[l].XRotation;
                    convBlock.SubRecords[k].Exterior.Block3dObjectRecords[l].YRotation = block.SubRecords[k].Exterior.Block3dObjectRecords[l].YRotation;
                    convBlock.SubRecords[k].Exterior.Block3dObjectRecords[l].ZRotation = block.SubRecords[k].Exterior.Block3dObjectRecords[l].ZRotation;
                }

                convBlock.SubRecords[k].Exterior.BlockFlatObjectRecords = new DFBlockRotation.RmbBlockFlatObjectRecord[block.SubRecords[k].Exterior.BlockFlatObjectRecords.Length];
                for (int m = 0; m < block.SubRecords[k].Exterior.BlockFlatObjectRecords.Length; m++)
                {
                    convBlock.SubRecords[k].Exterior.BlockFlatObjectRecords[m].Position = block.SubRecords[k].Exterior.BlockFlatObjectRecords[m].Position;
                    convBlock.SubRecords[k].Exterior.BlockFlatObjectRecords[m].XPos = block.SubRecords[k].Exterior.BlockFlatObjectRecords[m].XPos;
                    convBlock.SubRecords[k].Exterior.BlockFlatObjectRecords[m].YPos = block.SubRecords[k].Exterior.BlockFlatObjectRecords[m].YPos;
                    convBlock.SubRecords[k].Exterior.BlockFlatObjectRecords[m].ZPos = block.SubRecords[k].Exterior.BlockFlatObjectRecords[m].ZPos;
                    convBlock.SubRecords[k].Exterior.BlockFlatObjectRecords[m].TextureArchive = block.SubRecords[k].Exterior.BlockFlatObjectRecords[m].TextureArchive;
                    convBlock.SubRecords[k].Exterior.BlockFlatObjectRecords[m].TextureRecord = block.SubRecords[k].Exterior.BlockFlatObjectRecords[m].TextureRecord;
                    convBlock.SubRecords[k].Exterior.BlockFlatObjectRecords[m].FactionID = block.SubRecords[k].Exterior.BlockFlatObjectRecords[m].FactionID;
                    convBlock.SubRecords[k].Exterior.BlockFlatObjectRecords[m].Flags = block.SubRecords[k].Exterior.BlockFlatObjectRecords[m].Flags;
                }

                convBlock.SubRecords[k].Exterior.BlockSection3Records = new DFBlockRotation.RmbBlockSection3Record[block.SubRecords[k].Exterior.BlockSection3Records.Length];
                for (int n = 0; n < block.SubRecords[k].Exterior.BlockSection3Records.Length; n++)
                {
                    convBlock.SubRecords[k].Exterior.BlockSection3Records[n].XPos = block.SubRecords[k].Exterior.BlockSection3Records[n].XPos;
                    convBlock.SubRecords[k].Exterior.BlockSection3Records[n].YPos = block.SubRecords[k].Exterior.BlockSection3Records[n].YPos;
                    convBlock.SubRecords[k].Exterior.BlockSection3Records[n].ZPos = block.SubRecords[k].Exterior.BlockSection3Records[n].ZPos;
                }

                convBlock.SubRecords[k].Exterior.BlockPeopleRecords = new DFBlockRotation.RmbBlockPeopleRecord[block.SubRecords[k].Exterior.BlockPeopleRecords.Length];
                for (int o = 0; o < block.SubRecords[k].Exterior.BlockPeopleRecords.Length; o++)
                {
                    convBlock.SubRecords[k].Exterior.BlockPeopleRecords[o].Position = block.SubRecords[k].Exterior.BlockPeopleRecords[o].Position;
                    convBlock.SubRecords[k].Exterior.BlockPeopleRecords[o].XPos = block.SubRecords[k].Exterior.BlockPeopleRecords[o].XPos;
                    convBlock.SubRecords[k].Exterior.BlockPeopleRecords[o].YPos = block.SubRecords[k].Exterior.BlockPeopleRecords[o].YPos;
                    convBlock.SubRecords[k].Exterior.BlockPeopleRecords[o].ZPos = block.SubRecords[k].Exterior.BlockPeopleRecords[o].ZPos;
                    convBlock.SubRecords[k].Exterior.BlockPeopleRecords[o].TextureArchive = block.SubRecords[k].Exterior.BlockPeopleRecords[o].TextureArchive;
                    convBlock.SubRecords[k].Exterior.BlockPeopleRecords[o].TextureRecord = block.SubRecords[k].Exterior.BlockPeopleRecords[o].TextureRecord;
                    convBlock.SubRecords[k].Exterior.BlockPeopleRecords[o].FactionID = block.SubRecords[k].Exterior.BlockPeopleRecords[o].FactionID;
                    convBlock.SubRecords[k].Exterior.BlockPeopleRecords[o].Flags = block.SubRecords[k].Exterior.BlockPeopleRecords[o].Flags;
                }

                convBlock.SubRecords[k].Exterior.BlockDoorRecords = new DFBlockRotation.RmbBlockDoorRecord[block.SubRecords[k].Exterior.BlockDoorRecords.Length];
                for (int p = 0; p < block.SubRecords[k].Exterior.BlockDoorRecords.Length; p++)
                {
                    convBlock.SubRecords[k].Exterior.BlockDoorRecords[p].Position = block.SubRecords[k].Exterior.BlockDoorRecords[p].Position;
                    convBlock.SubRecords[k].Exterior.BlockDoorRecords[p].XPos = block.SubRecords[k].Exterior.BlockDoorRecords[p].XPos;
                    convBlock.SubRecords[k].Exterior.BlockDoorRecords[p].YPos = block.SubRecords[k].Exterior.BlockDoorRecords[p].YPos;
                    convBlock.SubRecords[k].Exterior.BlockDoorRecords[p].ZPos = block.SubRecords[k].Exterior.BlockDoorRecords[p].ZPos;
                    convBlock.SubRecords[k].Exterior.BlockDoorRecords[p].YRotation = block.SubRecords[k].Exterior.BlockDoorRecords[p].YRotation;
                    convBlock.SubRecords[k].Exterior.BlockDoorRecords[p].OpenRotation = block.SubRecords[k].Exterior.BlockDoorRecords[p].OpenRotation;
                    convBlock.SubRecords[k].Exterior.BlockDoorRecords[p].DoorModelIndex = block.SubRecords[k].Exterior.BlockDoorRecords[p].DoorModelIndex;
                }

                convBlock.SubRecords[k].Interior.Header.Position = block.SubRecords[k].Interior.Header.Position;
                convBlock.SubRecords[k].Interior.Header.Num3dObjectRecords = block.SubRecords[k].Interior.Header.Num3dObjectRecords;
                convBlock.SubRecords[k].Interior.Header.NumFlatObjectRecords = block.SubRecords[k].Interior.Header.NumFlatObjectRecords;
                convBlock.SubRecords[k].Interior.Header.NumSection3Records = block.SubRecords[k].Interior.Header.NumSection3Records;
                convBlock.SubRecords[k].Interior.Header.NumPeopleRecords = block.SubRecords[k].Interior.Header.NumPeopleRecords;
                convBlock.SubRecords[k].Interior.Header.NumDoorRecords = block.SubRecords[k].Interior.Header.NumDoorRecords;

                convBlock.SubRecords[k].Interior.Block3dObjectRecords = new DFBlockRotation.RmbBlock3dObjectRecord[block.SubRecords[k].Interior.Block3dObjectRecords.Length];
                for (int q = 0; q < block.SubRecords[k].Interior.Block3dObjectRecords.Length; q++)
                {
                    convBlock.SubRecords[k].Interior.Block3dObjectRecords[q].ModelId = block.SubRecords[k].Interior.Block3dObjectRecords[q].ModelId;
                    convBlock.SubRecords[k].Interior.Block3dObjectRecords[q].ModelIdNum = block.SubRecords[k].Interior.Block3dObjectRecords[q].ModelIdNum;
                    convBlock.SubRecords[k].Interior.Block3dObjectRecords[q].ObjectType = block.SubRecords[k].Interior.Block3dObjectRecords[q].ObjectType;
                    convBlock.SubRecords[k].Interior.Block3dObjectRecords[q].XPos = block.SubRecords[k].Interior.Block3dObjectRecords[q].XPos;
                    convBlock.SubRecords[k].Interior.Block3dObjectRecords[q].YPos = block.SubRecords[k].Interior.Block3dObjectRecords[q].YPos;
                    convBlock.SubRecords[k].Interior.Block3dObjectRecords[q].ZPos = block.SubRecords[k].Interior.Block3dObjectRecords[q].ZPos;
                    convBlock.SubRecords[k].Interior.Block3dObjectRecords[q].XRotation = block.SubRecords[k].Interior.Block3dObjectRecords[q].XRotation;
                    convBlock.SubRecords[k].Interior.Block3dObjectRecords[q].YRotation = block.SubRecords[k].Interior.Block3dObjectRecords[q].YRotation;
                    convBlock.SubRecords[k].Interior.Block3dObjectRecords[q].ZRotation = block.SubRecords[k].Interior.Block3dObjectRecords[q].ZRotation;
                }

                convBlock.SubRecords[k].Interior.BlockFlatObjectRecords = new DFBlockRotation.RmbBlockFlatObjectRecord[block.SubRecords[k].Interior.BlockFlatObjectRecords.Length];
                for (int r = 0; r < block.SubRecords[k].Interior.BlockFlatObjectRecords.Length; r++)
                {
                    convBlock.SubRecords[k].Interior.BlockFlatObjectRecords[r].Position = block.SubRecords[k].Interior.BlockFlatObjectRecords[r].Position;
                    convBlock.SubRecords[k].Interior.BlockFlatObjectRecords[r].XPos = block.SubRecords[k].Interior.BlockFlatObjectRecords[r].XPos;
                    convBlock.SubRecords[k].Interior.BlockFlatObjectRecords[r].YPos = block.SubRecords[k].Interior.BlockFlatObjectRecords[r].YPos;
                    convBlock.SubRecords[k].Interior.BlockFlatObjectRecords[r].ZPos = block.SubRecords[k].Interior.BlockFlatObjectRecords[r].ZPos;
                    convBlock.SubRecords[k].Interior.BlockFlatObjectRecords[r].TextureArchive = block.SubRecords[k].Interior.BlockFlatObjectRecords[r].TextureArchive;
                    convBlock.SubRecords[k].Interior.BlockFlatObjectRecords[r].TextureRecord = block.SubRecords[k].Interior.BlockFlatObjectRecords[r].TextureRecord;
                    convBlock.SubRecords[k].Interior.BlockFlatObjectRecords[r].FactionID = block.SubRecords[k].Interior.BlockFlatObjectRecords[r].FactionID;
                    convBlock.SubRecords[k].Interior.BlockFlatObjectRecords[r].Flags = block.SubRecords[k].Interior.BlockFlatObjectRecords[r].Flags;
                }

                convBlock.SubRecords[k].Interior.BlockSection3Records = new DFBlockRotation.RmbBlockSection3Record[block.SubRecords[k].Interior.BlockSection3Records.Length];
                for (int s = 0; s < block.SubRecords[k].Interior.BlockSection3Records.Length; s++)
                {
                    convBlock.SubRecords[k].Interior.BlockSection3Records[s].XPos = block.SubRecords[k].Interior.BlockSection3Records[s].XPos;
                    convBlock.SubRecords[k].Interior.BlockSection3Records[s].YPos = block.SubRecords[k].Interior.BlockSection3Records[s].YPos;
                    convBlock.SubRecords[k].Interior.BlockSection3Records[s].ZPos = block.SubRecords[k].Interior.BlockSection3Records[s].ZPos;
                }

                convBlock.SubRecords[k].Interior.BlockPeopleRecords = new DFBlockRotation.RmbBlockPeopleRecord[block.SubRecords[k].Interior.BlockPeopleRecords.Length];
                for (int t = 0; t < block.SubRecords[k].Interior.BlockPeopleRecords.Length; t++)
                {
                    convBlock.SubRecords[k].Interior.BlockPeopleRecords[t].Position = block.SubRecords[k].Interior.BlockPeopleRecords[t].Position;
                    convBlock.SubRecords[k].Interior.BlockPeopleRecords[t].XPos = block.SubRecords[k].Interior.BlockPeopleRecords[t].XPos;
                    convBlock.SubRecords[k].Interior.BlockPeopleRecords[t].YPos = block.SubRecords[k].Interior.BlockPeopleRecords[t].YPos;
                    convBlock.SubRecords[k].Interior.BlockPeopleRecords[t].ZPos = block.SubRecords[k].Interior.BlockPeopleRecords[t].ZPos;
                    convBlock.SubRecords[k].Interior.BlockPeopleRecords[t].TextureArchive = block.SubRecords[k].Interior.BlockPeopleRecords[t].TextureArchive;
                    convBlock.SubRecords[k].Interior.BlockPeopleRecords[t].TextureRecord = block.SubRecords[k].Interior.BlockPeopleRecords[t].TextureRecord;
                    convBlock.SubRecords[k].Interior.BlockPeopleRecords[t].FactionID = block.SubRecords[k].Interior.BlockPeopleRecords[t].FactionID;
                    convBlock.SubRecords[k].Interior.BlockPeopleRecords[t].Flags = block.SubRecords[k].Interior.BlockPeopleRecords[t].Flags;
                }

                convBlock.SubRecords[k].Interior.BlockDoorRecords = new DFBlockRotation.RmbBlockDoorRecord[block.SubRecords[k].Interior.BlockDoorRecords.Length];
                for (int u = 0; u < block.SubRecords[k].Interior.BlockDoorRecords.Length; u++)
                {
                    convBlock.SubRecords[k].Interior.BlockDoorRecords[u].Position = block.SubRecords[k].Interior.BlockDoorRecords[u].Position;
                    convBlock.SubRecords[k].Interior.BlockDoorRecords[u].XPos = block.SubRecords[k].Interior.BlockDoorRecords[u].XPos;
                    convBlock.SubRecords[k].Interior.BlockDoorRecords[u].YPos = block.SubRecords[k].Interior.BlockDoorRecords[u].YPos;
                    convBlock.SubRecords[k].Interior.BlockDoorRecords[u].ZPos = block.SubRecords[k].Interior.BlockDoorRecords[u].ZPos;
                    convBlock.SubRecords[k].Interior.BlockDoorRecords[u].YRotation = block.SubRecords[k].Interior.BlockDoorRecords[u].YRotation;
                    convBlock.SubRecords[k].Interior.BlockDoorRecords[u].OpenRotation = block.SubRecords[k].Interior.BlockDoorRecords[u].OpenRotation;
                    convBlock.SubRecords[k].Interior.BlockDoorRecords[u].DoorModelIndex = block.SubRecords[k].Interior.BlockDoorRecords[u].DoorModelIndex;
                }
            }

            convBlock.Misc3dObjectRecords = new DFBlockRotation.RmbBlock3dObjectRecord[block.Misc3dObjectRecords.Length];
            for (int v = 0; v < block.Misc3dObjectRecords.Length; v++)
            {
                convBlock.Misc3dObjectRecords[v].ModelId = block.Misc3dObjectRecords[v].ModelId;
                convBlock.Misc3dObjectRecords[v].ModelIdNum = block.Misc3dObjectRecords[v].ModelIdNum;
                convBlock.Misc3dObjectRecords[v].ObjectType = block.Misc3dObjectRecords[v].ObjectType;
                convBlock.Misc3dObjectRecords[v].XPos = block.Misc3dObjectRecords[v].XPos;
                convBlock.Misc3dObjectRecords[v].YPos = block.Misc3dObjectRecords[v].YPos;
                convBlock.Misc3dObjectRecords[v].ZPos = block.Misc3dObjectRecords[v].ZPos;
                convBlock.Misc3dObjectRecords[v].XRotation = block.Misc3dObjectRecords[v].XRotation;
                convBlock.Misc3dObjectRecords[v].YRotation = block.Misc3dObjectRecords[v].YRotation;
                convBlock.Misc3dObjectRecords[v].ZRotation = block.Misc3dObjectRecords[v].ZRotation;
            }

            convBlock.MiscFlatObjectRecords = new DFBlockRotation.RmbBlockFlatObjectRecord[block.MiscFlatObjectRecords.Length];
            for (int w = 0; w < block.MiscFlatObjectRecords.Length; w++)
            {
                convBlock.MiscFlatObjectRecords[w].Position = block.MiscFlatObjectRecords[w].Position;
                convBlock.MiscFlatObjectRecords[w].XPos = block.MiscFlatObjectRecords[w].XPos;
                convBlock.MiscFlatObjectRecords[w].YPos = block.MiscFlatObjectRecords[w].YPos;
                convBlock.MiscFlatObjectRecords[w].ZPos = block.MiscFlatObjectRecords[w].ZPos;
                convBlock.MiscFlatObjectRecords[w].TextureArchive = block.MiscFlatObjectRecords[w].TextureArchive;
                convBlock.MiscFlatObjectRecords[w].TextureRecord = block.MiscFlatObjectRecords[w].TextureRecord;
                convBlock.MiscFlatObjectRecords[w].FactionID = block.MiscFlatObjectRecords[w].FactionID;
                convBlock.MiscFlatObjectRecords[w].Flags = block.MiscFlatObjectRecords[w].Flags;
            }

            return convBlock;
        }
        
        public static DFBlock.RdbBlockDesc GetRdbBDForRdb(DFBlockRotation.RdbBlockDesc block)
        {
            DFBlock.RdbBlockDesc convBlock = new DFBlock.RdbBlockDesc();

            convBlock.ModelReferenceList = new DFBlock.RdbModelReference[block.ModelReferenceList.Length];
            for (int i = 0; i < block.ModelReferenceList.Length; i++)
            {
                convBlock.ModelReferenceList[i].ModelId = block.ModelReferenceList[i].ModelId;
                convBlock.ModelReferenceList[i].ModelIdNum = block.ModelReferenceList[i].ModelIdNum;
                convBlock.ModelReferenceList[i].Description = block.ModelReferenceList[i].Description;
            }

            convBlock.ObjectRootList = new DFBlock.RdbObjectRoot[block.ObjectRootList.Length];
            for (int j = 0; j < block.ObjectRootList.Length; j++)
            {
                if (block.ObjectRootList[j].RdbObjects == null)
                    continue;

                convBlock.ObjectRootList[j].RdbObjects = new DFBlock.RdbObject[block.ObjectRootList[j].RdbObjects.Length];
                for (int k = 0; k < convBlock.ObjectRootList[j].RdbObjects.Length; k++)
                {
                    convBlock.ObjectRootList[j].RdbObjects[k].Position = block.ObjectRootList[j].RdbObjects[k].Position;
                    convBlock.ObjectRootList[j].RdbObjects[k].Index = block.ObjectRootList[j].RdbObjects[k].Index;
                    convBlock.ObjectRootList[j].RdbObjects[k].XPos = block.ObjectRootList[j].RdbObjects[k].XPos;
                    convBlock.ObjectRootList[j].RdbObjects[k].YPos = block.ObjectRootList[j].RdbObjects[k].YPos;
                    convBlock.ObjectRootList[j].RdbObjects[k].ZPos = block.ObjectRootList[j].RdbObjects[k].ZPos;
                    convBlock.ObjectRootList[j].RdbObjects[k].Type = (DFBlock.RdbResourceTypes)block.ObjectRootList[j].RdbObjects[k].Type;

                    switch (convBlock.ObjectRootList[j].RdbObjects[k].Type)
                    {
                        case DFBlock.RdbResourceTypes.Model:
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.XRotation = block.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.XRotation;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.YRotation = block.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.YRotation;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ZRotation = block.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ZRotation;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ModelIndex = block.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ModelIndex;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.TriggerFlag_StartingLock = block.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.TriggerFlag_StartingLock;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.SoundIndex = block.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.SoundIndex;

                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.Position = block.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.Position;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.Axis = block.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.Axis;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.Duration = block.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.Duration;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.Magnitude = block.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.Magnitude;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.NextObjectOffset = block.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.NextObjectOffset;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.PreviousObjectOffset = block.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.PreviousObjectOffset;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.NextObjectIndex = block.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.NextObjectIndex;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.Flags = block.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.Flags;
                            break;

                        case DFBlock.RdbResourceTypes.Light:
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.LightResource.Unknown1 = block.ObjectRootList[j].RdbObjects[k].Resources.LightResource.Unknown1;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.LightResource.Unknown2 = block.ObjectRootList[j].RdbObjects[k].Resources.LightResource.Unknown2;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.LightResource.Radius = block.ObjectRootList[j].RdbObjects[k].Resources.LightResource.Radius;
                            break;

                        case DFBlock.RdbResourceTypes.Flat:
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.Position = block.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.Position;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.TextureArchive = block.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.TextureArchive;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.TextureRecord = block.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.TextureRecord;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.Flags = block.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.Flags;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.Magnitude = block.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.Magnitude;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.SoundIndex = block.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.SoundIndex;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.FactionOrMobileId = block.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.FactionOrMobileId;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.NextObjectOffset = block.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.NextObjectOffset;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.Action = block.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.Action;
                            break;

                        default:
                            Debug.Log("GetRdbBDForRdb = unknown RdbResourceTypes");
                            break;
                    }
                }
            }

            return convBlock;
        }

        public static DFBlockRotation.RdbBlockDesc GetRdbBDForRdb(DFBlock.RdbBlockDesc block)
        {
            DFBlockRotation.RdbBlockDesc convBlock = new DFBlockRotation.RdbBlockDesc();

            convBlock.ModelReferenceList = new DFBlockRotation.RdbModelReference[block.ModelReferenceList.Length];
            for (int i = 0; i < block.ModelReferenceList.Length; i++)
            {
                convBlock.ModelReferenceList[i].ModelId = block.ModelReferenceList[i].ModelId;
                convBlock.ModelReferenceList[i].ModelIdNum = block.ModelReferenceList[i].ModelIdNum;
                convBlock.ModelReferenceList[i].Description = block.ModelReferenceList[i].Description;
            }

            convBlock.ObjectRootList = new DFBlockRotation.RdbObjectRoot[block.ObjectRootList.Length];
            for (int j = 0; j < block.ObjectRootList.Length; j++)
            {
                if (block.ObjectRootList[j].RdbObjects == null)
                    continue;

                convBlock.ObjectRootList[j].RdbObjects = new DFBlockRotation.RdbObject[block.ObjectRootList[j].RdbObjects.Length];
                for (int k = 0; k < convBlock.ObjectRootList[j].RdbObjects.Length; k++)
                {
                    convBlock.ObjectRootList[j].RdbObjects[k].Position = block.ObjectRootList[j].RdbObjects[k].Position;
                    convBlock.ObjectRootList[j].RdbObjects[k].Index = block.ObjectRootList[j].RdbObjects[k].Index;
                    convBlock.ObjectRootList[j].RdbObjects[k].XPos = block.ObjectRootList[j].RdbObjects[k].XPos;
                    convBlock.ObjectRootList[j].RdbObjects[k].YPos = block.ObjectRootList[j].RdbObjects[k].YPos;
                    convBlock.ObjectRootList[j].RdbObjects[k].ZPos = block.ObjectRootList[j].RdbObjects[k].ZPos;
                    convBlock.ObjectRootList[j].RdbObjects[k].Type = (DFBlockRotation.RdbResourceTypes)block.ObjectRootList[j].RdbObjects[k].Type;

                    switch (convBlock.ObjectRootList[j].RdbObjects[k].Type)
                    {
                        case DFBlockRotation.RdbResourceTypes.Model:
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.XRotation = block.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.XRotation;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.YRotation = block.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.YRotation;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ZRotation = block.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ZRotation;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ModelIndex = block.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ModelIndex;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.TriggerFlag_StartingLock = block.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.TriggerFlag_StartingLock;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.SoundIndex = block.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.SoundIndex;

                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.Position = block.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.Position;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.Axis = block.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.Axis;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.Duration = block.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.Duration;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.Magnitude = block.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.Magnitude;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.NextObjectOffset = block.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.NextObjectOffset;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.PreviousObjectOffset = block.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.PreviousObjectOffset;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.NextObjectIndex = block.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.NextObjectIndex;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.Flags = block.ObjectRootList[j].RdbObjects[k].Resources.ModelResource.ActionResource.Flags;
                            break;

                        case DFBlockRotation.RdbResourceTypes.Light:
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.LightResource.Unknown1 = block.ObjectRootList[j].RdbObjects[k].Resources.LightResource.Unknown1;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.LightResource.Unknown2 = block.ObjectRootList[j].RdbObjects[k].Resources.LightResource.Unknown2;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.LightResource.Radius = block.ObjectRootList[j].RdbObjects[k].Resources.LightResource.Radius;
                            break;

                        case DFBlockRotation.RdbResourceTypes.Flat:
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.Position = block.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.Position;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.TextureArchive = block.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.TextureArchive;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.TextureRecord = block.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.TextureRecord;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.Flags = block.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.Flags;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.Magnitude = block.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.Magnitude;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.SoundIndex = block.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.SoundIndex;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.FactionOrMobileId = block.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.FactionOrMobileId;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.NextObjectOffset = block.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.NextObjectOffset;
                            convBlock.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.Action = block.ObjectRootList[j].RdbObjects[k].Resources.FlatResource.Action;
                            break;

                        default:
                            Debug.Log("GetRdbBDForRdb = unknown RdbResourceTypes");
                            break;
                    }
                }
            }

            return convBlock;
        }
        
        public static void RotateBlock(string blockName, int rotation)
        {
            DFBlock block = new DFBlock();
            DFBlock rotatedBlock = new DFBlock();
            string path = Path.Combine(testPath, "RMB", blockName + ".RMB.json");
            block = JsonConvert.DeserializeObject<DFBlock>(File.ReadAllText(path));

            rotatedBlock.Position = block.Position;
            rotatedBlock.Index = block.Index;
            rotatedBlock.Name = block.Name.Substring(0, 3) + (rotation + 1).ToString() + block.Name.Substring(4, 4) + ".RMB";
            rotatedBlock.Type = (DFBlock.BlockTypes)block.Type;

            switch (rotatedBlock.Type)
            {
                case DFBlock.BlockTypes.Rmb:
                    rotatedBlock.RmbBlock = RotateRMBBlock(rotatedBlock.Name, block.RmbBlock, rotation);
                    break;
                
                default:
                    break;
            }
            path = Path.Combine(testPath, "RMB", rotatedBlock.Name + ".json");
            var json = JsonConvert.SerializeObject(rotatedBlock, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            File.WriteAllText(path, json);
        }

        public static DFBlock.RmbBlockDesc RotateRMBBlock(string rotatedName, DFBlock.RmbBlockDesc rmbBlock, int rotation)
        {
            DFBlock.RmbBlockDesc rotatedRmb = new DFBlock.RmbBlockDesc();

            rotatedRmb.FldHeader.NumBlockDataRecords = rmbBlock.FldHeader.NumBlockDataRecords;
            rotatedRmb.FldHeader.NumMisc3dObjectRecords = rmbBlock.FldHeader.NumMisc3dObjectRecords;
            rotatedRmb.FldHeader.NumMiscFlatObjectRecords = rmbBlock.FldHeader.NumMiscFlatObjectRecords;

            rotatedRmb.FldHeader.BlockPositions = new DFBlock.RmbFldBlockPositions[rmbBlock.FldHeader.BlockPositions.Length];
            for (int i = 0; i < rmbBlock.FldHeader.BlockPositions.Length; i++)
            {
                rotatedRmb.FldHeader.BlockPositions[i] = RotateBlockPositions(rmbBlock.FldHeader.BlockPositions[i], rotation);
                rotatedRmb.FldHeader.BlockPositions[i].YRotation = rmbBlock.FldHeader.BlockPositions[i].YRotation - (512 * (rotation + 1));
                if (rotatedRmb.FldHeader.BlockPositions[i].YRotation >= 2048)
                    rotatedRmb.FldHeader.BlockPositions[i].YRotation -= 2048;
                if (rotatedRmb.FldHeader.BlockPositions[i].YRotation <= -2048)
                    rotatedRmb.FldHeader.BlockPositions[i].YRotation += 2048;
            }

            rotatedRmb.FldHeader.BuildingDataList = rmbBlock.FldHeader.BuildingDataList;
            rotatedRmb.FldHeader.BlockDataSizes = rmbBlock.FldHeader.BlockDataSizes;
            rotatedRmb.FldHeader.GroundData.Header = rmbBlock.FldHeader.GroundData.Header;

            rotatedRmb.FldHeader.GroundData.GroundTiles = null;
            rotatedRmb.FldHeader.GroundData.GroundScenery = null;
            rotatedRmb.FldHeader.GroundData.GroundTiles1d = new DFBlock.RmbGroundTiles[256];
            rotatedRmb.FldHeader.GroundData.GroundScenery1d = new DFBlock.RmbGroundScenery[256];
            
            for (int k = 0; k < 16; k++)
            {
                for (int j = 0; j < 16; j++)
                {
                    (int, int) rotPos = RotateCoord(j, k, rotation, 16);
                    rotatedRmb.FldHeader.GroundData.GroundTiles1d[j + k * 16] = RotateGroundTiles(rmbBlock.FldHeader.GroundData.GroundTiles1d[rotPos.Item1 + rotPos.Item2 * 16], rotation);

                    (int, int) gsMod = GetGSMod(rotation);
                    int gsIndex = (j + gsMod.Item1) + (k + gsMod.Item2) * 16;
                    if (gsIndex >= 0 && gsIndex < 256)
                        rotatedRmb.FldHeader.GroundData.GroundScenery1d[gsIndex] = rmbBlock.FldHeader.GroundData.GroundScenery1d[rotPos.Item1 + rotPos.Item2 * 16];
                }
            }

            rotatedRmb.FldHeader.AutoMapData = new uint[64 * 64];
            for (int l = 0; l < (64 * 64); l++)
            {
                int x = l % 64;
                int y = l / 64;
                (int, int) rotPos = RotateCoord(x, y, rotation, 64);
                rotatedRmb.FldHeader.AutoMapData[l] = rmbBlock.FldHeader.AutoMapData[rotPos.Item1 + rotPos.Item2 * 64];
            }

            rotatedRmb.FldHeader.Name = rotatedName;
            rotatedRmb.FldHeader.OtherNames = rmbBlock.FldHeader.OtherNames;

            rotatedRmb.SubRecords = new DFBlock.RmbSubRecord[rmbBlock.SubRecords.Length];
            for (int m = 0; m < rmbBlock.SubRecords.Length; m++)
            {
                rotatedRmb.SubRecords[m] = RotateSubRecordPositions(rmbBlock.SubRecords[m], rotation);
                rotatedRmb.SubRecords[m].YRotation = rmbBlock.SubRecords[m].YRotation - (512 * (rotation + 1));
                if (rotatedRmb.SubRecords[m].YRotation >= 2048)
                    rotatedRmb.SubRecords[m].YRotation -= 2048;
                if (rotatedRmb.SubRecords[m].YRotation <= -2048)
                    rotatedRmb.SubRecords[m].YRotation += 2048;

                rotatedRmb.SubRecords[m].Exterior = rmbBlock.SubRecords[m].Exterior;
                // rotatedRmb.SubRecords[m].Exterior = RotateExteriorInterior(rmbBlock.SubRecords[m].Exterior, rotation);
                rotatedRmb.SubRecords[m].Interior = rmbBlock.SubRecords[m].Interior;
                // rotatedRmb.SubRecords[m].Interior = RotateExteriorInterior(rmbBlock.SubRecords[m].Interior, rotation);
            }

            rotatedRmb.Misc3dObjectRecords = new DFBlock.RmbBlock3dObjectRecord[rmbBlock.Misc3dObjectRecords.Length];
            for (int n = 0; n < rmbBlock.Misc3dObjectRecords.Length; n++)
            {
                rotatedRmb.Misc3dObjectRecords[n] = RotateBlock3dObjectRecord(rmbBlock.Misc3dObjectRecords[n], rotation, true);
            }

            rotatedRmb.MiscFlatObjectRecords = new DFBlock.RmbBlockFlatObjectRecord[rmbBlock.MiscFlatObjectRecords.Length];
            for (int o = 0; o < rmbBlock.MiscFlatObjectRecords.Length; o++)
            {
                rotatedRmb.MiscFlatObjectRecords[o] = RotateBlockFlatObjectRecord(rmbBlock.MiscFlatObjectRecords[o], rotation, true);
            }
            return rotatedRmb;
        }

        public static (int, int) GetGSMod(int rotation)
        {
            (int, int) modifier;

            switch (rotation)
            {
                case 0:
                    return (1, 0);
                case 1:
                    return (1, 1);
                case 2:
                    return (0, 1);
                default:
                    Debug.Log("GetGSMod = wrong rotation value!");
                    return (0, 0);
            }
        }

        public static DFBlock.RmbBlockData RotateExteriorInterior(DFBlock.RmbBlockData blockData, int rotation)
        {
            DFBlock.RmbBlockData resultingBlockData = new DFBlock.RmbBlockData();
            resultingBlockData.Header = blockData.Header;

            resultingBlockData.Block3dObjectRecords = new DFBlock.RmbBlock3dObjectRecord[blockData.Block3dObjectRecords.Length];
            for (int i = 0; i < blockData.Block3dObjectRecords.Length; i++)
            {                
                resultingBlockData.Block3dObjectRecords[i] = RotateBlock3dObjectRecord(blockData.Block3dObjectRecords[i], rotation, false);
            }

            resultingBlockData.BlockFlatObjectRecords = new DFBlock.RmbBlockFlatObjectRecord[blockData.BlockFlatObjectRecords.Length];
            for (int j = 0; j < blockData.BlockFlatObjectRecords.Length; j++)
            {
                resultingBlockData.BlockFlatObjectRecords[j] = RotateBlockFlatObjectRecord(blockData.BlockFlatObjectRecords[j], rotation, false);
            }

            resultingBlockData.BlockSection3Records = new DFBlock.RmbBlockSection3Record[blockData.BlockSection3Records.Length];
            for (int k = 0; k < blockData.BlockSection3Records.Length; k++)
            {
                resultingBlockData.BlockSection3Records[k] = RotateBlockSection3Record(blockData.BlockSection3Records[k], rotation);
            }

            resultingBlockData.BlockPeopleRecords = new DFBlock.RmbBlockPeopleRecord[blockData.BlockPeopleRecords.Length];
            for (int l = 0; l < blockData.BlockPeopleRecords.Length; l++)
            {
                resultingBlockData.BlockPeopleRecords[l] = RotateBlockPeopleRecord(blockData.BlockPeopleRecords[l], rotation);
            }

            resultingBlockData.BlockDoorRecords = new DFBlock.RmbBlockDoorRecord[blockData.BlockDoorRecords.Length];
            for (int m = 0; m < blockData.BlockDoorRecords.Length; m++)
            {
                resultingBlockData.BlockDoorRecords[m] = RotateBlockDoorRecord(blockData.BlockDoorRecords[m], rotation);
            }
            return resultingBlockData;
        }

        public static DFBlock.RmbFldBlockPositions RotateBlockPositions(DFBlock.RmbFldBlockPositions blockPos, int rotation)
        {
            DFBlock.RmbFldBlockPositions rotatedBlockPos = new DFBlock.RmbFldBlockPositions();
            int size = 4096;
            bool zNegative = blockPos.ZPos < 0;
            if (zNegative)
                blockPos.ZPos *= -1;

            switch (rotation)
            {
                case 0: // 90 degrees
                    rotatedBlockPos.XPos = size - blockPos.ZPos;
                    rotatedBlockPos.ZPos = blockPos.XPos;
                    break;
                case 1: // 180 degrees
                    rotatedBlockPos.XPos = size - blockPos.XPos;
                    rotatedBlockPos.ZPos = size - blockPos.ZPos;
                    break;
                case 2: // 270 degrees
                    rotatedBlockPos.XPos = blockPos.ZPos;
                    rotatedBlockPos.ZPos = size - blockPos.XPos;
                    break;
                default:
                    Debug.Log("RotateBlockPosition = wrong rotation value!");
                    break;
            }
            if (zNegative)
                rotatedBlockPos.ZPos *= -1;
            rotatedBlockPos.YRotation = blockPos.YRotation; // YRotation get calculated outside this method, since it's the same calculation for every block.
            return rotatedBlockPos;
        }

        public static DFBlock.RmbSubRecord RotateSubRecordPositions(DFBlock.RmbSubRecord subRecord, int rotation)
        {
            DFBlock.RmbSubRecord rotatedSubPos = new DFBlock.RmbSubRecord();
            int size = 4096;

            switch (rotation)
            {
                case 0: // 90 degrees
                    rotatedSubPos.XPos = size - subRecord.ZPos;
                    rotatedSubPos.ZPos = subRecord.XPos;
                    break;
                case 1: // 180 degrees
                    rotatedSubPos.XPos = size - subRecord.XPos;
                    rotatedSubPos.ZPos = size - subRecord.ZPos;
                    break;
                case 2: // 270 degrees
                    rotatedSubPos.XPos = subRecord.ZPos;
                    rotatedSubPos.ZPos = size - subRecord.XPos;
                    break;
                default:
                    Debug.Log("RotateSubRecordPosition = wrong rotation value!");
                    break;
            }
            rotatedSubPos.YRotation = subRecord.YRotation; // YRotation get calculated outside this method, since it's the same calculation for every block.
            return rotatedSubPos;
        }

        public static DFBlock.RmbGroundTiles RotateGroundTiles(DFBlock.RmbGroundTiles tile, int rotation)
        {
            DFBlock.RmbGroundTiles resultingTile = new DFBlock.RmbGroundTiles();
            resultingTile.TileBitfield = (byte)tile.TextureRecord;
            resultingTile.TextureRecord = tile.TextureRecord;

            switch (rotation)
            {
                case 0:
                    // 1st version
                    resultingTile.IsRotated = !tile.IsRotated;
                    if (!tile.IsRotated)
                        resultingTile.IsFlipped = !tile.IsFlipped;
                    else resultingTile.IsFlipped = tile.IsFlipped;
                    break;
                case 1:
                    // Flipped 1st
                    resultingTile.IsRotated = tile.IsRotated;
                    resultingTile.IsFlipped = !tile.IsFlipped;
                    break;
                case 2:
                    // 1st version
                    resultingTile.IsRotated = !tile.IsRotated;
                    if (!tile.IsRotated)
                        resultingTile.IsFlipped = tile.IsFlipped;
                    else resultingTile.IsFlipped = !tile.IsFlipped;
                    break;
                default:
                    Debug.Log("RotateGroundTiles = wrong rotation value!");
                    break;
            }

            if (resultingTile.IsRotated)
                resultingTile.TileBitfield += 64;
            if (resultingTile.IsFlipped)
                resultingTile.TileBitfield += 128;

            return resultingTile;
        }

        public static DFBlock.RmbBlock3dObjectRecord RotateBlock3dObjectRecord(DFBlock.RmbBlock3dObjectRecord objRec, int rotation, bool isExterior)
        {
            DFBlock.RmbBlock3dObjectRecord resulting3dObj = new DFBlock.RmbBlock3dObjectRecord();
            resulting3dObj = objRec;
            (int, int) rotatedCoord;
            if (isExterior) rotatedCoord = RotateCoord3dObj(objRec.XPos, objRec.ZPos, rotation, 4096);
            else rotatedCoord = RotateCoordInt(objRec.XPos, objRec.ZPos, rotation);
            resulting3dObj.XPos = rotatedCoord.Item1;
            resulting3dObj.ZPos = rotatedCoord.Item2;
            // rotatedRmb.SubRecords[m].Exterior.Block3dObjectRecords[n].XPos = rmbBlock.SubRecords[m].Exterior.Block3dObjectRecords[n].XPos;
            // rotatedRmb.SubRecords[m].Exterior.Block3dObjectRecords[n].YPos = rmbBlock.SubRecords[m].Exterior.Block3dObjectRecords[n].YPos;
            // rotatedRmb.SubRecords[m].Exterior.Block3dObjectRecords[n].ZPos = rmbBlock.SubRecords[m].Exterior.Block3dObjectRecords[n].ZPos;
            // rotatedRmb.SubRecords[m].Exterior.Block3dObjectRecords[n].XRotation = rmbBlock.SubRecords[m].Exterior.Block3dObjectRecords[n].XRotation;
            resulting3dObj.YRotation = (short)(objRec.YRotation - (512 * (rotation + 1)));
            // if (isExterior) resulting3dObj.YRotation = (short)(objRec.YRotation - (512 * (rotation + 1)));
            // else resulting3dObj.YRotation = (short)(objRec.YRotation + (512 * (rotation + 1)));
            if (resulting3dObj.YRotation >= 2048)
                resulting3dObj.YRotation -= 2048;
             if (resulting3dObj.YRotation <= -2048)
                resulting3dObj.YRotation += 2048;
            // rotatedRmb.SubRecords[m].Exterior.Block3dObjectRecords[n].ZRotation = rmbBlock.SubRecords[m].Exterior.Block3dObjectRecords[n].ZRotation;
            return resulting3dObj;
        }

        // public static DFBlockRotation.RmbBlock3dObjectRecord RotateBlock3dObjectRecordInt(DFBlockRotation.RmbBlock3dObjectRecord objRec, int rotation, int area)
        // {
        //     DFBlockRotation.RmbBlock3dObjectRecord resulting3dObj = new DFBlockRotation.RmbBlock3dObjectRecord();
        //     resulting3dObj = objRec;
        //     (int, int) rotatedCoord = RotateCoordInt(objRec.XPos, objRec.ZPos, rotation);
        //     resulting3dObj.XPos = rotatedCoord.Item1;
        //     resulting3dObj.ZPos = rotatedCoord.Item2;
        //     // rotatedRmb.SubRecords[m].Exterior.Block3dObjectRecords[n].XPos = rmbBlock.SubRecords[m].Exterior.Block3dObjectRecords[n].XPos;
        //     // rotatedRmb.SubRecords[m].Exterior.Block3dObjectRecords[n].YPos = rmbBlock.SubRecords[m].Exterior.Block3dObjectRecords[n].YPos;
        //     // rotatedRmb.SubRecords[m].Exterior.Block3dObjectRecords[n].ZPos = rmbBlock.SubRecords[m].Exterior.Block3dObjectRecords[n].ZPos;
        //     // rotatedRmb.SubRecords[m].Exterior.Block3dObjectRecords[n].XRotation = rmbBlock.SubRecords[m].Exterior.Block3dObjectRecords[n].XRotation;
        //     resulting3dObj.YRotation = (short)(objRec.YRotation + (512 * (rotation + 1)));
        //     if (resulting3dObj.YRotation >= 2048)
        //         resulting3dObj.YRotation -= 2048;
        //      if (resulting3dObj.YRotation <= -2048)
        //         resulting3dObj.YRotation += 2048;
        //     // rotatedRmb.SubRecords[m].Exterior.Block3dObjectRecords[n].ZRotation = rmbBlock.SubRecords[m].Exterior.Block3dObjectRecords[n].ZRotation;
        //     return resulting3dObj;
        // }

        public static DFBlock.RmbBlockFlatObjectRecord RotateBlockFlatObjectRecord(DFBlock.RmbBlockFlatObjectRecord objRec, int rotation, bool isExterior)
        {
            DFBlock.RmbBlockFlatObjectRecord resultingFlatObj = new DFBlock.RmbBlockFlatObjectRecord();
            resultingFlatObj = objRec;
            (int, int) rotatedCoord;
            if (isExterior) rotatedCoord = RotateCoord(objRec.XPos, objRec.ZPos, rotation, 4096);
            else rotatedCoord = RotateCoordInt(objRec.XPos, objRec.ZPos, rotation);
            resultingFlatObj.XPos = rotatedCoord.Item1;
            resultingFlatObj.ZPos = rotatedCoord.Item2;
            return resultingFlatObj;
        }

        public static DFBlock.RmbBlockSection3Record RotateBlockSection3Record(DFBlock.RmbBlockSection3Record objRec, int rotation)
        {
            DFBlock.RmbBlockSection3Record resultingSec3Rec = new DFBlock.RmbBlockSection3Record();
            resultingSec3Rec = objRec;
            return resultingSec3Rec;
        }

        public static DFBlock.RmbBlockPeopleRecord RotateBlockPeopleRecord(DFBlock.RmbBlockPeopleRecord objRec, int rotation)
        {
            DFBlock.RmbBlockPeopleRecord resultingPeopleRec = new DFBlock.RmbBlockPeopleRecord();
            resultingPeopleRec = objRec;
            return resultingPeopleRec;
        }

        public static DFBlock.RmbBlockDoorRecord RotateBlockDoorRecord(DFBlock.RmbBlockDoorRecord objRec, int rotation)
        {
            DFBlock.RmbBlockDoorRecord resultingDoorRec = new DFBlock.RmbBlockDoorRecord();
            resultingDoorRec = objRec;
            return resultingDoorRec;
        }

        public static (int, int) RotateCoord(int x, int z, int rotation, int size)
        {
            (int, int) rotatedCoord;
            bool zNegative = z < 0;
            size--;
            if (zNegative)
                z *= -1;

            switch (rotation)
            {
                case 0: // 90 degrees
                    rotatedCoord = (z, size - x);
                    break;
                case 1: // 180 degrees
                    rotatedCoord = (size - x, size - z);
                    break;
                case 2: // 270 degrees
                    rotatedCoord = (size - z, x);
                    break;
                default:
                    Debug.Log("RotateCoord = wrong rotation value!");
                    rotatedCoord = (-1, -1);
                    break;
            }
            
            if (zNegative)
                rotatedCoord = (rotatedCoord.Item1, rotatedCoord.Item2 * -1);

            return rotatedCoord;
        }

        public static (int, int) RotateCoord3dObj(int x, int z, int rotation, int size)
        {
            (int, int) rotatedCoord;
            bool zNegative = z < 0;
            size--;
            if (zNegative)
                z *= -1;

            switch (rotation)
            {
                case 0: // 90 degrees
                    rotatedCoord = (size - z, x);
                    break;
                case 1: // 180 degrees
                    rotatedCoord = (size - x, size - z);
                    break;
                case 2: // 270 degrees
                    rotatedCoord = (z, size - x);
                    break;
                default:
                    Debug.Log("RotateCoord = wrong rotation value!");
                    rotatedCoord = (-1, -1);
                    break;
            }
            
            if (zNegative)
                rotatedCoord = (rotatedCoord.Item1, rotatedCoord.Item2 * -1);

            return rotatedCoord;
        }

        public static (int, int) RotateCoordInt(int x, int z, int rotation)
        {
            (int, int) rotatedCoord;

            switch (rotation)
            {
                case 0: // 90 degrees
                    rotatedCoord = (z, x * -1);
                    break;
                case 1: // 180 degrees
                    rotatedCoord = (x * -1, z * -1);
                    break;
                case 2: // 270 degrees
                    rotatedCoord = (z * -1, x);
                    break;
                default:
                    Debug.Log("RotateCoord = wrong rotation value!");
                    rotatedCoord = (-1, -1);
                    break;
            }
            return rotatedCoord;
        }

        public static void InsertRotatedAndNewBlocks()
        {
            Worldmap mapTile = new Worldmap();

            int[][] blockLimits = new int[blockPrefixes.Length][];
            blockLimits = JsonConvert.DeserializeObject<int[][]>(File.ReadAllText(Path.Combine(testPath, "BlockLimits.json")));

            int randomChance = UnityEngine.Random.Range(int.MaxValue, -1);
            int randomRotation;
            string sizeChar;
            int firstNew;
            int lastNew;

            for (int y = 0; y < 6144 / 64; y++)
            {
                for (int x = 0; x < 7680 / 64; x++)
                {
                    mapTile = JsonConvert.DeserializeObject<Worldmap>(File.ReadAllText(Path.Combine(testPath, "Locations", "map" + (x + y * MapsFile.TileX).ToString("00000") + ".json")));

                    if (mapTile.LocationCount <= 0)
                    {
                        mapTile.DiscardMapTile();
                        continue;
                    }

                    for (int i = 0; i < mapTile.Locations.Length; i++)
                    {
                        for (int j = 0; j < mapTile.Locations[i].Exterior.ExteriorData.BlockNames.Length; j++)
                        {
                            string blockName = mapTile.Locations[i].Exterior.ExteriorData.BlockNames[j];
                            randomRotation = UnityEngine.Random.Range(0, 4);

                            // Don't rotate custom (unique) blocks and ships, just change the name to the new format
                            if (blockName.StartsWith("CUS") || blockName.StartsWith("SHI"))
                            {
                                blockName = blockName.Substring(0, 3) + "0" + blockName.Substring(4, 8);
                                continue;
                            }

                            // Walls can rotate, but it has meaning only for Wayton's new wall blocks
                            else if (blockName.StartsWith("WAL"))
                            {
                                blockName = RandomizeWallVariant(blockName);
                                continue;
                            }

                            // This first time, I need to insert FG blocks with the new size indicators;
                            // From next time, FIG blocks should behave like any other.
                            else if (blockName.StartsWith("FIG"))
                            {
                                // randomChance = UnityEngine.Random.Range(0, 2);
                                switch (mapTile.Locations[i].MapTableData.LocationType)
                                {
                                    case DFRegion.LocationTypes.TownCity:
                                        sizeChar = "L";
                                        break;
                                    case DFRegion.LocationTypes.TownHamlet:
                                        sizeChar = "M";
                                        break;
                                    case DFRegion.LocationTypes.TownVillage:
                                    default:
                                        sizeChar = "S";
                                        break;
                                }
                                if ((blockName.Substring(4, 1)).Equals("A"))
                                {                                    
                                    blockName = "FIG" + randomRotation.ToString() + "A" + sizeChar + (randomChance % 2).ToString("00") + ".RMB";
                                }
                                else
                                {
                                    if (!sizeChar.Equals("S"))
                                        blockName = "FIG" + randomRotation.ToString() + blockName.Substring(4, 1) + sizeChar + "00.RMB";
                                    else
                                        blockName = "FIG" + randomRotation.ToString() + blockName.Substring(4, 1) + sizeChar + (randomChance % 2).ToString("00") + ".RMB";
                                }
                            }

                            // Only S sized WEAP block exists in the original game, I need to insert the others
                            // Since ARM blocks are the opposite (just L and M blocks), I could re-randomize their distribution
                            // whenever I encounter one or the other.
                            else if (blockName.StartsWith("WEA") || blockName.StartsWith("ARM"))
                            {
                                if (randomChance % 2 == 1)
                                {
                                    if (blockName.StartsWith("WEA"))
                                    {
                                        blockName.Replace("WEA", "ARM");
                                        blockName.Remove(3, 1);
                                        blockName.Insert(3, randomRotation.ToString());
                                        blockName.Replace(blockName.Substring(6, 2), ((randomChance % 2) % 2).ToString("00"));
                                    }
                                    else // if (blockName.StartsWith("ARM"))
                                    {
                                        blockName.Replace("ARM", "WEA");
                                        blockName.Remove(3, 1);
                                        blockName.Insert(3, randomRotation.ToString());

                                        if ((blockName.Substring(4, 2)).Equals("AL"))
                                        {
                                            blockName.Replace(blockName.Substring(6, 2), ((randomChance % 2) % 4).ToString("00"));
                                        }
                                        else if ((blockName.Substring(4, 2)).Equals("AM"))
                                        {
                                            blockName.Replace(blockName.Substring(6, 2), ((randomChance % 2) % 3).ToString("00"));
                                        }
                                        else
                                        {
                                            blockName.Replace(blockName.Substring(6, 2), ((randomChance % 2) % 2).ToString("00"));
                                        }
                                    }
                                }
                            }

                            // No S sized ALC, BAN, BOO, GEM, PAW block exists in the original game. Where could I insert those?
                            // What about a system based on internal routes? Too complicated?
                            else if (blockName.StartsWith("ALC"))
                            {

                            }

                            // Only S sized CLOT block exists in the original game.
                            else if (blockName.StartsWith("CLO"))
                            {

                            }

                            // // Only L sized LIB block exists in the original game. Actually, this could make sense
                            else if (blockName.StartsWith("LIB"))
                            {

                            }

                            // Only S sized MANR block exists in the original game.
                            else if (blockName.StartsWith("MAN"))
                            {

                            }

                            // TVRN, GENR, RESI, 
                            else
                            {

                            }

                            mapTile.Locations[i].Exterior.ExteriorData.BlockNames[j] = blockName;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Record the highest progressive for each block type and subgroup.
        /// This is later used to insert new blocks with the right proportions.
        /// </summary>
        /// <returns></returns>
        public static void CalculateBlockLimits()
        {
            int[][] blockLimits = new int[blockPrefixes.Length - 5][];
            List<string>[][] blockUsed = new List<string>[blockPrefixes.Length - 5][];

            for (int i = 0; i < blockPrefixes.Length - 5; i++)
            {
                if (i == Array.IndexOf(blockPrefixes, "TEM"))
                {
                    blockLimits[i] = new int[rmbBlockClimate.Length * deityCode.Length];
                    blockUsed[i] = new List<string>[rmbBlockClimate.Length * deityCode.Length];
                    for (int j = 0; j < (rmbBlockClimate.Length * deityCode.Length); j++)
                    {
                        blockLimits[i][j] = -1;
                        blockUsed[i][j] = new List<string>();
                    }
                }
                else
                {
                    blockLimits[i] = new int[rmbBlockClimate.Length];
                    blockUsed[i] = new List<string>[rmbBlockClimate.Length];
                    for (int j = 0; j < rmbBlockClimate.Length; j++)
                    {
                        blockLimits[i][j] = -1;
                        blockUsed[i][j] = new List<string>();
                    }
                }
            }

            Worldmap mapTile = new Worldmap();

            for (int y = 0; y < 6144 / 64; y++)
            {
                for (int x = 0; x < 7680 / 64; x++)
                {
                    mapTile = JsonConvert.DeserializeObject<Worldmap>(File.ReadAllText(Path.Combine(testPath, "Locations", "map" + (x + y * MapsFile.TileX).ToString("00000") + ".json")));

                    if (mapTile.LocationCount <= 0)
                    {
                        mapTile.DiscardMapTile();
                        continue;
                    }

                    for (int k = 0; k < mapTile.Locations.Length; k++)
                    {
                        for (int l = 0; l < mapTile.Locations[k].Exterior.ExteriorData.BlockNames.Length; l++)
                        {
                            string blockName = mapTile.Locations[k].Exterior.ExteriorData.BlockNames[l];
                            int prefixIndex = Array.IndexOf(blockPrefixes, blockName.Substring(0, 3));
                            int climateIndex = Array.IndexOf(rmbBlockClimate, blockName.Substring(4, 2));
                            int prog;
                            int deityProg = 0;

                            Debug.Log("Working on " + blockName + "; prefixIndex: " + prefixIndex + ", climateIndex: " + climateIndex + " in " + mapTile.Locations[k].Name + ", " + mapTile.Locations[k].RegionIndex);

                            if (prefixIndex == Array.IndexOf(blockPrefixes, "TEM"))
                            {
                                prog = int.Parse(blockName.Substring(7, 1));
                                deityProg = Array.IndexOf(deityCode, blockName.Substring(6, 1));
                                if (blockLimits[prefixIndex][climateIndex * deityCode.Length + deityProg] <= prog)
                                {
                                    blockLimits[prefixIndex][climateIndex * deityCode.Length + deityProg] = prog;
                                }
                                if (blockUsed[prefixIndex][climateIndex * deityCode.Length + deityProg] == null || !blockUsed[prefixIndex][climateIndex * deityCode.Length + deityProg].Contains(blockName))
                                    blockUsed[prefixIndex][climateIndex * deityCode.Length + deityProg].Add(blockName);
                            }
                            else
                            {
                                prog = int.Parse(blockName.Substring(6, 2));

                                if (blockLimits[prefixIndex][climateIndex] <= prog)
                                {
                                    blockLimits[prefixIndex][climateIndex] = prog;
                                }
                                if (blockUsed[prefixIndex][climateIndex] == null || !blockUsed[prefixIndex][climateIndex].Contains(blockName))
                                    blockUsed[prefixIndex][climateIndex].Add(blockName);
                            }
                        }
                    }
                }
            }
            string path = Path.Combine(testPath, "BlockLimits.json");
            var jsonBlockLimits = JsonConvert.SerializeObject(blockLimits, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            File.WriteAllText(path, jsonBlockLimits);

            path = Path.Combine(testPath, "BlockUsed.json");
            var jsonBlockUsed = JsonConvert.SerializeObject(blockUsed, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            File.WriteAllText(path, jsonBlockUsed);
        }

        public static string RandomizeWallVariant(string wallName)
        {
            int wallProg = int.Parse(wallName.Substring(6, 2));
            int diceRoll = UnityEngine.Random.Range(1, 101);

            // 2/3 chances the wall block will stay as the default one (but with the rotation index inserted)
            if (diceRoll <= 67)
                return wallName.Substring(0, 3) + "0" + wallName.Substring(4, 8);

            diceRoll = UnityEngine.Random.Range(0, 4);
            wallProg = GetWallSubgroup(wallProg, (4 - diceRoll));

            return "WAL" + diceRoll.ToString() + "AA" + (wallProg + 12).ToString("00") + ".RMB";
        }

        public static int GetWallSubgroup(int wallProg, int addition)
        {
            int lowLim = (wallProg / 4) * 4;
            int highLim = lowLim + 3;

            wallProg += addition;

            if (wallProg > highLim) wallProg -= 4;

            return wallProg;
        }
    }
}