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
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Utility;
using DaggerfallConnect.Arena2;

namespace DaggerfallWorkshop.Game.UserInterfaceWindows
{
    /// <summary>
    /// Implements the enter name window.
    /// </summary>
    public class CreateCharNameSelect : DaggerfallPopupWindow
    {
        const string nativeImgName = "CHAR00I0.IMG";
        const int minAge = 20;
        const int maxAge = 40;
        const int minDay = 1;
        const int maxDay = 30;
        const int minMonth = 0;
        const int maxMonth = 11;

        static string nativeTexturePath;
        static string birthsignTexturePath;

        NameHelper nameHelper = new NameHelper();
        RaceTemplate raceTemplate;
        Genders gender;
        int age = 20;
        int day = 1;
        int month = 0;
        int era = 3;
        int year = 385;
        int birthday = 0;

        Panel birthsignPanel = new Panel();
        Panel birthsignTextPanel = new Panel();

        Texture2D nativeTexture;
        Texture2D birthsignTexture;
        TextBox textBox = new TextBox();
        Button randomNameButton = new Button();
        Button prevAgeButton;
        Button nextAgeButton;
        Button prevDayButton;
        Button nextDayButton;
        Button prevMonthButton;
        Button nextMonthButton;
        Button randomBirthdayButton;
        Button okButton = new Button();
        TextLabel ageLabel;
        TextLabel dayLabel;
        TextLabel monthLabel;
        TextLabel promptLabel1;
        TextLabel promptLabel2;
        TextLabel promptLabel3;
        TextLabel birthsignLabel;

        public RaceTemplate RaceTemplate
        {
            get { return raceTemplate; }
            set { SetRaceTemplate(value); }
        }

        public Genders Gender
        {
            get { return gender; }
            set { SetGender(value); }
        }

        public string CharacterName
        {
            get { return textBox.Text; }
            set { textBox.Text = value; }
        }

        public int CharacterAge
        {
            get { return age; }
            set { age = value; }
        }

        public int CharacterBirthday
        {
            get { return birthday; }
            set { birthday = value; }
        }

        public CreateCharNameSelect(IUserInterfaceManager uiManager)
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
                throw new Exception("CreateCharNameSelect: Could not load native texture.");
            nativeTexture.filterMode = FilterMode.Point;

            birthsignTexture = new Texture2D(1, 1);
            birthsignTexturePath = Path.Combine(WorldMaps.texturePath, "Birthsigns", ((DaggerfallDateTime.BirthSigns)(month)).ToString() + ".png");
            ImageConversion.LoadImage(birthsignTexture, File.ReadAllBytes(birthsignTexturePath));
            if (!birthsignTexture)
                throw new Exception("CreateCharNameSelect: Could not load birthsign texture.");
            birthsignTexture.filterMode = FilterMode.Point;

            // Setup native panel background
            NativePanel.BackgroundTexture = nativeTexture;

            // Text edit box
            textBox.Position = new Vector2(80, 5);
            textBox.Size = new Vector2(214, 7);
            NativePanel.Components.Add(textBox);

            // Random name button
            randomNameButton = DaggerfallUI.AddButton(new Rect(266, 2, 50, 12), NativePanel);
            // randomNameButton.Label.Text = "Random";
            // randomNameButton.Label.ShadowColor = Color.black;
            // randomNameButton.BackgroundColor = new Color(0.5f, 0.5f, 0.5f, 0.75f);
            randomNameButton.OnMouseClick += RandomNameButton_OnMouseClick;

            birthsignPanel.Position = new Vector2(217, 85);
            birthsignPanel.Size = new Vector2(94, 72);
            birthsignPanel.BackgroundTexture = birthsignTexture;
            birthsignPanel.BackgroundTextureLayout = BackgroundLayout.ScaleToFit;
            NativePanel.Components.Add(birthsignPanel);

            birthsignTextPanel.Position = new Vector2(216, 159);
            birthsignTextPanel.Size = new Vector2(96, 9);
            NativePanel.Components.Add(birthsignTextPanel);

            promptLabel1 = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(45, 20), TextManager.Instance.GetLocalizedText("age"), NativePanel);
            promptLabel1.HorizontalAlignment = HorizontalAlignment.None;
            promptLabel1.TextColor = DaggerfallUI.DaggerfallDefaultTextColor;
            promptLabel1.ShadowColor = DaggerfallUI.DaggerfallDefaultShadowColor;
            promptLabel1.ShadowPosition = DaggerfallUI.DaggerfallDefaultShadowPos;

            promptLabel2 = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(195, 20), TextManager.Instance.GetLocalizedText("birthday"), NativePanel);
            promptLabel2.HorizontalAlignment = HorizontalAlignment.None;
            promptLabel2.TextColor = DaggerfallUI.DaggerfallDefaultTextColor;
            promptLabel2.ShadowColor = DaggerfallUI.DaggerfallDefaultShadowColor;
            promptLabel2.ShadowPosition = DaggerfallUI.DaggerfallDefaultShadowPos;

            ageLabel = DaggerfallUI.AddTextLabel(DaggerfallUI.LargeFont, new Vector2(44, 48), age.ToString(), NativePanel);
            ageLabel.HorizontalAlignment = HorizontalAlignment.None;
            ageLabel.TextColor = DaggerfallUI.DaggerfallDefaultTextColor;
            ageLabel.ShadowColor = DaggerfallUI.DaggerfallDefaultShadowColor;
            ageLabel.ShadowPosition = DaggerfallUI.DaggerfallDefaultShadowPos;

            dayLabel = DaggerfallUI.AddTextLabel(DaggerfallUI.LargeFont, new Vector2(159, 48), day.ToString(), NativePanel);
            dayLabel.HorizontalAlignment = HorizontalAlignment.None;
            dayLabel.TextColor = DaggerfallUI.DaggerfallDefaultTextColor;
            dayLabel.ShadowColor = DaggerfallUI.DaggerfallDefaultShadowColor;
            dayLabel.ShadowPosition = DaggerfallUI.DaggerfallDefaultShadowPos;

            monthLabel = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(244, 48), TextManager.Instance.GetLocalizedTextList("monthNames")[month], NativePanel);
            monthLabel.HorizontalAlignment = HorizontalAlignment.None;
            monthLabel.TextColor = DaggerfallUI.DaggerfallDefaultTextColor;
            monthLabel.ShadowColor = DaggerfallUI.DaggerfallDefaultShadowColor;
            monthLabel.ShadowPosition = DaggerfallUI.DaggerfallDefaultShadowPos;

            birthsignLabel = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont, new Vector2(247, 48), TextManager.Instance.GetLocalizedTextList("birthSignNames")[month], birthsignPanel);
            birthsignLabel.HorizontalAlignment = HorizontalAlignment.Center;
            birthsignLabel.VerticalAlignment = VerticalAlignment.Bottom;
            birthsignLabel.TextColor = DaggerfallUI.DaggerfallDefaultTextColor;
            birthsignLabel.ShadowColor = DaggerfallUI.DaggerfallDefaultShadowColor;
            birthsignLabel.ShadowPosition = DaggerfallUI.DaggerfallDefaultShadowPos;
            birthsignTextPanel.Components.Add(birthsignLabel);

            // Previous/Next buttons
            prevAgeButton = DaggerfallUI.AddButton(new Rect(14, 55, 9, 16), NativePanel);
            prevAgeButton.OnMouseClick += PrevAgeButton_OnMouseClick;
            nextAgeButton = DaggerfallUI.AddButton(new Rect(14, 35, 9, 16), NativePanel);
            nextAgeButton.OnMouseClick += NextAgeButton_OnMouseClick;

            prevDayButton = DaggerfallUI.AddButton(new Rect(127, 55, 9, 16), NativePanel);
            prevDayButton.OnMouseClick += PrevDayButton_OnMouseClick;
            nextDayButton = DaggerfallUI.AddButton(new Rect(127, 35, 9, 16), NativePanel);
            nextDayButton.OnMouseClick += NextDayButton_OnMouseClick;

            prevMonthButton = DaggerfallUI.AddButton(new Rect(230, 55, 9, 16), NativePanel);
            prevMonthButton.OnMouseClick += PrevMonthButton_OnMouseClick;
            nextMonthButton = DaggerfallUI.AddButton(new Rect(230, 35, 9, 16), NativePanel);
            nextMonthButton.OnMouseClick += NextMonthButton_OnMouseClick;

            // Random birthday button
            randomBirthdayButton = DaggerfallUI.AddButton(new Rect(216, 177, 21, 18), NativePanel);
            randomBirthdayButton.OnMouseClick += RandomBirthdayButton_OnMouseClick;

            // OK button
            okButton = DaggerfallUI.AddButton(new Rect(263, 172, 39, 22), NativePanel);
            okButton.OnMouseClick += OkButton_OnMouseClick;

            // First display of random button
            ShowRandomButton();
        }

        protected void UpdateLabels()
        {
            birthsignPanel.BackgroundTexture = birthsignTexture;
            ageLabel.Text = age.ToString();
            dayLabel.Text = day.ToString();
            monthLabel.Text = TextManager.Instance.GetLocalizedTextList("monthNames")[month];
            birthsignLabel.Text = TextManager.Instance.GetLocalizedTextList("birthSignNames")[month];

        }

        public override void OnPush()
        {
            // Subsequent display of random button
            ShowRandomButton();

            base.OnPush();
        }

        void ShowRandomButton()
        {
            // Must have a race template set
            if (raceTemplate == null)
            {
                randomNameButton.Enabled = false;
                return;
            }

            randomNameButton.Enabled = true;

            // Randomise DFRandom seed from System.Random
            // A bit of a hack but better than starting with a seed of 0 every time
            System.Random random = new System.Random();
            DFRandom.Seed = (uint)random.Next();
        }

        public override void Update()
        {
            base.Update();

            if (Input.GetKeyDown(KeyCode.Return))
                AcceptName();
        }

        void AcceptBirthday()
        {
            year = DaggerfallUnity.Instance.WorldTime.Now.Year - age;
            if (month > DaggerfallUnity.Instance.WorldTime.Now.Month ||
               (month == DaggerfallUnity.Instance.WorldTime.Now.Month &&
                day > DaggerfallUnity.Instance.WorldTime.Now.DayOfMonth))
                year--;
            birthday = year + era * 10000 + month * 100000 + day * 10000000;
        }

        void AcceptName()
        {
            if (textBox.Text.Length > 0)
                CloseWindow();
        }

        void SetRaceTemplate(RaceTemplate raceTemplate)
        {
            if (this.raceTemplate != null)
            {
                // Empty name textbox if race ID changed
                if (this.raceTemplate.ID != raceTemplate.ID)
                    textBox.Text = string.Empty;
            }

            this.raceTemplate = raceTemplate;
        }

        void SetGender(Genders gender)
        {
            // Empty name textbox if gender changed
            if (this.gender != gender)
                textBox.Text = string.Empty;

            this.gender = gender;
        }

        #region Event Handlers

        private void RandomNameButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            // Generate name based on race
            NameHelper.BankTypes bankType = MacroHelper.GetNameBank((Races)raceTemplate.ID);
            textBox.Text = nameHelper.FullName(bankType, gender);
        }

        void PrevAgeButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            age--;
            if (age < minAge)
                age = minAge;

            UpdateLabels();
        }

        void NextAgeButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            age++;
            if (age > maxAge)
                age = maxAge;

            UpdateLabels();
        }

        void PrevDayButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            day--;
            if (day < minDay)
                day = minDay;

            UpdateLabels();
        }

        void NextDayButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            day++;
            if (day > maxDay)
                day = maxDay;

            UpdateLabels();
        }

        void PrevMonthButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            month--;
            if (month < minMonth)
                month = minMonth;

            birthsignTexture = new Texture2D(1, 1);
            birthsignTexturePath = Path.Combine(WorldMaps.texturePath, "Birthsigns", ((DaggerfallDateTime.BirthSigns)(month)).ToString() + ".png");
            ImageConversion.LoadImage(birthsignTexture, File.ReadAllBytes(birthsignTexturePath));
            if (!birthsignTexture)
                throw new Exception("CreateCharNameSelect: Could not load birthsign texture.");
            birthsignTexture.filterMode = FilterMode.Point;

            UpdateLabels();
        }

        void NextMonthButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            month++;
            if (month > maxMonth)
                month = maxMonth;

            birthsignTexture = new Texture2D(1, 1);
            birthsignTexturePath = Path.Combine(WorldMaps.texturePath, "Birthsigns", ((DaggerfallDateTime.BirthSigns)(month)).ToString() + ".png");
            ImageConversion.LoadImage(birthsignTexture, File.ReadAllBytes(birthsignTexturePath));
            if (!birthsignTexture)
                throw new Exception("CreateCharNameSelect: Could not load birthsign texture.");
            birthsignTexture.filterMode = FilterMode.Point;

            UpdateLabels();
        }

        void RandomBirthdayButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            age = UnityEngine.Random.Range(minAge, maxAge + 1);
            day = UnityEngine.Random.Range(minDay, maxDay + 1);
            month = UnityEngine.Random.Range(minMonth, maxMonth + 1);

            birthsignTexture = new Texture2D(1, 1);
            birthsignTexturePath = Path.Combine(WorldMaps.texturePath, "Birthsigns", ((DaggerfallDateTime.BirthSigns)(month)).ToString() + ".png");
            ImageConversion.LoadImage(birthsignTexture, File.ReadAllBytes(birthsignTexturePath));
            if (!birthsignTexture)
                throw new Exception("CreateCharNameSelect: Could not load birthsign texture.");
            birthsignTexture.filterMode = FilterMode.Point;

            UpdateLabels();
        }

        void OkButton_OnMouseClick(BaseScreenComponent sender, Vector2 position)
        {
            AcceptBirthday();
            AcceptName();
        }

        #endregion
    }
}