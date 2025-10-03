// Project:         Daggerfall Unity
// Copyright:       Copyright (C) 2009-2022 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Gavin Clayton (interkarma@dfworkshop.net)
// Contributors:    Arneb
// 
// Notes:
//

using System;
using System.Collections.Generic;
using System.IO;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Utility.AssetInjection;
using UnityEngine;

namespace DaggerfallWorkshop.Game.UserInterfaceWindows
{
    /// <summary>
    /// Implements a Daggerfal popup message box dialog with variable buttons.
    /// Designed to take Daggerfall multiline text resource records.
    /// </summary>
    public class DaggerfallRoadsign : DaggerfallPopupWindow
    {
        const float minTimePresented = 0.0833f;
        
        Panel roadsignPanel = new Panel();
        Panel windrose = new Panel();
        Panel[] arrowPanel = new Panel[8];
        Panel[] messagePanel = new Panel[8];
        MultiFormatTextLabel[] label = new MultiFormatTextLabel[8];
        bool clickAnywhereToClose = true;
        int customYPos = -1;
        float presentationTime = 0;
        float textScale = 1.0f;

        /// <summary>
        /// Change the scale of text inside message box.
        /// Must set custom TextScale immediately after creating messagebox and before setting text/tokens.
        /// </summary>
        public float TextScale
        {
            get { return textScale; }
            set { textScale = value; }
        }

        public enum PanelTypes
        {
            Text,
            Arrow
        }

        public DaggerfallRoadsign(IUserInterfaceManager uiManager, string[][] signData)
            : base(uiManager)
        {
            SetupRoadsign(signData);
        }

        protected override void Setup()
        {
            if (IsSetup)
                return;

            base.Setup();

            allowFreeScaling = false;

            roadsignPanel.Size = new Vector2(320.0f, 200.0f);
            roadsignPanel.HorizontalAlignment = HorizontalAlignment.Center;
            roadsignPanel.VerticalAlignment = VerticalAlignment.Middle;
            DaggerfallUI.Instance.SetDaggerfallPopupStyle(DaggerfallUI.PopupStyle.Parchment, roadsignPanel);
            NativePanel.Components.Add(roadsignPanel);

            windrose.Size = new Vector2(42.0f, 38.0f);
            windrose.Position = new Vector2((roadsignPanel.Size.x / 2 - windrose.Size.x / 2), (roadsignPanel.Size.y / 2 - windrose.Size.y / 2 - 1.0f));
            byte[] fileBytes = File.ReadAllBytes(Path.Combine(WorldMaps.mapPath, "RoadsignData", "WR.png"));
            Texture2D windroseTex = new Texture2D(1, 1);
            ImageConversion.LoadImage(windroseTex, fileBytes);
            windrose.BackgroundTexture = windroseTex;

            roadsignPanel.Components.Add(windrose);

            for (int i = 0; i < 8; i++)
            {
                arrowPanel[i] = SetDirectionPanel(i, PanelTypes.Arrow);
                fileBytes = File.ReadAllBytes(Path.Combine(WorldMaps.mapPath, "RoadsignData", "arrow" + i + ".png"));
                Texture2D arrowTex = new Texture2D(1, 1);
                ImageConversion.LoadImage(arrowTex, fileBytes);
                arrowPanel[i].BackgroundTexture = arrowTex;
                arrowPanel[i].Enabled = false;

                messagePanel[i] = SetDirectionPanel(i, PanelTypes.Text);
                messagePanel[i].Enabled = false;
            
                roadsignPanel.Components.Add(arrowPanel[i]);
                roadsignPanel.Components.Add(messagePanel[i]);
            }
            IsSetup = true;
        }

        public override void OnPush()
        {
            base.OnPush();
            parentPanel.OnMouseClick += ParentPanel_OnMouseClick;
            parentPanel.OnRightMouseClick += ParentPanel_OnMouseClick;
            parentPanel.OnMiddleMouseClick += ParentPanel_OnMouseClick;
        }

        public override void OnPop()
        {
            base.OnPop();
            parentPanel.OnMouseClick -= ParentPanel_OnMouseClick;
            parentPanel.OnRightMouseClick -= ParentPanel_OnMouseClick;
            parentPanel.OnMiddleMouseClick -= ParentPanel_OnMouseClick;
        }

        #region Public Methods

        public void Show()
        {
            uiManager.PushWindow(this);
        }

        public Panel SetDirectionPanel(int dir, PanelTypes panelType)
        {
            Panel currentPanel = new Panel();

            switch (panelType)
            {
                case PanelTypes.Text:
                    currentPanel = SetTextPanel(dir);
                    break;
                case PanelTypes.Arrow:
                    currentPanel = SetArrowPanel(dir);
                    break;
            }
            return currentPanel;
        }

        public Panel SetTextPanel(int dir)
        {
            Panel currentPanel = new Panel();
            currentPanel.Size = new Vector2(80.0f, 40.0f);
            Vector2 center = new Vector2(roadsignPanel.Size.x / 2, roadsignPanel.Size.y / 2);

            switch (dir)
            {
                case 0:
                    // currentPanel.Position = new Vector2(95, 125);
                    currentPanel.Position = new Vector2(center.x - 50.0f - currentPanel.Size.x, center.y + 30.0f);
                    break;
                case 1:
                    // currentPanel.Position = new Vector2(140, 130);
                    currentPanel.Position = new Vector2(center.x - currentPanel.Size.x / 2, center.y + 50.0f);
                    break;
                case 2:
                    // currentPanel.Position = new Vector2(185, 125);
                    currentPanel.Position = new Vector2(center.x + 50.0f, center.y + 30.0f);
                    break;
                case 3:
                    // currentPanel.Position = new Vector2(190, 90);
                    currentPanel.Position = new Vector2(center.x + 70.0f, center.y - currentPanel.Size.y / 2);
                    break;
                case 4:
                    // currentPanel.Position = new Vector2(185, 55);
                    currentPanel.Position = new Vector2(center.x + 50.0f, center.y - 30.0f - currentPanel.Size.y);
                    break;
                case 5:
                    // currentPanel.Position = new Vector2(140, 50);
                    currentPanel.Position = new Vector2(center.x - currentPanel.Size.x / 2, center.y - 30.0f - currentPanel.Size.y);
                    break;
                case 6:
                    // currentPanel.Position = new Vector2(95, 55);
                    currentPanel.Position = new Vector2(center.x - 50.0f - currentPanel.Size.x, center.y - 30.0f - currentPanel.Size.y);
                    break;
                case 7:
                    currentPanel.Position = new Vector2(center.x - 70.0f - currentPanel.Size.x, center.y - currentPanel.Size.y / 2);
                    break;
            }

            return currentPanel;
        }

        public Panel SetArrowPanel(int dir)
        {
            Panel currentPanel = new Panel();
            Vector2 center = new Vector2(roadsignPanel.Size.x / 2, roadsignPanel.Size.y / 2);

            switch (dir)
            {
                case 0:
                    currentPanel.Size = new Vector2(9.0f, 9.0f);
                    currentPanel.Position = new Vector2(center.x - 40.0f, center.y + 11.0f);
                    break;
                case 1:
                    currentPanel.Size = new Vector2(11.0f, 10.0f);
                    currentPanel.Position = new Vector2(center.x - 5.0f, center.y + 30.0f);
                    break;
                case 2:
                    currentPanel.Size = new Vector2(9.0f, 9.0f);
                    currentPanel.Position = new Vector2(center.x + 31.0f, center.y + 11.0f);
                    break;
                case 3:
                    currentPanel.Size = new Vector2(11.0f, 11.0f);
                    currentPanel.Position = new Vector2(center.x + 39.0f, center.y - 6.0f);
                    break;
                case 4:
                    currentPanel.Size = new Vector2(9.0f, 9.0f);
                    currentPanel.Position = new Vector2(center.x + 31.0f, center.y - 20.0f);
                    break;
                case 5:
                    currentPanel.Size = new Vector2(11.0f, 10.0f);
                    currentPanel.Position = new Vector2(center.x - 5.0f, center.y - 40.0f);
                    break;
                case 6:
                    currentPanel.Size = new Vector2(9.0f, 9.0f);
                    currentPanel.Position = new Vector2(center.x - 40.0f, center.y - 20.0f);
                    break;
                case 7:
                    currentPanel.Size = new Vector2(10.0f, 11.0f);
                    currentPanel.Position = new Vector2(center.x - 50.0f, center.y - 6.0f);
                    break;
            }

            return currentPanel;
        }

        public TextFile.Token[] SetSignText(string[] sign)
        {
            List<TextFile.Token> text = new List<TextFile.Token>();
            
            for (int i = 0; i < sign.Length; i++)
            {
                TextFile.Token newText = new TextFile.Token();

                int interruption = 0;
                if (sign[i].Length > 20)
                {
                    interruption = sign[i].LastIndexOf(" ", 16);
                    string[] splitted = new string[2];
                    splitted[0] = sign[i].Substring(0, interruption);
                    splitted[1] = sign[i].Substring(interruption + 1);

                    newText.text = "- " + splitted[0];
                    newText.formatting = TextFile.Formatting.Text;
                    text.Add(newText);

                    newText = new TextFile.Token();
                    newText.formatting = TextFile.Formatting.NewLine;
                    text.Add(newText);

                    newText.text = splitted[1];
                    newText.formatting = TextFile.Formatting.Text;
                    text.Add(newText);
                }
                else
                {
                    newText.text = "- " + sign[i];
                    newText.formatting = TextFile.Formatting.Text;
                    text.Add(newText);
                }
                
                if (i < sign.Length - 1)
                {
                    newText = new TextFile.Token();
                    newText.formatting = TextFile.Formatting.NewLine;
                    text.Add(newText);
                }
            }
            return text.ToArray();
        }

        #endregion

        #region Private Methods

        void SetupRoadsign(string[][] signData)
        {
            Setup();
            PlayerGPS playerGPS = GameManager.Instance.PlayerGPS;
            Vector2 vecPlayerFacing = new Vector2(GameManager.Instance.MainCamera.transform.forward.x, GameManager.Instance.MainCamera.transform.forward.z);
            // float angle = Mathf.Acos(Vector2.Dot(vecDirectionToTarget, Vector2.right) / vecDirectionToTarget.magnitude) / Mathf.PI * 180.0f;
            float facing = Mathf.Acos(Vector2.Dot(vecPlayerFacing, Vector2.right) / vecPlayerFacing.magnitude) / Mathf.PI * 180.0f;
            // if (vecDirectionToTarget.y < 0)
            //     angle = 180.0f + (180.0f - angle);
            if (vecPlayerFacing.y < 0)
                facing = 180.0f + (180.0f - facing);
            int diff = 0;

            if ((facing >= 0.0f && facing < 22.5f) || (facing >= 337.5f && facing <= 360.0f))
                diff += 0;  // North
            else if (facing >= 157.5f && facing < 202.5f)
                diff += 4;  // South
            else if (facing >= 67.5f && facing < 112.5f)
                diff += 6;  // East
            else if (facing >= 247.5f && facing < 292.5f)
                diff += 2;  // West
            else if (facing >= 22.5f && facing < 67.5f)
                diff += 7;  // North-East
            else if (facing >= 292.5f && facing < 337.5f)
                diff +=1;   // North-West
            else if (facing >= 112.5f && facing < 157.5f)
                diff += 5;
            else if (facing >= 202.5f && facing < 247.5f)
                diff += 3;
            
            Debug.Log("facing: " + facing + ", difference: " + diff + ", arrowPanel.Length: " + arrowPanel.Length + ", messagePanel.Length: " + messagePanel.Length);
            for (int i = 0; i < 8; i++)
            {
                int correction = i + diff;
                if (correction >= 8) correction -= 8;
                if (signData[i] != null)
                {
                    Debug.Log("Activating panel " + i + " (" + correction + ")");
                    byte[] fileBytes = File.ReadAllBytes(Path.Combine(WorldMaps.mapPath, "RoadsignData", "arrow" + correction + ".png"));
                    Texture2D arrowTex = new Texture2D(1, 1);
                    ImageConversion.LoadImage(arrowTex, fileBytes);
                    arrowPanel[correction].BackgroundTexture = arrowTex;                    
                    arrowPanel[correction].Enabled = true;

                    TextFile.Token[] text = SetSignText(signData[i]);
                    label[correction] = new MultiFormatTextLabel();
                    label[correction].HorizontalAlignment = HorizontalAlignment.Center;
                    label[correction].VerticalAlignment = VerticalAlignment.Middle;
                    label[correction].Size = messagePanel[correction].Size;
                    label[correction].Position = messagePanel[correction].Position;
                    label[correction].TextScale = TextScale;
                    label[correction].SetText(text);
                    label[correction].Enabled = true;

                    messagePanel[correction].Components.Add(label[correction]);
                    messagePanel[correction].Enabled = true;                    
                }
            }
            roadsignPanel.Enabled = true;
        }

        #endregion

        #region Event Handlers

        private void ParentPanel_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            // Must be presented for minimum time before allowing to click through
            // This prevents capturing parent-level click events and closing immediately
            if (Time.realtimeSinceStartup - presentationTime < minTimePresented)
                return;

            // Filter out (mouse) fighting activity
            if (InputManager.Instance.GetKey(InputManager.Instance.GetBinding(InputManager.Actions.SwingWeapon)))
                return;

            if (uiManager.TopWindow == this)
            {
                // if (nextMessageBox != null)
                //     nextMessageBox.Show();
                if (clickAnywhereToClose)
                    CloseWindow();
            }
        }

        #endregion
    }
}
