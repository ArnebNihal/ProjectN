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

namespace MapEditor
{
    public class BlockInspector : EditorWindow
    {
        static BlockInspector blockInspectorWindow;
        const string windowTitle = "Block Inspector";
        public static int currentBlockIndex;
        BlocksFile blockFileReader;
        public static string[] RMBBlocks;
        public DFBlock currentBlockData;
        public bool RmbBlock;
        public Vector2 scrollView1;
        public Vector2 scrollView2;
        public Vector2 scrollView3;
        public const string arena2Path = "/home/arneb/Games/daggerfall/DaggerfallGameFiles/arena2";

        void Awake()
        {
            currentBlockIndex = 0;
            if (blockFileReader == null)
                blockFileReader = new BlocksFile(Path.Combine(arena2Path, BlocksFile.Filename), FileUsage.UseMemory, true);

            RMBBlocks = SetRMBBlocks();
            RmbBlock = false;
        }

        void OnGUI()
        {
            int oldBlockIndex = currentBlockIndex;
            currentBlockIndex = EditorGUILayout.Popup("Block", currentBlockIndex, RMBBlocks, GUILayout.MaxWidth(300.0f));
            if (currentBlockIndex != oldBlockIndex)
            {
                AnalyzeSelectedBlock();
            }

            GUILayout.Label(RMBBlocks[currentBlockIndex], EditorStyles.boldLabel);
            GUILayout.Space(50.0f);
            EditorGUILayout.LabelField("Position: ", (currentBlockData.Position).ToString());
            EditorGUILayout.LabelField("Index: ", (currentBlockData.Index).ToString());
            // EditorGUILayout.LabelField("Name: ", (currentBlockData.Name).ToString());
            EditorGUILayout.LabelField("Type: ", (currentBlockData.Type).ToString());
            RmbBlock = EditorGUILayout.Foldout(RmbBlock, "RMB Blocks Data");
            if (RmbBlock)
            {
                EditorGUILayout.LabelField("NumBlockDataRecords: ", (currentBlockData.RmbBlock.FldHeader.NumBlockDataRecords).ToString());
                EditorGUILayout.LabelField("NumMisc3dObjectRecords: ", (currentBlockData.RmbBlock.FldHeader.NumMisc3dObjectRecords).ToString());
                EditorGUILayout.LabelField("NumMiscFlatObjectRecords: ", (currentBlockData.RmbBlock.FldHeader.NumMiscFlatObjectRecords).ToString());

                scrollView1 = EditorGUILayout.BeginScrollView(scrollView1);
                for (int i = 0; i < currentBlockData.RmbBlock.FldHeader.BlockPositions.Length; i++)
                {
                    EditorGUILayout.LabelField("XPos - " + i + ": ", (currentBlockData.RmbBlock.FldHeader.BlockPositions[i].XPos).ToString());
                    EditorGUILayout.LabelField("ZPos - " + i + ": ", (currentBlockData.RmbBlock.FldHeader.BlockPositions[i].ZPos).ToString());
                    EditorGUILayout.LabelField("YRotation - " + i + ": ", (currentBlockData.RmbBlock.FldHeader.BlockPositions[i].YRotation).ToString());
                }
                EditorGUILayout.EndScrollView();

                scrollView2 = EditorGUILayout.BeginScrollView(scrollView2);
                for (int j = 0; j < currentBlockData.RmbBlock.FldHeader.BuildingDataList.Length; j++)
                {
                    EditorGUILayout.LabelField("NameSeed - " + j + ": ", (currentBlockData.RmbBlock.FldHeader.BuildingDataList[j].NameSeed).ToString());
                    EditorGUILayout.LabelField("FactionId - " + j + ": ", (currentBlockData.RmbBlock.FldHeader.BuildingDataList[j].FactionId).ToString());
                    EditorGUILayout.LabelField("Sector - " + j + ": ", (currentBlockData.RmbBlock.FldHeader.BuildingDataList[j].Sector).ToString());
                    EditorGUILayout.LabelField("LocationId - " + j + ": ", (currentBlockData.RmbBlock.FldHeader.BuildingDataList[j].LocationId).ToString());
                    EditorGUILayout.LabelField("BuildingType - " + j + ": ", (currentBlockData.RmbBlock.FldHeader.BuildingDataList[j].BuildingType).ToString());
                    EditorGUILayout.LabelField("Quality - " + j + ": ", (currentBlockData.RmbBlock.FldHeader.BuildingDataList[j].Quality).ToString());
                }
                EditorGUILayout.EndScrollView();

                EditorGUILayout.LabelField("Name: ", currentBlockData.RmbBlock.FldHeader.Name);

                scrollView3 = EditorGUILayout.BeginScrollView(scrollView3);
                for (int k = 0; k < currentBlockData.RmbBlock.FldHeader.OtherNames.Length; k++)
                {
                    EditorGUILayout.LabelField("Other names - " + k + ": ", currentBlockData.RmbBlock.FldHeader.Name);
                }
                EditorGUILayout.EndScrollView();
            }
        }

        protected string[] SetRMBBlocks()
        {
            string[] blockNames = new string[999];
            DFBlock block = new DFBlock();
            string rmbPath = Path.Combine(WorldMaps.mapPath, "RMB");

            for (int i = 0; i < blockNames.Length; i++)
            {
                block = blockFileReader.GetBlock(i);
            
                if (block.Name == null || block.Name == "")
                    break;

                if (block.Name.EndsWith("RMB"))
                    blockNames[i] = block.Name;
            }

            if (!Directory.Exists(rmbPath))
            {
                Debug.Log("invalid RMB directory: " + rmbPath);
            }
            else
            {
                var rmbFiles = Directory.GetFiles(rmbPath, "*" + "RMB", SearchOption.AllDirectories);
                // var rmbFileNames = new string[rmbFiles.Length];
                // var loadedRMBNames = GetAllRMBFileNames();

                for (int i = 0; i < rmbFiles.Length; i++)
                {
                    blockNames.Append<string>(rmbFiles[i]);
                }
            }

            List<string> blockNamesList = new List<string>();
            blockNamesList = blockNames.ToList();
            blockNamesList.Sort();
            blockNames = new string[blockNamesList.Count];
            blockNames = blockNamesList.ToArray();

            return blockNames;
        }

        protected void AnalyzeSelectedBlock()
        {
            currentBlockData = new DFBlock();
            int blockIndex = blockFileReader.GetBlockIndex(RMBBlocks[currentBlockIndex]);
            blockFileReader.LoadBlock(blockIndex, out currentBlockData);
        }
    }
}