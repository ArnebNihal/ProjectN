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
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.Player;
using DaggerfallWorkshop.Game.Entity;

namespace DaggerfallWorkshop.Game.UserInterfaceWindows
{
    /// <summary>
    /// Implements "Add Bonus Points" to skills window.
    /// </summary>
    public class CreateCharAddBonusSkills : DaggerfallPopupWindow
    {
        const string nativeImgName = "CHAR03I0.IMG";
        const int strYouMustDistributeYourBonusPoints = 14;

        Texture2D nativeTexture;
        DFCareer dfClass;
        int age;
        SkillsRollout skillsRollout;

        public DFCareer DFClass
        {
            get { return dfClass; }
            set { SetClass(value); }
        }

        public int Age
        {
            get { return age; }
            set { SetAge(value); }
        }

        public DaggerfallSkills StartingSkills
        {
            get { return skillsRollout.StartingSkills; }
        }

        public DaggerfallSkills WorkingSkills
        {
            get { return skillsRollout.WorkingSkills; }
        }

        public IEnumerable<int> SkillBonuses
        {
            get { return skillsRollout.SkillBonuses; }
            set { skillsRollout.SkillBonuses = value; }
        }

        public CreateCharAddBonusSkills(IUserInterfaceManager uiManager)
            : base(uiManager)
        {
        }

        protected override void Setup()
        {
            if (IsSetup)
                return;

            // Load native texture
            nativeTexture = DaggerfallUI.GetTextureFromImg(nativeImgName);
            if (!nativeTexture)
                throw new Exception("CreateCharAddBonusSkillsWindow: Could not load native texture.");

            // Setup native panel background
            NativePanel.BackgroundTexture = nativeTexture;

            // Add skills rollout
            skillsRollout = new SkillsRollout();
            skillsRollout.Position = new Vector2(0, 0);
            NativePanel.Components.Add(skillsRollout);

            // Add "OK" button
            Button okButton = DaggerfallUI.AddButton(new Rect(263, 172, 39, 22), NativePanel);
            okButton.OnMouseClick += OkButton_OnMouseClick;

            IsSetup = true;
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Draw()
        {
            base.Draw();
        }

        #region Private Methods

        void SetClass(DFCareer dfClass)
        {
            Setup();
            this.dfClass = dfClass;
            skillsRollout.SetClassSkills(dfClass, ref this.age);
        }

        void SetAge(int age)
        {
            this.age = age;
        }
        #endregion

        #region Event Handlers

        void OkButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            // if (skillsRollout.PrimarySkillBonusPoints > 0 ||
            //     skillsRollout.MajorSkillBonusPoints > 0 ||
            //     skillsRollout.MinorSkillBonusPoints > 0)
            // {
            //     DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, this);
            //     messageBox.SetTextTokens(strYouMustDistributeYourBonusPoints);
            //     messageBox.ClickAnywhereToClose = true;
            //     messageBox.Show();
            // }
            // else
            // {
                CloseWindow();
            // }
        }

        #endregion
    }
}