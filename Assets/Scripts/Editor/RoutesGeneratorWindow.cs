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

namespace MapEditor
{
    public class GenerateRoutesWindow : EditorWindow
    {
        static GenerateRoutesWindow generateRoutesWindow;
        const string windowTitle = "Routes Generator";

        public const int W = 128;
        public const int NW = 64;
        public const int N = 32;
        public const int NE = 16;
        public const int E = 8;
        public const int SE = 4;
        public const int S = 2;
        public const int SW = 1;

        public const long DIRTINDEX = 256;
        public const long TRACKINDEX = DIRTINDEX * DIRTINDEX;
        public const long FUNCINDEX = DIRTINDEX * TRACKINDEX;

        public const byte walledInternalSign = 27;
        public const byte internalSign = 26;

        const byte riseDeclineLimit = 4;
        const int routeCompletionLevelRequirement = 4;
        const int minimumWaveScan = 10;
        const int maximumWaveScan = 50;
        public int xCoord, yCoord, xTile, yTile;

        // routeGenerationOrder set the order in which trails are generated
        public static int[] routeGenerationOrder = { 0, 1, 2, 6, 8, 5, 3, 11, 12, 9, 4, 7, 10};
        public Roadsign signData;

        public class Roadsign
        {
            public List<string> locNames;
            public Dictionary<long, DFPosition> signPosition;
            public Dictionary<long, string[][]> roadsign;

            public Roadsign()
            {
                this.locNames = new List<string>();
                this.signPosition = new Dictionary<long, DFPosition>();
                this.roadsign = new Dictionary<long, string[][]>();
            }
        }

        public class RoutedLocation
        {
            public string name;
            public DFPosition position;
            public List<(ulong, float)> locDistance;
            public TrailTypes trailPreference;
            public byte completionLevel;
            public DFRegion.LocationTypes locType;

            public RoutedLocation()
            {
                this.name = string.Empty;                       // Name of routed location to use in roadsigns
                this.position = new DFPosition(0, 0);           // The coordinates of the location to route
                this.locDistance = new List<(ulong, float)>();  // Item1 refers to a location to reach, Item2 to the distance to that location
                this.trailPreference = TrailTypes.None;         // The type of trail this location will generate, if there aren't others
                this.completionLevel = 0;                       // An indicator of how many locations have been reached starting from this position
                this.locType = DFRegion.LocationTypes.None;     // Routed location type, to see if it has to be inserted in signpost location list
            }
        }

        public enum TrailTypes
        {
            None,
            Road = 1,
            DirtRoad = 2,
            Track = 3
        }

        void Awake()
        {
            
        }

        void OnGUI()
        {
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

            if (GUILayout.Button("Generate Routes", GUILayout.MaxWidth(200.0f)))
            {
                GenerateRoutes();
            }
        }

        protected void GenerateRoutes()
        {
            signData = new Roadsign();
            List<RoutedLocation>[] routedLocations = new List<RoutedLocation>[Enum.GetNames(typeof(DFRegion.LocationTypes)).Length];
            // for (int h = 0; h < Enum.GetNames(typeof(DFRegion.LocationTypes)).Length; h++)
            // {
            //     routedLocations[h] = new List<RoutedLocation>();
            // }
            List<DFPosition> routedLocSurroundings = SetLocSurroundings(out routedLocations);
            long[] pathData = ConvertToArray(GetTextureMatrix(xTile, yTile, "trail"));
            
            List<RoutedLocation> routedLocationsComplete = new List<RoutedLocation>();
            (byte, byte, byte, byte)[,] trailMap = new (byte, byte, byte, byte)[MapsFile.TileDim * 3, MapsFile.TileDim * 3];
            trailMap = LoadExistingTrails(pathData);

            // First we set values for routedLocations, to have a reference for which locations must be routed and in which way
            for (int j = 0; j < routedLocations.Length; j++)
            {
                // Debug.Log("routedLocations.Length: " + routedLocations.Length);
                if (j == (int)DFRegion.LocationTypes.Coven ||
                    j == (int)DFRegion.LocationTypes.HiddenLocation ||
                    j == (int)DFRegion.LocationTypes.HomeYourShips ||
                    j == (int)DFRegion.LocationTypes.None)
                    continue;

                for (int k = 0; k < routedLocations[j].Count; k++)
                {
                    FactionFile.FactionData region = FactionAtlas.FactionDictionary[FactionAtlas.FactionToId[WorldInfo.WorldSetting.RegionNames[PoliticInfo.ConvertMapPixelToRegionIndex(routedLocations[j][k].position.X, routedLocations[j][k].position.Y)]]];
                    int governmentType = (region.ruler + 1) / 2;
                    Debug.Log("routedLocations[" + j + "][" + k + "].position: " + routedLocations[j][k].position.X + ", " + routedLocations[j][k].position.Y + "; region: " + region.name + "; governmentType: " + governmentType);
                    int governmentMultiplier = GovTypeToGovMultiplier(governmentType);

                    switch (j)
                    {
                        case (int)DFRegion.LocationTypes.TownCity:
                            routedLocations[j][k].trailPreference = GetRandomTrail(governmentMultiplier, 50, 100, 100);
                            break;

                        case (int)DFRegion.LocationTypes.TownHamlet:
                            routedLocations[j][k].trailPreference = GetRandomTrail(governmentMultiplier, 40, 80, 100);
                            break;
                        case (int)DFRegion.LocationTypes.TownVillage:
                        case (int)DFRegion.LocationTypes.Tavern:
                            routedLocations[j][k].trailPreference = GetRandomTrail(governmentMultiplier, 20, 50, 100);
                            break;

                        case (int)DFRegion.LocationTypes.HomeFarms:
                            routedLocations[j][k].trailPreference = GetRandomTrail(governmentMultiplier, 2, 40, 100);
                            break;

                        case (int)DFRegion.LocationTypes.HomeWealthy:
                            routedLocations[j][k].trailPreference = GetRandomTrail(governmentMultiplier, 25, 60, 100);
                            break;

                        case (int)DFRegion.LocationTypes.ReligionTemple:
                            routedLocations[j][k].trailPreference = GetRandomTrail(governmentMultiplier, 10, 30, 100);
                            break;

                        // Locations that will have a track in most cases
                        case (int)DFRegion.LocationTypes.Graveyard:
                            routedLocations[j][k].trailPreference = GetRandomTrail(governmentMultiplier, 5, 15, 30);
                            break;

                        // Locations that have the same chances to have a track or no trail at all
                        case (int)DFRegion.LocationTypes.ReligionCult:
                        case (int)DFRegion.LocationTypes.HomePoor:
                            routedLocations[j][k].trailPreference = GetRandomTrail(governmentMultiplier, 0, 5, 15);
                            break;

                        // Dungeons are special cases
                        case (int)DFRegion.LocationTypes.DungeonLabyrinth:
                        case (int)DFRegion.LocationTypes.DungeonKeep:
                        case (int)DFRegion.LocationTypes.DungeonRuin:
                            MapSummary location = new MapSummary();
                            if (!Worldmaps.HasLocation(routedLocations[j][k].position.X, routedLocations[j][k].position.Y, out location))
                                break;

                            if (CheckDungeonRoutability(location.DungeonType))
                            {
                                switch (location.DungeonType)
                                {
                                    case DFRegion.DungeonTypes.Prison:
                                        routedLocations[j][k].trailPreference = GetRandomTrail(governmentMultiplier, 15, 100, 100);
                                        break;

                                    case DFRegion.DungeonTypes.HumanStronghold:
                                        routedLocations[j][k].trailPreference = GetRandomTrail(governmentMultiplier, 10, 100, 100);
                                        break;
                                    case DFRegion.DungeonTypes.Laboratory:
                                        routedLocations[j][k].trailPreference = GetRandomTrail(governmentMultiplier, 5, 40, 100);
                                        break;

                                    case DFRegion.DungeonTypes.BarbarianStronghold:
                                        routedLocations[j][k].trailPreference = GetRandomTrail(governmentMultiplier, 0, 1, 100);
                                        break;

                                    case DFRegion.DungeonTypes.Cemetery:
                                        routedLocations[j][k].trailPreference = GetRandomTrail(governmentMultiplier, 1, 5, 15);
                                        break;

                                    case DFRegion.DungeonTypes.DesecratedTemple:
                                        routedLocations[j][k].trailPreference = GetRandomTrail(governmentMultiplier, 0, 1, 10);
                                        break;

                                    case DFRegion.DungeonTypes.GiantStronghold:
                                        routedLocations[j][k].trailPreference = GetRandomTrail(governmentMultiplier, 0, 0, 100);
                                        break;

                                    case DFRegion.DungeonTypes.RuinedCastle:
                                        routedLocations[j][k].trailPreference = GetRandomTrail(governmentMultiplier, 0, 5, 100);
                                        break;

                                    default:
                                        break;
                                }
                            }
                            break;
                    }
                }
            }

            // Then we start drawing trails proper, a location type at a time, in the order given by routeGenerationOrder
            // First, we make a list of potential connection to every location that has to be routed, and sort it by distance
            for (int r = 0; r < routeGenerationOrder.Length; r++)
            {
                // Debug.Log("routedLocations[" + routeGenerationOrder[r] + "].Count: " + routedLocations[routeGenerationOrder[r]].Count);
                List<RoutedLocation> swapList = new List<RoutedLocation>();
                if (routedLocations[routeGenerationOrder[r]] == null)
                    continue;

                foreach (RoutedLocation locToRoute in routedLocations[routeGenerationOrder[r]])
                {
                    RoutedLocation swapLoc = locToRoute;
                    int distanceChecked = 0;

                    do
                    {
                        distanceChecked++;
                        swapLoc = CircularWaveScan(swapLoc, distanceChecked, ref routedLocations, routedLocSurroundings);
                    }
                    while ((swapLoc.completionLevel < routeCompletionLevelRequirement || distanceChecked < minimumWaveScan) && distanceChecked < maximumWaveScan);

                    swapLoc.locDistance.Sort((a, b) => a.Item2.CompareTo(b.Item2));
                    // for (int sld = 0; sld < swapLoc.locDistance.Count; sld++)
                        // Debug.Log("swapLoc.locDistance[" + sld + "].Item1: " + swapLoc.locDistance[sld].Item1);
                    swapList.Add(swapLoc);
                    // Debug.Log("swapList[0].locDistance[0].Item1: " + swapList[0].locDistance[0].Item1);
                }

                // routedLocations[routeGenerationOrder[r]] = new List<RoutedLocation>();
                // routedLocations[routeGenerationOrder[r]] = swapList;
                routedLocationsComplete.AddRange(swapList);
                // Debug.Log("routedLocationsComplete[0].locDistance[0].Item1: " + routedLocationsComplete[0].locDistance[0].Item1);
            }

            // Second, we compare different routed locations. Let's consider locations A, B and C: if A has to be connected with
            // both B and C, and B has to be connected to C too, and B is (significantly?) closer to C than A is,
            // we remove A connection to C.
            List<RoutedLocation> routedLocCompSwap = routedLocationsComplete;
            // Debug.Log("routedLocCompSwap[0].locDistance[0].Item1: " + routedLocCompSwap[0].locDistance[0].Item1);
            RoutedLocation locToRoute0Swap = new RoutedLocation();

            foreach (RoutedLocation locToRoute0 in routedLocationsComplete)
            {
                // int index = 0;
                List<RoutedLocation> locListCompare = new List<RoutedLocation>();
                List<(ulong, float)> locDistanceToCompare = new List<(ulong, float)>();
                locToRoute0Swap = locToRoute0;

                locToRoute0Swap = CompareLocDistance(locToRoute0, out locListCompare, ref routedLocCompSwap);

                foreach (RoutedLocation modLoc in locListCompare)
                {
                    routedLocCompSwap = MergeModifiedLocDist(routedLocCompSwap, modLoc);
                }

                // foreach((ulong, float) rLoc in locToRoute0.locDistance)
                // {
                //     locToRouteCompare = routedLocCompSwap.Find(x => x.position.Equals(MapsFile.GetPixelFromPixelID(rLoc.Item1)));


                //     index = routedLocCompSwap.FindIndex(y => y.position.Equals(MapsFile.GetPixelFromPixelID(rLoc.Item1)));
                //     // int index = 0;
                //     locDistanceToCompare.Add(rLoc);

                //     // locToRouteCompare = routedLocationsComplete.Find(x => x.position.Equals(MapsFile.GetPixelFromPixelID(rLoc.Item1)));

                //     // foreach ((ulong, float) rLoc2 in locToRoute0.locDistance)
                //     // {
                //     //     if (locToRouteCompare.locDistance.Exists(y => y.Item1 == rLoc2.Item1))
                //     //     {
                //     //         index = locToRouteCompare.locDistance.FindIndex(z => z.Item1 == rLoc2.Item1);

                //     //         if (rLoc2.Item2 < locToRouteCompare.locDistance[index].Item2)
                //     //         {
                //     //             locToRouteCompare.locDistance.RemoveAt(index);
                //     //             locToRouteCompare.completionLevel--;
                //     //         }
                //     //         else{
                //     //             locToRoute0Swap.locDistance.Remove(rLoc2);
                //     //             locToRoute0Swap.completionLevel--;
                //     //         }
                //     //     }
                //     // }

                //     // Debug.Log("locToRoute0.locDistance.Count: " + locToRoute0.locDistance.Count);

                //     // index = routedLocationsComplete.FindIndex(a => a.position == (MapsFile.GetPixelFromPixelID(rLoc.Item1)));
                //     // routedLocationsComplete.RemoveAt(index);
                //     // routedLocationsComplete.Insert(index, locToRouteCompare);
                // }

                routedLocCompSwap = MergeModifiedLocDist(routedLocCompSwap, locToRoute0Swap);

                routedLocCompSwap = PruneDoubleTrails(routedLocCompSwap);

                // Debug.Log("routedLocCompSwap.Count: " + routedLocCompSwap.Count);
                // Debug.Log("routedLocationsComplete.Count: " + routedLocationsComplete.Count);
            }

            routedLocationsComplete = routedLocCompSwap;
            string[][] roadpostData = new string[8][];
            
            string trailDataPath = Path.Combine(MapEditor.arena2Path, "Maps", "TrailData", "trailData_" + xTile + "_" + yTile + ".json");
            var json = JsonConvert.SerializeObject(routedLocationsComplete, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            File.WriteAllText(trailDataPath, json);

            // Third, we search for the best way to connect two locations, starting from the shortest route
            // and falling back to longer trails if adjacent pixels have a too steep rise/decline
            foreach (RoutedLocation locToRoute1 in routedLocationsComplete)
            {
                string strtName, arrvName = string.Empty;
                
                strtName = locToRoute1.name;
                int strtNameIndex = signData.locNames.FindIndex(x => x.Equals(strtName));

                foreach ((ulong, float) destination in locToRoute1.locDistance)
                {
                    // Debug.Log("locToRoute1.locDistance.Count: " + locToRoute1.locDistance.Count);
                    List<DFPosition> trail = new List<DFPosition>();
                    List<DFPosition> movementChoice = new List<DFPosition>();
                    int xDirection, yDirection, xDiff, yDiff;
                    DFPosition currentPosition = locToRoute1.position;
                    DFPosition arrivalDestination = MapsFile.GetPixelFromPixelID(destination.Item1);

                    int tileIndex = Worldmaps.GetTileIndex(destination.Item1);
                    
                    // Debug.Log("destination.Item1: " + destination.Item1 + ", tileIndex: " + tileIndex);
                    arrvName = (Worldmaps.GetLocation(Worldmaps.GetTileIndex(destination.Item1), Worldmaps.mapDict[destination.Item1].MapIndex)).Name;
                    int arrvNameIndex = -1;
                    if (!signData.locNames.Contains(arrvName))
                        signData.locNames.Add(arrvName);

                    arrvNameIndex = signData.locNames.FindIndex(y => y.Equals(arrvName));                    

                    CalculateDirectionAndDiff(currentPosition, arrivalDestination, out xDirection, out yDirection, out xDiff, out yDiff);

                    bool arrivedToDestination = false;
                    bool tryLongTrail = false;
                    bool noWay = false;
                    List<DFPosition> deadEnds = new List<DFPosition>();
                    int xDiffRatio, yDiffRatio;
                    int trailWorkProgress = 0;
                    trail.Add(currentPosition);

                    Debug.Log("Starting trail between " + currentPosition.X + ", " + currentPosition.Y + " and " + arrivalDestination.X + ", " + arrivalDestination.Y);

                    do
                    {
                        bool viableWay = false;
                        arrivedToDestination = false;
                        List<DFPosition> stepToRemove = new List<DFPosition>();
                        movementChoice = new List<DFPosition>();

                        currentPosition = trail[trailWorkProgress];
                        CalculateDirectionAndDiff(currentPosition, arrivalDestination, out xDirection, out yDirection, out xDiff, out yDiff);
                        byte startingHeight = SmallHeightmap.GetHeightMapValue(currentPosition);

                        if (trailWorkProgress == 0 && deadEnds.Count > 0)
                            tryLongTrail = true;
                        else tryLongTrail = false;
                        movementChoice = ProbeDirections(currentPosition, xDirection, yDirection, startingHeight, trail, deadEnds, tryLongTrail);

                        Debug.Log("movementChoice.Count: " + movementChoice.Count);
                        if (movementChoice.Count > 0 && CheckDFPositionListContent(movementChoice, arrivalDestination))
                        {
                            Debug.Log("Forcing movement to destination " + arrivalDestination.X + ", " + arrivalDestination.Y);
                            trail.Add(arrivalDestination);
                            trailWorkProgress++;
                        }
                        else if (movementChoice.Count > 0)
                        {
                            bool junctionPresent = false;
                            foreach (DFPosition checkJunction in movementChoice)
                            {
                                DFPosition checkJunctionConv = ConvertToRelative(checkJunction);
                                if (trailMap[checkJunctionConv.X, checkJunctionConv.Y].Item1 > 0 || trailMap[checkJunctionConv.X, checkJunctionConv.Y].Item2 > 0)
                                {
                                    Debug.Log("Junction present at " + checkJunction.X + ", " + checkJunction.Y);
                                    junctionPresent = true;
                                }
                            }
                            if (junctionPresent)
                            {
                                foreach (DFPosition removeNonJunction in movementChoice)
                                {
                                    DFPosition removeNonJunctionConv = ConvertToRelative(removeNonJunction);
                                    if (trailMap[removeNonJunctionConv.X, removeNonJunctionConv.Y].Item1 == 0 && !(CheckDFPositionListContent(stepToRemove, removeNonJunction)) &&
                                        trailMap[removeNonJunctionConv.X, removeNonJunctionConv.Y].Item2 == 0 && !(CheckDFPositionListContent(stepToRemove, removeNonJunction)))
                                    {
                                        Debug.Log("Removing movementChoice " + removeNonJunction.X + ", " + removeNonJunction.Y + " not being a junction");
                                        stepToRemove.Add(removeNonJunction);
                                    }
                                }
                            }

                            byte bestDiff = riseDeclineLimit;
                            if (!junctionPresent)
                            {
                                foreach (DFPosition potentialStep in movementChoice)
                                {
                                    if (Math.Abs(startingHeight - SmallHeightmap.GetHeightMapValue(potentialStep)) > bestDiff)
                                    {
                                        Debug.Log("Removing potentialStep " + potentialStep.X + ", " + potentialStep.Y);
                                        stepToRemove.Add(potentialStep);
                                    }
                                    else if (!CheckDFPositionListContent(deadEnds, potentialStep))
                                    {
                                        bestDiff = (byte)Math.Abs(startingHeight - SmallHeightmap.GetHeightMapValue(potentialStep));
                                        Debug.Log("New bestDiff: " + bestDiff);
                                    }
                                }
                            }

                            if (stepToRemove.Count > 0)
                            {
                                foreach (DFPosition stepToRem in stepToRemove)
                                {
                                    Debug.Log("Removing step " + stepToRem.X + ", " + stepToRem.Y);
                                    movementChoice.Remove(stepToRem);
                                }
                                movementChoice.TrimExcess();
                            }

                            foreach (DFPosition noGoodWay in deadEnds)
                            {
                                if (CheckDFPositionListContent(movementChoice, noGoodWay))
                                {
                                    Debug.Log("Removing movementChoice " + noGoodWay.X + ", " + noGoodWay.Y + " because it won't bring anywhere");
                                    movementChoice.Remove(noGoodWay);
                                }
                            }
                            movementChoice.TrimExcess();

                            if (movementChoice.Count > 0)
                            {
                                int randomSelection = UnityEngine.Random.Range(0, movementChoice.Count - 1);
                                trail.Add(movementChoice[randomSelection]);
                                trailWorkProgress++;
                            }
                            else
                            {
                                Debug.Log("Adding " + currentPosition.X + ", " + currentPosition.Y + " to deadEnds");
                                deadEnds.Add(currentPosition);
                                trailWorkProgress--;
                            }
                        }
                        else
                        {
                            Debug.Log("Adding " + currentPosition.X + ", " + currentPosition.Y + " to deadEnds");
                            deadEnds.Add(currentPosition);
                            trail.RemoveAt(trailWorkProgress);
                            trailWorkProgress--;
                        }

                        while (trailWorkProgress >= 0 && deadEnds.Count > 0 && CheckDFPositionListContent(deadEnds, trail[trailWorkProgress]))
                        {
                            Debug.Log("trailWorkProgress: " + trailWorkProgress);
                            trailWorkProgress--;
                        }

                        if (trailWorkProgress < 0 || (trailWorkProgress == 0 && CheckDFPositionListContent(deadEnds, trail[trailWorkProgress])))
                        {
                            noWay = true;
                            Debug.Log("Last trail attempt was unsuccesful. Think what you wanna do of it.");
                        }

                        if (!noWay && trailWorkProgress >= 0)
                        {
                            Debug.Log("trailWorkProgress: " + trailWorkProgress);
                            Debug.Log("trail[trailWorkProgress]: " + trail[trailWorkProgress].X + ", " + trail[trailWorkProgress].Y);
                            if (trail[trailWorkProgress].Equals(arrivalDestination))
                            {
                                Debug.Log("Arrived to destination!");
                                arrivedToDestination = true;
                            }
                        }
                    }
                    while (!arrivedToDestination && !noWay);

                    trailMap = GenerateTrailMap(trail, trailMap, locToRoute1.trailPreference, strtNameIndex, arrvNameIndex);
                }
            }

            string fileDataPath;
            byte[] trailsByteArray;

            for (int ipsilon = 0; ipsilon < 3; ipsilon++)
            {
                for (int ics = 0; ics < 3; ics++)
                {
                    (byte, byte, byte, byte)[,] subTile = GetSubTile(trailMap, ics, ipsilon);
                    fileDataPath = Path.Combine(MapEditor.arena2Path, "Maps", "Tiles", "trail_" + (xTile + ics - 1) + "_" + (yTile + ipsilon - 1) + ".png");
                    trailsByteArray = MapEditor.ConvertToGrayscale(subTile, (int)TrailTypes.None);
                    File.WriteAllBytes(fileDataPath, trailsByteArray);
                }
            }
            // string fileDataPath = Path.Combine(MapEditor.testPath, "Roads.png");
            // byte[] trailsByteArray = MapEditor.ConvertToGrayscale(trailMap, (int)TrailTypes.Road);
            // File.WriteAllBytes(fileDataPath, trailsByteArray);

            // fileDataPath = Path.Combine(MapEditor.testPath, "Tracks.png");
            // trailsByteArray = MapEditor.ConvertToGrayscale(trailMap, (int)TrailTypes.Track);
            // File.WriteAllBytes(fileDataPath, trailsByteArray);

            // Now we have generic pixel-per-pixel roads and tracks. It's time to refine trails drawing
            // by setting where, in a 5x5, pixel-based grid, those trails actually pass.
            // For that, we use the large heightmap as a reference.

            int tileX, tileY;
            long[,] tile = new long[MapsFile.TileDim * 5, MapsFile.TileDim * 5];
            List<(int, int)> trailTilesList = new List<(int, int)>();

            for (int x = 0; x < MapsFile.TileDim * 3; x++)
            {
                for (int y = 0; y < MapsFile.TileDim * 3; y++)
                {
                    if (trailMap[x, y].Item1 > 0 || trailMap[x, y].Item2 > 0)
                    {
                        tileX = x / MapsFile.TileDim + (xTile - 1);
                        tileY = y / MapsFile.TileDim + (yTile - 1);

                        if (!trailTilesList.Contains((tileX, tileY)))
                            trailTilesList.Add((tileX, tileY));
                    }
                }
            }

            foreach ((int, int) trailTile in trailTilesList)
            {
                tile = GenerateTile(trailTile, trailMap);

                tile = RefineTileBorder(tile);

                fileDataPath = Path.Combine(MapEditor.arena2Path, "Maps", "Tiles", "trailExp_" + trailTile.Item1 + "_" + trailTile.Item2 + ".png");
                trailsByteArray = ConvertToGrayscale(tile);
                File.WriteAllBytes(fileDataPath, trailsByteArray);
            }

            List<long> dicKeys = signData.roadsign.Keys.ToList();
            foreach (long roadsignKey in dicKeys)
            {
                int check = 0;
                long newRoadsignKey = roadsignKey;
                for (int css = 0; css < 8; css++)
                {
                    if (signData.roadsign[roadsignKey][css] != null)
                        check++;
                }
                if (check > 2)
                {
                    int x = (int)(roadsignKey % MapsFile.WorldWidth);
                    int y = (int)(roadsignKey / MapsFile.WorldWidth);
                    string tileDataPath = Path.Combine(MapEditor.arena2Path, "Maps", "Tiles", "trailExp_" + (x / MapsFile.TileDim) + "_" + (y / MapsFile.TileDim) + ".png");
                    Texture2D tex = new Texture2D(1, 1);
                    ImageConversion.LoadImage(tex, File.ReadAllBytes(tileDataPath));
                    int[,] tileToPrint = MapEditor.ConvertToMatrixExp(tex);
                    long crossroadIndex = 0;

                    for (int ics = ((x % MapsFile.TileDim) * 5); ics < (((x + 1) % MapsFile.TileDim) * 5); ics++)
                    {
                        for (int ipsilon = ((y % MapsFile.TileDim) * 5); ipsilon < (((y + 1) % MapsFile.TileDim) * 5); ipsilon++)
                        {
                            long pixel = (long)(tileToPrint[ics, ipsilon]);
                            if (pixel > FUNCINDEX)
                            {
                                crossroadIndex = pixel / FUNCINDEX;
                                Debug.Log("crossroadIndex: " + crossroadIndex + ", roadsignKey: " + roadsignKey);
                                newRoadsignKey = roadsignKey + crossroadIndex * (int)Math.Pow(10.0f, 8.0f);
                            }
                            else
                            {
                                crossroadIndex = (ipsilon * 5 + ics) + 1;
                            }

                            pixel -= (crossroadIndex * FUNCINDEX);
                            byte road = (byte)(pixel % DIRTINDEX);
                            byte dirt = (byte)((pixel % TRACKINDEX) / DIRTINDEX);
                            byte track = (byte)(pixel / TRACKINDEX);
                            byte crossroad = (byte)(road | dirt | track);
                            // roadsignKey += crossroadIndex * Math.Pow(10, 8);

                            DFPosition signPosition = GetSignPosition(crossroad, crossroadIndex);
                            DFPosition posCorrection = new DFPosition((ics % 5 * 26), (ipsilon % 5 * 26));
                            signPosition = new DFPosition(128 - (signPosition.X + posCorrection.X), 128 - (signPosition.Y + posCorrection.Y));
                            Debug.Log("roadsignData.Key: " + newRoadsignKey + ", signPosition: " + signPosition.X + ", " + signPosition.Y);
                            if (!signData.signPosition.ContainsKey(newRoadsignKey))
                                signData.signPosition.Add(newRoadsignKey, signPosition);
                            string[][] correctedRoadsignData = CorrectRoadsignDirection(signData.roadsign[roadsignKey], crossroad, crossroadIndex);
                            // signData.roadsign.Remove(roadsignKey);
                            int counter = 0;
                            if (signData.roadsign.ContainsKey(newRoadsignKey))
                            {
                                fileDataPath = Path.Combine(MapEditor.arena2Path, "Maps", "RoadsignData", "signdataError" + newRoadsignKey + counter + ".json");
                                var roadsignError = JsonConvert.SerializeObject(signData.roadsign, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                                File.WriteAllText(fileDataPath, roadsignError);
                                counter++;
                            }
                            else
                                signData.roadsign.Add(newRoadsignKey, correctedRoadsignData);

                            if (signData.signPosition.ContainsKey(newRoadsignKey))
                            {
                                (DFPosition, string[][]) signToPrint;
                                signToPrint.Item1 = signData.signPosition[newRoadsignKey];
                                signToPrint.Item2 = signData.roadsign[newRoadsignKey];
                                fileDataPath = Path.Combine(MapEditor.arena2Path, "Maps", "RoadsignData", "signdata_" + newRoadsignKey + ".json");
                                var signJson = JsonConvert.SerializeObject(signToPrint, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
                                File.WriteAllText(fileDataPath, signJson);
                            }
                            else Debug.Log("signData.signPosition does not contain key " + newRoadsignKey);
                        }
                    }
                }
                if (!signData.roadsign.Remove(roadsignKey))
                    Debug.Log("roadsign with key " + roadsignKey + " not found");
            }
        }

        public static TrailTypes GetRandomTrail(int governmentMultiplier, int roadChance, int dirtChance, int trackChance)
        {
            int randomChance = UnityEngine.Random.Range(1, 101);
            if (randomChance <= roadChance * governmentMultiplier)
                return TrailTypes.Road;
            if (randomChance <= dirtChance * governmentMultiplier)
                return TrailTypes.DirtRoad;
            if (randomChance <= trackChance * governmentMultiplier)
                return TrailTypes.Track;
            return TrailTypes.None;
        }

        public static int GovTypeToGovMultiplier(int govType)
        {
            switch (govType)
            {
                case (int)GovernmentType.Empire:
                    return govType + 1;
                case (int)GovernmentType.None:
                    return 1;
                default:
                    return 8 - govType;
            }
            return -1;
        }

        public static byte[] ConvertToGrayscale(long[,] map)
        {
            Texture2D grayscaleImage = new Texture2D(map.GetLength(0), map.GetLength(1));
            Color32[] grayscaleMap = new Color32[map.GetLength(0) * map.GetLength(1)];
            byte[] grayscaleBuffer = new byte[map.GetLength(0) * map.GetLength(1) * 4];
            
            for (int x = 0; x < map.GetLength(0); x++)
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    int offset = (((map.GetLength(1) - y - 1) * map.GetLength(0)) + x);
                    long value = map[x, y];
                    byte green = 0;
                    byte red = 0; 
                    byte blue = 0;
                    byte alpha = 0;

                    if (value == 0)
                    {
                        green = red = blue = 0;
                        alpha = byte.MaxValue;
                    }
                    else
                    {
                        alpha = (byte)(value / FUNCINDEX);
                        green = (byte)((value % FUNCINDEX) / TRACKINDEX);
                        blue = (byte)((value % TRACKINDEX) / DIRTINDEX);
                        red = (byte)(value % DIRTINDEX);
                    }
                    grayscaleMap[offset] = new Color32(red, green, blue, alpha);
                }
            }

            grayscaleImage.SetPixels32(grayscaleMap);
            grayscaleImage.Apply();
            grayscaleBuffer = ImageConversion.EncodeToPNG(grayscaleImage);

            return grayscaleBuffer;
        }

        public static long[,] GetTextureMatrix(int xPos, int yPos, string matrixType = "trail")
        {
            // DFPosition position = WorldMaps.LocalPlayerGPS.CurrentMapPixel;
            // int xPos = position.X / MapsFile.TileDim;
            // int yPos = position.Y / MapsFile.TileDim;
            int sizeModifier = 1;
            if (matrixType.Equals("trailExp"))
                sizeModifier = 5;

            long[,] matrix = new long[MapsFile.TileDim * 3 * sizeModifier, MapsFile.TileDim * 3 * sizeModifier];
            long[][,] textureArray = new long[9][,];


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

                    if (matrixType == "trailExp" || matrixType == "trail")
                        ConvertToMatrix(textureTiles, out textureArray[i]);
                    // else PoliticData.ConvertToMatrix(textureTiles, out textureArray[i]);
                }
                else
                {
                    Debug.Log("tile " + matrixType + "_" + posX + "_" + posY + ".png not present.");
                    textureArray[i] = new long[MapsFile.TileDim * sizeModifier, MapsFile.TileDim * sizeModifier];
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

        public static void ConvertToMatrix(Texture2D imageToTranslate, out long[,] matrix)
        {
            Color32[] grayscaleMap = imageToTranslate.GetPixels32();
            // byte[] buffer = imageToTranslate.GetRawTextureData();
            // Debug.Log("buffer.Length: " + buffer.Length);
            matrix = new long[imageToTranslate.width, imageToTranslate.height];
            for (int x = 0; x < imageToTranslate.width; x++)
            {
                for (int y = 0; y < imageToTranslate.height; y++)
                {
                    int offset = (((imageToTranslate.height - y - 1) * imageToTranslate.width) + x);
                    long intValue = grayscaleMap[offset].r + grayscaleMap[offset].b * DIRTINDEX + grayscaleMap[offset].g * TRACKINDEX + grayscaleMap[offset].a * FUNCINDEX;
                    matrix[x, y] = intValue;                    
                }
            }
        }

        protected static long[] ConvertToArray(long[,] matrix)
        {
            // int[,] matrix = new int[imageToTranslate.Width * imageToTranslate.Height];
            // ConvertToMatrix(imageToTranslate, out matrix);

            long[] resultingArray = new long[matrix.GetLength(0) * matrix.GetLength(1)];

            for (int x = 0; x < matrix.GetLength(0); x++)
            {
                for (int y = 0; y < matrix.GetLength(1); y++)
                {
                    resultingArray[(y * matrix.GetLength(0)) + x] = matrix[x, y];
                }
            }
            return resultingArray;
        }

        protected static string[][] CorrectRoadsignDirection(string[][] startingRoadsign, byte crossroad, long crossroadIndex)
        {
            string[][] resultingRoadsign = new string[8][];
            bool walled = false;
            bool town = false;
            if (crossroadIndex == walledInternalSign)
                walled = true;
            if (crossroadIndex == internalSign)
                town = true;

            for (int i = 0; i < 8; i++)
            {
                if (startingRoadsign[i] != null && HasTrail(crossroad, i))
                {
                    resultingRoadsign[i] = startingRoadsign[i];
                }
                if (startingRoadsign[i] != null && !HasTrail(crossroad, i))
                {
                    int iMinus = i - 1;
                    int iPlus = i + 1;
                    if (iMinus < 0) iMinus = 7;
                    if (iPlus > 7) iPlus = 0;
                    if (HasTrail(crossroad, iMinus) && !HasTrail(crossroad, iPlus) && resultingRoadsign[iMinus] == null)
                        resultingRoadsign[iMinus] = startingRoadsign[i];
                    else if (!HasTrail(crossroad, iMinus) && HasTrail(crossroad, iPlus) && resultingRoadsign[iMinus] == null)
                        resultingRoadsign[iPlus] = startingRoadsign[i];
                    else if (HasTrail(crossroad, iMinus) && HasTrail(crossroad, iPlus))
                    {
                        if (resultingRoadsign[iMinus] == null && resultingRoadsign[iPlus] != null)
                            resultingRoadsign[iMinus] = startingRoadsign[i];
                        else if (resultingRoadsign[iMinus] != null && resultingRoadsign[iPlus] == null)
                            resultingRoadsign[iPlus] = startingRoadsign[i];
                    }
                    else
                    {
                        iMinus--;
                        iPlus++;
                        if (iMinus < 0) iMinus += 8;
                        if (iPlus > 7) iPlus -= 8;
                        if (HasTrail(crossroad, iMinus) && !HasTrail(crossroad, iPlus) && resultingRoadsign[iMinus] == null)
                            resultingRoadsign[iMinus] = startingRoadsign[i];
                        else if (!HasTrail(crossroad, iMinus) && HasTrail(crossroad, iPlus) && resultingRoadsign[iMinus] == null)
                            resultingRoadsign[iPlus] = startingRoadsign[i];
                        else if (HasTrail(crossroad, iMinus) && HasTrail(crossroad, iPlus))
                        {
                            if (resultingRoadsign[iMinus] == null && resultingRoadsign[iPlus] != null)
                                resultingRoadsign[iMinus] = startingRoadsign[i];
                            else if (resultingRoadsign[iMinus] != null && resultingRoadsign[iPlus] == null)
                                resultingRoadsign[iPlus] = startingRoadsign[i];
                        }
                        else Debug.Log("Unable to determine best roadsign correction!");
                    }
                }
                if (startingRoadsign[i] == null && HasTrail(crossroad, i))
                {
                    int iMinus = i - 1;
                    int iPlus = i + 1;
                    if (iMinus < 0) iMinus = 7;
                    if (iPlus > 7) iPlus = 0;
                    if (startingRoadsign[iMinus] != null && !HasTrail(crossroad, iMinus) && startingRoadsign[iPlus] == null)
                        resultingRoadsign[i] = startingRoadsign[iMinus];
                    else if (startingRoadsign[iMinus] == null && startingRoadsign[iPlus] != null && !HasTrail(crossroad, iPlus))
                        resultingRoadsign[i] = startingRoadsign[iPlus];
                    else if (startingRoadsign[iMinus] != null && startingRoadsign[iPlus] != null &&
                             !HasTrail(crossroad, iMinus) && !HasTrail(crossroad, iPlus))
                    {
                        resultingRoadsign[i] = new string[startingRoadsign[iMinus].Length + startingRoadsign[iPlus].Length];
                        startingRoadsign[iMinus].CopyTo(resultingRoadsign[i], 0);
                        startingRoadsign[iPlus].CopyTo(resultingRoadsign[i], startingRoadsign[iMinus].Length);
                    }
                }
            }
            if (walled)
            {
                resultingRoadsign = CorrectRoadsignWalled(resultingRoadsign);
            }
            return resultingRoadsign;
        }

        protected static string[][] CorrectRoadsignWalled(string[][]startingRoadsign)
        {
            string[][] resultingRoadsign = new string[8][];

            return resultingRoadsign;
        }

        protected static bool HasTrail(byte crossroad, int direction)
        {
            bool test = (crossroad >> direction) % 2 != 0;
            Debug.Log("crossroad: " + crossroad + ", direction: " + direction + ", result: " + test);
            return (crossroad >> direction) % 2 != 0;
        }

        protected static DFPosition GetSignPosition(byte crossroad, long signType)
        {
            // If it's a walled-town roadsign...
            if (signType == walledInternalSign)
            {
                // Worldmaps.HasLocation(xRel + (trailTile.Item1 * MapsFile.TileDim), yRel + (trailTile.Item2 * MapsFile.TileDim), out MapSummary locSummary);
                // location = Worldmaps.GetLocation(locSummary.RegionIndex, locSummary.MapIndex);
            }
                return new DFPosition();

            bool[] freeRoom = new bool[8];
            DFPosition resultingPos = new DFPosition();

            
            for (int i = 0; i < 8; i++)
            {
                if (crossroad % 2 != 0)
                    freeRoom[i] = false;
                else freeRoom[i] = true;

                crossroad /= 2;
            }

            int randomPos = UnityEngine.Random.Range(0, 8);

            while (!freeRoom[randomPos])
                randomPos = UnityEngine.Random.Range(0, 8);

            switch (randomPos)
            {
                case 0:
                    return new DFPosition(10, 13);
                case 1:
                    return new DFPosition(11, 14);
                case 2:
                    return new DFPosition(13, 13);
                case 3:
                    return new DFPosition(14, 12);
                case 4:
                    return new DFPosition(13, 10);
                case 5:
                    return new DFPosition(12, 9);
                case 6:
                    return new DFPosition(10, 10);
                case 7:
                    return new DFPosition(9, 11);
            }

            return resultingPos;
        }

        protected DFPosition ConvertToRelative(DFPosition pos)
        {
            DFPosition resultingPos = new DFPosition(pos.X - (xTile - 1) * MapsFile.TileDim, pos.Y - (yTile - 1) * MapsFile.TileDim);
            return resultingPos;
        }

        protected (byte, byte, byte, byte)[,] GetSubTile((byte, byte, byte, byte)[,] trailMap, int tileX, int tileY)
        {
            (byte, byte, byte, byte)[,] subTile = new (byte, byte, byte, byte)[MapsFile.TileDim, MapsFile.TileDim];
            int X, Y;
            
            for (int y = 0; y < MapsFile.TileDim; y++)
            {
                for (int x = 0; x < MapsFile.TileDim; x++)
                {
                    X = (MapsFile.TileDim * tileX) + x;
                    Y = (MapsFile.TileDim * tileY) + y;

                    subTile[x, y] = trailMap[X, Y];
                }
            }

            return subTile;
        }

        protected (byte, byte, byte, byte)[,] LoadExistingTrails(long[] pathData)
        {
            (byte, byte, byte, byte)[,] existingTrails = new (byte, byte, byte, byte)[MapsFile.TileDim * 3, MapsFile.TileDim * 3];
            int xCorrection, yCorrection;

            for (int i = 0; i < pathData.Length; i++)
            {
                if (pathData[i] != 0)
                {
                    xCorrection = (i % (MapsFile.TileDim * 3));
                    yCorrection = (i / (MapsFile.TileDim * 3));

                    existingTrails[xCorrection, yCorrection] = ((byte)(pathData[i] % DIRTINDEX), (byte)((pathData[i] % TRACKINDEX) / DIRTINDEX), (byte)((pathData[i] % FUNCINDEX) / TRACKINDEX), (byte)(pathData[i] / FUNCINDEX));
                }
            }
            return existingTrails;
        }

        /// <summary>
        /// Generate the 3x3 tiles area of locations that will be taken into consideration while
        /// drawing the trails of this area. Keep in mind that only the trails of the central tile
        /// will be extensively covered.
        /// </summary>
        protected List<DFPosition> SetLocSurroundings(out List<RoutedLocation>[] locationToRoute)
        {
            locationToRoute = new List<RoutedLocation>[Enum.GetNames(typeof(DFRegion.LocationTypes)).Length];
            for (int t = 0; t < Enum.GetNames(typeof(DFRegion.LocationTypes)).Length; t++)
                locationToRoute[t] = new List<RoutedLocation>();
            List<DFPosition> locSurroundings = new List<DFPosition>();
            int xMin, yMin, xMax, yMax, xMinMiddle, yMinMiddle, xMaxMiddle, yMaxMiddle;
            MapSummary locationSummary;

            if (xTile > 0) xMin = ((xTile - 1) * MapsFile.TileDim);
            else xMin = xTile * MapsFile.TileDim;
            if (yTile > 0) yMin = ((yTile - 1) * MapsFile.TileDim);
            else yMin = yTile * MapsFile.TileDim;
            if (xTile < MapsFile.TileX - 1) xMax = ((xTile + 2) * MapsFile.TileDim);
            else xMax = (xTile + 1) * MapsFile.TileDim;
            if (yTile < MapsFile.TileY - 1) yMax = ((yTile + 2) * MapsFile.TileDim);
            else yMax = (yTile + 1) * MapsFile.TileDim;

            xMinMiddle = xTile * MapsFile.TileDim;
            yMinMiddle = yTile * MapsFile.TileDim;
            xMaxMiddle = (xTile + 1) * MapsFile.TileDim;
            yMaxMiddle = (yTile + 1) * MapsFile.TileDim;

            for (int x = xMin; x < xMax; x++)
            {
                for (int y = yMin; y < yMax; y++)
                {
                    RoutedLocation location = new RoutedLocation();
                    if (Worldmaps.HasLocation(x, y, out locationSummary))
                    {
                        if (locationSummary.LocationType != DFRegion.LocationTypes.Coven &&
                            locationSummary.LocationType != DFRegion.LocationTypes.HiddenLocation &&
                            locationSummary.LocationType != DFRegion.LocationTypes.HomeYourShips &&
                            locationSummary.LocationType != DFRegion.LocationTypes.None)
                        {
                            int locationType = (int)locationSummary.LocationType;

                            DFPosition locPosition = MapsFile.GetPixelFromPixelID(locationSummary.ID);
                            DFLocation locationData = Worldmaps.GetLocation((x / MapsFile.TileDim) + ((y / MapsFile.TileDim) * 120), locationSummary.MapIndex);
                            locSurroundings.Add(locPosition);

                            if (locPosition.X >= xMinMiddle && locPosition.X < xMaxMiddle && locPosition.Y >= yMinMiddle && locPosition.Y < yMaxMiddle)
                            {
                                location.name = locationData.Name;
                                location.position = locPosition;
                                location.locDistance = new List<(ulong, float)>();
                                location.trailPreference = TrailTypes.None;
                                location.completionLevel = 0;
                                location.locType = (DFRegion.LocationTypes)locationType;

                                locationToRoute[locationType].Add(location);
                            }
                            if (!signData.locNames.Contains(location.name))
                                signData.locNames.Add(location.name);
                        }
                    }
                }
            }

            return locSurroundings;
        }

        /// <summary>
        /// Generate a tile of roads and tracks.
        /// </summary>
        /// <param name="trailTile">Trail tile that is used for every sub-tile creation.</param> 
        /// <param name="trailMap">The generic trail map used to create the sub-tiles.</param>
        protected long[,] GenerateTile((int, int) trailTile, (byte, byte, byte, byte)[,] trailMap)
        {
            // Debug.Log("Generating trailTile " + trailTile.Item1 + ", " + trailTile.Item2);
            long[,] tile = new long[MapsFile.TileDim * 5, MapsFile.TileDim * 5];
            Texture2D tileImage = new Texture2D(1, 1);
            if (File.Exists(Path.Combine(MapEditor.arena2Path, "Maps", "Tiles", "trailExp_" + trailTile.Item1 + "_" + trailTile.Item2 + ".png")))
            {
                ImageConversion.LoadImage(tileImage, File.ReadAllBytes(Path.Combine(MapEditor.arena2Path, "Maps", "Tiles", "trailExp_" + trailTile.Item1 + "_" + trailTile.Item2 + ".png")));
                ConvertToMatrix(tileImage, out tile);
            }

            // MapSummary location;
            // PixelData pixelData;

            ImageConversion.LoadImage(tileImage, File.ReadAllBytes(Path.Combine(MapEditor.arena2Path, "Maps", "Tiles", "woodsLarge_" + trailTile.Item1 + "_" + trailTile.Item2 + ".png")));
            byte[,] heightmapTile = MapEditor.ConvertToMatrix(tileImage);

            int crssNumb = 0;
            for (int xRel = 0; xRel < MapsFile.TileDim; xRel++)
            {
                for (int yRel = 0; yRel < MapsFile.TileDim; yRel++)
                {
                    Debug.Log("Working on xRel: " + xRel + ", yRel: " + yRel);
                    bool hasLocation = Worldmaps.HasLocation(xRel + (trailTile.Item1 * MapsFile.TileDim), yRel + (trailTile.Item2 * MapsFile.TileDim));
                    DFLocation location = new DFLocation();
                    if (hasLocation)
                    {
                        Worldmaps.HasLocation(xRel + (trailTile.Item1 * MapsFile.TileDim), yRel + (trailTile.Item2 * MapsFile.TileDim), out MapSummary locSummary);
                        location = Worldmaps.GetLocation(locSummary.RegionIndex, locSummary.MapIndex);
                    }
                    
                    bool loadedBuffer = false;
                    byte[,] buffer = GetLargeHeightmapPixel(heightmapTile, xRel, yRel);
                    long[,] tileBuffer = new long[5, 5];

                    if (trailMap[xRel + ((trailTile.Item1 - (xTile - 1)) * MapsFile.TileDim), yRel + ((trailTile.Item2 - (yTile - 1)) * MapsFile.TileDim)].Item3 > 0)
                    {
                        tileBuffer = GetTileBuffer(trailMap[xRel + ((trailTile.Item1 - (xTile - 1)) * MapsFile.TileDim), yRel + ((trailTile.Item2 - (yTile - 1)) * MapsFile.TileDim)].Item3, TrailTypes.Track, buffer, tileBuffer, hasLocation, location, crssNumb);
                        loadedBuffer = true;
                    }

                    if (trailMap[xRel + ((trailTile.Item1 - (xTile - 1)) * MapsFile.TileDim), yRel + ((trailTile.Item2 - (yTile - 1)) * MapsFile.TileDim)].Item2 > 0)
                    {
                        tileBuffer = GetTileBuffer(trailMap[xRel + ((trailTile.Item1 - (xTile - 1)) * MapsFile.TileDim), yRel + ((trailTile.Item2 - (yTile - 1)) * MapsFile.TileDim)].Item2, TrailTypes.DirtRoad, buffer, tileBuffer, hasLocation, location, crssNumb);
                        loadedBuffer = true;
                    }

                    if (trailMap[xRel + ((trailTile.Item1 - (xTile - 1)) * MapsFile.TileDim), yRel + ((trailTile.Item2 - (yTile - 1)) * MapsFile.TileDim)].Item1 > 0)
                    {
                        tileBuffer = GetTileBuffer(trailMap[xRel + ((trailTile.Item1 - (xTile - 1)) * MapsFile.TileDim), yRel + ((trailTile.Item2 - (yTile - 1)) * MapsFile.TileDim)].Item1, TrailTypes.Road, buffer, tileBuffer, hasLocation, location, crssNumb);
                        loadedBuffer = true;
                    }

                    for (int bufferCount = 0; bufferCount < 25; bufferCount++)
                    {
                        tile[bufferCount % 5 + xRel * 5, bufferCount / 5 + yRel * 5] = tileBuffer[bufferCount % 5, bufferCount / 5];
                    }
                }
            }
            return tile;
        }

        protected TrailTypes GetTrailType(byte direction, List<byte> road, List<byte> dirt, List<byte> track)
        {
            if (road.Contains(direction))
                return TrailTypes.Road;
            if (dirt.Contains(direction))
                return TrailTypes.DirtRoad;
            if (track.Contains(direction))
                return TrailTypes.Track;
            return TrailTypes.None;
        }

        protected long[,] RefineTileBorder(long[,] tile)
        {
            int tileSize = MapsFile.TileDim * 5;
            bool isCrossroad;
            for (int x = 0; x < tileSize; x++)
            {
                for (int y = 0; y < tileSize; y++)
                {
                    int crssNumb = 0;
                    if (tile[x, y] > 0)
                    {
                        int xBorder = x % 5;
                        int yBorder = y % 5;
                        int x1, x2, x3, y1, y2, y3;
                        List<byte> road = GetByte(tile[x, y], TrailTypes.Road);
                        List<byte> dirt = GetByte(tile[x, y], TrailTypes.DirtRoad);
                        List<byte> track = GetByte(tile[x, y], TrailTypes.Track);
                        List<byte> otherRoad = new List<byte>();
                        List<byte> otherDirt = new List<byte>();
                        List<byte> otherTrack = new List<byte>();
                        List<byte> otherTrail = new List<byte>();
                        TrailTypes trailType = TrailTypes.None;
                        List<byte> corresponding1, corresponding2, corresponding3;

                        if ((xBorder > 0 && xBorder < 4) && yBorder == 0 && y - 1 >= 0)
                        {
                            if (road.Contains(N) || dirt.Contains(N) || track.Contains(N))
                            {
                                trailType = GetTrailType(N, road, dirt, track);
                                x1 = x - x % 5 + 1;
                                x2 = x - x % 5 + 2;
                                x3 = x - x % 5 + 3;
                                y1 = y - 1;
                                corresponding1 = GetByte(tile[x1, y1], trailType);
                                corresponding2 = GetByte(tile[x2, y1], trailType);
                                corresponding3 = GetByte(tile[x3, y1], trailType);

                                if (xBorder == 1)
                                {
                                    if (corresponding2.Contains(S))
                                    {
                                        otherTrail = GetByte(tile[x + 1, y], trailType);
                                        if (!corresponding1.Contains(S) && !otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, trailType, N);
                                            tile[x2, y1] = AddByte(tile[x2, y1], SW, trailType, S);
                                        }
                                        else if (!corresponding1.Contains(S) && otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, trailType, N);
                                            tile[x2, y1] = AddByte(tile[x2, y1], SW, trailType);
                                        }
                                        else if (corresponding1.Contains(S) && !otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, trailType);
                                            tile[x2, y1] = AddByte(tile[x2, y1], SW, trailType, S);
                                        }
                                    }
                                    else if (corresponding3.Contains(S))
                                    {
                                        otherTrail = GetByte(tile[x + 2, y], trailType);
                                        if (!corresponding1.Contains(S) && !otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, trailType, N);
                                            tile[x3, y1] = AddByte(tile[x3, y1], W, trailType, S);
                                            tile[x2, y1] = AddByte(tile[x2, y1], SW, E, trailType);
                                        }
                                        else if (!corresponding1.Contains(S) && otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, trailType, N);
                                            tile[x3, y1] = AddByte(tile[x3, y1], W, trailType);
                                            tile[x2, y1] = AddByte(tile[x2, y1], SW, E, trailType);
                                        }
                                        else if (corresponding1.Contains(S) && !otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, trailType);
                                            tile[x3, y1] = AddByte(tile[x3, y1], W, trailType, S);
                                            tile[x2, y1] = AddByte(tile[x2, y1], SW, E, trailType);
                                        }
                                    }
                                }
                                if (xBorder == 2)
                                {
                                    if (corresponding1.Contains(S))
                                    {
                                        otherTrail = GetByte(tile[x - 1, y], trailType);
                                        if (!corresponding2.Contains(S) && !otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, trailType, N);
                                            tile[x1, y1] = AddByte(tile[x1, y1], SE, trailType, S);
                                        }
                                        else if (!corresponding2.Contains(S) && otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, trailType, N);
                                            tile[x1, y1] = AddByte(tile[x1, y1], SE, trailType);
                                        }
                                        else if (corresponding2.Contains(S) && !otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, trailType);
                                            tile[x1, y1] = AddByte(tile[x1, y1], SE, trailType, S);
                                        }
                                    }
                                    else if (corresponding3.Contains(S))
                                    {
                                        otherTrail = GetByte(tile[x + 1, y], trailType);
                                        if (!corresponding2.Contains(S) && !otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, trailType, N);
                                            tile[x3, y1] = AddByte(tile[x3, y1], SW, trailType, S);
                                        }
                                        else if (!corresponding2.Contains(S) && otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, trailType, N);
                                            tile[x3, y1] = AddByte(tile[x3, y1], SW, trailType);
                                        }
                                        else if (corresponding2.Contains(S) && !otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, trailType);
                                            tile[x3, y1] = AddByte(tile[x3, y1], SW, trailType, S);
                                        }
                                    }
                                }
                                if (xBorder == 3)
                                {
                                    if (corresponding2.Contains(S))
                                    {
                                        otherTrail = GetByte(tile[x - 1, y], trailType);
                                        if (!corresponding3.Contains(S) && !otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, trailType, N);
                                            tile[x2, y1] = AddByte(tile[x2, y1], SE, trailType, S);
                                        }
                                        else if (!corresponding3.Contains(S) && otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, trailType, N);
                                            tile[x2, y1] = AddByte(tile[x2, y1], SE, trailType);
                                        }
                                        else if (corresponding3.Contains(S) && !otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, trailType);
                                            tile[x2, y1] = AddByte(tile[x2, y1], SE, trailType, S);
                                        }
                                    }
                                    else if (corresponding1.Contains(S))
                                    {
                                        otherTrail = GetByte(tile[x - 2, y], trailType);
                                        if (!corresponding3.Contains(S) && !otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, trailType, N);
                                            tile[x1, y1] = AddByte(tile[x1, y1], E, trailType, S);
                                            tile[x2, y1] = AddByte(tile[x2, y1], W, SE, trailType);
                                        }
                                        else if (!corresponding3.Contains(S) && otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, trailType, N);
                                            tile[x1, y1] = AddByte(tile[x1, y1], E, trailType);
                                            tile[x2, y1] = AddByte(tile[x2, y1], W, SE, trailType);
                                        }
                                        else if (corresponding3.Contains(S) && !otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, trailType);
                                            tile[x1, y1] = AddByte(tile[x1, y1], E, trailType, S);
                                            tile[x2, y1] = AddByte(tile[x2, y1], W, SE, trailType);
                                        }
                                    }
                                }
                                // if (IsCrossRoad(tile[x1, y1])) tile[x1, y1] += (25 * 256 * 256);
                                // if (IsCrossRoad(tile[x2, y1])) tile[x2, y1] += (25 * 256 * 256);
                                // if (IsCrossRoad(tile[x3, y1])) tile[x3, y1] += (25 * 256 * 256);
                            }
                        }
                        else if ((xBorder > 0 && xBorder < 4) && yBorder == 4 && y + 1 < tileSize)
                        {
                            if (road.Contains(S) || dirt.Contains(S) || track.Contains(S))
                            {
                                trailType = GetTrailType(S, road, dirt, track);
                                x1 = x - x % 5 + 1;
                                x2 = x - x % 5 + 2;
                                x3 = x - x % 5 + 3;
                                y1 = y + 1;
                                corresponding1 = GetByte(tile[x1, y1], trailType);
                                corresponding2 = GetByte(tile[x2, y1], trailType);
                                corresponding3 = GetByte(tile[x3, y1], trailType);

                                if (xBorder == 1)
                                {
                                    if (corresponding2.Contains(N))
                                    {
                                        // Checking tile[x + 1, y];
                                        otherTrail = GetByte(tile[x + 1, y], trailType);
                                        if (!corresponding1.Contains(N) && !otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, trailType, S);
                                            tile[x2, y1] = AddByte(tile[x2, y1], NW, trailType, N);
                                        }
                                        else if (!corresponding1.Contains(N) && otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, trailType, S);
                                            tile[x2, y1] = AddByte(tile[x2, y1], NW, trailType);
                                        }
                                        else if (corresponding1.Contains(N) && !otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, trailType);
                                            tile[x2, y1] = AddByte(tile[x2, y1], NW, trailType, N);
                                        }
                                    }
                                    else if (corresponding3.Contains(N))
                                    {
                                        // Checking tile[x + 2, y];
                                        otherTrail = GetByte(tile[x + 2, y], trailType);
                                        if (!corresponding1.Contains(N) && !otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, trailType, S);
                                            tile[x3, y1] = AddByte(tile[x3, y1], W, trailType, N);
                                            tile[x2, y1] = AddByte(tile[x2, y1], NW, E, trailType);
                                        }
                                        else if (!corresponding1.Contains(N) && otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, trailType, S);
                                            tile[x3, y1] = AddByte(tile[x3, y1], W, trailType);
                                            tile[x2, y1] = AddByte(tile[x2, y1], NW, E, trailType);
                                        }
                                        else if (corresponding1.Contains(N) && !otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, trailType);
                                            tile[x3, y1] = AddByte(tile[x3, y1], W, trailType, N);
                                            tile[x2, y1] = AddByte(tile[x2, y1], NW, E, trailType);
                                        }
                                    }
                                }
                                if (xBorder == 2)
                                {
                                    if (corresponding1.Contains(N))
                                    {
                                        otherTrail = GetByte(tile[x - 1, y], trailType);
                                        if (!corresponding2.Contains(N) && !otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, trailType, S);
                                            tile[x1, y1] = AddByte(tile[x1, y1], NE, trailType, N);
                                        }
                                        else if (!corresponding2.Contains(N) && otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, trailType, S);
                                            tile[x1, y1] = AddByte(tile[x1, y1], NE, trailType);
                                        }
                                        else if (corresponding2.Contains(N) && !otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, trailType);
                                            tile[x1, y1] = AddByte(tile[x1, y1], NE, trailType, N);
                                        }
                                    }
                                    else if (corresponding3.Contains(N))
                                    {
                                        otherTrail = GetByte(tile[x + 1, y], trailType);
                                        if (!corresponding2.Contains(N) && !otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, trailType, S);
                                            tile[x1, y1] = AddByte(tile[x1, y1], NW, trailType, N);
                                        }
                                        else if (!corresponding2.Contains(N) && otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, trailType, S);
                                            tile[x1, y1] = AddByte(tile[x1, y1], NW, trailType);
                                        }
                                        else if (corresponding2.Contains(N) && !otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, trailType);
                                            tile[x1, y1] = AddByte(tile[x1, y1], NW, trailType, N);
                                        }
                                    }
                                }
                                if (xBorder == 3)
                                {
                                    if (corresponding2.Contains(S))
                                    {
                                        otherTrail = GetByte(tile[x - 1, y], trailType);
                                        if (!corresponding3.Contains(N) && !otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, trailType, N);
                                            tile[x2, y1] = AddByte(tile[x2, y1], SE, trailType, S);
                                        }
                                        else if (!corresponding3.Contains(N) && otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, trailType, N);
                                            tile[x2, y1] = AddByte(tile[x2, y1], SE, trailType);
                                        }
                                        else if (corresponding3.Contains(N) && !otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, trailType);
                                            tile[x2, y1] = AddByte(tile[x2, y1], SE, trailType, S);
                                        }
                                    }
                                    else if (corresponding1.Contains(N))
                                    {
                                        otherTrail = GetByte(tile[x - 2, y], trailType);
                                        if (!corresponding3.Contains(N) && !otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, trailType, S);
                                            tile[x1, y1] = AddByte(tile[x1, y1], E, trailType, N);
                                            tile[x2, y1] = AddByte(tile[x2, y1], W, NE, trailType);
                                        }
                                        else if (!corresponding3.Contains(N) && otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, trailType, S);
                                            tile[x1, y1] = AddByte(tile[x1, y1], E, trailType);
                                            tile[x2, y1] = AddByte(tile[x2, y1], W, NE, trailType);
                                        }
                                        else if (corresponding3.Contains(N) && !otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, trailType);
                                            tile[x1, y1] = AddByte(tile[x1, y1], E, trailType, N);
                                            tile[x2, y1] = AddByte(tile[x2, y1], W, NE, trailType);
                                        }
                                    }
                                }
                                // if (IsCrossRoad(tile[x1, y1])) tile[x1, y1] += (25 * 256 * 256);
                                // if (IsCrossRoad(tile[x2, y1])) tile[x2, y1] += (25 * 256 * 256);
                                // if (IsCrossRoad(tile[x3, y1])) tile[x3, y1] += (25 * 256 * 256);
                            }
                        }
                        else if ((yBorder > 0 && yBorder < 4) && xBorder == 0 && x - 1 >= 0)
                        {
                            if (road.Contains(W) || dirt.Contains(W) || track.Contains(W))
                            {
                                trailType = GetTrailType(W, road, dirt, track);
                                x1 = x - 1;
                                y1 = y - y % 5 + 1;
                                y2 = y - y % 5 + 2;
                                y3 = y - y % 5 + 3;
                                corresponding1 = GetByte(tile[x1, y1], trailType);
                                corresponding2 = GetByte(tile[x1, y2], trailType);
                                corresponding3 = GetByte(tile[x1, y3], trailType);

                                if (yBorder == 1)
                                {
                                    if (corresponding2.Contains(E))
                                    {
                                        otherTrail = GetByte(tile[x, y + 1], trailType);
                                        if (!corresponding1.Contains(E) && !otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, trailType, W);
                                            tile[x1, y2] = AddByte(tile[x1, y2], NE, trailType, E);
                                        }
                                        else if (!corresponding1.Contains(E) && otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, trailType, W);
                                            tile[x1, y2] = AddByte(tile[x1, y2], NE, trailType);
                                        }
                                        else if (corresponding1.Contains(E) && !otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, trailType);
                                            tile[x1, y2] = AddByte(tile[x1, y2], NE, trailType, E);
                                        }
                                    }
                                    else if (corresponding3.Contains(E))
                                    {
                                        otherTrail = GetByte(tile[x, y + 2], trailType);
                                        if (!corresponding1.Contains(E) && !otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, trailType, W);
                                            tile[x1, y3] = AddByte(tile[x1, y3], N, trailType, E);
                                            tile[x1, y2] = AddByte(tile[x1, y2], NE, S, trailType);
                                        }
                                        else if (!corresponding1.Contains(E) && otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, trailType, W);
                                            tile[x1, y3] = AddByte(tile[x1, y3], N, trailType);
                                            tile[x1, y2] = AddByte(tile[x1, y2], NE, S, trailType);
                                        }
                                        else if (corresponding1.Contains(E) && !otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, trailType);
                                            tile[x1, y3] = AddByte(tile[x1, y3], N, trailType, E);
                                            tile[x1, y2] = AddByte(tile[x1, y2], NE, S, trailType);
                                        }
                                    }
                                }
                                if (yBorder == 2)
                                {
                                    if (corresponding1.Contains(E))
                                    {
                                        otherTrail = GetByte(tile[x, y - 1], trailType);
                                        if (!corresponding2.Contains(E) && !otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, trailType, W);
                                            tile[x1, y1] = AddByte(tile[x1, y1], SE, trailType, E);
                                        }
                                        else if (!corresponding2.Contains(E) && otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, trailType, W);
                                            tile[x1, y1] = AddByte(tile[x1, y1], SE, trailType);
                                        }
                                        else if (corresponding2.Contains(E) && !otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, trailType);
                                            tile[x1, y1] = AddByte(tile[x1, y1], SE, trailType, E);
                                        }
                                    }
                                    else if (corresponding3.Contains(E))
                                    {
                                        otherTrail = GetByte(tile[x, y + 1], trailType);
                                        if (!corresponding2.Contains(E) && !otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, trailType, W);
                                            tile[x1, y3] = AddByte(tile[x1, y3], NE, trailType, E);
                                        }
                                        else if (!corresponding2.Contains(E) && otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, trailType, W);
                                            tile[x1, y3] = AddByte(tile[x1, y3], NE, trailType);
                                        }
                                        else if (corresponding2.Contains(E) && !otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, trailType);
                                            tile[x1, y3] = AddByte(tile[x1, y3], NE, trailType, E);
                                        }
                                    }
                                }
                                if (yBorder == 3)
                                {
                                    if (corresponding2.Contains(E))
                                    {
                                        otherTrail = GetByte(tile[x, y - 1], trailType);
                                        if (!corresponding3.Contains(E) && !otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, trailType, W);
                                            tile[x1, y2] = AddByte(tile[x1, y2], SE, trailType, E);
                                        }
                                        else if (!corresponding3.Contains(E) && otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, trailType, W);
                                            tile[x1, y2] = AddByte(tile[x1, y2], SE, trailType);
                                        }
                                        else if (corresponding3.Contains(E) && !otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, trailType);
                                            tile[x1, y2] = AddByte(tile[x1, y2], SE, trailType, E);
                                        }
                                    }
                                    else if (corresponding1.Contains(E))
                                    {
                                        otherTrail = GetByte(tile[x, y - 2], trailType);
                                        if (!corresponding3.Contains(E) && !otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, trailType, W);
                                            tile[x1, y3] = AddByte(tile[x1, y3], S, trailType, E);
                                            tile[x1, y2] = AddByte(tile[x1, y2], SE, N, trailType);
                                        }
                                        else if (!corresponding3.Contains(E) && otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, trailType, W);
                                            tile[x1, y3] = AddByte(tile[x1, y3], S, trailType);
                                            tile[x1, y2] = AddByte(tile[x1, y2], SE, N, trailType);
                                        }
                                        else if (corresponding3.Contains(E) && !otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, trailType);
                                            tile[x1, y3] = AddByte(tile[x1, y3], S, trailType, E);
                                            tile[x1, y2] = AddByte(tile[x1, y2], SE, N, trailType);
                                        }
                                    }
                                }
                                // if (IsCrossRoad(tile[x1, y1])) tile[x1, y1] += (25 * 256 * 256);
                                // if (IsCrossRoad(tile[x1, y2])) tile[x1, y2] += (25 * 256 * 256);
                                // if (IsCrossRoad(tile[x1, y3])) tile[x1, y3] += (25 * 256 * 256);
                            }
                        }
                        else if ((yBorder > 0 && yBorder < 4) && xBorder == 4 && x + 1 < tileSize)
                        {
                            if (road.Contains(E) || dirt.Contains(E) || track.Contains(E))
                            {
                                trailType = GetTrailType(E, road, dirt, track);
                                x1 = x + 1;
                                y1 = y - y % 5 + 1;
                                y2 = y - y % 5 + 2;
                                y3 = y - y % 5 + 3;
                                corresponding1 = GetByte(tile[x1, y1], trailType);
                                corresponding2 = GetByte(tile[x1, y2], trailType);
                                corresponding3 = GetByte(tile[x1, y3], trailType);

                                if (yBorder == 1)
                                {
                                    if (corresponding2.Contains(W))
                                    {
                                        otherTrail = GetByte(tile[x , y + 1], trailType);
                                        if (!corresponding1.Contains(W) && !otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, trailType, E);
                                            tile[x1, y2] = AddByte(tile[x1, y2], NW, trailType, W);
                                        }
                                        else if (!corresponding1.Contains(W) && otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, trailType, E);
                                            tile[x1, y2] = AddByte(tile[x1, y2], NW, trailType);
                                        }
                                        else if (corresponding1.Contains(W) && !otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, trailType);
                                            tile[x1, y2] = AddByte(tile[x1, y2], NW, trailType, W);
                                        }
                                    }
                                    else if (corresponding3.Contains(W))
                                    {
                                        otherTrail = GetByte(tile[x, y + 2], trailType);
                                        if (!corresponding1.Contains(W) && !otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, trailType, E);
                                            tile[x1, y3] = AddByte(tile[x1, y3], N, trailType, W);
                                            tile[x1, y2] = AddByte(tile[x1, y2], S, NW, trailType);
                                        }
                                        else if (!corresponding1.Contains(W) && otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, trailType, E);
                                            tile[x1, y3] = AddByte(tile[x1, y3], N, trailType);
                                            tile[x1, y2] = AddByte(tile[x1, y2], S, NW, trailType);
                                        }
                                        else if (corresponding1.Contains(W) && !otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, trailType);
                                            tile[x1, y3] = AddByte(tile[x1, y3], N, trailType, W);
                                            tile[x1, y2] = AddByte(tile[x1, y2], S, NW, trailType);
                                        }
                                    }
                                }
                                if (yBorder == 2)
                                {
                                    if (corresponding1.Contains(W))
                                    {
                                        otherTrail = GetByte(tile[x, y - 1], trailType);
                                        if (!corresponding2.Contains(W) && !otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, trailType, E);
                                            tile[x1, y1] = AddByte(tile[x1, y1], SW, trailType, W);
                                        }
                                        else if (!corresponding2.Contains(W) && otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, trailType, E);
                                            tile[x1, y1] = AddByte(tile[x1, y1], SW, trailType);
                                        }
                                        else if (corresponding2.Contains(W) && !otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, trailType);
                                            tile[x1, y1] = AddByte(tile[x1, y1], SW, trailType, W);
                                        }
                                    }
                                    else if (corresponding3.Contains(W))
                                    {
                                        otherTrail = GetByte(tile[x, y + 1], trailType);
                                        if (!corresponding2.Contains(W) && !otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, trailType, E);
                                            tile[x1, y3] = AddByte(tile[x1, y3], NW, trailType, W);
                                        }
                                        else if (!corresponding2.Contains(W) && otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, trailType, E);
                                            tile[x1, y3] = AddByte(tile[x1, y3], NW, trailType);
                                        }
                                        else if (corresponding2.Contains(W) && !otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, trailType);
                                            tile[x1, y3] = AddByte(tile[x1, y3], NW, trailType, W);
                                        }
                                    }
                                }
                                if (yBorder == 3)
                                {
                                    if (corresponding2.Contains(W))
                                    {
                                        otherTrail = GetByte(tile[x, y - 1], trailType);
                                        if (!corresponding3.Contains(W) && !otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, trailType, E);
                                            tile[x1, y2] = AddByte(tile[x1, y2], SW, trailType, W);
                                        }
                                        else if (!corresponding3.Contains(W) && otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, trailType, E);
                                            tile[x1, y2] = AddByte(tile[x1, y2], SW, trailType);
                                        }
                                        else if (corresponding3.Contains(W) && !otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, trailType);
                                            tile[x1, y2] = AddByte(tile[x1, y2], SW, trailType, W);
                                        }
                                    }
                                    else if (corresponding1.Contains(W))
                                    {
                                        otherTrail = GetByte(tile[x, y - 2], trailType);
                                        if (!corresponding3.Contains(W) && !otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, trailType, E);
                                            tile[x1, y1] = AddByte(tile[x1, y1], S, trailType, W);
                                            tile[x1, y2] = AddByte(tile[x1, y2], SW, N, trailType);
                                        }
                                        else if (!corresponding3.Contains(W) && otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, trailType, E);
                                            tile[x1, y1] = AddByte(tile[x1, y1], S, trailType);
                                            tile[x1, y2] = AddByte(tile[x1, y2], SW, N, trailType);
                                        }
                                        else if (corresponding3.Contains(W) && !otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, trailType);
                                            tile[x1, y1] = AddByte(tile[x1, y1], S, trailType, W);
                                            tile[x1, y2] = AddByte(tile[x1, y2], SW, N, trailType);
                                        }
                                    }
                                }
                                // if (IsCrossRoad(tile[x1, y1])) tile[x1, y1] += (25 * 256 * 256);
                                // if (IsCrossRoad(tile[x1, y2])) tile[x1, y2] += (25 * 256 * 256);
                                // if (IsCrossRoad(tile[x1, y3])) tile[x1, y3] += (25 * 256 * 256);
                            }
                        }
                        if (IsCrossRoad(tile[x, y]))
                        {
                            int crssIndex = ((y % 5) * 5 + (x % 5)) + 1;
                            Debug.Log("tile[" + x + ", " + y + "] is crossroad with a value of " + tile[x, y] + "; adding " + crssIndex + " * FUNCINDEX to it");
                            tile[x, y] = (tile[x, y] % (FUNCINDEX)) + (crssIndex * FUNCINDEX);
                        }
                    }
                }
            }
            return tile;
        }

        protected List<byte> GetByte(long trail, TrailTypes trailType)
        {
            List<byte> result = new List<byte>();
            int baseValue = 2;
            int pow = 7;
            byte factor;

            if (trail >= (FUNCINDEX)) trail = trail % (FUNCINDEX);

            switch (trailType)
            {
                case TrailTypes.Road:
                    trail %= DIRTINDEX;
                    break;
                case TrailTypes.DirtRoad:
                    trail = ((trail % TRACKINDEX) / DIRTINDEX);
                    break;
                case TrailTypes.Track:
                    trail /= TRACKINDEX;
                    break;
                default:
                    Debug.Log("GetByte = wrong TrailTypes passed to this function");
                    break;
            }

            do{
                factor = (byte)Math.Pow(baseValue, pow);

                if (trail >= factor)
                {
                    trail -= factor;
                    result.Add(factor);
                }

                pow--;
            }
            while (pow >= 0 && trail > 0);

            return result;            
        }

        protected long AddByte(long trail, byte trailToAdd, TrailTypes trailType, byte trailToRemove = 0)
        {
            List<byte> resultTrack = GetByte(trail, TrailTypes.Track);
            List<byte> resultDirt = GetByte(trail, TrailTypes.DirtRoad);
            List<byte> resultRoad = GetByte(trail, TrailTypes.Road);

            resultTrack.Remove(trailToAdd);
            resultTrack.Remove(trailToRemove);
            resultDirt.Remove(trailToAdd);
            resultDirt.Remove(trailToRemove);
            resultRoad.Remove(trailToAdd);
            resultRoad.Remove(trailToRemove);

            trail = 0;
            foreach (byte track in resultTrack) trail += track * TRACKINDEX;
            foreach (byte dirt in resultDirt) trail += dirt * DIRTINDEX;
            foreach (byte road in resultRoad) trail += road;

            switch (trailType)
            {
                case TrailTypes.Road:
                    trail += trailToAdd;
                    break;
                case TrailTypes.DirtRoad:
                    trail += trailToAdd * DIRTINDEX;
                    break;
                case TrailTypes.Track:
                    trail += trailToAdd * TRACKINDEX;
                    break;
                default:
                    Debug.Log("AddByte = wrong TrailTypes passed to this function");
                    break;
            }
            return trail;            
        }

        protected long AddByte(long trail, byte trailToAdd1, byte trailToAdd2, TrailTypes trailType, byte trailToRemove = 0)
        {
            List<byte> resultTrack = GetByte(trail, TrailTypes.Track);
            List<byte> resultDirt = GetByte(trail, TrailTypes.DirtRoad);
            List<byte> resultRoad = GetByte(trail, TrailTypes.Road);

            resultTrack.Remove(trailToAdd1);
            resultTrack.Remove(trailToAdd2);
            resultTrack.Remove(trailToRemove);
            resultDirt.Remove(trailToAdd1);
            resultDirt.Remove(trailToAdd2);
            resultDirt.Remove(trailToRemove);
            resultRoad.Remove(trailToAdd1);
            resultRoad.Remove(trailToAdd2);
            resultRoad.Remove(trailToRemove);

            trail = 0;
            foreach (byte track in resultTrack) trail += track * TRACKINDEX;
            foreach (byte dirt in resultDirt) trail += dirt * DIRTINDEX;
            foreach (byte road in resultRoad) trail += road;

            switch (trailType)
            {
                case TrailTypes.Road:
                    trail += trailToAdd1;
                    trail += trailToAdd2;
                    break;
                case TrailTypes.DirtRoad:
                    trail += trailToAdd1 * DIRTINDEX;
                    trail += trailToAdd2 * DIRTINDEX;
                    break;
                case TrailTypes.Track:
                    trail += trailToAdd1 * TRACKINDEX;
                    trail += trailToAdd2 * TRACKINDEX;
                    break;
            }
            return trail;            
        }

        protected byte[,] GetLargeHeightmapPixel(byte[,] tile, int x, int y)
        {
            byte[,] outPixel = new byte[5, 5];

            for (int X = 0; X < 5; X++)
            {
                for (int Y = 0; Y < 5; Y++)
                {
                    outPixel[X, Y] = tile[x * 5 + X, y * 5 + Y];
                }
            }

            return outPixel;
        }

        /// <summary>
        /// Generate a 5x5 tile of trails, taking into consideration the pixel enter/exit
        /// and the need to align to a walled or un-walled location.
        /// </summary>
        protected long[,] GetTileBuffer(byte trailAsset, TrailTypes trailType, byte[,] buffer, long[,] tileBuffer, bool hasLocation, DFLocation location, int crssNumb)
        {
            long[,] resultingBuffer = new long[5, 5];
            resultingBuffer = tileBuffer;

            // if (resultingBuffer[12] == 0)
            //     resultingBuffer[12] = trailType;

            int baseValue = 2;
            int pow = 7;

            bool[,] thread = new bool[5, 5];
            List<(int, int)> tileExit = new List<(int, int)>();
            List<(int, int)> walledExit = new List<(int, int)>();

            // if (trailType == 1)
            //     trailType *= 32;
            // else trailType *= 64;

            // Here we set enter/exit sectors based on cardinals
            do{
                byte factor = (byte)Math.Pow(baseValue, pow);

                if (trailAsset >= factor)
                {
                    trailAsset -= factor;

                    // Debug.Log("factor: " + factor);

                    switch (factor)
                    {
                        case SW:
                            thread[0, 4] = true;
                            tileExit.Add((0, 4));
                            break;

                        case S:
                            thread[1, 4] = thread[2, 4] = thread[3, 4] = true;
                            // tileExit.Add((1, 4));
                            tileExit.Add((2, 4));
                            // tileExit.Add((3, 4));
                            break;

                        case SE:
                            thread[4, 4] = true;
                            tileExit.Add((4, 4));
                            break;

                        case E:
                            thread[4, 1] = thread[4, 2] = thread[4, 3] = true;
                            // tileExit.Add((4, 1));
                            tileExit.Add((4, 2));
                            // tileExit.Add((4, 3));
                            break;

                        case NE:
                            thread[4, 0] = true;
                            tileExit.Add((4, 0));
                            break;

                        case N:
                            thread[1, 0] = thread[2, 0] = thread[3, 0] = true;
                            // tileExit.Add((1, 0));
                            tileExit.Add((2, 0));
                            // tileExit.Add((3, 0));
                            break;

                        case NW:
                            thread[0, 0] = true;
                            tileExit.Add((0, 0));
                            break;

                        case W:
                            thread[0, 1] = thread[0, 2] = thread[0, 3] = true;
                            // tileExit.Add((0, 1));
                            tileExit.Add((0, 2));
                            // tileExit.Add((0, 3));
                            break;

                        default:
                            Debug.Log("Error while generating tile buffer!");
                            break;
                    }
                }

                pow--;
            }
            while (pow >= 0 && trailAsset > 0);

            // Now we set some sort of "waypoints" that the trails has to traverse,
            // based on the presence of walled or un-walled locations.
            if (hasLocation)
            {
                if (IsWalledLocation(location))
                {
                    List<(int, int)> exitRef = new List<(int, int)>();
                    exitRef = tileExit;
                    int exitNumb = tileExit.Count();
                    int counter = 1;
                    for (int ext = 0; ext < exitNumb; ext++)
                    {
                        (int, int) wllExt = GetWalledLocEntrance(location, exitRef[ext]);
                        if (!tileExit.Contains(wllExt))
                        {
                            tileExit.Insert((counter), wllExt);
                            counter++;
                        }
                        walledExit.Add(wllExt);
                    }
                }
                else
                    tileExit.Insert(1, (2, 2));
            }

            bool getToJunction = false;
            if (tileExit.Count == 1)
            {
                Debug.Log("tileExit.Count == 1; creating fake passage");
                (int, int) fakePassage = (2, 2);
                if (tileExit[0].Item1 == 0) fakePassage.Item1 = 4;
                if (tileExit[0].Item1 == 4) fakePassage.Item1 = 0;
                if (tileExit[0].Item2 == 0) fakePassage.Item2 = 4;
                if (tileExit[0].Item2 == 4) fakePassage.Item2 = 0;
                if (tileExit[0].Item1 == 2) fakePassage.Item1 = 2;
                if (tileExit[0].Item2 == 2) fakePassage.Item2 = 2;
                tileExit.Add(fakePassage);
                getToJunction = true;
            }

            List<(int, int)> trail = new List<(int, int)>();

            for (int browse = 0; browse < tileExit.Count - 1; browse++)
            {
                trail = new List<(int, int)>();
                
                (int, int) start = tileExit[browse];
                (int, int) arrival = tileExit[browse + 1];

                // Debug.Log("start: " + start.Item1 + ", " + start.Item2 + "; arrival: " + arrival.Item1 + ", " + arrival.Item2);

                int xDirection, yDirection;
                bool gotToDestination = false;

                trail.Add(start);

                do
                {
                    CalculateDirectionAndDiff(start, arrival, out xDirection, out yDirection);

                    byte startingHeight = 0;
                    int heightDiff = 0;
                    gotToDestination = false;
                    
                    List<(int, int)> stepToRemove = new List<(int, int)>();
                    List<(int, int)> movementChoice = ProbeDirections(start, xDirection, yDirection, buffer);

                    if (movementChoice.Count > 0)
                    {
                        byte bestDiff = byte.MaxValue;

                        foreach ((int, int) potentialStep in movementChoice)
                        {
                            if (Math.Abs(startingHeight - buffer[potentialStep.Item1, potentialStep.Item2]) > bestDiff)
                            {
                                stepToRemove.Add(potentialStep);
                            }
                            else
                            {
                                bestDiff = (byte)Math.Abs(startingHeight - buffer[potentialStep.Item1, potentialStep.Item2]);
                            }
                        }
                    }

                    if (stepToRemove.Count > 0)
                    {
                        // Debug.Log("stepToRemove.Count: " + stepToRemove.Count);
                        foreach ((int, int) stepToRem in stepToRemove)
                        {
                            movementChoice.Remove(stepToRem);
                        }
                        movementChoice.TrimExcess();
                    }

                    if (movementChoice.Count > 0)
                    {
                        bool destinationPresent = false;
                        bool junctionPresent = false;
                        List<int> movementSelected = new List<int>();

                        for (int a = 0; a < movementChoice.Count; a++)
                        {
                            if (CheckArrival(movementChoice[a], arrival))
                            {
                                movementSelected.Add(a);
                                Debug.Log("Destination present among movement choices: " + movementChoice[a]);
                                destinationPresent = true;
                            }
                        }

                        if (!destinationPresent)
                        {
                            for (int b = 0; b < movementChoice.Count; b++)
                            {
                                if (trail.Contains(movementChoice[b]))
                                {
                                    movementSelected.Add(b);
                                    Debug.Log("Junction present among movement choices: " + movementChoice[b]);
                                    junctionPresent = true;

                                    if (getToJunction)
                                    {
                                        Debug.Log("Fake trail terminated at " + movementChoice[b].Item1 + ", " + movementChoice[b].Item2);
                                        gotToDestination = true;
                                    }
                                }
                            }
                        }

                        int randomSelection;

                        if (movementSelected.Count > 0)
                        {
                            randomSelection = UnityEngine.Random.Range(0, movementSelected.Count - 1);
                            Debug.Log("movementChoice[movementSelected[randomSelection]]: " + movementChoice[movementSelected[randomSelection]]);
                            trail.Add(movementChoice[movementSelected[randomSelection]]);
                        }
                        else{
                            randomSelection = UnityEngine.Random.Range(0, movementChoice.Count - 1);
                            Debug.Log("movementChoice[randomSelection]: " + movementChoice[randomSelection]);
                            trail.Add(movementChoice[randomSelection]);
                        }

                        if (CheckArrival(trail[trail.Count - 1], arrival) || gotToDestination)
                        {
                            Debug.Log("Arrived at " + trail[trail.Count - 1].Item1 + ", " + trail[trail.Count - 1].Item2);
                            gotToDestination = true;
                        }
                        else start = movementChoice[randomSelection];
                    }
                    else{
                        Debug.Log("No way to go!");
                        break;
                    }
                }
                while (!gotToDestination);

                resultingBuffer = MergeWithTrail(resultingBuffer, trail, trailType, hasLocation, location, walledExit);
            }
            return resultingBuffer;
        }

        protected (int, int) GetWalledLocEntrance(DFLocation location, (int, int) exitRef)
        {
            Debug.Log("exitRef: " + exitRef.Item1 + ", " + exitRef.Item2);
            byte width = location.Exterior.ExteriorData.Width;
            byte height = location.Exterior.ExteriorData.Height;
            int ext = ConvertExitToDirection(exitRef);

            return GetGatePosition(location.Exterior.ExteriorData.BlockNames, width, height, ext);
        }

        /// <summary>
        /// Get which tile sector a gate placed in a certain spot of the wall interests.
        /// </summary>
        protected (int, int) GetGatePosition(string[] blocks, byte width, byte height, int ext)
        {
            int gateIndex = GetWallGate(blocks, width, height, ext);
            switch (width)
            {
                case 5:
                    switch (height)
                    {
                        case 6:
                            switch (gateIndex)
                            {
                                case 1:  return (1, 4); // 154
                                case 2:  return (2, 4); // 174
                                case 3:  return (3, 4); // 189
                                case 5:  return (0, 3); // 254
                                case 9:  return (4, 3); // 255
                                case 10:                // 100
                                case 15: return (0, 2); // 99
                                case 14:                // 98
                                case 19: return (4, 2); // 97
                                case 20: return (0, 1); // 96
                                case 24: return (4, 1); // 95
                                case 26: return (1, 0); // 145
                                case 27: return (2, 0); // 165
                                case 28: return (3, 0); // 180
                                default:
                                    Debug.Log("GetWallGate returned an incorrect gateIndex for a " + width + "x" + height + "settlement.");
                                    return (-1, -1);
                            }
                        case 7:
                            switch (gateIndex)
                            {
                                case 1:  return (1, 4); // 156
                                case 2:  return (2, 4); // 175
                                case 3:  return (3, 4); // 191
                                case 5:                 // 244
                                case 10: return (0, 3); // 246
                                case 9:                 // 245
                                case 14: return (4, 3); // 247
                                case 15: return (0, 2); // 248
                                case 19: return (4, 2); // 249
                                case 20:                // 250
                                case 25: return (0, 1); // 251
                                case 24:                // 252
                                case 29: return (4, 1); // 253
                                case 31: return (1, 0); // 147
                                case 32: return (2, 0); // 166
                                case 33: return (3, 0); // 182
                                default:
                                    Debug.Log("GetWallGate returned an incorrect gateIndex for a " + width + "x" + height + "settlement.");
                                    return (-1, -1);
                            }
                        default:
                            Debug.Log("GetWallGate returned an incorrect gateIndex for a " + width + "x" + height + "settlement.");
                            return (-1, -1);
                    }
                case 6:
                    switch (height)
                    {
                        case 5:
                            switch (gateIndex)
                            {
                                case 1:  return (1, 4); // 94
                                case 2:                 // 93
                                case 3:  return (2, 4); // 92
                                case 4:  return (3, 4); // 91
                                case 6:  return (0, 3); // 128
                                case 11: return (4, 3); // 221
                                case 12: return (0, 2); // 109
                                case 17: return (4, 2); // 212
                                case 18: return (0, 1); // 113
                                case 23: return (4, 1); // 202
                                case 25: return (1, 0); // 90
                                case 26:                // 89
                                case 27: return (2, 0); // 88
                                case 28: return (3, 0); // 87
                                default:
                                    Debug.Log("GetWallGate returned an incorrect gateIndex for a " + width + "x" + height + "settlement.");
                                    return (-1, -1);
                            }
                        case 6:
                            switch (gateIndex)
                            {
                                case 1:  return (1, 4); // 150
                                case 2:                 // 168
                                case 3:  return (2, 4); // 169
                                case 4:  return (3, 4); // 186
                                case 6:  return (0, 3); // 126
                                case 11: return (4, 3); // 219
                                case 12:                // 107
                                case 18: return (0, 2); // 108
                                case 17:                // 210
                                case 23: return (4, 2); // 211
                                case 24: return (0, 1); // 116
                                case 29: return (4, 1); // 201
                                case 31: return (1, 0); // 141
                                case 32:                // 159
                                case 33: return (2, 0); // 160
                                case 34: return (3, 0); // 177
                                default:
                                    Debug.Log("GetWallGate returned an incorrect gateIndex for a " + width + "x" + height + "settlement.");
                                    return (-1, -1);
                            }
                        case 7:
                            switch (gateIndex)
                            {
                                case 1:  return (1, 4); // 151
                                case 2:                 // 170
                                case 3:  return (2, 4); // 171
                                case 4:  return (3, 4); // 187
                                case 6:                 // 127
                                case 12: return (0, 3); // 128
                                case 11:                // 220
                                case 17: return (4, 3); // 221
                                case 18: return (0, 2); // 109
                                case 23: return (4, 2); // 212
                                case 24:                // 113
                                case 30: return (0, 1); // 114
                                case 29:                // 202
                                case 35: return (4, 1); // 203
                                case 37: return (1, 0); // 142
                                case 38:                // 161
                                case 39: return (2, 0); // 162
                                case 40: return (3, 0); // 178
                                default:
                                    Debug.Log("GetWallGate returned an incorrect gateIndex for a " + width + "x" + height + "settlement.");
                                    return (-1, -1);
                            }
                        case 8:
                            switch (gateIndex)
                            {
                                case 1:  return (1, 4); // 152
                                case 2:                 // 172
                                case 3:  return (2, 4); // 173
                                case 4:  return (3, 4); // 188
                                case 6:  return (0, 4); // 135
                                case 12: return (0, 3); // 126
                                case 11:                // 228
                                case 17: return (4, 3); // 219
                                case 18:                // 107
                                case 24: return (0, 2); // 108
                                case 23:                // 210
                                case 29: return (4, 2); // 211
                                case 30:                // 116
                                case 36: return (0, 1); // 106
                                case 35: return (4, 1); // 201
                                case 41: return (4, 0); // 195
                                case 43: return (1, 0); // 143
                                case 44:                // 163
                                case 45: return (2, 0); // 164
                                case 46: return (3, 0); // 179
                                default:
                                    Debug.Log("GetWallGate returned an incorrect gateIndex for a " + width + "x" + height + "settlement.");
                                    return (-1, -1);
                            }
                        default:
                            Debug.Log("Not contemplated location shape: " + width + "x" + height);
                            return (-1, -1);
                    }
                case 7:
                    switch (height)
                    {
                        case 5:
                            switch (gateIndex)
                            {
                                case 1:                 // 234
                                case 2:  return (1, 4); // 235
                                case 3:  return (2, 4); // 236
                                case 4:                 // 237
                                case 5:  return (3, 4); // 238
                                case 7:  return (0, 3); // 131
                                case 13: return (4, 3); // 224
                                case 14: return (0, 2); // 112
                                case 20: return (4, 2); // 215
                                case 21: return (0, 1); // 118
                                case 27: return (4, 1); // 205
                                case 29:                // 239
                                case 30: return (1, 0); // 240
                                case 31: return (2, 0); // 241
                                case 32:                // 242
                                case 33: return (3, 0); // 243
                                default:
                                    Debug.Log("GetWallGate returned an incorrect gateIndex for a " + width + "x" + height + "settlement.");
                                    return (-1, -1);
                            }
                        case 6:
                            switch (gateIndex)
                            {
                                case 1:                 // 153
                                case 2:  return (1, 4); // 154
                                case 3:  return (2, 4); // 174
                                case 4:                 // 189
                                case 5:  return (3, 4); // 190
                                case 7:  return (0, 3); // 129
                                case 13: return (4, 3); // 222
                                case 14:                // 110
                                case 21: return (0, 2); // 111
                                case 20:                // 213
                                case 27: return (4, 2); // 214
                                case 28: return (0, 1); // 117
                                case 34: return (4, 1); // 204
                                case 36:                // 144
                                case 37: return (1, 0); // 145
                                case 38: return (2, 0); // 165
                                case 39:                // 180
                                case 40: return (3, 0); // 181
                                default:
                                    Debug.Log("GetWallGate returned an incorrect gateIndex for a " + width + "x" + height + "settlement.");
                                    return (-1, -1);
                            }
                        case 7:
                            switch (gateIndex)
                            {
                                case 1:                 // 155
                                case 2:  return (1, 4); // 156
                                case 3:  return (2, 4); // 175
                                case 4:                 // 191
                                case 5:  return (3, 4); // 192
                                case 7:                 // 130
                                case 14: return (0, 3); // 131
                                case 13:                // 223
                                case 20: return (4, 3); // 224
                                case 21: return (0, 2); // 112
                                case 27: return (4, 2); // 215
                                case 28:                // 118
                                case 35: return (0, 1); // 119
                                case 34:                // 205
                                case 41: return (4, 1); // 206
                                case 43:                // 146
                                case 44: return (1, 0); // 147
                                case 45: return (2, 0); // 166
                                case 46:                // 182
                                case 47: return (3, 0); // 183
                                default:
                                    Debug.Log("GetWallGate returned an incorrect gateIndex for a " + width + "x" + height + "settlement.");
                                    return (-1, -1);
                            }
                        case 8:
                            switch (gateIndex)
                            {
                                case 1:                 // 157
                                case 2:  return (1, 4); // 158
                                case 3:  return (2, 4); // 176
                                case 4:                 // 193
                                case 5:  return (3, 4); // 194
                                case 7:  return (0, 4); // 136
                                case 14: return (0, 3); // 129
                                case 13:                // 229
                                case 20: return (4, 3); // 222
                                case 21:                // 110
                                case 28: return (0, 2); // 111
                                case 27:                // 213
                                case 34: return (4, 2); // 214
                                case 35:                // 117
                                case 42: return (0, 1); // 101
                                case 41: return (4, 1); // 204
                                case 48: return (4, 0); // 196
                                case 50:                // 148
                                case 51: return (1, 0); // 149
                                case 52: return (2, 0); // 167
                                case 53:                // 184
                                case 54: return (3, 0); // 185
                                default:
                                    Debug.Log("GetWallGate returned an incorrect gateIndex for a " + width + "x" + height + "settlement.");
                                    return (-1, -1);
                            }
                        default:
                            Debug.Log("Not contemplated location shape: " + width + "x" + height);
                            return (-1, -1);
                    }
                case 8:
                    switch (height)
                    {
                        case 6:
                            switch (gateIndex)
                            {
                                case 1:  return (0, 4); // 137
                                case 2:  return (1, 4); // 150
                                case 3:                 // 168
                                case 4:  return (2, 4); // 169
                                case 5:                 // 186
                                case 6:  return (3, 4); // 230
                                case 8:  return (0, 3); // 132
                                case 15: return (4, 3); // 225
                                case 16:                // 115
                                case 24: return (0, 2); // 120
                                case 23:                // 216
                                case 31: return (4, 2); // 217
                                case 32: return (0, 1); // 121
                                case 39: return (4, 1); // 207
                                case 41: return (0, 0); // 105
                                case 42: return (1, 0); // 141
                                case 43:                // 159
                                case 44: return (2, 0); // 160
                                case 45: return (3, 0); // 177
                                case 46: return (4, 0); // 197
                                default:
                                    Debug.Log("Not contemplated location shape: " + width + "x" + height);
                                    return (-1, -1);
                            }
                        case 7:
                            switch (gateIndex)
                            {
                                case 1:  return (0, 4); // 138
                                case 2:  return (1, 4); // 151
                                case 3:                 // 170
                                case 4:  return (2, 4); // 171
                                case 5:                 // 187
                                case 6:  return (3, 4); // 231
                                case 8:                 // 133
                                case 16: return (0, 3); // 134
                                case 15:                // 226
                                case 23: return (4, 3); // 227
                                case 24: return (0, 2); // 125
                                case 31: return (4, 2); // 218
                                case 32:                // 122
                                case 40: return (0, 1); // 123
                                case 39:                // 208
                                case 47: return (4, 1); // 209
                                case 49: return (0, 0); // 104
                                case 50: return (1, 0); // 142
                                case 51:                // 161
                                case 52: return (2, 0); // 162
                                case 53:                // 178
                                case 54: return (3, 0); // 198
                                default:
                                    Debug.Log("Not contemplated location shape: " + width + "x" + height);
                                    return (-1, -1);
                            }
                        case 8:
                            switch (gateIndex)
                            {
                                case 1:                 // 139
                                case 8:  return (0, 4); // 140
                                case 2:  return (1, 4); // 152
                                case 3:                 // 172
                                case 4:  return (2, 4); // 173
                                case 5:                 // 188
                                case 6:  return (3, 4); // 232                                
                                case 16: return (0, 3); // 132
                                case 15:                // 233
                                case 23: return (4, 3); // 223
                                case 24:                // 115
                                case 32: return (0, 2); // 120
                                case 31:                // 216
                                case 39: return (4, 2); // 217
                                case 40:                // 124
                                case 48: return (0, 1); // 102
                                case 47: return (4, 1); // 207
                                case 57: return (0, 0); // 103
                                case 55: return (4, 0); // 199
                                case 58: return (1, 0); // 143
                                case 59:                // 163
                                case 60: return (2, 0); // 164
                                case 61:                // 179
                                case 62: return (3, 0); // 200
                                default:
                                    Debug.Log("Not contemplated location shape: " + width + "x" + height);
                                    return (-1, -1);
                            }
                        default:
                            Debug.Log("Not contemplated location shape: " + width + "x" + height);
                            return (-1, -1);
                    }
                default:
                    Debug.Log("Not contemplated location shape: " + width + "x" + height);
                    return (-1, -1);
            }
            Debug.Log("Not contemplated location shape: " + width + "x" + height);
            return (-1, -1);
        }

        /// <summary>
        /// Get the block index of the ckecked gate.
        /// </summary>
        protected int GetWallGate(string[] blocks, byte width, byte height, int side)
        {
            int b, c, n, s, w, e, gateIndex1, gateIndex2;
            n = s = w = e = gateIndex1 = gateIndex2 = -1;
            int counter = 0;
            switch (side)
            {
                case N:
                    for (int a = (blocks.Length - width); a < blocks.Length; a++)
                        if (blocks[a].Equals("WALLAA08.RMB") || blocks[a].Equals("WALLAA20.RMB"))
                            return a;
                    break;
                case S:
                    for (int a = 0; a < width; a++)
                        if (blocks[a].Equals("WALLAA10.RMB") || blocks[a].Equals("WALLAA22.RMB"))
                            return a;
                    break;
                case W:
                    for (int a = width; a < (blocks.Length - width); a += width)
                        if (blocks[a].Equals("WALLAA11.RMB") || blocks[a].Equals("WALLAA23.RMB"))
                            return a;
                    break;
                case E:
                    for (int a = (width + width - 1); a < (blocks.Length - width); a += width)
                        if (blocks[a].Equals("WALLAA09.RMB") || blocks[a].Equals("WALLAA21.RMB"))
                            return a;
                    break;

                case NW:
                    counter = 0;
                    for (b = (blocks.Length - width); b < blocks.Length; b++)
                    {
                        if (blocks[b].Equals("WALLAA08.RMB") || blocks[b].Equals("WALLAA20.RMB"))
                        {
                            n = counter;
                            gateIndex1 = b;
                        }                            
                        counter++;
                    }                        
                    for (c = 0; c <= (blocks.Length - width); c += width)
                    {
                        if (blocks[c].Equals("WALLAA11.RMB") || blocks[c].Equals("WALLAA23.RMB"))
                        {
                            w = counter;
                            gateIndex2 = c;
                        }                            
                        counter++;
                    }
                    if (n < (height - w)) return gateIndex1;
                    if ((height - w) < n) return gateIndex2;
                    if (width > height) return gateIndex2;
                    if (height > width) return gateIndex1;
                    if (UnityEngine.Random.Range(0, 2) == 0) return gateIndex1;
                    else return gateIndex2;
                case NE:
                    counter = 0;
                    for (b = (blocks.Length - width); b < blocks.Length; b++)
                    {
                        if (blocks[b].Equals("WALLAA08.RMB") || blocks[b].Equals("WALLAA20.RMB"))
                        {
                            n = counter;
                            gateIndex1 = b;
                        }                            
                        counter++;
                    }                        
                    for (c = (width - 1); c < blocks.Length; c += width)
                    {
                        if (blocks[c].Equals("WALLAA09.RMB") || blocks[c].Equals("WALLAA21.RMB"))
                        {
                            e = counter;
                            gateIndex2 = c;
                        }                            
                        counter++;
                    }
                    if (n > e) return gateIndex1;
                    if (e > n) return gateIndex2;
                    if (width > height) return gateIndex2;
                    if (height > width) return gateIndex1;
                    if (UnityEngine.Random.Range(0, 2) == 0) return gateIndex1;
                    else return gateIndex2;
                case SW:
                    counter = 0;
                    for (b = 0; b < width; b++)
                    {
                        if (blocks[b].Equals("WALLAA10.RMB") || blocks[b].Equals("WALLAA22.RMB"))
                        {
                            s = counter;
                            gateIndex1 = b;
                        }                            
                        counter++;
                    }                        
                    for (c = 0; c <= (blocks.Length - width); c += width)
                    {
                        if (blocks[c].Equals("WALLAA11.RMB") || blocks[c].Equals("WALLAA23.RMB"))
                        {
                            w = counter;
                            gateIndex2 = c;
                        }                            
                        counter++;
                    }
                    if (s < w) return gateIndex1;
                    if (w < s) return gateIndex2;
                    if (width > height) return gateIndex2;
                    if (height > width) return gateIndex1;
                    if (UnityEngine.Random.Range(0, 2) == 0) return gateIndex1;
                    else return gateIndex2;
                case SE:
                    counter = 0;
                    for (b = 0; b < width; b++)
                    {
                        if (blocks[b].Equals("WALLAA10.RMB") || blocks[b].Equals("WALLAA22.RMB"))
                        {
                            s = counter;
                            gateIndex1 = b;
                        }                            
                        counter++;
                    }                        
                    for (c = (width + width - 1); c < (blocks.Length - width); c += width)
                    {
                        if (blocks[c].Equals("WALLAA09.RMB") || blocks[c].Equals("WALLAA21.RMB"))
                        {
                            e = counter;
                            gateIndex2 = c;
                        }                            
                        counter++;
                    }
                    if (s > (height - e)) return gateIndex1;
                    if ((height - e) > s) return gateIndex2;
                    if (width > height) return gateIndex2;
                    if (height > width) return gateIndex1;
                    if (UnityEngine.Random.Range(0, 2) == 0) return gateIndex1;
                    else return gateIndex2;
                default:
                    break;
            }
            Debug.Log("Could not find gate in GetWallGate method");
            return -1;
        }

        protected int ConvertExitToDirection((int, int) exitRef)
        {
            if (exitRef.Equals((0, 0))) return NW;            
            if (exitRef.Equals((4, 0))) return NE;            
            if (exitRef.Equals((0, 4))) return SW;            
            if (exitRef.Equals((4, 4))) return SE;

            if (exitRef.Item2 == 0) return N;
            if (exitRef.Item1 == 0) return W;
            if (exitRef.Item1 == 4) return E;
            if (exitRef.Item2 == 4) return S;

            Debug.Log("Problem in ConvertExitToDirection method - fix it");
            return -1;
        }

        protected bool CheckArrival((int, int) movement, (int, int) arrival)
        {
            if (movement.Equals(arrival))
                return true;

            if ((arrival.Item1 == 0 || arrival.Item1 == 4) && (arrival.Item2 == 0 || arrival.Item2 == 4))
                return false;

            if ((arrival.Item1 == 0 && movement.Item1 == 0) || (arrival.Item1 == 4 && movement.Item1 == 4))
            {
                if (movement.Item2 > 0 && movement.Item2 < 4)
                    return true;
            }

            if ((arrival.Item2 == 0 && movement.Item2 == 0) || (arrival.Item2 == 4 && movement.Item2 == 4))
            {
                if (movement.Item1 > 0 && movement.Item1 < 4)
                    return true;
            }

            return false;
        }

        protected long[,] MergeWithTrail(long[,] baseBuffer, List<(int, int)> trailToAdd, TrailTypes trailType, bool hasLocation, DFLocation location, List<(int, int)> wllExt)
        {
            foreach ((int, int) trail in trailToAdd)
            {
                byte trailShape = GetTrailShape(trail, trailToAdd);

                if (baseBuffer[trail.Item1, trail.Item2] <= 0)
                {
                    switch (trailType)
                    {
                        case TrailTypes.Road:
                            baseBuffer[trail.Item1, trail.Item2] = trailShape;
                            break;
                        case TrailTypes.DirtRoad:
                            baseBuffer[trail.Item1, trail.Item2] = trailShape * DIRTINDEX;
                            break;
                        case TrailTypes.Track:
                            baseBuffer[trail.Item1, trail.Item2] = trailShape * TRACKINDEX;
                            break;
                    }
                }
                else
                {
                    // (byte, byte) result;
                    int baseValue = 2;
                    int pow = 7;

                    byte roadWiP = 0;
                    bool hasRoad = false;
                    byte dirtWiP = 0;
                    bool hasDirt = false;
                    byte trackWiP = 0;
                    bool hasTrack = false;

                    roadWiP = (byte)(baseBuffer[trail.Item1, trail.Item2] % DIRTINDEX);
                    dirtWiP = (byte)((baseBuffer[trail.Item1, trail.Item2] % TRACKINDEX) / DIRTINDEX);
                    trackWiP = (byte)((baseBuffer[trail.Item1, trail.Item2] % FUNCINDEX) / TRACKINDEX);

                    if (trackWiP > 0)
                    {
                        hasTrack = true;
                    }
                    if (dirtWiP > 0)
                    {
                        hasDirt = true;
                    }
                    if (roadWiP > 0)
                    {
                        hasRoad = true;
                    }
                    Debug.Log("MergeWithTrail = baseBuffer[trail.Item1, trail.Item2]: " + baseBuffer[trail.Item1, trail.Item2] + ", roadWiP: " + roadWiP + ", dirtWiP: " + dirtWiP + ", trackWiP: " + trackWiP);

                    do
                    {
                        byte factor = (byte)Math.Pow(baseValue, pow);

                        if (trailShape >= factor)
                        {
                            switch (trailType)
                            {
                                case TrailTypes.Road:
                                    baseBuffer[trail.Item1, trail.Item2] += factor;
                                    break;
                                case TrailTypes.DirtRoad:
                                    baseBuffer[trail.Item1, trail.Item2] += factor * DIRTINDEX;
                                    break;
                                case TrailTypes.Track:
                                    baseBuffer[trail.Item1, trail.Item2] += factor * TRACKINDEX;
                                    break;
                            }
                        }
                        if (roadWiP >= factor) roadWiP -= factor;
                        if (trackWiP >= factor) trackWiP -= factor;
                        if (trailShape >= factor) trailShape -= factor;

                        pow--;
                    }
                    while (pow >= 0 && trailShape >= 0);
                }

                if (IsCrossRoad(baseBuffer[trail.Item1, trail.Item2]))
                {
                    if (hasLocation && CrossroadInsideLocation(trail, location))
                    {
                        if (IsWalledLocation(location))
                            baseBuffer[trail.Item1, trail.Item2] = baseBuffer[trail.Item1, trail.Item2] % FUNCINDEX + (walledInternalSign * FUNCINDEX);
                        else
                            baseBuffer[trail.Item1, trail.Item2] += baseBuffer[trail.Item1, trail.Item2] % FUNCINDEX + (internalSign * FUNCINDEX);
                    }
                    else
                    {
                        int crssIndex = (trail.Item2 * 5 + trail.Item1) + 1;
                        Debug.Log("baseBuffer[" + trail.Item1 + ", " + trail.Item2 + "] is crossroad with a value of " + baseBuffer[trail.Item1, trail.Item2] + "; adding " + crssIndex + " * FUNCINDEX to it");
                        baseBuffer[trail.Item1, trail.Item2] = (baseBuffer[trail.Item1, trail.Item2] % FUNCINDEX) + (crssIndex * FUNCINDEX);
                    }
                }
                else    // Giving special index to trails that have to align with RMB's roads and gates
                {
                    if (hasLocation)
                    {
                        if (IsWalledLocation(location))
                        {
                            if (wllExt.Contains(trail))
                            {
                                baseBuffer[trail.Item1, trail.Item2] = GetWalledIndex(baseBuffer[trail.Item1, trail.Item2], trail, location);
                            }
                            // foreach ((int, int) wExt in wllExt)
                            // {
                            //     Debug.Log("wExt.Item1, wExt.Item2: " + wExt.Item1 + ", " + wExt.Item2);
                            //     baseBuffer[wExt.Item1, wExt.Item2] = GetWalledIndex(baseBuffer[wExt.Item1, wExt.Item2], wExt, location);
                            // }
                        }
                        else // No wall location
                        {
                            baseBuffer[trail.Item1, trail.Item2] = GetTrailIndex(baseBuffer[trail.Item1, trail.Item2], trail, location);
                        }
                    }
                }
            }
            return baseBuffer;
        }

        protected int GetTrailIndex(long trail, (int, int) position, DFLocation location)
        {
            int width = location.Exterior.ExteriorData.Width;
            int height = location.Exterior.ExteriorData.Height;

            byte crossroad = (byte)(trail / FUNCINDEX);
            byte track = (byte)((trail % FUNCINDEX) / TRACKINDEX);
            byte dirt = (byte)((trail % TRACKINDEX) / DIRTINDEX);
            byte road = (byte)(trail % DIRTINDEX);
            Debug.Log("GetTrailIndex = trail: " + trail + ", crossroad: " + crossroad + ", track: " + track + ", dirt: " + dirt + ", road: " + road);
            int variant = 0;

            // TODO!!!
            return -1;
        }

        protected long GetWalledIndex(long trail, (int, int) position, DFLocation location)
        {
            int width = location.Exterior.ExteriorData.Width;
            int height = location.Exterior.ExteriorData.Height;

            byte crossroad = (byte)(trail / FUNCINDEX);
            byte track = (byte)((trail % FUNCINDEX) / TRACKINDEX);
            byte dirt = (byte)((trail % TRACKINDEX) / DIRTINDEX);
            byte road = (byte)(trail % DIRTINDEX);
            Debug.Log("GetWalledIndex = trail: " + trail + ", crossroad: " + crossroad + ", track: " + track + ", dirt: " + dirt + ", road: " + road);
            int variant = 0;
            int gate = -1;

            List<int> tracks = new List<int>();
            if (track > 0)
                tracks = GetIndexesFromByte((byte)track);

            List<int> dirtRoads = new List<int>();
            if (dirt > 0)
                dirtRoads = GetIndexesFromByte((byte)dirt);

            List<int> roads = new List<int>();
            if (road > 0)
                roads = GetIndexesFromByte((byte)road);

            switch (position.Item1)
            {
                case 0:
                    switch (position.Item2)
                    {
                        case 0:
                            if (width == 8 && height == 6)
                            {
                                roads.Remove(2);
                                variant = 105;
                                return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                            }
                            if (width == 8 && height == 7)
                            {
                                roads.Remove(2);
                                variant = 104;
                                return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                            }
                            if (width == 8 && height == 8)
                            {
                                roads.Remove(2);
                                roads.Remove(3);
                                variant = 103;
                                return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                            }
                            break;
                        case 1:
                            if (width == 5 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 20)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 96;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 5 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 20)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 250;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 25)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 251;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 5)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 18)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 113;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 24)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 116;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 24)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 113;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 30)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 114;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 24)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 116;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 30)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 106;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 5)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 21)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 118;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 21)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 117;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 28)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 118;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate  == 35)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 119;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 35)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 117;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 42)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 101;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 6)
                            {
                                roads.Remove(1);
                                roads.Remove(2);
                                roads.Remove(3);
                                roads.Remove(4);
                                variant = 121;
                                return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                            }
                            if (width == 8 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 32)
                                {
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 122;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 40)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    variant = 123;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 40)
                                {
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    variant = 124;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 48)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    variant = 102;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            break;
                        case 2:
                            if (width == 5 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 10)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 100;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 15)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 99;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 5 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 15)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 248;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 5)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 12)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 109;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 12)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 107;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 18)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 108;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 18)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 109;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 18)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 107;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 24)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 108;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 5)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 14)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 112;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 14)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 110;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 21)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 111;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 21)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 112;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 21)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 110;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 28)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 111;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 16)
                                {
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 115;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 24)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 120;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 24)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 125;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            break;
                        case 3:
                            if (width == 5 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 5)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 254;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 5 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 5)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 244;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 10)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 246;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 5)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 6)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 128;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 6)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 126;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 6)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 127;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 12)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 128;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 12)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 126;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 5)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 7)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 131;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 7)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 129;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 7)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 130;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 14)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 131;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 14)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 129;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 8)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 132;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 8)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 133;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 16)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 134;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 16)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 132;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            break;
                        case 4:
                            if (width == 6 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 1)
                                {
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 137;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 6)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 135;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 7)
                                {
                                    roads.Remove(2);
                                    roads.Remove(3);
                                    roads.Remove(4);
                                    variant = 136;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 1)
                                {
                                    roads.Remove(4);
                                    variant = 138;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 1)
                                {
                                    roads.Remove(4);
                                    variant = 139;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, W);
                                if (gate == 8)
                                {
                                    roads.Remove(4);
                                    variant = 140;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            break;
                        default:
                            Debug.Log("GetWalledIndex: wrong position " + position.Item1 + "x" + position.Item2 + "settlement.");
                            return -1;
                    }
                    break;
                case 1:
                    switch (position.Item2)
                    {
                        case 0:
                            if (width == 5 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 26)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 145;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 5 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 31)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 147;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 5)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 25)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 90;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 31)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 141;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 37)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 142;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 43)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 143;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 5)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 29)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 239;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 30)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 240;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 36)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 144;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 37)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 145;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 43)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 146;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 44)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 147;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 50)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 148;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 51)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 149;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 42)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 141;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 50)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 142;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 58)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 143;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            break;
                        case 4:
                            if (width == 5 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 1)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 154;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 5 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 1)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 156;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 5)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 1)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 94;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 1)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 150;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 1)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 151;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 1)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 152;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 5)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 1)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 234;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 2)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 235;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 1)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 153;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 2)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 154;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 1)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 155;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 2)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 156;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 1)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 157;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 2)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 158;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 2)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 150;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 2)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 151;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 2)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 152;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            break;
                        default:
                            Debug.Log("GetWalledIndex: wrong position " + position.Item1 + "x" + position.Item2 + "settlement.");
                            return -1;
                    }
                    break;
                case 2:
                    switch (position.Item2)
                    {
                        case 0:
                            if (width == 5 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 27)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 165;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 5 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 32)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 166;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 5)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 26)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 89;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 27)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 88;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 32)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 159;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 33)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 160;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 38)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 161;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 39)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 162;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 44)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 163;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 45)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 164;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 5)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 31)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 241;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 38)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 165;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 45)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 166;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 52)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 167;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 43)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 159;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 44)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 160;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 51)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 161;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 52)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 162;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 59)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 163;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 60)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 164;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            break;
                        case 4:
                            if (width == 5 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 2)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 174;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 5 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 2)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 175;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 5)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 2)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 93;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 3)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 92;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 2)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 168;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 3)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 169;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 2)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 170;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 3)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 171;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 2)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 172;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 3)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 173;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 5)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 3)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 236;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 3)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 174;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 3)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 175;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 3)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 176;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 3)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 168;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 4)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 169;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 3)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 170;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 4)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 171;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 3)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 172;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 4)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 173;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            break;
                        default:
                            Debug.Log("GetWalledIndex: wrong position " + position.Item1 + "x" + position.Item2 + "settlement.");
                            return -1;
                    }
                    break;
                case 3:
                    switch (position.Item2)
                    {
                        case 0:
                            if (width == 5 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 28)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 180;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 5 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 33)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 182;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 5)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 28)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 87;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 34)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 177;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 40)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 178;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 46)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 179;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 5)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 32)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 242;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 33)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 243;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 39)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 180;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 40)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 181;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 46)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 182;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 47)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 183;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 53)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 184;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 54)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 185;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 45)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 177;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 53)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 178;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 54)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 198;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }                                
                            }
                            if (width == 8 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 61)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 179;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 62)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 200;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            break;
                        case 4:
                            if (width == 5 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 3)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 189;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 5 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 3)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 191;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 5)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 4)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 91;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 4)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 186;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 4)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 187;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 4)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 188;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 5 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 4)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 237;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 5)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 238;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 4)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 189;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 5)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 190;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 4)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 191;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 5)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 192;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 4)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 193;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 5)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 194;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 5)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 186;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 6)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 230;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 5)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 187;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 6)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 231;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, S);
                                if (gate == 5)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 188;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 6)
                                {
                                    roads.Remove(4);
                                    roads.Remove(5);
                                    roads.Remove(6);
                                    variant = 232;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            break;
                        default:
                            Debug.Log("GetWalledIndex: wrong position " + position.Item1 + "x" + position.Item2 + "settlement.");
                            return -1;
                    }
                    break;
                case 4:
                    switch (position.Item2)
                    {
                        case 0:
                            if (width == 6 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 41)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 195;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 48)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 196;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, N);
                                if (gate == 46)
                                {
                                    roads.Remove(0);
                                    roads.Remove(1);
                                    roads.Remove(2);
                                    variant = 197;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 55)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 199;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            break;
                        case 1:
                            if (width == 5 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 24)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 95;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 5 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 24)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 252;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 29)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 253;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 5)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 23)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 202;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 29)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 201;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 29)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 202;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 35)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 203;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 6 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 35)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 201;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 5)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 27)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 205;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 34)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 204;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 34)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 205;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 41)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 206;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 7 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 41)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 204;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 39)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 207;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 39)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 208;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 47)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 209;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if (width == 8 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 47)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 207;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            break;
                        case 2:
                            if ( width == 5 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 14)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 98;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 19)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 97;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if ( width == 5 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 19)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 249;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if ( width == 6 && height == 5)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 17)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 212;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if ( width == 6 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 17)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 210;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 23)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 211;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if ( width == 6 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 23)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 212;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if ( width == 6 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 23)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 210;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 29)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 211;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if ( width == 7 && height == 5)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 20)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 215;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if ( width == 7 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 20)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 213;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 27)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 214;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if ( width == 7 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 27)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 215;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if ( width == 7 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 27)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 213;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 34)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 214;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if ( width == 8 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 23)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 216;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 31)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 217;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if ( width == 8 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 31)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 218;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if ( width == 8 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 31)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 216;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 39)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 217;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            break;
                        case 3:
                            if ( width == 5 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 9)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 255;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if ( width == 5 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 9)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 245;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 14)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 247;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if ( width == 6 && height == 5)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 11)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 221;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if ( width == 6 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 11)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 219;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if ( width == 6 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 11)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 220;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 17)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 221;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if ( width == 6 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 11)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 228;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 17)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 219;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if ( width == 7 && height == 5)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 13)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 224;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if ( width == 7 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 13)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 222;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if ( width == 7 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 13)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 223;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 20)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 224;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if ( width == 7 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 13)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 229;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 20)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 222;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if ( width == 8 && height == 6)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 15)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 225;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if ( width == 8 && height == 7)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 15)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 226;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 23)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 227;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            if ( width == 8 && height == 8)
                            {
                                gate = GetWallGate(location.Exterior.ExteriorData.BlockNames, (byte)width, (byte)height, E);
                                if (gate == 15)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 233;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                                if (gate == 23)
                                {
                                    roads.Remove(0);
                                    roads.Remove(6);
                                    roads.Remove(7);
                                    variant = 225;
                                    return RecodeTrail(roads, dirtRoads, tracks, crossroad, variant);
                                }
                            }
                            break;
                        default:
                            Debug.Log("GetWalledIndex: wrong position " + position.Item1 + "x" + position.Item2 + "settlement.");
                            return -1;
                    }
                    break;
                default:
                    Debug.Log("GetWalledIndex: wrong position " + position.Item1 + "x" + position.Item2 + "settlement.");
                    return -1;
            }
            Debug.Log("GetWalledIndex: wrong position " + position.Item1 + "x" + position.Item2 + "settlement.");
            return -1;
        }

        protected long RecodeTrail(List<int> road, List<int> dirt, List<int> track, int crossroad, int variant)
        {
            long trail = 0;
            int roads = RecalculateByteFromList(road);
            int dirtRoads = RecalculateByteFromList(dirt);
            int tracks = RecalculateByteFromList(track);

            if (variant == 0)
                trail = roads + (dirtRoads * DIRTINDEX) + (tracks * TRACKINDEX) + (crossroad * FUNCINDEX);
            else
                trail = roads + (dirtRoads * DIRTINDEX) + (tracks * TRACKINDEX) + (variant * FUNCINDEX);

            return trail;
        }

        protected int RecalculateByteFromList(List<int> trail)
        {
            int trails = 0;
            foreach (int track in trail)
            {
                trails += (int)Math.Pow(2.0f, (double)track);
            }
            return trails;
        }

        protected bool CrossroadInsideLocation((int, int) trail, DFLocation location)
        {
            int locWidth = location.Exterior.ExteriorData.Width * 16;
            int locHeight = location.Exterior.ExteriorData.Height * 16;

            if ((64 - locWidth / 2 <= trail.Item1 * 25 && 64 + locWidth / 2 >= trail.Item1 * 25) &&
                (64 - locHeight / 2 <= trail.Item2 * 25 && 64 + locHeight / 2 >= trail.Item2 * 25))
                return true;
            return false;
        }

        /// <summary>
        /// Check if a location is walled based on the presence of "WALL" RMBs;
        /// simple, but effective.
        /// </summary>
        protected bool IsWalledLocation(DFLocation location)
        {
            int counter = 0;
            bool isWalled = false;

            while (counter < location.Exterior.ExteriorData.BlockNames.Length && !isWalled)
            {
                if (location.Exterior.ExteriorData.BlockNames[counter].StartsWith("WALL"))
                    isWalled = true;
                counter++;
            }
            return isWalled;
        }

        protected bool RoadEnteringLocation(int compositeTrail, (int, int) trail, DFLocation location)
        {
            int width = location.Exterior.ExteriorData.Width;
            int height = location.Exterior.ExteriorData.Height;

            if (IsWalledLocation(location))
            {
                width -= 2;
                height -= 2;
            }

            int locWidth = width * 16;
            int locHeight = height * 16;

            byte trailShape = (byte)((compositeTrail % FUNCINDEX) / TRACKINDEX + (compositeTrail % TRACKINDEX) / DIRTINDEX + compositeTrail % DIRTINDEX);
            int dir1 = -1;
            int dir2 = -1;
            List<int> directions = new List<int>();

            for (int shift = 7; shift >= 0; shift--)
            {
                if ((trailShape >> shift) % 2 != 0)
                {
                    if (dir1 == -1) dir1 = shift;
                    else dir2 = shift;
                }
            }
            directions.Add(dir1);
            directions.Add(dir2);

            switch (width)
            {
                case 1:
                    switch (height)
                    {
                        case 1:
                            if (trail.Equals((2, 2)))
                                return true;
                            else return false;
                            
                        case 2:
                            if (trail.Equals((2, 2)) && (directions.Contains(0) ||
                                                         directions.Contains(2) ||
                                                         directions.Contains(3) ||
                                                         directions.Contains(4) ||
                                                         directions.Contains(6) ||
                                                         directions.Contains(7)))
                                return true;
                            if (trail.Equals((2, 1)) && directions.Contains(1)) return true;
                            if (trail.Equals((2, 3)) && directions.Contains(5)) return true;
                            return false;

                        case 3: // Rare 1x3 village shape
                            if (trail.Equals((2, 2)) && (directions.Contains(0) ||
                                                         directions.Contains(2) ||
                                                         directions.Contains(3) ||
                                                         directions.Contains(4) ||
                                                         directions.Contains(6) ||
                                                         directions.Contains(7)))
                                return true;
                            if (trail.Equals((2, 1)) && (directions.Contains(0) || 
                                                         directions.Contains(1) ||
                                                         directions.Contains(2)))
                                return true;
                            if (trail.Equals((2, 3)) && (directions.Contains(4) ||
                                                         directions.Contains(5) ||
                                                         directions.Contains(6)))
                                return true;
                            return false;
                        default:
                            Debug.Log("Not contemplated location shape: " + width + "x" + height);
                            return false;
                    }
                case 2:
                    switch (height)
                    {
                        case 1:
                            if (trail.Equals((2, 2)) && (directions.Contains(0) ||
                                                         directions.Contains(1) ||
                                                         directions.Contains(2) ||
                                                         directions.Contains(4) ||
                                                         directions.Contains(5) ||
                                                         directions.Contains(6)))
                                return true;
                            if (trail.Equals((1, 2)) && directions.Contains(3)) return true;
                            if (trail.Equals((3, 2)) && directions.Contains(7)) return true;
                            return false;
                        case 2:
                        case 3: // Should be treated the same as 2x2 (TEST required, but at the moment EVERYTHING requires test...)
                            if (trail.Equals((1, 1)) && directions.Contains(2)) return true;
                            if (trail.Equals((3, 1)) && directions.Contains(0)) return true;
                            if (trail.Equals((1, 3)) && directions.Contains(4)) return true;
                            if (trail.Equals((3, 3)) && directions.Contains(6)) return true;
                            if (trail.Equals((2, 1)) && (directions.Contains(0) ||
                                                         directions.Contains(1) ||
                                                         directions.Contains(2))) 
                                return true;
                            if (trail.Equals((1, 2)) && (directions.Contains(2) ||
                                                         directions.Contains(3) ||
                                                         directions.Contains(4))) 
                                return true;
                            if (trail.Equals((3, 2)) && (directions.Contains(0) ||
                                                         directions.Contains(6) ||
                                                         directions.Contains(7))) 
                                return true;
                            if (trail.Equals((2, 3)) && (directions.Contains(4) ||
                                                         directions.Contains(5) ||
                                                         directions.Contains(6))) 
                                return true;
                            return false;
                        case 4: // Rare 2x4 hamlet shape
                            if (trail.Equals((2, 1)) && (directions.Contains(4) ||
                                                         directions.Contains(5) ||
                                                         directions.Contains(6))) 
                                return true;
                            if (trail.Equals((2, 3)) && (directions.Contains(0) ||
                                                         directions.Contains(1) ||
                                                         directions.Contains(2))) 
                                return true;
                            if (trail.Equals((1, 1)) && (directions.Contains(2) ||
                                                         directions.Contains(3)))
                                return true;
                            if (trail.Equals((3, 1)) && (directions.Contains(0) ||
                                                         directions.Contains(7)))
                                return true;
                            if (trail.Equals((1, 3)) && (directions.Contains(3) ||
                                                         directions.Contains(4)))
                                return true;
                            if (trail.Equals((3, 3)) && (directions.Contains(6) ||
                                                         directions.Contains(7)))
                                return true;
                            if (trail.Equals((1, 2)) && (directions.Contains(2) ||
                                                         directions.Contains(3) ||
                                                         directions.Contains(4)))
                                return true;
                            if (trail.Equals((3, 2)) && (directions.Contains(0) ||
                                                         directions.Contains(6) ||
                                                         directions.Contains(7)))
                                return true;
                            return false;
                        default:
                            Debug.Log("Not contemplated location shape: " + width + "x" + height);
                            return false;
                    }
                case 3:
                    switch (height)
                    {
                        case 1: // Rare 3x1 village shape
                            if (trail.Equals((2, 2)) && (directions.Contains(0) ||
                                                         directions.Contains(1) ||
                                                         directions.Contains(2) ||
                                                         directions.Contains(4) ||
                                                         directions.Contains(5) ||
                                                         directions.Contains(6)))
                                return true;
                            if (trail.Equals((1, 2)) && (directions.Contains(2) || 
                                                         directions.Contains(3) ||
                                                         directions.Contains(4)))
                                return true;
                            if (trail.Equals((3, 2)) && (directions.Contains(0) || 
                                                         directions.Contains(6) ||
                                                         directions.Contains(7)))
                                return true;
                            return false;
                        case 2:
                        case 3:
                            if (trail.Equals((1, 1)) && directions.Contains(2)) return true;
                            if (trail.Equals((3, 1)) && directions.Contains(0)) return true;
                            if (trail.Equals((1, 3)) && directions.Contains(4)) return true;
                            if (trail.Equals((3, 3)) && directions.Contains(6)) return true;
                            if (trail.Equals((2, 1)) && (directions.Contains(0) ||
                                                         directions.Contains(1) ||
                                                         directions.Contains(2))) 
                                return true;
                            if (trail.Equals((1, 2)) && (directions.Contains(2) ||
                                                         directions.Contains(3) ||
                                                         directions.Contains(4))) 
                                return true;
                            if (trail.Equals((3, 2)) && (directions.Contains(0) ||
                                                         directions.Contains(6) ||
                                                         directions.Contains(7))) 
                                return true;
                            if (trail.Equals((2, 3)) && (directions.Contains(4) ||
                                                         directions.Contains(5) ||
                                                         directions.Contains(6))) 
                                return true;
                            return false;
                        case 4:
                            if (trail.Equals((2, 1)) && (directions.Contains(4) ||
                                                         directions.Contains(5) ||
                                                         directions.Contains(6))) 
                                return true;
                            if (trail.Equals((2, 3)) && (directions.Contains(0) ||
                                                         directions.Contains(1) ||
                                                         directions.Contains(2))) 
                                return true;
                            if ((trail.Equals((1, 1)) ||
                                 trail.Equals((1, 2)) ||
                                 trail.Equals((1, 3))) && (directions.Contains(2) ||
                                                           directions.Contains(3) ||
                                                           directions.Contains(4)))
                                return true;
                            if ((trail.Equals((3, 1)) ||
                                 trail.Equals((3, 2)) ||
                                 trail.Equals((3, 3))) && (directions.Contains(0) ||
                                                           directions.Contains(6) ||
                                                           directions.Contains(7)))
                                return true;
                            return false;
                        default:
                            Debug.Log("Not contemplated location shape: " + width + "x" + height);
                            return false;
                    }
                case 4:
                    switch (height)
                    {
                        case 2: // Rare 4x2 hamlet shape
                            if (trail.Equals((1, 2)) && (directions.Contains(0) ||
                                                         directions.Contains(6) ||
                                                         directions.Contains(7))) 
                                return true;
                            if (trail.Equals((3, 2)) && (directions.Contains(2) ||
                                                         directions.Contains(3) ||
                                                         directions.Contains(4))) 
                                return true;
                            if (trail.Equals((1, 1)) && (directions.Contains(1) ||
                                                         directions.Contains(2)))
                                return true;
                            if (trail.Equals((3, 1)) && (directions.Contains(0) ||
                                                         directions.Contains(1)))
                                return true;
                            if (trail.Equals((1, 3)) && (directions.Contains(4) ||
                                                         directions.Contains(5)))
                                return true;
                            if (trail.Equals((3, 3)) && (directions.Contains(5) ||
                                                         directions.Contains(6)))
                                return true;
                            if (trail.Equals((2, 1)) && (directions.Contains(0) ||
                                                         directions.Contains(1) ||
                                                         directions.Contains(2)))
                                return true;
                            if (trail.Equals((2, 3)) && (directions.Contains(4) ||
                                                         directions.Contains(5) ||
                                                         directions.Contains(6)))
                                return true;
                            return false;
                        case 3:
                            if (trail.Equals((1, 2)) && (directions.Contains(0) ||
                                                         directions.Contains(6) ||
                                                         directions.Contains(7))) 
                                return true;
                            if (trail.Equals((3, 2)) && (directions.Contains(2) ||
                                                         directions.Contains(3) ||
                                                         directions.Contains(4))) 
                                return true;
                            if ((trail.Equals((1, 1)) ||
                                 trail.Equals((2, 1)) ||
                                 trail.Equals((3, 1))) && (directions.Contains(0) ||
                                                           directions.Contains(1) ||
                                                           directions.Contains(2)))
                                return true;
                            if ((trail.Equals((1, 3)) ||
                                 trail.Equals((2, 3)) ||
                                 trail.Equals((3, 3))) && (directions.Contains(4) ||
                                                           directions.Contains(5) ||
                                                           directions.Contains(6)))
                                return true;
                            return false;
                        case 4:
                            if (trail.Equals((1, 1)) && (directions.Contains(0) ||
                                                         directions.Contains(4) ||
                                                         directions.Contains(5) ||
                                                         directions.Contains(6) ||
                                                         directions.Contains(7)))
                                return true;
                            if (trail.Equals((3, 1)) && (directions.Contains(2) ||
                                                         directions.Contains(3) ||
                                                         directions.Contains(4) ||
                                                         directions.Contains(5) ||
                                                         directions.Contains(6)))
                                return true;
                            if (trail.Equals((1, 3)) && (directions.Contains(0) ||
                                                         directions.Contains(1) ||
                                                         directions.Contains(2) ||
                                                         directions.Contains(6) ||
                                                         directions.Contains(7)))
                                return true;
                            if (trail.Equals((3, 3)) && (directions.Contains(0) ||
                                                         directions.Contains(1) ||
                                                         directions.Contains(2) ||
                                                         directions.Contains(3) ||
                                                         directions.Contains(4)))
                                return true;
                            if (trail.Equals((2, 1)) && (directions.Contains(4) ||
                                                         directions.Contains(5) ||
                                                         directions.Contains(6)))
                                return true;
                            if (trail.Equals((2, 3)) && (directions.Contains(0) ||
                                                         directions.Contains(1) ||
                                                         directions.Contains(2)))
                                return true;
                            if (trail.Equals((1, 2)) && (directions.Contains(0) ||
                                                         directions.Contains(6) ||
                                                         directions.Contains(7)))
                                return true;
                            if (trail.Equals((3, 2)) && (directions.Contains(2) ||
                                                         directions.Contains(3) ||
                                                         directions.Contains(4)))
                                return true;
                            return false;
                        case 5:
                            if ((trail.Equals((1, 1)) ||
                                 trail.Equals((1, 2)) ||
                                 trail.Equals((1, 3))) && (directions.Contains(0) ||
                                                           directions.Contains(6) ||
                                                           directions.Contains(7)))
                                return true;
                            if ((trail.Equals((3, 1)) ||
                                 trail.Equals((3, 2)) ||
                                 trail.Equals((3, 3))) && (directions.Contains(2) ||
                                                           directions.Contains(3) ||
                                                           directions.Contains(4)))
                                return true;
                            if (trail.Equals((1, 0)) && (directions.Contains(1) ||
                                                         directions.Contains(2)))
                                return true;
                            if (trail.Equals((3, 0)) && (directions.Contains(0) ||
                                                         directions.Contains(1)))
                                return true;
                            if (trail.Equals((1, 4)) && (directions.Contains(4) ||
                                                         directions.Contains(5)))
                                return true;
                            if (trail.Equals((3, 0)) && (directions.Contains(5) ||
                                                         directions.Contains(6)))
                                return true;
                            if (trail.Equals((2, 0)) && (directions.Contains(0) ||
                                                         directions.Contains(1) ||
                                                         directions.Contains(2)))
                                return true;
                            if (trail.Equals((2, 4)) && (directions.Contains(4) ||
                                                         directions.Contains(5) ||
                                                         directions.Contains(6)))
                                return true;
                            return false;
                        case 6:
                            if ((trail.Equals((1, 0)) ||
                                 trail.Equals((2, 0)) ||
                                 trail.Equals((3, 0))) && (directions.Contains(0) ||
                                                           directions.Contains(1) ||
                                                           directions.Contains(2)))
                                return true;
                            if ((trail.Equals((1, 4)) ||
                                 trail.Equals((2, 4)) ||
                                 trail.Equals((3, 4))) && (directions.Contains(4) ||
                                                           directions.Contains(5) ||
                                                           directions.Contains(6)))
                                return true;
                            if ((trail.Equals((1, 1)) ||
                                 trail.Equals((1, 2)) ||
                                 trail.Equals((1, 3))) && (directions.Contains(0) ||
                                                           directions.Contains(6) ||
                                                           directions.Contains(7)))
                                return true;
                            if ((trail.Equals((3, 1)) ||
                                 trail.Equals((3, 2)) ||
                                 trail.Equals((3, 3))) && (directions.Contains(2) ||
                                                           directions.Contains(3) ||
                                                           directions.Contains(4)))
                                return true;
                            return false;
                        case 7:
                            if ((trail.Equals((1, 1)) ||
                                 trail.Equals((1, 2)) ||
                                 trail.Equals((1, 3))) && (directions.Contains(0) ||
                                                           directions.Contains(6) ||
                                                           directions.Contains(7)))
                                return true;
                            if ((trail.Equals((3, 1)) ||
                                 trail.Equals((3, 2)) ||
                                 trail.Equals((3, 3))) && (directions.Contains(2) ||
                                                           directions.Contains(3) ||
                                                           directions.Contains(4)))
                                return true;
                            if (trail.Equals((2, 0)) && (directions.Contains(4) ||
                                                         directions.Contains(5) ||
                                                         directions.Contains(6)))
                                return true;
                            if (trail.Equals((2, 4)) && (directions.Contains(0) ||
                                                         directions.Contains(1) ||
                                                         directions.Contains(2)))
                                return true;
                            if (trail.Equals((1, 0)) && (directions.Contains(0) ||
                                                         directions.Contains(4) ||
                                                         directions.Contains(5) ||
                                                         directions.Contains(6) ||
                                                         directions.Contains(7)))
                                return true;
                            if (trail.Equals((3, 0)) && (directions.Contains(2) ||
                                                         directions.Contains(3) ||
                                                         directions.Contains(4) ||
                                                         directions.Contains(5) ||
                                                         directions.Contains(6)))
                                return true;
                            if (trail.Equals((1, 4)) && (directions.Contains(0) ||
                                                         directions.Contains(1) ||
                                                         directions.Contains(2) ||
                                                         directions.Contains(6) ||
                                                         directions.Contains(7)))
                                return true;
                            if (trail.Equals((3, 4)) && (directions.Contains(0) ||
                                                         directions.Contains(1) ||
                                                         directions.Contains(2) ||
                                                         directions.Contains(3) ||
                                                         directions.Contains(4)))
                                return true;
                            return false;
                        default:
                            Debug.Log("Not contemplated location shape: " + width + "x" + height);
                            return false;
                    }
                case 5:
                    switch (height)
                    {
                        case 4:
                            if ((trail.Equals((1, 1)) ||
                                 trail.Equals((2, 1)) ||
                                 trail.Equals((3, 1))) && (directions.Contains(4) ||
                                                           directions.Contains(5) ||
                                                           directions.Contains(6)))
                                return true;
                            if ((trail.Equals((1, 3)) ||
                                 trail.Equals((2, 3)) ||
                                 trail.Equals((3, 3))) && (directions.Contains(0) ||
                                                           directions.Contains(1) ||
                                                           directions.Contains(2)))
                                return true;
                            if (trail.Equals((0, 1)) && (directions.Contains(2) ||
                                                         directions.Contains(3)))
                                return true;
                            if (trail.Equals((4, 1)) && (directions.Contains(0) ||
                                                         directions.Contains(7)))
                                return true;
                            if (trail.Equals((0, 3)) && (directions.Contains(3) ||
                                                         directions.Contains(4)))
                                return true;
                            if (trail.Equals((4, 3)) && (directions.Contains(6) ||
                                                         directions.Contains(7)))
                                return true;
                            if (trail.Equals((0, 2)) && (directions.Contains(2) ||
                                                         directions.Contains(3) ||
                                                         directions.Contains(4)))
                                return true;
                            if (trail.Equals((4, 2)) && (directions.Contains(0) ||
                                                         directions.Contains(6) ||
                                                         directions.Contains(7)))
                                return true;
                            return false;
                        case 5:
                        case 6:
                            if ((trail.Equals((1, 0)) ||
                                 trail.Equals((2, 0)) ||
                                 trail.Equals((3, 0))) && (directions.Contains(0) ||
                                                           directions.Contains(1) ||
                                                           directions.Contains(2)))
                                return true;
                            if ((trail.Equals((1, 4)) ||
                                 trail.Equals((2, 4)) ||
                                 trail.Equals((3, 4))) && (directions.Contains(4) ||
                                                           directions.Contains(5) ||
                                                           directions.Contains(6)))
                                return true;
                            if ((trail.Equals((0, 1)) ||
                                 trail.Equals((0, 2)) ||
                                 trail.Equals((0, 3))) && (directions.Contains(2) ||
                                                           directions.Contains(3) ||
                                                           directions.Contains(4)))
                                return true;
                            if ((trail.Equals((4, 1)) ||
                                 trail.Equals((4, 2)) ||
                                 trail.Equals((4, 3))) && (directions.Contains(0) ||
                                                           directions.Contains(6) ||
                                                           directions.Contains(7)))
                                return true;
                            if (trail.Equals((0, 0)) && directions.Contains(2))
                                return true;
                            if (trail.Equals((4, 0)) && directions.Contains(0))
                                return true;
                            if (trail.Equals((0, 4)) && directions.Contains(4))
                                return true;
                            if (trail.Equals((4, 4)) && directions.Contains(6))
                                return true;
                            return false;
                        case 7:
                            if ((trail.Equals((1, 0)) ||
                                 trail.Equals((2, 0)) ||
                                 trail.Equals((3, 0))) && (directions.Contains(4) ||
                                                           directions.Contains(5) ||
                                                           directions.Contains(6)))
                                return true;
                            if ((trail.Equals((1, 4)) ||
                                 trail.Equals((2, 4)) ||
                                 trail.Equals((3, 4))) && (directions.Contains(0) ||
                                                           directions.Contains(1) ||
                                                           directions.Contains(2)))
                                return true;
                            if ((trail.Equals((0, 1)) ||
                                 trail.Equals((0, 2)) ||
                                 trail.Equals((0, 3))) && (directions.Contains(2) ||
                                                           directions.Contains(3) ||
                                                           directions.Contains(4)))
                                return true;
                            if ((trail.Equals((4, 1)) ||
                                 trail.Equals((4, 2)) ||
                                 trail.Equals((4, 3))) && (directions.Contains(0) ||
                                                           directions.Contains(6) ||
                                                           directions.Contains(7)))
                                return true;
                            if (trail.Equals((0, 0)) && (directions.Contains(2) ||
                                                         directions.Contains(3)))
                                return true;
                            if (trail.Equals((4, 0)) && (directions.Contains(0) ||
                                                         directions.Contains(7)))
                                return true;
                            if (trail.Equals((0, 4)) && (directions.Contains(3) ||
                                                         directions.Contains(4)))
                                return true;
                            if (trail.Equals((4, 4)) && (directions.Contains(6) ||
                                                         directions.Contains(7)))
                                return true;
                            return false;
                        default:
                            Debug.Log("Not contemplated location shape: " + width + "x" + height);
                            return false;
                    }
                case 6:
                    switch (height)
                    {
                        case 4:
                            if ((trail.Equals((1, 1)) ||
                                 trail.Equals((1, 2)) ||
                                 trail.Equals((1, 3))) && (directions.Contains(4) ||
                                                           directions.Contains(5) ||
                                                           directions.Contains(6)))
                                return true;
                            if ((trail.Equals((1, 3)) ||
                                 trail.Equals((2, 3)) ||
                                 trail.Equals((3, 3))) && (directions.Contains(0) ||
                                                           directions.Contains(1) ||
                                                           directions.Contains(2)))
                                return true;
                            if ((trail.Equals((0, 1)) ||
                                 trail.Equals((0, 2)) ||
                                 trail.Equals((0, 3))) && (directions.Contains(2) ||
                                                           directions.Contains(3) ||
                                                           directions.Contains(4)))
                                return true;
                            if ((trail.Equals((4, 1)) ||
                                 trail.Equals((4, 2)) ||
                                 trail.Equals((4, 3))) && (directions.Contains(0) ||
                                                           directions.Contains(6) ||
                                                           directions.Contains(7)))
                                return true;
                            return false;
                        case 5:
                        case 6:
                            if ((trail.Equals((0, 1)) ||
                                 trail.Equals((0, 2)) ||
                                 trail.Equals((0, 3))) && (directions.Contains(2) ||
                                                           directions.Contains(3) ||
                                                           directions.Contains(4)))
                                return true;
                            if ((trail.Equals((4, 1)) ||
                                 trail.Equals((4, 2)) ||
                                 trail.Equals((4, 3))) && (directions.Contains(0) ||
                                                           directions.Contains(6) ||
                                                           directions.Contains(7)))
                                return true;
                            if ((trail.Equals((1, 0)) ||
                                 trail.Equals((2, 0)) ||
                                 trail.Equals((3, 0))) && (directions.Contains(0) ||
                                                           directions.Contains(1) ||
                                                           directions.Contains(2)))
                                return true;
                            if ((trail.Equals((1, 4)) ||
                                 trail.Equals((2, 4)) ||
                                 trail.Equals((3, 4))) && (directions.Contains(4) ||
                                                           directions.Contains(5) ||
                                                           directions.Contains(6)))
                                return true;
                            if (trail.Equals((0, 0)) && directions.Contains(2))
                                return true;
                            if (trail.Equals((4, 0)) && directions.Contains(0))
                                return true;
                            if (trail.Equals((0, 4)) && directions.Contains(4))
                                return true;
                            if (trail.Equals((4, 4)) && directions.Contains(6))
                                return true;
                            return false;
                        case 7:
                            if ((trail.Equals((0, 0)) ||
                                 trail.Equals((0, 1)) ||
                                 trail.Equals((0, 2)) ||
                                 trail.Equals((0, 3))) && (directions.Contains(2) ||
                                                           directions.Contains(3) ||
                                                           directions.Contains(4)))
                                return true;
                            if ((trail.Equals((4, 0)) ||
                                 trail.Equals((4, 1)) ||
                                 trail.Equals((4, 2)) ||
                                 trail.Equals((4, 3))) && (directions.Contains(0) ||
                                                           directions.Contains(6) ||
                                                           directions.Contains(7)))
                                return true;
                            if ((trail.Equals((1, 0)) ||
                                 trail.Equals((2, 0)) ||
                                 trail.Equals((3, 0))) && (directions.Contains(4) ||
                                                           directions.Contains(5) ||
                                                           directions.Contains(6)))
                                return true;
                            if ((trail.Equals((1, 4)) ||
                                 trail.Equals((2, 4)) ||
                                 trail.Equals((3, 4))) && (directions.Contains(0) ||
                                                           directions.Contains(1) ||
                                                           directions.Contains(2)))
                                return true;
                            return false;
                        default:
                            Debug.Log("Not contemplated location shape: " + width + "x" + height);
                            return false;
                    }
                case 7:
                    switch (height)
                    {
                        case 4:
                            if ((trail.Equals((1, 1)) ||
                                 trail.Equals((2, 1)) ||
                                 trail.Equals((3, 1))) && (directions.Contains(4) ||
                                                           directions.Contains(5) ||
                                                           directions.Contains(6)))
                                return true;
                            if ((trail.Equals((1, 3)) ||
                                 trail.Equals((2, 3)) ||
                                 trail.Equals((3, 3))) && (directions.Contains(0) ||
                                                           directions.Contains(1) ||
                                                           directions.Contains(2)))
                                return true;
                            if (trail.Equals((0, 2)) && (directions.Contains(0) ||
                                                         directions.Contains(6) ||
                                                         directions.Contains(7)))
                                return true;
                            if (trail.Equals((4, 2)) && (directions.Contains(2) ||
                                                         directions.Contains(3) ||
                                                         directions.Contains(4)))
                                return true;
                            if (trail.Equals((0, 1)) && (directions.Contains(0) ||
                                                         directions.Contains(4) ||
                                                         directions.Contains(5) ||
                                                         directions.Contains(6) ||
                                                         directions.Contains(7)))
                                return true;
                            if (trail.Equals((4, 1)) && (directions.Contains(2) ||
                                                         directions.Contains(3) ||
                                                         directions.Contains(4) ||
                                                         directions.Contains(5) ||
                                                         directions.Contains(6)))
                                return true;
                            if (trail.Equals((0, 3)) && (directions.Contains(0) ||
                                                         directions.Contains(1) ||
                                                         directions.Contains(2) ||
                                                         directions.Contains(6) ||
                                                         directions.Contains(7)))
                                return true;
                            if (trail.Equals((4, 3)) && (directions.Contains(0) ||
                                                         directions.Contains(1) ||
                                                         directions.Contains(2) ||
                                                         directions.Contains(3) ||
                                                         directions.Contains(4)))
                                return true;
                            return false;
                        case 5:
                            if ((trail.Equals((1, 0)) ||
                                 trail.Equals((2, 0)) ||
                                 trail.Equals((3, 0))) && (directions.Contains(0) ||
                                                           directions.Contains(1) ||
                                                           directions.Contains(2)))
                                return true;
                            if ((trail.Equals((1, 4)) ||
                                 trail.Equals((2, 4)) ||
                                 trail.Equals((3, 4))) && (directions.Contains(4) ||
                                                           directions.Contains(5) ||
                                                           directions.Contains(6)))
                                return true;
                            if ((trail.Equals((0, 1)) ||
                                 trail.Equals((0, 2)) ||
                                 trail.Equals((0, 3))) && (directions.Contains(0) ||
                                                           directions.Contains(6) ||
                                                           directions.Contains(7)))
                                return true;
                            if ((trail.Equals((4, 1)) ||
                                 trail.Equals((4, 2)) ||
                                 trail.Equals((4, 3))) && (directions.Contains(2) ||
                                                           directions.Contains(3) ||
                                                           directions.Contains(4)))
                                return true;
                            if (trail.Equals((0, 0)) && (directions.Contains(1) ||
                                                         directions.Contains(2)))
                                return true;
                            if (trail.Equals((4, 0)) && (directions.Contains(0) ||
                                                         directions.Contains(1)))
                                return true;
                            if (trail.Equals((0, 4)) && (directions.Contains(4) ||
                                                         directions.Contains(5)))
                                return true;
                            if (trail.Equals((4, 4)) && (directions.Contains(5) ||
                                                         directions.Contains(6)))
                                return true;
                            return false;
                        case 6:
                            if ((trail.Equals((0, 0)) ||
                                 trail.Equals((1, 0)) ||
                                 trail.Equals((2, 0)) ||
                                 trail.Equals((3, 0)) ||
                                 trail.Equals((4, 0))) && (directions.Contains(0) ||
                                                           directions.Contains(1) ||
                                                           directions.Contains(2)))
                                return true;
                            if ((trail.Equals((0, 4)) ||
                                 trail.Equals((1, 4)) ||
                                 trail.Equals((2, 4)) ||
                                 trail.Equals((3, 4)) ||
                                 trail.Equals((4, 4))) && (directions.Contains(4) ||
                                                           directions.Contains(5) ||
                                                           directions.Contains(6)))
                                return true;
                            if ((trail.Equals((0, 1)) ||
                                 trail.Equals((0, 2)) ||
                                 trail.Equals((0, 3))) && (directions.Contains(0) ||
                                                           directions.Contains(6) ||
                                                           directions.Contains(7)))
                                return true;
                            if ((trail.Equals((4, 1)) ||
                                 trail.Equals((4, 2)) ||
                                 trail.Equals((4, 3))) && (directions.Contains(2) ||
                                                           directions.Contains(3) ||
                                                           directions.Contains(4)))
                                return true;
                            return false;
                        case 7:
                            if ((trail.Equals((1, 0)) ||
                                 trail.Equals((2, 0)) ||
                                 trail.Equals((3, 0))) && (directions.Contains(4) ||
                                                           directions.Contains(5) ||
                                                           directions.Contains(6)))
                                return true;
                            if ((trail.Equals((1, 4)) ||
                                 trail.Equals((2, 4)) ||
                                 trail.Equals((3, 4))) && (directions.Contains(0) ||
                                                           directions.Contains(1) ||
                                                           directions.Contains(2)))
                                return true;
                            if ((trail.Equals((0, 1)) ||
                                 trail.Equals((0, 2)) ||
                                 trail.Equals((0, 3))) && (directions.Contains(0) ||
                                                           directions.Contains(6) ||
                                                           directions.Contains(7)))
                                return true;
                            if ((trail.Equals((4, 1)) ||
                                 trail.Equals((4, 2)) ||
                                 trail.Equals((4, 3))) && (directions.Contains(2) ||
                                                           directions.Contains(3) ||
                                                           directions.Contains(4)))
                                return true;
                            if (trail.Equals((0, 0)) && (directions.Contains(0) ||
                                                         directions.Contains(4) ||
                                                         directions.Contains(5) ||
                                                         directions.Contains(6) ||
                                                         directions.Contains(7)))
                                return true;
                            if (trail.Equals((4, 0)) && (directions.Contains(2) ||
                                                         directions.Contains(3) ||
                                                         directions.Contains(4) ||
                                                         directions.Contains(5) ||
                                                         directions.Contains(6)))
                                return true;
                            if (trail.Equals((0, 4)) && (directions.Contains(0) ||
                                                         directions.Contains(1) ||
                                                         directions.Contains(2) ||
                                                         directions.Contains(6) ||
                                                         directions.Contains(7)))
                                return true;
                            if (trail.Equals((4, 4)) && (directions.Contains(0) ||
                                                         directions.Contains(1) ||
                                                         directions.Contains(2) ||
                                                         directions.Contains(3) ||
                                                         directions.Contains(4)))
                                return true;
                            return false;
                        default:
                            Debug.Log("Not contemplated location shape: " + width + "x" + height);
                            return false;
                    }
                default:
                    Debug.Log("Not contemplated location shape: " + width + "x" + height);
                    return false;
            }            
            return false;
        }

        protected byte GetTrailShape((int, int) trail, List<(int, int)> trailToAdd)
        {
            int index = trailToAdd.FindIndex(x => x.Item1 == trail.Item1 && x.Item2 == trail.Item2);
            (byte, byte) exits = (0, 0);
            byte exit = 0;
            int xDir;
            int yDir;

            // Setting enter/exit for starting/final trail tile
            if (index == 0 || index == trailToAdd.Count - 1)
            {
                // Debug.Log("index: " + index);
                if (trail.Item1 == 0)
                {
                    if (trail.Item2 == 0)
                        exit = NW;
                    else if (trail.Item2 == 4)
                        exit = SW;
                    else exit = W;
                }                    
                else if (trail.Item1 == 4)
                {
                    if (trail.Item2 == 0)
                        exit = NE;
                    else if (trail.Item2 == 4)
                        exit = SE;
                    else exit = E;
                }
                else if (trail.Item2 == 0)
                    exit = N;
                else if (trail.Item2 == 4)
                    exit = S;

                if (index == 0)
                    exits.Item1 = exit;
                else exits.Item2 = exit;
            }

            // Setting enter
            if (index != 0)
            {
                xDir = trail.Item1 - trailToAdd[index - 1].Item1;
                yDir = trail.Item2 - trailToAdd[index - 1].Item2;

                if (xDir < 0)
                {
                    if (yDir < 0)
                        exits.Item1 = SE;
                    else if (yDir > 0)
                        exits.Item1 = NE;
                    else exits.Item1 = E;
                }
                else if (xDir > 0)
                {
                    if (yDir < 0)
                        exits.Item1 = SW;
                    else if (yDir > 0)
                        exits.Item1 = NW;
                    else exits.Item1 = W;
                }
                else{
                    if (yDir < 0)
                        exits.Item1 = S;
                    else
                        exits.Item1 = N;
                }
            }

            // Setting exit
            if (index < trailToAdd.Count - 1)
            {
                xDir = trail.Item1 - trailToAdd[index + 1].Item1;
                yDir = trail.Item2 - trailToAdd[index + 1].Item2;

                if (xDir < 0)
                {
                    if (yDir < 0)
                        exits.Item2 = SE;
                    else if (yDir > 0)
                        exits.Item2 = NE;
                    else exits.Item2 = E;
                }
                else if (xDir > 0)
                {
                    if (yDir < 0)
                        exits.Item2 = SW;
                    else if (yDir > 0)
                        exits.Item2 = NW;
                    else exits.Item2 = W;
                }
                else{
                    if (yDir < 0)
                        exits.Item2 = S;
                    else
                        exits.Item2 = N;
                }
            }
            // Debug.Log("Returnin trailShape: " + (exits.Item1 + exits.Item2));
            return (byte)(exits.Item1 + exits.Item2);
        }

        protected (byte, byte, byte, byte)[,] GenerateTrailMap(List<DFPosition> trail, (byte, byte, byte, byte)[,] trailMap, TrailTypes trailPreference, int strtIndex, int arrvIndex)
        {
            int counter = 0;
            byte trailToPlace1 = 0;
            byte trailToPlace2 = 0;
            byte resultingTrailToPlace = 0;
            // byte[,] trailResult = new byte[MapsFile.MaxMapPixelX, MapsFile.MaxMapPixelY];

            foreach (DFPosition trailSection in trail)
            {
                int xDir, yDir;
                xDir = yDir = 0;
                DFPosition trailSectionConv = ConvertToRelative(trailSection);

                switch (trailPreference)
                {
                    case TrailTypes.Road:
                        resultingTrailToPlace = trailMap[trailSectionConv.X, trailSectionConv.Y].Item1;
                        break;
                    case TrailTypes.DirtRoad:
                        resultingTrailToPlace = trailMap[trailSectionConv.X, trailSectionConv.Y].Item2;
                        break;
                    case TrailTypes.Track:
                        resultingTrailToPlace = trailMap[trailSectionConv.X, trailSectionConv.Y].Item3;
                        break;
                }

                if (counter > 0)
                {
                    xDir = trailSection.X - trail[counter - 1].X;
                    yDir = trailSection.Y - trail[counter - 1].Y;

                    trailToPlace1 = GetTrailToPlace(xDir, yDir);
                    resultingTrailToPlace = OverwriteTrail(trailToPlace1, resultingTrailToPlace);
                }

                if (counter < (trail.Count - 1))
                {
                    xDir = trailSection.X - trail[counter + 1].X;
                    yDir = trailSection.Y - trail[counter + 1].Y;

                    trailToPlace2 = GetTrailToPlace(xDir, yDir);
                    resultingTrailToPlace = OverwriteTrail(trailToPlace2, resultingTrailToPlace);
                }

                int mapId = MapEditor.GetMapPixelIDFromPosition(trailSection);

                // byte direction = GetTrailToPlace(xDir, yDir);
                GetIndexFromByte(trailToPlace1, out int index1);
                GetIndexFromByte(trailToPlace2, out int index2);
                // Debug.Log("xDir: " + xDir + ", yDir: " + yDir + ", direction: " + direction + ", result.Item1: " + result.Item1 + ", index1: " + index1 + ", index2: " + index2);
                // Debug.Log("signData.locNames.Count: " + signData.locNames.Count + ", strtIndex: " + strtIndex + ", arrvIndex: " + arrvIndex);
                string[][] roadsign = new string[8][];

                if (!signData.roadsign.ContainsKey(mapId))
                {
                    roadsign[index1] = new string[] { signData.locNames[strtIndex] };
                    roadsign[index2] = new string[] { signData.locNames[arrvIndex] };
                    signData.roadsign.Add(mapId, roadsign);
                }
                else
                {
                    for (int i = 7; i >= 0; i--)
                    {
                        if (signData.roadsign[mapId][i] == null)
                        {
                            if (i == index1)
                            {
                                signData.roadsign[mapId][i] = new string[] { signData.locNames[strtIndex] };
                            }
                            if (i == index2)
                            {
                                signData.roadsign[mapId][i] = new string[] { signData.locNames[arrvIndex] };
                            }
                        }
                        else
                        {
                            if (i == index1 || i == index2)
                            {
                                if (i == index1 && !signData.roadsign[mapId][i].Contains(signData.locNames[strtIndex]))
                                {
                                    roadsign[i] = new string[signData.roadsign[mapId].Length + 1];
                                    roadsign[i] = (signData.roadsign[mapId][i].Concat(new string[] { signData.locNames[strtIndex] })).ToArray<string>();
                                }
                                if (i == index2 && !signData.roadsign[mapId][i].Contains(signData.locNames[arrvIndex]))
                                {
                                    roadsign[i] = new string[signData.roadsign[mapId].Length + 1];
                                    roadsign[i] = (signData.roadsign[mapId][i].Concat(new string[] { signData.locNames[arrvIndex] })).ToArray<string>();
                                }
                                signData.roadsign[mapId][i] = roadsign[i];
                            }
                        }
                    }
                }

                switch (trailPreference)
                {
                    case TrailTypes.Road:
                        trailMap[trailSectionConv.X, trailSectionConv.Y].Item1 = resultingTrailToPlace;                    
                        trailMap[trailSectionConv.X, trailSectionConv.Y] = MergeTrail(resultingTrailToPlace, trailMap[trailSectionConv.X, trailSectionConv.Y].Item2, trailMap[trailSectionConv.X, trailSectionConv.Y].Item3, mapId, strtIndex, arrvIndex, xDir, yDir);
                        break;
                    case TrailTypes.DirtRoad:
                        trailMap[trailSectionConv.X, trailSectionConv.Y].Item2 = resultingTrailToPlace;                    
                        trailMap[trailSectionConv.X, trailSectionConv.Y] = MergeTrail(trailMap[trailSectionConv.X, trailSectionConv.Y].Item1, resultingTrailToPlace, trailMap[trailSectionConv.X, trailSectionConv.Y].Item3, mapId, strtIndex, arrvIndex, xDir, yDir);
                        break;
                    case TrailTypes.Track:
                        trailMap[trailSectionConv.X, trailSectionConv.Y].Item3 = resultingTrailToPlace;                    
                        trailMap[trailSectionConv.X, trailSectionConv.Y] = MergeTrail(trailMap[trailSectionConv.X, trailSectionConv.Y].Item1, trailMap[trailSectionConv.X, trailSectionConv.Y].Item2, resultingTrailToPlace, mapId, strtIndex, arrvIndex, xDir, yDir);
                        break;
                }
                counter++;
            }

            return trailMap;
        }

        protected byte OverwriteTrail(byte trail1, byte trail2)
        {
            int baseValue = 2;
            int pow = 7;
            byte resultTrail = 0;

            do{
                byte factor = (byte)Math.Pow(baseValue, pow);

                if (trail1 >= factor || trail2 >= factor)
                    resultTrail += factor;

                if (trail1 >= factor) trail1 -= factor;
                if (trail2 >= factor) trail2 -= factor;
                
                pow--;
            }
            while (pow >= 0 && (trail1 >= 0 || trail2 >= 0));

            // Debug.Log("OverwriteTrail -> returning " + resultTrail);
            return resultTrail;
        }

        protected (byte, byte, byte, byte) MergeTrail(byte roadByte, byte dirtByte, byte trackByte, int mapID, int strtIndex, int arrvIndex, int xDir = 0, int yDir = 0)
        {
            (byte, byte, byte, byte) result = (0, 0, 0, 0);
            int baseValue = 2;
            int pow = 7;
            // result.Item3 = 0;
            byte roadWiP = roadByte;
            byte dirtWiP = dirtByte;
            byte trackWiP = trackByte;

            do{
                byte factor = (byte)Math.Pow(baseValue, pow);

                if (roadByte >= factor)
                    result.Item1 += factor;
                else if (dirtByte >= factor)
                    result.Item2 += factor;
                else if (trackByte >= factor)
                    result.Item3 += factor;

                if (roadByte >= factor) roadByte -= factor;
                if (dirtByte >= factor) dirtByte -= factor;
                if (trackByte >= factor) trackByte -= factor;

                pow--;
            }
            while (pow >= 0 && (roadByte >= 0 || trackByte >= 0));

            // byte direction = GetTrailToPlace(xDir, yDir);
            // GetIndexFromByte((byte)(result.Item1 - direction), out int index1);
            // GetIndexFromByte(result.Item1, out int index2);
            // Debug.Log("xDir: " + xDir + ", yDir: " + yDir + ", direction: " + direction + ", result.Item1: " + result.Item1 + ", index1: " + index1 + ", index2: " + index2);
            // Debug.Log("signData.locNames.Count: " + signData.locNames.Count + ", strtIndex: " + strtIndex + ", arrvIndex: " + arrvIndex);
            // if (!signData.roadsign.ContainsKey(mapID))
            // {
            //     string[][] roadsign = new string[8][];
               
            //     roadsign[index1] = new string[] { signData.locNames[strtIndex] };
            //     roadsign[index2] = new string[] { signData.locNames[arrvIndex] };
            //     signData.roadsign.Add(mapID, roadsign);
            // }
            // else
            // {
            //     string[][] roadsign = new string[8][];
            //     for (int i = 7; i >= 0; i--)
            //     {
            //         if (signData.roadsign[mapID][i] == null)
            //         {
            //             if (i == index1)
            //             {
            //                 signData.roadsign[mapID][i] = new string[] { signData.locNames[strtIndex] };
            //             }
            //             if (i == index2)
            //             {
            //                 signData.roadsign[mapID][i] = new string[] { signData.locNames[arrvIndex] };
            //             }
            //         }
            //         else
            //         {
            //             if (i == index1 || i == index2)
            //             {
            //                 if (i == index1 && !signData.roadsign[mapID][i].Contains(signData.locNames[strtIndex]))
            //                 {
            //                     roadsign[i] = new string[signData.roadsign[mapID].Length + 1];
            //                     roadsign[i] = (signData.roadsign[mapID][i].Concat(new string[] { signData.locNames[strtIndex] })).ToArray<string>();
            //                 }
            //                 if (i == index2 && !signData.roadsign[mapID][i].Contains(signData.locNames[arrvIndex]))
            //                 {
            //                     roadsign[i] = new string[signData.roadsign[mapID].Length + 1];
            //                     roadsign[i] = (signData.roadsign[mapID][i].Concat( new string[] { signData.locNames[arrvIndex] })).ToArray<string>();
            //                 }
            //                 signData.roadsign[mapID][i] = roadsign[i];
            //             }
            //         }
            //     }
            // }

            // Debug.Log("roadWiP: " + roadWiP + ", trackWiP: " + trackWiP);
            if (IsCrossRoad(roadWiP, dirtWiP, trackWiP, out byte CRshape))
            {
                result.Item4 = 25;
                int index = 0;
                List<int> directions = GetIndexesFromByte(CRshape);
                Debug.Log("mapID: " + mapID);

                for (int dir = 0; dir < 8; dir++)
                {
                    if (directions.Contains(dir))
                    {
                        if (signData.roadsign[mapID][dir] == null)
                        {
                            Debug.Log("Found null roadsign at mapID: " + mapID + ", direction: " + dir);
                            // if (dir == index1)
                            //     signData.roadsign[mapID][dir] = new string[] { signData.locNames[strtIndex] };
                            // else if (dir == index2)
                            //     signData.roadsign[mapID][dir] = new string[] { signData.locNames[arrvIndex] };
                        }
                    }                    
                    else{
                        signData.roadsign[mapID][dir] = null;
                        // if (dir == index1 || dir == index2)
                        // {
                        //     List<string> arraySum = signData.roadsign[mapID][dir].ToList();
                        //     if (dir == index1 && !signData.roadsign[mapID][dir].Contains(signData.locNames[strtIndex]))
                        //         arraySum.Add(signData.locNames[strtIndex]);
                        //     else if (dir == index2 && !signData.roadsign[mapID][dir].Contains(signData.locNames[arrvIndex]))
                        //         arraySum.Add(signData.locNames[arrvIndex]);
                        //     signData.roadsign[mapID][dir] = arraySum.ToArray();
                        // }
                    }
                }
            }
            if (Worldmaps.HasLocation(mapID / 7680, mapID % 7680, out MapSummary location))
            {
                result.Item3 = internalSign;
                int index = 0;
                List<int> directions = GetIndexesFromByte(CRshape);
                Debug.Log("mapID: " + mapID);

                for (int dir = 0; dir < 8; dir++)
                {
                    if (directions.Contains(dir))
                    {
                        if (signData.roadsign[mapID][dir] == null)
                        {
                            Debug.Log("Found null roadsign at mapID: " + mapID + ", direction: " + dir);
                            // if (dir == index1)
                            //     signData.roadsign[mapID][dir] = new string[] { signData.locNames[strtIndex] };
                            // else if (dir == index2)
                            //     signData.roadsign[mapID][dir] = new string[] { signData.locNames[arrvIndex] };
                        }
                    }                    
                    else{
                        signData.roadsign[mapID][dir] = null;
                        // if (dir == index1 || dir == index2)
                        // {
                        //     List<string> arraySum = signData.roadsign[mapID][dir].ToList();
                        //     if (dir == index1 && !signData.roadsign[mapID][dir].Contains(signData.locNames[strtIndex]))
                        //         arraySum.Add(signData.locNames[strtIndex]);
                        //     else if (dir == index2 && !signData.roadsign[mapID][dir].Contains(signData.locNames[arrvIndex]))
                        //         arraySum.Add(signData.locNames[arrvIndex]);
                        //     signData.roadsign[mapID][dir] = arraySum.ToArray();
                        // }
                    }
                }
            }
            // Debug.Log("MergeTrail -> returning " + result);
            return result;
        }

        protected static bool IsCrossRoad(byte road, byte dirt, byte track, out byte shape)
        {
            byte result = 0;
            shape = (byte)(road | dirt | track);
            Debug.Log("road: " + road + ", dirt: " + dirt + ", track: " + track + ", shape: " + shape);

            for (int shift = 7; shift >= 0; shift--)
            {
                if ((shape >> shift) % 2 != 0)
                {
                    result++;
                }
            }
            Debug.Log("result: " + result);

            if (result >= 3)
                return true;
            return false;
        }

        protected static bool IsCrossRoad(long compositeTrail)
        {
            byte result = 0;
            compositeTrail = compositeTrail % (FUNCINDEX);
            byte trailShape = (byte)(compositeTrail / TRACKINDEX + (compositeTrail % TRACKINDEX) / DIRTINDEX + compositeTrail % DIRTINDEX);

            Debug.Log("IsCrossroad = compositeTrail: " + compositeTrail + ", trailShape: " + trailShape);
            for (int shift = 7; shift >= 0; shift--)
            {
                if ((trailShape >> shift) % 2 != 0)
                {
                    result++;
                }
            }
            Debug.Log("result: " + result);

            if (result >= 3)
                return true;
            return false;
        }

        protected static bool IsCrossRoad(byte trailShape)
        {
            byte result = 0;

            // Debug.Log("trailShape: " + trailShape);
            for (int shift = 7; shift > 0; shift--)
            {
                if ((trailShape >> shift) % 2 != 0 )
                {
                    result++;
                }
            }
            // Debug.Log("result: " + result);

            if (result >= 3)
                return true;
            return false;
        }

        ///<summary>
        /// Get a 0-7 (SW - W) index from byte IF said byte is a single direction
        ///</summary>
        protected static bool GetIndexFromByte(byte direction, out int index)
        {
            int count = 0;
            int result = 0;
            index = 0;
            Debug.Log("Working on direction: " + direction);
            while (direction > 0 && count < 2)
            {
                if (direction % 2 > 0)
                {
                    index = result;
                    count++;
                }
                result++;
                direction >>= 1;
            }
            // Debug.Log("direction: " + direction + ", index: " + index + ", count: " + count);
            if (count > 1) return false;
            return true;
        }

        ///<summary>
        /// Get a list of 0-7 indexes from a byte (7 == west; 0 == southwest)
        ///</summary>
        protected static List<int> GetIndexesFromByte(byte shape)
        {
            byte result = 7;
            List<int> indexes = new List<int>();
            // Debug.Log("shape: " + shape);

            for (int shift = 7; shift > 0; shift--)
            {
                if ((shape >> shift) % 2 != 0)
                {
                    indexes.Add(result);
                    // Debug.Log("result: " + result);
                }
                result--;
            }
            return indexes;
        }

        protected byte GetTrailToPlace(int xDir, int yDir)
        {
            if (xDir > 0 && yDir == 0)
                return W;
            if (xDir > 0 && yDir > 0)
                return NW;
            if (xDir == 0 && yDir > 0)
                return N;
            if (xDir < 0 && yDir > 0)
                return NE;
            if (xDir < 0 && yDir == 0)
                return E;
            if (xDir < 0 && yDir < 0)
                return SE;
            if (xDir == 0 && yDir < 0)
                return S;
            if (xDir > 0 && yDir < 0)
                return SW;
            else{
                Debug.Log("Error in determining trail to place!");
                return 0;
            }
        }

        protected bool CheckDFPositionListContent(List<DFPosition> list, DFPosition content)
        {
            foreach (DFPosition element in list)
            {
                if (element.Equals(content))
                    return true;
            }

            return false;
        }

        protected RoutedLocation CompareLocDistance(RoutedLocation loc, out List<RoutedLocation> locListToSwap, ref List<RoutedLocation> locComp)
        {
            List<(ulong, float)> locDistanceSwap = loc.locDistance;
            List<ulong> locsToCheck = new List<ulong>();
            RoutedLocation locToCompare = new RoutedLocation();
            locListToSwap = new List<RoutedLocation>();

            foreach ((ulong, float) locDist in loc.locDistance)
            {
                // If the locDist object isn't in the same tile of the loc object, continue; why? Maybe this was outdated. Taking it out for the moment.
                // if (((int)locDist.Item1 % MapsFile.WorldWidth) / MapsFile.TileDim != xTile || ((int)locDist.Item1 / MapsFile.WorldWidth) / MapsFile.TileDim != yTile)
                //     continue;

                int counter = 0;
                if (locComp.Exists(z => z.position.Equals(MapsFile.GetPixelFromPixelID(locDist.Item1))))
                {
                    locsToCheck.Add(locDist.Item1);
                }
            }

            if (locsToCheck.Count > 0)
            {
                foreach (ulong locToCheck in locsToCheck)
                {
                    locToCompare = locComp.Find(x => x.position.Equals(MapsFile.GetPixelFromPixelID(locToCheck)));
                    List<ulong> locToDoubleCheck1 = new List<ulong>();
                    List<ulong> locToDoubleCheck2 = new List<ulong>();
                    
                    foreach ((ulong, float) lTC2 in locToCompare.locDistance)
                    {
                        locToDoubleCheck2.Add(lTC2.Item1);
                    }
                    foreach ((ulong, float) lTC1 in loc.locDistance)
                    {
                        locToDoubleCheck1.Add(lTC1.Item1);
                    }
                    List<ulong> lTCIntersect = locToDoubleCheck1.Intersect(locToDoubleCheck2).ToList();

                    foreach (ulong lTC in lTCIntersect)
                    {
                        int index1 = loc.locDistance.FindIndex(x1 => x1.Item1 == lTC);
                        int index2 = locToCompare.locDistance.FindIndex(x2 => x2.Item1 == lTC);

                        if (locToCompare.locDistance[index2].Item2 < loc.locDistance[index1].Item2)
                        {
                            loc.locDistance.RemoveAt(index1);
                        }
                        else if (locToCompare.locDistance[index2].Item2 > loc.locDistance[index1].Item2)
                        {
                            locToCompare.locDistance.RemoveAt(index2);
                            locListToSwap.Add(locToCompare);
                        }
                        else if (UnityEngine.Random.Range(0, 1) == 0)
                        {
                            loc.locDistance.RemoveAt(index1);
                        }
                        else
                        {
                            locToCompare.locDistance.RemoveAt(index2);
                            locListToSwap.Add(locToCompare);
                        }
                    }
                }
            }
            return loc;
        }

        protected List<RoutedLocation> MergeModifiedLocDist(List<RoutedLocation> compList, RoutedLocation loc)
        {
            int index = -1;

            if (compList.Exists(x => x.position == loc.position))
            {
                index = compList.FindIndex(y => y.position == loc.position);
                compList[index].locDistance = loc.locDistance;
            }
            return compList;
        }

        protected List<RoutedLocation> PruneDoubleTrails(List<RoutedLocation> compList)
        {
            for (int i = 0; i < compList.Count; i++)
            {
                for (int j = 0; j < compList[i].locDistance.Count; j++)
                {
                    DFPosition destPos = MapsFile.GetPixelFromPixelID(compList[i].locDistance[j].Item1);
                    if (compList.Exists(a => a.position.Equals(destPos)))
                    {
                        int index1 = compList.FindIndex(b => b.position.Equals(destPos));
                        if (compList[index1].locDistance.Exists(c => c.Item1 == MapsFile.GetMapPixelID(compList[i].position.X, compList[i].position.Y)))
                        {
                            int index2 = compList[index1].locDistance.FindIndex(d => d.Item1 == MapsFile.GetMapPixelID(compList[i].position.X, compList[i].position.Y));

                            if (compList[index1].trailPreference == TrailTypes.Track)
                            {
                                compList[i].locDistance.RemoveAt(j);
                            }
                            else
                            {
                                compList[index1].locDistance.RemoveAt(index2);
                            }
                        }
                    }
                }
            }

            // List<RoutedLocation> compListSwap = new List<RoutedLocation>();
            // compListSwap = compList;
            // foreach (RoutedLocation routedLoc in compList)
            // {
            //     RoutedLocation routedLocSwap = routedLoc;
            //     RoutedLocation locSwap = new RoutedLocation();
            //     foreach ((ulong, float) locDist in routedLoc.locDistance)
            //     {
            //         locSwap = compListSwap[compListSwap.FindIndex(a => a.position.Equals(MapsFile.GetPixelFromPixelID(locDist.Item1)))];
            //         if (locSwap.locDistance.Exists(b => b.Item1.Equals(MapsFile.GetMapPixelID(routedLocSwap.position.X, routedLocSwap.position.Y))))
            //         {
            //             int index = locSwap.locDistance.FindIndex(c => c.Item1.Equals(MapsFile.GetMapPixelID(routedLocSwap.position.X, routedLocSwap.position.Y)));
            //             if (routedLocSwap.trailPreference == TrailTypes.Road && locSwap.trailPreference == TrailTypes.Track)
            //             {
            //                 locSwap.locDistance.RemoveAt(index);
            //                 compListSwap[compListSwap.FindIndex(d => d.position.Equals(locSwap.position))] = locSwap;
            //             }
            //             else 
            //             // if (routedLoc.trailPreference == TrailTypes.Track && locSwap.trailPreference == TrailTypes.Road)
            //             {
            //                 int indexPr = routedLocSwap.locDistance.FindIndex(f => f.Item1.Equals(MapsFile.GetMapPixelID(locSwap.position.X, locSwap.position.Y)));
            //                 locSwap = routedLocSwap;
            //                 locSwap.locDistance.RemoveAt(indexPr);
            //                 compListSwap[compListSwap.FindIndex(e => e.position.Equals(locSwap.position))] = locSwap;
            //             }
            //         }
            //     }
            // }
            return compList;
        }

        protected void CalculateDirectionAndDiff(DFPosition currentPosition, DFPosition arrivalDestination, out int xDirection, out int yDirection, out int xDiffRes, out int yDiffRes)
        {
            int xDiff = currentPosition.X - arrivalDestination.X;
            int yDiff = currentPosition.Y - arrivalDestination.Y;

            if (xDiff < 0)
                xDirection = 1;
            else if (xDiff > 0)
                xDirection = -1;
            else xDirection = 0;
            xDiffRes = Math.Abs(xDiff);

            if (yDiff < 0)
                yDirection = 1;
            else if (yDiff > 0)
                yDirection = -1;
            else yDirection = 0;
            yDiffRes = Math.Abs(yDiff);
        }

        protected void CalculateDirectionAndDiff((int, int) currentPosition, (int, int) arrivalDestination, out int xDirection, out int yDirection)
        {
            int xDiff = currentPosition.Item1 - arrivalDestination.Item1;
            int yDiff = currentPosition.Item2 - arrivalDestination.Item2;

            if (xDiff < 0)
                xDirection = 1;
            else if (xDiff > 0)
                xDirection = -1;
            else xDirection = 0;

            if (yDiff < 0)
                yDirection = 1;
            else if (yDiff > 0)
                yDirection = -1;
            else yDirection = 0;
        }

        protected List<DFPosition> ProbeDirections(DFPosition currentPosition, int xDirection, int yDirection, byte startingHiehgt, List<DFPosition> deadEnds, List<DFPosition> trail, bool longTrail = false)
        {
            List<DFPosition> resultingChoice = new List<DFPosition>();
            byte rise_decline;
            DFPosition probing0, probing1, probing2, probing3, probing4;

            if (xDirection != 0 && yDirection != 0)
            {
                probing0 = new DFPosition(currentPosition.X + (xDirection), currentPosition.Y + (yDirection));
                rise_decline = (byte)Math.Abs(SmallHeightmap.GetHeightMapValue(currentPosition) - SmallHeightmap.GetHeightMapValue(probing0));
                if (rise_decline < riseDeclineLimit && SmallHeightmap.GetHeightMapValue(probing0) > 3 && !CheckDFPositionListContent(deadEnds, probing0) && !CheckDFPositionListContent(trail, probing0))
                    resultingChoice.Add(probing0);

                probing1 = new DFPosition(currentPosition.X + (0), currentPosition.Y + (yDirection));
                rise_decline = (byte)Math.Abs(SmallHeightmap.GetHeightMapValue(currentPosition) - SmallHeightmap.GetHeightMapValue(probing1));
                if (rise_decline < riseDeclineLimit && SmallHeightmap.GetHeightMapValue(probing1) > 3 && !CheckDFPositionListContent(deadEnds, probing1) && !CheckDFPositionListContent(trail, probing1))
                    resultingChoice.Add(probing1);

                probing2 = new DFPosition(currentPosition.X + (xDirection), currentPosition.Y + (0));
                rise_decline = (byte)Math.Abs(SmallHeightmap.GetHeightMapValue(currentPosition) - SmallHeightmap.GetHeightMapValue(probing2));
                if (rise_decline < riseDeclineLimit && SmallHeightmap.GetHeightMapValue(probing2) > 3 && !CheckDFPositionListContent(deadEnds, probing2) && !CheckDFPositionListContent(trail, probing2))
                    resultingChoice.Add(probing2);

                if (longTrail)
                {
                    probing3 = new DFPosition(currentPosition.X - (xDirection), currentPosition.Y + (yDirection));
                    rise_decline = (byte)Math.Abs(SmallHeightmap.GetHeightMapValue(currentPosition) - SmallHeightmap.GetHeightMapValue(probing3));
                    if (rise_decline < riseDeclineLimit && SmallHeightmap.GetHeightMapValue(probing3) > 3 && !CheckDFPositionListContent(deadEnds, probing3) && !CheckDFPositionListContent(trail, probing3))
                        resultingChoice.Add(probing3);

                    probing4 = new DFPosition(currentPosition.X + (xDirection), currentPosition.Y - (yDirection));
                    rise_decline = (byte)Math.Abs(SmallHeightmap.GetHeightMapValue(currentPosition) - SmallHeightmap.GetHeightMapValue(probing4));
                    if (rise_decline < riseDeclineLimit && SmallHeightmap.GetHeightMapValue(probing4) > 3 && !CheckDFPositionListContent(deadEnds, probing4) && !CheckDFPositionListContent(trail, probing4))
                        resultingChoice.Add(probing4);
                }
            }
            else if (xDirection == 0)
            {
                probing0 = new DFPosition(currentPosition.X + (xDirection), currentPosition.Y + (yDirection));
                rise_decline = (byte)Math.Abs(SmallHeightmap.GetHeightMapValue(currentPosition) - SmallHeightmap.GetHeightMapValue(probing0));
                if (rise_decline < riseDeclineLimit && SmallHeightmap.GetHeightMapValue(probing0) > 3 && !CheckDFPositionListContent(deadEnds, probing0) && !CheckDFPositionListContent(trail, probing0))
                    resultingChoice.Add(probing0);

                probing1 = new DFPosition(currentPosition.X + (-1), currentPosition.Y + (yDirection));
                rise_decline = (byte)Math.Abs(SmallHeightmap.GetHeightMapValue(currentPosition) - SmallHeightmap.GetHeightMapValue(probing1));
                if (rise_decline < riseDeclineLimit && SmallHeightmap.GetHeightMapValue(probing1) > 3 && !CheckDFPositionListContent(deadEnds, probing1) && !CheckDFPositionListContent(trail, probing1))
                    resultingChoice.Add(probing1);

                probing2 = new DFPosition(currentPosition.X + (+1), currentPosition.Y + (yDirection));
                rise_decline = (byte)Math.Abs(SmallHeightmap.GetHeightMapValue(currentPosition) - SmallHeightmap.GetHeightMapValue(probing2));
                if (rise_decline < riseDeclineLimit && SmallHeightmap.GetHeightMapValue(probing2) > 3 && !CheckDFPositionListContent(deadEnds, probing2) && !CheckDFPositionListContent(trail, probing2))
                    resultingChoice.Add(probing2);

                if (longTrail)
                {
                    probing3 = new DFPosition(currentPosition.X - (1), currentPosition.Y + (0));
                    rise_decline = (byte)Math.Abs(SmallHeightmap.GetHeightMapValue(currentPosition) - SmallHeightmap.GetHeightMapValue(probing3));
                    if (rise_decline < riseDeclineLimit && SmallHeightmap.GetHeightMapValue(probing3) > 3 && !CheckDFPositionListContent(deadEnds, probing3) && !CheckDFPositionListContent(trail, probing3))
                        resultingChoice.Add(probing3);

                    probing4 = new DFPosition(currentPosition.X + (1), currentPosition.Y + (0));
                    rise_decline = (byte)Math.Abs(SmallHeightmap.GetHeightMapValue(currentPosition) - SmallHeightmap.GetHeightMapValue(probing4));
                    if (rise_decline < riseDeclineLimit && SmallHeightmap.GetHeightMapValue(probing4) > 3 && !CheckDFPositionListContent(deadEnds, probing4) && !CheckDFPositionListContent(trail, probing4))
                        resultingChoice.Add(probing4);
                }
            }
            else if (yDirection == 0)
            {
                probing0 = new DFPosition(currentPosition.X + (xDirection), currentPosition.Y + (yDirection));
                rise_decline = (byte)Math.Abs(SmallHeightmap.GetHeightMapValue(currentPosition) - SmallHeightmap.GetHeightMapValue(probing0));
                if (rise_decline < riseDeclineLimit && SmallHeightmap.GetHeightMapValue(probing0) > 3 && !CheckDFPositionListContent(deadEnds, probing0) && !CheckDFPositionListContent(trail, probing0))
                    resultingChoice.Add(probing0);

                probing1 = new DFPosition(currentPosition.X + (xDirection), currentPosition.Y + (-1));
                rise_decline = (byte)Math.Abs(SmallHeightmap.GetHeightMapValue(currentPosition) - SmallHeightmap.GetHeightMapValue(probing1));
                if (rise_decline < riseDeclineLimit && SmallHeightmap.GetHeightMapValue(probing1) > 3 && !CheckDFPositionListContent(deadEnds, probing1) && !CheckDFPositionListContent(trail, probing1))
                    resultingChoice.Add(probing1);

                probing2 = new DFPosition(currentPosition.X + (xDirection), currentPosition.Y + (+1));
                rise_decline = (byte)Math.Abs(SmallHeightmap.GetHeightMapValue(currentPosition) - SmallHeightmap.GetHeightMapValue(probing2));
                if (rise_decline < riseDeclineLimit && SmallHeightmap.GetHeightMapValue(probing2) > 3 && !CheckDFPositionListContent(deadEnds, probing2) && !CheckDFPositionListContent(trail, probing2))
                    resultingChoice.Add(probing2);

                if (longTrail)
                {
                    probing3 = new DFPosition(currentPosition.X + (0), currentPosition.Y - (1));
                    rise_decline = (byte)Math.Abs(SmallHeightmap.GetHeightMapValue(currentPosition) - SmallHeightmap.GetHeightMapValue(probing3));
                    if (rise_decline < riseDeclineLimit && SmallHeightmap.GetHeightMapValue(probing3) > 3 && !CheckDFPositionListContent(deadEnds, probing3) && !CheckDFPositionListContent(trail, probing3))
                        resultingChoice.Add(probing3);

                    probing4 = new DFPosition(currentPosition.X + (0), currentPosition.Y + (1));
                    rise_decline = (byte)Math.Abs(SmallHeightmap.GetHeightMapValue(currentPosition) - SmallHeightmap.GetHeightMapValue(probing4));
                    if (rise_decline < riseDeclineLimit && SmallHeightmap.GetHeightMapValue(probing4) > 3 && !CheckDFPositionListContent(deadEnds, probing4) && !CheckDFPositionListContent(trail, probing4))
                        resultingChoice.Add(probing4);
                }
            }

            return resultingChoice;
        }

        protected List<(int, int)> ProbeDirections((int, int) start, int xDirection, int yDirection, byte[,] buffer)
        {
            List<(int, int)> resultingChoice = new List<(int, int)>();
            byte rise_decline;
            (int, int) probing0, probing1, probing2;
            bool checkProbing = false;

            if (xDirection != 0 && yDirection != 0)
            {
                probing0 = (start.Item1 + (xDirection), start.Item2 + (yDirection));
                if (CheckProbing(probing0))
                {
                    rise_decline = (byte)Math.Abs(buffer[start.Item1, start.Item2] - buffer[probing0.Item1, probing0.Item2]);
                    if (buffer[probing0.Item1, probing0.Item2] > 3)
                        resultingChoice.Add(probing0);
                }

                probing1 = (start.Item1 + (0), start.Item2 + (yDirection));
                if (CheckProbing(probing1))
                {
                    rise_decline = (byte)Math.Abs(buffer[start.Item1, start.Item2] - buffer[probing1.Item1, probing1.Item2]);
                    if (buffer[probing1.Item1, probing1.Item2] > 3)
                        resultingChoice.Add(probing1);
                }

                probing2 = (start.Item1 + (xDirection), start.Item2 + (0));
                if (CheckProbing(probing2))
                {
                    rise_decline = (byte)Math.Abs(buffer[start.Item1, start.Item2] - buffer[probing2.Item1, probing2.Item2]);
                    if (buffer[probing2.Item1, probing2.Item2] > 3)
                        resultingChoice.Add(probing2);
                }
            }
            else if (xDirection == 0)
            {
                probing0 = (start.Item1 + (xDirection), start.Item2 + (yDirection));
                if (CheckProbing(probing0))
                {
                    rise_decline = (byte)Math.Abs(buffer[start.Item1, start.Item2] - buffer[probing0.Item1, probing0.Item2]);
                    if (buffer[probing0.Item1, probing0.Item2] > 3)
                        resultingChoice.Add(probing0);
                }

                probing1 = (start.Item1 + (-1), start.Item2 + (yDirection));
                if (CheckProbing(probing1))
                {
                    rise_decline = (byte)Math.Abs(buffer[start.Item1, start.Item2] - buffer[probing1.Item1, probing1.Item2]);
                    if (buffer[probing1.Item1, probing1.Item2] > 3)
                        resultingChoice.Add(probing1);
                }

                probing2 = (start.Item1 + (+1), start.Item2 + (yDirection));
                if (CheckProbing(probing2))
                {
                    rise_decline = (byte)Math.Abs(buffer[start.Item1, start.Item2] - buffer[probing2.Item1, probing2.Item2]);
                    if (buffer[probing2.Item1, probing2.Item2] > 3)
                        resultingChoice.Add(probing2);
                }
            }
            else if (yDirection == 0)
            {
                probing0 = (start.Item1 + (xDirection), start.Item2 + (yDirection));
                if (CheckProbing(probing0))
                {
                    rise_decline = (byte)Math.Abs(buffer[start.Item1, start.Item2] - buffer[probing0.Item1, probing0.Item2]);
                    if (buffer[probing0.Item1, probing0.Item2] > 3)
                        resultingChoice.Add(probing0);
                }

                probing1 = (start.Item1 + (xDirection), start.Item2 + (-1));
                if (CheckProbing(probing1))
                {
                    rise_decline = (byte)Math.Abs(buffer[start.Item1, start.Item2] - buffer[probing1.Item1, probing1.Item2]);
                    if (buffer[probing1.Item1, probing1.Item2] > 3)
                        resultingChoice.Add(probing1);
                }

                probing2 = (start.Item1 + (xDirection), start.Item2 + (+1));
                if (CheckProbing(probing2))
                {
                    rise_decline = (byte)Math.Abs(buffer[start.Item1, start.Item2] - buffer[probing2.Item1, probing2.Item2]);
                    if (buffer[probing2.Item1, probing2.Item2] > 3)
                        resultingChoice.Add(probing2);
                }
            }

            return resultingChoice;
        }

        protected bool CheckProbing((int, int) probe)
        {
            if (probe.Item1 < 0 || probe.Item1 >= 5 || probe.Item2 < 0 || probe.Item2 >= 5)
                return false;

            return true;
        }

        protected RoutedLocation CircularWaveScan(RoutedLocation location, int waveSize, ref List<RoutedLocation>[] routedLocations, List<DFPosition> destinations)
        {
            int xDir, yDir;
            (int, int, int, int)[] modifiers = { (0, -1, 1, 1), (1, 0, -1, 1), (0, 1, -1, -1), (-1, 0, 1, -1) };
            MapSummary locationFound;

            for (int i = 0; i < waveSize * 4; i++)
            {
                int quadrant = i / waveSize;
                int sector = i % waveSize;

                xDir = modifiers[quadrant].Item1 * waveSize + sector * modifiers[quadrant].Item3;
                yDir = modifiers[quadrant].Item2 * waveSize + sector * modifiers[quadrant].Item4;
                DFPosition positionToCheck = new DFPosition(location.position.X + xDir, location.position.Y + yDir);

                if (CheckDFPositionListContent(destinations, positionToCheck))
                {
                    // Debug.Log("locationFound.ID: " + locationFound.ID + "; x: " + (MapsFile.GetPixelFromPixelID(locationFound.ID)).X + ", y: " + (MapsFile.GetPixelFromPixelID(locationFound.ID)).Y);
                    // if (routedLocations[(int)locationFound.LocationType].Exists( x => x.position.Equals(MapsFile.GetPixelFromPixelID(locationFound.ID))))
                    // {
                    // Debug.Log("locationFound.ID (" + locationFound.ID + ") exists!");
                    location.locDistance.Add((MapsFile.GetMapPixelID(positionToCheck.X, positionToCheck.Y), GenerateLocationsWindow.CalculateDistance(location.position, positionToCheck)));
                    location.completionLevel++;
                    // }
                }
            }
            Debug.Log("location.locDistance.Count: " + location.locDistance.Count);

            return location;
        }

        protected bool CheckDungeonRoutability(DFRegion.DungeonTypes dungeon)
        {
            if (dungeon == DFRegion.DungeonTypes.BarbarianStronghold ||
                dungeon == DFRegion.DungeonTypes.HumanStronghold ||
                dungeon == DFRegion.DungeonTypes.Laboratory ||
                dungeon == DFRegion.DungeonTypes.OrcStronghold ||
                dungeon == DFRegion.DungeonTypes.Prison ||
                dungeon == DFRegion.DungeonTypes.VampireHaunt)
                return true;

            else if (dungeon == DFRegion.DungeonTypes.Cemetery ||
                     dungeon == DFRegion.DungeonTypes.GiantStronghold ||
                     dungeon == DFRegion.DungeonTypes.Mine)
            {
                if (UnityEngine.Random.Range(0, 10) > 4)
                    return true;
            }

            else if (dungeon == DFRegion.DungeonTypes.DesecratedTemple ||
                     dungeon == DFRegion.DungeonTypes.Crypt ||
                     dungeon == DFRegion.DungeonTypes.RuinedCastle)
            {
                if (UnityEngine.Random.Range(0, 10) > 6)
                    return true;
            }
            return false;
        }
    }
}