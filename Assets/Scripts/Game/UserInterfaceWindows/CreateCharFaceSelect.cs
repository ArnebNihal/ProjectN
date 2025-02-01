// Project:         Daggerfall Unity
// Copyright:       Copyright (C) 2009-2022 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Gavin Clayton (interkarma@dfworkshop.net)
// Contributors:    
// 
// Notes:
//

using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Player;

namespace DaggerfallWorkshop.Game.UserInterfaceWindows
{
    /// <summary>
    /// Implements the select face window.
    /// </summary>
    public class CreateCharFaceSelect : DaggerfallPopupWindow
    {
        const string nativeImgName = "CHAR01I0.IMG";

        Texture2D nativeTexture;
        string nativeTexturePath;
        Button okButton;
        FacePicker facePicker = new FacePicker();

        public int FaceIndex
        {
            get { return facePicker.FaceIndex; }
        }

        public CreateCharFaceSelect(IUserInterfaceManager uiManager)
            : base(uiManager)
        {
        }

        protected override void Setup()
        {
            // Load native texture
            // nativeTexture = DaggerfallUI.GetTextureFromImg(nativeImgName);
            nativeTexture = new Texture2D(1, 1);
            nativeTexturePath = Path.Combine(WorldMaps.texturePath, "Img", nativeImgName + ".png");
            ImageConversion.LoadImage(nativeTexture, File.ReadAllBytes(nativeTexturePath));
            if (!nativeTexture)
                throw new Exception("CreateCharSummary: Could not load native texture.");
            nativeTexture.filterMode = FilterMode.Point;

            // Setup native panel background
            NativePanel.BackgroundTexture = nativeTexture;

            // Add face picker
            NativePanel.Components.Add(facePicker);

            // OK button
            okButton = DaggerfallUI.AddButton(new Rect(263, 172, 39, 22), NativePanel);
            okButton.OnMouseClick += OkButton_OnMouseClick;
        }

        public void SetFaceTextures(RaceTemplate raceTemplate, Genders raceGender)
        {
            facePicker.SetFaceTextures(raceTemplate, raceGender);
        }

        void OkButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            CloseWindow();
        }
    }
}