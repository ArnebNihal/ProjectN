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
    /// Implements final summary window at end of character creation.
    /// </summary>
    public class CreateCharSummary : DaggerfallPopupWindow
    {
        const string nativeImgName = "CHAR04I0.IMG";
        const int strYouMustDistributeYourBonusPoints = 14;

        Texture2D nativeTexture;
        string nativeTexturePath;
        TextLabel levelLabel;
        TextBox textBox = new TextBox();
        StatsRollout statsRollout = new StatsRollout();
        SkillsRollout skillsRollout = new SkillsRollout(true);
        ReflexPicker reflexPicker = new ReflexPicker();
        FacePicker facePicker = new FacePicker();
        CharacterDocument characterDocument;

        public CharacterDocument CharacterDocument
        {
            get { return characterDocument; }
            set { SetCharacterSheet(value); }
        }

        public CreateCharSummary(IUserInterfaceManager uiManager)
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

            // Add stats rollout
            NativePanel.Components.Add(statsRollout);

            // Add skills rollout
            NativePanel.Components.Add(skillsRollout);

            // Add reflex picker
            reflexPicker.Position = new Vector2(246, 95);
            NativePanel.Components.Add(reflexPicker);

            // Add face picker
            NativePanel.Components.Add(facePicker);

            // Add level indicator
            // levelLabel = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(59, 5), age.ToString(), NativePanel);
            levelLabel.HorizontalAlignment = HorizontalAlignment.None;
            levelLabel.TextColor = DaggerfallUI.DaggerfallDefaultTextColor;
            levelLabel.ShadowColor = DaggerfallUI.DaggerfallDefaultShadowColor;
            levelLabel.ShadowPosition = DaggerfallUI.DaggerfallDefaultShadowPos;

            // Add name editor
            textBox.Position = new Vector2(155, 5);
            textBox.Size = new Vector2(158, 7);
            NativePanel.Components.Add(textBox);

            // Add "Restart" button
            Button restartButton = DaggerfallUI.AddButton(new Rect(263, 147, 39, 22), NativePanel);
            restartButton.OnMouseClick += RestartButton_OnMouseClick;

            // Add "OK" button
            Button okButton = DaggerfallUI.AddButton(new Rect(263, 172, 39, 22), NativePanel);
            okButton.OnMouseClick += OkButton_OnMouseClick;
        }

        #region Private Methods

        void SetCharacterSheet(CharacterDocument characterDocument)
        {
            this.characterDocument = characterDocument;
            this.textBox.Text = characterDocument.name;
            this.statsRollout.StartingStats = characterDocument.startingStats;
            this.statsRollout.WorkingStats = characterDocument.workingStats;
            this.skillsRollout.SetClassSkills(characterDocument.career, ref characterDocument.age);
            this.skillsRollout.StartingSkills = characterDocument.startingSkills;
            this.skillsRollout.WorkingSkills = characterDocument.workingSkills;
            this.skillsRollout.SkillBonuses = BiogFile.GetSkillEffects(characterDocument.biographyEffects);
            this.SetStartingLevelUpSkillSum();
            this.CalculateStartingLevel();
            this.CalculateStatBonusPool();
            this.levelLabel = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(59, 5), characterDocument.level.ToString(), NativePanel);
            // this.skillsRollout.PrimarySkillBonusPoints = 0;
            // this.skillsRollout.MajorSkillBonusPoints = 0;
            // this.skillsRollout.MinorSkillBonusPoints = 0;
            this.facePicker.FaceIndex = characterDocument.faceIndex;
            this.facePicker.SetFaceTextures(characterDocument.raceTemplate, characterDocument.gender);
            this.reflexPicker.PlayerReflexes = characterDocument.reflexes;
        }

        public CharacterDocument GetUpdatedCharacterDocument()
        {
            characterDocument.name = textBox.Text;
            characterDocument.startingStats = statsRollout.StartingStats;
            characterDocument.workingStats = statsRollout.WorkingStats;
            characterDocument.startingSkills = skillsRollout.StartingSkills;
            characterDocument.workingSkills = skillsRollout.WorkingSkills;
            characterDocument.faceIndex = facePicker.FaceIndex;
            characterDocument.reflexes = reflexPicker.PlayerReflexes;
            return characterDocument;
        }

        public void SetStartingLevelUpSkillSum()
        {
            short sum = 0;
            short lowestMajorSkillValue = 0;
            short highestMinorSkillValue = 0;
            List<DFCareer.Skills> primarySkills = new List<DFCareer.Skills> { characterDocument.career.PrimarySkill1, characterDocument.career.PrimarySkill2, characterDocument.career.PrimarySkill3 };
            List<DFCareer.Skills> majorSkills = new List<DFCareer.Skills> { characterDocument.career.MajorSkill1, characterDocument.career.MajorSkill2, characterDocument.career.MajorSkill3 };
            List<DFCareer.Skills> minorSkills = new List<DFCareer.Skills> { characterDocument.career.MinorSkill1, characterDocument.career.MinorSkill2, characterDocument.career.MinorSkill3, characterDocument.career.MinorSkill4, characterDocument.career.MinorSkill5, characterDocument.career.MinorSkill6 };
            for (int i = 0; i < primarySkills.Count; i++)
            {
                sum += characterDocument.startingSkills.GetPermanentSkillValue(primarySkills[i]);
            }

            for (int i = 0; i < majorSkills.Count; i++)
            {
                short value = characterDocument.startingSkills.GetPermanentSkillValue(majorSkills[i]);
                sum += value;
                if (i == 0)
                    lowestMajorSkillValue = value;
                else if (value < lowestMajorSkillValue)
                    lowestMajorSkillValue = value;
            }

            sum -= lowestMajorSkillValue;

            for (int i = 0; i < minorSkills.Count; i++)
            {
                short value = characterDocument.startingSkills.GetPermanentSkillValue(minorSkills[i]);
                if (i == 0)
                    highestMinorSkillValue = value;
                else if (value > highestMinorSkillValue)
                    highestMinorSkillValue = value;
            }

            sum += highestMinorSkillValue;

            Debug.Log("sum: " + sum);

            characterDocument.startingLevelUpSkillSum = sum;
        }

        public void CalculateStartingLevel()
        {
            int skillIncrements = characterDocument.startingLevelUpSkillSum - StartNewGameWizard.startingLevelUpSkillsSum;
            int level = 1;

            Debug.Log("skillIncrements: " + skillIncrements);
            while (skillIncrements > 0)
            {
                skillIncrements -= (9 + level);
                if (skillIncrements >= 0)
                    level++;

                Debug.Log("skillIncrements: " + skillIncrements);
            }

            characterDocument.level = level;
        }

        public void CalculateStatBonusPool()
        {
            int bonusPool = 0;
            for (int i = 2; i <= characterDocument.level; i++)
            {
                bonusPool += UnityEngine.Random.Range(4, 7);
            }

            this.statsRollout.BonusPool = bonusPool;
        }

        #endregion

        #region Events

        public delegate void OnRestartHandler();
        public event OnRestartHandler OnRestart;
        void RaiseOnRestartEvent()
        {
            if (OnRestart != null)
                OnRestart();
        }

        #endregion

        #region Event Handlers

        void RestartButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            PopWindow();
            RaiseOnRestartEvent();
        }

        void OkButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            if (statsRollout.BonusPool > 0 
            // || 
            //     skillsRollout.PrimarySkillBonusPoints > 0 ||
            //     skillsRollout.MajorSkillBonusPoints > 0 ||
            //     skillsRollout.MinorSkillBonusPoints > 0
            )
            {
                DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, this);
                messageBox.SetTextTokens(strYouMustDistributeYourBonusPoints);
                messageBox.ClickAnywhereToClose = true;
                messageBox.Show();
            }
            else
            {
                CloseWindow();
            }
        }

        #endregion
    }
}