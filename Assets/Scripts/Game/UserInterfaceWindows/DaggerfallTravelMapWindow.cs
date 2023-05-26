// Project:         Daggerfall Unity
// Copyright:       Copyright (C) 2009-2022 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Lypyl (lypyl@dfworkshop.net), Gavin Clayton (interkarma@dfworkshop.net)
// Contributors:    Hazelnut, TheLacus, Arneb
// 
// Notes:
//

using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using System.IO;
using DaggerfallConnect;
using DaggerfallConnect.Utility;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.UserInterface;
using System.Collections.Generic;
using Wenzil.Console;
using Wenzil.Console.Commands;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Utility.AssetInjection;
using DaggerfallWorkshop.Game.Questing;
using UnityEditor.Localization.UI;

namespace DaggerfallWorkshop.Game.UserInterfaceWindows
{
    /// <summary>
    /// Implements Daggerfall's travel map.
    /// </summary>
    public class DaggerfallTravelMapWindow : DaggerfallPopupWindow
    {
        #region Fields

        protected const int betonyIndex = 19;

        protected const string overworldImgName                       = "TRAV0I00.IMG";
        protected const string regionPickerImgName                    = "TRAV0I01.IMG";
        protected const string findAtButtonImgName                    = "TRAV0I03.IMG";
        protected const string locationFilterButtonEnabledImgName     = "TRAV01I0.IMG";
        protected const string locationFilterButtonDisabledImgName    = "TRAV01I1.IMG";
        protected const string downArrowImgName                       = "TRAVAI05.IMG";
        protected const string upArrowImgName                         = "TRAVBI05.IMG";
        protected const string rightArrowImgName                      = "TRAVCI05.IMG";
        protected const string leftArrowImgName                       = "TRAVDI05.IMG";
        protected const string regionBorderImgName                    = "MBRD00I0.IMG";
        protected const string colorPaletteColName                    = "FMAP_PAL.COL";
        protected const string toggleRegionName                       = "politicToggle";
        protected const string toggleClimateName                      = "climateToggle";
        protected const int regionPanelOffset                         = 12;
        protected const int identifyFlashCount                        = 4;
        protected const int identifyFlashCountSelected                = 2;
        protected const float identifyFlashInterval                   = 0.5f;
        protected const int dotsOutlineThickness                      = 1;
        protected const int mapAlphaChannel                           = 180;
        protected Color32 dotOutlineColor                             = new Color32(0, 0, 0, 128);
        protected Vector2[] outlineDisplacements =
        {
            new Vector2(-0.5f, -0f),
            new Vector2(0f, -0.5f),
            new Vector2(0f, 0.5f),
            new Vector2(0.5f, 0f)
        };

        protected DaggerfallTravelPopUp popUp;

        protected Dictionary<string, Vector2> offsetLookup = new Dictionary<string, Vector2>();
        protected string[] selectedRegionMapNames;

        protected Place gotoPlace; // Used by journal click-through to fast travel to a specific quest location

        protected DFBitmap regionPickerBitmap;
        protected Color32[] dynamicTravelMap;
        protected const int dynamicMapWidth = 40;
        protected const int dynamicMapHeight = 25;
        protected DFRegion currentDFRegion;
        protected int currentDFRegionIndex = -1;
        protected int lastQueryLocationIndex = -1;
        protected string lastQueryLocationName;
        protected MapSummary locationSummary;

        protected KeyCode toggleClosedBinding;

        protected Panel borderPanel;
        protected Panel regionTextureOverlayPanel;
        protected Panel[] regionLocationDotsOutlinesOverlayPanel;
        protected Panel regionLocationDotsOverlayPanel;
        protected Panel regionPanel;
        protected Panel climatePanel;
        protected Panel playerRegionOverlayPanel;
        protected Panel identifyOverlayPanel;

        protected TextLabel regionLabel;

        protected Texture2D overworldTexture;
        protected Texture2D identifyTexture;
        protected Texture2D locationDotsTexture;
        protected Texture2D locationDotsOutlineTexture;
        protected Texture2D findButtonTexture;
        protected Texture2D atButtonTexture;
        protected Texture2D dungeonFilterButtonEnabled;
        protected Texture2D dungeonFilterButtonDisabled;
        protected Texture2D templesFilterButtonEnabled;
        protected Texture2D templesFilterButtonDisabled;
        protected Texture2D homesFilterButtonEnabled;
        protected Texture2D homesFilterButtonDisabled;
        protected Texture2D townsFilterButtonEnabled;
        protected Texture2D townsFilterButtonDisabled;
        protected Texture2D upArrowTexture;
        protected Texture2D downArrowTexture;
        protected Texture2D leftArrowTexture;
        protected Texture2D rightArrowTexture;
        protected Texture2D borderTexture;
        protected Texture2D toggleRegionTexture;
        protected Texture2D toggleClimateTexture;
        protected Texture2D regionTexture;
        protected Texture2D climateTexture;

        protected Button findButton;
        protected Button atButton;
        protected Button exitButton;
        protected Button upArrowButton    = new Button();
        protected Button downArrowButton      = new Button();
        protected Button dungeonsFilterButton     = new Button();
        protected Button templesFilterButton      = new Button();
        protected Button homesFilterButton        = new Button();
        protected Button townsFilterButton        = new Button();
        protected Button regionToggleButton       = new Button();
        protected Button climateToggleButton       = new Button();

        protected Rect playerRegionOverlayPanelRect   = new Rect(0, 0, 320, 200);
        protected Rect regionTextureOverlayPanelRect  = new Rect(0, 0, 320, 200);
        protected Rect regionPanelRect                = new Rect(0, 0, 320, 200);
        protected Rect climatePanelRect               = new Rect(0, 0, 320, 200);
        protected Rect dungeonsFilterButtonSrcRect    = new Rect(0, 0, 99, 11);
        protected Rect templesFilterButtonSrcRect     = new Rect(0, 11, 99, 11);
        protected Rect homesFilterButtonSrcRect       = new Rect(99, 0, 80, 11);
        protected Rect townsFilterButtonSrcRect       = new Rect(99, 11, 80, 11);
        protected Rect findButtonRect                 = new Rect(0, 0, 45, 11);
        protected Rect atButtonRect                   = new Rect(0, 11, 45, 11);
        protected Rect politicButtonRect              = new Rect(0, 0, 42, 9);
        protected Rect climateButtonRect              = new Rect(0, 0, 42, 9);

        protected Color32[] identifyPixelBuffer;
        protected Color32[] locationDotsPixelBuffer;
        protected Color32[] locationDotsOutlinePixelBuffer;
        protected Color32[] locationPixelColors;              // Pixel colors for different location types
        protected Color identifyFlashColor;
        protected Color32[] regionBuffer;
        protected Color32[] climateBuffer;

        protected DFPosition mapCenter;
        protected int zoomfactor                  = 4;
        protected int maxZoomFactor               = 15;
        protected int mouseOverRegion             = -1;
        protected int selectedRegion              = -1;
        protected int mapIndex                    = 0;        // Current index of loaded map from selectedRegionMapNames
        protected float scale                     = 1.0f;
        protected float identifyLastChangeTime    = 0;
        protected float identifyChanges           = 0;

        protected bool identifyState          = false;
        protected bool identifying            = false;
        protected bool locationSelected       = false;
        protected bool findingLocation        = false;
        protected bool zoom                   = false;        // Toggles zoom mode
        protected bool teleportationTravel    = false;        // Indicates travel should be by teleportation
        protected static bool revealUndiscoveredLocations;    // Flag used to indicate cheat/debugging mode for revealing undiscovered locations

        protected bool filterDungeons = false;
        protected bool filterTemples = false;
        protected bool filterHomes = false;
        protected bool filterTowns = false;
        protected bool regionToggle = false;
        protected bool climateToggle = false;

        protected Vector2 lastMousePos = Vector2.zero;
        protected Vector2 zoomOffset = Vector2.zero;
        protected Vector2 zoomPosition = Vector2.zero;

        protected readonly Dictionary<string, Texture2D> regionTextures = new Dictionary<string, Texture2D>();
        protected readonly Dictionary<int, Texture2D> importedOverlays = new Dictionary<int, Texture2D>();

        protected readonly int maxMatchingResults = 1000;
        protected string distanceRegionName = null;
        protected IDistance distance;

        // Populated with localized names whenever player searches or lists inside this region
        // Used to complete search and list on localized names over canonical names
        protected Dictionary<string, int> localizedMapNameLookup = new Dictionary<string, int>();

        #endregion

        #region Properties

        protected string RegionImgName { get; set; }

        protected bool HasMultipleMaps
        {
            get { return (selectedRegionMapNames.Length > 1) ? true : false; }
        }

        protected bool HasVerticalMaps
        {
            get { return (selectedRegionMapNames.Length > 2) ? true : false; }
        }

        protected bool RegionSelected
        {
            get { return selectedRegion != -1; }
        }

        protected bool MouseOverRegion
        {
            get { return mouseOverRegion != -1; }
        }

        protected bool MouseOverOtherRegion
        {
            get { return RegionSelected && (selectedRegion != mouseOverRegion); }
        }

        protected bool FindingLocation
        {
            get { return identifying && findingLocation && RegionSelected; }
        }

        public MapSummary LocationSummary { get => locationSummary; }

        public void ActivateTeleportationTravel()
        {
            teleportationTravel = true;
        }

        public void GotoPlace(Place place)
        {
            gotoPlace = place;
        }

        #endregion

        #region Constructors

        public DaggerfallTravelMapWindow(IUserInterfaceManager uiManager)
            : base(uiManager)
        {
            // Register console commands
            try
            {
                TravelMapConsoleCommands.RegisterCommands();
            }
            catch (Exception ex)
            {
                Debug.LogError(string.Format("Error Registering Travelmap Console commands: {0}", ex.Message));
            }

            // Prevent duplicate close calls with base class's exitKey (Escape)
            AllowCancel = false;
        }

        #endregion

        #region User Interface

        protected override void Setup()
        {
            ParentPanel.BackgroundColor = Color.black;

            // Set location pixel colors and identify flash color from palette file
            DFPalette colors = new DFPalette();
            if (!colors.Load(Path.Combine(DaggerfallUnity.Instance.Arena2Path, colorPaletteColName)))
                throw new Exception("DaggerfallTravelMap: Could not load color palette.");

            locationPixelColors = new Color32[]
            {
                new Color32(colors.GetRed(237), colors.GetGreen(237), colors.GetBlue(237), 255),  //dunglab (R215, G119, B39)
                new Color32(colors.GetRed(240), colors.GetGreen(240), colors.GetBlue(240), 255),  //dungkeep (R191, G87, B27)
                new Color32(colors.GetRed(243), colors.GetGreen(243), colors.GetBlue(243), 255),  //dungruin (R171, G51, B15)
                new Color32(colors.GetRed(246), colors.GetGreen(246), colors.GetBlue(246), 255),  //graveyards (R147, G15, B7)
                new Color32(colors.GetRed(0), colors.GetGreen(0), colors.GetBlue(0), 255),        //coven (R15, G15, B15)
                new Color32(colors.GetRed(53), colors.GetGreen(53), colors.GetBlue(53), 255),     //farms (R165, G100, B70)
                new Color32(colors.GetRed(51), colors.GetGreen(51), colors.GetBlue(51), 255),     //wealthy (R193, G133, B100)
                new Color32(colors.GetRed(55), colors.GetGreen(55), colors.GetBlue(55), 255),     //poor (R140, G86, B55)
                new Color32(colors.GetRed(96), colors.GetGreen(96), colors.GetBlue(96), 255),     //temple (R176, G205, B255)
                new Color32(colors.GetRed(101), colors.GetGreen(101), colors.GetBlue(101), 255),  //cult (R68, G124, B192)
                new Color32(colors.GetRed(39), colors.GetGreen(39), colors.GetBlue(39), 255),     //tavern (R126, G81, B89)
                new Color32(colors.GetRed(33), colors.GetGreen(33), colors.GetBlue(33), 255),     //city (R220, G177, B177)
                new Color32(colors.GetRed(35), colors.GetGreen(35), colors.GetBlue(35), 255),     //hamlet (R188, G138, B138)
                new Color32(colors.GetRed(37), colors.GetGreen(37), colors.GetBlue(37), 255),     //village (R155, G105, B106)
            };

            identifyFlashColor = new Color32(colors.GetRed(244), colors.GetGreen(244), colors.GetBlue(244), 255); // (R163, G39, B15)

            // Add region label
            regionLabel = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(0, 2), string.Empty, NativePanel);
            regionLabel.HorizontalAlignment = HorizontalAlignment.Center;

            // Handle clicks
            NativePanel.OnMouseClick += ClickHandler;

            // Setup buttons for first time
            LoadButtonTextures();
            UpdateSearchButtons();

            // Region overlay panel
            regionTextureOverlayPanel = DaggerfallUI.AddPanel(regionTextureOverlayPanelRect, NativePanel);
            regionTextureOverlayPanel.Enabled = false;

            // Location dots overlay panel
            if (DaggerfallUnity.Settings.TravelMapLocationsOutline)
            {
                regionLocationDotsOutlinesOverlayPanel = new Panel[outlineDisplacements.Length];
                for (int i = 0; i < outlineDisplacements.Length; i++)
                {
                    Rect modifedPanelRect = regionTextureOverlayPanelRect;
                    modifedPanelRect.x += outlineDisplacements[i].x * dotsOutlineThickness / NativePanel.LocalScale.x;
                    modifedPanelRect.y += outlineDisplacements[i].y * dotsOutlineThickness / NativePanel.LocalScale.y;
                    regionLocationDotsOutlinesOverlayPanel[i] = DaggerfallUI.AddPanel(modifedPanelRect, NativePanel);
                    regionLocationDotsOutlinesOverlayPanel[i].Enabled = false;
                }
            }
            regionLocationDotsOverlayPanel = DaggerfallUI.AddPanel(regionTextureOverlayPanelRect, NativePanel);
            regionLocationDotsOverlayPanel.Enabled = true;

            regionPanel = DaggerfallUI.AddPanel(regionPanelRect, NativePanel);
            regionPanel.Enabled = false;

            climatePanel = DaggerfallUI.AddPanel(climatePanelRect, NativePanel);
            climatePanel.Enabled = false;

            // Current region overlay panel
            playerRegionOverlayPanel = DaggerfallUI.AddPanel(playerRegionOverlayPanelRect, NativePanel);
            playerRegionOverlayPanel.Enabled = false;

            // Overlay for the region panel
            identifyOverlayPanel = DaggerfallUI.AddPanel(regionTextureOverlayPanelRect, NativePanel);
            identifyOverlayPanel.Enabled = true;

            // Borders around the region maps
            // borderTexture = DaggerfallUI.GetTextureFromImg(regionBorderImgName);
            // borderPanel = DaggerfallUI.AddPanel(new Rect(new Vector2(0, regionTextureOverlayPanelRect.position.y), regionTextureOverlayPanelRect.size), NativePanel);
            // borderPanel.BackgroundTexture = borderTexture;
            // borderPanel.Enabled = false;

            // Load native overworld texture
            mapCenter = TravelTimeCalculator.GetPlayerTravelPosition();
            JustifyMapCenter();
            SetupTravelMap();

            // Setup pixel buffer and texture for region/location identify
            identifyPixelBuffer = new Color32[(int)regionTextureOverlayPanelRect.width * (int)regionTextureOverlayPanelRect.height];
            identifyTexture = new Texture2D((int)regionTextureOverlayPanelRect.width, (int)regionTextureOverlayPanelRect.height, TextureFormat.ARGB32, false);
            identifyTexture.filterMode = FilterMode.Point;

            // Setup pixel buffer and texture for location dots overlay
            SetupPixelBuffer();
            SetupRegionBuffer();
            SetupClimateBuffer();

            // Load map names for player region
            // selectedRegionMapNames = GetRegionMapNames(GetPlayerRegion());

            // Identify current region
            // StartIdentify();
            // UpdateIdentifyTextureForPlayerRegion();
            UpdateMapLocationDotsTexture();
            UpdateRegionAreaTexture();
            UpdateClimateAreaTexture();
            SetupButtons();
        }

        public override void OnPush()
        {
            base.OnPush();

            toggleClosedBinding = InputManager.Instance.GetBinding(InputManager.Actions.TravelMap);

            if (IsSetup)
            {
                // StartIdentify();
                // UpdateIdentifyTextureForPlayerRegion();
                // CloseRegionPanel();
            }

        }

        public override void OnPop()
        {
            base.OnPop();
            teleportationTravel = false;
            findingLocation = false;
            gotoPlace = null;
            distanceRegionName = null;
            distance = null;
        }

        public override void Update()
        {
            base.Update();

            // Toggle window closed with same hotkey used to open it
            if (InputManager.Instance.GetKeyUp(toggleClosedBinding) || InputManager.Instance.GetBackButtonUp())
            {
                CloseWindow();
            }

            // Input handling
            HotkeySequence.KeyModifiers keyModifiers = HotkeySequence.GetKeyboardKeyModifiers();
            Vector2 currentMousePos = new Vector2((NativePanel.ScaledMousePosition.x), (NativePanel.ScaledMousePosition.y));

            if (currentMousePos != lastMousePos)
            {
                lastMousePos = currentMousePos;
                UpdateMouseOverRegion();
                UpdateMouseOverLocation();
            }

            if (InputManager.Instance.GetMouseButtonUp(1))
            {
                // Zoom to mouse position
                zoomPosition = GetCoordinates();
                mapCenter = new DFPosition((int)zoomPosition.x, (int)zoomPosition.y);
                JustifyMapCenter();
                SetupTravelMap();
                SetupPixelBuffer();
                SetupRegionBuffer();
                SetupClimateBuffer();
                UpdateMapLocationDotsTexture();
                UpdateRegionAreaTexture();
                UpdateClimateAreaTexture();
                // ZoomMapTextures();
            }
            else if ((Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && zoom && NativePanel.MouseOverComponent)
            {
                // Scrolling while zoomed in
                zoomPosition = currentMousePos;
                // ZoomMapTextures();
            }

            UpdateRegionLabel();

            if (DaggerfallShortcut.GetBinding(DaggerfallShortcut.Buttons.TravelMapList).IsUpWith(keyModifiers))
            {

                if (currentDFRegion.LocationCount < 1)
                    return;

                string[] locations = GetCurrentRegionLocalizedMapNames().OrderBy(p => p).ToArray();
                ShowLocationPicker(locations, true);
            }
            else if (DaggerfallShortcut.GetBinding(DaggerfallShortcut.Buttons.TravelMapFind).IsUpWith(keyModifiers))
                FindlocationButtonClickHandler(null, Vector2.zero);
            else
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    // if (identifying)
                        // OpenRegionPanel(GetPlayerRegion());
                }
            }

            // Show/hide identify panel when identify is running
            identifyOverlayPanel.Enabled = identifying && identifyState;
            AnimateIdentify();

            // If a goto location specified, find it and ask if player wants to travel.
            if (gotoPlace != null)
            {
                // Get localized name for search with fallback to canonical name
                string localizedGotoPlaceName = TextManager.Instance.GetLocalizedLocationName(gotoPlace.SiteDetails.mapId, gotoPlace.SiteDetails.locationName);

                // Open region and search for localizedGotoPlaceName
                mouseOverRegion = MapsFile.PatchRegionIndex(gotoPlace.SiteDetails.regionIndex, gotoPlace.SiteDetails.regionName);
                // OpenRegionPanel(mouseOverRegion);
                // UpdateRegionLabel();
                HandleLocationFindEvent(null, localizedGotoPlaceName);
                gotoPlace = null;
            }
        }

        #endregion

        #region Setup

        // Initial button setup
        void SetupButtons()
        {
            // Exit button
            // exitButton = DaggerfallUI.AddButton(new Rect(278, 175, 39, 22), NativePanel);
            // exitButton.OnMouseClick += ExitButtonClickHandler;

            // Find button
            findButton = DaggerfallUI.AddButton(new Rect(3, 175, findButtonRect.width, findButtonRect.height), NativePanel);
            findButton.BackgroundTexture = findButtonTexture;
            findButton.OnMouseClick += FindlocationButtonClickHandler;

            // I'm At button
            atButton = DaggerfallUI.AddButton(new Rect(3, 186, atButtonRect.width, atButtonRect.height), NativePanel);
            atButton.BackgroundTexture = atButtonTexture;
            atButton.OnMouseClick += AtButtonClickHandler;

            // Dungeons filter button
            dungeonsFilterButton.Position = new Vector2(50, 175);
            dungeonsFilterButton.Size = new Vector2(dungeonsFilterButtonSrcRect.width, dungeonsFilterButtonSrcRect.height);
            dungeonsFilterButton.Name = "dungeonsFilterButton";
            dungeonsFilterButton.OnMouseClick += FilterButtonClickHandler;
            NativePanel.Components.Add(dungeonsFilterButton);

            // Temples filter button
            templesFilterButton.Position = new Vector2(50, 186);
            templesFilterButton.Size = new Vector2(templesFilterButtonSrcRect.width, templesFilterButtonSrcRect.height);
            templesFilterButton.Name = "templesFilterButton";
            templesFilterButton.OnMouseClick += FilterButtonClickHandler;
            NativePanel.Components.Add(templesFilterButton);

            // Homes filter button
            homesFilterButton.Position = new Vector2(149, 175);
            homesFilterButton.Size = new Vector2(homesFilterButtonSrcRect.width, homesFilterButtonSrcRect.height);
            homesFilterButton.Name = "homesFilterButton";
            homesFilterButton.OnMouseClick += FilterButtonClickHandler;
            NativePanel.Components.Add(homesFilterButton);

            // Towns filter button
            townsFilterButton.Position = new Vector2(149, 186);
            townsFilterButton.Size = new Vector2(townsFilterButtonSrcRect.width, townsFilterButtonSrcRect.height);
            townsFilterButton.Name = "townsFilterButton";
            townsFilterButton.OnMouseClick += FilterButtonClickHandler;
            NativePanel.Components.Add(townsFilterButton);

            // Up arrow button
            upArrowButton.Position = new Vector2(230, 175);
            upArrowButton.Size = new Vector2(22, 20);
            // verticalArrowButton.Enabled = false;
            NativePanel.Components.Add(upArrowButton);
            upArrowButton.Name = "upArrowButton";
            upArrowButton.BackgroundTexture = upArrowTexture;
            upArrowButton.OnMouseClick += ArrowButtonClickHandler;

            // Down arrow button
            downArrowButton.Position = new Vector2(253, 175);
            downArrowButton.Size = new Vector2(22, 20);
            // verticalArrowButton.Enabled = false;
            NativePanel.Components.Add(downArrowButton);
            downArrowButton.Name = "downArrowButton";
            downArrowButton.BackgroundTexture = downArrowTexture;
            downArrowButton.OnMouseClick += ArrowButtonClickHandler;

            // Region toggle button
            regionToggleButton.Position = new Vector2(276, 175);
            regionToggleButton.Size = new Vector2(politicButtonRect.width, politicButtonRect.height);
            NativePanel.Components.Add(regionToggleButton);
            regionToggleButton.Name = "regionToggleButton";
            regionToggleButton.BackgroundTexture = toggleRegionTexture;
            regionToggleButton.OnMouseClick += ToggleButtonClickHandler;

            // Climate toggle button
            climateToggleButton.Position = new Vector2(276, 186);
            climateToggleButton.Size = new Vector2(climateButtonRect.width, climateButtonRect.height);
            NativePanel.Components.Add(climateToggleButton);
            climateToggleButton.Name = "climateToggleButton";
            climateToggleButton.BackgroundTexture = toggleClimateTexture;
            climateToggleButton.OnMouseClick += ToggleButtonClickHandler;

            // Store toggle closed binding for this window
            toggleClosedBinding = InputManager.Instance.GetBinding(InputManager.Actions.TravelMap);

        }

        // Loads textures for buttons
        void LoadButtonTextures()
        {
            Texture2D baselocationFilterButtonEnabledText = ImageReader.GetTexture(locationFilterButtonEnabledImgName);
            Texture2D baselocationFilterButtonDisabledText = ImageReader.GetTexture(locationFilterButtonDisabledImgName);
            DFSize baseSize = new DFSize(179, 22);

            // Dungeons toggle button
            dungeonFilterButtonEnabled = ImageReader.GetSubTexture(baselocationFilterButtonEnabledText, dungeonsFilterButtonSrcRect, baseSize);
            dungeonFilterButtonDisabled = ImageReader.GetSubTexture(baselocationFilterButtonDisabledText, dungeonsFilterButtonSrcRect, baseSize);

            // Dungeons toggle button
            templesFilterButtonEnabled = ImageReader.GetSubTexture(baselocationFilterButtonEnabledText, templesFilterButtonSrcRect, baseSize);
            templesFilterButtonDisabled = ImageReader.GetSubTexture(baselocationFilterButtonDisabledText, templesFilterButtonSrcRect, baseSize);

            // Homes toggle button
            homesFilterButtonEnabled = ImageReader.GetSubTexture(baselocationFilterButtonEnabledText, homesFilterButtonSrcRect, baseSize);
            homesFilterButtonDisabled = ImageReader.GetSubTexture(baselocationFilterButtonDisabledText, homesFilterButtonSrcRect, baseSize);

            // Towns toggle button
            townsFilterButtonEnabled = ImageReader.GetSubTexture(baselocationFilterButtonEnabledText, townsFilterButtonSrcRect, baseSize);
            townsFilterButtonDisabled = ImageReader.GetSubTexture(baselocationFilterButtonDisabledText, townsFilterButtonSrcRect, baseSize);

            DFSize buttonsFullSize = new DFSize(45, 22);

            findButtonTexture = ImageReader.GetTexture(findAtButtonImgName);
            findButtonTexture = ImageReader.GetSubTexture(findButtonTexture, findButtonRect, buttonsFullSize);

            atButtonTexture = ImageReader.GetTexture(findAtButtonImgName);
            atButtonTexture = ImageReader.GetSubTexture(atButtonTexture, atButtonRect, buttonsFullSize);

            // Arrows
            upArrowTexture = ImageReader.GetTexture(upArrowImgName);
            downArrowTexture = ImageReader.GetTexture(downArrowImgName);
            leftArrowTexture = ImageReader.GetTexture(leftArrowImgName);
            rightArrowTexture = ImageReader.GetTexture(rightArrowImgName);

            // Toggles  
            if (!TextureReplacement.TryImportTexture(toggleRegionName, true, out toggleRegionTexture))
                toggleRegionTexture = Resources.Load<Texture2D>(toggleRegionName);

            if (!TextureReplacement.TryImportTexture(toggleClimateName, true, out toggleClimateTexture))
                toggleClimateTexture = Resources.Load<Texture2D>(toggleClimateName);
        }

        #endregion

        #region Map Texture Management

        protected virtual void SetupPixelBuffer()
        {
            locationDotsOutlinePixelBuffer = new Color32[(int)regionTextureOverlayPanelRect.width * (int)regionTextureOverlayPanelRect.height];
            locationDotsPixelBuffer = new Color32[(dynamicMapWidth * zoomfactor) * (dynamicMapHeight * zoomfactor)];
            locationDotsOutlineTexture = new Texture2D((int)regionTextureOverlayPanelRect.width, (int)regionTextureOverlayPanelRect.height, TextureFormat.ARGB32, false);
            locationDotsOutlineTexture.filterMode = FilterMode.Point;
            locationDotsTexture = new Texture2D(dynamicMapWidth * zoomfactor, dynamicMapHeight * zoomfactor, TextureFormat.ARGB32, false);
            locationDotsTexture.filterMode = FilterMode.Point;
        }

        // Updates location dots
        protected virtual void UpdateMapLocationDotsTexture()
        {
            // Get map and dimensions
            // string mapName = selectedRegionMapNames[mapIndex];
            Vector2 origin = new Vector2(0, 0);
            int width = dynamicMapWidth * zoomfactor;
            int height = dynamicMapHeight * zoomfactor;
            int originX = mapCenter.X - (width / 2);
            int originY = mapCenter.Y - (height / 2);

            // Plot locations to color array
            Array.Clear(locationDotsPixelBuffer, 0, locationDotsPixelBuffer.Length);
            // Array.Clear(locationDotsOutlinePixelBuffer, 0, locationDotsOutlinePixelBuffer.Length);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int offset = (((height - y - 1) * width) + x);
                    if (offset >= (width * height))
                        continue;
                    int sampleRegion = PoliticData.Politic[originX + x, originY + y] - 128;

                    MapSummary summary;
                    if (WorldMaps.HasLocation(originX + x, originY + y, out summary))
                    {
                        if (!checkLocationDiscovered(summary))
                            continue;

                        int index = GetPixelColorIndex(summary.LocationType);
                        if (index == -1)
                            continue;
                        else
                        {
                            if (DaggerfallUnity.Settings.TravelMapLocationsOutline)
                                locationDotsOutlinePixelBuffer[offset] = dotOutlineColor;
                            locationDotsPixelBuffer[offset] = locationPixelColors[index];
                        }
                    }
                    else locationDotsPixelBuffer[offset] = new Color32(0, 0, 0, 0);
                }
            }

            // Apply updated color array to texture
            if (DaggerfallUnity.Settings.TravelMapLocationsOutline)
            {
                locationDotsOutlineTexture.SetPixels32(locationDotsOutlinePixelBuffer);
                locationDotsOutlineTexture.Apply();
            }
            locationDotsTexture.SetPixels32(locationDotsPixelBuffer);
            locationDotsTexture.Apply();

            // Present texture
            if (DaggerfallUnity.Settings.TravelMapLocationsOutline)
                for (int i = 0; i < outlineDisplacements.Length; i++)
                    regionLocationDotsOutlinesOverlayPanel[i].BackgroundTexture = locationDotsOutlineTexture;
            regionLocationDotsOverlayPanel.BackgroundTexture = locationDotsTexture;
        }

        protected virtual void SetupRegionBuffer()
        {
            regionBuffer = new Color32[(dynamicMapWidth * zoomfactor) * (dynamicMapHeight * zoomfactor)];
            regionTexture = new Texture2D(dynamicMapWidth * zoomfactor, dynamicMapHeight * zoomfactor, TextureFormat.ARGB32, false);
            regionTexture.filterMode = FilterMode.Point;
        }

        protected virtual void SetupClimateBuffer()
        {
            climateBuffer = new Color32[(dynamicMapWidth * zoomfactor) * (dynamicMapHeight * zoomfactor)];
            climateTexture = new Texture2D(dynamicMapWidth * zoomfactor, dynamicMapHeight * zoomfactor, TextureFormat.ARGB32, false);
            climateTexture.filterMode = FilterMode.Point;
        }

        protected virtual void UpdateClimateAreaTexture()
        {
            Vector2 origin = new Vector2(0, 0);
            int width = dynamicMapWidth * zoomfactor;
            int height = dynamicMapHeight * zoomfactor;
            int originX = mapCenter.X - (width / 2);
            int originY = mapCenter.Y - (height / 2);

            int actualClimate = ClimateData.Climate[mapCenter.X, mapCenter.Y];
            Color32[] colours = new Color32[width * height];
            Array.Clear(climateBuffer, 0, climateBuffer.Length);

            if (!climateToggle)
                return;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    DFPosition area = new DFPosition(mapCenter.X + x - (width / 2), mapCenter.Y + y - (height / 2));
                    byte heightmap = WoodsData.GetHeightMapValue(area.X, area.Y);
                    int value = 0;

                    value = ClimateData.Climate[area.X, area.Y];

                    Color32 colour;

                    switch (value)
                    {
                        case 223:   // Ocean 
                            colour = new Color32(0, 0, 0, 0);
                            break;

                        case 224:   // Desert
                            colour = new Color32(217, 217, 217, mapAlphaChannel);
                            break;

                        case 225:   // Desert2
                            colour = new Color32(255, 255, 255, mapAlphaChannel);
                            break;

                        case 226:   // Mountains
                            colour = new Color32(230, 196, 230, mapAlphaChannel);
                            break;

                        case 227:   // RainForest
                            colour = new Color32(0, 152, 25, mapAlphaChannel);
                            break;

                        case 228:   // Swamp
                            colour = new Color32(115, 153, 141, mapAlphaChannel);
                            break;

                        case 229:   // Sub tropical
                            colour = new Color32(180, 180, 179, mapAlphaChannel);
                            break;

                        case 230:   // Woodland hills (aka Mountain Woods)
                            colour = new Color32(191, 143, 191, mapAlphaChannel);
                            break;

                        case 231:   // TemperateWoodland (aka Woodlands)
                            colour = new Color32(0, 190, 0, mapAlphaChannel);
                            break;

                        case 232:   // Haunted woodland
                            colour = new Color32(190, 166, 143, mapAlphaChannel);
                            break;

                        default:
                            colour = new Color32(0, 0, 0, 0);
                            break;
                    }

                    climateBuffer[(height - 1 - y) * width + x] = colour;
                }
            }
            
            climateTexture.SetPixels32(climateBuffer);
            climateTexture.Apply();
            climatePanel.BackgroundTexture = climateTexture;
        }

        protected virtual void UpdateRegionAreaTexture()
        {
            Vector2 origin = new Vector2(0, 0);
            int width = dynamicMapWidth * zoomfactor;
            int height = dynamicMapHeight * zoomfactor;
            int originX = mapCenter.X - (width / 2);
            int originY = mapCenter.Y - (height / 2);

            int actualPolitic = PoliticData.ConvertMapPixelToRegionIndex(mapCenter.X, mapCenter.Y);
            Color32[] colours = new Color32[width * height];
            Array.Clear(regionBuffer, 0, regionBuffer.Length);

            if (!regionToggle)
                return;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    DFPosition area = new DFPosition(mapCenter.X + x - (width / 2), mapCenter.Y + y - (height / 2));
                    int value = PoliticData.ConvertMapPixelToRegionIndex(area.X, area.Y);

                    Color32 colour;

                    if (value == actualPolitic)                        
                        value = -1;

                    switch (value)
                    {
                        case 64:    // Sea
                            colour = new Color32(0, 0, 0, 0);
                            break;

                        case 0:     // The Alik'r Desert
                            colour = new Color32(55, 170, 253, mapAlphaChannel);
                            break;

                        case 1:     // The Dragontail Mountains
                            colour = new Color32(149, 43, 29, mapAlphaChannel);
                            break;

                        case 2:     // Glenpoint Foothills - unused
                            colour = new Color32(123, 156, 118, mapAlphaChannel);
                            break;

                        case 3:     // Daggerfall Bluffs - unused
                            colour = new Color32(107, 144, 109, mapAlphaChannel);
                            break;

                        case 4:     // Yeorth Burrowland - unused
                            colour = new Color32(93, 130, 94, mapAlphaChannel);
                            break;

                        case 5:     // Dwynnen
                            colour = new Color32(212, 180, 105, mapAlphaChannel);
                            break;

                        case 6:     // Ravennian Forest - unused
                            colour = new Color32(77, 110, 78, mapAlphaChannel);
                            break;

                        case 7:     // Devilrock - unused
                            colour = new Color32(68, 99, 67, mapAlphaChannel);
                            break;

                        case 8:     // Malekna Forest - unused
                            colour = new Color32(61, 89, 53, mapAlphaChannel);
                            break;

                        case 9:     // The Isle of Balfiera
                            colour = new Color32(158, 0, 0, mapAlphaChannel);
                            break;

                        case 10:    // Bantha - unused
                            colour = new Color32(34, 51, 34, mapAlphaChannel);
                            break;

                        case 11:    // Dak'fron
                            colour = new Color32(36, 116, 84, mapAlphaChannel);
                            break;

                        case 12:    // The Islands in the Western Iliac Bay - unused
                            colour = new Color32(36, 116, 84, mapAlphaChannel);
                            break;

                        case 13:    // Tamarilyn Point - unused
                            colour = new Color32(36, 116, 84, mapAlphaChannel);
                            break;

                        case 14:    // Lainlyn Cliffs - unused
                            colour = new Color32(36, 116, 84, mapAlphaChannel);
                            break;

                        case 15:    // Bjoulsae River - unused
                            colour = new Color32(36, 116, 84, mapAlphaChannel);
                            break;

                        case 16:    // The Wrothgarian Mountains
                            colour = new Color32(250, 201, 11, mapAlphaChannel);
                            break;

                        case 17:    // Daggerfall
                            colour = new Color32(0, 126, 13, mapAlphaChannel);
                            break;

                        case 18:    // Glenpoint
                            colour = new Color32(152, 152, 152, mapAlphaChannel);
                            break;

                        case 19:    // Betony
                            colour = new Color32(31, 55, 132, mapAlphaChannel);
                            break;

                        case 20:    // Sentinel
                            colour = new Color32(158, 134, 17, mapAlphaChannel);
                            break;

                        case 21:    // Anticlere
                            colour = new Color32(30, 30, 30, mapAlphaChannel);
                            break;

                        case 22:    // Lainlyn
                            colour = new Color32(38, 127, 0, mapAlphaChannel);
                            break;

                        case 23:    // Wayrest
                            colour = new Color32(0, 248, 255, mapAlphaChannel);
                            break;

                        case 24:    // Gen Tem High Rock village - unused
                            colour = new Color32(158, 134, 17, mapAlphaChannel);
                            break;

                        case 25:    // Gen Rai Hammerfell village - unused
                            colour = new Color32(158, 134, 17, mapAlphaChannel);
                            break;

                        case 26:    // The Orsinium Area
                            colour = new Color32(0, 99, 46, mapAlphaChannel);
                            break;

                        case 27:    // Skeffington Wood - unused
                            colour = new Color32(0, 99, 46, mapAlphaChannel);
                            break;

                        case 28:    // Hammerfell bay coast - unused
                            colour = new Color32(0, 99, 46, mapAlphaChannel);
                            break;

                        case 29:    // Hammerfell sea coast - unused
                            colour = new Color32(0, 99, 46, mapAlphaChannel);
                            break;

                        case 30:    // High Rock bay coast - unused
                            colour = new Color32(0, 99, 46, mapAlphaChannel);
                            break;

                        case 31:    // High Rock sea coast
                            colour = new Color32(0, 0, 0, 0);
                            break;

                        case 32:    // Northmoor
                            colour = new Color32(127, 127, 127, mapAlphaChannel);
                            break;

                        case 33:    // Menevia
                            colour = new Color32(229, 115, 39, mapAlphaChannel);
                            break;

                        case 34:    // Alcaire
                            colour = new Color32(238, 90, 0, mapAlphaChannel);
                            break;

                        case 35:    // Koegria
                            colour = new Color32(0, 83, 165, mapAlphaChannel);
                            break;

                        case 36:    // Bhoriane
                            colour = new Color32(255, 124, 237, mapAlphaChannel);
                            break;

                        case 37:    // Kambria
                            colour = new Color32(0, 19, 127, mapAlphaChannel);
                            break;

                        case 38:    // Phrygias
                            colour = new Color32(81, 46, 26, mapAlphaChannel);
                            break;

                        case 39:    // Urvaius
                            colour = new Color32(12, 12, 12, mapAlphaChannel);
                            break;

                        case 40:    // Ykalon
                            colour = new Color32(87, 0, 127, mapAlphaChannel);
                            break;

                        case 41:    // Daenia
                            colour = new Color32(32, 142, 142, mapAlphaChannel);
                            break;

                        case 42:    // Shalgora
                            colour = new Color32(202, 0, 0, mapAlphaChannel);
                            break;

                        case 43:    // Abibon-Gora
                            colour = new Color32(142, 74, 173, mapAlphaChannel);
                            break;

                        case 44:    // Kairou
                            colour = new Color32(68, 27, 0, mapAlphaChannel);
                            break;

                        case 45:    // Pothago
                            colour = new Color32(207, 20, 43, mapAlphaChannel);
                            break;

                        case 46:    // Myrkwasa
                            colour = new Color32(119, 108, 59, mapAlphaChannel);
                            break;

                        case 47:    // Ayasofya
                            colour = new Color32(74, 35, 1, mapAlphaChannel);
                            break;

                        case 48:    // Tigonus
                            colour = new Color32(255, 127, 127, mapAlphaChannel);
                            break;

                        case 49:    // Kozanset
                            colour = new Color32(127, 127, 127, mapAlphaChannel);
                            break;

                        case 50:    // Satakalaam
                            colour = new Color32(255, 46, 0, mapAlphaChannel);
                            break;

                        case 51:    // Totambu
                            colour = new Color32(193, 77, 0, mapAlphaChannel);
                            break;

                        case 52:    // Mournoth
                            colour = new Color32(153, 28, 0, mapAlphaChannel);
                            break;

                        case 53:    // Ephesus
                            colour = new Color32(253, 103, 0, mapAlphaChannel);
                            break;

                        case 54:    // Santaki
                            colour = new Color32(1, 255, 144, mapAlphaChannel);
                            break;

                        case 55:    // Antiphyllos
                            colour = new Color32(229, 182, 64, mapAlphaChannel);
                            break;

                        case 56:    // Bergama
                            colour = new Color32(196, 169, 37, mapAlphaChannel);
                            break;

                        case 57:    // Gavaudon
                            colour = new Color32(240, 8, 47, mapAlphaChannel);
                            break;

                        case 58:    // Tulune
                            colour = new Color32(0, 73, 126, mapAlphaChannel);
                            break;

                        case 59:    // Glenumbra Moors
                            colour = new Color32(15, 0, 61, mapAlphaChannel);
                            break;

                        case 60:    // Ilessan Hills
                            colour = new Color32(236, 42, 50, mapAlphaChannel);
                            break;

                        case 61:    // Cybiades
                            colour = new Color32(255, 255, 255, mapAlphaChannel);
                            break;

                        case -1:
                        default:
                            colour = new Color32(0, 0, 0, 0);
                            break;
                    }

                    regionBuffer[(height - 1 - y) * width + x] = colour;
                }
            }
            
            regionTexture.SetPixels32(regionBuffer);
            regionTexture.Apply();
            regionPanel.BackgroundTexture = regionTexture;
        }

        protected virtual void UpdateCrosshair()
        {
            if (FindingLocation)
                UpdateIdentifyTextureForPosition(MapsFile.GetPixelFromPixelID(locationSummary.ID), locationSummary.RegionIndex);
            else
                UpdateIdentifyTextureForPosition(TravelTimeCalculator.GetPlayerTravelPosition());
        }

        protected virtual void UpdateIdentifyTextureForPosition(DFPosition pos, int regionIndex = -1)
        {
            if (regionIndex == -1)
                regionIndex = GetPlayerRegion();
            UpdateIdentifyTextureForPosition(pos.X, pos.Y, regionIndex);
        }

        // Set location crosshair for identify overlay
        protected virtual void UpdateIdentifyTextureForPosition(int mapPixelX, int mapPixelY, int regionIndex)
        {
            // Only for regions
            // if (!RegionSelected)
            //     return;

            int zoomedWidth = dynamicMapWidth * zoomfactor;
            int zoomedHeight = dynamicMapHeight * zoomfactor;

            if (mapPixelX < mapCenter.X - (zoomedWidth / 2) || mapPixelX > mapCenter.X + (zoomedWidth / 2) ||
                mapPixelY < mapCenter.Y - (zoomedHeight / 2) || mapPixelY > mapCenter.Y + (zoomedHeight / 2))
            {
                mapCenter = new DFPosition(mapPixelX, mapPixelY);
                JustifyMapCenter();
                SetupTravelMap();
                UpdateMapLocationDotsTexture();
                UpdateRegionAreaTexture();
                UpdateClimateAreaTexture();
            }

            identifyTexture = new Texture2D(zoomedWidth, zoomedHeight);

            // Clear existing pixel buffer
            identifyPixelBuffer = new Color32[zoomedWidth * zoomedHeight];
            Array.Clear(identifyPixelBuffer, 0, identifyPixelBuffer.Length);

            // float scale = GetRegionMapScale(regionIndex);

            for (int x = 0; x < zoomedWidth; x++)
            {
                for (int y = 0; y < zoomedHeight; y++)
                {
                    if (x == (zoomedWidth / 2) + (mapPixelX - mapCenter.X) || y == (zoomedHeight / 2) + (mapPixelY - mapCenter.Y))
                    {
                        identifyPixelBuffer[(zoomedHeight - y - 1) * zoomedWidth + x] = identifyFlashColor;
                    }
                }
            }
            identifyTexture.SetPixels32(identifyPixelBuffer);
            identifyTexture.Apply();
            identifyOverlayPanel.BackgroundTexture = identifyTexture;
        }

        #endregion

        #region Event Handlers

        // Handle clicks on the main panel
        protected virtual void ClickHandler(BaseScreenComponent sender, Vector2 position)
        {
            // position.y -= regionPanelOffset;

            // Ensure clicks are inside region texture
            if (position.x < 0 || position.x > regionTextureOverlayPanelRect.width || position.y < 0 || position.y > regionTextureOverlayPanelRect.height)
                return;

            // if (RegionSelected == false)
            // {
            //     if (MouseOverRegion)
            //         OpenRegionPanel(mouseOverRegion);
            // }
            else if (locationSelected)
            {
                if (FindingLocation)
                    StopIdentify(true);
                else
                    CreatePopUpWindow();
            }
            else if (MouseOverOtherRegion)
            {
                // If clicked while mouse over other region & not a location, switch to that region
                // OpenRegionPanel(mouseOverRegion);
                // CreateDynamicTravelMap();
            }
        }

        protected virtual void ExitButtonClickHandler(BaseScreenComponent sender, Vector2 position)
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            CloseTravelWindows();
        }

        protected virtual void AtButtonClickHandler(BaseScreenComponent sender, Vector2 position)
        {
            // Identify region or map location
            findingLocation = false;
            StartIdentify();
            UpdateCrosshair();
        }

        protected virtual void FindlocationButtonClickHandler(BaseScreenComponent sender, Vector2 position)
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            DaggerfallInputMessageBox findPopUp = new DaggerfallInputMessageBox(uiManager, null, 31, TextManager.Instance.GetLocalizedText("findLocationPrompt"), true, this);
            findPopUp.TextPanelDistanceY = 5;
            findPopUp.TextBox.WidthOverride = 308;
            findPopUp.TextBox.MaxCharacters = 32;
            findPopUp.OnGotUserInput += HandleLocationFindEvent;
            findPopUp.Show();
        }

        /// <summary>
        /// Button handler for travel confirmation pop up. This is a temporary solution until implementing the final pop-up.
        /// </summary>
        protected virtual void ConfirmTravelPopupButtonClick(DaggerfallMessageBox sender, DaggerfallMessageBox.MessageBoxButtons messageBoxButton)
        {
            DaggerfallUI.Instance.PlayOneShot(SoundClips.ButtonClick);
            sender.CloseWindow();
            if (messageBoxButton == DaggerfallMessageBox.MessageBoxButtons.Yes)
                CreatePopUpWindow();
            else
                StopIdentify();
        }

        /// <summary>
        /// Handles click events for the arrow buttons in the region view
        /// </summary>
        protected virtual void ArrowButtonClickHandler(BaseScreenComponent sender, Vector2 position)
        {
            // if (RegionSelected == false || !HasMultipleMaps)
            //     return;
            // int newIndex = mapIndex;

            if (sender.Name == "upArrowButton" && zoomfactor < maxZoomFactor)
            {
                zoomfactor += 1;
            }
            else if (sender.Name == "downArrowButton" && zoomfactor > 1)
            {
                zoomfactor -= 1;
            }
            else
            {
                return;
            }

            // SetupArrowButtons();
            JustifyMapCenter();
            SetupTravelMap();
            SetupPixelBuffer();
            SetupRegionBuffer();
            SetupClimateBuffer();
            UpdateMapLocationDotsTexture();
            UpdateRegionAreaTexture();
            UpdateClimateAreaTexture();
            // UpdateCrosshair();
        }

        /// <summary>
        /// Handles click events for the filter buttons in the region view
        /// </summary>
        protected virtual void FilterButtonClickHandler(BaseScreenComponent sender, Vector2 position)
        {
            if (sender.Name == "dungeonsFilterButton")
            {
                filterDungeons = !filterDungeons;
            }
            else if (sender.Name == "templesFilterButton")
            {
                filterTemples = !filterTemples;
            }
            else if (sender.Name == "homesFilterButton")
            {
                filterHomes = !filterHomes;
            }
            else if (sender.Name == "townsFilterButton")
            {
                filterTowns = !filterTowns;
            }
            else
            {
                return;
            }

            if (filterDungeons)
                dungeonsFilterButton.BackgroundTexture = dungeonFilterButtonDisabled;
            else
                dungeonsFilterButton.BackgroundTexture = dungeonFilterButtonEnabled;
            if (filterTemples)
                templesFilterButton.BackgroundTexture = templesFilterButtonDisabled;
            else
                templesFilterButton.BackgroundTexture = templesFilterButtonEnabled;
            if (filterHomes)
                homesFilterButton.BackgroundTexture = homesFilterButtonDisabled;
            else
                homesFilterButton.BackgroundTexture = homesFilterButtonEnabled;
            if (filterTowns)
                townsFilterButton.BackgroundTexture = townsFilterButtonDisabled;
            else
                townsFilterButton.BackgroundTexture = townsFilterButtonEnabled;

            UpdateMapLocationDotsTexture();
        }

        protected virtual void ToggleButtonClickHandler(BaseScreenComponent sender, Vector2 position)
        {
            if (sender.Name == "regionToggleButton")
            {
                regionToggle = !regionToggle;
                
                if (regionToggle)
                    climateToggle = false;
            }
            else if (sender.Name == "climateToggleButton")
            {
                climateToggle = !climateToggle;

                if (climateToggle)
                    regionToggle = false;
            }
            else
            {
                return;
            }

            if (regionToggle)
            {
                UpdateClimateAreaTexture();
                UpdateRegionAreaTexture();
                climatePanel.Enabled = false;
                regionPanel.Enabled = true;
            }
            else if (climateToggle)
            {
                UpdateRegionAreaTexture();
                UpdateClimateAreaTexture();
                regionPanel.Enabled = false;
                climatePanel.Enabled = true;
            }
            else
            {
                UpdateRegionAreaTexture();
                UpdateClimateAreaTexture();
                regionPanel.Enabled = false;
                climatePanel.Enabled = false;
            }
        }

        #endregion

        #region Private Methods

        // Check if location with MapSummary summary is already discovered
        protected virtual bool checkLocationDiscovered(MapSummary summary)
        {
            // Check location MapTableData.Discovered flag in world replacement data then cached MAPS.BSA data
            DFLocation location;
            bool discovered = false;

            if (WorldDataReplacement.GetDFLocationReplacementData(summary.RegionIndex, summary.MapIndex, out location))
                discovered = location.MapTableData.Discovered;

            return GameManager.Instance.PlayerGPS.HasDiscoveredLocation(summary.ID) || discovered || revealUndiscoveredLocations == true;
        }

        // Check if place is discovered, so it can be found on map.
        public bool CanFindPlace(string regionName, string name)
        {
            DFLocation location;
            if (WorldMaps.GetLocation(regionName, name, out location))
            {
                DFPosition mapPixel = MapsFile.LongitudeLatitudeToMapPixel(location.MapTableData.Longitude, location.MapTableData.Latitude);
                MapSummary summary;
                if (WorldMaps.HasLocation(mapPixel.X, mapPixel.Y, out summary))
                    return checkLocationDiscovered(summary);
            }
            return false;
        }

        protected Vector2 GetCoordinates()
        {
            // string mapName = selectedRegionMapNames[mapIndex];
            Vector2 origin = new Vector2(mapCenter.X - (dynamicMapWidth * zoomfactor) / 2, mapCenter.Y - (dynamicMapHeight * zoomfactor) / 2);
            int height = dynamicMapHeight * zoomfactor;

            Vector2 results = Vector2.zero;
            Vector2 pos = (NativePanel.ScaledMousePosition / (8.0f / zoomfactor));

            results.x = (int)Math.Floor(pos.x + origin.x);
            results.y = (int)Math.Floor(origin.y + pos.y);

            return results;
        }


        // Check if player mouse over valid location while region selected & not finding location
        protected virtual void UpdateMouseOverLocation()
        {
            if (FindingLocation)
                return;

            locationSelected = false;


            if (lastMousePos.x < 0 ||
                lastMousePos.x > 320 ||
                lastMousePos.y < 0 ||
                lastMousePos.y > 200)
                return;

            // float scale = GetRegionMapScale(selectedRegion);
            Vector2 coordinates = GetCoordinates();
            int x = (int)(coordinates.x);
            int y = (int)(coordinates.y);

            int sampleRegion = PoliticData.ConvertMapPixelToRegionIndex(x, y);
            if (sampleRegion != 64)
            {
                mouseOverRegion = sampleRegion;

                currentDFRegion = WorldMaps.ConvertWorldMapsToDFRegion(sampleRegion);

                // if (sampleRegion >= 0 && sampleRegion < MapsFile.TempRegionCount)
                // {
                //     mouseOverRegion = sampleRegion;
                //     return;
                // }

                if (WorldMaps.HasLocation(x, y))
                {
                    WorldMaps.HasLocation(x, y, out locationSummary);

                    if (locationSummary.MapIndex < 0 || locationSummary.MapIndex >= currentDFRegion.MapNames.Length)
                        return;
                    else
                    {
                        int index = GetPixelColorIndex(locationSummary.LocationType);
                        if (index == -1)
                            return;

                        // Only make location selectable if it is already discovered
                        if (!checkLocationDiscovered(locationSummary))
                            return;

                        locationSelected = true;
                    }
                }
            }
        }

        // Check if mouse over a region
        protected virtual void UpdateMouseOverRegion()
        {
            mouseOverRegion = -1;

            Vector2 pos = GetCoordinates();
            int x = (int)pos.x;
            int y = (int)pos.y;

            int sampleRegion = PoliticData.ConvertMapPixelToRegionIndex(x, y);
            if (sampleRegion == 64)
            {
                return;
            }

            mouseOverRegion = sampleRegion;
        }

        // Updates the text label at top of screen
        protected virtual void UpdateRegionLabel()
        {
            if (FindingLocation)
                return;

            if (locationSelected)
            {
                regionLabel.Text = string.Format("{0} : {1}", GetRegionName(mouseOverRegion), GetLocationNameInCurrentRegion(locationSummary.MapIndex, true));
            }
            // else if (MouseOverOtherRegion)
            //     regionLabel.Text = string.Format(TextManager.Instance.GetLocalizedText("switchToRegion"), GetRegionName(mouseOverRegion));
            else
            {
                Vector2 mousePos = GetCoordinates();
                string pos = (mousePos.x + ", " + mousePos.y);
                regionLabel.Text = string.Format("{0}, {1}", mousePos.x, mousePos.y);
            }
        }

        // Closes windows based on context
        public void CloseTravelWindows(bool forceClose = false)
        {
            if (forceClose)
                CloseWindow();
            else
                return;
        }

        // Updates search button toggle state based on current flags
        protected virtual void UpdateSearchButtons()
        {
            // Dungeons
            if (!filterDungeons)
                dungeonsFilterButton.BackgroundTexture = dungeonFilterButtonEnabled;
            else
                dungeonsFilterButton.BackgroundTexture = dungeonFilterButtonDisabled;

            // Temples
            if (!filterTemples)
                templesFilterButton.BackgroundTexture = templesFilterButtonEnabled;
            else
                templesFilterButton.BackgroundTexture = templesFilterButtonDisabled;

            // Homes
            if (!filterHomes)
                homesFilterButton.BackgroundTexture = homesFilterButtonEnabled;
            else
                homesFilterButton.BackgroundTexture = homesFilterButtonDisabled;

            // Towns
            if (!filterTowns)
                townsFilterButton.BackgroundTexture = townsFilterButtonEnabled;
            else
                townsFilterButton.BackgroundTexture = townsFilterButtonDisabled;
        }

        public TravelMapSaveData GetTravelMapSaveData()
        {
            TravelMapSaveData data = new TravelMapSaveData();
            data.filterDungeons = filterDungeons;
            data.filterHomes = filterHomes;
            data.filterTemples = filterTemples;
            data.filterTowns = filterTowns;

            if (popUp != null)
            {
                data.sleepInn = popUp.SleepModeInn;
                data.speedCautious = popUp.SpeedCautious;
                data.travelShip = popUp.TravelShip;
            }

            return data;
        }

        public void SetTravelMapFromSaveData(TravelMapSaveData data)
        {
            // If doesn't have save data, use defaults
            if (data == null)
                data = new TravelMapSaveData();

            filterDungeons = data.filterDungeons;
            filterHomes = data.filterHomes;
            filterTemples = data.filterTemples;
            filterTowns = data.filterTowns;

            if (popUp == null)
            {
                popUp = (DaggerfallTravelPopUp)UIWindowFactory.GetInstanceWithArgs(UIWindowType.TravelPopUp, new object[] { uiManager, this, this });
            }

            popUp.SleepModeInn = data.sleepInn;
            popUp.SpeedCautious = data.speedCautious;
            popUp.TravelShip = data.travelShip;

            UpdateSearchButtons();
        }
        #endregion

        #region Helper Methods

        // Get index to locationPixelColor array or -1 if invalid or filtered
        protected virtual int GetPixelColorIndex(DFRegion.LocationTypes locationType)
        {
            int index = -1;
            switch (locationType)
            {
                case DFRegion.LocationTypes.DungeonLabyrinth:
                    index = 0;
                    break;
                case DFRegion.LocationTypes.DungeonKeep:
                    index = 1;
                    break;
                case DFRegion.LocationTypes.DungeonRuin:
                    index = 2;
                    break;
                case DFRegion.LocationTypes.Graveyard:
                    index = 3;
                    break;
                case DFRegion.LocationTypes.Coven:
                    index = 4;
                    break;
                case DFRegion.LocationTypes.HomeFarms:
                    index = 5;
                    break;
                case DFRegion.LocationTypes.HomeWealthy:
                    index = 6;
                    break;
                case DFRegion.LocationTypes.HomePoor:
                    index = 7;
                    break;
                case DFRegion.LocationTypes.HomeYourShips:
                    break;
                case DFRegion.LocationTypes.ReligionTemple:
                    index = 8;
                    break;
                case DFRegion.LocationTypes.ReligionCult:
                    index = 9;
                    break;
                case DFRegion.LocationTypes.Tavern:
                    index = 10;
                    break;
                case DFRegion.LocationTypes.TownCity:
                    index = 11;
                    break;
                case DFRegion.LocationTypes.TownHamlet:
                    index = 12;
                    break;
                case DFRegion.LocationTypes.TownVillage:
                    index = 13;
                    break;
                default:
                    break;
            }
            if (index < 0)
                return index;
            else if (index < 5 && filterDungeons)
                index = -1;
            else if (index > 4 && index < 8 && filterHomes)
                index = -1;
            else if (index > 7 && index < 10 && filterTemples)
                index = -1;
            else if (index > 9 && index < 14 && filterTowns)
                index = -1;
            return index;
        }

        // Handles events from Find Location pop-up.
        protected virtual void HandleLocationFindEvent(DaggerfallInputMessageBox inputMessageBox, string locationName)
        {
            List<DistanceMatch> matching;
            if (FindLocation(locationName, out matching))
            {
                if (matching.Count == 1)
                { //place flashing crosshair over location
                    locationSelected = true;
                    findingLocation = true;
                    StartIdentify();
                    UpdateCrosshair();
                }
                else
                {
                    ShowLocationPicker(matching.ConvertAll(match => match.text).ToArray(), false);
                }
            }
            else
            {
                TextFile.Token[] textTokens = DaggerfallUnity.Instance.TextProvider.GetRSCTokens(13);
                DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, this);
                messageBox.SetTextTokens(textTokens);
                messageBox.ClickAnywhereToClose = true;
                uiManager.PushWindow(messageBox);
                return;
            }
        }

        // Get localized names of all locations in current region with fallback to canonical name
        // Builds a new name lookup dictionary for this region on every call used to complete search
        protected string[] GetCurrentRegionLocalizedMapNames()
        {
            localizedMapNameLookup.Clear();
            List<string> localizedNames = new List<string>(currentDFRegion.MapNames.Length);
            for (int l = 0; l < currentDFRegion.MapNames.Length; l++)
            {
                // Handle duplicate names in same way as Region.MapNameLookup
                string name = TextManager.Instance.GetLocalizedLocationName(currentDFRegion.MapTable[l].MapId, currentDFRegion.MapNames[l]);
                if (!localizedNames.Contains(name))
                {
                    localizedNames.Add(name);
                    localizedMapNameLookup.Add(name, l);
                }
            }
            return localizedNames.ToArray();
        }

        // Find location by name
        protected virtual bool FindLocation(string name, out List<DistanceMatch> matching)
        {
            matching = new List<DistanceMatch>();
            if (string.IsNullOrEmpty(name))
            {
                return false;
            }

            if (distanceRegionName != currentDFRegion.Name)
            {
                distanceRegionName = currentDFRegion.Name;
                distance = DaggerfallDistance.GetDistance();
                distance.SetDictionary(GetCurrentRegionLocalizedMapNames());
            }

            DistanceMatch[] bestMatches = distance.FindBestMatches(name, maxMatchingResults);

            // Check if selected locations actually exist/are visible
            MatchesCutOff cutoff = null;
            MapSummary findLocationSummary;

            foreach (DistanceMatch match in bestMatches)
            {
                // Must have called GetCurrentRegionLocalizedMapNames() prior to this point
                if (!localizedMapNameLookup.ContainsKey(match.text))
                {
                    Debug.LogWarningFormat("Error: location name '{0}' key not found in localizedMapNameLookup dictionary for this region.", match.text);
                    continue;
                }
                int index = localizedMapNameLookup[match.text];
                DFRegion.RegionMapTable locationInfo = currentDFRegion.MapTable[index];
                DFPosition pos = MapsFile.LongitudeLatitudeToMapPixel((int)locationInfo.Longitude, (int)locationInfo.Latitude);
                if (WorldMaps.HasLocation(pos.X, pos.Y, out findLocationSummary))
                {
                    // only make location searchable if it is already discovered
                    if (!checkLocationDiscovered(findLocationSummary))
                        continue;

                    if (cutoff == null)
                    {
                        cutoff = new MatchesCutOff(match.relevance);

                        // Set locationSummary to first result's MapSummary in case we skip the location list picker step
                        locationSummary = findLocationSummary;
                    }
                    else
                    {
                        if (!cutoff.Keep(match.relevance))
                            break;
                    }
                    matching.Add(match);
                }
            }

            return matching.Count > 0;
        }

        private class MatchesCutOff
        {
            private readonly float threshold;

            public MatchesCutOff(float bestRelevance)
            {
                // If perfect match exists, return all perfect matches only
                // Normally there should be only one perfect match, but if string canonization generates collisions that's no longer guaranteed
                threshold = bestRelevance == 1f ? 1f : bestRelevance * 0.5f;
            }

            public bool Keep(float relevance)
            {
                return relevance >= threshold;
            }
        }

        // Creates a ListPickerWindow with a list of locations from current region
        // Locations displayed will be filtered out depending on the dungeon / town / temple / home button settings
        private void ShowLocationPicker(string[] locations, bool applyFilters)
        {
            DaggerfallListPickerWindow locationPicker = new DaggerfallListPickerWindow(uiManager, this);
            locationPicker.OnItemPicked += HandleLocationPickEvent;
            locationPicker.ListBox.MaxCharacters = 29;

            for (int i = 0; i < locations.Length; i++)
            {
                if (applyFilters)
                {
                    // Must have called GetCurrentRegionLocalizedMapNames() prior to this point
                    int index = localizedMapNameLookup[locations[i]];
                    if (GetPixelColorIndex(currentDFRegion.MapTable[index].LocationType) == -1)
                        continue;
                }
                locationPicker.ListBox.AddItem(locations[i]);
            }

            uiManager.PushWindow(locationPicker);
        }

        public void HandleLocationPickEvent(int index, string locationName)
        {
            if (!RegionSelected || currentDFRegion.LocationCount < 1)
                return;

            CloseWindow();
            HandleLocationFindEvent(null, locationName);
        }

        // Gets current player region or -1 if player not in any region (e.g. in ocean)
        protected int GetPlayerRegion()
        {
            DFPosition position = TravelTimeCalculator.GetPlayerTravelPosition();
            int region = PoliticData.ConvertMapPixelToRegionIndex(position.X, position.Y);
            if (region < 0 || region >= MapsFile.TempRegionCount)
                return -1;

            return region;
        }

        // Gets name of region
        protected string GetRegionName(int region)
        {
            return TextManager.Instance.GetLocalizedRegionName(region);
        }

        protected string GetRegionNameForMapReplacement(int region)
        {
            return WorldMaps.WorldMap[region].Name; // Using non-localized name for map replacement path
        }

        // Gets name of location in currently open region - tries world data replacement then falls back to MAPS.BSA
        protected virtual string GetLocationNameInCurrentRegion(int locationIndex, bool cacheName = false)
        {
            // Must have a region open
            // if (currentDFRegionIndex == -1)
            //     return string.Empty;

            // Cache the last location index when requested and only update it when index changes
            if (cacheName && lastQueryLocationIndex == locationIndex)
                return lastQueryLocationName;

            // Localized name has first priority if one exists
            string localizedName = TextManager.Instance.GetLocalizedLocationName(locationSummary.MapID, string.Empty);
            if (!string.IsNullOrEmpty(localizedName))
                return localizedName;

            // Get location name from world data replacement if available or fall back to MAPS.BSA cached names
            DFLocation location;
            if (WorldDataReplacement.GetDFLocationReplacementData(currentDFRegionIndex, locationIndex, out location))
            {
                lastQueryLocationName = location.Name;
                lastQueryLocationIndex = locationIndex;
                return location.Name;
            }
            else
            {
                return currentDFRegion.MapNames[locationSummary.MapIndex];
            }
        }

        protected virtual void CreateConfirmationPopUp()
        {
            const int doYouWishToTravelToTextId = 31;

            if (!locationSelected)
                return;

            // Get text tokens
            TextFile.Token[] textTokens = DaggerfallUnity.Instance.TextProvider.GetRSCTokens(doYouWishToTravelToTextId);

            // Hack to set location name in text token for now
            textTokens[2].text = textTokens[2].text.Replace(
                "%tcn",
                TextManager.Instance.GetLocalizedLocationName(locationSummary.MapID, GetLocationNameInCurrentRegion(locationSummary.MapIndex)));

            DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, this);
            messageBox.SetTextTokens(textTokens);
            messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.Yes);
            messageBox.AddButton(DaggerfallMessageBox.MessageBoxButtons.No);
            messageBox.OnButtonClick += ConfirmTravelPopupButtonClick;
            uiManager.PushWindow(messageBox);
        }

        protected virtual void CreatePopUpWindow()
        {
            DFPosition pos = MapsFile.GetPixelFromPixelID(locationSummary.ID);
            if (teleportationTravel)
            {
                DaggerfallTeleportPopUp telePopup = (DaggerfallTeleportPopUp)UIWindowFactory.GetInstanceWithArgs(UIWindowType.TeleportPopUp, new object[] { uiManager, uiManager.TopWindow, this });
                telePopup.DestinationPos = pos;
                telePopup.DestinationName = GetLocationNameInCurrentRegion(locationSummary.MapIndex);
                uiManager.PushWindow(telePopup);
            }
            else
            {
                if (popUp == null)
                {
                    popUp = (DaggerfallTravelPopUp)UIWindowFactory.GetInstanceWithArgs(UIWindowType.TravelPopUp, new object[] { uiManager, uiManager.TopWindow, this });
                }
                popUp.EndPos = pos;
                uiManager.PushWindow(popUp);
            }
        }

        protected virtual void JustifyMapCenter()
        {
            if ((mapCenter.X - (dynamicMapWidth * zoomfactor / 2)) < 0)
                mapCenter.X = dynamicMapWidth * zoomfactor / 2;
            else if ((mapCenter.X + (dynamicMapWidth * zoomfactor / 2)) > MapsFile.WorldWidth)
                mapCenter.X = MapsFile.WorldWidth - 1 - (dynamicMapWidth * zoomfactor / 2);

            if ((mapCenter.Y - (dynamicMapHeight * zoomfactor / 2)) < 0)
                mapCenter.Y = dynamicMapHeight * zoomfactor / 2;
            else if ((mapCenter.Y + (dynamicMapHeight * zoomfactor / 2)) > MapsFile.WorldHeight)
                mapCenter.Y = MapsFile.WorldHeight - 1 - (dynamicMapHeight * zoomfactor / 2);
        }

        protected virtual void SetupTravelMap()
        {
            selectedRegion = GetPlayerRegion();
            overworldTexture = new Texture2D(dynamicMapWidth * zoomfactor, dynamicMapHeight * zoomfactor, TextureFormat.ARGB32, false);
            overworldTexture.filterMode = FilterMode.Point;
            dynamicTravelMap = new Color32[(dynamicMapWidth * zoomfactor) * (dynamicMapHeight * zoomfactor)];
            dynamicTravelMap = CreateDynamicTravelMap();
            overworldTexture.SetPixels32(dynamicTravelMap);
            overworldTexture.Apply();
            NativePanel.BackgroundTexture = overworldTexture;
            NativePanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
        }

        protected virtual Color32[] CreateDynamicTravelMap()
        {
            int zoomedWidth = dynamicMapWidth * zoomfactor;
            int zoomedHeight = dynamicMapHeight * zoomfactor;

            int actualPolitic = PoliticData.Politic[mapCenter.X, mapCenter.Y];
            Color32[] colours = new Color32[zoomedWidth * zoomedHeight];

            for (int x = 0; x < zoomedWidth; x++)
            {
                for (int y = 0; y < zoomedHeight; y++)
                {
                    DFPosition area = new DFPosition(mapCenter.X + x - (zoomedWidth / 2), mapCenter.Y + y - (zoomedHeight / 2));
                    byte value = WoodsData.GetHeightMapValue(area.X, area.Y);
                    int terrain;
                    Color32 colour;
                    MapSummary location;

                    if (value < 3)
                        terrain = -1;

                    // else if (PoliticData.IsBorderPixel(area.X, area.Y, actualPolitic))
                    //     terrain = -2;

                    else terrain = (value / 10);

                    switch (terrain)
                    {
                        case -2:
                            colour = new Color32(241, 238, 45, 255);
                            break;

                        case -1:
                            colour = new Color32(40, 71, 166, 255);
                            break;

                        case 0:
                            colour = new Color32(175, 200, 168, 255);
                            break;

                        case 1:
                            colour = new Color32(148, 176, 141, 255);
                            break;

                        case 2:
                            colour = new Color32(123, 156, 118, 255);
                            break;

                        case 3:
                            colour = new Color32(107, 144, 109, 255);
                            break;

                        case 4:
                            colour = new Color32(93, 130, 94, 255);
                            break;

                        case 5:
                            colour = new Color32(82, 116, 86, 255);
                            break;

                        case 6:
                            colour = new Color32(77, 110, 78, 255);
                            break;

                        case 7:
                            colour = new Color32(68, 99, 67, 255);
                            break;

                        case 8:
                            colour = new Color32(61, 89, 53, 255);
                            break;

                        case 9:
                            colour = new Color32(52, 77, 45, 255);
                            break;

                        case 10:
                            colour = new Color32(34, 51, 34, 255);
                            break;

                        default:
                            colour = new Color32(40, 47, 40, 255);
                            break;
                    }
                    colours[(zoomedHeight - 1 - y) * zoomedWidth + x] = colour;
                }
            }

            return colours;
        }

        #endregion


        #region Region Identification

        // Start region identification & location crosshair
        void StartIdentify()
        {
            // Stop animation
            if (identifying)
                StopIdentify(false);

            identifying = true;
            identifyState = false;
            identifyChanges = 0;
            identifyLastChangeTime = 0;
        }

        // Stop region identification & location crosshair
        void StopIdentify(bool createPopUp = true)
        {
            if (FindingLocation && createPopUp)
                CreateConfirmationPopUp();

            identifying = false;
            identifyState = false;
            identifyChanges = 0;
            identifyLastChangeTime = 0;
        }

        // Animate region identification & location crosshair
        void AnimateIdentify()
        {
            if (!identifying)
                return;

            // Check if enough time has elapsed since last flash and toggle state
            bool lastIdentifyState = identifyState;
            float time = Time.realtimeSinceStartup;

            if (time > identifyLastChangeTime + identifyFlashInterval)
            {
                identifyState = !identifyState;
                identifyLastChangeTime = time;
            }

            // Turn off flash after specified number of on states
            if (!lastIdentifyState && identifyState)
            {
                int flashCount = locationSelected ? identifyFlashCountSelected : identifyFlashCount;
                if (++identifyChanges > flashCount)
                {
                    StopIdentify();
                }
            }
        }


        #endregion

        #region console_commands

        public static class TravelMapConsoleCommands
        {
            public static void RegisterCommands()
            {
                try
                {
                    ConsoleCommandsDatabase.RegisterCommand(RevealLocations.name, RevealLocations.description, RevealLocations.usage, RevealLocations.Execute);
                    ConsoleCommandsDatabase.RegisterCommand(HideLocations.name, HideLocations.description, HideLocations.usage, HideLocations.Execute);
                    ConsoleCommandsDatabase.RegisterCommand(RevealLocation.name, RevealLocation.description, RevealLocation.usage, RevealLocation.Execute);
                }
                catch (System.Exception ex)
                {
                    DaggerfallUnity.LogMessage(ex.Message, true);
                }
            }

            private static class RevealLocations
            {
                public static readonly string name = "map_reveallocations";
                public static readonly string description = "Reveals undiscovered locations on travelmap (temporary)";
                public static readonly string usage = "map_reveallocations";


                public static string Execute(params string[] args)
                {
                    if (GameManager.Instance.IsPlayerInside)
                    {
                        return "this command only has an effect when outside";
                    }

                    revealUndiscoveredLocations = true;
                    return "undiscovered locations have been revealed (temporary) on the travelmap";
                }
            }

            private static class HideLocations
            {
                public static readonly string name = "map_hidelocations";
                public static readonly string description = "Hides undiscovered locations on travelmap";
                public static readonly string usage = "map_hidelocations";


                public static string Execute(params string[] args)
                {
                    if (GameManager.Instance.IsPlayerInside)
                    {
                        return "this command only has an effect when outside";
                    }

                    revealUndiscoveredLocations = false;
                    return "undiscovered locations have been hidden on the travelmap again";
                }

            }

            private static class RevealLocation
            {
                public static readonly string name = "map_reveallocation";
                public static readonly string error = "Failed to reveal location with given regionName and locatioName on travelmap";
                public static readonly string description = "Permanently reveals the location with [locationName] in region [regionName] on travelmap";
                public static readonly string usage = "map_reveallocation [regionName] [locationName] - inside the name strings use underscores instead of spaces, e.g Dragontail_Mountains";

                public static string Execute(params string[] args)
                {
                    if (args == null || args.Length < 2)
                    {
                        try
                        {
                            Wenzil.Console.Console.Log("please provide both a region name as well as a location name");
                            return HelpCommand.Execute(RevealLocation.name);
                        }
                        catch
                        {
                            return HelpCommand.Execute(RevealLocation.name);
                        }
                    }
                    else
                    {
                        string regionName = args[0];
                        string locationName = args[1];
                        regionName = regionName.Replace("_", " ");
                        locationName = locationName.Replace("_", " ");
                        try
                        {
                            GameManager.Instance.PlayerGPS.DiscoverLocation(regionName, locationName);
                            return String.Format("revealed location {0} : {1} on the travelmap", regionName, locationName);
                        }
                        catch (Exception ex)
                        {
                            return string.Format("Could not reveal location: {0}", ex.Message);
                        }
                    }
                }
            }
        }

        #endregion
    }
}
