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
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Utility.AssetInjection;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Player;
using Newtonsoft.Json;

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
            new List<int> { 0, 10, 11, 12, 13, 14, 15, 16, 17 },      // 10."What kind of settlement your home was?"
            new List<int> { 0, 26, 27, 28, 29, 30 },                  // 11."What happened to your parents?"
            new List<int> { 0 },              // 12."How old were you when you left home?"
        };                                         

        int generalQuestionIndex = 0;
        int questionIndex = 0;
        int multipageIndex = 0;
        bool generalQuestionEnded = false;
        List<int> answers = new List<int>();
        Texture2D nativeTexture;
        TextLabel[] questionLabels = new TextLabel[questionLines];
        Button[] answerButtons = new Button[buttonCount];
        TextLabel[] answerLabels = new TextLabel[buttonCount];
        BiogFile biogFile;
        public StartLocQuestions sLQuestions;        

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
            }

            PopulateControls(generalQuestionIndex);
            // PopulateControls(biogFile.Questions[questionIndex]);

            IsSetup = true;
        }

        private void PopulateControls(int generalQuestionIndex)
        {
            questionLabels[0].Text = sLQuestions.Questions[generalQuestionIndex];
            // questionLabels[1].Text = string.Empty;

            switch(generalQuestionIndex)
            {
                case 0:
                case 10:
                case 11:
                    for (int i = 0; i < questionAnswers[generalQuestionIndex].Count; i++)
                    {
                        answerLabels[i].Text = sLQuestions.Answers[questionAnswers[generalQuestionIndex][i]];
                    }
                    break;

                case 1:
                    for (int i = 0; i < Enum.GetNames(typeof(ProvinceNames)).Length; i++)
                    {
                        if (i == 0)
                            answerLabels[i].Text = sLQuestions.Answers[0];
                        else answerLabels[i].Text = Enum.GetName(typeof(ProvinceNames), i);
                    }
                    break;

                case 9:
                    for (int i = 0; i < buttonCount; i++)
                    {
                        if (multipageIndex == 0 && i == 0)
                            answerLabels[i].Text = sLQuestions.Answers[0];
                        else if (i == 0)
                            answerLabels[i].Text = sLQuestions.Answers[sLQuestions.Answers.Length - 2];
                        else if (i < buttonCount - 1)
                            answerLabels[i].Text = WorldData.WorldSetting.RegionNames[WorldData.WorldSetting.regionInProvince[answers[1]][i + 8 * multipageIndex]];
                        else answerLabels[i].Text = sLQuestions.Answers[sLQuestions.Answers.Length - 1];
                    }
                    break;

                case 12:
                    switch(answers[generalQuestionIndex - 1])
                    {
                        case 26:
                            questionAnswers[generalQuestionIndex].AddRange(new List<int> { 19, 20, 21, 22, 23, 24 });
                            break;

                        case 27:
                        case 29:
                            questionAnswers[generalQuestionIndex].AddRange(new List<int> { 19, 20, 21 });
                            break;

                        case 28:
                            questionAnswers[generalQuestionIndex].AddRange(new List<int> { 19, 20 });
                            break;

                        case 30:
                            questionAnswers[generalQuestionIndex].AddRange(new List<int> { 20, 21, 22 });
                            break;
                    }
                    for (int i = 0; i < questionAnswers[generalQuestionIndex].Count; i++)
                    {
                        answerLabels[i].Text = sLQuestions.Answers[questionAnswers[generalQuestionIndex][i]];
                    }
                    break;

                default:
                    break;
            }
            for (int i = questionAnswers[generalQuestionIndex].Count; i < buttonCount; i++)
            {
                answerLabels[i].Text = string.Empty;
            }
        }

        private void PopulateControls(BiogFile.Question question)
        {
            questionLabels[0].Text = question.Text[0];
            questionLabels[1].Text = question.Text[1];
            for (int i = 0; i < question.Answers.Count; i++)
            {
                answerLabels[i].Text = question.Answers[i].Text;
            }
            // blank out remaining labels
            for (int i = question.Answers.Count; i < buttonCount; i++)
            {
                answerLabels[i].Text = string.Empty;
            }
        }

        void AnswerButton_OnMouseClick(BaseScreenComponent sender, Vector2 pos)
        {
            int answerIndex = (int)sender.Tag;

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
                if (answerIndex >= questionAnswers[generalQuestionIndex].Count)
                {
                    return; // not an answer for this question
                }
                else if (questionIndex < questionAnswers[generalQuestionIndex].Count - 1)
                {
                    // foreach (string effect in curAnswers[answerIndex].Effects)
                    // {
                    //     biogFile.AddEffect(effect, questionIndex);
                    // }
                    generalQuestionIndex++;
                    PopulateControls(generalQuestionIndex);
                }
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