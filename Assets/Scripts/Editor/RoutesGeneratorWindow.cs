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

        const byte riseDeclineLimit = 4;
        const int routeCompletionLevelRequirement = 4;
        const int minimumWaveScan = 10;
        const int maximumWaveScan = 50;
        public int xCoord, yCoord, xTile, yTile;

        // routeGenerationOrder set the order in which trails are generated
        public static int[] routeGenerationOrder = { 0, 1, 2, 6, 8, 5, 3, 11, 12, 9, 4, 7, 10};

        public class RoutedLocation
        {
            public DFPosition position;
            public List<(ulong, float)> locDistance;
            public TrailTypes trailPreference;
            public byte completionLevel;

            public RoutedLocation()
            {
                this.position = new DFPosition(0, 0);
                this.locDistance = new List<(ulong, float)>();
                this.trailPreference = TrailTypes.None;
                this.completionLevel = 0;
            }
        }

        public enum TrailTypes
        {
            None,
            Road = 1,
            Track = 2
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
            List<RoutedLocation>[] routedLocations = new List<RoutedLocation>[Enum.GetNames(typeof(DFRegion.LocationTypes)).Length];
            // for (int h = 0; h < Enum.GetNames(typeof(DFRegion.LocationTypes)).Length; h++)
            // {
            //     routedLocations[h] = new List<RoutedLocation>();
            // }
            List<DFPosition> routedLocSurroundings = SetLocSurroundings(out routedLocations);
            byte[] pathData = ClimateInfo.ConvertToArray(ClimateInfo.GetTextureMatrix(xTile, yTile, "trail"));
            
            List<RoutedLocation> routedLocationsComplete = new List<RoutedLocation>();
            (byte, byte)[,] trailMap = new (byte, byte)[MapsFile.MaxMapPixelX, MapsFile.MaxMapPixelY];

            // First we set values for routedLocations, to have a reference for which locations must be routed and in which way

            for (int j = 0; j < routedLocations.Length; j++)
            {
                Debug.Log("routedLocations.Length: " + routedLocations.Length);
                if (j == (int)DFRegion.LocationTypes.Coven ||
                    j == (int)DFRegion.LocationTypes.HiddenLocation ||
                    j == (int)DFRegion.LocationTypes.HomeYourShips ||
                    j == (int)DFRegion.LocationTypes.None)
                    continue;

                for (int k = 0; k < routedLocations[j].Count; k++)
                {
                    // if ((int)Worldmaps.Worldmap[RegionManager.currentRegionIndex].Locations[j].MapTableData.LocationType != i)
                    //     continue;

                    // for (int i = 0; i < Enum.GetNames(typeof(DFRegion.LocationTypes)).Length; i++)

                    // RoutedLocation location = new RoutedLocation();
                    // location.position = MapsFile.GetPixelFromPixelID(Worldmaps.Worldmap[RegionManager.currentRegionIndex].Locations[j].MapTableData.MapId);
                    // int locationType = (int)Worldmaps.Worldmap[RegionManager.currentRegionIndex].Locations[j].MapTableData.LocationType;
                    Debug.Log("routedLocations[" + (DFRegion.LocationTypes)j +  "].Count: " + routedLocations[j].Count);

                    switch (j)
                    {
                        // Locations that will always have a road
                        case (int)DFRegion.LocationTypes.TownCity:
                        case (int)DFRegion.LocationTypes.TownHamlet:
                        case (int)DFRegion.LocationTypes.TownVillage:
                        case (int)DFRegion.LocationTypes.Tavern:
                            routedLocations[j][k].trailPreference = TrailTypes.Road;
                            break;

                        // Locations that will always have a track
                        case (int)DFRegion.LocationTypes.HomeFarms:
                            routedLocations[j][k].trailPreference = TrailTypes.Track;
                            break;

                        // Locations that will have a road in most cases
                        case (int)DFRegion.LocationTypes.HomeWealthy:
                            if (UnityEngine.Random.Range(0, 10) > 2)
                                routedLocations[j][k].trailPreference = TrailTypes.Road;
                            else routedLocations[j][k].trailPreference = TrailTypes.Track;
                            break;

                        // Locations that have the same chances to have a road or a track
                        case (int)DFRegion.LocationTypes.ReligionTemple:
                            if (UnityEngine.Random.Range(0, 10) > 4)
                                routedLocations[j][k].trailPreference = TrailTypes.Road;
                            else routedLocations[j][k].trailPreference = TrailTypes.Track;
                            break;

                        // Locations that will have a track in most cases
                        case (int)DFRegion.LocationTypes.Graveyard:
                            if (UnityEngine.Random.Range(0, 10) > 7)
                                routedLocations[j][k].trailPreference = TrailTypes.Road;
                            else routedLocations[j][k].trailPreference = TrailTypes.Track;
                            break;

                        // Locations that have the same chances to have a track or no trail at all
                        case (int)DFRegion.LocationTypes.ReligionCult:
                        case (int)DFRegion.LocationTypes.HomePoor:
                            if (UnityEngine.Random.Range(0, 10) > 4)
                            {
                                routedLocations[j][k].trailPreference = TrailTypes.Track;
                            }
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
                                        routedLocations[j][k].trailPreference = TrailTypes.Road;
                                        break;

                                    case DFRegion.DungeonTypes.HumanStronghold:
                                    case DFRegion.DungeonTypes.Laboratory:
                                        if (UnityEngine.Random.Range(0, 10) > 4)
                                        {
                                            routedLocations[j][k].trailPreference = TrailTypes.Road;
                                        }
                                        else routedLocations[j][k].trailPreference = TrailTypes.Track;
                                        break;

                                    case DFRegion.DungeonTypes.BarbarianStronghold:
                                    // case DFRegion.DungeonTypes.Cemetery:
                                    case DFRegion.DungeonTypes.DesecratedTemple:
                                    case DFRegion.DungeonTypes.GiantStronghold:
                                    case DFRegion.DungeonTypes.RuinedCastle:
                                        routedLocations[j][k].trailPreference = TrailTypes.Track;
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
                Debug.Log("routedLocations[" + routeGenerationOrder[r] + "].Count: " + routedLocations[routeGenerationOrder[r]].Count);
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
                    Debug.Log("swapLoc.locDistance[0].Item1: " + swapLoc.locDistance[0].Item1);
                    swapList.Add(swapLoc);
                    Debug.Log("swapList[0].locDistance[0].Item1: " + swapList[0].locDistance[0].Item1);
                }

                // routedLocations[routeGenerationOrder[r]] = new List<RoutedLocation>();
                // routedLocations[routeGenerationOrder[r]] = swapList;
                routedLocationsComplete.AddRange(swapList);
                Debug.Log("routedLocationsComplete[0].locDistance[0].Item1: " + routedLocationsComplete[0].locDistance[0].Item1);
            }

            // Second, we compare different routed locations. Let's consider locations A, B and C: if A has to be connected with
            // both B and C, and B has to be connected to C too, and B is (significantly?) closer to C than A is,
            // we remove A connection to C.
            List<RoutedLocation> routedLocCompSwap = routedLocationsComplete;
            Debug.Log("routedLocCompSwap[0].locDistance[0].Item1: " + routedLocCompSwap[0].locDistance[0].Item1);
            RoutedLocation locToRoute0Swap = new RoutedLocation();

            foreach (RoutedLocation locToRoute0 in routedLocationsComplete)
            {
                // int index = 0;
                RoutedLocation locToRouteCompare = new RoutedLocation();
                List<(ulong, float)> locDistanceToCompare = new List<(ulong, float)>();
                locToRoute0Swap = locToRoute0;

                locToRoute0Swap = CompareLocDistance(locToRoute0, out locToRouteCompare, ref routedLocCompSwap);

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
                routedLocCompSwap = MergeModifiedLocDist(routedLocCompSwap, locToRouteCompare);

                // Debug.Log("routedLocCompSwap.Count: " + routedLocCompSwap.Count);
                // Debug.Log("routedLocationsComplete.Count: " + routedLocationsComplete.Count);
            }

            routedLocationsComplete = routedLocCompSwap;

            // Third, we search for the best way to connect two locations, starting from the shortest route
            // and falling back to longer trails if adjacent pixels have a too steep rise/decline
            foreach (RoutedLocation locToRoute1 in routedLocationsComplete)
            {
                foreach ((ulong, float) destination in locToRoute1.locDistance)
                {
                    Debug.Log("locToRoute1.locDistance.Count: " + locToRoute1.locDistance.Count);
                    List<DFPosition> trail = new List<DFPosition>();
                    trail = LoadExistingTrails(pathData);
                    List<DFPosition> movementChoice = new List<DFPosition>();
                    int xDirection, yDirection, xDiff, yDiff;
                    DFPosition currentPosition = locToRoute1.position;
                    DFPosition arrivalDestination = MapsFile.GetPixelFromPixelID(destination.Item1);

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
                                if (trailMap[checkJunction.X, checkJunction.Y].Item1 > 0 || trailMap[checkJunction.X, checkJunction.Y].Item2 > 0)
                                {
                                    Debug.Log("Junction present at " + checkJunction.X + ", " + checkJunction.Y);
                                    junctionPresent = true;
                                }
                            }
                            if (junctionPresent)
                            {
                                foreach (DFPosition removeNonJunction in movementChoice)
                                {
                                    if (trailMap[removeNonJunction.X, removeNonJunction.Y].Item1 == 0 && !(CheckDFPositionListContent(stepToRemove, removeNonJunction)) &&
                                        trailMap[removeNonJunction.X, removeNonJunction.Y].Item2 == 0 && !(CheckDFPositionListContent(stepToRemove, removeNonJunction)))
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

                    trailMap = GenerateTrailMap(trail, trailMap, locToRoute1.trailPreference);
                }
            }

            string fileDataPath;
            byte[] trailsByteArray;

            for (int ipsilon = -1; ipsilon < 2; ipsilon++)
            {
                for (int ics = -1; ics < 2; ics++)
                {
                    (byte, byte)[,] subTile = GetSubTile(trailMap, xTile + ics, yTile + ipsilon);
                    fileDataPath = Path.Combine(MapEditor.arena2Path, "Maps", "Tiles", "trail_" + (xTile + ics) + "_" + (yTile + ipsilon) + ".png");
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
            int[,] tile = new int[MapsFile.TileDim * 5, MapsFile.TileDim * 5];
            List<(int, int)> trailTilesList = new List<(int, int)>();

            for (int x = 0; x < MapsFile.MaxMapPixelX; x++)
            {
                for (int y = 0; y < MapsFile.MaxMapPixelY; y++)
                {
                    if (trailMap[x, y].Item1 > 0 || trailMap[x, y].Item2 > 0)
                    {
                        tileX = x / MapsFile.TileDim;
                        tileY = y / MapsFile.TileDim;

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
                trailsByteArray = MapEditor.ConvertToGrayscale(tile, true);
                File.WriteAllBytes(fileDataPath, trailsByteArray);
            }
        }

        protected (byte, byte)[,] GetSubTile((byte, byte)[,] trailMap, int tileX, int tileY)
        {
            (byte, byte)[,] subTile = new (byte, byte)[MapsFile.TileDim, MapsFile.TileDim];
            int X, Y;
            
            for (int y = 0; y < MapsFile.TileDim; y++)
            {
                for (int x = 0; x < MapsFile.TileDim; x++)
                {
                    X = tileX * MapsFile.TileDim + x;
                    Y = tileY * MapsFile.TileDim + y;

                    subTile[x, y] = trailMap[X, Y];
                }
            }

            return subTile;
        }

        protected List<DFPosition> LoadExistingTrails(byte[] pathData)
        {
            List<DFPosition> existingTrails = new List<DFPosition>();
            int xCorrection, yCorrection;

            for (int i = 0; i < pathData.Length; i++)
            {
                if (pathData[i] != 0)
                {
                    xCorrection = (i % (MapsFile.TileDim * 3)) + ((xTile - 1) * MapsFile.TileDim);
                    yCorrection = (i / (MapsFile.TileDim * 3)) + ((yTile - 1) * MapsFile.TileDim);

                    existingTrails.Add(new DFPosition(xCorrection, yCorrection));
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
                            locSurroundings.Add(locPosition);

                            if (locPosition.X >= xMinMiddle && locPosition.X < xMaxMiddle && locPosition.Y >= yMinMiddle && locPosition.Y < yMaxMiddle)
                            {
                                location.position = locPosition;
                                location.locDistance = new List<(ulong, float)>();
                                location.trailPreference = TrailTypes.None;
                                location.completionLevel = 0;

                                locationToRoute[locationType].Add(location);
                            }
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
        protected int[,] GenerateTile((int, int) trailTile, (byte, byte)[,] trailMap)
        {
            Debug.Log("Generating trailTile " + trailTile.Item1 + ", " + trailTile.Item2);
            int[,] tile = new int[MapsFile.TileDim * 5, MapsFile.TileDim * 5];
            Texture2D tileImage = new Texture2D(1, 1);
            if (File.Exists(Path.Combine(MapEditor.arena2Path, "Maps", "Tiles", "trailExp_" + trailTile.Item1 + "_" + trailTile.Item2 + ".png")))
            {
                ImageConversion.LoadImage(tileImage, File.ReadAllBytes(Path.Combine(MapEditor.arena2Path, "Maps", "Tiles", "trailExp_" + trailTile.Item1 + "_" + trailTile.Item2 + ".png")));
                MapEditor.ConvertToMatrix(tileImage, out tile);
            }

            // MapSummary location;
            // PixelData pixelData;

            ImageConversion.LoadImage(tileImage, File.ReadAllBytes(Path.Combine(MapEditor.arena2Path, "Maps", "Tiles", "woodsLarge_" + trailTile.Item1 + "_" + trailTile.Item2 + ".png")));
            byte[,] heightmapTile = MapEditor.ConvertToMatrix(tileImage);

            for (int xRel = 0; xRel < MapsFile.TileDim; xRel++)
            {
                for (int yRel = 0; yRel < MapsFile.TileDim; yRel++)
                {
                    Debug.Log("Working on xRel: " + xRel + ", yRel: " + yRel);
                    bool hasLocation = Worldmaps.HasLocation(xRel + (trailTile.Item1 * MapsFile.TileDim), yRel + (trailTile.Item2 * MapsFile.TileDim));
                    
                    bool loadedBuffer = false;
                    byte[,] buffer = GetLargeHeightmapPixel(heightmapTile, xRel, yRel);
                    int[,] tileBuffer = new int[5, 5];

                    if (trailMap[xRel + (trailTile.Item1 * MapsFile.TileDim), yRel + (trailTile.Item2 * MapsFile.TileDim)].Item2 > 0)
                    {
                        tileBuffer = GetTileBuffer(trailMap[xRel + (trailTile.Item1 * MapsFile.TileDim), yRel + (trailTile.Item2 * MapsFile.TileDim)].Item2, (byte)TrailTypes.Track, buffer, tileBuffer, hasLocation);
                        loadedBuffer = true;
                    }

                    if (trailMap[xRel + (trailTile.Item1 * MapsFile.TileDim), yRel + (trailTile.Item2 * MapsFile.TileDim)].Item1 > 0)
                    {
                        tileBuffer = GetTileBuffer(trailMap[xRel + (trailTile.Item1 * MapsFile.TileDim), yRel + (trailTile.Item2 * MapsFile.TileDim)].Item1, (byte)TrailTypes.Road, buffer, tileBuffer, hasLocation);
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

        protected int[,] RefineTileBorder(int[,] tile)
        {
            int tileSize = MapsFile.TileDim * 5;
            for (int x = 0; x < tileSize; x++)
            {
                for (int y = 0; y < tileSize; y++)
                {
                    if (tile[x, y] > 0)
                    {
                        int xBorder = x % 5;
                        int yBorder = y % 5;
                        int x1, x2, x3, y1, y2, y3;
                        List<byte> road = GetByte(tile[x, y], true);
                        List<byte> track = GetByte(tile[x, y], false);
                        List<byte> otherRoad = new List<byte>();
                        List<byte> otherTrack = new List<byte>();
                        List<byte> otherTrail = new List<byte>();
                        bool isRoad = true;
                        List<byte> corresponding1, corresponding2, corresponding3;

                        if ((xBorder > 0 && xBorder < 4) && yBorder == 0 && y - 1 >= 0)
                        {
                            if (road.Contains(N) || track.Contains(N))
                            {
                                if (track.Contains(N))
                                {
                                    isRoad = false;
                                }
                                x1 = x - x % 5 + 1;
                                x2 = x - x % 5 + 2;
                                x3 = x - x % 5 + 3;
                                y1 = y - 1;
                                corresponding1 = GetByte(tile[x1, y1], isRoad);
                                corresponding2 = GetByte(tile[x2, y1], isRoad);
                                corresponding3 = GetByte(tile[x3, y1], isRoad);

                                if (xBorder == 1)
                                {
                                    if (corresponding2.Contains(S))
                                    {
                                        otherTrail = GetByte(tile[x + 1, y], isRoad);
                                        if (!corresponding1.Contains(S) && !otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, isRoad, N);
                                            tile[x2, y1] = AddByte(tile[x2, y1], SW, isRoad, S);
                                        }
                                        else if (!corresponding1.Contains(S) && otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, isRoad, N);
                                            tile[x2, y1] = AddByte(tile[x2, y1], SW, isRoad);
                                        }
                                        else if (corresponding1.Contains(S) && !otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, isRoad);
                                            tile[x2, y1] = AddByte(tile[x2, y1], SW, isRoad, S);
                                        }
                                    }
                                    else if (corresponding3.Contains(S))
                                    {
                                        otherTrail = GetByte(tile[x + 2, y], isRoad);
                                        if (!corresponding1.Contains(S) && !otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, isRoad, N);
                                            tile[x3, y1] = AddByte(tile[x3, y1], W, isRoad, S);
                                            tile[x2, y1] = AddByte(tile[x2, y1], SW, E, isRoad);
                                        }
                                        else if (!corresponding1.Contains(S) && otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, isRoad, N);
                                            tile[x3, y1] = AddByte(tile[x3, y1], W, isRoad);
                                            tile[x2, y1] = AddByte(tile[x2, y1], SW, E, isRoad);
                                        }
                                        else if (corresponding1.Contains(S) && !otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, isRoad);
                                            tile[x3, y1] = AddByte(tile[x3, y1], W, isRoad, S);
                                            tile[x2, y1] = AddByte(tile[x2, y1], SW, E, isRoad);
                                        }
                                    }
                                }
                                if (xBorder == 2)
                                {
                                    if (corresponding1.Contains(S))
                                    {
                                        otherTrail = GetByte(tile[x - 1, y], isRoad);
                                        if (!corresponding2.Contains(S) && !otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, isRoad, N);
                                            tile[x1, y1] = AddByte(tile[x1, y1], SE, isRoad, S);
                                        }
                                        else if (!corresponding2.Contains(S) && otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, isRoad, N);
                                            tile[x1, y1] = AddByte(tile[x1, y1], SE, isRoad);
                                        }
                                        else if (corresponding2.Contains(S) && !otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, isRoad);
                                            tile[x1, y1] = AddByte(tile[x1, y1], SE, isRoad, S);
                                        }
                                    }
                                    else if (corresponding3.Contains(S))
                                    {
                                        otherTrail = GetByte(tile[x + 1, y], isRoad);
                                        if (!corresponding2.Contains(S) && !otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, isRoad, N);
                                            tile[x3, y1] = AddByte(tile[x3, y1], SW, isRoad, S);
                                        }
                                        else if (!corresponding2.Contains(S) && otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, isRoad, N);
                                            tile[x3, y1] = AddByte(tile[x3, y1], SW, isRoad);
                                        }
                                        else if (corresponding2.Contains(S) && !otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, isRoad);
                                            tile[x3, y1] = AddByte(tile[x3, y1], SW, isRoad, S);
                                        }
                                    }
                                }
                                if (xBorder == 3)
                                {
                                    if (corresponding2.Contains(S))
                                    {
                                        otherTrail = GetByte(tile[x - 1, y], isRoad);
                                        if (!corresponding3.Contains(S) && !otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, isRoad, N);
                                            tile[x2, y1] = AddByte(tile[x2, y1], SE, isRoad, S);
                                        }
                                        else if (!corresponding3.Contains(S) && otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, isRoad, N);
                                            tile[x2, y1] = AddByte(tile[x2, y1], SE, isRoad);
                                        }
                                        else if (corresponding3.Contains(S) && !otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, isRoad);
                                            tile[x2, y1] = AddByte(tile[x2, y1], SE, isRoad, S);
                                        }
                                    }
                                    else if (corresponding1.Contains(S))
                                    {
                                        otherTrail = GetByte(tile[x - 2, y], isRoad);
                                        if (!corresponding3.Contains(S) && !otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, isRoad, N);
                                            tile[x1, y1] = AddByte(tile[x1, y1], E, isRoad, S);
                                            tile[x2, y1] = AddByte(tile[x2, y1], W, SE, isRoad);
                                        }
                                        else if (!corresponding3.Contains(S) && otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, isRoad, N);
                                            tile[x1, y1] = AddByte(tile[x1, y1], E, isRoad);
                                            tile[x2, y1] = AddByte(tile[x2, y1], W, SE, isRoad);
                                        }
                                        else if (corresponding3.Contains(S) && !otherTrail.Contains(N))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, isRoad);
                                            tile[x1, y1] = AddByte(tile[x1, y1], E, isRoad, S);
                                            tile[x2, y1] = AddByte(tile[x2, y1], W, SE, isRoad);
                                        }
                                    }
                                }
                            }
                        }
                        else if ((xBorder > 0 && xBorder < 4) && yBorder == 4 && y + 1 < tileSize)
                        {
                            if (road.Contains(S) || track.Contains(S))
                            {
                                if (track.Contains(S))
                                {
                                    isRoad = false;
                                }
                                x1 = x - x % 5 + 1;
                                x2 = x - x % 5 + 2;
                                x3 = x - x % 5 + 3;
                                y1 = y + 1;
                                corresponding1 = GetByte(tile[x1, y1], isRoad);
                                corresponding2 = GetByte(tile[x2, y1], isRoad);
                                corresponding3 = GetByte(tile[x3, y1], isRoad);

                                if (xBorder == 1)
                                {
                                    if (corresponding2.Contains(N))
                                    {
                                        // Checking tile[x + 1, y];
                                        otherTrail = GetByte(tile[x + 1, y], isRoad);
                                        if (!corresponding1.Contains(N) && !otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, isRoad, S);
                                            tile[x2, y1] = AddByte(tile[x2, y1], NW, isRoad, N);
                                        }
                                        else if (!corresponding1.Contains(N) && otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, isRoad, S);
                                            tile[x2, y1] = AddByte(tile[x2, y1], NW, isRoad);
                                        }
                                        else if (corresponding1.Contains(N) && !otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, isRoad);
                                            tile[x2, y1] = AddByte(tile[x2, y1], NW, isRoad, N);
                                        }
                                    }
                                    else if (corresponding3.Contains(N))
                                    {
                                        // Checking tile[x + 2, y];
                                        otherTrail = GetByte(tile[x + 2, y], isRoad);
                                        if (!corresponding1.Contains(N) && !otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, isRoad, S);
                                            tile[x3, y1] = AddByte(tile[x3, y1], W, isRoad, N);
                                            tile[x2, y1] = AddByte(tile[x2, y1], NW, E, isRoad);
                                        }
                                        else if (!corresponding1.Contains(N) && otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, isRoad, S);
                                            tile[x3, y1] = AddByte(tile[x3, y1], W, isRoad);
                                            tile[x2, y1] = AddByte(tile[x2, y1], NW, E, isRoad);
                                        }
                                        else if (corresponding1.Contains(N) && !otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, isRoad);
                                            tile[x3, y1] = AddByte(tile[x3, y1], W, isRoad, N);
                                            tile[x2, y1] = AddByte(tile[x2, y1], NW, E, isRoad);
                                        }
                                    }
                                }
                                if (xBorder == 2)
                                {
                                    if (corresponding1.Contains(N))
                                    {
                                        otherTrail = GetByte(tile[x - 1, y], isRoad);
                                        if (!corresponding2.Contains(N) && !otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, isRoad, S);
                                            tile[x1, y1] = AddByte(tile[x1, y1], NE, isRoad, N);
                                        }
                                        else if (!corresponding2.Contains(N) && otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, isRoad, S);
                                            tile[x1, y1] = AddByte(tile[x1, y1], NE, isRoad);
                                        }
                                        else if (corresponding2.Contains(N) && !otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, isRoad);
                                            tile[x1, y1] = AddByte(tile[x1, y1], NE, isRoad, N);
                                        }
                                    }
                                    else if (corresponding3.Contains(N))
                                    {
                                        otherTrail = GetByte(tile[x + 1, y], isRoad);
                                        if (!corresponding2.Contains(N) && !otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, isRoad, S);
                                            tile[x1, y1] = AddByte(tile[x1, y1], NW, isRoad, N);
                                        }
                                        else if (!corresponding2.Contains(N) && otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, isRoad, S);
                                            tile[x1, y1] = AddByte(tile[x1, y1], NW, isRoad);
                                        }
                                        else if (corresponding2.Contains(N) && !otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, isRoad);
                                            tile[x1, y1] = AddByte(tile[x1, y1], NW, isRoad, N);
                                        }
                                    }
                                }
                                if (xBorder == 3)
                                {
                                    if (corresponding2.Contains(S))
                                    {
                                        otherTrail = GetByte(tile[x - 1, y], isRoad);
                                        if (!corresponding3.Contains(N) && !otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, isRoad, N);
                                            tile[x2, y1] = AddByte(tile[x2, y1], SE, isRoad, S);
                                        }
                                        else if (!corresponding3.Contains(N) && otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, isRoad, N);
                                            tile[x2, y1] = AddByte(tile[x2, y1], SE, isRoad);
                                        }
                                        else if (corresponding3.Contains(N) && !otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, isRoad);
                                            tile[x2, y1] = AddByte(tile[x2, y1], SE, isRoad, S);
                                        }
                                    }
                                    else if (corresponding1.Contains(N))
                                    {
                                        otherTrail = GetByte(tile[x - 2, y], isRoad);
                                        if (!corresponding3.Contains(N) && !otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, isRoad, S);
                                            tile[x1, y1] = AddByte(tile[x1, y1], E, isRoad, N);
                                            tile[x2, y1] = AddByte(tile[x2, y1], W, NE, isRoad);
                                        }
                                        else if (!corresponding3.Contains(N) && otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, isRoad, S);
                                            tile[x1, y1] = AddByte(tile[x1, y1], E, isRoad);
                                            tile[x2, y1] = AddByte(tile[x2, y1], W, NE, isRoad);
                                        }
                                        else if (corresponding3.Contains(N) && !otherTrail.Contains(S))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, isRoad);
                                            tile[x1, y1] = AddByte(tile[x1, y1], E, isRoad, N);
                                            tile[x2, y1] = AddByte(tile[x2, y1], W, NE, isRoad);
                                        }
                                    }
                                }
                            }
                        }
                        else if ((yBorder > 0 && yBorder < 4) && xBorder == 0 && x - 1 >= 0)
                        {
                            if (road.Contains(W) || track.Contains(W))
                            {
                                if (track.Contains(W))
                                {
                                    isRoad = false;
                                }
                                x1 = x - 1;
                                y1 = y - y % 5 + 1;
                                y2 = y - y % 5 + 2;
                                y3 = y - y % 5 + 3;
                                corresponding1 = GetByte(tile[x1, y1], isRoad);
                                corresponding2 = GetByte(tile[x1, y2], isRoad);
                                corresponding3 = GetByte(tile[x1, y3], isRoad);

                                if (yBorder == 1)
                                {
                                    if (corresponding2.Contains(E))
                                    {
                                        otherTrail = GetByte(tile[x, y + 1], isRoad);
                                        if (!corresponding1.Contains(E) && !otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, isRoad, W);
                                            tile[x1, y2] = AddByte(tile[x1, y2], NE, isRoad, E);
                                        }
                                        else if (!corresponding1.Contains(E) && otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, isRoad, W);
                                            tile[x1, y2] = AddByte(tile[x1, y2], NE, isRoad);
                                        }
                                        else if (corresponding1.Contains(E) && !otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, isRoad);
                                            tile[x1, y2] = AddByte(tile[x1, y2], NE, isRoad, E);
                                        }
                                    }
                                    else if (corresponding3.Contains(E))
                                    {
                                        otherTrail = GetByte(tile[x, y + 2], isRoad);
                                        if (!corresponding1.Contains(E) && !otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, isRoad, W);
                                            tile[x1, y3] = AddByte(tile[x1, y3], N, isRoad, E);
                                            tile[x1, y2] = AddByte(tile[x1, y2], NE, S, isRoad);
                                        }
                                        else if (!corresponding1.Contains(E) && otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, isRoad, W);
                                            tile[x1, y3] = AddByte(tile[x1, y3], N, isRoad);
                                            tile[x1, y2] = AddByte(tile[x1, y2], NE, S, isRoad);
                                        }
                                        else if (corresponding1.Contains(E) && !otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, isRoad);
                                            tile[x1, y3] = AddByte(tile[x1, y3], N, isRoad, E);
                                            tile[x1, y2] = AddByte(tile[x1, y2], NE, S, isRoad);
                                        }
                                    }
                                }
                                if (yBorder == 2)
                                {
                                    if (corresponding1.Contains(E))
                                    {
                                        otherTrail = GetByte(tile[x, y - 1], isRoad);
                                        if (!corresponding2.Contains(E) && !otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, isRoad, W);
                                            tile[x1, y1] = AddByte(tile[x1, y1], SE, isRoad, E);
                                        }
                                        else if (!corresponding2.Contains(E) && otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, isRoad, W);
                                            tile[x1, y1] = AddByte(tile[x1, y1], SE, isRoad);
                                        }
                                        else if (corresponding2.Contains(E) && !otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, isRoad);
                                            tile[x1, y1] = AddByte(tile[x1, y1], SE, isRoad, E);
                                        }
                                    }
                                    else if (corresponding3.Contains(E))
                                    {
                                        otherTrail = GetByte(tile[x, y + 1], isRoad);
                                        if (!corresponding2.Contains(E) && !otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, isRoad, W);
                                            tile[x1, y3] = AddByte(tile[x1, y3], NE, isRoad, E);
                                        }
                                        else if (!corresponding2.Contains(E) && otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, isRoad, W);
                                            tile[x1, y3] = AddByte(tile[x1, y3], NE, isRoad);
                                        }
                                        else if (corresponding2.Contains(E) && !otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SW, isRoad);
                                            tile[x1, y3] = AddByte(tile[x1, y3], NE, isRoad, E);
                                        }
                                    }
                                }
                                if (yBorder == 3)
                                {
                                    if (corresponding2.Contains(E))
                                    {
                                        otherTrail = GetByte(tile[x, y - 1], isRoad);
                                        if (!corresponding3.Contains(E) && !otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, isRoad, W);
                                            tile[x1, y2] = AddByte(tile[x1, y2], SE, isRoad, E);
                                        }
                                        else if (!corresponding3.Contains(E) && otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, isRoad, W);
                                            tile[x1, y2] = AddByte(tile[x1, y2], SE, isRoad);
                                        }
                                        else if (corresponding3.Contains(E) && !otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, isRoad);
                                            tile[x1, y2] = AddByte(tile[x1, y2], SE, isRoad, E);
                                        }
                                    }
                                    else if (corresponding1.Contains(E))
                                    {
                                        otherTrail = GetByte(tile[x, y - 2], isRoad);
                                        if (!corresponding3.Contains(E) && !otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, isRoad, W);
                                            tile[x1, y3] = AddByte(tile[x1, y3], S, isRoad, E);
                                            tile[x1, y2] = AddByte(tile[x1, y2], SE, N, isRoad);
                                        }
                                        else if (!corresponding3.Contains(E) && otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, isRoad, W);
                                            tile[x1, y3] = AddByte(tile[x1, y3], S, isRoad);
                                            tile[x1, y2] = AddByte(tile[x1, y2], SE, N, isRoad);
                                        }
                                        else if (corresponding3.Contains(E) && !otherTrail.Contains(W))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NW, isRoad);
                                            tile[x1, y3] = AddByte(tile[x1, y3], S, isRoad, E);
                                            tile[x1, y2] = AddByte(tile[x1, y2], SE, N, isRoad);
                                        }
                                    }
                                }
                            }
                        }
                        else if ((yBorder > 0 && yBorder < 4) && xBorder == 4 && x + 1 < tileSize)
                        {
                            if (road.Contains(E) || track.Contains(E))
                            {
                                if (track.Contains(E))
                                {
                                    isRoad = false;
                                }
                                x1 = x + 1;
                                y1 = y - y % 5 + 1;
                                y2 = y - y % 5 + 2;
                                y3 = y - y % 5 + 3;
                                corresponding1 = GetByte(tile[x1, y1], isRoad);
                                corresponding2 = GetByte(tile[x1, y2], isRoad);
                                corresponding3 = GetByte(tile[x1, y3], isRoad);

                                if (yBorder == 1)
                                {
                                    if (corresponding2.Contains(W))
                                    {
                                        otherTrail = GetByte(tile[x , y + 1], isRoad);
                                        if (!corresponding1.Contains(W) && !otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, isRoad, E);
                                            tile[x1, y2] = AddByte(tile[x1, y2], NW, isRoad, W);
                                        }
                                        else if (!corresponding1.Contains(W) && otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, isRoad, E);
                                            tile[x1, y2] = AddByte(tile[x1, y2], NW, isRoad);
                                        }
                                        else if (corresponding1.Contains(W) && !otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, isRoad);
                                            tile[x1, y2] = AddByte(tile[x1, y2], NW, isRoad, W);
                                        }
                                    }
                                    else if (corresponding3.Contains(W))
                                    {
                                        otherTrail = GetByte(tile[x, y + 2], isRoad);
                                        if (!corresponding1.Contains(W) && !otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, isRoad, E);
                                            tile[x1, y3] = AddByte(tile[x1, y3], N, isRoad, W);
                                            tile[x1, y2] = AddByte(tile[x1, y2], S, NW, isRoad);
                                        }
                                        else if (!corresponding1.Contains(W) && otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, isRoad, E);
                                            tile[x1, y3] = AddByte(tile[x1, y3], N, isRoad);
                                            tile[x1, y2] = AddByte(tile[x1, y2], S, NW, isRoad);
                                        }
                                        else if (corresponding1.Contains(W) && !otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, isRoad);
                                            tile[x1, y3] = AddByte(tile[x1, y3], N, isRoad, W);
                                            tile[x1, y2] = AddByte(tile[x1, y2], S, NW, isRoad);
                                        }
                                    }
                                }
                                if (yBorder == 2)
                                {
                                    if (corresponding1.Contains(W))
                                    {
                                        otherTrail = GetByte(tile[x, y - 1], isRoad);
                                        if (!corresponding2.Contains(W) && !otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, isRoad, E);
                                            tile[x1, y1] = AddByte(tile[x1, y1], SW, isRoad, W);
                                        }
                                        else if (!corresponding2.Contains(W) && otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, isRoad, E);
                                            tile[x1, y1] = AddByte(tile[x1, y1], SW, isRoad);
                                        }
                                        else if (corresponding2.Contains(W) && !otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, isRoad);
                                            tile[x1, y1] = AddByte(tile[x1, y1], SW, isRoad, W);
                                        }
                                    }
                                    else if (corresponding3.Contains(W))
                                    {
                                        otherTrail = GetByte(tile[x, y + 1], isRoad);
                                        if (!corresponding2.Contains(W) && !otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, isRoad, E);
                                            tile[x1, y3] = AddByte(tile[x1, y3], NW, isRoad, W);
                                        }
                                        else if (!corresponding2.Contains(W) && otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, isRoad, E);
                                            tile[x1, y3] = AddByte(tile[x1, y3], NW, isRoad);
                                        }
                                        else if (corresponding2.Contains(W) && !otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], SE, isRoad);
                                            tile[x1, y3] = AddByte(tile[x1, y3], NW, isRoad, W);
                                        }
                                    }
                                }
                                if (yBorder == 3)
                                {
                                    if (corresponding2.Contains(W))
                                    {
                                        otherTrail = GetByte(tile[x, y - 1], isRoad);
                                        if (!corresponding3.Contains(W) && !otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, isRoad, E);
                                            tile[x1, y2] = AddByte(tile[x1, y2], SW, isRoad, W);
                                        }
                                        else if (!corresponding3.Contains(W) && otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, isRoad, E);
                                            tile[x1, y2] = AddByte(tile[x1, y2], SW, isRoad);
                                        }
                                        else if (corresponding3.Contains(W) && !otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, isRoad);
                                            tile[x1, y2] = AddByte(tile[x1, y2], SW, isRoad, W);
                                        }
                                    }
                                    else if (corresponding1.Contains(W))
                                    {
                                        otherTrail = GetByte(tile[x, y - 2], isRoad);
                                        if (!corresponding3.Contains(W) && !otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, isRoad, E);
                                            tile[x1, y1] = AddByte(tile[x1, y1], S, isRoad, W);
                                            tile[x1, y2] = AddByte(tile[x1, y2], SW, N, isRoad);
                                        }
                                        else if (!corresponding3.Contains(W) && otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, isRoad, E);
                                            tile[x1, y1] = AddByte(tile[x1, y1], S, isRoad);
                                            tile[x1, y2] = AddByte(tile[x1, y2], SW, N, isRoad);
                                        }
                                        else if (corresponding3.Contains(W) && !otherTrail.Contains(E))
                                        {
                                            tile[x, y] = AddByte(tile[x, y], NE, isRoad);
                                            tile[x1, y1] = AddByte(tile[x1, y1], S, isRoad, W);
                                            tile[x1, y2] = AddByte(tile[x1, y2], SW, N, isRoad);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return tile;
        }

        protected List<byte> GetByte(int trail, bool road)
        {
            List<byte> result = new List<byte>();
            int baseValue = 2;
            int pow = 7;
            byte factor;
            if (road)
            {
                trail = trail % 256;
            }
            else trail = trail / 256;

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

        protected int AddByte(int trail, byte trailToAdd, bool isRoad, byte trailToRemove = 0)
        {
            List<byte> resultTrack = GetByte(trail, false);
            List<byte> resultRoad = GetByte(trail, true);

            resultTrack.Remove(trailToAdd);
            resultTrack.Remove(trailToRemove);
            resultRoad.Remove(trailToAdd);
            resultRoad.Remove(trailToRemove);

            trail = 0;
            foreach (byte track in resultTrack) trail += track * 256;
            foreach (byte road in resultRoad) trail += road;

            if (isRoad) trail += trailToAdd;
            else trail += trailToAdd * 256;

            return trail;            
        }

        protected int AddByte(int trail, byte trailToAdd1, byte trailToAdd2, bool isRoad, byte trailToRemove = 0)
        {
            List<byte> resultTrack = GetByte(trail, false);
            List<byte> resultRoad = GetByte(trail, true);

            resultTrack.Remove(trailToAdd1);
            resultTrack.Remove(trailToAdd2);
            resultTrack.Remove(trailToRemove);
            resultRoad.Remove(trailToAdd1);
            resultRoad.Remove(trailToAdd2);
            resultRoad.Remove(trailToRemove);

            trail = 0;
            foreach (byte track in resultTrack) trail += track * 256;
            foreach (byte road in resultRoad) trail += road;

            if (isRoad)
            {
                trail += trailToAdd1;
                trail += trailToAdd2;
            }
            else
            {
                trail += trailToAdd1 * 256;
                trail += trailToAdd2 * 256;
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

        protected int[,] GetTileBuffer(byte trailAsset, byte trailType, byte[,] buffer, int[,] tileBuffer, bool hasLocation)
        {
            int[,] resultingBuffer = new int[5, 5];
            resultingBuffer = tileBuffer;

            // if (resultingBuffer[12] == 0)
            //     resultingBuffer[12] = trailType;

            int baseValue = 2;
            int pow = 7;

            bool[,] thread = new bool[5, 5];
            List<(int, int)> tileExit = new List<(int, int)>();

            // if (trailType == 1)
            //     trailType *= 32;
            // else trailType *= 64;

            do{
                byte factor = (byte)Math.Pow(baseValue, pow);

                if (trailAsset >= factor)
                {
                    trailAsset -= factor;

                    Debug.Log("factor: " + factor);

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

            if (hasLocation)
            {
                tileExit.Insert((UnityEngine.Random.Range(0, tileExit.Count)), (2, 2));
            }

            bool getToJunction = false;
            if (tileExit.Count == 1)
            {
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

                Debug.Log("start: " + start.Item1 + ", " + start.Item2 + "; arrival: " + arrival.Item1 + ", " + arrival.Item2);

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
                        Debug.Log("stepToRemove.Count: " + stepToRemove.Count);
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

                resultingBuffer = MergeWithTrail(resultingBuffer, trail, trailType);
            }

            return resultingBuffer;
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

        protected int[,] MergeWithTrail(int[,] baseBuffer, List<(int, int)> trailToAdd, byte trailType)
        {
            foreach ((int, int) trail in trailToAdd)
            {
                byte trailShape = GetTrailShape(trail, trailToAdd);

                if (baseBuffer[trail.Item1, trail.Item2] == 0)
                {
                    if (trailType == (int)TrailTypes.Track)
                        baseBuffer[trail.Item1, trail.Item2] = (trailShape * 256);
                    else baseBuffer[trail.Item1, trail.Item2] = trailShape;
                }
                else
                {
                    // (byte, byte) result;
                    int baseValue = 2;
                    int pow = 7;

                    byte roadWiP = 0;
                    bool hasRoad = false;
                    byte trackWiP = 0;
                    bool hasTrack = false;

                    if (baseBuffer[trail.Item1, trail.Item2] > 256)
                    {
                        trackWiP = (byte)(baseBuffer[trail.Item1, trail.Item2] / 256);
                        hasTrack = true;
                    }
                    if (baseBuffer[trail.Item1, trail.Item2] - (trackWiP * 256) > 0)
                    {
                        roadWiP = (byte)(baseBuffer[trail.Item1, trail.Item2] % 256);
                        hasRoad = true;
                    }

                    do
                    {
                        byte factor = (byte)Math.Pow(baseValue, pow);

                        if (roadWiP < factor && trackWiP < factor && trailShape >= factor)
                        {
                            if (trailType == (int)TrailTypes.Track)
                                baseBuffer[trail.Item1, trail.Item2] += (factor * 256);
                            else baseBuffer[trail.Item1, trail.Item2] += factor;

                            if (factor == byte.MaxValue)
                                Debug.Log("Track set to 255 with roadWiP " + roadWiP + " and trackWiP " + trackWiP);
                        }
                        else if (roadWiP < factor && trackWiP >= factor && trailShape >= factor && trailType == 1)
                        {
                            baseBuffer[trail.Item1, trail.Item2] += factor;
                            baseBuffer[trail.Item1, trail.Item2] -= (factor * 256);
                        }
                        if (roadWiP >= factor) roadWiP -= factor;
                        if (trackWiP >= factor) trackWiP -= factor;
                        if (trailShape >= factor) trailShape -= factor;

                        pow--;
                    }
                    while (pow >= 0 && trailShape >= 0);
                }
            }

            return baseBuffer;
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
                Debug.Log("index: " + index);
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
            Debug.Log("Returnin trailShape: " + (exits.Item1 + exits.Item2));
            return (byte)(exits.Item1 + exits.Item2);
        }

        protected (byte, byte)[,] GenerateTrailMap(List<DFPosition> trail, (byte, byte)[,] trailMap, TrailTypes trailPreference)
        {
            int counter = 0;
            byte trailToPlace1;
            byte trailToPlace2;
            byte resultingTrailToPlace;
            byte[,] trailResult = new byte[MapsFile.MaxMapPixelX, MapsFile.MaxMapPixelY];

            foreach (DFPosition trailSection in trail)
            {
                int xDir, yDir;

                if (trailPreference == TrailTypes.Road)
                {
                    resultingTrailToPlace = trailMap[trailSection.X, trailSection.Y].Item1;
                }
                else{
                    resultingTrailToPlace = trailMap[trailSection.X, trailSection.Y].Item2;
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

                if (trailPreference == TrailTypes.Road)
                {
                    trailMap[trailSection.X, trailSection.Y].Item1 = resultingTrailToPlace;
                    if (trailMap[trailSection.X, trailSection.Y].Item2 > 0)
                    {
                        trailMap[trailSection.X, trailSection.Y] = MergeTrail(resultingTrailToPlace, trailMap[trailSection.X, trailSection.Y].Item2);
                    }
                }                    
                else{
                    trailMap[trailSection.X, trailSection.Y].Item2 = resultingTrailToPlace;
                    if (trailMap[trailSection.X, trailSection.Y].Item1 > 0)
                    {
                        trailMap[trailSection.X, trailSection.Y] = MergeTrail(trailMap[trailSection.X, trailSection.Y].Item1, resultingTrailToPlace);
                    }
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

            Debug.Log("OverwriteTrail -> returning " + resultTrail);
            return resultTrail;
        }

        protected (byte, byte) MergeTrail(byte roadByte, byte trackByte)
        {
            (byte, byte) result = (0, 0);
            int baseValue = 2;
            int pow = 7;
            // byte roadWiP = roadByte;
            // byte trackWiP = 0;

            do{
                byte factor = (byte)Math.Pow(baseValue, pow);

                if (roadByte >= factor)
                    result.Item1 += factor;
                else if (trackByte >= factor)
                    result.Item2 += factor;

                if (roadByte >= factor) roadByte -= factor;
                if (trackByte >= factor) trackByte -= factor;

                pow--;
            }
            while (pow >= 0 && (roadByte >= 0 || trackByte >= 0));

            Debug.Log("MergeTrail -> returning " + result);
            return result;
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

        protected RoutedLocation CompareLocDistance(RoutedLocation loc, out RoutedLocation locToCompare, ref List<RoutedLocation> locComp)
        {
            List<(ulong, float)> locDistanceSwap = loc.locDistance;
            int counter = 0;

            locToCompare = new RoutedLocation();

            foreach ((ulong, float) locDist in loc.locDistance)
            {
                if (((int)locDist.Item1 % MapsFile.WorldWidth) / MapsFile.TileDim != xTile || ((int)locDist.Item1 / MapsFile.WorldWidth) / MapsFile.TileDim != yTile)
                    continue;

                locToCompare = locComp.Find(x => x.position.Equals(MapsFile.GetPixelFromPixelID(locDist.Item1)));
                Debug.Log("locDist.Item1: " + locDist.Item1 + "; locDist.Item2: " + locDist.Item2);
                // Debug.Log("locToCompare.locDistance: " + locToCompare.locDistance[counter].Item1 + ", " + locToCompare.locDistance[counter].Item2);

                if (locToCompare.locDistance.Exists(x => x.Item1 == locDist.Item1))
                {
                    int index = -1;
                    index = locToCompare.locDistance.FindIndex(y => y.Item1 == locDist.Item1);

                    if (locToCompare.locDistance[index].Item2 < locDist.Item2)
                    {
                        locDistanceSwap.RemoveAt(counter);
                    }
                    else if (locToCompare.locDistance[index].Item2 > locDist.Item2)
                    {
                        locToCompare.locDistance.RemoveAt(index);
                    }
                    else if (UnityEngine.Random.Range(0, 1) == 0)
                    {
                        locDistanceSwap.RemoveAt(counter);
                    }
                    else{
                        locToCompare.locDistance.RemoveAt(index);
                    }
                }
                counter++;
            }
            loc.locDistance = locDistanceSwap;

            return loc;
        }

        protected List<RoutedLocation> MergeModifiedLocDist(List<RoutedLocation> compList, RoutedLocation loc)
        {
            int index = -1;

            if (compList.Exists(x => x.position == loc.position))
            {
                index = compList.FindIndex(y => y.position == loc.position);

                // foreach ((ulong, float) locDist in compList[index].locDistance)
                // {
                //     if 
                // }
                List<(ulong, float)> intersect = compList[index].locDistance.Intersect(loc.locDistance).ToList();
                compList[index].locDistance = intersect;
            }
            else compList.Add(loc);

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
            (int, int, int, int)[] modifiers = { (0, -1, 1, 1), (1, 0, -1, 1), (0, 1, 1, -1), (-1, 0, 1, 1) };
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
            // else
            // {
            //     if (dungeon.Exterior.ExteriorData.BlockNames[0].Contains("CASTAA") ||
            //         dungeon.Exterior.ExteriorData.BlockNames[0].Contains("RUINAA") ||
            //         dungeon.Exterior.ExteriorData.BlockNames[0].Contains("GRVEAS"))
            //         return true;
            // }

            return false;
        }
    }
}