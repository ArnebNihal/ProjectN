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

namespace MapEditor
{
    public class PixelData
    {
        public bool hasLocation;
        public string Name;
        public string RegionName;
        public ulong MapId;        
        public int Latitude;
        public int Longitude;
        public int LocationType = -1;
        public int DungeonType = -1;
        public int Key;
        public Exterior exterior = new Exterior();
        public Dungeon dungeon = new Dungeon();
        public DFLocation.ClimateSettings climateSettings;
        public int Politic;
        public int RegionIndex;
        public int LocationIndex;

        public int Elevation;
        public int Region;
        public int Climate;

        public void GetPixelData(int x, int y)
        {
            if (Worldmaps.HasLocation(x, y))
            {
                MapSummary mapSummary;
                Worldmaps.HasLocation(x, y, out mapSummary);

                hasLocation = true;
                // Debug.Log("Worldmaps.Worldmap.Length: " + Worldmaps.Worldmap.Length + " Worldmaps.Worldmap[mapSummary.RegionIndex].Locations.Length: " + Worldmaps.Worldmap[mapSummary.RegionIndex].Locations.Length + ", mapSummary.RegionIndex: " + mapSummary.RegionIndex + ", mapSummary.MapIndex: " + mapSummary.MapIndex);
                Name = Worldmaps.Worldmap[mapSummary.RegionIndex].Locations[mapSummary.MapIndex].Name;
                RegionName = Worldmaps.Worldmap[mapSummary.RegionIndex].Name;
                MapId = Worldmaps.Worldmap[mapSummary.RegionIndex].Locations[mapSummary.MapIndex].MapTableData.MapId;
                Latitude = Worldmaps.Worldmap[mapSummary.RegionIndex].Locations[mapSummary.MapIndex].MapTableData.Latitude;
                Longitude = Worldmaps.Worldmap[mapSummary.RegionIndex].Locations[mapSummary.MapIndex].MapTableData.Longitude;
                LocationType = (int)Worldmaps.Worldmap[mapSummary.RegionIndex].Locations[mapSummary.MapIndex].MapTableData.LocationType;

                // if ((int)Worldmaps.Worldmap[mapSummary.RegionIndex].Locations[mapSummary.MapIndex].MapTableData.DungeonType == 255)
                //     DungeonType = Enum.GetNames(typeof(DFRegion.DungeonTypes)).Length -1;
                DungeonType = (int)Worldmaps.Worldmap[mapSummary.RegionIndex].Locations[mapSummary.MapIndex].MapTableData.DungeonType;
                Key = (int)Worldmaps.Worldmap[mapSummary.RegionIndex].Locations[mapSummary.MapIndex].MapTableData.Key;
                exterior.GetExteriorData(x, y, mapSummary.RegionIndex, mapSummary.MapIndex);
                dungeon.GetDungeonData(x, y, mapSummary.RegionIndex, mapSummary.MapIndex);
                climateSettings = Worldmaps.Worldmap[mapSummary.RegionIndex].Locations[mapSummary.MapIndex].Climate;
                Politic = Worldmaps.Worldmap[mapSummary.RegionIndex].Locations[mapSummary.MapIndex].Politic;
                RegionIndex = Worldmaps.Worldmap[mapSummary.RegionIndex].Locations[mapSummary.MapIndex].RegionIndex;
                LocationIndex = Worldmaps.Worldmap[mapSummary.RegionIndex].MapIdLookup[MapId];
            }
            else hasLocation = false;

            Elevation = SmallHeightmap.GetHeightMapValue(x, y);
            Region = PoliticInfo.ConvertMapPixelToRegionIndex(x, y);
            Climate = (ClimateInfo.Climate[x, y] - (int)MapsFile.Climates.Ocean);
        }
    }

    public class Exterior
    {
        public string AnotherName;
        public int X;
        public int Y;
        public int LocationId;
        public int ExteriorLocationId;
        public int BuildingCount;
        public Buildings[] buildings = new Buildings[0];
        public int Width;
        public int Height;
        public bool PortTown;
        public string[] BlockNames;

        public void GetExteriorData(int x, int y, int regionIndex, int mapIndex)
        {
            AnotherName = Worldmaps.Worldmap[regionIndex].Locations[mapIndex].Exterior.ExteriorData.AnotherName;
            X = Worldmaps.Worldmap[regionIndex].Locations[mapIndex].Exterior.RecordElement.Header.X;
            Y = Worldmaps.Worldmap[regionIndex].Locations[mapIndex].Exterior.RecordElement.Header.Y;
            LocationId = (int)Worldmaps.Worldmap[regionIndex].Locations[mapIndex].Exterior.RecordElement.Header.LocationId;
            ExteriorLocationId = (int)Worldmaps.Worldmap[regionIndex].Locations[mapIndex].Exterior.RecordElement.Header.ExteriorLocationId;
            BuildingCount = Worldmaps.Worldmap[regionIndex].Locations[mapIndex].Exterior.BuildingCount;

            if (BuildingCount > 0)
            {
                buildings = new Buildings[BuildingCount];
                for (int building = 0; building < BuildingCount; building++)
                {
                    buildings[building] = new Buildings();
                    buildings[building].GetBuildingsData(x, y, regionIndex, mapIndex, building);
                }
            }
            else
                buildings = new Buildings[0];

            Width = Worldmaps.Worldmap[regionIndex].Locations[mapIndex].Exterior.ExteriorData.Width;
            Height = Worldmaps.Worldmap[regionIndex].Locations[mapIndex].Exterior.ExteriorData.Height;
            if (Worldmaps.Worldmap[regionIndex].Locations[mapIndex].Exterior.ExteriorData.PortTownAndUnknown == 0)
                PortTown = false;
            else PortTown = true;
            BlockNames = Worldmaps.Worldmap[regionIndex].Locations[mapIndex].Exterior.ExteriorData.BlockNames;
        }
    }

    public class Buildings
    {
        public int NameSeed;
        public int FactionId;
        public int Sector;
        // public int LocationId = taken from Exterior class
        public int BuildingType;
        public int Quality;

        public void GetBuildingsData(int x, int y, int regionIndex, int mapIndex, int building)
        {
            NameSeed = Worldmaps.Worldmap[regionIndex].Locations[mapIndex].Exterior.Buildings[building].NameSeed;
            FactionId = Worldmaps.Worldmap[regionIndex].Locations[mapIndex].Exterior.Buildings[building].FactionId;
            Sector = Worldmaps.Worldmap[regionIndex].Locations[mapIndex].Exterior.Buildings[building].Sector;
            BuildingType = (int)Worldmaps.Worldmap[regionIndex].Locations[mapIndex].Exterior.Buildings[building].BuildingType;
            Quality = Worldmaps.Worldmap[regionIndex].Locations[mapIndex].Exterior.Buildings[building].Quality;
        }
    }

    public class Dungeon
    {
        public int X;
        public int Y;
        public int LocationId;
        public string DungeonName;
        public int BlockCount;
        public Blocks[] blocks;

        public void GetDungeonData(int x, int y, int regionIndex, int mapIndex)
        {
            X = Worldmaps.Worldmap[regionIndex].Locations[mapIndex].Dungeon.RecordElement.Header.X;
            Y = Worldmaps.Worldmap[regionIndex].Locations[mapIndex].Dungeon.RecordElement.Header.Y;
            LocationId = Worldmaps.Worldmap[regionIndex].Locations[mapIndex].Dungeon.RecordElement.Header.LocationId;
            DungeonName = Worldmaps.Worldmap[regionIndex].Locations[mapIndex].Dungeon.RecordElement.Header.LocationName;
            BlockCount = Worldmaps.Worldmap[regionIndex].Locations[mapIndex].Dungeon.Header.BlockCount;

            if (BlockCount > 0)
            {
                blocks = new Blocks[BlockCount];
                for (int block = 0; block < BlockCount; block++)
                {
                    blocks[block] = new Blocks();
                    blocks[block].GetBlocksData(x, y, regionIndex, mapIndex, block);
                }
            }
            else
                blocks = new Blocks[0];
        }
    }

    public class Blocks
    {
        public int X;
        public int Z;
        public bool IsStartingBlock;
        public string BlockName;
        public int WaterLevel;
        public bool CastleBlock;

        public void GetBlocksData(int x, int y, int regionIndex, int mapIndex, int block)
        {
            X = Worldmaps.Worldmap[regionIndex].Locations[mapIndex].Dungeon.Blocks[block].X;
            Z = Worldmaps.Worldmap[regionIndex].Locations[mapIndex].Dungeon.Blocks[block].Z;
            IsStartingBlock = Worldmaps.Worldmap[regionIndex].Locations[mapIndex].Dungeon.Blocks[block].IsStartingBlock;
            BlockName = Worldmaps.Worldmap[regionIndex].Locations[mapIndex].Dungeon.Blocks[block].BlockName;
            WaterLevel = Worldmaps.Worldmap[regionIndex].Locations[mapIndex].Dungeon.Blocks[block].WaterLevel;
            CastleBlock = Worldmaps.Worldmap[regionIndex].Locations[mapIndex].Dungeon.Blocks[block].CastleBlock;
        }
    }
}