using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop;

namespace MapEditor
{
    public class TileCreator : EditorWindow
    {
        static TileCreator tileCreatorWindow;
        const string tileCreatorTitle = "Tile Creator";
        public static int mapType;
        public static string[] mapTypeNames = new string[Enum.GetNames(typeof(MapTypes)).Length];
        public static int tileDim;
        public static int tileDimIndex;
        public static int tileNumb;
        public static string[] tileDimOptions = { "1", "2", "4", "8", "16", "32", "64", "128", "256", "512" };
        public static int numberOfSessions;
        public static int sessionIndex;
        public static string buttonLabel;
        public static int tilesPerSession;
        public static int[,] intMatrix = new int[MapsFile.MaxMapPixelX, MapsFile.MaxMapPixelY];
        public static byte[,] byteMatrix = new byte[MapsFile.MaxMapPixelX, MapsFile.MaxMapPixelY];
        public static int recoverSession = 0;

        public class TileCreatorData
        {

        }

        public enum MapTypes
        {
            Climate,
            Politic,
            Woods,
            Trail
        }

        void Awake()
        {
            for (int i = 0; i < Enum.GetNames(typeof(MapTypes)).Length; i++)
            {
                mapTypeNames[i] = ((MapTypes)i).ToString();
            }
        }

        void OnGUI()
        {
            mapType = EditorGUILayout.Popup("Map Type: ", mapType, mapTypeNames, GUILayout.MaxWidth(600.0f));

            EditorGUILayout.BeginHorizontal();
            tileDimIndex = EditorGUILayout.Popup("Tile Dimension: ", tileDimIndex, tileDimOptions, GUILayout.MaxWidth(200.0f));
            tileDim = (int)(Math.Pow( 2, tileDimIndex));
            tileNumb = (MapsFile.MaxMapPixelX / tileDim) * (MapsFile.MaxMapPixelY / tileDim);
            EditorGUILayout.LabelField("Tiles to be created: ", tileNumb.ToString());
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            numberOfSessions = EditorGUILayout.IntField("Number of Sessions: ", numberOfSessions);
            if (numberOfSessions <= 0)
                numberOfSessions = 1;

            tilesPerSession = tileNumb / numberOfSessions;
            EditorGUILayout.LabelField("Number of tiles in each session: ", tilesPerSession.ToString());
            EditorGUILayout.EndHorizontal();

            recoverSession = EditorGUILayout.IntField("Start from session: ", recoverSession);

            if (sessionIndex == 0)
                buttonLabel = "Start!";
            else
                buttonLabel = ("Continue with session n." + (sessionIndex + 1));

            if (sessionIndex < numberOfSessions && GUILayout.Button(buttonLabel))
            {
                CreateTiles((MapTypes)mapType, tileDim);
            }

            if (GUILayout.Button("Complete Tiles"))
            {
                CompleteTiles((MapTypes)mapType);
            }
        }

        protected void CreateTiles(MapTypes mapType, int tileDim)
        {
            string sourceFile;
            string tileName;
            byte[,] byteTile = new byte[tileDim, tileDim];
            int[,] intTile = new int[tileDim, tileDim];

            switch (mapType)
            {
                case MapTypes.Climate:
                    sourceFile = "Climate";
                    tileName = "climate";
                    break;

                case MapTypes.Politic:
                    sourceFile = "Politic";
                    tileName = "politic";
                    break;

                case MapTypes.Woods:
                    sourceFile = "Woods";
                    tileName = "woods";
                    break;

                case MapTypes.Trail:
                    sourceFile = "Trail";
                    tileName = "trail";
                    break;

                default:
                    sourceFile = "";
                    tileName = "";
                    break;
            }

            if (recoverSession != 0)
                sessionIndex = recoverSession - 1;

            int columnsNum = MapsFile.MaxMapPixelX / tileDim;
            int rowsNum = MapsFile.MaxMapPixelY / tileDim;
            int columnsMin = sessionIndex * tilesPerSession;
            int columnsMax = (sessionIndex * tilesPerSession) + tilesPerSession;      // This is exclusive
            int rowsMin = (sessionIndex * tilesPerSession) / columnsNum;;
            int rowsMax = ((sessionIndex * tilesPerSession) + tilesPerSession)  / columnsNum;

            Texture2D tileImage = new Texture2D(1, 1);
            ImageConversion.LoadImage(tileImage, File.ReadAllBytes(Path.Combine(MapEditor.arena2Path, "Maps", "Tiles", sourceFile + ".png")));

            if (mapType == MapTypes.Woods)
                byteMatrix = MapEditor.ConvertToMatrix(tileImage);
            else if (mapType == MapTypes.Trail)
                MapEditor.ConvertToMatrix(tileImage, out intMatrix, true);
            else MapEditor.ConvertToMatrix(tileImage, out intMatrix);

            for (int tileIndex = 0 + columnsMin; tileIndex < columnsMax; tileIndex++)
            {
                int tileX = tileIndex % columnsNum;
                int tileY = tileIndex / columnsNum;
                byteTile = new byte[tileDim, tileDim];
                intTile = new int[tileDim, tileDim];

                for (int x = 0; x < tileDim; x++)
                {
                    for (int y = 0; y < tileDim; y++)
                    {
                        if (mapType == MapTypes.Woods)
                            byteTile[x, y] = byteMatrix[(tileX * tileDim) + x, (tileY * tileDim) + y];
                        else intTile[x, y] = intMatrix[(tileX * tileDim) + x, (tileY * tileDim) + y];
                    }
                }

                string fileDataPath = Path.Combine(MapEditor.arena2Path, "Maps", "Tiles", tileName + "_" + tileX + "_" + tileY + ".png");

                // if (!Directory.Exists(Path.Combine(MapEditor.arena2Path, "Maps", "Tiles", tileX.ToString())))
                //     Directory.CreateDirectory(Path.Combine(MapEditor.arena2Path, "Maps", "Tiles", tileX.ToString()));

                if (mapType == MapTypes.Woods)
                {
                    byte[] byteBuffer = MapEditor.ConvertToGrayscale(byteTile);
                    File.WriteAllBytes(fileDataPath, byteBuffer);
                }
                else if (mapType == MapTypes.Trail)
                {
                    byte[] intBuffer = MapEditor.ConvertToGrayscale(intTile, true);
                    File.WriteAllBytes(fileDataPath, intBuffer);
                }
                else{
                    byte[] intBuffer = MapEditor.ConvertToGrayscale(intTile);
                    File.WriteAllBytes(fileDataPath, intBuffer);
                }
            }

            sessionIndex++;
        }

        protected void CompleteTiles(MapTypes mapType)
        {
            string sourceFile;
            string tileName;
            byte[,] byteTile = new byte[tileDim, tileDim];
            int[,] intTile = new int[tileDim, tileDim];
            int tilesCompleted = 0;

            switch (mapType)
            {
                case MapTypes.Climate:
                    sourceFile = "Climate";
                    tileName = "climate";
                    break;

                case MapTypes.Politic:
                    sourceFile = "Politic";
                    tileName = "politic";
                    break;

                case MapTypes.Woods:
                    sourceFile = "Woods";
                    tileName = "woods";
                    break;

                case MapTypes.Trail:
                    sourceFile = "Trail";
                    tileName = "trail";
                    break;

                default:
                    sourceFile = "";
                    tileName = "";
                    break;
            }

            int columnsNum = MapsFile.MaxMapPixelX / tileDim;
            // int rowsNum = MapsFile.MaxMapPixelY / tileDim;
            // int columnsMin = index * tilesPerSession;
            // int columnsMax = (sessionIndex * tilesPerSession) + tilesPerSession;      // This is exclusive
            // int rowsMin = (sessionIndex * tilesPerSession) / columnsNum;;
            // int rowsMax = ((sessionIndex * tilesPerSession) + tilesPerSession)  / columnsNum;

            Texture2D tileImage = new Texture2D(1, 1);
            ImageConversion.LoadImage(tileImage, File.ReadAllBytes(Path.Combine(MapEditor.arena2Path, "Maps", "Tiles", sourceFile + ".png")));

            if (mapType == MapTypes.Woods)
                byteMatrix = MapEditor.ConvertToMatrix(tileImage);
            else if (mapType == MapTypes.Trail)
                MapEditor.ConvertToMatrix(tileImage, out intMatrix, true);
            else MapEditor.ConvertToMatrix(tileImage, out intMatrix);

            for (int index = 0; index < columnsNum; index++)
            {
                int tileX = index % columnsNum;
                int tileY = index / columnsNum;
                byteTile = new byte[tileDim, tileDim];
                intTile = new int[tileDim, tileDim];

                // DirectoryInfo di = new DirectoryInfo(Path.Combine(MapEditor.arena2Path, "Maps", "Tiles", tileX.ToString()));
                // FileInfo[] fi = di.GetFiles();
                // int fileIndex = Array.IndexOf(fi, Path.Combine(MapEditor.arena2Path, "Maps", "Tiles", tileX.ToString(), tileName + "_" + tileX + "_" + tileY + ".png"));

                if (!File.Exists(Path.Combine(MapEditor.arena2Path, "Maps", "Tiles", tileName + "_" + tileX + "_" + tileY + ".png")))
                {
                    for (int x = 0; x < tileDim; x++)
                    {
                        for (int y = 0; y < tileDim; y++)
                        {
                            if (mapType == MapTypes.Woods)
                                byteTile[x, y] = byteMatrix[(tileX * tileDim) + x, (tileY * tileDim) + y];
                            else intTile[x, y] = intMatrix[(tileX * tileDim) + x, (tileY * tileDim) + y];
                        }
                    }

                    string fileDataPath = Path.Combine(MapEditor.arena2Path, "Maps", "Tiles", tileName + "_" + tileX + "_" + tileY + ".png");

                    if (!Directory.Exists(Path.Combine(MapEditor.arena2Path, "Maps", "Tiles")))
                        Directory.CreateDirectory(Path.Combine(MapEditor.arena2Path, "Maps", "Tiles"));

                    if (mapType == MapTypes.Woods)
                    {
                        byte[] byteBuffer = MapEditor.ConvertToGrayscale(byteTile);
                        File.WriteAllBytes(fileDataPath, byteBuffer);
                    }
                    else if (mapType == MapTypes.Trail)
                    {
                        byte[] intBuffer = MapEditor.ConvertToGrayscale(intTile, true);
                        File.WriteAllBytes(fileDataPath, intBuffer);
                    }
                    else
                    {
                        byte[] intBuffer = MapEditor.ConvertToGrayscale(intTile);
                        File.WriteAllBytes(fileDataPath, intBuffer);
                    }

                    tilesCompleted++;
                }
            }

            Debug.Log("Tiles completed: " + tilesCompleted);
        }
    }
}