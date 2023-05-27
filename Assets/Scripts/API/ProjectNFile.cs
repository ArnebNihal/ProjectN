// Project:         Daggerfall Unity - ProjectN Fork
// Copyright:       
// Web Site:        
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     
// Original Author: Arneb Nihal
// Contributors:    
// 
// Notes:
//

#region Using Statements

using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using DaggerfallConnect;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Utility;
using Newtonsoft.Json;
using System.Linq;
using DaggerfallWorkshop.Game.Guilds;

#endregion

namespace DaggerfallConnect.Arena2
{
    /// <summary>
    /// File containing regions and locations data
    /// </summary>
    public class WorldMap
    {
        #region Class Variables

        public string Name;
        public int LocationCount;
        public string[] MapNames;
        public DFRegion.RegionMapTable[] MapTable;
        public Dictionary<ulong, int> MapIdLookup;
        public Dictionary<string, int> MapNameLookup;
        public DFLocation[] Locations;
        public const string mapPath = "/home/arneb/Games/daggerfall/DaggerfallGameFiles/arena2/Maps/";

        #endregion
    }

    public static class WorldMaps
    {
        #region Class Fields

        public static WorldMap[] WorldMap;
        public static Dictionary<ulong, MapSummary> mapDict;

        static WorldMaps()
        {
            WorldMap = JsonConvert.DeserializeObject<WorldMap[]>(File.ReadAllText(Path.Combine(mapPath, "Maps.json")));
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
            if (WorldMaps.WorldMap[region].MapTable[location].LocationId != 0)
                return WorldMaps.WorldMap[region].MapTable[location].LocationId;

            // Get datafile location count (excluding added locations)
            int locationCount = WorldMaps.WorldMap[region].LocationCount;

            // Read the LocationId
            ulong locationId = WorldMaps.WorldMap[region].Locations[location].Exterior.RecordElement.Header.LocationId;

            return locationId;
        }

        public static DFRegion ConvertWorldMapsToDFRegion(int currentRegionIndex)
        {
            DFRegion dfRegion = new DFRegion();

            dfRegion.Name = WorldMaps.WorldMap[currentRegionIndex].Name;
            dfRegion.LocationCount = WorldMaps.WorldMap[currentRegionIndex].LocationCount;
            dfRegion.MapNames = WorldMaps.WorldMap[currentRegionIndex].MapNames;
            dfRegion.MapTable = WorldMaps.WorldMap[currentRegionIndex].MapTable;
            dfRegion.MapIdLookup = WorldMaps.WorldMap[currentRegionIndex].MapIdLookup;
            dfRegion.MapNameLookup = WorldMaps.WorldMap[currentRegionIndex].MapNameLookup;

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
            dfLocation = WorldMaps.WorldMap[region].Locations[location];

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
            locationOut = WorldMaps.GetLocation(regionIndex, locationIndex);
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
            if (!WorldMaps.WorldMap[Region].MapNameLookup.ContainsKey(locationName))
                return new DFLocation();

            // Get location index
            int Location = WorldMaps.WorldMap[Region].MapNameLookup[locationName];

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
            for (int i = 0; i < MapsFile.TempRegionCount; i++)
            {
                if (WorldMaps.WorldMap[i].Name == name)
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
            if (mapDict.ContainsKey(id))
            {
                summaryOut = mapDict[id];
                return true;
            }

            summaryOut = new MapSummary();
            return false;
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
            if (WorldMaps.mapDict.ContainsKey(mapId))
            {
                MapSummary summary = WorldMaps.mapDict[mapId];
                return GetLocation(summary.RegionIndex, summary.MapIndex, out locationOut);
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

        /// <summary>
        /// Build dictionary of locations.
        /// </summary>
        private static Dictionary<ulong, MapSummary> EnumerateMaps()
        {
            //System.Diagnostics.Stopwatch s = System.Diagnostics.Stopwatch.StartNew();
            //long startTime = s.ElapsedMilliseconds;

            Dictionary<ulong, MapSummary> mapDictSwap = new Dictionary<ulong, MapSummary>();
            // locationIdToMapIdDict = new Dictionary<ulong, ulong>();

            for (int region = 0; region < MapsFile.TempRegionCount; region++)
            {
                DFRegion dfRegion = WorldMaps.ConvertWorldMapsToDFRegion(region);
                for (int location = 0; location < dfRegion.LocationCount; location++)
                {
                    MapSummary summary = new MapSummary();
                    // Get map summary
                    DFRegion.RegionMapTable mapTable = dfRegion.MapTable[location];
                    summary.ID = mapTable.MapId;
                    summary.MapID = WorldMaps.WorldMap[region].Locations[location].Exterior.RecordElement.Header.LocationId;
                    summary.RegionIndex = region;
                    summary.MapIndex = WorldMaps.WorldMap[region].Locations[location].LocationIndex;

                    summary.LocationType = mapTable.LocationType;
                    summary.DungeonType = mapTable.DungeonType;

                    // TODO: This by itself doesn't account for DFRegion.LocationTypes.GraveyardForgotten locations that start the game discovered in classic
                    summary.Discovered = mapTable.Discovered;

                    mapDictSwap.Add(summary.ID, summary);

                    // Link locationId with mapId - adds ~25ms overhead
                    // ulong locationId = WorldMaps.ReadLocationIdFast(region, location);
                    // locationIdToMapIdDict.Add(locationId, summary.ID);
                }
            }

            string fileDataPath = Path.Combine("/home/arneb/Games/daggerfall/DaggerfallGameFiles/arena2/Maps/", "mapDict.json");
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

    public class ClimateData
    {
        #region Class Fields

        public static int[,] Climate;

        static ClimateData()        
        {
            Climate = JsonConvert.DeserializeObject<int[,]>(File.ReadAllText(Path.Combine(mapPath, "Climate.json")));
        }

        public static int[,] ClimateModified;

        #endregion
    }

    public static class PoliticData
    {
        #region Class Fields

        public static int[,] Politic;

        static PoliticData()
        {
            Politic = JsonConvert.DeserializeObject<int[,]>(File.ReadAllText(Path.Combine(mapPath, "Politic.json")));
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

                    if ((X < 0 || X > MapsFile.WorldWidth || Y < 0 || Y > MapsFile.WorldHeight) ||
                        (i == 0 && j == 0))
                        continue;

                    if (politicIndex != Politic[X, Y] && 
                        politicIndex != actualPolitic && 
                        (WoodsData.GetHeightMapValue(x, y) > 3) && 
                        (WoodsData.GetHeightMapValue(X, Y) > 3))
                        return true;
                }
            }

            return false;
        }

        public static int ConvertMapPixelToRegionIndex(int x, int y)
        {
            int regionIndex = Politic[x, y];

            if (regionIndex == 64)
                return regionIndex;

            regionIndex -= 128;
            return regionIndex;
        }

        #endregion
    }

    public static class WoodsData
    {
        #region Class Fields

        public static byte[,] Woods;

        static WoodsData()
        {
            Woods = JsonConvert.DeserializeObject<byte[,]>(File.ReadAllText(Path.Combine(mapPath, "Woods.json")));
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
            if (mapPixelX >= MapsFile.WorldWidth) mapPixelX = MapsFile.WorldWidth - 1;

            // Clamp Y
            if (mapPixelY < 0) mapPixelY = 0;
            if (mapPixelY >= MapsFile.WorldHeight) mapPixelY = MapsFile.WorldHeight - 1;

            return WoodsData.Woods[mapPixelX, mapPixelY];
        }

        #endregion
    }

    public static class WoodsLargeData
    {
        #region Class Variables

        public static byte[,] WoodsLarge;

        static WoodsLargeData()
        {
            WoodsLarge = JsonConvert.DeserializeObject<byte[,]>(File.ReadAllText(Path.Combine(mapPath, "WoodsLarge.json")));
        }

        #endregion

        #region Public Methods

        public static Byte[,] GetLargeHeightMapValuesRange(int mapPixelX, int mapPixelY, int dim)
        {
            const int offsetx = 1;
            const int offsety = 1;
            const int len = 3;

            Byte[,] dstData = new Byte[dim * len, dim * len];
            for (int y = 0; y < dim; y++)
            {
                for (int x = 0; x < dim; x++)
                {
                    // Get source 5x5 data
                    Byte[,] srcData = GetLargeMapData(mapPixelX + x, mapPixelY - y);

                    // Write 3x3 heightmap pixels to destination array
                    int startX = x * len;
                    int startY = y * len;
                    for (int iy = offsety; iy < offsety + len; iy++)
                    {
                        for (int ix = offsetx; ix < offsetx + len; ix++)
                        {
                            dstData[startX + ix - offsetx, startY + iy - offsety] = srcData[ix, 4 - iy];
                        }
                    }
                }
            }

            return dstData;
        }

        public static Byte[,] GetLargeMapData(int mapPixelX, int mapPixelY)
        {
            // Clamp X
            if (mapPixelX < 0) mapPixelX = 0;
            if (mapPixelX >= MapsFile.WorldWidth - 1) mapPixelX = MapsFile.WorldWidth - 1;

            // Clamp Y
            if (mapPixelY < 0) mapPixelY = 0;
            if (mapPixelY >= MapsFile.WorldHeight - 1) mapPixelY = MapsFile.WorldHeight - 1;

            // Offset directly to this pixel data
            // BinaryReader reader = managedFile.GetReader();
            // reader.BaseStream.Position = dataOffsets[mapPixelY * mapWidthValue + mapPixelX] + 22;

            // Read 5x5 data
            Byte[,] data = new Byte[5, 5];
            for (int y = 0; y < 5; y++)
            {
                for (int x = 0; x < 5; x++)
                {
                    data[x, y] = WoodsLargeData.WoodsLarge[mapPixelX + x, mapPixelY + y];
                }
            }

            return data;
        }

        #endregion
    }
}