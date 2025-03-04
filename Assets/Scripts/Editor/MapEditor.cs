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
using DaggerfallWorkshop.Game.Player;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEditor.Build.Pipeline.Tasks;
using UnityEngine.EventSystems;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using DaggerfallWorkshop.Game.Player;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Serialization;

namespace MapEditor
{
    //[CustomEditor(typeof(MapEditor))]
    public class MapEditor : EditorWindow
    {
        static MapEditor window;
        const string windowTitle = "Map Editor";
        const string menuPath = "Daggerfall Tools/Map Editor";
        public static Vector2 mousePosition;

        // Map alpha channel variables and const
        public static float mapAlphaChannel = 255.0f;
        const float level1 = 255.0f;
        const float level2 = 200.0f;
        const float level3 = 180.0f;
        const float level4 = 160.0f;

        string worldName = "Tamriel";
        bool groupEnabled;

        //Levels and tools
        static bool heightmap = false;
        static bool climate = false;
        static bool politics = false;
        static bool locations = false;
        static bool trails = false;
        static bool mapImage = false;
        static bool drawHeightmap = false;
        static bool drawClimate = false;
        static bool drawPolitics = false;
        public static int drawnBufferCount;
        public static List<(int, int)> drawnBuffer;
        public static int colourIndex;
        public static Color32 paintBrush;

        //Rects and Rect constants
        static Rect mapView;
        static Rect heightmapView = new Rect(0, 0, heightmapWidth, heightmapHeight);
        static Rect heightmapRect = new Rect(-500, -500 , heightmapWidth, heightmapHeight);
        static Rect mapRect;
        static Rect townBlocksRect;
        const int heightmapOriginX = 0;
        const int heightmapOriginY = 0;
        static int heightmapRectWidth = MapsFile.MaxMapPixelX;
        static int heightmapRectHeight = MapsFile.MaxMapPixelY;
        const float setMovement = 0.01f;
        const float dataField = 600.0f;
        const float dataFieldBig = 1000.0f;
        const float dataFieldSmall = 200.0f;
        const string noBlock = "[__________]";

        const string buttonUp = "UP";
        const string buttonDown = "DOWN";
        const string buttonLeft = "LEFT";
        const string buttonRight = "RIGHT";
        const string buttonReset = "RESET";
        const string buttonZoomIn = "ZOOM IN";
        const string buttonZoomOut = "ZOOM OUT";
        static Rect layerPosition = new Rect(-500, -500, 1, 1);
        Texture2D referenceMap;
        Texture2D heightMap;
        byte[] heightmapByteArray;
        Texture2D locationsMap;
        Texture2D climateMap;
        Texture2D politicMap;
        Texture2D trailsMap;
        public static byte topValue;
        public static byte bottomValue;
        static GUIStyle guiStyle = new GUIStyle();
        static float zoomLevel = 1.0f;

        // mapReference constants
        const int mapReferenceWidth = 3440;
        const int mapReferenceHeight = 2400;
        public const int heightmapWidth = 5120, heightmapHeight = 5120;
        const int mapReferenceMCD = 80;
        const int mapRefWidthUnit = mapReferenceWidth / mapReferenceMCD;
        const int mapRefHeightUnit = mapReferenceHeight / mapReferenceMCD;
        const int mapReferenceMultiplier = 25;
        const float layerOriginX = 0.2639999f;
        const float layerOriginY = 0.5899986f;
        const float startingWidth = 0.1622246f;
        const float startingHeight = 0.1943558f;
        const float widthProportion = 0.2575f;
        const float heightProportion = 0.3085f;
        const float proportionMultiplier = 1.5f;

        public static PixelData pixelData;
        public PixelData modifiedPixelData;
        public Vector2 pixelCoordinates;
        public static bool pixelSelected = false;
        public bool exteriorContent = false;
        public bool dungeonContent = false;
        public bool buildingList = false;
        public bool blockList = false;
        public bool dungeonModified = false;
        public bool widthModified = false;
        public bool heightModified = false;
        public int width = 0;
        public int height = 0;
        public Vector2 buildingScroll;
        public Vector2 townBlocksScroll;
        public Vector2 dungeonScroll;
        public Vector2 blockScroll;
        public string[] regionNames;
        public string[] climateNames;
        public static readonly string[] locationTypes = {
            "City", "Hamlet", "Village", "Farm", "Dungeon Labyrinth", "Temple", "Tavern", "Dungeon Keep", "Wealthy Home", "Cult", "Dungeon Ruin", "Home Poor", "Graveyard", "Coven"
        };

        public static readonly string[] dungeonTypes = {
            "Crypt", "Orc Stronghold", "Human Stronghold", "Prison", "Desecrated Temple", "Mine", "Natural Cave", "Coven", "Vampire Haunt", "Laboratory", "Harpy Nest", "Ruined Castle", "Spider Nest", "Giant Stronghold", "Dragon's Den", "Barbarian Stronghold", "Volcanic Caves", "Scorpion Nest", "Cemetery", "No Dungeon"
        };

        public static readonly string[] buildingTypes = {
            "Alchemist", "House for Sale", "Armorer", "Bank", "Town4", "Bookseller", "Clothing Store", "Furniture Store", "Gem Store", "General Store", "Library", "Guild Hall", "Pawn Shop", "Weapon Smith", "Temple", "Tavern", "Palace", "House1", "House2", "House3", "House4", "House5", "House6"
        };

        public readonly string[] rmbBlockPrefixes = {
            "TVRN", "GENR", "RESI", "WEAP", "ARMR", "ALCH", "BANK", "BOOK",
            "CLOT", "FURN", "GEMS", "LIBR", "PAWN", "TEMP", "TEMP", "PALA",
            "FARM", "DUNG", "CAST", "MANR", "SHRI", "RUIN", "SHCK", "GRVE",
            "FILL", "KRAV", "KDRA", "KOWL", "KMOO", "KCAN", "KFLA", "KHOR",
            "KROS", "KWHE", "KSCA", "KHAW", "MAGE", "THIE", "DARK", "FIGH",
            "CUST", "WALL", "MARK", "SHIP", "WITC"
        };

        private readonly string[] rdbBlockLetters = { "N", "W", "L", "S", "B", "M" };

        private readonly string[] elevationIndex = { "-1", "0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "+" };

        public static BlocksFile blockFileReader;
        public string[] RMBBlocks;
        public string[] RDBBlocks;
        // public string[] RMBBlocksByPrefix;

        public string[] townBlocks;

        public string worldSavePath;
        public string sourceFilesPath;
        public const string testPath = "/home/arneb/Games/daggerfall/DaggerfallGameFiles/arena2/Maps/Tamriel/";
        public const string arena2Path = "/home/arneb/Games/daggerfall/DaggerfallGameFiles/arena2";
        public int lastDungeonType = -1;
        public int availableBlocks = 0;
        public int availableDBlocks = 0;
        public int lastAvailable = -1;
        public int availablePrefixes = 0;
        public int lastAvailableLetter = -1;
        public int availableLetters = 0;
        public int selectedX = 0;
        public int selectedY = 0;
        public int selectedCoordinates = 0;
        public int pointedX = 0;
        public int pointedZ = 0;
        public (int, int) pointedCoordinates = (0, 0);
        public int xCoord = 0;
        public int yCoord = 0;
        public int xTile = 0;
        public int yTile = 0;
        // public int dungeonMinX = 0;
        // public int dungeonMaxX = 0;
        // public int dungeonMinZ = 0;
        // public int dungeonMaxZ = 0;
        public List<string> modifiedTownBlocks = new List<string>();
        public List<Blocks> modifiedDungeonBlocks = new List<Blocks>();
        public List<int> modifiedTiles = new List<int>();
        public static void ShowWindow() 
        {
            EditorWindow.GetWindow(typeof(MapEditor));
        }

        void OnEnable()
        {

        }

        void OnDisable()
        {

        }

        void OnValidate()
        {
            Debug.Log("Setting map alpha channel");
        }

        void OnBackingScaleFactorChanged()
        {
            layerPosition = UpdateLayerPosition();
            Graphics.DrawTexture(mapView, referenceMap, layerPosition, 0, 0, 0, 0);
        }

        [MenuItem(menuPath)]
        static void Init()
        {
            window = (MapEditor)EditorWindow.GetWindow(typeof(MapEditor));
            window.titleContent = new GUIContent(windowTitle);
        }

        void Update()
        {

        }

        public void LoadMapFile(string path)
        {
            
        }

        void Awake()
        {
            guiStyle = SetGUIStyle();

            drawnBuffer = new List<(int, int)>();
            heightmapByteArray = new byte[0];

            SetHeightmapRect();

            SetMaps();

            if (!File.Exists(Path.Combine(testPath, "Trails.png")))
            {
                byte[,] tempEmptyTrails = new byte[MapsFile.MaxMapPixelX, MapsFile.MaxMapPixelY];
                string fileDataPath = Path.Combine(testPath, "Trails.png");
                byte[] trailsByteArray = ConvertToGrayscale(tempEmptyTrails);
                File.WriteAllBytes(fileDataPath, trailsByteArray);
            }

            SetTrailsMap();
            SetLocationsMap();

            SetRegionNames();
            SetClimateNames();
            ResetSelectedCoordinates();

            if (blockFileReader == null)
                blockFileReader = new BlocksFile(Path.Combine(arena2Path, BlocksFile.Filename), FileUsage.UseMemory, true);

            pixelData = new PixelData();
            modifiedPixelData = new PixelData();

            string path = "Assets/Scripts/Editor/3170483-1327182302.jpg";
            referenceMap = new Texture2D(3440, 2400);
            referenceMap = new Texture2D(referenceMap.width, referenceMap.height, TextureFormat.ARGB32, false, true);
            ImageConversion.LoadImage(referenceMap, File.ReadAllBytes(path));
            layerPosition = new Rect(layerOriginX, layerOriginY, startingWidth, startingHeight);
            heightmapRect = new Rect(layerOriginX, layerOriginY, startingWidth, startingHeight);
        }

        void OnGUI()
        {
            drawnBufferCount = drawnBuffer.Count;

            GUILayout.Label("Base Settings", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            worldName = EditorGUILayout.TextField("World Name", worldName, GUILayout.MaxWidth(dataFieldBig));
            if (GUILayout.Button("Open Region Manager", GUILayout.MaxWidth(dataFieldSmall)))
            {
                OpenRegionManager();
            }

            if (GUILayout.Button("Open Faction Manager", GUILayout.MaxWidth(dataFieldSmall)))
            {
                OpenFactionManager();
            }

            if (GUILayout.Button("Create Large Heightmap", GUILayout.MaxWidth(dataFieldSmall)))
            {
                CreateLargeHeightmap();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Create MapDict", GUILayout.MaxWidth(dataFieldSmall)))
            {
                CreateMapDict();
            }

            if (GUILayout.Button("Create NameGen", GUILayout.MaxWidth(dataFieldSmall)))
            {
                CreateNameGen();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Create Tiles", GUILayout.MaxWidth(dataFieldSmall)))
            {
                OpenTileCreator();
            }

            if (GUILayout.Button("Split Locations", GUILayout.MaxWidth(dataFieldSmall)))
            {
                SplitLocations();
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Set Location Keys", GUILayout.MaxWidth(dataFieldSmall)))
            {
                SetLocationKeys();
            }

            EditorGUILayout.EndHorizontal();
            // if (GUILayout.Button("Set as current world", GUILayout.MaxWidth(200)))
            // {
            //     SetCurrentWorld();
            // }

            groupEnabled = EditorGUILayout.BeginToggleGroup("Unlock map editing", groupEnabled);

            Rect r = EditorGUILayout.BeginHorizontal(GUILayout.Height(30));
            heightmap = EditorGUILayout.ToggleLeft("Heightmap", heightmap);
            climate = EditorGUILayout.ToggleLeft("Climate", climate);
            politics = EditorGUILayout.ToggleLeft("Politics", politics);
            locations = EditorGUILayout.ToggleLeft("Locations", locations);
            mapImage = EditorGUILayout.ToggleLeft("Map Reference", mapImage);
            EditorGUILayout.EndHorizontal();

            r = EditorGUILayout.BeginHorizontal(GUILayout.Height(MapsFile.MaxMapPixelY / 40 * 3));
            mapView = EditorGUILayout.GetControlRect(false, 0.0f, GUILayout.Width(MapsFile.MaxMapPixelX / 40 * 3), GUILayout.Height(MapsFile.MaxMapPixelY / 40 * 3));
            mapView.x += 5.0f;

            if (mapImage)
            {
                Graphics.DrawTexture(mapView, referenceMap, layerPosition, 0, 0, 0, 0);
            }

            if (heightmap)
            {
                Graphics.DrawTexture(mapView, heightMap, mapRect, 0, 0, 0, 0);
            }

            if (climate)
            {
                Graphics.DrawTexture(mapView, climateMap, mapRect, 0, 0, 0, 0);
            }

            if (politics)
            {
                Graphics.DrawTexture(mapView, politicMap, mapRect, 0, 0, 0, 0);
            }

            if (locations)
            {
                Graphics.DrawTexture(mapView, locationsMap, mapRect, 0, 0, 0, 0);
            }

            if (trails)
            {
                Graphics.DrawTexture(mapView, trailsMap, mapRect, 0, 0, 0, 0);
            }

            if (drawHeightmap)
            {

            }

            if (drawClimate)
            {
                
            }

            if (drawPolitics)
            {
                
            }

            if (mousePosition == null)
                mousePosition = Vector2.zero;

            mousePosition = GetMouseCoordinates();

            float tempMapAlphaChannel = SetMapAlphaChannel();
            if (mapAlphaChannel != tempMapAlphaChannel)
            {
                mapAlphaChannel = tempMapAlphaChannel;
                SetMaps();
            }

            EditorGUILayout.BeginVertical();

            if (!pixelSelected)
            {
                string mousePos = ((int)mousePosition.x).ToString() + ", " + ((int)mousePosition.y).ToString();
                EditorGUILayout.LabelField("Coordinates: ", mousePos);

                if (pixelData.hasLocation)
                {
                    EditorGUILayout.LabelField("Location Name: ", pixelData.Name);
                    EditorGUILayout.LabelField("Region Name: ", pixelData.RegionName);
                    EditorGUILayout.LabelField("Map ID: ", pixelData.MapId.ToString());
                    EditorGUILayout.LabelField("Latitude: ", pixelData.Latitude.ToString());
                    EditorGUILayout.LabelField("Longitude", pixelData.Longitude.ToString());
                    EditorGUILayout.LabelField("Location Type: ", ((DFRegion.LocationTypes)pixelData.LocationType).ToString());
                    EditorGUILayout.LabelField("Dungeon Type: ", ((DFRegion.DungeonTypes)pixelData.DungeonType).ToString());
                    EditorGUILayout.LabelField("Key: ", pixelData.Key.ToString());
                    EditorGUILayout.LabelField("Politic: ", pixelData.Politic.ToString());
                    EditorGUILayout.LabelField("Region Index: ", pixelData.RegionIndex.ToString());
                    EditorGUILayout.LabelField("Location Index: ", pixelData.LocationIndex.ToString());
                }

                EditorGUILayout.LabelField("Elevation: ", pixelData.Elevation.ToString());

                if (pixelData.Region == -1)
                    EditorGUILayout.LabelField("Region: ", "Water body");
                else 
                {
                    EditorGUILayout.LabelField("Region: ", regionNames[pixelData.Region]);
                }
                
                EditorGUILayout.LabelField("Climate: ", climateNames[pixelData.Climate]);

                EditorGUILayout.Space(100.0f);

                EditorGUILayout.BeginHorizontal();
                xCoord = EditorGUILayout.IntField("xCoord: ", xCoord);
                yCoord = EditorGUILayout.IntField("yCoord: ", yCoord);
                xTile = xCoord / MapsFile.TileDim;
                yTile = yCoord / MapsFile.TileDim;
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Tile ", xTile.ToString());
                EditorGUILayout.LabelField(", ", yTile.ToString());
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Relative Coordinates: ", (xCoord % MapsFile.TileDim).ToString());
                EditorGUILayout.LabelField(", ", (yCoord % MapsFile.TileDim).ToString());
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Generate Routes", GUILayout.MaxWidth(200.0f)))
                {
                    OpenGenerateRoutesWindow();
                }
                EditorGUILayout.EndHorizontal();
            }

            if (pixelSelected)
            {
                string mousePos = ((int)pixelCoordinates.x).ToString() + ", " + ((int)pixelCoordinates.y).ToString();
                EditorGUILayout.LabelField("Coordinates: ", mousePos.ToString());

                if (modifiedPixelData.hasLocation)
                {
                    modifiedPixelData.Name = EditorGUILayout.TextField("Location Name: ", modifiedPixelData.Name, GUILayout.MaxWidth(dataField));
                    if (modifiedPixelData.RegionName == "")
                        modifiedPixelData.RegionName = WorldInfo.WorldSetting.RegionNames[modifiedPixelData.Region];

                    EditorGUILayout.LabelField("Region Name: ", modifiedPixelData.RegionName);
                    EditorGUILayout.LabelField("Map ID: ", modifiedPixelData.MapId.ToString());
                    EditorGUILayout.LabelField("Latitude: ", modifiedPixelData.Latitude.ToString());
                    EditorGUILayout.LabelField("Longitude", modifiedPixelData.Longitude.ToString());
                    modifiedPixelData.LocationType = EditorGUILayout.Popup("Location Type: ", modifiedPixelData.LocationType, locationTypes, GUILayout.MaxWidth(dataField));
                    modifiedPixelData.DungeonType = EditorGUILayout.Popup("Dungeon Type: ", modifiedPixelData.DungeonType, dungeonTypes, GUILayout.MaxWidth(dataField));
                    if (lastDungeonType != modifiedPixelData.DungeonType)
                    {
                        if (modifiedPixelData.DungeonType == 255 || modifiedPixelData.DungeonType >= Enum.GetNames(typeof(DFRegion.DungeonTypes)).Length - 1)
                            modifiedPixelData.dungeon.BlockCount = 0;
                        else modifiedPixelData.dungeon = SetNewDungeon();

                        lastDungeonType = modifiedPixelData.DungeonType;
                    }
                    EditorGUILayout.LabelField("Key: ", modifiedPixelData.Key.ToString());

                    exteriorContent = EditorGUILayout.Foldout(exteriorContent, "Exterior Data");
                    if (exteriorContent)
                    {
                        dungeonContent = false;
                        EditorGUILayout.LabelField("X: ", modifiedPixelData.exterior.X.ToString());
                        EditorGUILayout.LabelField("Y: ", modifiedPixelData.exterior.Y.ToString());
                        EditorGUILayout.LabelField("Location ID: ", modifiedPixelData.exterior.LocationId.ToString());
                        EditorGUILayout.LabelField("Exterior Location ID: ", modifiedPixelData.exterior.ExteriorLocationId.ToString());
                        modifiedPixelData.exterior.AnotherName = EditorGUILayout.TextField("Another Name: ", modifiedPixelData.exterior.AnotherName);
                        EditorGUILayout.LabelField("Building Count: ", modifiedPixelData.exterior.BuildingCount.ToString());

                        if (!widthModified)
                        {
                            width = EditorGUILayout.IntSlider("Width: ", modifiedPixelData.exterior.Width, 1, 8, GUILayout.MaxWidth(dataField));
                            widthModified = true;
                        }
                        width = EditorGUILayout.IntSlider("Width: ", width, 1, 8, GUILayout.MaxWidth(dataField));

                        if (!heightModified)
                        {
                            height = EditorGUILayout.IntSlider("Height: ", modifiedPixelData.exterior.Height, 1, 8, GUILayout.MaxWidth(dataField));
                            heightModified = true;
                        }
                        height = EditorGUILayout.IntSlider("Height: ", height, 1, 8, GUILayout.MaxWidth(dataField));

                        modifiedPixelData.exterior.PortTown = EditorGUILayout.Toggle("Port Town", modifiedPixelData.exterior.PortTown);

                        townBlocks = new string[modifiedPixelData.exterior.Width * modifiedPixelData.exterior.Height];
                        townBlocks = modifiedPixelData.exterior.BlockNames;

                        if (modifiedTownBlocks.Count <= 0)
                            modifiedTownBlocks = townBlocks.ToList();
                        
                        if (modifiedTownBlocks.Count != (width * height))
                            modifiedTownBlocks = SetListCount(modifiedTownBlocks, width, height);

                        int counter = 0;
                        int offset = 0;
                        for (int i = 0; i < (width * height); i++)
                        {
                            if (counter >= modifiedPixelData.exterior.Width && width > modifiedPixelData.exterior.Width)
                            {
                                // ResetSelectedCoordinates();
                                counter = width - modifiedPixelData.exterior.Width;
                                do
                                {
                                    if (modifiedTownBlocks[i + offset] == null)
                                        modifiedTownBlocks[i + offset] = noBlock;
                                    // if (modifiedTownBlocks.ContainsKey(i + offset))
                                    //     modifiedTownBlocks.Remove(i + offset);
                                    // modifiedTownBlocks.Add((i + offset), noBlock);
                                    counter--;
                                    offset++;
                                }
                                while (counter > 0);
                            }
                            counter++;

                            if (i < modifiedPixelData.exterior.BlockNames.Length && modifiedTownBlocks.Count > (i + offset) && modifiedTownBlocks[i + offset] == null)
                            {
                                // if (modifiedTownBlocks.Count >= (i + offset))
                                // {
                                //     Debug.Log("modifiedTownBlocks contains key " + (i + offset) + " of value " + modifiedTownBlocks[i + offset]);
                                //     string modTownBlock;
                                //     modTownBlock = modifiedTownBlocks[i + offset];
                                //     modifiedTownBlocks.Insert((i + offset), modTownBlock);
                                // }
                                // else 
                                modifiedTownBlocks[i + offset] = modifiedPixelData.exterior.BlockNames[i];
                            }
                            else if (modifiedTownBlocks.Count <= (i + offset))
                                break;
                            else
                            {
                                for (int j = (i + offset); j < modifiedTownBlocks.Count; j++)
                                {
                                    if (modifiedTownBlocks[j] == null)
                                        modifiedTownBlocks[j] = noBlock;
                                    // if (modifiedTownBlocks.ContainsKey(j))
                                    //     modifiedTownBlocks.Remove(j);
                                    // modifiedTownBlocks.Add(j, noBlock);
                                }
                                break;
                            }
                        }

                        // Debug.Log("townBlocks.Length: " + townBlocks.Length);
                        // for (int i = 0; i < townBlocks.Length; i++)
                        // {                
                        //     if (modifiedTownBlocks.ContainsKey(i))
                        //         modifiedTownBlocks.Remove(i);     
                        //     Debug.Log("Adding block " + townBlocks[i]);
                        //     modifiedTownBlocks. Add(i, townBlocks[i]);
                        // }

                        EditorGUILayout.LabelField("Block Names: ");

                        townBlocksScroll = EditorGUILayout.BeginScrollView(townBlocksScroll, GUILayout.Height(100));

                        int column = 0;
                        int index = 0;
                        int row = (height - 1);

                        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(dataField));
                        for (int i = 0; i < modifiedTownBlocks.Count; i++)
                        {
                            if (i != 0 && (i % width) == 0)
                            {
                                EditorGUILayout.EndHorizontal();
                                row--;
                                column = 0;
                                EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(dataField));
                            }

                            index = column + (width * row);
                            if (index == selectedCoordinates)
                                EditorGUILayout.LabelField(modifiedTownBlocks[index], EditorStyles.whiteBoldLabel);
                            else EditorGUILayout.LabelField(modifiedTownBlocks[index]);
                            column++;
                        }

                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndScrollView();

                        EditorGUILayout.BeginHorizontal();
                        availablePrefixes = EditorGUILayout.Popup("Prefix: ", availablePrefixes, rmbBlockPrefixes, GUILayout.MaxWidth(dataField));

                        if (availablePrefixes != lastAvailable)
                        {
                            RMBBlocks = SetBlocks(availablePrefixes, true);
                            lastAvailable = availablePrefixes;
                        }
                        
                        availableBlocks = EditorGUILayout.Popup("RMB: ", availableBlocks, RMBBlocks, GUILayout.MaxWidth(dataField));
                        EditorGUILayout.EndHorizontal();

                        EditorGUILayout.BeginHorizontal();
                        selectedX = EditorGUILayout.IntSlider("Set to block X:", selectedX, 1, width, GUILayout.MaxWidth(dataField));
                        selectedY = EditorGUILayout.IntSlider(" Y:", selectedY, 1, height, GUILayout.MaxWidth(dataField));
                        selectedCoordinates = (selectedX - 1) + (height - selectedY) * width;

                        if (GUILayout.Button("Set Block", GUILayout.MaxWidth(dataFieldSmall)))
                        {
                            // Debug.Log("townBlocks.Length: " + townBlocks.Length);
                            // modifiedTownBlocks = new Dictionary<int, string>();
                            // for (int i = 0; i < townBlocks.Length; i++)
                            // {
                            //     modifiedTownBlocks.Add(i, townBlocks[i]);
                            // }

                            modifiedTownBlocks.RemoveAt(selectedCoordinates); 
                            modifiedTownBlocks.Insert(selectedCoordinates, RMBBlocks[availableBlocks]);

                            // Debug.Log("modifiedTownBlocks.Count: " + modifiedTownBlocks.Count);
                            // for (int i = 0; i < modifiedTownBlocks.Count; i++)
                            // {
                            //     Debug.Log("Working on key n. " + i);
                            //     if (modifiedTownBlocks.ContainsKey(i))
                            //         modifiedTownBlocksString.Add(modifiedTownBlocks[i]);
                            // }

                            modifiedPixelData.exterior.BuildingCount = GetBuildingCount(width, height, modifiedTownBlocks);
                            modifiedPixelData.exterior.buildings = new Buildings[modifiedPixelData.exterior.BuildingCount];
                            modifiedPixelData.exterior.buildings = SetBuildings(modifiedPixelData, modifiedTownBlocks);
                            // townBlocks = modifiedTownBlocksString.ToArray();
                        }
                        EditorGUILayout.EndHorizontal();

                        if (modifiedPixelData.exterior.BuildingCount > 0)
                        {
                            buildingList = EditorGUILayout.Foldout(buildingList, "Building List");
                            if (buildingList)
                            {
                                buildingScroll = EditorGUILayout.BeginScrollView(buildingScroll);
                                for (int building = 0; building < modifiedPixelData.exterior.BuildingCount; building++)
                                {
                                    modifiedPixelData.exterior.buildings[building].NameSeed = EditorGUILayout.IntField("Name Seed: ", modifiedPixelData.exterior.buildings[building].NameSeed, GUILayout.MaxWidth(dataField));

                                    EditorGUILayout.BeginHorizontal();
                                    modifiedPixelData.exterior.buildings[building].FactionId = EditorGUILayout.IntField("Faction ID: ", modifiedPixelData.exterior.buildings[building].FactionId, GUILayout.MaxWidth(dataField));
                                    EditorGUILayout.LabelField(" ", ((FactionFile.FactionIDs)modifiedPixelData.exterior.buildings[building].FactionId).ToString());
                                    EditorGUILayout.EndHorizontal();
                                    EditorGUILayout.LabelField("Sector: ", modifiedPixelData.exterior.buildings[building].Sector.ToString());
                                    modifiedPixelData.exterior.buildings[building].BuildingType = EditorGUILayout.Popup("Building Type: ", modifiedPixelData.exterior.buildings[building].BuildingType, buildingTypes, GUILayout.MaxWidth(dataField));
                                    modifiedPixelData.exterior.buildings[building].Quality = EditorGUILayout.IntSlider("Quality: ", modifiedPixelData.exterior.buildings[building].Quality, 1, 20, GUILayout.MaxWidth(dataField));
                                    EditorGUILayout.Space();
                                }
                                EditorGUILayout.EndScrollView();
                            }
                        }
                    }

                    if (modifiedPixelData.dungeon.BlockCount > 0)
                    {
                        dungeonContent = EditorGUILayout.Foldout(dungeonContent, "DungeonData");
                        if (dungeonContent)
                        {
                            exteriorContent = false;
                            modifiedPixelData.dungeon.DungeonName = EditorGUILayout.TextField("Dungeon Name: ", modifiedPixelData.dungeon.DungeonName);
                            EditorGUILayout.LabelField("X: ", modifiedPixelData.dungeon.X.ToString());
                            EditorGUILayout.LabelField("Y: ", modifiedPixelData.dungeon.Y.ToString());
                            EditorGUILayout.LabelField("Location ID: ", modifiedPixelData.dungeon.LocationId.ToString());
                            EditorGUILayout.LabelField("Block Count: ", modifiedPixelData.dungeon.BlockCount.ToString());

                            if (modifiedDungeonBlocks.Count <= 0)
                                modifiedDungeonBlocks = modifiedPixelData.dungeon.blocks.ToList();

                            modifiedPixelData.dungeon.BlockCount = modifiedDungeonBlocks.Count;
                            modifiedPixelData.dungeon.blocks = modifiedDungeonBlocks.ToArray();

                            (int, int) x = (0, 0);
                            (int, int) z = (0, 0);
                            foreach (Blocks blockElement in modifiedDungeonBlocks)
                            {
                                if (blockElement.X < x.Item1)
                                    x.Item1 = blockElement.X;
                                if (blockElement.X > x.Item2)
                                    x.Item2 = blockElement.X;
                                if (blockElement.Z < z.Item1)
                                    z.Item1 = blockElement.Z;
                                if (blockElement.Z > z.Item2)
                                    z.Item2 = blockElement.Z;
                            }
                            EditorGUILayout.LabelField("Block Names: ");

                            dungeonScroll = EditorGUILayout.BeginScrollView(dungeonScroll, GUILayout.Height(100));
                            EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(dataField));

                            for (int Z = z.Item1; Z <= z.Item2; Z++)
                            {
                                for (int X = x.Item1; X <= x.Item2; X++)
                                {
                                    Blocks currentBlock = new Blocks();
                                    bool[] border = {false, false, false, false}; //0: top, 1: bottom, 2: left, 3: right
                                    if (Z == z.Item1 || !(modifiedDungeonBlocks.Exists(blk => (blk.X, blk.Z) == (X , Z - 1))))
                                        border[0] = true;
                                    else if (Z == z.Item2 || !(modifiedDungeonBlocks.Exists(blk => (blk.X, blk.Z) == (X, Z + 1))))
                                        border[1] = true;
                                    
                                    if (X == x.Item1 || !(modifiedDungeonBlocks.Exists(blk => (blk.X, blk.Z) == (X - 1, Z))))
                                        border[2] = true;
                                    else if (X == x.Item2|| !(modifiedDungeonBlocks.Exists(blk => (blk.X, blk.Z) == (X + 1, Z))))
                                        border[3] = true;
                                    
                                    bool found = false;
                                    for (int block = 0; block < modifiedDungeonBlocks.Count; block++)
                                    {
                                        if (modifiedDungeonBlocks[block].X == X && modifiedDungeonBlocks[block].Z == Z)
                                        {
                                            currentBlock = modifiedDungeonBlocks[block];
                                            if (pointedCoordinates == (X, Z))
                                                EditorGUILayout.LabelField(currentBlock.BlockName, EditorStyles.whiteBoldLabel);
                                            else EditorGUILayout.LabelField(currentBlock.BlockName);

                                            found = true;
                                        }
                                    }

                                    if (found && currentBlock.BlockName.StartsWith("B"))
                                    {
                                        char initial = 'B';
                                        if ((modifiedDungeonBlocks.Exists(blk => (blk.X, blk.Z, blk.BlockName[0]) == (X, Z - 1, initial)) || !modifiedDungeonBlocks.Exists(blk => (blk.X, blk.Z) == (X, Z - 1))) &&
                                            (modifiedDungeonBlocks.Exists(blk => (blk.X, blk.Z, blk.BlockName[0]) == (X, Z + 1, initial)) || !modifiedDungeonBlocks.Exists(blk => (blk.X, blk.Z) == (X, Z + 1))) &&
                                            (modifiedDungeonBlocks.Exists(blk => (blk.X, blk.Z, blk.BlockName[0]) == (X - 1, Z, initial)) || !modifiedDungeonBlocks.Exists(blk => (blk.X, blk.Z) == (X - 1, Z))) &&
                                            (modifiedDungeonBlocks.Exists(blk => (blk.X, blk.Z, blk.BlockName[0]) == (X + 1, Z, initial)) || !modifiedDungeonBlocks.Exists(blk => (blk.X, blk.Z) == (X + 1, Z))))
                                            modifiedDungeonBlocks.Remove(currentBlock);
                                    }

                                    if (found && !currentBlock.BlockName.StartsWith("B"))
                                    {
                                        if (border[0])
                                        {
                                            modifiedDungeonBlocks.Add(RandomBorderBlock(Z - 1, X));
                                            // modifiedPixelData.dungeon.BlockCount++;
                                        }

                                        if (border[1])
                                        {
                                            modifiedDungeonBlocks.Add(RandomBorderBlock(Z + 1, X));
                                            // modifiedPixelData.dungeon.BlockCount++;
                                        }

                                        if (border[2])
                                        {
                                            modifiedDungeonBlocks.Add(RandomBorderBlock(Z, X - 1));
                                            // modifiedPixelData.dungeon.BlockCount++;
                                        }

                                        if (border[3])
                                        {
                                            modifiedDungeonBlocks.Add(RandomBorderBlock(Z, X + 1));
                                            // modifiedPixelData.dungeon.BlockCount++;
                                        }
                                    }

                                    if (!found)
                                        EditorGUILayout.LabelField(noBlock);

                                    if (X == x.Item2)
                                    {
                                        EditorGUILayout.EndHorizontal();
                                        EditorGUILayout.BeginHorizontal(GUILayout.MaxWidth(dataField));
                                    }
                                }
                            }
                            EditorGUILayout.EndHorizontal();
                            EditorGUILayout.EndScrollView();

                            EditorGUILayout.BeginHorizontal();
                            availableLetters = EditorGUILayout.Popup("Letter: ", availableLetters, rdbBlockLetters, GUILayout.MaxWidth(dataField));

                            if (availableLetters != lastAvailableLetter)
                            {
                                RDBBlocks = SetBlocks(availableLetters, false);
                                lastAvailableLetter = availableLetters;
                            }

                            availableDBlocks = EditorGUILayout.Popup("RDB: ", availableDBlocks, RDBBlocks, GUILayout.MaxWidth(dataField));
                            EditorGUILayout.EndHorizontal();

                            EditorGUILayout.BeginHorizontal();
                            pointedX = EditorGUILayout.IntSlider("Set to block X:", pointedX, x.Item1, x.Item2, GUILayout.MaxWidth(dataField));
                            pointedZ = EditorGUILayout.IntSlider(" Z:", pointedZ, z.Item1, z.Item2, GUILayout.MaxWidth(dataField));
                            pointedCoordinates = (pointedX, pointedZ);

                            if (GUILayout.Button("Set Block", GUILayout.MaxWidth(dataFieldSmall)))
                            {
                                int blockIndex = GetDBlockIndex();
                                modifiedDungeonBlocks.RemoveAt(blockIndex);
                                modifiedDungeonBlocks.Insert(blockIndex, GetDungeonBlockData());

                                modifiedPixelData.dungeon.BlockCount = modifiedDungeonBlocks.Count;
                                modifiedPixelData.dungeon.blocks = new Blocks[modifiedDungeonBlocks.Count];
                                modifiedPixelData.dungeon.blocks = modifiedDungeonBlocks.ToArray();
                            }
                            EditorGUILayout.EndHorizontal();

                            blockList = EditorGUILayout.Foldout(blockList, "Block List");
                            if (blockList)
                            {
                                blockScroll = EditorGUILayout.BeginScrollView(blockScroll);

                                for (int Z = z.Item1; Z <= z.Item2; Z++)
                                {
                                    for (int X = x.Item1; X <= x.Item2; X++)
                                    {
                                        for (int block = 0; block < modifiedDungeonBlocks.Count; block++)
                                        {
                                            if (modifiedDungeonBlocks[block].X == X && modifiedDungeonBlocks[block].Z == Z)
                                            {
                                                EditorGUILayout.LabelField("X: ", (modifiedDungeonBlocks[block].X).ToString());
                                                EditorGUILayout.LabelField("Z: ", (modifiedDungeonBlocks[block].Z).ToString());
                                                modifiedDungeonBlocks[block].IsStartingBlock = EditorGUILayout.Toggle("Starting Block: ", modifiedDungeonBlocks[block].IsStartingBlock);
                                                EditorGUILayout.LabelField("Block Name: ", modifiedDungeonBlocks[block].BlockName);
                                                modifiedDungeonBlocks[block].WaterLevel = EditorGUILayout.IntField("Water Level: ", modifiedDungeonBlocks[block].WaterLevel);
                                                modifiedDungeonBlocks[block].CastleBlock = EditorGUILayout.Toggle("Castle Block: ", modifiedDungeonBlocks[block].CastleBlock);
                                                EditorGUILayout.Space();
                                            }
                                        }
                                    }
                                }
                                EditorGUILayout.EndScrollView();
                            }
                        }
                    }
                }

                if (!modifiedPixelData.hasLocation)
                    if (GUILayout.Button("Create location", GUILayout.MaxWidth(100)))
                    {
                        SetNewLocation();
                    }

                modifiedPixelData.Elevation = EditorGUILayout.IntField("Elevation: ", modifiedPixelData.Elevation, GUILayout.MaxWidth(dataField));
                modifiedPixelData.Region = EditorGUILayout.Popup("Region: ", modifiedPixelData.Region, regionNames, GUILayout.MaxWidth(dataField));

                modifiedPixelData.Climate = EditorGUILayout.Popup("Climate: ", modifiedPixelData.Climate, climateNames, GUILayout.MaxWidth(dataField));

                EditorGUILayout.BeginHorizontal(GUILayout.Height(30));
                if (GUILayout.Button("Apply Changes", GUILayout.MaxWidth(100)))
                {
                    modifiedPixelData.exterior.Width = width;
                    modifiedPixelData.exterior.Height = height;

                    modifiedPixelData.exterior.BlockNames = new string[modifiedPixelData.exterior.Width * modifiedPixelData.exterior.Height];
                    if ((modifiedPixelData.exterior.Width * modifiedPixelData.exterior.Height) > 0)
                    {
                        for (int i = 0; i < (modifiedPixelData.exterior.Width * modifiedPixelData.exterior.Height); i++)
                        {
                            modifiedPixelData.exterior.BlockNames[i] = modifiedTownBlocks[i];
                        }
                    }
                    ApplyChanges();
                    if (!modifiedTiles.Contains(Worldmaps.GetTileIndex(modifiedPixelData.MapId)))
                        modifiedTiles.Add(Worldmaps.GetTileIndex(modifiedPixelData.MapId));
                    SetMaps();
                }

                if (GUILayout.Button("Revert Changes", GUILayout.MaxWidth(100)))
                {
                    modifiedPixelData.GetPixelData((int)pixelCoordinates.x, (int)pixelCoordinates.y);
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            r = EditorGUILayout.BeginHorizontal(GUILayout.Height(30));

            EditorGUILayout.BeginVertical();
            drawHeightmap = EditorGUILayout.ToggleLeft("Draw Heightmap Mode", drawHeightmap);
            if (drawHeightmap)
            {                
                colourIndex = EditorGUILayout.Popup("Elevation: ", colourIndex, elevationIndex, GUILayout.MaxWidth(dataFieldSmall));
                paintBrush = GetElevationColour(colourIndex - 1);

                if (GUILayout.Button("Apply", GUILayout.MaxWidth(dataFieldSmall)))
                {
                    foreach ((int, int) modifiedElevationPixel in drawnBuffer)
                    {
                        int elevation = 0;
                        if (colourIndex == 0)
                        {
                            elevation = UnityEngine.Random.Range(0, 3);
                        }
                        else if (colourIndex == 1)
                        {
                            elevation = UnityEngine.Random.Range(4, 10);
                        }
                        else if (colourIndex == elevationIndex.Length - 1)
                        {
                            elevation = UnityEngine.Random.Range((colourIndex - 1) * 10, 256);
                        }
                        else
                            elevation = UnityEngine.Random.Range((colourIndex - 1) * 10, colourIndex * 10);

                        SmallHeightmap.Woods[modifiedElevationPixel.Item1, modifiedElevationPixel.Item2] = (byte)elevation;
                    }
                    drawnBuffer = new List<(int, int)>();
                }

                if (GUILayout.Button("Save Changes", GUILayout.MaxWidth(dataFieldSmall)))
                {
                    string fileDataPath = Path.Combine(testPath, "Woods.json");
                    var json = JsonConvert.SerializeObject(SmallHeightmap.Woods, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    File.WriteAllText(fileDataPath, json);
                }

                if (GUILayout.Button("Save Image", GUILayout.MaxWidth(dataFieldSmall)))
                {
                    string fileDataPath = Path.Combine(testPath, "Woods.png");
                    heightmapByteArray = ConvertToGrayscale(SmallHeightmap.Woods);
                    // var json = JsonConvert.SerializeObject(heightmapByteArray, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    File.WriteAllBytes(fileDataPath, heightmapByteArray);
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            drawClimate = EditorGUILayout.ToggleLeft("Draw Climate Mode", drawClimate);
            if (drawClimate)
            {                
                colourIndex = EditorGUILayout.Popup("Climate: ", colourIndex, Enum.GetNames(typeof(MapsFile.Climates)), GUILayout.MaxWidth(dataFieldSmall));
                paintBrush = GetClimateColour(colourIndex + (int)MapsFile.Climates.Ocean);

                if (GUILayout.Button("Apply", GUILayout.MaxWidth(dataFieldSmall)))
                {
                    foreach ((int, int) modifiedClimatePixel in drawnBuffer)
                    {
                        ClimateInfo.Climate[modifiedClimatePixel.Item1, modifiedClimatePixel.Item2] = colourIndex;
                    }
                    drawnBuffer = new List<(int, int)>();
                }

                if (GUILayout.Button("Save Changes", GUILayout.MaxWidth(dataFieldSmall)))
                {
                    string fileDataPath = Path.Combine(testPath, "Climate.json");
                    var json = JsonConvert.SerializeObject(ClimateInfo.Climate, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    File.WriteAllText(fileDataPath, json);
                }

                if (GUILayout.Button("Save Image", GUILayout.MaxWidth(dataFieldSmall)))
                {
                    string fileDataPath = Path.Combine(testPath, "Climate.png");
                    heightmapByteArray = ConvertToGrayscale(ClimateInfo.Climate);
                    // var json = JsonConvert.SerializeObject(heightmapByteArray, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    File.WriteAllBytes(fileDataPath, heightmapByteArray);
                }
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            drawPolitics = EditorGUILayout.ToggleLeft("Draw Politics Mode", drawPolitics);
            if (drawPolitics)
            {                
                colourIndex = EditorGUILayout.Popup("Region: ", colourIndex, WorldInfo.WorldSetting.RegionNames, GUILayout.MaxWidth(dataFieldSmall));
                paintBrush = GetRegionColour(colourIndex);

                if (GUILayout.Button("Apply", GUILayout.MaxWidth(dataFieldSmall)))
                {
                    foreach ((int, int) modifiedPoliticPixel in drawnBuffer)
                    {
                        PoliticInfo.Politic[modifiedPoliticPixel.Item1, modifiedPoliticPixel.Item2] = colourIndex;
                    }
                    drawnBuffer = new List<(int, int)>();
                }

                if (GUILayout.Button("Save Changes", GUILayout.MaxWidth(dataFieldSmall)))
                {
                    string fileDataPath = Path.Combine(testPath, "Politic.json");
                    var json = JsonConvert.SerializeObject(PoliticInfo.Politic, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    File.WriteAllText(fileDataPath, json);
                }

                if (GUILayout.Button("Save Image", GUILayout.MaxWidth(dataFieldSmall)))
                {
                    string fileDataPath = Path.Combine(testPath, "Politic.png");
                    heightmapByteArray = ConvertToGrayscale(PoliticInfo.Politic);
                    // var json = JsonConvert.SerializeObject(heightmapByteArray, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    File.WriteAllBytes(fileDataPath, heightmapByteArray);
                }
            }
            EditorGUILayout.EndVertical();

            // EditorGUILayout.BeginVertical();
            // if (GUILayout.Button("Inspect Light Heightmap", GUILayout.MaxWidth(dataFieldSmall)))
            // {
            //     InspectHeightmap(true);
            // }
            // if (GUILayout.Button("Inspect Dark Heightmap", GUILayout.MaxWidth(dataFieldSmall)))
            // {
            //     InspectHeightmap(false);
            // }

            // GUILayout.Label("Map: ", EditorStyles.boldLabel);
            // EditorGUILayout.LabelField("Highest Point: ", topValue.ToString());
            // EditorGUILayout.LabelField("Lowest Point", bottomValue.ToString());
            // EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();

            r = EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
            if (GUILayout.Button(buttonZoomOut, GUILayout.MaxWidth(100)))
            {
                zoomLevel /= 2.0f;
                layerPosition = DirectionButton(buttonZoomOut, layerPosition, true);
                mapRect = DirectionButton(buttonZoomOut, mapRect);
                // heightmapRect = DirectionButton(buttonZoomOut, heightmapRect);
            }

            if (GUILayout.Button(buttonUp, GUILayout.MaxWidth(50)))
            {
                layerPosition = DirectionButton(buttonUp, layerPosition, true);
                mapRect = DirectionButton(buttonUp, mapRect);
                // heightmapRect = DirectionButton(buttonUp, heightmapRect);
            }

            if (GUILayout.Button(buttonZoomIn, GUILayout.MaxWidth(100)))
            {
                zoomLevel *= 2.0f;
                layerPosition = DirectionButton(buttonZoomIn, layerPosition, true);
                mapRect = DirectionButton(buttonZoomIn, mapRect);
                // heightmapRect = DirectionButton(buttonZoomIn, heightmapRect);
            }
            EditorGUILayout.EndHorizontal();

            r = EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
            if (GUILayout.Button(buttonLeft, GUILayout.MaxWidth(50)))
            {
                layerPosition = DirectionButton(buttonLeft, layerPosition, true);
                mapRect = DirectionButton(buttonLeft, mapRect);
                // heightmapRect = DirectionButton(buttonLeft, heightmapRect);
            }

            GUILayout.Space(25);

            if (GUILayout.Button(buttonReset, GUILayout.MaxWidth(100)))
            {
                zoomLevel = 1.0f;
                layerPosition = new Rect(layerOriginX, layerOriginY, startingWidth, startingHeight);
                mapRect = DirectionButton(buttonReset, mapRect);
            }

            GUILayout.Space(25);

            if (GUILayout.Button(buttonRight, GUILayout.MaxWidth(50)))
            {
                layerPosition = DirectionButton(buttonRight, layerPosition, true);
                mapRect = DirectionButton(buttonRight, mapRect);
                // heightmapRect = DirectionButton(buttonRight, heightmapRect);
            }
            EditorGUILayout.EndHorizontal();

            r = EditorGUILayout.BeginHorizontal(GUILayout.Height(20));
            GUILayout.Space(100);
            if (GUILayout.Button(buttonDown, GUILayout.MaxWidth(50)))
            {
                layerPosition = DirectionButton(buttonDown, layerPosition, true);
                mapRect = DirectionButton(buttonDown, mapRect);
                // heightmapRect = DirectionButton(buttonDown, heightmapRect);
            }
            GUILayout.Space(100);
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("SAVE CURRENT WORLD", GUILayout.MaxWidth(200)))
            {
                SaveCurrentWorld();
            }

            EditorGUILayout.EndToggleGroup();

            Event mouse = Event.current;
            if (groupEnabled && !pixelSelected && mapView.Contains(mouse.mousePosition))
            {
                Vector2 pixel = GetMouseCoordinates();
                if ((int)pixel.x > 0 && (int)pixel.x < MapsFile.MaxMapPixelX && (int)pixel.y > 0 && (int)pixel.y < MapsFile.MaxMapPixelY)
                    pixelData.GetPixelData((int)pixel.x, (int)pixel.y);
            }

            if (groupEnabled && !drawHeightmap && !drawClimate && !drawPolitics && mapView.Contains(mouse.mousePosition) && mouse.button == 0 && mouse.type == EventType.MouseUp)
            {
                modifiedPixelData = new PixelData();
                modifiedPixelData = pixelData;
                pixelCoordinates = GetMouseCoordinates();
                pixelSelected = true;
                lastDungeonType = modifiedPixelData.DungeonType;
                widthModified = false;
                heightModified = false;
            }

            if (groupEnabled && drawHeightmap && mapView.Contains(mouse.mousePosition) && mouse.button == 0 && mouse.type == EventType.MouseUp)
            {
                pixelCoordinates = GetMouseCoordinates();
                if (!drawnBuffer.Contains(((int)pixelCoordinates.y, (int)pixelCoordinates.y)))
                    drawnBuffer.Add(((int)pixelCoordinates.x, (int)pixelCoordinates.y));

                if (drawnBufferCount != drawnBuffer.Count)
                {
                    SetHeightmap();
                }
            }

            if (groupEnabled && drawClimate && mapView.Contains(mouse.mousePosition) && mouse.button == 0 && mouse.type == EventType.MouseUp)
            {
                pixelCoordinates = GetMouseCoordinates();
                if (!drawnBuffer.Contains(((int)pixelCoordinates.y, (int)pixelCoordinates.y)))
                    drawnBuffer.Add(((int)pixelCoordinates.x, (int)pixelCoordinates.y));

                if (drawnBufferCount != drawnBuffer.Count)
                {
                    SetClimateMap();
                }
            }

            if (groupEnabled && drawPolitics && mapView.Contains(mouse.mousePosition) && mouse.button == 0 && mouse.type == EventType.MouseUp)
            {
                pixelCoordinates = GetMouseCoordinates();
                if (!drawnBuffer.Contains(((int)pixelCoordinates.y, (int)pixelCoordinates.y)))
                    drawnBuffer.Add(((int)pixelCoordinates.x, (int)pixelCoordinates.y));

                if (drawnBufferCount != drawnBuffer.Count)
                {
                    SetPoliticMap();
                }
            }

            if (mapView.Contains(mouse.mousePosition) && mouse.button == 1 && mouse.type == EventType.MouseUp)
            {
                modifiedTownBlocks = new List<string>();
                modifiedDungeonBlocks = new List<Blocks>();
                modifiedPixelData = new PixelData();
                pixelSelected = false;
            }

            window.Repaint();
        }

        protected Vector2 GetMouseCoordinates()
        {
            Vector2 coordinates = Vector2.zero;
            Event mouse = Event.current;
            if (mapView.Contains(mouse.mousePosition))
            {
                coordinates.x = mouse.mousePosition.x - mapView.x;
                coordinates.x = (coordinates.x * ((float)MapsFile.MaxMapPixelX / zoomLevel)) / mapView.width;
                coordinates.x += mapRect.x * (float)MapsFile.MaxMapPixelX;

                coordinates.y = mouse.mousePosition.y - mapView.y;
                coordinates.y = (coordinates.y * (float)MapsFile.MaxMapPixelY / zoomLevel) / mapView.height;
                coordinates.y += ((1.0f - mapRect.y) * (float)MapsFile.MaxMapPixelY)- (float)MapsFile.MaxMapPixelY / zoomLevel;

                return coordinates;
            }

            else
            {
                coordinates = new Vector2(-1.0f, -1.0f);
                return coordinates;
            };
        }

        protected List<string> SetListCount(List<string> rmBlocks, int width, int height)
        {
            int difference = width * height - rmBlocks.Count;
            if (difference > 0)
            {
                for (int i = 0; i < difference; i++)
                {
                    rmBlocks.Add(noBlock);
                }
            }
            else if (difference < 0)
                rmBlocks.RemoveRange(rmBlocks.Count + difference, -1 * difference);

            return rmBlocks;
        }

        protected void SetNewLocation()
        {
            modifiedPixelData.hasLocation = true;
            modifiedPixelData.Name = "";
            modifiedPixelData.Region = modifiedPixelData.Politic;
            modifiedPixelData.MapId = (ulong)((int)pixelCoordinates.y * MapsFile.MaxMapPixelX + (int)pixelCoordinates.x);
            modifiedPixelData.Latitude = SetPixelLongitudeLatitude(false);
            modifiedPixelData.Longitude = SetPixelLongitudeLatitude(true);
            modifiedPixelData.LocationType = -1;
            modifiedPixelData.DungeonType = 255;
            modifiedPixelData.Key = 0;
            modifiedPixelData.exterior = new Exterior();
            modifiedPixelData.exterior.X = SetPixelXY(true);
            modifiedPixelData.exterior.Y = SetPixelXY(false);
            modifiedPixelData.exterior.AnotherName = "";
            modifiedPixelData.exterior.LocationId = 0;
            modifiedPixelData.exterior.ExteriorLocationId = 0;
            modifiedPixelData.exterior.BuildingCount = 0;
            modifiedPixelData.exterior.buildings = new Buildings[0];
            modifiedPixelData.exterior.Width = 1;
            modifiedPixelData.exterior.Height = 1;
            modifiedPixelData.exterior.PortTown = false;
            modifiedPixelData.exterior.BlockNames = new string[1];
            modifiedPixelData.exterior.BlockNames[0] = noBlock;
            // modifiedPixelData.dungeon = SetNewDungeon();

            // modifiedPixelData.Politic = modifiedPixelData.Region + 128;
            // modifiedPixelData.RegionIndex = modifiedPixelData.Politic;
            // modifiedPixelData.LocationIndex = 0;
        }

        protected int SetPixelLongitudeLatitude(bool isLongitude)
        {
            int size;
            int pixelSize;

            if (isLongitude)
            {
                size = (int)pixelCoordinates.x * MapsFile.WorldMapTileDim;
                pixelSize = modifiedPixelData.exterior.Width;
            }
            else{
                size = (MapsFile.MaxMapPixelY - (int)pixelCoordinates.y) * MapsFile.WorldMapTileDim;
                pixelSize = modifiedPixelData.exterior.Height;
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

        protected int SetPixelXY(bool isX)
        {
            int size;

            if (isX)
            {
                size = (int)pixelCoordinates.x * MapsFile.WorldMapTerrainDim + (8 - modifiedPixelData.exterior.Width) * 2048;
            }
            else{
                size = ((int)pixelCoordinates.y * MapsFile.WorldMapTerrainDim + (8 - modifiedPixelData.exterior.Height) * 2048) + 1;
            }
            return size;
        }

        protected void ApplyChanges()
        {
            if (modifiedPixelData.hasLocation)
            {
                // Converting DFRegion.RegionMapTable
                DFRegion.RegionMapTable modifiedLocation = new DFRegion.RegionMapTable();
                List<DFRegion.RegionMapTable> mapTableList = new List<DFRegion.RegionMapTable>();
                mapTableList = Worldmaps.Worldmap[Worldmaps.GetTileIndex(modifiedPixelData.MapId)].MapTable.ToList();

                modifiedLocation.MapId = modifiedPixelData.MapId;
                modifiedLocation.Latitude = modifiedPixelData.Latitude;
                modifiedLocation.Longitude = modifiedPixelData.Longitude;
                modifiedLocation.LocationType = (DFRegion.LocationTypes)modifiedPixelData.LocationType;
                modifiedLocation.DungeonType = (DFRegion.DungeonTypes)modifiedPixelData.DungeonType;
                modifiedLocation.Discovered = false;
                modifiedLocation.Key = (uint)modifiedPixelData.Key;

                if (mapTableList.Exists(x => x.MapId == modifiedLocation.MapId))
                    mapTableList.RemoveAll(x => x.MapId == modifiedLocation.MapId);
                Debug.Log("Adding location " + modifiedLocation.MapId);
                mapTableList.Add(modifiedLocation);
                mapTableList.Sort();

                Worldmaps.Worldmap[Worldmaps.GetTileIndex(modifiedPixelData.MapId)].LocationCount = mapTableList.Count();
                Worldmaps.Worldmap[Worldmaps.GetTileIndex(modifiedPixelData.MapId)].MapTable = new DFRegion.RegionMapTable[Worldmaps.Worldmap[Worldmaps.GetTileIndex(modifiedPixelData.MapId)].LocationCount];
                Worldmaps.Worldmap[Worldmaps.GetTileIndex(modifiedPixelData.MapId)].MapTable = mapTableList.ToArray();
                DFLocation[] newLocations = new DFLocation[Worldmaps.Worldmap[Worldmaps.GetTileIndex(modifiedPixelData.MapId)].LocationCount];

                // Recreating new MapNames, MapIdLookup and MapName Lookup for the region
                bool newLocationAdded = false;
                bool locationModified = false;
                int counter = 0;
                string[] newMapNames = new string[Worldmaps.Worldmap[Worldmaps.GetTileIndex(modifiedPixelData.MapId)].LocationCount];
                Dictionary<ulong, int> newMapIdLookup = new Dictionary<ulong, int>();
                Dictionary<string, int> newMapNameLookup = new Dictionary<string, int>();

                foreach (DFRegion.RegionMapTable mapTable in mapTableList)
                {
                    if (newMapIdLookup.ContainsKey(mapTable.MapId))
                        newMapIdLookup.Remove(mapTable.MapId);
                    newMapIdLookup.Add(mapTable.MapId, counter);

                    DFLocation location = new DFLocation();

                    if (!newLocationAdded)
                    {
                        if (Worldmaps.Worldmap[Worldmaps.GetTileIndex(modifiedPixelData.MapId)].Locations.Length > counter)
                            Worldmaps.GetLocation(Worldmaps.GetTileIndex(modifiedPixelData.MapId), counter, out location);
                        else location = GetDFLocationFromPixelData(modifiedPixelData);
                    }
                    else Worldmaps.GetLocation(Worldmaps.GetTileIndex(modifiedPixelData.MapId), (counter - 1), out location);

                    if (location.MapTableData.MapId == mapTable.MapId && modifiedLocation.MapId == mapTable.MapId)
                    {
                        if (!newMapNameLookup.ContainsKey(modifiedPixelData.Name))
                            newMapNameLookup.Add(modifiedPixelData.Name, counter);
                        newMapNames[counter] = modifiedPixelData.Name;
                        DFLocation createdLocation = new DFLocation();
                        createdLocation = GetDFLocationFromPixelData(modifiedPixelData);
                        createdLocation.LocationIndex = counter;
                        newLocations[counter] = new DFLocation();
                        newLocations[counter] = createdLocation;
                        locationModified = true;
                        Debug.Log("Location modified: " + createdLocation.Name + "; counter: " + counter);
                        counter++;
                    }
                    else if (location.MapTableData.MapId == mapTable.MapId)
                    {
                        if (!newMapNameLookup.ContainsKey(Worldmaps.Worldmap[Worldmaps.GetTileIndex(modifiedPixelData.MapId)].MapNames[counter]))
                            newMapNameLookup.Add(Worldmaps.Worldmap[Worldmaps.GetTileIndex(modifiedPixelData.MapId)].MapNames[counter], counter);
                        newMapNames[counter] = Worldmaps.Worldmap[Worldmaps.GetTileIndex(modifiedPixelData.MapId)].MapNames[counter];
                        newLocations[counter] = new DFLocation();
                        location.LocationIndex = counter;
                        newLocations[counter] = location;
                        counter++;
                    }
                    else {
                        if (!newMapNameLookup.ContainsKey(modifiedPixelData.Name))
                            newMapNameLookup.Add(modifiedPixelData.Name, counter);
                        newMapNames[counter] = modifiedPixelData.Name;
                        newLocationAdded = true;
                        DFLocation createdLocation = new DFLocation();
                        createdLocation = GetDFLocationFromPixelData(modifiedPixelData);
                        createdLocation.LocationIndex = counter;
                        newLocations[counter] = new DFLocation();
                        newLocations[counter] = createdLocation;
                        Debug.Log("New location added: " + createdLocation.Name + "; counter: " + counter);
                    }

                    // if (newLocationAdded && (counter + 1) < mapTableList.Count)
                    // {
                    //     if (!newMapNameLookup.ContainsKey(Worldmaps.Worldmap[modifiedPixelData.Region].MapNames[counter]))
                    //         newMapNameLookup.Add(Worldmaps.Worldmap[modifiedPixelData.Region].MapNames[counter], (counter + 1));
                    //     newMapNames[counter + 1] = Worldmaps.Worldmap[modifiedPixelData.Region].MapNames[counter];
                    // }

                    if ((counter) == mapTableList.Count)
                    {
                        break;
                    }
                }

                Worldmaps.Worldmap[Worldmaps.GetTileIndex(modifiedPixelData.MapId)].MapNames = new string[Worldmaps.Worldmap[Worldmaps.GetTileIndex(modifiedPixelData.MapId)].LocationCount];
                Worldmaps.Worldmap[Worldmaps.GetTileIndex(modifiedPixelData.MapId)].MapNames = newMapNames;

                Worldmaps.Worldmap[Worldmaps.GetTileIndex(modifiedPixelData.MapId)].MapTable = mapTableList.ToArray();

                Worldmaps.Worldmap[Worldmaps.GetTileIndex(modifiedPixelData.MapId)].MapIdLookup = new Dictionary<ulong, int>();
                Worldmaps.Worldmap[Worldmaps.GetTileIndex(modifiedPixelData.MapId)].MapIdLookup = newMapIdLookup;

                Worldmaps.Worldmap[Worldmaps.GetTileIndex(modifiedPixelData.MapId)].MapNameLookup = new Dictionary<string, int>();
                Worldmaps.Worldmap[Worldmaps.GetTileIndex(modifiedPixelData.MapId)].MapNameLookup = newMapNameLookup;

                Worldmaps.Worldmap[Worldmaps.GetTileIndex(modifiedPixelData.MapId)].Locations = newLocations;
                
                // Debug.Log("Enumerating maps from changes applying");
                Worldmaps.mapDict = Worldmaps.EnumerateMaps();
            }

            SmallHeightmap.Woods[(int)pixelCoordinates.x, (int)pixelCoordinates.y] = (byte)modifiedPixelData.Elevation;
            PoliticInfo.Politic[(int)pixelCoordinates.x, (int)pixelCoordinates.y] = ConvertRegionIndexToPoliticIndex(modifiedPixelData.Region);
            ClimateInfo.Climate[(int)pixelCoordinates.x, (int)pixelCoordinates.y] = (modifiedPixelData.Climate + (int)MapsFile.Climates.Ocean);
        }

        protected void OpenRegionManager()
        {
            RegionManager regionManager = (RegionManager) EditorWindow.GetWindow(typeof(RegionManager), false, "Region Manager");
            regionManager.Show();
        }

        protected void OpenFactionManager()
        {
            FactionManager factionManager = (FactionManager) EditorWindow.GetWindow(typeof(FactionManager), false, "Faction Manager");
            factionManager.Show();
        }

        protected void OpenTileCreator()
        {
            TileCreator tileCreator = (TileCreator) EditorWindow.GetWindow(typeof(TileCreator), false, "Tile Creator");
            tileCreator.Show();
        }

        protected void OpenGenerateRoutesWindow()
        {
            GenerateRoutesWindow generateRoutesWindow = (GenerateRoutesWindow) EditorWindow.GetWindow(typeof(GenerateRoutesWindow), false, "Routes Generator");
            generateRoutesWindow.Show();
        }

        protected void SplitLocations()
        {
            Worldmap[] splittedLocations = new Worldmap[MapsFile.TileX * MapsFile.TileY];

            for (int i = 0; i < (MapsFile.TileX * MapsFile.TileY); i++)
            {
                splittedLocations[i] = new Worldmap();

                splittedLocations[i].Name = i.ToString();
                splittedLocations[i].LocationCount = 0;
                splittedLocations[i].MapNames = null;
                splittedLocations[i].MapTable = null;
                splittedLocations[i].MapIdLookup = null;
                splittedLocations[i].MapNameLookup = null;
                splittedLocations[i].Locations = null;
            }

            for (int j = 0; j < Worldmaps.WholeWM.Length; j++)
            {
                // Debug.Log("j: " + j);
                if (Worldmaps.WholeWM[j].LocationCount == 0)
                {
                    Worldmaps.WholeWM[j] = new Worldmap();
                }
                else
                {
                for (int k = 0; k < Worldmaps.WholeWM[j].Locations.Length; k++)
                {
                    DFLocation location = Worldmaps.WholeWM[j].Locations[k];

                    if (location.Name == null || location.Name.Contains("Test")) continue;
                    int splitLocIndex = (int)((location.MapTableData.MapId / MapsFile.MaxMapPixelX) / MapsFile.TileDim) * MapsFile.TileX + (int)((location.MapTableData.MapId % MapsFile.MaxMapPixelX) / MapsFile.TileDim);

                    List<string> mapNames;
                    if (splittedLocations[splitLocIndex].MapNames != null) mapNames = splittedLocations[splitLocIndex].MapNames.ToList<string>();
                    else mapNames = new List<string>();
                    mapNames.Add(location.Name);
                    splittedLocations[splitLocIndex].MapNames = mapNames.ToArray();

                    List<DFRegion.RegionMapTable> mapTable;
                    if (splittedLocations[splitLocIndex].MapTable != null) mapTable = splittedLocations[splitLocIndex].MapTable.ToList();
                    else mapTable = new List<DFRegion.RegionMapTable>();
                    mapTable.Add(Worldmaps.WholeWM[j].MapTable[k]);
                    splittedLocations[splitLocIndex].MapTable = mapTable.ToArray();

                    if (splittedLocations[splitLocIndex].MapIdLookup == null) splittedLocations[splitLocIndex].MapIdLookup = new Dictionary<ulong, int>();
                    if (splittedLocations[splitLocIndex].MapIdLookup.ContainsKey(location.MapTableData.MapId)) Debug.Log("Found existing MapId: " + location.MapTableData.MapId);
                    else splittedLocations[splitLocIndex].MapIdLookup.Add(location.MapTableData.MapId, splittedLocations[splitLocIndex].LocationCount);

                    if (splittedLocations[splitLocIndex].MapNameLookup == null) splittedLocations[splitLocIndex].MapNameLookup = new Dictionary<string, int>();
                    if (!splittedLocations[splitLocIndex].MapNameLookup.ContainsKey(location.Name)) splittedLocations[splitLocIndex].MapNameLookup.Add(location.Name, splittedLocations[splitLocIndex].LocationCount);

                    List<DFLocation> locations;
                    if (splittedLocations[splitLocIndex].Locations != null) locations = splittedLocations[splitLocIndex].Locations.ToList();
                    else locations = new List<DFLocation>();
                    locations.Add(location);
                    splittedLocations[splitLocIndex].Locations = locations.ToArray();

                    splittedLocations[splitLocIndex].LocationCount++;
                }
                }
            }

            for (int l = 0; l < splittedLocations.Length; l++)
            {
                string fileDataPath = Path.Combine(MapEditor.arena2Path, "Maps", "Locations", "map" + l.ToString("00000") + ".json");
                var json = JsonConvert.SerializeObject(splittedLocations[l], new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                File.WriteAllText(fileDataPath, json);
            }
        }

        protected void SetLocationKeys()
        {
            Worldmap tile = new Worldmap();

            for (int x = 0; x < MapsFile.TileX; x++)
            {
                for (int y = 0; y < MapsFile.TileY; y++)
                {
                    int tileIndex = x + y * MapsFile.TileX;

                    tile = JsonConvert.DeserializeObject<Worldmap>(File.ReadAllText(Path.Combine(MapEditor.arena2Path, "Maps", "Locations", "map" + tileIndex.ToString("00000") + ".json")));

                    if (tile.LocationCount <= 0)
                        continue;

                    for (int i = 0; i < tile.MapTable.Length; i++)
                    {
                        int resultingKey = 0;
                        int blocks = tile.Locations[i].Exterior.ExteriorData.Width * tile.Locations[i].Exterior.ExteriorData.Height;
                        for (int j = 0; j < tile.Locations[i].Exterior.BuildingCount; j++)
                        {
                            resultingKey = (resultingKey | GetPartialKey(tile.Locations[i].Exterior.Buildings[j]));
                        }
                        tile.MapTable[i].Key = (uint)resultingKey;
                        tile.Locations[i].MapTableData.Key = (uint)resultingKey;
                    }

                    string path = Path.Combine(MapEditor.arena2Path, "Maps", "Locations", "map" + tileIndex.ToString("00000") + ".json");
                    var json = JsonConvert.SerializeObject(tile, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    File.WriteAllText(path, json);
                }
            }
        }

        protected int GetPartialKey(DFLocation.BuildingData building)
        {
            switch (building.BuildingType)
            {
                case DFLocation.BuildingTypes.Alchemist:
                    return 2048;
                case DFLocation.BuildingTypes.Armorer:
                    return 1024;
                case DFLocation.BuildingTypes.Bank:
                    return 4096;
                case DFLocation.BuildingTypes.Bookseller:
                    return 8192;
                case DFLocation.BuildingTypes.ClothingStore:
                    return 16384;
                case DFLocation.BuildingTypes.GemStore:
                    return 32768;
                case DFLocation.BuildingTypes.Library:
                    return 65536;
                case DFLocation.BuildingTypes.PawnShop:
                    return 4194304;                
                case DFLocation.BuildingTypes.Tavern:
                    return 256;
                case DFLocation.BuildingTypes.WeaponSmith:
                    return 512;
                case DFLocation.BuildingTypes.Temple:
                    switch (building.FactionId)
                    {
                        case 21:
                            return 2;
                        case 22:
                            return 128;
                        case 24:
                            return 32;
                        case 26:
                            return 1;
                        case 27:
                            return 8;
                        case 29:
                            return 4;
                        case 33:
                            return 64;
                        case 35:
                            return 16;
                        default:
                            return 0;
                    }
                case DFLocation.BuildingTypes.GuildHall:
                    switch (building.FactionId)
                    {
                        case 40:
                            return 262144;
                        case 41:
                            return 2097152;
                        case 368:
                        case 408:
                        case 409:
                        case 410:
                        case 411:
                        case 413:
                        case 414:
                        case 415:
                        case 416:
                        case 417:
                            return 131072;
                        default:
                            return 0;
                    }
                default:
                    return 0;
            }
        }

        protected void CreateMapDict()
        {
            string fileDataPath = Path.Combine(Worldmaps.tilesPath, "mapDict.json");
            Dictionary<int, List<(int, int)>> regionTiles = new Dictionary<int, List<(int, int)>>();
            int tileIndex;
            int previousTileIndex = -1;
            int[,] jsonTile = new int[MapsFile.TileDim, MapsFile.TileDim];
            List<(int, int)>[] tempLists = new List<(int, int)>[WorldData.WorldSetting.RegionNames.Length];
            for (int x = 0; x < MapsFile.MaxMapPixelX; x++)
            {
                for (int y = 0; y < MapsFile.MaxMapPixelY; y++)
                {
                    int tileX = x / MapsFile.TileDim;
                    int tileY = y / MapsFile.TileDim;
                    tileIndex = (tileX) + ((tileY) * MapsFile.TileX);
                    int regionInd = PoliticInfo.ConvertMapPixelToRegionIndex(x, y);

                    if (tempLists[regionInd] == null)
                        tempLists[regionInd] = new List<(int, int)>();

                    if (!tempLists[regionInd].Exists(z => z.Item1 == tileIndex))
                        tempLists[regionInd].Add((tileIndex, 0));

                    if (Worldmaps.HasLocation(x, y))
                    {
                        int index = tempLists[regionInd].FindIndex(w => w.Item1 == tileIndex);
                        tempLists[regionInd][index] = (tempLists[regionInd][index].Item1, (tempLists[regionInd][index].Item2 + 1));
                    }
                }
                Debug.Log("X done: " + x);
            }
            for (int region = 0; region < WorldInfo.WorldSetting.RegionNames.Length; region++)
            {
                regionTiles.Add(region, tempLists[region]);
                Debug.Log("Region " + region + " added.");
            }
            var json = JsonConvert.SerializeObject(regionTiles, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            File.WriteAllText(fileDataPath, json);
        }

        protected void CreateLargeHeightmap()
        {
            int heightmapVariation;
            const string tilesPath = "/home/arneb/Games/daggerfall/DaggerfallGameFiles/arena2/Maps/Tiles";

            // Texture2D textureTiles = new Texture2D(MapsFile.TileDim, MapsFile.TileDim);
            // ImageConversion.LoadImage(textureTiles, File.ReadAllBytes(Path.Combine(testPath, "Woods.png")));

            for (int tileX = 0; tileX < MapsFile.TileX; tileX++)
            {
                for (int tileY = 0; tileY < MapsFile.TileY; tileY++)
                {
                    if (File.Exists(Path.Combine(tilesPath, "woodsLarge_" + tileX + "_" + tileY + ".png")))
                                continue;
                                
                    byte[,] largeHeightmap = new byte[MapsFile.TileDim * 5, MapsFile.TileDim * 5];
                    byte[,] heightmap = new byte[MapsFile.TileDim, MapsFile.TileDim];
                    Texture2D textureTiles = new Texture2D(MapsFile.TileDim, MapsFile.TileDim);
                    ImageConversion.LoadImage(textureTiles, File.ReadAllBytes(Path.Combine(tilesPath, "woods_" + tileX + "_" + tileY + ".png")));
                    heightmap = ConvertToMatrix(textureTiles);

                    for (int x = 0; x < (MapsFile.TileDim * 5); x++)
                    {
                        for (int y = 0; y < (MapsFile.TileDim * 5); y++)
                        {
                            int subX = x / 5;
                            int subY = y / 5;
                            int subValue = heightmap[subX, subY];
                            int randomRange = (UnityEngine.Random.Range(0, 100) + 1);
                            int currentClimate = ClimateInfo.Climate[subX + (tileX * MapsFile.TileDim), subY + (tileY * MapsFile.TileDim)];

                            int check = 0;

                            if (subValue < 3)
                                check = subValue;
                            else
                            {
                                heightmapVariation = GetHeightmapVariation(randomRange, currentClimate);
                                check = subValue + heightmapVariation;

                                if (check < 4)
                                check = 4;
                            }

                            if (check < 0)
                                check = 0;
                            if (check > byte.MaxValue)
                                check = byte.MaxValue;

                            largeHeightmap[x, y] = (byte)check;
                        }
                    }

                    string fileDataPath = Path.Combine(tilesPath, "woodsLarge_" + tileX + "_" + tileY + ".png");
                    byte[] woodsLargeBuffer = ConvertToGrayscale(largeHeightmap);
                    File.WriteAllBytes(fileDataPath, woodsLargeBuffer);
                    // var json = JsonConvert.SerializeObject(largeHeightmap, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                    // File.WriteAllText(fileDataPath, json);
                }
            }
        }

        protected int GetHeightmapVariation(int randomRange, int currentClimate, int heightmapVariation = 5)
        {
            int heightmapVariationUnit = heightmapVariation / 5;
            int randomPositiveNegative = UnityEngine.Random.Range(0, 2);
            if (randomPositiveNegative == 0)
                randomPositiveNegative = -1;

            switch (currentClimate)
            {
                case (int)MapsFile.Climates.Desert:         // Low variation
                case (int)MapsFile.Climates.Swamp:
                    if (randomRange <= 25)
                        return 0;
                    if (randomRange <= 50)
                        return (heightmapVariationUnit * randomPositiveNegative);
                    if (randomRange <= 75)
                        return (heightmapVariationUnit * 2 * randomPositiveNegative);
                    if (randomRange <= 90)
                        return (heightmapVariationUnit * 3 * randomPositiveNegative);
                    if (randomRange <= 99)
                        return (heightmapVariationUnit * 4 * randomPositiveNegative);

                    return (heightmapVariationUnit * 5 * randomPositiveNegative);

                case (int)MapsFile.Climates.Desert2:        // Mid-low variation
                case (int)MapsFile.Climates.Subtropical:
                case (int)MapsFile.Climates.Woodlands:
                case (int)MapsFile.Climates.Maquis:
                    if (randomRange <= 20)
                        return 0;
                    if (randomRange <= 45)
                        return (heightmapVariationUnit * randomPositiveNegative);
                    if (randomRange <= 70)
                        return (heightmapVariationUnit * 2 * randomPositiveNegative);
                    if (randomRange <= 85)
                        return (heightmapVariationUnit * 3 * randomPositiveNegative);
                    if (randomRange <= 95)
                        return (heightmapVariationUnit * 4 * randomPositiveNegative);

                    return (heightmapVariationUnit * 5 * randomPositiveNegative);

                case (int)MapsFile.Climates.Rainforest:     // Mid variation
                case (int)MapsFile.Climates.HauntedWoodlands:
                    if (randomRange <= 10)
                        return 0;
                    if (randomRange <= 25)
                        return (heightmapVariationUnit * randomPositiveNegative);
                    if (randomRange <= 45)
                        return (heightmapVariationUnit * 2 * randomPositiveNegative);
                    if (randomRange <= 65)
                        return (heightmapVariationUnit * 3 * randomPositiveNegative);
                    if (randomRange <= 85)
                        return (heightmapVariationUnit * 4 * randomPositiveNegative);

                    return (heightmapVariationUnit * 5 * randomPositiveNegative);

                case (int)MapsFile.Climates.MountainWoods:  // Mid-high variation
                    if (randomRange <= 5)
                        return 0;
                    if (randomRange <= 10)
                        return (heightmapVariationUnit * (UnityEngine.Random.Range(1, 4)) * randomPositiveNegative);
                    if (randomRange <= 40)
                        return (heightmapVariationUnit * (UnityEngine.Random.Range(4, 7)) * randomPositiveNegative);
                    if (randomRange <= 60)
                        return (heightmapVariationUnit * (UnityEngine.Random.Range(7, 10)) * randomPositiveNegative);
                    if (randomRange <= 80)
                        return (heightmapVariationUnit * (UnityEngine.Random.Range(10, 13)) * randomPositiveNegative);

                    return (heightmapVariationUnit * (UnityEngine.Random.Range(13, 16)) * randomPositiveNegative);

                case (int)MapsFile.Climates.Mountain:       // High variation
                    if (randomRange <= 1)
                        return 0;
                    if (randomRange <= 5)
                        return (heightmapVariationUnit * (UnityEngine.Random.Range(1, 4)) * randomPositiveNegative);
                    if (randomRange <= 10)
                        return (heightmapVariationUnit * (UnityEngine.Random.Range(4, 7)) * randomPositiveNegative);
                    if (randomRange <= 20)
                        return (heightmapVariationUnit * (UnityEngine.Random.Range(7, 10)) * randomPositiveNegative);
                    if (randomRange <= 40)
                        return (heightmapVariationUnit * (UnityEngine.Random.Range(13, 16)) * randomPositiveNegative);
                    if (randomRange <= 70)
                        return (heightmapVariationUnit * (UnityEngine.Random.Range(16, 19)) * randomPositiveNegative);

                    return (heightmapVariationUnit * (UnityEngine.Random.Range(19, 21)) * randomPositiveNegative);

                default:
                    return 0;
            }

            
        }

        protected void CreateLargeClimatemap()
        {
            Texture2D climateMapImage = new Texture2D(MapsFile.WorldWidth, MapsFile.WorldHeight);
            Color32[] climateMap = new Color32[MapsFile.WorldWidth * MapsFile.WorldHeight];
            byte[] climateArray = new byte[MapsFile.WorldWidth * MapsFile.WorldHeight * 4];
            bool hemisphereNorth;
            int offset = 0;

            for (int x = 0; x < MapsFile.WorldWidth; x++)
            {
                for (int y = 0; y < MapsFile.WorldHeight; y++)
                {
                    offset = (MapsFile.WorldHeight - y - 1) * MapsFile.WorldWidth + x;

                    if (SmallHeightmap.Woods[x, y] > 80 + y * (200 / MapsFile.WorldHeight))
                        climateMap[offset] = new Color32((byte)MapsFile.Climates.Mountain, (byte)MapsFile.Climates.Mountain, (byte)MapsFile.Climates.Mountain, 255);
                    else if (SmallHeightmap.Woods[x, y] > 50 + y * (200 / MapsFile.WorldHeight))
                        climateMap[offset] = new Color32((byte)MapsFile.Climates.MountainWoods, (byte)MapsFile.Climates.MountainWoods, (byte)MapsFile.Climates.MountainWoods, 255);
                    else climateMap[offset] = new Color32((byte)MapsFile.Climates.Ocean, (byte)MapsFile.Climates.Ocean, (byte)MapsFile.Climates.Ocean, 255);
                }
            }

            climateMapImage.SetPixels32(climateMap);
            climateArray = ImageConversion.EncodeToPNG(climateMapImage);
            string fileDataPath = Path.Combine(testPath, "ClimateMap.png");
            File.WriteAllBytes(fileDataPath, climateArray);
        }

        protected void CreateWorldSettings()
        {
            string fileDataPath = Path.Combine(WorldMaps.mapPath, "WorldData.json");
            // File.Create(fileDataPath);
            WorldInfo.WorldSetting = new WorldStats();
            var json = JsonConvert.SerializeObject(WorldData.WorldSetting, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            File.WriteAllText(fileDataPath, json);
        }

        protected void CreateNameGen()
        {
            string fileDataPath = Path.Combine(MapEditor.arena2Path, "Maps", "NameGen.json");
            // File.Create(fileDataPath);
            TextAsset nameGenText = Resources.Load<TextAsset>("NameGen") as TextAsset;
            Dictionary<NameHelper.BankTypes, NameHelper.NameBank> bankDict = SaveLoadManager.Deserialize(typeof(Dictionary<NameHelper.BankTypes, NameHelper.NameBank>), nameGenText.text) as Dictionary<NameHelper.BankTypes, NameHelper.NameBank>;
            var json = JsonConvert.SerializeObject(bankDict, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            File.WriteAllText(fileDataPath, json);
        }

        protected void SaveCurrentWorld()
        {
            string fileDataPath;
            
            foreach (int modifiedTile in modifiedTiles)
            {
                fileDataPath = Path.Combine(Path.Combine(Worldmaps.locationsPath, "map" + (modifiedTile).ToString("00000") + ".json"));
                var json = JsonConvert.SerializeObject(Worldmaps.Worldmap[modifiedTile], new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                File.WriteAllText(fileDataPath, json);
            }

            // fileDataPath = Path.Combine(testPath, "mapDict.json");
            // json = JsonConvert.SerializeObject(Worldmaps.mapDict, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            // File.WriteAllText(fileDataPath, json);

            // fileDataPath = Path.Combine(testPath, "Climate.json");
            // json = JsonConvert.SerializeObject(ClimateInfo.Climate, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            // File.WriteAllText(fileDataPath, json);

            // fileDataPath = Path.Combine(testPath, "Politic.json");
            // json = JsonConvert.SerializeObject(PoliticInfo.Politic, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            // File.WriteAllText(fileDataPath, json);

            // fileDataPath = Path.Combine(testPath, "Woods.json");
            // json = JsonConvert.SerializeObject(SmallHeightmap.Woods, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            // File.WriteAllText(fileDataPath, json);
        }

        protected void SetCurrentWorld()
        {
            // worldSavePath = EditorUtility.SaveFolderPanel("Select a path", "", "");

            // if (!Directory.Exists(Path.Combine(worldSavePath, worldName)))
            // {
            //     Directory.CreateDirectory(Path.Combine(path, worldName));
            //     string path2 = EditorGUIUtilityBridge.OpenFolderPanel("Select source files path", "", "");

            //     File.Copy(Path.Combine(path2, "Maps.json"), Path.Combine(worldSavePath, "Maps.json"));
            //     File.Copy(Path.Combine(path2, "Climate.json"), Path.Combine(worldSavePath, "Climate.json"));
            //     File.Copy(Path.Combine(path2, "Politic.json"), Path.Combine(worldSavePath, "Politic.json"));
            //     File.Copy(Path.Combine(path2, "Woods.json"), Path.Combine(worldSavePath, "Woods.json"));
            // }
        }

        protected string[] SetBlocks(int pref, bool isRMB)
        {
            List<string> blockNames = new List<string>();
            DFBlock block = new DFBlock();
            string blockPath;
            string blockExtension;
            string prefix;
            if (isRMB)
            {
                blockExtension = "RMB";
                prefix = rmbBlockPrefixes[pref];
            }
            else{
                blockExtension = "RDB";
                prefix = rdbBlockLetters[pref];
            }

            blockPath = Path.Combine(testPath, blockExtension);

            int index = 0;

            while (blockFileReader.LoadBlock(index, out block))
            {
                if (block.Name.StartsWith(prefix) && block.Name.EndsWith(blockExtension))
                {
                    blockNames.Add(block.Name);
                }
                index++;
            }

            if (!Directory.Exists(blockPath))
            {
                Debug.Log("invalid blocks directory: " + blockPath);
            }
            else
            {
                string[] blockFiles = Directory.GetFiles(blockPath, prefix + "*." + blockExtension + ".json");
                // var rmbFileNames = new string[rmbFiles.Length];
                // var loadedRMBNames = GetAllRMBFileNames();

                for (int i = 0; i < blockFiles.Length; i++)
                {
                    string blockToAdd = blockFiles[i].Remove(0, blockPath.Length + 1);
                    blockToAdd = blockToAdd.Remove(12);
                    blockNames.Add(blockToAdd);
                }
            }

            blockNames.Sort();
    
            return blockNames.ToArray();
        }

        protected int GetDBlockIndex()
        {
            int counter = 0;

            foreach (Blocks block in modifiedDungeonBlocks)
            {
                if (pointedCoordinates == (block.X, block.Z))
                    return counter;

                counter++;
            }

            return -1;
        }

        protected Blocks GetDungeonBlockData()
        {
            Blocks toBlock = new Blocks();

            toBlock.X = pointedCoordinates.Item1;
            toBlock.Z = pointedCoordinates.Item2;
            if (pointedCoordinates == (0, 0))
                toBlock.IsStartingBlock = true;
            else toBlock.IsStartingBlock = false;
            toBlock.BlockName = RDBBlocks[availableDBlocks];
            toBlock.WaterLevel = 0;
            toBlock.CastleBlock = false;

            return toBlock;
        }

        protected Dungeon SetNewDungeon()
        {
            Dungeon dungeon = new Dungeon();

            dungeon.DungeonName = modifiedPixelData.Name;
            dungeon.X = modifiedPixelData.exterior.X;
            dungeon.Y = modifiedPixelData.exterior.Y;
            dungeon.LocationId = 0;
            dungeon.BlockCount = 5;
            dungeon.blocks = new Blocks[5];
            modifiedDungeonBlocks = new List<Blocks>();

            (short, short)[] blockPosition = {(0, -1), (-1, 0), (0, 0), (1, 0), (1, 1)};
            for (int i = 0; i < dungeon.BlockCount; i++)
            {
                if (i == 2)
                {
                    dungeon.blocks[i] = new Blocks();
                    dungeon.blocks[i].X = blockPosition[i].Item1;
                    dungeon.blocks[i].Z = blockPosition[i].Item2;
                    dungeon.blocks[i].BlockName = "N0000000.RDB";

                    dungeon.blocks[i].IsStartingBlock = true;

                    dungeon.blocks[i].WaterLevel = 0;
                    dungeon.blocks[i].CastleBlock = false;
                    modifiedDungeonBlocks.Add(dungeon.blocks[i]);
                }
                else modifiedDungeonBlocks.Add(RandomBorderBlock(blockPosition[i].Item2, blockPosition[i].Item1));
            }

            return dungeon;
        }

        protected Blocks RandomBorderBlock(int z, int x)
        {
            Blocks block = new Blocks();

            block.X = x;
            block.Z = z;
            block.IsStartingBlock = false;
            block.BlockName = "B000000" + (UnityEngine.Random.Range(0, 10)).ToString() + ".RDB";
            block.WaterLevel = 0;
            block.CastleBlock = false;

            return block;
        }

        protected static DFLocation.ClimateSettings SetClimate(int climateIndex)
        {
            DFLocation.ClimateSettings climate = new DFLocation.ClimateSettings();

            switch (climateIndex)
            {
                case 224:
                    climate.WorldClimate = 224;
                    climate.ClimateType = 0;
                    climate.NatureSet = (DFLocation.ClimateTextureSet)503;
                    climate.GroundArchive = 2;
                    climate.NatureArchive = 503;
                    climate.SkyBase = 8;
                    climate.People = (FactionFile.FactionRaces)2;
                    climate.Names = 0;
                    break;

                case 225:
                    climate.WorldClimate = 225;
                    climate.ClimateType = 0;
                    climate.NatureSet = (DFLocation.ClimateTextureSet)503;
                    climate.GroundArchive = 2;
                    climate.NatureArchive = 503;
                    climate.SkyBase = 8;
                    climate.People = (FactionFile.FactionRaces)2;
                    climate.Names = 0;
                    break;

                case 226:
                    climate.WorldClimate = 226;
                    climate.ClimateType = (DFLocation.ClimateBaseType)100;
                    climate.NatureSet = (DFLocation.ClimateTextureSet)510;
                    climate.GroundArchive = 102;
                    climate.NatureArchive = 510;
                    climate.SkyBase = 0;
                    climate.People = (FactionFile.FactionRaces)0;
                    climate.Names = 0;
                    break;

                case 227:
                    climate.WorldClimate = 227;
                    climate.ClimateType = (DFLocation.ClimateBaseType)400;
                    climate.NatureSet = (DFLocation.ClimateTextureSet)500;
                    climate.GroundArchive = 402;
                    climate.NatureArchive = 500;
                    climate.SkyBase = 24;
                    climate.People = (FactionFile.FactionRaces)2;
                    climate.Names = 0;
                    break;

                case 228:
                    climate.WorldClimate = 228;
                    climate.ClimateType = (DFLocation.ClimateBaseType)400;
                    climate.NatureSet = (DFLocation.ClimateTextureSet)502;
                    climate.GroundArchive = 402;
                    climate.NatureArchive = 502;
                    climate.SkyBase = 24;
                    climate.People = (FactionFile.FactionRaces)3;
                    climate.Names = 0;
                    break;

                case 229:
                    climate.WorldClimate = 229;
                    climate.ClimateType = 0;
                    climate.NatureSet = (DFLocation.ClimateTextureSet)501;
                    climate.GroundArchive = 2;
                    climate.NatureArchive = 501;
                    climate.SkyBase = 24;
                    climate.People = (FactionFile.FactionRaces)3;
                    climate.Names = 0;
                    break;

                case 230:
                    climate.WorldClimate = 230;
                    climate.ClimateType = (DFLocation.ClimateBaseType)300;
                    climate.NatureSet = (DFLocation.ClimateTextureSet)506;
                    climate.GroundArchive = 102;
                    climate.NatureArchive = 506;
                    climate.SkyBase = 16;
                    climate.People = (FactionFile.FactionRaces)3;
                    climate.Names = 0;
                    break;

                case 231:
                    climate.WorldClimate = 231;
                    climate.ClimateType = (DFLocation.ClimateBaseType)300;
                    climate.NatureSet = (DFLocation.ClimateTextureSet)504;
                    climate.GroundArchive = 302;
                    climate.NatureArchive = 504;
                    climate.SkyBase = 16;
                    climate.People = (FactionFile.FactionRaces)3;
                    climate.Names = 0;
                    break;

                case 232:
                    climate.WorldClimate = 232;
                    climate.ClimateType = (DFLocation.ClimateBaseType)300;
                    climate.NatureSet = (DFLocation.ClimateTextureSet)508;
                    climate.GroundArchive = 302;
                    climate.NatureArchive = 508;
                    climate.SkyBase = 16;
                    climate.People = (FactionFile.FactionRaces)3;
                    climate.Names = 0;
                    break;

                case 233:
                    climate.WorldClimate = 233;
                    climate.ClimateType = (DFLocation.ClimateBaseType)1100;
                    climate.NatureSet = (DFLocation.ClimateTextureSet)1030;
                    climate.GroundArchive = 402;
                    climate.NatureArchive = 1030;
                    climate.SkyBase = 24;
                    climate.People = (FactionFile.FactionRaces)3;
                    climate.Names = 0;
                    break;

                default:
                    break;
            }

            return climate;
        }

        protected void ResetSelectedCoordinates()
        {
            selectedX = selectedY = 1;
        }

        protected void SetRegionNames()
        {
            regionNames = new string[WorldInfo.WorldSetting.Regions];

            for (int i = 0; i < WorldInfo.WorldSetting.Regions; i++)
            {
                regionNames[i] = WorldInfo.WorldSetting.RegionNames[i];
            }
        }

        protected void SetClimateNames()
        {
            climateNames = new string[Enum.GetNames(typeof(MapsFile.Climates)).Length];

            for (int i = 0; i < Enum.GetNames(typeof(MapsFile.Climates)).Length; i++)
            {
                climateNames[i] = ((MapsFile.Climates)(i + (int)MapsFile.Climates.Ocean)).ToString();
            }
        }

        protected void SetHeightmap()
        {
            Color32[] heightmapBuffer = new Color32[MapsFile.MaxMapPixelX * MapsFile.MaxMapPixelY];
            heightMap = new Texture2D(MapsFile.MaxMapPixelX, MapsFile.MaxMapPixelY, TextureFormat.ARGB32, false);
            heightMap.filterMode = FilterMode.Point;
            heightMap.wrapMode = TextureWrapMode.Clamp;
            heightmapBuffer = CreateHeightmap();
            heightMap.SetPixels32(heightmapBuffer);
            heightMap.Apply();
        }

        protected void SetLocationsMap()
        {
            Color32[] locationsMapBuffer = new Color32[MapsFile.MaxMapPixelX * MapsFile.MaxMapPixelY];
            locationsMap = new Texture2D(MapsFile.MaxMapPixelX, MapsFile.MaxMapPixelY, TextureFormat.ARGB32, false);
            locationsMap.filterMode = FilterMode.Point;
            locationsMap.wrapMode = TextureWrapMode.Clamp;
            locationsMapBuffer = CreateLocationsMap();
            locationsMap.SetPixels32(locationsMapBuffer);
            locationsMap.Apply();
        }

        protected void SetTrailsMap()
        {
            Color32[] trailsMapBuffer = new Color32[MapsFile.MaxMapPixelX * MapsFile.MaxMapPixelY];
            trailsMap = new Texture2D(MapsFile.MaxMapPixelX, MapsFile.MaxMapPixelY, TextureFormat.ARGB32, false);
            trailsMap.filterMode = FilterMode.Point;
            trailsMap.wrapMode = TextureWrapMode.Clamp;
            trailsMapBuffer = CreateTrailsMap();
            trailsMap.SetPixels32(trailsMapBuffer);
            trailsMap.Apply();
        }

        protected void SetClimateMap()
        {
            Color32[] climateMapBuffer = new Color32[MapsFile.MaxMapPixelX * MapsFile.MaxMapPixelY];
            climateMap = new Texture2D(MapsFile.MaxMapPixelX, MapsFile.MaxMapPixelY, TextureFormat.ARGB32, false);
            climateMap.filterMode = FilterMode.Point;
            climateMap.wrapMode = TextureWrapMode.Clamp;
            climateMapBuffer = CreateClimateMap();
            climateMap.SetPixels32(climateMapBuffer);
            climateMap.Apply();
        }

        protected void SetPoliticMap()
        {
            Color32[] politicMapBuffer = new Color32[MapsFile.MaxMapPixelX * MapsFile.MaxMapPixelY];
            politicMap = new Texture2D(MapsFile.MaxMapPixelX, MapsFile.MaxMapPixelY, TextureFormat.ARGB32, false);
            politicMap.filterMode = FilterMode.Point;
            politicMap.wrapMode = TextureWrapMode.Clamp;
            politicMapBuffer = CreatePoliticMap();
            politicMap.SetPixels32(politicMapBuffer);
            politicMap.Apply();
        }

        protected void SetHeightmapRect()
        {
            mapRect = new Rect(heightmapOriginX, heightmapOriginY, 1, 1);
        }

        protected void SetMaps()
        {
            SetHeightmap();
            SetClimateMap();
            SetPoliticMap();
        }

        static Color32[] CreateHeightmap()
        {
            Color32[] colours = new Color32[MapsFile.MaxMapPixelX * MapsFile.MaxMapPixelY];

            for (int x = 0; x < MapsFile.MaxMapPixelX; x++)
            {
                for (int y = 0; y < MapsFile.MaxMapPixelY; y++)
                {
                    int offset = (((MapsFile.MaxMapPixelY - y - 1) * MapsFile.MaxMapPixelX) + x);
                    byte value = SmallHeightmap.GetHeightMapValue(x, y);
                    int terrain;
                    Color32 colour = new Color32();;

                    if (drawnBuffer.Count > 0 && drawnBuffer.Contains((x, y)))
                    {
                        colours[offset] = paintBrush;
                    }
                    else
                    {
                        if (value < 4)
                            terrain = -1;

                        else
                            terrain = value;

                        colour = GetElevationColour(terrain);
                        colours[offset] = colour;
                    }
                }
            }

            return colours;
        }

        public static Color32 GetElevationColour(int index)
        {
            Color32 colour = new Color32();

            switch (index)
            {
                case -1:
                    colour = new Color32(218, 246, 246, (byte)mapAlphaChannel);
                    break;

                // case 0:
                //     colour = new Color32(175, 200, 168, (byte)mapAlphaChannel);
                //     break;

                // case 1:
                //     colour = new Color32(148, 176, 141, (byte)mapAlphaChannel);
                //     break;

                // case 2:
                //     colour = new Color32(123, 156, 118, (byte)mapAlphaChannel);
                //     break;

                // case 3:
                //     colour = new Color32(107, 144, 109, (byte)mapAlphaChannel);
                //     break;

                // case 4:
                //     colour = new Color32(93, 130, 94, (byte)mapAlphaChannel);
                //     break;

                // case 5:
                //     colour = new Color32(82, 116, 86, (byte)mapAlphaChannel);
                //     break;

                // case 6:
                //     colour = new Color32(77, 110, 78, (byte)mapAlphaChannel);
                //     break;

                // case 7:
                //     colour = new Color32(68, 99, 67, (byte)mapAlphaChannel);
                //     break;

                // case 8:
                //     colour = new Color32(61, 89, 53, (byte)mapAlphaChannel);
                //     break;

                // case 9:
                //     colour = new Color32(52, 77, 45, (byte)mapAlphaChannel);
                //     break;

                // case 10:
                //     colour = new Color32(34, 51, 34, (byte)mapAlphaChannel);
                //     break;

                default:
                    colour = new Color32((byte)(index / 2), (byte)(128 + (index / 2)), (byte)(index / 4), (byte)mapAlphaChannel);
                    break;
            }

            return colour;
        }

        static Color32[] CreateTrailsMap()
        {
            Color32[] colours = new Color32[MapsFile.MaxMapPixelX * MapsFile.MaxMapPixelY];

            for (int x = 0; x < MapsFile.MaxMapPixelX; x++)
            {
                for (int y = 0; y < MapsFile.MaxMapPixelY; y++)
                {
                    int offset = (((MapsFile.MaxMapPixelY - y - 1) * MapsFile.MaxMapPixelX) + x);
                    Color32 colour = new Color32();

                    switch (TrailsInfo.Trails[x, y])
                    {
                        case 1:
                            colour = new Color32(60, 60, 60, 255);
                            break;

                        case 2:
                            colour = new Color32(160, 118, 74, 255);
                            break;

                        case 0:
                        default:
                            colour = new Color32(0, 0, 0, 0);
                            break;
                    }
                    colours[offset] = colour;
                }
            }

            return colours;
        }

        static Color32[] CreateLocationsMap()
        {
            Color32[] colours = new Color32[MapsFile.MaxMapPixelX * MapsFile.MaxMapPixelY];

            for (int x = 0; x < MapsFile.MaxMapPixelX; x++)
            {
                for (int y = 0; y < MapsFile.MaxMapPixelY; y++)
                {
                    int offset = x + (MapsFile.MaxMapPixelY - y - 1) * MapsFile.MaxMapPixelX;
                    int sampleRegion = PoliticInfo.ConvertMapPixelToRegionIndex(x, y);

                    MapSummary summary;
                    if (Worldmaps.HasLocation(x, y, out summary))
                    {
                        Color32 colour = new Color32();
                        int index = (int)summary.LocationType;
                        if (index == -1)
                            continue;
                        else
                        {
                            switch (index)
                            {
                                case 0:
                                    colour = new Color32(220, 177, 177, 255);
                                    break;

                                case 1:
                                    colour = new Color32(188, 138, 138, 255);
                                    break;

                                case 2:
                                    colour = new Color32(155, 105, 106, 255);
                                    break;

                                case 3:
                                    colour = new Color32(165, 100, 70, 255);
                                    break;

                                case 4:
                                    colour = new Color32(215, 119, 39, 255);
                                    break;

                                case 5:
                                    colour = new Color32(176, 205, 255, 255);
                                    break;

                                case 6:
                                    colour = new Color32(126, 81, 89, 255);
                                    break;

                                case 7:
                                    colour = new Color32(191, 87, 27, 255);
                                    break;

                                case 8:
                                    colour = new Color32(193, 133, 100, 255);
                                    break;

                                case 9:
                                    colour = new Color32(68, 124, 192, 255);
                                    break;

                                case 10:
                                    colour = new Color32(171, 51, 15, 255);
                                    break;

                                case 11:
                                    colour = new Color32(140, 86, 55, 255);
                                    break;

                                case 12:
                                    colour = new Color32(147, 15, 7, 255);
                                    break;

                                case 13:
                                    colour = new Color32(15, 15, 15, 255);
                                    break;

                                default:
                                    colour = new Color32(40, 47, 40, 255);
                                    break;
                            }
                        }
                        colours[offset] = colour;
                    }
                    else colours[offset] = new Color32(0, 0, 0, 0);
                }

            }
            return colours;
        }

        static Color32[] CreateClimateMap()
        {
            Color32[] colours = new Color32[MapsFile.MaxMapPixelX * MapsFile.MaxMapPixelY];
            Color32 colour = new Color32();

            for (int x = 0; x < MapsFile.MaxMapPixelX; x++)
            {
                for (int y = 0; y < MapsFile.MaxMapPixelY; y++)
                {
                    int offset = (((MapsFile.MaxMapPixelY - y - 1) * MapsFile.MaxMapPixelX) + x);

                    int value = ClimateInfo.Climate[x, y];

                    if (drawnBuffer.Count > 0 && drawnBuffer.Contains((x, y)))
                    {
                        colours[offset] = paintBrush;
                    }
                    else
                    {
                        colour = GetClimateColour(value);
                        colours[offset] = colour;
                    }
                }
            }
            return colours;
        }

        public static Color32 GetClimateColour(int index)
        {
            Color32 colour = new Color32();

            switch (index)
            {
                case -1:    // transparent frame
                case 223:   // Ocean 
                    colour = new Color32(0, 0, 0, 0);
                    break;

                case 224:   // Desert
                    colour = new Color32(217, 217, 217, (byte)mapAlphaChannel);
                    break;

                case 225:   // Desert2
                    colour = new Color32(255, 255, 255, (byte)mapAlphaChannel);
                    break;

                case 226:   // Mountains
                    colour = new Color32(230, 196, 230, (byte)mapAlphaChannel);
                    break;

                case 227:   // RainForest
                    colour = new Color32(0, 152, 25, (byte)mapAlphaChannel);
                    break;

                case 228:   // Swamp
                    colour = new Color32(115, 153, 141, (byte)mapAlphaChannel);
                    break;

                case 229:   // Sub tropical
                    colour = new Color32(180, 180, 179, (byte)mapAlphaChannel);
                    break;

                case 230:   // Woodland hills (aka Mountain Woods)
                    colour = new Color32(191, 143, 191, (byte)mapAlphaChannel);
                    break;

                case 231:   // TemperateWoodland (aka Woodlands)
                    colour = new Color32(0, 190, 0, (byte)mapAlphaChannel);
                    break;

                case 232:   // Haunted woodland
                    colour = new Color32(190, 166, 143, (byte)mapAlphaChannel);
                    break;

                default:
                    colour = new Color32(0, 0, 0, 0);
                    break;
            }

                    return colour;
        }

        static Color32[] CreatePoliticMap()
        {
            Color32[] colours = new Color32[MapsFile.MaxMapPixelX * MapsFile.MaxMapPixelY];
            Color32 colour = new Color32();

            for (int x = 0; x < MapsFile.MaxMapPixelX; x++)
            {
                for (int y = 0; y < MapsFile.MaxMapPixelY; y++)
                {
                    int offset = (((MapsFile.MaxMapPixelY - y - 1) * MapsFile.MaxMapPixelX) + x);
                    int value = PoliticInfo.ConvertMapPixelToRegionIndex(x, y);

                    if (drawnBuffer.Count > 0 && drawnBuffer.Contains((x, y)))
                    {
                        colours[offset] = paintBrush;
                    }
                    else{
                        colour = GetRegionColour(value);
                        colours[offset] = colour;
                    }
                }
            }
            return colours;
        }

        public static Color32 GetRegionColour(int index)
        {
            Color32 colour = new Color32();

            if (index == 31)
            {
                colour = new Color32(0, 0, 0, 0);
                return colour;
            }

            index = index % 23;

            switch (index)
            {
                case 0:     // The Alik'r Desert
                    colour = new Color32(212, 180, 105, (byte)mapAlphaChannel);
                    break;

                case 1:     // The Dragontail Mountains
                    colour = new Color32(149, 43, 29, (byte)mapAlphaChannel);
                    break;

                // case 2:     // Glenpoint Foothills - unused
                //     colour = new Color32(123, 156, 118, (byte)mapAlphaChannel);
                //     break;

                // case 3:     // Daggerfall Bluffs - unused
                //     colour = new Color32(107, 144, 109, (byte)mapAlphaChannel);
                //     break;

                // case 4:     // Yeorth Burrowland - unused
                //     colour = new Color32(93, 130, 94, (byte)mapAlphaChannel);
                //     break;

                case 2:     // Dwynnen
                    colour = new Color32(236, 42, 50, (byte)mapAlphaChannel);
                    break;

                // case 6:     // Ravennian Forest - unused
                //     colour = new Color32(77, 110, 78, (byte)mapAlphaChannel);
                //     break;

                // case 7:     // Devilrock - unused
                //     colour = new Color32(68, 99, 67, (byte)mapAlphaChannel);
                //     break;

                // case 8:     // Malekna Forest - unused
                //     colour = new Color32(61, 89, 53, (byte)mapAlphaChannel);
                //     break;

                case 3:     // The Isle of Balfiera
                    colour = new Color32(158, 0, 0, (byte)mapAlphaChannel);
                    break;

                // case 10:    // Bantha - unused
                //     colour = new Color32(34, 51, 34, (byte)mapAlphaChannel);
                //     break;

                case 4:    // Dak'fron
                    colour = new Color32(36, 116, 84, (byte)mapAlphaChannel);
                    break;

                // case 12:    // The Islands in the Western Iliac Bay - unused
                //     colour = new Color32(36, 116, 84, (byte)mapAlphaChannel);
                //     break;

                // case 13:    // Tamarilyn Point - unused
                //     colour = new Color32(36, 116, 84, (byte)mapAlphaChannel);
                //     break;

                // case 14:    // Lainlyn Cliffs - unused
                //     colour = new Color32(36, 116, 84, (byte)mapAlphaChannel);
                //     break;

                // case 15:    // Bjoulsae River - unused
                //     colour = new Color32(36, 116, 84, (byte)mapAlphaChannel);
                //     break;

                case 5:    // The Wrothgarian Mountains
                    colour = new Color32(250, 201, 11, (byte)mapAlphaChannel);
                    break;

                case 6:    // Daggerfall
                    colour = new Color32(0, 126, 13, (byte)mapAlphaChannel);
                    break;

                case 7:    // Glenpoint
                    colour = new Color32(152, 152, 152, (byte)mapAlphaChannel);
                    break;

                case 8:    // Betony
                    colour = new Color32(31, 55, 132, (byte)mapAlphaChannel);
                    break;

                case 9:    // Sentinel
                    colour = new Color32(158, 134, 17, (byte)mapAlphaChannel);
                    break;

                case 10:    // Anticlere
                    colour = new Color32(30, 30, 30, (byte)mapAlphaChannel);
                    break;

                case 11:    // Lainlyn
                    colour = new Color32(38, 127, 0, (byte)mapAlphaChannel);
                    break;

                case 12:    // Wayrest
                    colour = new Color32(0, 248, 255, (byte)mapAlphaChannel);
                    break;

                // case 24:    // Gen Tem High Rock village - unused
                //     colour = new Color32(158, 134, 17, (byte)mapAlphaChannel);
                //     break;

                // case 25:    // Gen Rai Hammerfell village - unused
                //     colour = new Color32(158, 134, 17, (byte)mapAlphaChannel);
                //     break;

                case 13:    // The Orsinium Area
                    colour = new Color32(0, 99, 46, (byte)mapAlphaChannel);
                    break;

                // case 27:    // Skeffington Wood - unused
                //     colour = new Color32(0, 99, 46, (byte)mapAlphaChannel);
                //     break;

                // case 28:    // Hammerfell bay coast - unused
                //     colour = new Color32(0, 99, 46, (byte)mapAlphaChannel);
                //     break;

                // case 29:    // Hammerfell sea coast - unused
                //     colour = new Color32(0, 99, 46, (byte)mapAlphaChannel);
                //     break;

                // case 30:    // High Rock bay coast - unused
                //     colour = new Color32(0, 99, 46, (byte)mapAlphaChannel);
                //     break;

                // case 31:    // High Rock sea coast
                //     colour = new Color32(0, 0, 0, 0);
                //     break;

                case 14:    // Northmoor
                    colour = new Color32(127, 127, 127, (byte)mapAlphaChannel);
                    break;

                case 15:    // Menevia
                    colour = new Color32(81, 46, 26, (byte)mapAlphaChannel);
                    break;

                case 16:    // Alcaire
                    colour = new Color32(238, 90, 0, (byte)mapAlphaChannel);
                    break;

                case 17:    // Koegria
                    colour = new Color32(0, 83, 165, (byte)mapAlphaChannel);
                    break;

                case 18:    // Bhoriane
                    colour = new Color32(255, 124, 237, (byte)mapAlphaChannel);
                    break;

                case 19:    // Kambria
                    colour = new Color32(0, 19, 127, (byte)mapAlphaChannel);
                    break;

                case 20:    // Phrygias
                    colour = new Color32(229, 115, 39, (byte)mapAlphaChannel);
                    break;

                case 21:    // Urvaius
                    colour = new Color32(246, 207, 74, (byte)mapAlphaChannel);
                    break;

                case 22:    // Ykalon
                    colour = new Color32(87, 0, 127, (byte)mapAlphaChannel);
                    break;

                // case 41:    // Daenia
                //     colour = new Color32(32, 142, 142, (byte)mapAlphaChannel);
                //     break;

                // case 42:    // Shalgora
                //     colour = new Color32(202, 0, 0, (byte)mapAlphaChannel);
                //     break;

                // case 43:    // Abibon-Gora
                //     colour = new Color32(142, 74, 173, (byte)mapAlphaChannel);
                //     break;

                // case 44:    // Kairou
                //     colour = new Color32(68, 27, 0, (byte)mapAlphaChannel);
                //     break;

                // case 45:    // Pothago
                //     colour = new Color32(207, 20, 43, (byte)mapAlphaChannel);
                //     break;

                // case 46:    // Myrkwasa
                //     colour = new Color32(119, 108, 59, (byte)mapAlphaChannel);
                //     break;

                // case 47:    // Ayasofya
                //     colour = new Color32(74, 35, 1, (byte)mapAlphaChannel);
                //     break;

                // case 48:    // Tigonus
                //     colour = new Color32(255, 127, 127, (byte)mapAlphaChannel);
                //     break;

                // case 49:    // Kozanset
                //     colour = new Color32(127, 127, 127, (byte)mapAlphaChannel);
                //     break;

                // case 50:    // Satakalaam
                //     colour = new Color32(255, 46, 0, (byte)mapAlphaChannel);
                //     break;

                // case 51:    // Totambu
                //     colour = new Color32(193, 77, 0, (byte)mapAlphaChannel);
                //     break;

                // case 52:    // Mournoth
                //     colour = new Color32(153, 28, 0, (byte)mapAlphaChannel);
                //     break;

                // case 53:    // Ephesus
                //     colour = new Color32(253, 103, 0, (byte)mapAlphaChannel);
                //     break;

                // case 54:    // Santaki
                //     colour = new Color32(1, 255, 144, (byte)mapAlphaChannel);
                //     break;

                // case 55:    // Antiphyllos
                //     colour = new Color32(229, 182, 64, (byte)mapAlphaChannel);
                //     break;

                // case 56:    // Bergama
                //     colour = new Color32(196, 169, 37, (byte)mapAlphaChannel);
                //     break;

                // case 57:    // Gavaudon
                //     colour = new Color32(240, 8, 47, (byte)mapAlphaChannel);
                //     break;

                // case 58:    // Tulune
                //     colour = new Color32(0, 73, 126, (byte)mapAlphaChannel);
                //     break;

                // case 59:    // Glenumbra Moors
                //     colour = new Color32(15, 0, 61, (byte)mapAlphaChannel);
                //     break;

                // case 60:    // Ilessan Hills
                //     colour = new Color32(236, 42, 50, (byte)mapAlphaChannel);
                //     break;

                // case 61:    // Cybiades
                //     colour = new Color32(255, 255, 255, (byte)mapAlphaChannel);
                //     break;

                // case -1:
                default:
                    colour = new Color32(0, 0, 0, 0);
                    break;
            }

            return colour;
        }

        public static int ConvertRegionIndexToPoliticIndex(int regionIndex)
        {
            if (regionIndex == 64)
                return regionIndex;

            regionIndex += 128;
            return regionIndex;
        }

        public static DFLocation GetDFLocationFromPixelData(PixelData sourcePixel)
        {
            DFLocation createdLocation = new DFLocation();

            sourcePixel = ConsolidateLocDimension(sourcePixel);

            createdLocation.Loaded = true;
            createdLocation.Name = sourcePixel.Name;
            createdLocation.RegionName = sourcePixel.RegionName;

            if (sourcePixel.DungeonType == 255 || sourcePixel.DungeonType >= Enum.GetNames(typeof(DFRegion.DungeonTypes)).Length)
                createdLocation.HasDungeon = false;
            else createdLocation.HasDungeon = true;

            createdLocation.MapTableData.MapId = sourcePixel.MapId;
            createdLocation.MapTableData.Latitude = sourcePixel.Latitude;
            createdLocation.MapTableData.Longitude = sourcePixel.Longitude;
            createdLocation.MapTableData.LocationType = (DFRegion.LocationTypes)sourcePixel.LocationType;
            createdLocation.MapTableData.DungeonType = (DFRegion.DungeonTypes)sourcePixel.DungeonType;
            createdLocation.MapTableData.Discovered = false;
            createdLocation.MapTableData.Key = (uint)sourcePixel.Key;
            createdLocation.Exterior.RecordElement.Header.X = sourcePixel.exterior.X;
            createdLocation.Exterior.RecordElement.Header.Y = sourcePixel.exterior.Y;
            createdLocation.Exterior.RecordElement.Header.IsExterior = 32768; // TODO: must check what this does
            createdLocation.Exterior.RecordElement.Header.Unknown2 = 0; // TODO: must check what this does

            if (sourcePixel.exterior.LocationId != 0)
                createdLocation.Exterior.RecordElement.Header.LocationId = (ushort)sourcePixel.exterior.LocationId;
            else createdLocation.Exterior.RecordElement.Header.LocationId = (ushort)GenerateNewLocationId();
            createdLocation.Exterior.RecordElement.Header.IsInterior = 0; // TODO: must check what this does
            createdLocation.Exterior.RecordElement.Header.ExteriorLocationId = 0;
            createdLocation.Exterior.RecordElement.Header.LocationName = sourcePixel.Name;
            createdLocation.Exterior.BuildingCount = (ushort)sourcePixel.exterior.BuildingCount;
            createdLocation.Exterior.Buildings = new DFLocation.BuildingData[createdLocation.Exterior.BuildingCount];
            
            for (int i = 0; i < createdLocation.Exterior.BuildingCount; i++)
            {
                createdLocation.Exterior.Buildings[i].NameSeed = (ushort)sourcePixel.exterior.buildings[i].NameSeed;
                createdLocation.Exterior.Buildings[i].FactionId = (ushort)sourcePixel.exterior.buildings[i].FactionId;
                createdLocation.Exterior.Buildings[i].Sector = (short)sourcePixel.exterior.buildings[i].Sector;
                createdLocation.Exterior.Buildings[i].BuildingType = (DFLocation.BuildingTypes)sourcePixel.exterior.buildings[i].BuildingType;
                createdLocation.Exterior.Buildings[i].Quality = (byte)sourcePixel.exterior.buildings[i].Quality;
            }
            createdLocation.Exterior.ExteriorData.AnotherName = sourcePixel.Name;
            createdLocation.Exterior.ExteriorData.MapId = sourcePixel.MapId;
            createdLocation.Exterior.ExteriorData.LocationId = 0;
            createdLocation.Exterior.ExteriorData.Width = (byte)sourcePixel.exterior.Width;
            createdLocation.Exterior.ExteriorData.Height = (byte)sourcePixel.exterior.Height;

            if (sourcePixel.exterior.PortTown)
                createdLocation.Exterior.ExteriorData.PortTownAndUnknown = 1;
            else createdLocation.Exterior.ExteriorData.PortTownAndUnknown = 0;

            createdLocation.Exterior.ExteriorData.BlockNames = new string[createdLocation.Exterior.ExteriorData.Width * createdLocation.Exterior.ExteriorData.Height];
            createdLocation.Exterior.ExteriorData.BlockNames = sourcePixel.exterior.BlockNames;

            if (sourcePixel.DungeonType != 255 && sourcePixel.DungeonType < (Enum.GetNames(typeof(DFRegion.DungeonTypes)).Length - 1))
            {
                createdLocation.Dungeon.RecordElement.Header.X = sourcePixel.dungeon.X;
                createdLocation.Dungeon.RecordElement.Header.Y = sourcePixel.dungeon.Y;
                createdLocation.Dungeon.RecordElement.Header.IsExterior = 0;
                createdLocation.Dungeon.RecordElement.Header.Unknown2 = 0;
                createdLocation.Dungeon.RecordElement.Header.LocationId = (ushort)(createdLocation.Exterior.RecordElement.Header.LocationId + 1);
                createdLocation.Dungeon.RecordElement.Header.IsInterior = 1;
                createdLocation.Dungeon.RecordElement.Header.ExteriorLocationId = createdLocation.Exterior.RecordElement.Header.LocationId;
                createdLocation.Dungeon.RecordElement.Header.LocationName = sourcePixel.Name;
                createdLocation.Dungeon.Header.BlockCount = (ushort)sourcePixel.dungeon.BlockCount;
                createdLocation.Dungeon.Blocks = new DFLocation.DungeonBlock[createdLocation.Dungeon.Header.BlockCount];

                for (int i = 0; i < createdLocation.Dungeon.Header.BlockCount; i++)
                {
                    createdLocation.Dungeon.Blocks[i] = new DFLocation.DungeonBlock();
                    createdLocation.Dungeon.Blocks[i].X = (sbyte)sourcePixel.dungeon.blocks[i].X;
                    createdLocation.Dungeon.Blocks[i].Z = (sbyte)sourcePixel.dungeon.blocks[i].Z;
                    createdLocation.Dungeon.Blocks[i].IsStartingBlock = sourcePixel.dungeon.blocks[i].IsStartingBlock;
                    createdLocation.Dungeon.Blocks[i].BlockName = sourcePixel.dungeon.blocks[i].BlockName;
                    createdLocation.Dungeon.Blocks[i].WaterLevel = (short)sourcePixel.dungeon.blocks[i].WaterLevel;
                    createdLocation.Dungeon.Blocks[i].CastleBlock = sourcePixel.dungeon.blocks[i].CastleBlock;
                }
            }
            else
            {
                createdLocation.Dungeon.RecordElement.Header.X = 0;
                createdLocation.Dungeon.RecordElement.Header.Y = 0;
                createdLocation.Dungeon.RecordElement.Header.IsExterior = 0;
                createdLocation.Dungeon.RecordElement.Header.Unknown2 = 0;
                createdLocation.Dungeon.RecordElement.Header.LocationId = 0;
                createdLocation.Dungeon.RecordElement.Header.IsInterior = 0;
                createdLocation.Dungeon.RecordElement.Header.ExteriorLocationId = 0;
                createdLocation.Dungeon.RecordElement.Header.LocationName = null;
                createdLocation.Dungeon.Header.BlockCount = 0;
                createdLocation.Dungeon.Blocks = new DFLocation.DungeonBlock[0];
            }

            createdLocation.Climate = SetClimate(sourcePixel.Climate + (int)MapsFile.Climates.Ocean);
            createdLocation.Politic = sourcePixel.Politic;
            createdLocation.RegionIndex = sourcePixel.RegionIndex;
            createdLocation.LocationIndex = sourcePixel.LocationIndex;

            return createdLocation;
        }

        public static PixelData ConsolidateLocDimension(PixelData pixel)
        {
            string[] blockNames = new string[pixel.exterior.Width * pixel.exterior.Height];

            for (int i = 0; i < blockNames.Length; i++)
            {
                blockNames[i] = pixel.exterior.BlockNames[i];
            }
            pixel.exterior.BlockNames = blockNames;

            return pixel;
        }

        public static ulong GenerateNewLocationId()
        {
            bool found = false;
            ulong counter = 0;
            ushort extValue;

            do{
                if (!Worldmaps.locationIdList.Contains(counter) && !Worldmaps.locationIdList.Contains(counter + 1))
                {
                    found = true;
                    return counter;
                }

                counter += 2;
            }
            while (!found);

            return 0;
        }

        public static ushort GetBuildingCount(int width, int height, List<string> townBlocks)
        {
            DFBlock block;
            int locDim = width * height;
            int buildCount = 0;
            foreach (string tBlock in townBlocks)
            {
                block = new DFBlock();

                if (tBlock != noBlock && tBlock != null)
                {
                    block = blockFileReader.GetBlock(tBlock);
                    // int counter = 0;

                    buildCount += block.RmbBlock.SubRecords.Length;

                    // while (block.RmbBlock.FldHeader.BuildingDataList[counter].Quality != 0)
                    // {
                    //     buildCount++;
                    //     counter++;
                    // }
                }
            }
            return (ushort)buildCount;
        }

        public static Buildings[] SetBuildings(PixelData pixel, List<string> blockNames)
        {
            DFBlock block;
            Buildings[] buildings = new Buildings[pixel.exterior.buildings.Length];
            List<Buildings> buildingList = new List<Buildings>();
            foreach (string bNames in blockNames)
            {
                block = new DFBlock();

                if (bNames != noBlock && bNames != null)
                {
                    block = blockFileReader.GetBlock(bNames);
                    pixel.exterior.buildings = new Buildings[block.RmbBlock.SubRecords.Length];

                    for (int j = 0; j < block.RmbBlock.SubRecords.Length; j++)
                    {
                        Buildings buildingElement = new Buildings();
                        if (block.RmbBlock.FldHeader.BuildingDataList[j].NameSeed != 0)
                            buildingElement.NameSeed = block.RmbBlock.FldHeader.BuildingDataList[j].NameSeed;
                        else
                        {
                            UInt16 nameSeed = (ushort)UnityEngine.Random.Range(0, (UInt16.MaxValue + 1));
                            buildingElement.NameSeed = nameSeed;
                        }
                        buildingElement.FactionId = block.RmbBlock.FldHeader.BuildingDataList[j].FactionId;
                        buildingElement.Sector = block.RmbBlock.FldHeader.BuildingDataList[j].Sector;
                        buildingElement.BuildingType = (int)block.RmbBlock.FldHeader.BuildingDataList[j].BuildingType;
                        buildingElement.Quality = block.RmbBlock.FldHeader.BuildingDataList[j].Quality;
                        buildingList.Add(buildingElement);
                    }
                }
            }
            buildings = buildingList.ToArray();
            return buildings;
        }

        static GUIStyle SetGUIStyle()
        {
            GUIStyle style = new GUIStyle();

            style.fixedHeight = 1200.0f;
            style.fixedWidth = 3440.0f;

            // style.stretchHeight = false;
            // style.stretchWidth = false;

            return style;
        }

        static Rect UpdateLayerPosition()
        {
            Rect position = new Rect(layerPosition.x, layerPosition.y, mapView.width, mapView.height);
            Debug.Log("Updating layer Position");
            return position;
        }

        static float SetMapAlphaChannel()
        {
            short numberOfChecks = CountCheckNumber();

            switch (numberOfChecks)
            {
                case 1:
                    return level1;

                case 2:
                    return level2;

                case 3:
                    return level3;

                case 4:
                    return level4;

                default:
                    return 0.0f;
            }
        }

        static short CountCheckNumber()
        {
            short numberOfChecks = 0;

            if (heightmap)
                numberOfChecks++;

            if (climate)
                numberOfChecks++;

            if (politics)
                numberOfChecks++;

            if (mapImage)
                numberOfChecks++;

            if ((drawHeightmap || drawClimate || drawPolitics) && mapImage)
                numberOfChecks = 4;

            if (numberOfChecks < 0 || numberOfChecks > 4)
            {
                Debug.LogError("Invalid check count!");
                return 0;
            }
            else return numberOfChecks;
        }

        static Rect DirectionButton(string direction, Rect position, bool refMap = false)
        {
            int multiplier;
            Event key = Event.current;
            if (Input.GetKeyDown(KeyCode.LeftControl))
                multiplier = 10;
            else multiplier = 1;

            switch (direction)
            {
                case buttonUp:
                    if (refMap)
                        position.y += (position.height * (setMovement * multiplier * zoomLevel));
                    else position.y += setMovement * multiplier;
                    break;

                case buttonDown:
                    if (refMap)
                        position.y -= (position.height * (setMovement * multiplier * zoomLevel));
                    else position.y -= setMovement * multiplier;
                    break;

                case buttonLeft:
                    if (refMap)
                        position.x -= (position.width * (setMovement * multiplier * zoomLevel));
                    else position.x -= setMovement * multiplier;
                    break;

                case buttonRight:
                    if (refMap)
                        position.x += (position.width * (setMovement * multiplier * zoomLevel));
                    else position.x += setMovement * multiplier;
                    break;

                case buttonReset:
                    position = new Rect(0, 0, 1, 1);
                    break;

                case buttonZoomOut:
                    position = new Rect(position.x - position.width / 2, position.y - position.height / 2, position.width * 2, position.height * 2);
                    break;

                case buttonZoomIn:
                    position = new Rect(position.x + position.width / 4, position.y + position.height / 4, position.width / 2, position.height / 2);
                    break;

                default:
                    break;    
            }

            return position;
        }

        public static byte[] ConvertToGrayscale(int[,] map, bool isTrailmap = false)
        {
            Texture2D grayscaleImage = new Texture2D(map.GetLength(0), map.GetLength(1));
            Color32[] grayscaleMap = new Color32[map.GetLength(0) * map.GetLength(1)];
            byte[] grayscaleBuffer = new byte[map.GetLength(0) * map.GetLength(1) * 4];
            
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    int offset = (((map.GetLength(1) - y - 1) * map.GetLength(0)) + x);
                    int value = map[x, y];
                    byte green = 0;
                    byte red = 0; 
                    byte blue = 0;
                    byte alpha = 255;

                    if (!isTrailmap)
                    {
                        if (value == 0)
                        {
                            green = red = blue = alpha = 0;
                        }
                        else if (value <= byte.MaxValue)
                            green = red = blue = (byte)value;
                        else if (value > byte.MaxValue && value < 510)
                        {
                            green = blue = byte.MaxValue;
                            red = (byte)(value - byte.MaxValue);
                        }
                        else
                        // if (value >= 510)
                        {
                            red = blue = byte.MaxValue;
                            green = (byte)(value - 509);
                        }
                    }
                    else
                    {
                        if (value == 0)
                        {
                            green = red = blue = 0;
                        }
                        else
                        {
                            green = 0;
                            red = (byte)(value % 256);
                            blue = (byte)(value / 256);
                        }
                    }

                    grayscaleMap[offset] = new Color32(red, green, blue, alpha);
                }
            }

            grayscaleImage.SetPixels32(grayscaleMap);
            grayscaleImage.Apply();
            grayscaleBuffer = ImageConversion.EncodeToPNG(grayscaleImage);

            return grayscaleBuffer;
        }

        public static byte[] ConvertToGrayscale(byte[,] map)
        {
            Texture2D grayscaleImage = new Texture2D(map.GetLength(0), map.GetLength(1));
            Color32[] grayscaleMap = new Color32[map.Length];
            byte[] grayscaleBuffer = new byte[map.Length * 4];
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    int offset = (((map.GetLength(1) - y - 1) * map.GetLength(0)) + x);
                    int value = map[x, y];
                    grayscaleMap[offset] = new Color32((byte)value, (byte)value, (byte)value, 255);
                }
            }
            grayscaleImage.SetPixels32(grayscaleMap);
            grayscaleImage.Apply();
            grayscaleBuffer = ImageConversion.EncodeToPNG(grayscaleImage);

            return grayscaleBuffer;
        }

        public static byte[] ConvertToGrayscale((byte, byte)[,] map, int trailType)
        {
            Texture2D grayscaleImage = new Texture2D(map.GetLength(0), map.GetLength(1));
            Color32[] grayscaleMap = new Color32[map.Length];
            byte[] grayscaleBuffer = new byte[map.Length * 4];
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    int offset = (((map.GetLength(1) - y - 1) * map.GetLength(0)) + x);
                    int value = 0;
                    int r = 0;
                    int g = 0;
                    int b = 0;
                    
                    if (trailType == 1)
                        value = map[x, y].Item1;
                    else if (trailType == 2) 
                        value = map[x, y].Item2;
                    else if (map[x, y].Item1 != 0 || map[x, y].Item2 != 0)
                    {
                       r = map[x, y].Item1;
                       g = byte.MaxValue;
                       b = map[x, y].Item2;
                    }
                    
                    if (trailType == 0)
                    {
                        grayscaleMap[offset] = new Color32((byte)r, (byte)g, (byte)b, 255);
                    }
                    else
                        grayscaleMap[offset] = new Color32((byte)value, (byte)value, (byte)value, 255);
                }
            }
            grayscaleImage.SetPixels32(grayscaleMap);
            grayscaleImage.Apply();
            grayscaleBuffer = ImageConversion.EncodeToPNG(grayscaleImage);

            return grayscaleBuffer;
        }

        public static void ConvertToMatrix(Texture2D imageToTranslate, out int[,] matrix, bool isTrailMap = false)
        {
            Color32[] grayscaleMap = imageToTranslate.GetPixels32();
            // byte[] buffer = imageToTranslate.GetRawTextureData();
            matrix = new int[imageToTranslate.width, imageToTranslate.height];
            for (int x = 0; x < imageToTranslate.width; x++)
            {
                for (int y = 0; y < imageToTranslate.height; y++)
                {
                    int offset = (((imageToTranslate.height - y - 1) * imageToTranslate.width) + x);

                    int value = -1;
                    int r, g, b;
                    r = g = b = 0;

                    if (!isTrailMap)
                    {
                        if (grayscaleMap[offset].g == grayscaleMap[offset].r && grayscaleMap[offset].g == grayscaleMap[offset].b)
                            value = (int)grayscaleMap[offset].g;
                        else if (grayscaleMap[offset].g == 255 && grayscaleMap[offset].b == 255)
                            value = (int)grayscaleMap[offset].g + grayscaleMap[offset].r;
                        else if (grayscaleMap[offset].r == 255 && grayscaleMap[offset].b == 255)
                            value = (int)grayscaleMap[offset].g + 509;
                    }
                    else                   
                        value = grayscaleMap[offset].r + (grayscaleMap[offset].b * byte.MaxValue);

                    matrix[x, y] = value;
                }
            }
        }

        public static byte[,] ConvertToMatrix(Texture2D imageToTranslate)
        {
            Color32[] grayscaleMap = imageToTranslate.GetPixels32();
            // byte[] buffer = imageToTranslate.GetRawTextureData();
            byte[,] matrix = new byte[imageToTranslate.width, imageToTranslate.height];
            for (int x = 0; x < imageToTranslate.width; x++)
            {
                for (int y = 0; y < imageToTranslate.height; y++)
                {
                    int offset = (((imageToTranslate.height - y - 1) * imageToTranslate.width) + x);
                    byte value = grayscaleMap[offset].g;
                    matrix[x, y] = value;
                }
            }
            return matrix;
        }

        public static void InspectHeightmap(bool isLight)
        {
            string path = "/home/arneb/DFU_resources/Tamriel_Heightmap";
            string mapToLoad;
            topValue = 0;
            bottomValue = byte.MaxValue;
            Texture2D imageToTranslate = new Texture2D(1, 1);

            if (isLight)
            {
                mapToLoad = "Tamriel_Light.png";
            }
            else
            { 
                mapToLoad = "Tamriel_Dark.png";
            }

            
            // ImageConversion.LoadImage(imageToTranslate, File.ReadAllBytes(Path.Combine(path, mapToLoad)));            
            // byte[,] heightMap = new byte [imageToTranslate.width, imageToTranslate.height];
            // heightMap = MapEditor.ConvertToMatrix(imageToTranslate);

            byte[] heightMap = new byte [20481 * 16385];
            heightMap = File.ReadAllBytes(Path.Combine(path, "Tamriel_Light.data"));

            // for (int x = 0; x < imageToTranslate.width; x++)
            // {
            //     for (int y = 0; y < imageToTranslate.height; y++)
            //     {

            for (int i = 0; i < heightMap.Length; i++)
            {
                byte analyzedPixel = heightMap[i];

                if (analyzedPixel > topValue)
                        topValue = analyzedPixel;

                if (analyzedPixel < bottomValue)
                        bottomValue = analyzedPixel;
            }
                    // byte analyzedPixel = heightMap[x, y];

                    // if (analyzedPixel > topValue)
                    //     topValue = analyzedPixel;

                    // if (analyzedPixel < bottomValue)
                    //     bottomValue = analyzedPixel;
            //     }
            // }

            EditorGUILayout.LabelField("Map: ", mapToLoad, EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Highest Point: ", topValue.ToString());
            EditorGUILayout.LabelField("Lowest Point", bottomValue.ToString());
        }
    }    

    /// <summary>
    /// File containing regions and locations data
    /// </summary>
    public class Worldmap
    {
        #region Class Variables

        public string Name;
        public int LocationCount;
        public string[] MapNames;
        public DFRegion.RegionMapTable[] MapTable;
        public Dictionary<ulong, int> MapIdLookup;
        public Dictionary<string, int> MapNameLookup;
        public DFLocation[] Locations;

        #endregion
    }

    public static class Worldmaps
    {
        #region Class Fields

        public static Worldmap[] Worldmap;
        public static Worldmap[] WholeWM;
        public static Dictionary<ulong, MapSummary> mapDict;
        public static List<ulong> locationIdList;
        public static string tilesPath = "/home/arneb/Games/daggerfall/DaggerfallGameFiles/arena2/Maps/Tiles";
        public static string locationsPath = "/home/arneb/Games/daggerfall/DaggerfallGameFiles/arena2/Maps/Locations";

        static Worldmaps()
        {
            Worldmap = LoadWorldMap();
            WholeWM = JsonConvert.DeserializeObject<Worldmap[]>(File.ReadAllText(Path.Combine(MapEditor.testPath, "MapsTest.json")));
            // Debug.Log("Enumerating maps from constructor");
            mapDict = EnumerateMaps();
        }

        #endregion

        #region Variables

        public static Dictionary<ulong, ulong> locationIdToMapIdDict;

        #endregion

        #region Public Methods

        public static ulong ReadLocationIdFast(int region, int location)
        {
            // Added new locations will put the LocationId in regions map table, since it doesn't exist in classic data
            if (Worldmaps.Worldmap[region].MapTable[location].LocationId != 0)
                return Worldmaps.Worldmap[region].MapTable[location].LocationId;

            // Get datafile location count (excluding added locations)
            int locationCount = Worldmaps.Worldmap[region].LocationCount;

            // Read the LocationId
            ulong locationId = Worldmaps.Worldmap[region].Locations[location].Exterior.RecordElement.Header.LocationId;

            return locationId;
        }

        public static DFRegion ConvertWorldMapsToDFRegion(int currentRegionIndex)
        {
            DFRegion dfRegion = new DFRegion();

            if (currentRegionIndex >= Worldmaps.Worldmap.Length)
                return dfRegion;

            dfRegion.Name = Worldmaps.Worldmap[currentRegionIndex].Name;
            dfRegion.LocationCount = Worldmaps.Worldmap[currentRegionIndex].LocationCount;
            dfRegion.MapNames = Worldmaps.Worldmap[currentRegionIndex].MapNames;
            dfRegion.MapTable = Worldmaps.Worldmap[currentRegionIndex].MapTable;
            dfRegion.MapIdLookup = Worldmaps.Worldmap[currentRegionIndex].MapIdLookup;
            dfRegion.MapNameLookup = Worldmaps.Worldmap[currentRegionIndex].MapNameLookup;

            return dfRegion;
        }

        /// <summary>
        /// Gets a DFLocation representation of a location.
        /// </summary>
        /// <param name="region">Index of region.</param>
        /// <param name="location">Index of location.</param>
        /// <returns>DFLocation.</returns>
        public static DFLocation GetLocation(int region, int location)
        {
            // Read location
            DFLocation dfLocation = new DFLocation();
            // Debug.Log("Getting location from region " + region + ", location n." + location);
            dfLocation = Worldmaps.Worldmap[region].Locations[location];

            // Store indices
            dfLocation.RegionIndex = region;
            dfLocation.LocationIndex = location;

            // Generate smaller dungeon when possible
            // if (UseSmallerDungeon(dfLocation))
            //     GenerateSmallerDungeon(ref dfLocation);

            return dfLocation;
        }

        /// <summary>
        /// Attempts to get a Daggerfall location from MAPS.BSA.
        /// </summary>
        /// <param name="regionIndex">Index of region.</param>
        /// <param name="locationIndex">Index of location.</param>
        /// <param name="locationOut">DFLocation data out.</param>
        /// <returns>True if successful.</returns>
        public static bool GetLocation(int regionIndex, int locationIndex, out DFLocation locationOut)
        {
            locationOut = new DFLocation();

            // Get location data
            locationOut = Worldmaps.GetLocation(regionIndex, locationIndex);
            if (!locationOut.Loaded)
            {
                DaggerfallUnity.LogMessage(string.Format("Unknown location RegionIndex='{0}', LocationIndex='{1}'.", regionIndex, locationIndex), true);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets DFLocation representation of a location.
        /// </summary>
        /// <param name="regionName">Name of region.</param>
        /// <param name="locationName">Name of location.</param>
        /// <returns>DFLocation.</returns>
        public static DFLocation GetLocation(string regionName, string locationName)
        {
            // Load region
            int Region = GetRegionIndex(regionName);

            // Check location exists
            if (!Worldmaps.Worldmap[Region].MapNameLookup.ContainsKey(locationName))
                return new DFLocation();

            // Get location index
            int Location = Worldmaps.Worldmap[Region].MapNameLookup[locationName];

            return GetLocation(Region, Location);
        }

        /// <summary>
        /// Attempts to get a Daggerfall location from MAPS.BSA.
        /// </summary>
        /// <param name="regionName">Name of region.</param>
        /// <param name="locationName">Name of location.</param>
        /// <param name="locationOut">DFLocation data out.</param>
        /// <returns>True if successful.</returns>
        public static bool GetLocation(string regionName, string locationName, out DFLocation locationOut)
        {
            locationOut = new DFLocation();

            // Get location data
            locationOut = GetLocation(regionName, locationName);
            if (!locationOut.Loaded)
            {
                DaggerfallUnity.LogMessage(string.Format("Unknown location RegionName='{0}', LocationName='{1}'.", regionName, locationName), true);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets index of region with specified name. Does not change the currently loaded region.
        /// </summary>
        /// <param name="name">Name of region.</param>
        /// <returns>Index of found region, or -1 if not found.</returns>
        public static int GetRegionIndex(string name)
        {
            // Search for region name
            for (int i = 0; i < WorldInfo.WorldSetting.Regions; i++)
            {
                if (Worldmaps.Worldmap[i].Name == name)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Determines if the current WorldCoord has a location.
        /// </summary>
        /// <param name="mapPixelX">Map pixel X.</param>
        /// <param name="mapPixelY">Map pixel Y.</param>
        /// <returns>True if there is a location at this map pixel.</returns>
        public static bool HasLocation(int mapPixelX, int mapPixelY, out MapSummary summaryOut)
        {
            // MapDictCheck();

            ulong id = MapsFile.GetMapPixelID(mapPixelX, mapPixelY);
            int index = GetTileIndex(mapPixelX, mapPixelY);
            
            if (Worldmaps.Worldmap[index].MapIdLookup != null && Worldmaps.Worldmap[index].MapIdLookup.ContainsKey(id))
            {
                summaryOut.ID = id;
                summaryOut.MapID = 0;
                summaryOut.RegionIndex = index;
                summaryOut.MapIndex = Worldmaps.Worldmap[index].MapIdLookup[id];
                summaryOut.LocationType = Worldmaps.Worldmap[index].MapTable[Worldmaps.Worldmap[index].MapIdLookup[id]].LocationType;
                summaryOut.DungeonType = Worldmaps.Worldmap[index].MapTable[Worldmaps.Worldmap[index].MapIdLookup[id]].DungeonType;
                summaryOut.Discovered = Worldmaps.Worldmap[index].MapTable[Worldmaps.Worldmap[index].MapIdLookup[id]].Discovered;
                return true;
            }

            summaryOut = new MapSummary();
            return false;
        }

        public static int GetTileIndex(int x, int y)
        {
            return ((x / MapsFile.TileDim) + (y / MapsFile.TileDim) * MapsFile.TileX);
        }

        public static int GetTileIndex(ulong mapId)
        {
            int x = (int)(mapId % MapsFile.MaxMapPixelX);
            int y = (int)(mapId / MapsFile.MaxMapPixelX);
            return GetTileIndex(x, y);
        }

        /// <summary>
        /// Determines if the current WorldCoord has a location.
        /// </summary>
        /// <param name="mapPixelX">Map pixel X.</param>
        /// <param name="mapPixelY">Map pixel Y.</param>
        /// <returns>True if there is a location at this map pixel.</returns>
        public static bool HasLocation(int mapPixelX, int mapPixelY)
        {
            // MapDictCheck();

            ulong id = MapsFile.GetMapPixelID(mapPixelX, mapPixelY);
            if (mapDict.ContainsKey(id))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Attempts to get a Daggerfall location from MAPS.BSA using a locationId from quest system.
        /// The locationId is different to the mapId, which is derived from location coordinates in world.
        /// At this time, best known way to determine locationId is from LocationRecordElementHeader data.
        /// This is linked to mapId at in EnumerateMaps().
        /// Note: Not all locations have a locationId, only certain key locations
        /// </summary>
        /// <param name="locationId">LocationId of map from quest system.</param>
        /// <param name="locationOut">DFLocation data out.</param>
        /// <returns>True if successful.</returns>
        public static bool GetQuestLocation(ulong locationId, out DFLocation locationOut)
        {
            locationOut = new DFLocation();

            // MapDictCheck();

            // Get mapId from locationId
            ulong mapId = LocationIdToMapId(locationId);
            if (Worldmaps.mapDict.ContainsKey(mapId))
            {
                MapSummary summary = Worldmaps.mapDict[mapId];
                return GetLocation(Worldmaps.GetTileIndex(summary.ID), summary.MapIndex, out locationOut);
            }

            return false;
        }

        /// <summary>
        /// Converts LocationId from quest system to a MapId for map lookups.
        /// </summary>
        /// <param name="locationId">LocationId from quest system.</param>
        /// <returns>MapId if present or -1.</returns>
        public static ulong LocationIdToMapId(ulong locationId)
        {
            if (locationIdToMapIdDict.ContainsKey(locationId))
            {
                return locationIdToMapIdDict[locationId];
            }

            return 0;
        }

        // private static void MapDictCheck()
        // {
        //     // Build map lookup dictionary
        //     if (mapDict == null)
        //         EnumerateMaps();
        // }

        public static Worldmap[] LoadWorldMap()
        {
            Worldmap[] worldMap = new Worldmap[MapsFile.TileX * MapsFile.TileY];
            // int xTile = LocalPlayerGPS.CurrentMapPixel.X / MapsFile.TileDim;
            // int yTile = LocalPlayerGPS.CurrentMapPixel.Y / MapsFile.TileDim;
            int index = 0;

            for (int y = 0; y < MapsFile.TileY; y++)
            {
                for (int x = 0; x < MapsFile.TileX; x++)
                {
                    worldMap[index] = JsonConvert.DeserializeObject<Worldmap>(File.ReadAllText(Path.Combine(Worldmaps.locationsPath, "map" + (x + y * MapsFile.TileX).ToString("00000") + ".json")));
                    index++;
                }
            }

            return worldMap;
        }

        /// <summary>
        /// Build dictionary of locations.
        /// </summary>
        public static Dictionary<ulong, MapSummary> EnumerateMaps()
        {
            //System.Diagnostics.Stopwatch s = System.Diagnostics.Stopwatch.StartNew();
            //long startTime = s.ElapsedMilliseconds;

            Dictionary<ulong, MapSummary> mapDictSwap = new Dictionary<ulong, MapSummary>();
            // locationIdToMapIdDict = new Dictionary<ulong, ulong>();
            locationIdList = new List<ulong>();

            for (int region = 0; region < MapsFile.TileX * MapsFile.TileY; region++)
            {
                DFRegion dfRegion = Worldmaps.ConvertWorldMapsToDFRegion(region);
                for (int location = 0; location < dfRegion.LocationCount; location++)
                {
                    MapSummary summary = new MapSummary();
                    // Get map summary
                    DFRegion.RegionMapTable mapTable = dfRegion.MapTable[location];
                    summary.ID = mapTable.MapId;
                    summary.MapID = Worldmaps.Worldmap[region].Locations[location].Exterior.RecordElement.Header.LocationId;
                    locationIdList.Add(summary.MapID);
                    summary.RegionIndex = region;
                    summary.MapIndex = Worldmaps.Worldmap[region].Locations[location].LocationIndex;

                    summary.LocationType = mapTable.LocationType;
                    summary.DungeonType = mapTable.DungeonType;

                    // TODO: This by itself doesn't account for DFRegion.LocationTypes.GraveyardForgotten locations that start the game discovered in classic
                    summary.Discovered = mapTable.Discovered;

                    // Debug.Log("summary.ID: " + summary.ID + ", summary.RegionIndex: " + summary.RegionIndex + ", Worldmaps.Worldmap[region].Locations[location].Length: " + Worldmaps.Worldmap[region].Locations.Length);
                    mapDictSwap.Add(summary.ID, summary);

                    // Link generatedLocation.MapTableData with mapId - adds ~25ms overhead
                    // ulong locationId = WorldMaps.ReadLocationIdFast(region, location);
                    // locationIdToMapIdDict.Add(locationId, summary.ID);
                }
            }

            locationIdList.Sort();

            string fileDataPath = Path.Combine(MapEditor.testPath, "mapDict.json");
            var json = JsonConvert.SerializeObject(mapDictSwap, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            File.WriteAllText(fileDataPath, json);

            return mapDictSwap;
        }

        /// <summary>
        /// Lookup block name for exterior block from location data provided.
        /// </summary>
        /// <param name="dfLocation">DFLocation to read block name.</param>
        /// <param name="x">Block X coordinate.</param>
        /// <param name="y">Block Y coordinate.</param>
        /// <returns>Block name.</returns>
        public static string GetRmbBlockName(in DFLocation dfLocation, int x, int y)
        {
            int index = y * dfLocation.Exterior.ExteriorData.Width + x;
            return dfLocation.Exterior.ExteriorData.BlockNames[index];
        }

        #endregion
    }

    public struct MapSummary
    {
        public ulong ID;                  // mapTable.MapId & 0x000fffff for dict key and matching with ExteriorData.MapId
        public ulong MapID;               // Full mapTable.MapId for matching with localization key
        public int RegionIndex;
        public int MapIndex;
        public DFRegion.LocationTypes LocationType;
        public DFRegion.DungeonTypes DungeonType;
        public bool Discovered;
    }

    public class ClimateInfo
    {
        #region Class Fields

        public static int[,] Climate;

        static ClimateInfo()
        {
            Texture2D imageToTranslate = new Texture2D(MapsFile.WorldWidth, MapsFile.WorldHeight);
            ImageConversion.LoadImage(imageToTranslate, File.ReadAllBytes(Path.Combine(MapEditor.testPath, "Climate.png")));            
            MapEditor.ConvertToMatrix(imageToTranslate, out Climate);
        }

        public static int[,] ClimateModified;

        #endregion

        public static int[] ConvertToArray(int[,] matrix)
        {
            // int[,] matrix = new int[imageToTranslate.Width * imageToTranslate.Height];
            // ConvertToMatrix(imageToTranslate, out matrix);

            int[] resultingArray = new int[matrix.GetLength(0) * matrix.GetLength(1)];

            for (int x = 0; x < matrix.GetLength(0); x++)
            {
                for (int y = 0; y < matrix.GetLength(1); y++)
                {
                    resultingArray[(y * matrix.GetLength(0)) + x] = matrix[x, y];
                }
            }

            return resultingArray;
        }

        public static int[,] GetTextureMatrix(int xPos, int yPos, string matrixType = "trail")
        {
            // DFPosition position = WorldMaps.LocalPlayerGPS.CurrentMapPixel;
            // int xPos = position.X / MapsFile.TileDim;
            // int yPos = position.Y / MapsFile.TileDim;
            int sizeModifier = 1;
            if (matrixType.Equals("trailExp"))
                sizeModifier = 5;

            int[,] matrix = new int[MapsFile.TileDim * 3 * sizeModifier, MapsFile.TileDim * 3 * sizeModifier];
            int[][,] textureArray = new int[9][,];


            for (int i = 0; i < 9; i++)
            {
                int posX = xPos + (i % 3 - 1);
                int posY = yPos + (i / 3 - 1);
                if (posX < 0) posX = 0;
                if (posY < 0) posY = 0;
                if (posX >= MapsFile.TileX) posX = MapsFile.TileX - 1;
                if (posY >= MapsFile.TileY) posY = MapsFile.TileY - 1;

                Texture2D textureTiles = new Texture2D(MapsFile.TileDim * sizeModifier, MapsFile.TileDim * sizeModifier);
                if (File.Exists(Path.Combine(MapEditor.arena2Path, "Maps", "Tiles", matrixType + "_" + posX + "_" + posY + ".png")))
                {
                    if (!ImageConversion.LoadImage(textureTiles, File.ReadAllBytes(Path.Combine(Worldmaps.tilesPath, matrixType + "_" + posX + "_" + posY + ".png"))))
                        Debug.Log("Failed to load tile " + matrixType + "_" + posX + "_" + posY + ".png");
                    else Debug.Log("Tile " + matrixType + "_" + posX + "_" + posY + ".png succesfully loaded");

                    if (matrixType == "climate")
                        ConvertToMatrix(textureTiles, out textureArray[i]);
                    else if (matrixType == "trailExp" || matrixType == "trail")
                        ConvertToMatrix(textureTiles, out textureArray[i], true);
                    // else PoliticData.ConvertToMatrix(textureTiles, out textureArray[i]);
                }
                else
                {
                    Debug.Log("tile " + matrixType + "_" + posX + "_" + posY + ".png not present.");
                    textureArray[i] = new int[MapsFile.TileDim * sizeModifier, MapsFile.TileDim * sizeModifier];
                }
            }

            for (int x = 0; x < MapsFile.TileDim * 3 * sizeModifier; x++)
            {
                for (int y = 0; y < MapsFile.TileDim * 3 * sizeModifier; y++)
                {
                    int xTile = x / (MapsFile.TileDim * sizeModifier);
                    int yTile = y / (MapsFile.TileDim * sizeModifier);
                    int X = x % (MapsFile.TileDim * sizeModifier);
                    int Y = y % (MapsFile.TileDim * sizeModifier);

                    matrix[x, y] = textureArray[yTile * 3 + xTile][X, Y];
                }
            }

            return matrix;
        }

        public static void ConvertToMatrix(Texture2D imageToTranslate, out int[,] matrix, bool isTrail = false)
        {
            Color32[] grayscaleMap = imageToTranslate.GetPixels32();
            // byte[] buffer = imageToTranslate.GetRawTextureData();
            // Debug.Log("buffer.Length: " + buffer.Length);
            matrix = new int[imageToTranslate.width, imageToTranslate.height];
            for (int x = 0; x < imageToTranslate.width; x++)
            {
                for (int y = 0; y < imageToTranslate.height; y++)
                {
                    int offset = (((imageToTranslate.height - y - 1) * imageToTranslate.width) + x);

                    if (!isTrail)
                    {
                        byte value = grayscaleMap[offset].g;
                        matrix[x, y] = (int)value;
                    }
                    else{
                        int intValue = grayscaleMap[offset].r + grayscaleMap[offset].b * (byte.MaxValue + 1);
                        matrix[x, y] = intValue;
                    }
                }
            }
        }
    }

    public static class PoliticInfo
    {
        #region Class Fields

        public static int[,] Politic;

        static PoliticInfo()
        {
            Texture2D imageToTranslate = new Texture2D(MapsFile.WorldWidth, MapsFile.WorldHeight);
            ImageConversion.LoadImage(imageToTranslate, File.ReadAllBytes(Path.Combine(MapEditor.testPath, "Politic.png")));            
            MapEditor.ConvertToMatrix(imageToTranslate, out Politic);
        }

        #endregion

        #region Public Methods

        public static bool IsBorderPixel(int x, int y, int actualPolitic)
        {
            int politicIndex = Politic[x, y];

            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    int X = x + i;
                    int Y = y + j;

                    if ((X < 0 || X > MapsFile.MaxMapPixelX || Y < 0 || Y > MapsFile.MaxMapPixelY) ||
                        (i == 0 && j == 0))
                        continue;

                    if (politicIndex != Politic[X, Y] &&
                        politicIndex != actualPolitic &&
                        (SmallHeightmap.GetHeightMapValue(x, y) > 3) &&
                        (SmallHeightmap.GetHeightMapValue(X, Y) > 3))
                        return true;
                }
            }

            return false;
        }

        public static int ConvertMapPixelToRegionIndex(int x, int y)
        {
            int regionIndex = Politic[x, y];

            if (regionIndex == 0 || regionIndex < 128)
                return 31;

            regionIndex -= 128;
            return regionIndex;
        }

        #endregion
    }

    public static class SmallHeightmap
    {
        #region Class Fields

        public static byte[,] Woods;

        static SmallHeightmap()
        {
            Texture2D imageToTranslate = new Texture2D(MapsFile.WorldWidth, MapsFile.WorldHeight);
            ImageConversion.LoadImage(imageToTranslate, File.ReadAllBytes(Path.Combine(MapEditor.testPath, "Woods.png")));            
            Woods = MapEditor.ConvertToMatrix(imageToTranslate);
            // Woods = JsonConvert.DeserializeObject<byte[,]>(File.ReadAllText(Path.Combine(MapEditor.testPath, "Woods.json")));
        }

        #endregion

        #region Public Methods

        public static Byte[] GetHeightMapValuesRange1Dim(int mapPixelX, int mapPixelY, int dim)
        {
            Byte[] dstData = new Byte[dim * dim];
            for (int y = 0; y < dim; y++)
            {
                for (int x = 0; x < dim; x++)
                {
                    dstData[x + (y * dim)] = GetHeightMapValue(mapPixelX + x, mapPixelY + y);
                }
            }
            return dstData;
        }

        public static Byte GetHeightMapValue(int mapPixelX, int mapPixelY)
        {
            // Clamp X
            if (mapPixelX < 0) mapPixelX = 0;
            if (mapPixelX >= MapsFile.MaxMapPixelX) mapPixelX = MapsFile.MaxMapPixelX - 1;

            // Clamp Y
            if (mapPixelY < 0) mapPixelY = 0;
            if (mapPixelY >= MapsFile.MaxMapPixelY) mapPixelY = MapsFile.MaxMapPixelY - 1;

            return Woods[mapPixelX, mapPixelY];
        }

        public static Byte GetHeightMapValue(DFPosition position)
        {
            // No clamping for this method override
            return Woods[position.X, position.Y];
        }

        #endregion
    }

    // public static class LargeHeightmap()
    // {
    //     public static byte[,] WoodsLarge;

    //     static LargeHeightmap()
    //     {
    //         WoodsLarge = new byte[MapsFile.WorldWidth * MapsFile.WorldHeight * 25];
    //         WoodsLarge = JsonConvert.DeserializeObject<byte[,]>(File.ReadAllText(Path.Combine(testPath, "WoodsLarge.json")));
    //     }
    // }

    public static class TrailsInfo
    {
        public static byte[,] Trails;

        static TrailsInfo()
        {
            Texture2D imageToTranslate = new Texture2D(MapsFile.WorldWidth, MapsFile.WorldHeight);
            ImageConversion.LoadImage(imageToTranslate, File.ReadAllBytes(Path.Combine(MapEditor.testPath, "Trails.png")));            
            Trails = MapEditor.ConvertToMatrix(imageToTranslate);
            // Woods = JsonConvert.DeserializeObject<byte[,]>(File.ReadAllText(Path.Combine(MapEditor.testPath, "Woods.json")));
        }
    }

    public static class WorldInfo
    {
        public static WorldStats WorldSetting;

        static WorldInfo()
        {
            WorldSetting = new WorldStats();
            WorldSetting = JsonConvert.DeserializeObject<WorldStats>(File.ReadAllText(Path.Combine(MapEditor.testPath, "WorldData.json")));
        }
    }

    public static class NameGen
    {

    }

    public class WorldStats
    {
        public int Regions;
        public string[] RegionNames;
        // public int WorldWidth;
        // public int WorldHeight;
        public int[] regionRaces;
        public int[] regionTemples;
        public int[] regionBorders;

        public WorldStats()
        {
            
        }
    }

    public static class FactionAtlas
    {
        public static Dictionary<int, FactionFile.FactionData> FactionDictionary = new Dictionary<int, FactionFile.FactionData>();
        public static Dictionary<string, int> FactionToId = new Dictionary<string, int>();

        static FactionAtlas()
        {
            FactionDictionary = JsonConvert.DeserializeObject<Dictionary<int, FactionFile.FactionData>>(File.ReadAllText(Path.Combine(MapEditor.testPath, "Faction.json")));

            foreach (KeyValuePair<int, FactionFile.FactionData> faction in FactionDictionary)
            {
                if (!FactionToId.ContainsKey(faction.Value.name))
                    FactionToId.Add(faction.Value.name, faction.Key);
                else {
                    FactionToId.Add(faction.Value.name + "_(" + ((FactionFile.FactionIDs)faction.Value.parent).ToString() + ")", faction.Key);
                    int indexSwap = FactionToId[faction.Value.name];
                    FactionFile.FactionData factionSwap = FactionDictionary[indexSwap];
                    FactionToId.Remove(faction.Value.name);
                    FactionToId.Add(factionSwap.name + "_(" + ((FactionFile.FactionIDs)factionSwap.parent).ToString() + ")", indexSwap);
                }
            }
        }

        public static bool GetFactionData(int factionID, out FactionFile.FactionData factionDataOut)
        {
            factionDataOut = new FactionFile.FactionData();
            if (FactionAtlas.FactionDictionary.ContainsKey(factionID))
            {
                factionDataOut = FactionAtlas.FactionDictionary[factionID];
                return true;
            }

            return false;
        }
    }

    public static class LocationNamesList
    {
        public enum NameTypes
        {
            HighRockVanilla,
            HighRockModern,
            Hammerfell,
            Skyrim,
            Reachmen,
            Morrowind,
            SumursetIsle,
            Valenwood,
            Elsweyr,
            BlackMarsh,
            Cyrodiil,
            Orsinium
        }

        public enum NameParts
        {
            Prefix = 0,
            Suffix1 = 1,
            Suffix2 = 2,
            Extra = 3,
        }

        public enum PoorHomeEpithets
        {
            Cabin,
            Hovel,
            Place,
            Shack,
        }

        public enum WealthyHomeEpithets
        {
            
            Court,            
            Hall,            
            Manor,            
            Palace,
        }

        public enum FarmsteadEpithets
        {
            Farm,
            Farmstead,
            Grange,
            Orchard,
            Plantation,
        }

        public enum TavernEpithets
        {
            Hostel,
            Inn,
            Lodge,
            Pub,
            Tavern
        }

        public enum TempleParts
        {
            Deity,
            Adjectif,
            Noun,
            Shrine
        }

        public enum RMBTypes
        {
            Town,
            Dungeon,
            Home,
            Knight,
            Special
        }

        public enum RDBTypes
        {
            Castle,
            Tower,
            HarpyNest,
            BugsNest,
            Cave,
            Mine,
            Haunt,
            Coven,
            Laboratory,
            Temple,
            Crypt,
            Prison,
            Ruins,
            DragonsDen,
            VolcanicCaves,
            Cemetery,
            Generic,
            Border
        }

        public static string[][][] NamesList;
        public static string[][] DungeonNamesList;
        public static string[][] TempleNamesList;
        public static string[][] RMBNames;
        public static string[][] RDBNames;

        static LocationNamesList()
        {
            NamesList = new string[Enum.GetNames(typeof(NameTypes)).Length][][];
            NamesList = JsonConvert.DeserializeObject<string[][][]>(File.ReadAllText(Path.Combine(MapEditor.testPath, "LocationNames.json")));

            DungeonNamesList = new string[Enum.GetNames(typeof(DFRegion.DungeonTypes)).Length][];
            DungeonNamesList = JsonConvert.DeserializeObject<string[][]>(File.ReadAllText(Path.Combine(MapEditor.testPath, "DungeonEpithets.json")));

            TempleNamesList = new string[Enum.GetNames(typeof(TempleParts)).Length][];
            TempleNamesList = JsonConvert.DeserializeObject<string[][]>(File.ReadAllText(Path.Combine(MapEditor.testPath, "TempleEpithets.json")));

            RMBNames = new string[Enum.GetNames(typeof(RMBTypes)).Length][];
            RMBNames = JsonConvert.DeserializeObject<string[][]>(File.ReadAllText(Path.Combine(MapEditor.testPath, "RMBlocks.json")));

            RDBNames = new string[Enum.GetNames(typeof(RDBTypes)).Length][];
            RDBNames = JsonConvert.DeserializeObject<string[][]>(File.ReadAllText(Path.Combine(MapEditor.testPath, "DungeonRDBs.json")));
        }
    }
}