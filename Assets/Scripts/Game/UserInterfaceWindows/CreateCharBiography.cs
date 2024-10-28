// Project:         Daggerfall Unity
// Copyright:       Copyright (C) 2009-2022 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Numidium
// Contributors:    
// 
// Notes:
//

using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.Questing;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Utility.AssetInjection;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Player;
using Newtonsoft.Json;
using DaggerfallWorkshop.Game.Utility;
using System.Linq;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop.Game.Guilds;
using DaggerfallWorkshop.Game.Banking;

namespace DaggerfallWorkshop.Game.UserInterfaceWindows
{
    /// <summary>
    /// Implements biography questionnaire
    /// </summary>
    public class CreateCharBiography : DaggerfallPopupWindow
    {
        const string nativeImgName = "BIOG00I0.IMG";
        const int questionLines = 2;
        const int questionLineSpace = 11;
        const int questionLeft = 30;
        const int questionTop = 23;
        const int questionWidth = 156;
        const int questionHeight = 45;
        const int buttonCount = 10;
        const int buttonsLeft = 10;
        const int buttonsTop = 71;
        const int buttonWidth = 149;
        const int buttonHeight = 24;
        readonly int[] provinces = { 0, 9, 0, 0, 0, 0, 0, 0, 0 };
        public const int reputationToken = 35;
        List<int>[] questionAnswers =
        { 
            new List<int> { 0, 1, 2, 3, 4, 5, 6, 7, 8 },              // 0."In which area of the world were you born?"
            new List<int> { 0 },                                      // 1."In which Imperial Province were you born?"
            new List<int> { },                                        // 2."PLACEHOLDER Akavir",
            new List<int> { },                                        // 3."PLACEHOLDER Aldmeris",
            new List<int> { },                                        // 4."PLACEHOLDER Atmora",
            new List<int> { },                                        // 5."PLACEHOLDER Lyg",
            new List<int> { },                                        // 6."PLACEHOLDER Pyandonea",
            new List<int> { },                                        // 7."PLACEHOLDER Yokuda",
            new List<int> { },                                        // 8."PLACEHOLDER isles",
            new List<int> { 0 },                                      // 9."In which region were you born?"
            new List<int> { 0, 11, 12, 13, 14, 15, 16, 17 },          // 10."What kind of settlement your home was?"
            new List<int> { 0, 26, 27, 28, 29, 30 },                  // 11."What happened to your parents?"
            new List<int> { 0 },                                      // 12."How old were you when you left home?"
        };

        int generalQuestionIndex = 0;
        int questionIndex = 0;
        int multipageIndex = 0;
        bool generalQuestionEnded = false;
        public static List<int> answers = new List<int>();
        Texture2D nativeTexture;
        TextLabel[] questionLabels = new TextLabel[questionLines];
        Button[] answerButtons = new Button[buttonCount];
        TextLabel[] answerLabels = new TextLabel[buttonCount];
        TextLabel[] answerLabelsBis = new TextLabel[buttonCount];
        BiogFile biogFile;
        public StartLocQuestions sLQuestions;
        public static DataForBiog dataForBiog;

        public CreateCharBiography(IUserInterfaceManager uiManager, CharacterDocument document)
            : base(uiManager)
        {
            Document = document;
        }

        public class StartLocQuestions
        {
            public string[] Questions;
            public string[] Answers;
        }

        public class DataForBiog
        {
            public string HomecontinentName;
            public string HomeprovinceName;
            public string HomeregionName;
            public string HometownName;
            public string MovedcontinentName;
            public string MovedprovinceName;
            public string MovedregionName;
            public string MovedtownName;
            public bool StartsFromPrimary;
            public string FamilyEnemy = string.Empty;
            public string AbroadSchool = string.Empty;
        }

        protected override void Setup()
        {
            if (IsSetup)
                return;
            
            // Load native texture
            nativeTexture = DaggerfallUI.GetTextureFromImg(nativeImgName);
            if (!nativeTexture)
                throw new Exception("CreateCharBiography: Could not load native texture.");

            // Load question data
            biogFile = new BiogFile(Document);
            sLQuestions = JsonConvert.DeserializeObject<StartLocQuestions>(File.ReadAllText(Path.Combine(WorldMaps.mapPath, "StartingLocationQuestions.json")));

            // Set background
            NativePanel.BackgroundTexture = nativeTexture;

            // Set question text
            questionLabels[0] = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont,
                                                          new Vector2(questionLeft, questionTop),
                                                          string.Empty,
                                                          NativePanel);
            questionLabels[1] = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont,
                                                          new Vector2(questionLeft, questionTop + questionLineSpace),
                                                          string.Empty,
                                                          NativePanel);
            // Setup buttons
            for (int i = 0; i < buttonCount; i++)
            {
                int left = i % 2 == 0 ? buttonsLeft : buttonsLeft + buttonWidth;

                answerButtons[i] = DaggerfallUI.AddButton(new Rect((float)left,
                                                                   (float)(buttonsTop + (i / 2) * buttonHeight),
                                                                   (float)buttonWidth,
                                                                   (float)buttonHeight), NativePanel);
                answerButtons[i].Tag = i;
                answerButtons[i].OnMouseClick += AnswerButton_OnMouseClick;
                answerLabels[i] = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont,
                                                            new Vector2(21f, 5f),
                                                            string.Empty,
                                                            answerButtons[i]);
                answerLabelsBis[i] = DaggerfallUI.AddTextLabel(DaggerfallUI.DefaultFont,
                                                            new Vector2(21f, 15f),
                                                            string.Empty,
                                                            answerButtons[i]);
            }

            PopulateControls(generalQuestionIndex);
            // PopulateControls(biogFile.Questions[questionIndex]);

            IsSetup = true;
        }

        private void PopulateControls(int generalQuestionIndex)
        {
            if (!sLQuestions.Questions[generalQuestionIndex].Contains('*'))
                questionLabels[0].Text = sLQuestions.Questions[generalQuestionIndex];
            else{
                string[] splitQuestion = sLQuestions.Questions[generalQuestionIndex].Split('*');
                for (int h = 0; h < questionLines; h++)
                    questionLabels[h].Text = splitQuestion[h];
            }
            // questionLabels[1].Text = string.Empty;
            Debug.Log("generalQuestionIndex: " + generalQuestionIndex);

            switch(generalQuestionIndex)
            {
                case 0:
                case 10:
                case 11:
                    for (int i = 0; i < buttonCount; i++)
                    {
                        if (i >= questionAnswers[generalQuestionIndex].Count)
                        {
                            answerLabels[i].Text = string.Empty;
                        }
                        else
                        {
                            if (!sLQuestions.Answers[questionAnswers[generalQuestionIndex][i]].Contains('*'))
                            {
                                answerLabels[i].Text = sLQuestions.Answers[questionAnswers[generalQuestionIndex][i]];
                                answerLabelsBis[i].Text = string.Empty;
                            }
                            else{
                                string[] splitAnswer = sLQuestions.Answers[questionAnswers[generalQuestionIndex][i]].Split('*');
                                answerLabels[i].Text = splitAnswer[0];
                                answerLabelsBis[i].Text = splitAnswer[1];
                            }
                        }
                    }
                    break;

                case 1:
                    for (int i = 0; i < Enum.GetNames(typeof(ProvinceNames)).Length; i++)
                    {
                        if (i >= Enum.GetNames(typeof(ProvinceNames)).Length)
                            answerLabels[i].Text = string.Empty;
                        else if (i == 0)
                            answerLabels[i].Text = sLQuestions.Answers[0];
                        else{
                            answerLabels[i].Text = Enum.GetName(typeof(ProvinceNames), i);
                            Debug.Log("answerLabels[i].Text: " + answerLabels[i].Text);
                        }
                    }
                    break;

                case 9:
                    for (int i = 0; i < buttonCount; i++)
                    {
                        if (multipageIndex > WorldData.WorldSetting.regionInProvince[answers[1]].Length / 8)
                            multipageIndex--;
                        if (multipageIndex == 0 && i == 0)
                            answerLabels[i].Text = sLQuestions.Answers[0];
                        else if (i == 0)
                            answerLabels[i].Text = sLQuestions.Answers[sLQuestions.Answers.Length - 2];
                        else if (i < buttonCount - 1)
                            {
                                if (i + (8 * multipageIndex) - 1 < WorldData.WorldSetting.regionInProvince[answers[1]].Length)
                                    answerLabels[i].Text = WorldData.WorldSetting.RegionNames[WorldData.WorldSetting.regionInProvince[answers[1]][i + (8 * multipageIndex) - 1]];
                                else answerLabels[i].Text = string.Empty;
                            }
                        else if (i == (buttonCount - 1) && multipageIndex < WorldData.WorldSetting.regionInProvince[answers[1]].Length / 8) 
                            answerLabels[i].Text = sLQuestions.Answers[sLQuestions.Answers.Length - 1];
                        else 
                            answerLabels[i].Text = string.Empty;
                    }
                    break;

                case 12:
                    Debug.Log("answers.Count: " + answers.Count);
                    switch(answers[answers.Count - 1])
                    {
                        case 1:
                            questionAnswers[generalQuestionIndex].AddRange(new List<int> { 19, 20, 21, 22, 23, 24 });
                            break;

                        case 2:
                        case 4:
                            questionAnswers[generalQuestionIndex].AddRange(new List<int> { 19, 20, 21 });
                            break;

                        case 3:
                            questionAnswers[generalQuestionIndex].AddRange(new List<int> { 19, 20 });
                            break;

                        case 5:
                            questionAnswers[generalQuestionIndex].AddRange(new List<int> { 20, 21, 22 });
                            break;
                    }
                    for (int i = 0; i < buttonCount; i++)
                    {
                        if (i >= questionAnswers[generalQuestionIndex].Count)
                        {
                            answerLabels[i].Text = string.Empty;
                            answerLabelsBis[i].Text = string.Empty;
                        }
                        if (!sLQuestions.Answers[questionAnswers[generalQuestionIndex][i]].Contains('*'))
                            {
                                answerLabels[i].Text = sLQuestions.Answers[questionAnswers[generalQuestionIndex][i]];
                                answerLabelsBis[i].Text = string.Empty;
                            }
                            else{
                                string[] splitAnswer = sLQuestions.Answers[questionAnswers[generalQuestionIndex][i]].Split('*');
                                answerLabels[i].Text = splitAnswer[0];
                                answerLabelsBis[i].Text = splitAnswer[1];
                            }
                    }
                    break;

                default:
                    break;
            }
            // for (int i = questionAnswers[generalQuestionIndex].Count; i < buttonCount; i++)
            // {
            //     answerLabels[i].Text = string.Empty;
            // }
        }

        private void PopulateControls(BiogFile.Question question)
        {
            questionLabels[0].Text = question.Text[0];
            questionLabels[1].Text = question.Text[1];

            int[] excludedAnswers = new int[0];
            int picked = -1;
            if (question.Answers.Count > buttonCount)
            {
                excludedAnswers = new int[question.Answers.Count - buttonCount];
                for (int exAns = 0; exAns < (question.Answers.Count - buttonCount); exAns++)
                {
                    do{
                        picked = UnityEngine.Random.Range(0, question.Answers.Count);
                        excludedAnswers[exAns] = picked;
                    }
                    while (excludedAnswers.Contains(picked));
                }
            }

            for (int i = 0; i < question.Answers.Count; i++)
            {
                if (question.Answers[i].Text.Contains('*'))
                {
                    string[] splitAnswer = question.Answers[i].Text.Split('*');
                    answerLabels[i].Text = splitAnswer[0];
                    answerLabelsBis[i].Text = splitAnswer[1];
                }
                else{
                    answerLabels[i].Text = question.Answers[i].Text;
                    answerLabelsBis[i].Text = string.Empty;
                }
            }
            // blank out remaining labels
            for (int i = question.Answers.Count; i < buttonCount; i++)
            {
                answerLabels[i].Text = string.Empty;
                answerLabelsBis[i].Text = string.Empty;
            }
        }

        void AnswerButton_OnMouseClick(BaseScreenComponent sender, Vector2 pos)
        {
            int answerIndex = (int)sender.Tag;
            Debug.Log("answerIndex: " + answerIndex);

            if (generalQuestionEnded)
            {
                List<BiogFile.Answer> curAnswers = biogFile.Questions[questionIndex].Answers;

                if (answerIndex >= curAnswers.Count)
                {
                    return; // not an answer for this question
                }
                else if (questionIndex < biogFile.Questions.Length - 1)
                {
                    foreach (string effect in curAnswers[answerIndex].Effects)
                    {
                        biogFile.AddEffect(effect, questionIndex);
                    }
                    questionIndex++;
                    PopulateControls(biogFile.Questions[questionIndex]);
                }
                else
                {
                    // Add final effects
                    foreach (string effect in curAnswers[answerIndex].Effects)
                    {
                        biogFile.AddEffect(effect, questionIndex);
                    }

                    // Create text biography
                    BackStory = biogFile.GenerateBackstory();

                    // Show reputation changes
                    biogFile.DigestRepChanges();
                    DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, this);
                    messageBox.SetTextTokens(reputationToken, biogFile);
                    messageBox.ClickAnywhereToClose = true;
                    messageBox.OnClose += MessageBox_OnClose;
                    messageBox.Show();
                }
            }
            else{
                if (answerIndex >= answerLabels.Length)
                {
                    return; // not an answer for this question
                }
                else if (generalQuestionIndex == 9 && answerIndex == 9)
                {
                    multipageIndex++;
                    PopulateControls(generalQuestionIndex);
                }
                else if (generalQuestionIndex == 9 && multipageIndex != 0 && answerIndex == 0)
                {
                    multipageIndex--;
                    PopulateControls(generalQuestionIndex);
                }
                else if (generalQuestionIndex < questionAnswers.Length - 1)
                {
                    Debug.Log("generalQuestionIndex(" + generalQuestionIndex + ") < questionAnswers.Length(" + questionAnswers.Length + ") - 1");
                    if (generalQuestionIndex == 9)
                        answers.Add((answerIndex - 1) + multipageIndex * 8);
                    else answers.Add(answerIndex);
                    generalQuestionIndex = GetNextQuestion(generalQuestionIndex, answerIndex);
                    PopulateControls(generalQuestionIndex);
                }
                else
                {
                    Debug.Log("generalQuestionIndex: " + generalQuestionIndex);
                    answers.Add(answerIndex);
                    StartGameBehaviour.startingState = GenerateStartingState(answers);
                    generalQuestionEnded = true;
                    PopulateControls(biogFile.Questions[questionIndex]);
                }
            }
        }

        int GetNextQuestion(int questionIndex, int answerIndex)
        {
            switch (questionIndex)
            {
                case 0:
                    return answerIndex;

                case 1:
                    return 9;

                case 9:
                case 10:
                case 11:
                case 12:
                    return ++questionIndex;

                default:
                    break;
            }

            return 13;
        }

        StartGameBehaviour.StartLocation GenerateStartingState(List<int> givenAnswers)
        {
            StartGameBehaviour.StartLocation startingData = new StartGameBehaviour.StartLocation();
            List<int> regions = new List<int>();
            Dictionary<int, List<int>> locationsSelected = new Dictionary<int, List<int>>();
            List<int> locationsList = new List<int>();
            Dictionary<int, List<int>> simpleLocSelect = new Dictionary<int, List<int>>();
            (DFPosition, int, int, int, DFBlock) parentsPlace;
            int randomProvince = 0;
            int region = -1;
            int provinceOffset = 0;
            dataForBiog = new DataForBiog();

            for (int i = 0; i < givenAnswers.Count; i++)
            {
                Debug.Log("givenAnswers.Count: " + givenAnswers.Count);
                switch (i)
                {
                    case 0:
                        for (int j = 0; j < givenAnswers[i]; j++)
                            provinceOffset += provinces[j];
                            dataForBiog.HomecontinentName = dataForBiog.MovedcontinentName = ((WorldAreaNames)givenAnswers[i]).ToString();
                        break;

                    case 1:
                        regions.AddRange(WorldData.WorldSetting.regionInProvince[givenAnswers[i] + provinceOffset].ToList());
                        dataForBiog.HomeprovinceName = dataForBiog.MovedprovinceName = ((ProvinceNames)(givenAnswers[i] + provinceOffset)).ToString();
                        Debug.Log("givenAnswers[i]: " + (ProvinceNames)(givenAnswers[i] + provinceOffset));
                        break;

                    case 2:
                        // int index = (givenAnswers[i] / 10 * 8) + (givenAnswers[i] % 10 - 1);
                        // Debug.Log("givenAnswers[i]: " + givenAnswers[i] + ", index: " + index);
                        region = regions[givenAnswers[i]];
                        dataForBiog.HomeregionName = dataForBiog.MovedregionName = WorldData.WorldSetting.RegionNames[region];
                        Debug.Log("region: " + WorldData.WorldSetting.RegionNames[region]);
                        break;

                    case 3:
                        int locType = -1;
                        switch (givenAnswers[i])
                        {
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                                locType = givenAnswers[i] - 1;
                                break;

                            case 5:
                            case 6:
                                locType = givenAnswers[i] - 0;
                                break;

                            case 7:
                                locType = givenAnswers[i] + 1;
                                break;

                            case 8:
                                locType = givenAnswers[i] + 3;
                                break;

                            default:
                                locType = 999;
                                break;
                        }

                        simpleLocSelect = new Dictionary<int, List<int>>();
                        foreach (int tile in WorldMaps.regionTiles[region])
                        {
                            DFRegion tileLoaded = WorldMaps.ConvertWorldMapsToDFRegion(tile, true);
                            locationsList = new List<int>();
                            for (int j = 0; j < tileLoaded.LocationCount; j++)
                            {
                                if (PoliticData.GetAbsPoliticValue((int)tileLoaded.MapTable[j].MapId % MapsFile.MaxMapPixelX, (int)tileLoaded.MapTable[j].MapId / MapsFile.MaxMapPixelX) == region && 
                                   (int)tileLoaded.MapTable[j].LocationType == locType)
                                   {
                                        Debug.Log("Adding " + tileLoaded.MapTable[j].MapId + " (" + tileLoaded.MapNames[j] + ") to locationsList, with a location index of " + j + " in tile n." + tile);
                                        locationsList.Add(j);
                                   }
                            }
                            simpleLocSelect.Add(tile, locationsList);
                        }

                        parentsPlace = PickRemoteTownSite(DFLocation.BuildingTypes.AnyHouse, region, out dataForBiog.HometownName);
                        startingData.primaryPosition = parentsPlace.Item1;
                        startingData.primaryBuildingIndex = (parentsPlace.Item2, parentsPlace.Item3, parentsPlace.Item4, parentsPlace.Item5);
                        Debug.Log("startingData.primaryPosition: " + startingData.primaryPosition);
                        break;

                    case 5:    //  "How old were you when you left home?"
                        switch (givenAnswers[i - 1])
                        {
                            case 1:    // "They moved away because of their occupations."

                                // This require for both parents to have the same kind of specialization
                                startingData.motherSpec = (DFCareer.Skills)UnityEngine.Random.Range(0, Enum.GetNames(typeof(DFCareer.Skills)).Length);
                                startingData.motherJob = GetJob(startingData.motherSpec);
                                startingData.fatherSpec = GetSpecFromJob(startingData.motherJob);

                                // The random province stuff at the moment isn't used because most of them haven't any location.
                                // TODO: make it used (d'oh!)
                                randomProvince = UnityEngine.Random.Range(0, Enum.GetNames(typeof(ProvinceNames)).Length);
                                regions = new List<int>();
                                regions.AddRange(WorldData.WorldSetting.regionInProvince[givenAnswers[1]].ToList());

                                do{
                                    region = regions[UnityEngine.Random.Range(0, regions.Count)];
                                    dataForBiog.MovedregionName = WorldData.WorldSetting.RegionNames[region];
                                }
                                while (WorldMaps.regionTiles[region] == null || region > 61);

                                foreach (int tile in WorldMaps.regionTiles[region])
                                {
                                    DFRegion tileLoaded = WorldMaps.ConvertWorldMapsToDFRegion(tile, true);
                                    locationsList = new List<int>();
                                    for (int j = 0; j < tileLoaded.LocationCount; j++)
                                    {
                                        // The parents moved away for their career, thus they went to some big city that was offering more compensation for their work.
                                        if (PoliticData.GetAbsPoliticValue((int)tileLoaded.MapTable[j].MapId % MapsFile.MaxMapPixelX, (int)tileLoaded.MapTable[j].MapId / MapsFile.MaxMapPixelX) == region &&
                                           (int)tileLoaded.MapTable[j].LocationType == (int)DFRegion.LocationTypes.TownCity)
                                            locationsList.Add(j);
                                    }
                                    locationsSelected.Add(tile, locationsList);
                                }

                                switch (givenAnswers[i])
                                {
                                    case 1:    // "Just a baby, I have no memories of it."
                                        startingData.startsFromPrimary = dataForBiog.StartsFromPrimary = false;
                                        startingData.areaKnowledge = (0, 15);
                                        break;
                                    
                                    case 2:    // "A child, I have only fragmentary memories of it."
                                        startingData.startsFromPrimary = dataForBiog.StartsFromPrimary = false;
                                        startingData.areaKnowledge = (2, 10);
                                        break;

                                    case 3:    // "An adolescent."
                                        startingData.startsFromPrimary = dataForBiog.StartsFromPrimary = false;
                                        startingData.areaKnowledge = (5, 10);
                                        break;

                                    case 4:    // "A young adult, but not older than 20."
                                        startingData.startsFromPrimary = dataForBiog.StartsFromPrimary = false;
                                        startingData.areaKnowledge = (8, 5);
                                        break;

                                    case 5:    // "Between 20 and 30 years old."
                                        startingData.startsFromPrimary = dataForBiog.StartsFromPrimary = false;
                                        startingData.areaKnowledge = (10, 2);
                                        break;

                                    case 6:    // "I never left my home, I'm still there."
                                        startingData.startsFromPrimary = dataForBiog.StartsFromPrimary = true;
                                        startingData.areaKnowledge = (15, 0);
                                        break;

                                    default:
                                        startingData.areaKnowledge = (0, 0);
                                        break;
                                }

                                startingData.startingHouse = ((!startingData.startsFromPrimary && startingData.motherJob == 510) || startingData.startsFromPrimary);
                                parentsPlace = PickRemoteTownSite(GetWorkPlace(startingData.motherJob).Item1, region, out dataForBiog.MovedtownName, locationsSelected, GetWorkPlace(startingData.motherJob).Item2);
                                startingData.secondaryPosition = parentsPlace.Item1;
                                startingData.secondaryBuildingIndex = (parentsPlace.Item2, parentsPlace.Item3, parentsPlace.Item4, parentsPlace.Item5);

                                if (startingData.startsFromPrimary)
                                    startingData.startingHouseData = AcquireBuilding((startingData.primaryPosition, startingData.primaryBuildingIndex.Item1, startingData.primaryBuildingIndex.Item2, startingData.primaryBuildingIndex.Item3, startingData.primaryBuildingIndex.Item4), PoliticData.GetAbsPoliticValue(startingData.primaryPosition.X, startingData.primaryPosition.Y), StartGameBehaviour.StartingHouseNameTypes.Residence);
                                else startingData.startingHouseData = AcquireBuilding((startingData.secondaryPosition, startingData.secondaryBuildingIndex.Item1, startingData.secondaryBuildingIndex.Item2, startingData.secondaryBuildingIndex.Item3, startingData.secondaryBuildingIndex.Item4), region, StartGameBehaviour.StartingHouseNameTypes.Store);
                                MapsFile.DiscoverCloseLocations(startingData.primaryPosition, startingData.areaKnowledge.Item1, AgeRanges.Infant, (AgeRanges)givenAnswers[i], givenAnswers[i] == (int)AgeRanges.OldEnough, (givenAnswers[i]) / 2);
                                MapsFile.DiscoverCloseLocations(startingData.secondaryPosition, startingData.areaKnowledge.Item2, (AgeRanges)givenAnswers[i], AgeRanges.OldEnough, givenAnswers[i] < (int)AgeRanges.Child, (int)AgeRanges.OldEnough / 2);
                                break;

                            case 2:    // "They fled our home persecuted by enemies, taking me with them."
                                // randomProvince = givenAnswers[1];
                                regions = new List<int>();
                                for (int st = 20 * WorldData.WorldSetting.regionInProvince[givenAnswers[1] + provinceOffset][givenAnswers[2]]; st < 20 * (WorldData.WorldSetting.regionInProvince[givenAnswers[1] + provinceOffset][givenAnswers[2]] + 1); st++)
                                {
                                    if (WorldData.WorldSetting.regionBorders[st] < WorldData.WorldSetting.RegionNames.Length)
                                    {
                                        Debug.Log("Analyzing " + WorldData.WorldSetting.RegionNames[WorldData.WorldSetting.regionBorders[st]]);
                                        regions.Add(WorldData.WorldSetting.regionBorders[st]);
                                    }
                                }
                                region = regions[UnityEngine.Random.Range(0, regions.Count)];
                                dataForBiog.MovedregionName = WorldData.WorldSetting.RegionNames[region];

                                foreach (int tile in WorldMaps.regionTiles[region])
                                {
                                    DFRegion tileLoaded = WorldMaps.ConvertWorldMapsToDFRegion(tile, true);
                                    locationsList = new List<int>();
                                    for (int j = 0; j < tileLoaded.LocationCount; j++)
                                    {
                                        // The idea here is that, fleeing their home region, the parent chose some small, under-the-radar settlement.
                                        if (PoliticData.GetAbsPoliticValue((int)tileLoaded.MapTable[j].MapId % MapsFile.MaxMapPixelX, (int)tileLoaded.MapTable[j].MapId / MapsFile.MaxMapPixelX) == region &&
                                          ((int)tileLoaded.MapTable[j].LocationType == (int)DFRegion.LocationTypes.TownVillage || 
                                           (int)tileLoaded.MapTable[j].LocationType == (int)DFRegion.LocationTypes.ReligionTemple || 
                                           (int)tileLoaded.MapTable[j].LocationType == (int)DFRegion.LocationTypes.Tavern || 
                                           (int)tileLoaded.MapTable[j].LocationType == (int)DFRegion.LocationTypes.HomePoor))
                                            locationsList.Add(j);
                                    }
                                    locationsSelected.Add(tile, locationsList);
                                }
                                switch (givenAnswers[i])
                                {
                                    case 1:    // "Just a baby, I have no memories of it."
                                        startingData.startsFromPrimary = dataForBiog.StartsFromPrimary = false;
                                        startingData.areaKnowledge = (0, 15);
                                        break;
                                    
                                    case 2:    // "A child, I have only fragmentary memories of it."
                                        startingData.startsFromPrimary = dataForBiog.StartsFromPrimary = false;
                                        startingData.areaKnowledge = (2, 10);
                                        break;

                                    case 3:    // "An adolescent."
                                        startingData.startsFromPrimary = dataForBiog.StartsFromPrimary = false;
                                        startingData.areaKnowledge = (5, 10);
                                        break;

                                    default:
                                        startingData.areaKnowledge = (0, 0);
                                        break;
                                }

                                // TODO: add the fact that now the player is legally hated in his/her region of origin (or maybe he is hated by a certain faction?).
                                startingData.startingHouse = !startingData.startsFromPrimary;
                                parentsPlace = PickRemoteTownSite(DFLocation.BuildingTypes.AnyHouse, region, out dataForBiog.MovedtownName, locationsSelected);
                                startingData.secondaryPosition = parentsPlace.Item1;
                                startingData.secondaryBuildingIndex = (parentsPlace.Item2, parentsPlace.Item3, parentsPlace.Item4, parentsPlace.Item5);

                                startingData.startingHouseData = AcquireBuilding((startingData.secondaryPosition, startingData.secondaryBuildingIndex.Item1, startingData.secondaryBuildingIndex.Item2, startingData.secondaryBuildingIndex.Item3, startingData.secondaryBuildingIndex.Item4), region, StartGameBehaviour.StartingHouseNameTypes.Residence);
                                MapsFile.DiscoverCloseLocations(startingData.primaryPosition, startingData.areaKnowledge.Item1, AgeRanges.Infant, (AgeRanges)givenAnswers[i], givenAnswers[i] == (int)AgeRanges.OldEnough, (givenAnswers[i]) / 2);
                                MapsFile.DiscoverCloseLocations(startingData.secondaryPosition, startingData.areaKnowledge.Item2, (AgeRanges)givenAnswers[i], AgeRanges.OldEnough, givenAnswers[i] < (int)AgeRanges.Child, (int)AgeRanges.OldEnough / 2);
                                break;

                            case 3:     // "I lost my parents and grew up in an orphanage"
                                // randomProvince = givenAnswers[1];
                                // regions = new List<int>();
                                // regions.AddRange(WorldData.WorldSetting.regionInProvince[givenAnswers[1]].ToList());
                                region = givenAnswers[2];

                                foreach (int tile in WorldMaps.regionTiles[region])
                                {
                                    DFRegion tileLoaded = WorldMaps.ConvertWorldMapsToDFRegion(tile, true);
                                    for (int j = 0; j < tileLoaded.LocationCount; j++)
                                    {
                                        // At the moment, Temples of Stendarr are used as orphanages. Later more options could be added.
                                        if (PoliticData.GetAbsPoliticValue((int)tileLoaded.MapTable[j].MapId % MapsFile.MaxMapPixelX, (int)tileLoaded.MapTable[j].MapId / MapsFile.MaxMapPixelX) == region &&
                                           ((int)tileLoaded.MapTable[j].LocationType == (int)DFRegion.LocationTypes.TownCity ||
                                            (int)tileLoaded.MapTable[j].LocationType == (int)DFRegion.LocationTypes.TownHamlet ||
                                            (int)tileLoaded.MapTable[j].LocationType == (int)DFRegion.LocationTypes.TownVillage ||
                                            (int)tileLoaded.MapTable[j].LocationType == (int)DFRegion.LocationTypes.ReligionTemple))
                                            locationsList.Add(j);
                                    }
                                    locationsSelected.Add(tile, locationsList);
                                }
                                switch (givenAnswers[i])
                                {
                                    case 1:    // "Just a baby, I have no memories of it."                                        
                                        startingData.startsFromPrimary = dataForBiog.StartsFromPrimary = false;
                                        startingData.areaKnowledge = (0, 15);
                                        break;
                                    
                                    case 2:    // "A child, I have only fragmentary memories of it."                                        
                                        startingData.startsFromPrimary = dataForBiog.StartsFromPrimary = false;
                                        startingData.areaKnowledge = (2, 10);
                                        break;

                                    default:
                                        startingData.areaKnowledge = (0, 0);
                                        break;
                                }

                                startingData.startingHouse = false;
                                parentsPlace = PickRemoteTownSite(DFLocation.BuildingTypes.Temple, region, out dataForBiog.MovedtownName, locationsSelected, 33);
                                startingData.secondaryPosition = parentsPlace.Item1;
                                startingData.secondaryBuildingIndex = (parentsPlace.Item2, parentsPlace.Item3, parentsPlace.Item4, parentsPlace.Item5);
                                
                                startingData.startingHouseData = AcquireBuilding((startingData.secondaryPosition, startingData.secondaryBuildingIndex.Item1, startingData.secondaryBuildingIndex.Item2, startingData.secondaryBuildingIndex.Item3, startingData.secondaryBuildingIndex.Item4), region, StartGameBehaviour.StartingHouseNameTypes.None);
                                MapsFile.DiscoverCloseLocations(startingData.primaryPosition, startingData.areaKnowledge.Item1, AgeRanges.Infant, (AgeRanges)givenAnswers[i], givenAnswers[i] == (int)AgeRanges.OldEnough, (givenAnswers[i]) / 2);
                                MapsFile.DiscoverCloseLocations(startingData.secondaryPosition, startingData.areaKnowledge.Item2, (AgeRanges)givenAnswers[i], AgeRanges.OldEnough, givenAnswers[i] < (int)AgeRanges.Child, (int)AgeRanges.OldEnough / 2);
                                break;

                            case 4:     // "They sent me to some relatives, for reasons unknown"
                                regions = new List<int>();
                                for (int st = 20 * givenAnswers[2]; st < 20 * (givenAnswers[2] + 1); st++)
                                {
                                    if (WorldData.WorldSetting.regionBorders[st] < WorldData.WorldSetting.RegionNames.Length)
                                    {
                                        Debug.Log("Adding region " + WorldData.WorldSetting.RegionNames[WorldData.WorldSetting.regionBorders[st]] + " to the borders of " + WorldData.WorldSetting.RegionNames[givenAnswers[2]]);
                                        regions.Add(WorldData.WorldSetting.regionBorders[st]);
                                    }
                                }
                                region = regions[UnityEngine.Random.Range(0, regions.Count)];

                                foreach (int tile in WorldMaps.regionTiles[region])
                                {
                                    DFRegion tileLoaded = WorldMaps.ConvertWorldMapsToDFRegion(tile, true);
                                    locationsList = new List<int>();
                                    for (int j = 0; j < tileLoaded.LocationCount; j++)
                                    {
                                        // The relatives could be in any settlement, small or big.
                                        if (PoliticData.GetAbsPoliticValue((int)tileLoaded.MapTable[j].MapId % MapsFile.MaxMapPixelX, (int)tileLoaded.MapTable[j].MapId / MapsFile.MaxMapPixelX) == region &&
                                          ((int)tileLoaded.MapTable[j].LocationType == (int)DFRegion.LocationTypes.TownCity ||
                                           (int)tileLoaded.MapTable[j].LocationType == (int)DFRegion.LocationTypes.TownHamlet || 
                                           (int)tileLoaded.MapTable[j].LocationType == (int)DFRegion.LocationTypes.TownVillage ||
                                           (int)tileLoaded.MapTable[j].LocationType == (int)DFRegion.LocationTypes.HomeFarms ||
                                           (int)tileLoaded.MapTable[j].LocationType == (int)DFRegion.LocationTypes.ReligionTemple || 
                                           (int)tileLoaded.MapTable[j].LocationType == (int)DFRegion.LocationTypes.Tavern || 
                                           (int)tileLoaded.MapTable[j].LocationType == (int)DFRegion.LocationTypes.HomeWealthy))
                                            locationsList.Add(j);
                                    }
                                    locationsSelected.Add(tile, locationsList);
                                }

                                switch (givenAnswers[i])
                                {
                                    case 1:    // "Just a baby, I have no memories of it."                                        
                                        startingData.startsFromPrimary = dataForBiog.StartsFromPrimary = false;
                                        startingData.areaKnowledge = (0, 15);
                                        break;
                                    
                                    case 2:    // "A child, I have only fragmentary memories of it."                                        
                                        startingData.startsFromPrimary = dataForBiog.StartsFromPrimary = false;
                                        startingData.areaKnowledge = (2, 10);
                                        break;

                                    case 3:    // "An adolescent."                                        
                                        startingData.startsFromPrimary = dataForBiog.StartsFromPrimary = false;
                                        startingData.areaKnowledge = (5, 10);
                                        break;

                                    default:
                                        startingData.areaKnowledge = (0, 0);
                                        break;
                                }

                                startingData.startingHouse = true;
                                parentsPlace = PickRemoteTownSite(DFLocation.BuildingTypes.AnyHouse, region, out dataForBiog.MovedtownName, locationsSelected);
                                startingData.secondaryPosition = parentsPlace.Item1;
                                startingData.secondaryBuildingIndex = (parentsPlace.Item2, parentsPlace.Item3, parentsPlace.Item4, parentsPlace.Item5);

                                startingData.startingHouseData = AcquireBuilding((startingData.secondaryPosition, startingData.secondaryBuildingIndex.Item1, startingData.secondaryBuildingIndex.Item2, startingData.secondaryBuildingIndex.Item3, startingData.secondaryBuildingIndex.Item4), region, StartGameBehaviour.StartingHouseNameTypes.Residence);
                                MapsFile.DiscoverCloseLocations(startingData.primaryPosition, startingData.areaKnowledge.Item1, AgeRanges.Infant, (AgeRanges)givenAnswers[i], givenAnswers[i] == (int)AgeRanges.OldEnough, (givenAnswers[i]) / 2);
                                MapsFile.DiscoverCloseLocations(startingData.secondaryPosition, startingData.areaKnowledge.Item2, (AgeRanges)givenAnswers[i], AgeRanges.OldEnough, givenAnswers[i] < (int)AgeRanges.Child, (int)AgeRanges.OldEnough / 2);
                                break;

                            case 5:     // "They sent me abroad to receive a better education"
                                // randomProvince = UnityEngine.Random.Range(0, Enum.GetNames(typeof(ProvinceNames)).Length);
                                // regions = new List<int>();
                                regions.AddRange(WorldData.WorldSetting.regionInProvince[givenAnswers[1]].ToList());
                                regions.Remove(givenAnswers[2]);
                                region = regions[UnityEngine.Random.Range(0, regions.Count)];

                                foreach (int tile in WorldMaps.regionTiles[region])
                                {
                                    DFRegion tileLoaded = WorldMaps.ConvertWorldMapsToDFRegion(tile, true);
                                    for (int j = 0; j < tileLoaded.LocationCount; j++)
                                    {
                                        // He was sent to some kind of settlement with a Mages Guild or a Temple of Julianos.
                                        if (PoliticData.GetAbsPoliticValue((int)tileLoaded.MapTable[j].MapId % MapsFile.MaxMapPixelX, (int)tileLoaded.MapTable[j].MapId / MapsFile.MaxMapPixelX) == region &&
                                           ((int)tileLoaded.MapTable[j].LocationType == (int)DFRegion.LocationTypes.TownCity  ||
                                            (int)tileLoaded.MapTable[j].LocationType == (int)DFRegion.LocationTypes.ReligionTemple))
                                            locationsList.Add(j);
                                    }
                                    locationsSelected.Add(tile, locationsList);
                                }

                                switch (givenAnswers[i])
                                {
                                    case 2:    // "A child, I have only fragmentary memories of it."                                        
                                        startingData.startsFromPrimary = dataForBiog.StartsFromPrimary = false;
                                        startingData.areaKnowledge = (2, 10);
                                        break;

                                    case 3:    // "An adolescent."                                        
                                        startingData.startsFromPrimary = dataForBiog.StartsFromPrimary = false;
                                        startingData.areaKnowledge = (5, 10);
                                        break;

                                    case 4:    // "A young adult, but not older than 20."                                        
                                        startingData.startsFromPrimary = dataForBiog.StartsFromPrimary = false;
                                        startingData.areaKnowledge = (8, 5);
                                        break;

                                    default:
                                        startingData.areaKnowledge = (0, 0);
                                        break;
                                }

                                startingData.startingHouse = false;
                                if (UnityEngine.Random.Range(0, 2) == 0)
                                {
                                    parentsPlace = PickRemoteTownSite(DFLocation.BuildingTypes.GuildHall, region, out dataForBiog.MovedtownName, locationsSelected, 40);
                                    dataForBiog.AbroadSchool = "Mages Guild";
                                }
                                else{
                                    parentsPlace = PickRemoteTownSite(DFLocation.BuildingTypes.Temple, region, out dataForBiog.MovedtownName, locationsSelected, 27);
                                    dataForBiog.AbroadSchool = "School of Julianos";
                                }
                                
                                startingData.secondaryPosition = parentsPlace.Item1;
                                startingData.secondaryBuildingIndex = (parentsPlace.Item2, parentsPlace.Item3, parentsPlace.Item4, parentsPlace.Item5);

                                startingData.startingHouseData = AcquireBuilding((startingData.secondaryPosition, startingData.secondaryBuildingIndex.Item1, startingData.secondaryBuildingIndex.Item2, startingData.secondaryBuildingIndex.Item3, startingData.secondaryBuildingIndex.Item4), region, StartGameBehaviour.StartingHouseNameTypes.None);
                                MapsFile.DiscoverCloseLocations(startingData.primaryPosition, startingData.areaKnowledge.Item1, AgeRanges.Infant, (AgeRanges)givenAnswers[i], givenAnswers[i] == (int)AgeRanges.OldEnough, (givenAnswers[i]) / 2);
                                MapsFile.DiscoverCloseLocations(startingData.secondaryPosition, startingData.areaKnowledge.Item2, (AgeRanges)givenAnswers[i], AgeRanges.OldEnough, givenAnswers[i] < (int)AgeRanges.Child, (int)AgeRanges.OldEnough / 2);
                                break;
                                
                            default:
                                break;
                        }
                        break;

                    case 4:
                    default:
                        break;
                }
            }
            Debug.Log("startingData.secondaryPosition: " + startingData.secondaryPosition);

            return startingData;
        }

        public (DFLocation.BuildingTypes, int) GetWorkPlace(int jobType)
        {
            switch (jobType)
            {
                case 108:   // DB
                case 41:    // FG
                case 40:    // MG
                case 42:    // TG
                    return (DFLocation.BuildingTypes.GuildHall, jobType);
                
                case (int)Temple.Divines.Akatosh:
                case (int)Temple.Divines.Arkay:
                case (int)Temple.Divines.Dibella:
                case (int)Temple.Divines.Julianos:
                case (int)Temple.Divines.Kynareth:
                case (int)Temple.Divines.Mara:
                case (int)Temple.Divines.Stendarr:
                case (int)Temple.Divines.Zenithar:
                    return (DFLocation.BuildingTypes.Temple, jobType);
                
                case 510:   // Merchants
                default:
                    return (DFLocation.BuildingTypes.AnyShop, -1);
            }
        }

        /// <summary>
        /// Copypasted from Place.cs and used to select locations with certain buildings
        /// to use as primary/secondary starting locations.
        /// </summary>
        (DFPosition, int, int, int, DFBlock) PickRemoteTownSite(DFLocation.BuildingTypes requiredBuildingType, int region, out string locName, Dictionary<int, List<int>> suitableLoc = null, int subBuilding = -1)
        {
            DFRegion regionData = new DFRegion();
            List<(DFPosition, int, int, int, DFBlock, string)> foundSites = new List<(DFPosition, int, int, int, DFBlock, string)>();

            const int maxAttemptsBeforeFallback = 250;
            const int maxAttemptsBeforeFailure = 500;

            // Get region to be used
            List<int> tileIndex = WorldMaps.regionTiles[region];

            foreach (int tile in tileIndex)
            {
                Debug.Log("Analyzing tile " + tile.ToString("00000"));
                regionData = WorldMaps.ConvertWorldMapsToDFRegion(tile, true);
                // int playerLocationIndex = GameManager.Instance.PlayerGPS.CurrentLocationIndex;

                // Cannot use a tile with no locations
                if (regionData.LocationCount == 0)
                    continue;

                // Find random town containing building
                int attempts = 0;
                bool found = false;

                while (!found)
                {
                    // Increment attempts and do some fallback
                    if (++attempts >= maxAttemptsBeforeFallback &&
                        requiredBuildingType >= DFLocation.BuildingTypes.House1 &&
                        requiredBuildingType <= DFLocation.BuildingTypes.House6)
                        requiredBuildingType = DFLocation.BuildingTypes.AnyHouse;

                    if (attempts >= maxAttemptsBeforeFailure)
                    {
                        Debug.LogWarningFormat("Could not find remote town site with building type {0} within {1} attempts in tile {2}", requiredBuildingType.ToString(), attempts, tile);
                        break;
                    }

                    // Get a random location index (from the list, if it has been passed)
                    int locationIndex = -1;
                    if (suitableLoc == null)
                        locationIndex = UnityEngine.Random.Range(0, (int)regionData.LocationCount);
                    else if (suitableLoc[tile].Count > 0)
                        locationIndex = suitableLoc[tile][UnityEngine.Random.Range(0, suitableLoc[tile].Count)];
                    else
                    {
                        Debug.Log("Breaking out");
                        break;
                    }

                    // Discard all dungeon location types
                    if (regionData.MapTable[locationIndex].LocationType == DFRegion.LocationTypes.DungeonLabyrinth ||
                        regionData.MapTable[locationIndex].LocationType == DFRegion.LocationTypes.DungeonKeep ||
                        regionData.MapTable[locationIndex].LocationType == DFRegion.LocationTypes.DungeonRuin ||
                        regionData.MapTable[locationIndex].LocationType == DFRegion.LocationTypes.Graveyard)
                        continue;

                    // Get location data for town
                    DFLocation location = WorldMaps.GetLocation(tile.ToString("00000"), regionData.MapNames[locationIndex]);
                    // if (!location.Loaded)
                    //     continue;

                    // Get list of valid sites
                    DFBlock[] blocks = RMBLayout.GetLocationBuildingData(location);
                    for (int block = 0; block < blocks.Length; block++)
                    {
                        for (int bld = 0; bld < blocks[block].RmbBlock.FldHeader.NumBlockDataRecords; bld++)
                        {
                            Debug.Log("requiredBuildingType: " + requiredBuildingType + ", block: " + block + " - " + blocks[block].Name + ", bld: " + bld);
                            if (CheckBuildingCompatibility(requiredBuildingType, blocks[block].RmbBlock.FldHeader.BuildingDataList[bld], subBuilding))
                            {
                                int width = location.Exterior.ExteriorData.Width;
                                int x = block % location.Exterior.ExteriorData.Width;
                                int y = block / location.Exterior.ExteriorData.Width;
                                int bldKey = BuildingDirectory.MakeBuildingKey((byte)x, (byte)y, (byte)bld);
                                Debug.Log("bldKey: " + bldKey);
                                foundSites.Add((MapsFile.GetPixelFromPixelID(location.MapTableData.MapId), bldKey, bld, locationIndex, blocks[block], location.Name));
                                found = true;
                            }
                        }
                    }                    
                }
            }

            Debug.Log("foundSites.Count: " + foundSites.Count);
            (DFPosition, int, int, int, DFBlock, string) building = foundSites[UnityEngine.Random.Range(0, foundSites.Count)];
            locName = building.Item6;

            return (building.Item1, building.Item2, building.Item3, building.Item4, building.Item5);
        }

        public StartGameBehaviour.StartingHouseData AcquireBuilding((DFPosition, int, int, int, DFBlock) building, int region, StartGameBehaviour.StartingHouseNameTypes nameType)
        {
            StartGameBehaviour.StartingHouseData startingHouseData = new StartGameBehaviour.StartingHouseData();
            DaggerfallBankManager.SetupHouses();

            Debug.Log("Allocating building: " + building.Item2);
            startingHouseData.shPosition = building.Item1;
            startingHouseData.shBldKey = building.Item2;
            startingHouseData.shBldIndex = building.Item3;
            startingHouseData.shLocIndex = building.Item4;
            startingHouseData.shBlock = building.Item5;
            startingHouseData.shRegion = region;
            startingHouseData.shNameType = nameType;

            return startingHouseData;
        }

        public bool CheckBuildingCompatibility(DFLocation.BuildingTypes bld1, DFLocation.BuildingData bld2, int subType = -1)
        {
            if (bld1 == DFLocation.BuildingTypes.GuildHall && 
                bld2.BuildingType == DFLocation.BuildingTypes.GuildHall &&
                subType == bld2.FactionId)
                return true;
            if (bld1 == DFLocation.BuildingTypes.Temple &&
                bld2.BuildingType == DFLocation.BuildingTypes.Temple &&
                subType == bld2.FactionId)
                return true;
            if (bld1 == bld2.BuildingType)
                return true;
            if (bld1 == DFLocation.BuildingTypes.AnyHouse && ((int)bld2.BuildingType >= (int)DFLocation.BuildingTypes.House1 && 
                                                              (int)bld2.BuildingType <= (int)DFLocation.BuildingTypes.House6))
                return true;
            if (bld1 == DFLocation.BuildingTypes.AnyShop && (bld2.BuildingType == DFLocation.BuildingTypes.Alchemist ||
                                                             bld2.BuildingType == DFLocation.BuildingTypes.Armorer ||
                                                             bld2.BuildingType == DFLocation.BuildingTypes.Bookseller ||
                                                             bld2.BuildingType == DFLocation.BuildingTypes.ClothingStore ||
                                                             bld2.BuildingType == DFLocation.BuildingTypes.GemStore ||
                                                             bld2.BuildingType == DFLocation.BuildingTypes.WeaponSmith))
                return true;
            return false;
        }

        public int GetJob(DFCareer.Skills specSkill)
        {
            List<int> jobChoices = new List<int>();

            if (DarkBrotherhood.guildSkills.Contains(specSkill))
                jobChoices.Add(DarkBrotherhood.FactionId);
            if (FightersGuild.guildSkills.Contains(specSkill))
                jobChoices.Add(FightersGuild.FactionId);
            // if (KnightlyOrder.guildSkills.Contains(specSkill))
            //     jobChoices.Add(KnightlyOrder.); TODO: ID selection must be added based on region
            if (MagesGuild.guildSkills.Contains(specSkill))
                jobChoices.Add(MagesGuild.FactionId);
            foreach (KeyValuePair<Temple.Divines, List<DFCareer.Skills>> templeDict in Temple.guildSkills)
            {
                if (templeDict.Value.Contains(specSkill))
                    jobChoices.Add((int)templeDict.Key);
            }
            if (ThievesGuild.guildSkills.Contains(specSkill))
                jobChoices.Add(ThievesGuild.FactionId);

            if (jobChoices.Count > 0)
                return jobChoices[UnityEngine.Random.Range(0, jobChoices.Count)];

            return FactionsAtlas.FactionToId["The Merchants"];
        }

        public DFCareer.Skills GetSpecFromJob(int job)
        {
            switch (job)
            {
                case 108:   // DB
                    return DarkBrotherhood.guildSkills[UnityEngine.Random.Range(0, DarkBrotherhood.guildSkills.Count)];
                case 41:    // FG
                    return FightersGuild.guildSkills[UnityEngine.Random.Range(0, FightersGuild.guildSkills.Count)];
                case 40:
                    return MagesGuild.guildSkills[UnityEngine.Random.Range(0, MagesGuild.guildSkills.Count)];
                case (int)Temple.Divines.Akatosh:
                case (int)Temple.Divines.Arkay:
                case (int)Temple.Divines.Dibella:
                case (int)Temple.Divines.Julianos:
                case (int)Temple.Divines.Kynareth:
                case (int)Temple.Divines.Mara:
                case (int)Temple.Divines.Stendarr:
                case (int)Temple.Divines.Zenithar:
                    return Temple.guildSkills[(Temple.Divines)job][UnityEngine.Random.Range(0,  Temple.guildSkills[(Temple.Divines)job].Count)];
                case 42:    // TG
                    return ThievesGuild.guildSkills[UnityEngine.Random.Range(0, ThievesGuild.guildSkills.Count)];
                case 510:   // Merchants
                default:
                    DFCareer.Skills[] merchantSkills = { DFCareer.Skills.Etiquette, DFCareer.Skills.HandToHand, DFCareer.Skills.Lockpicking, DFCareer.Skills.Mercantile, DFCareer.Skills.Pickpocket, DFCareer.Skills.Streetwise };
                    return merchantSkills[UnityEngine.Random.Range(0, merchantSkills.Length)];
            }
        }

        void MessageBox_OnClose()
        {
            CloseWindow();
        }

        public override void Update()
        {
            base.Update();
        }

        public CharacterDocument Document { get; set; }

        public int ClassIndex
        {
            set { Document.classIndex = value; }
            get { return Document.classIndex; }
        }

        public List<string> PlayerEffects
        {
            get { return biogFile.AnswerEffects; }
        }

        public List<string> BackStory { get; set; }
    }
}