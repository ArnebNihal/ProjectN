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
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.Player;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Entity;

namespace DaggerfallWorkshop.Game.UserInterfaceWindows
{
    /// <summary>
    /// Strings together the various parts of character creation and starting a new game.
    /// </summary>
    public class StartNewGameWizard : DaggerfallBaseWindow
    {
        const string newGameCinematic1 = "ANIM0000.VID";
        const string newGameCinematic2 = "ANIM0011.VID";
        const string newGameCinematic3 = "DAG2.VID";

        public const int startingLevelUpSkillsSum = 60;

        WizardStages wizardStage;
        CharacterDocument characterDocument = new CharacterDocument();
        StartGameBehaviour startGameBehaviour;

        CreateCharRaceSelect createCharRaceSelectWindow;
        CreateCharGenderSelect createCharGenderSelectWindow;
        CreateCharChooseClassGen createCharChooseClassGenWindow;
        CreateCharClassQuestions createCharClassQuestionsWindow;
        CreateCharClassSelect createCharClassSelectWindow;
        CreateCharCustomClass createCharCustomClassWindow;
        // CreateCharStartingLocation createCharStartingLocation;
        CreateCharChooseBio createCharChooseBioWindow;
        CreateCharBiography createCharBiographyWindow;
        CreateCharNameSelect createCharNameSelectWindow;
        CreateCharFaceSelect createCharFaceSelectWindow;
        CreateCharAddBonusStats createCharAddBonusStatsWindow;
        CreateCharAddBonusSkills createCharAddBonusSkillsWindow;
        CreateCharReflexSelect createCharReflexSelectWindow;
        CreateCharSummary createCharSummaryWindow;

        WizardStages WizardStage
        {
            get { return wizardStage; }
        }

        public enum WizardStages
        {
            SelectRace,
            SelectGender,
            SelectClassMethod,
            GenerateClass,
            SelectClassFromList,
            CustomClassBuilder,
            SelectBiographyMethod,
            BiographyQuestions,
            SelectName,
            SelectFace,
            AddBonusStats,
            AddBonusSkills,
            SelectReflexes,
            Summary,
        }

        public StartNewGameWizard(IUserInterfaceManager uiManager)
            : base(uiManager)
        {
        }

        protected override void Setup()
        {
            // Must have a start game object to transmit character sheet
            startGameBehaviour = GameObject.FindObjectOfType<StartGameBehaviour>();
            if (!startGameBehaviour)
                throw new Exception("Could not find StartGameBehaviour in scene.");

            // Wizard starts with race selection
            SetRaceSelectWindow();
        }

        public override void Update()
        {
            base.Update();

            // If user has backed out of all windows then drop back to start screen
            if (uiManager.TopWindow == this)
                CloseWindow();
        }

        public override void Draw()
        {
            base.Draw();
        }

        #region Window Management

        void SetRaceSelectWindow()
        {
            if (createCharRaceSelectWindow == null)
            {
                createCharRaceSelectWindow = new CreateCharRaceSelect(uiManager);
                createCharRaceSelectWindow.OnClose += RaceSelectWindow_OnClose;
            }

            createCharRaceSelectWindow.Reset();
            characterDocument.raceTemplate = null;

            wizardStage = WizardStages.SelectRace;

            if (uiManager.TopWindow != createCharRaceSelectWindow)
                uiManager.PushWindow(createCharRaceSelectWindow);
        }

        void SetGenderSelectWindow()
        {
            if (createCharGenderSelectWindow == null)
            {
                createCharGenderSelectWindow = new CreateCharGenderSelect(uiManager, createCharRaceSelectWindow);
                createCharGenderSelectWindow.OnClose += GenderSelectWindow_OnClose;
            }

            wizardStage = WizardStages.SelectGender;
            uiManager.PushWindow(createCharGenderSelectWindow);
        }

        void SetChooseClassGenWindow()
        {
            createCharChooseClassGenWindow = new CreateCharChooseClassGen(uiManager, createCharRaceSelectWindow);
            createCharChooseClassGenWindow.OnClose += ChooseClassGen_OnClose;
            wizardStage = WizardStages.SelectClassMethod;
            uiManager.PushWindow(createCharChooseClassGenWindow);
        }

        void SetClassQuestionsWindow()
        {
            createCharClassQuestionsWindow = new CreateCharClassQuestions(uiManager);
            createCharClassQuestionsWindow.OnClose += CreateCharClassQuestions_OnClose;
            wizardStage = WizardStages.GenerateClass;
            uiManager.PushWindow(createCharClassQuestionsWindow);
        }

        void SetClassSelectWindow()
        {
            if (createCharClassSelectWindow == null)
            {
                createCharClassSelectWindow = new CreateCharClassSelect(uiManager, createCharRaceSelectWindow);
                createCharClassSelectWindow.OnClose += ClassSelectWindow_OnClose;
            }

            wizardStage = WizardStages.SelectClassFromList;
            uiManager.PushWindow(createCharClassSelectWindow);
        }

        void SetCustomClassWindow()
        {
            createCharCustomClassWindow = new CreateCharCustomClass(uiManager);
            createCharCustomClassWindow.OnClose += CreateCharCustomClassWindow_OnClose;
            wizardStage = WizardStages.CustomClassBuilder;
            uiManager.PushWindow(createCharCustomClassWindow);
        }

        void SetChooseBioWindow()
        {
            createCharChooseBioWindow = new CreateCharChooseBio(uiManager, createCharRaceSelectWindow);
            createCharChooseBioWindow.OnClose += CreateCharChooseBioWindow_OnClose;

            wizardStage = WizardStages.SelectBiographyMethod;
            uiManager.PushWindow(createCharChooseBioWindow);
        }

        void SetBiographyWindow()
        {
            createCharBiographyWindow = new CreateCharBiography(uiManager, characterDocument);
            createCharBiographyWindow.OnClose += CreateCharBiographyWindow_OnClose;
                
            createCharBiographyWindow.ClassIndex = characterDocument.classIndex;
            wizardStage = WizardStages.BiographyQuestions;
            uiManager.PushWindow(createCharBiographyWindow);
        }

        void SetNameSelectWindow()
        {
            if (createCharNameSelectWindow == null)
            {
                createCharNameSelectWindow = new CreateCharNameSelect(uiManager);
                createCharNameSelectWindow.OnClose += NameSelectWindow_OnClose;
            }

            createCharNameSelectWindow.RaceTemplate = characterDocument.raceTemplate;
            createCharNameSelectWindow.Gender = characterDocument.gender;

            wizardStage = WizardStages.SelectName;
            uiManager.PushWindow(createCharNameSelectWindow);
        }

        void SetFaceSelectWindow()
        {
            if (createCharFaceSelectWindow == null)
            {
                createCharFaceSelectWindow = new CreateCharFaceSelect(uiManager);
                createCharFaceSelectWindow.OnClose += FaceSelectWindow_OnClose;
            }

            createCharFaceSelectWindow.SetFaceTextures(characterDocument.raceTemplate, characterDocument.gender);

            wizardStage = WizardStages.SelectFace;
            uiManager.PushWindow(createCharFaceSelectWindow);
        }

        void SetAddBonusStatsWindow()
        {
            PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
            if (createCharAddBonusStatsWindow == null)
            {
                createCharAddBonusStatsWindow = new CreateCharAddBonusStats(uiManager);
                createCharAddBonusStatsWindow.OnClose += AddBonusStatsWindow_OnClose;
                createCharAddBonusStatsWindow.DFClass = characterDocument.career;
                createCharAddBonusStatsWindow.Race = characterDocument.raceTemplate;
                createCharAddBonusStatsWindow.CharacterGender = characterDocument.gender;
                createCharAddBonusStatsWindow.Age = characterDocument.age;
                createCharAddBonusStatsWindow.Reroll();
            }

            // Update class and reroll if player changed class, race or gender selection
            if (createCharAddBonusStatsWindow.DFClass != characterDocument.career ||
                createCharAddBonusStatsWindow.Race != characterDocument.raceTemplate ||
                createCharAddBonusStatsWindow.CharacterGender != characterDocument.gender)
            {
                createCharAddBonusStatsWindow.DFClass = characterDocument.career;
                createCharAddBonusStatsWindow.Race = characterDocument.raceTemplate;
                createCharAddBonusStatsWindow.CharacterGender = characterDocument.gender;
                createCharAddBonusStatsWindow.Reroll();
            }

            wizardStage = WizardStages.AddBonusStats;
            uiManager.PushWindow(createCharAddBonusStatsWindow);
        }

        void SetAddBonusSkillsWindow()
        {
            if (createCharAddBonusSkillsWindow == null)
            {
                createCharAddBonusSkillsWindow = new CreateCharAddBonusSkills(uiManager);
                createCharAddBonusSkillsWindow.OnClose += AddBonusSkillsWindow_OnClose;
                createCharAddBonusSkillsWindow.Age = characterDocument.age;
                createCharAddBonusSkillsWindow.DFClass = characterDocument.career;
                createCharAddBonusSkillsWindow.SkillBonuses = BiogFile.GetSkillEffects(characterDocument.biographyEffects);
            }

            // Update class if player changes class selection
            if (createCharAddBonusSkillsWindow.DFClass != characterDocument.career ||
                createCharAddBonusSkillsWindow.Age != characterDocument.age)
            {
                createCharAddBonusSkillsWindow.Age = characterDocument.age;
                createCharAddBonusSkillsWindow.DFClass = characterDocument.career;
            }

            wizardStage = WizardStages.AddBonusSkills;
            uiManager.PushWindow(createCharAddBonusSkillsWindow);
        }

        void SetSelectReflexesWindow()
        {
            if (createCharReflexSelectWindow == null)
            {
                createCharReflexSelectWindow = new CreateCharReflexSelect(uiManager);
                createCharReflexSelectWindow.OnClose += ReflexSelectWindow_OnClose;
            }

            wizardStage = WizardStages.SelectReflexes;
            uiManager.PushWindow(createCharReflexSelectWindow);
        }

        void SetSummaryWindow()
        {
            if (createCharSummaryWindow == null)
            {
                createCharSummaryWindow = new CreateCharSummary(uiManager);
                createCharSummaryWindow.OnRestart += SummaryWindow_OnRestart;
                createCharSummaryWindow.OnClose += SummaryWindow_OnClose;
            }

            createCharSummaryWindow.CharacterDocument = characterDocument;

            wizardStage = WizardStages.Summary;
            uiManager.PushWindow(createCharSummaryWindow);
        }

        #endregion

        #region Event Handlers

        void RaceSelectWindow_OnClose()
        {
            if (!createCharRaceSelectWindow.Cancelled)
            {
                characterDocument.raceTemplate = createCharRaceSelectWindow.SelectedRace;
                SetGenderSelectWindow();
            }
            else
            {
                characterDocument.raceTemplate = null;
            }
        }

        void GenderSelectWindow_OnClose()
        {
            if (!createCharGenderSelectWindow.Cancelled)
            {
                characterDocument.gender = createCharGenderSelectWindow.SelectedGender;
                SetNameSelectWindow();
            }
            else
            {
                SetRaceSelectWindow();
            }
        }

        void NameSelectWindow_OnClose()
        {
            if (!createCharNameSelectWindow.Cancelled)
            {
                characterDocument.age = createCharNameSelectWindow.CharacterAge;
                characterDocument.birthday = createCharNameSelectWindow.CharacterBirthday;
                characterDocument.name = createCharNameSelectWindow.CharacterName;
                SetFaceSelectWindow();
            }
            else
            {
                SetRaceSelectWindow();
            }
        }

        void FaceSelectWindow_OnClose()
        {
            if (!createCharFaceSelectWindow.Cancelled)
            {
                characterDocument.faceIndex = createCharFaceSelectWindow.FaceIndex;
                SetChooseClassGenWindow();
            }
            else
            {
                SetNameSelectWindow();
            }
        }

        void ChooseClassGen_OnClose()
        {
            if (createCharChooseClassGenWindow.ChoseGenerate)
            {
                SetClassQuestionsWindow();
            }
            else
            {
                SetClassSelectWindow();
            }
        }

        void CreateCharClassQuestions_OnClose()
        {
            byte classIndex = createCharClassQuestionsWindow.ClassIndex;
            if (classIndex != CreateCharClassQuestions.noClassIndex)
            {
                string fileName = "CLASS" + classIndex.ToString("00") + ".CFG";
                string[] files = Directory.GetFiles(DaggerfallUnity.Instance.Arena2Path, fileName);
                if (files == null)
                {
                    throw new Exception("Could not load class file: " + fileName);
                }
                ClassFile classFile = new ClassFile(files[0]);
                characterDocument.career = classFile.Career;
                characterDocument.classIndex = classIndex;
                SetChooseBioWindow();
            }
            else
            {
                SetClassSelectWindow();
            }
        }

        void ClassSelectWindow_OnClose()
        {
            if (!createCharClassSelectWindow.Cancelled)
            {
                if (createCharClassSelectWindow.SelectedClass == null) // Custom class
                {
                    characterDocument.isCustom = true;
                    SetCustomClassWindow();
                }
                else
                {
                    characterDocument.career = createCharClassSelectWindow.SelectedClass;
                    characterDocument.classIndex = createCharClassSelectWindow.SelectedClassIndex;
                    SetChooseBioWindow();
                }
            }
            else
            {
                SetFaceSelectWindow();
            }
        }

        void CreateCharCustomClassWindow_OnClose()
        {
            if (!createCharCustomClassWindow.Cancelled)
            {
                characterDocument.career = createCharCustomClassWindow.CreatedClass;
                characterDocument.career.Name = createCharCustomClassWindow.ClassName;

                // Determine the most similar class so that we can choose the biography quiz
                characterDocument.classIndex = BiogFile.GetClassAffinityIndex(characterDocument.career, createCharClassSelectWindow.ClassList);

                // Set reputation adjustments
                characterDocument.reputationMerchants = createCharCustomClassWindow.MerchantsRep;
                characterDocument.reputationCommoners = createCharCustomClassWindow.PeasantsRep;
                characterDocument.reputationScholars = createCharCustomClassWindow.ScholarsRep;
                characterDocument.reputationNobility = createCharCustomClassWindow.NobilityRep;
                characterDocument.reputationUnderworld = createCharCustomClassWindow.UnderworldRep;

                // Set attributes
                characterDocument.career.Strength = createCharCustomClassWindow.Stats.WorkingStats.LiveStrength;
                characterDocument.career.Intelligence = createCharCustomClassWindow.Stats.WorkingStats.LiveIntelligence;
                characterDocument.career.Willpower = createCharCustomClassWindow.Stats.WorkingStats.LiveWillpower;
                characterDocument.career.Agility = createCharCustomClassWindow.Stats.WorkingStats.LiveAgility;
                characterDocument.career.Endurance = createCharCustomClassWindow.Stats.WorkingStats.LiveEndurance;
                characterDocument.career.Personality = createCharCustomClassWindow.Stats.WorkingStats.LivePersonality;
                characterDocument.career.Speed = createCharCustomClassWindow.Stats.WorkingStats.LiveSpeed;
                characterDocument.career.Luck = createCharCustomClassWindow.Stats.WorkingStats.LiveLuck;

                SetChooseBioWindow();
            }
            else
            {
                SetClassSelectWindow();
            }
        }

        void CreateCharChooseBioWindow_OnClose()
        {
            if (!createCharChooseBioWindow.Cancelled)
            {
                // Pick a biography template, 0 by default
                // Classic only has a T0 template for each class, but mods can add more
                Regex reg = new Regex($"BIOG{characterDocument.classIndex:D2}T([0-9]+).TXT");
                IEnumerable<Match> biogMatches = Directory.EnumerateFiles(BiogFile.BIOGSourceFolder, "*.TXT")
                    .Select(FilePath => reg.Match(FilePath))
                    .Where(FileMatch => FileMatch.Success);

                // For now, we choose at random between all available ones
                // Maybe eventually, have a window for selecting a biography template when more than 1 is available?
                // int biogCount = biogMatches.Count();
                // int selectedBio = UnityEngine.Random.Range(0, biogCount);
                // Match selectedMatch = biogMatches.ElementAt(selectedBio);
                // characterDocument.biographyIndex = int.Parse(selectedMatch.Groups[1].Value);

                if (!createCharChooseBioWindow.ChoseQuestions)
                {
                    // Choose answers at random
                    System.Random rand = new System.Random(System.DateTime.Now.Millisecond);
                    BiogFile autoBiog = new BiogFile(characterDocument);
                    for (int i = 0; i < autoBiog.Questions.Length; i++)
                    {
                        List<BiogFile.Answer> answers;
                        answers = autoBiog.Questions[i].Answers;
                        int index = rand.Next(0, answers.Count);
                        for (int j = 0; j < answers[index].Effects.Count; j++)
                        {
                            autoBiog.AddEffect(answers[index].Effects[j], i);
                        }
                    }
                    // Show reputation changes
                    autoBiog.DigestRepChanges();
                    DaggerfallMessageBox messageBox = new DaggerfallMessageBox(uiManager, createCharChooseBioWindow);
                    messageBox.SetTextTokens(CreateCharBiography.reputationToken, autoBiog);
                    messageBox.ClickAnywhereToClose = true;
                    messageBox.Show();
                    messageBox.OnClose += ReputationBox_OnClose;

                    characterDocument.biographyEffects = autoBiog.AnswerEffects;
                    characterDocument.backStory = autoBiog.GenerateBackstory();
                }
                else
                {
                    SetBiographyWindow();
                }
            }
            else
            {
                SetClassSelectWindow();
            }
        }

        private void ReputationBox_OnClose()
        {
            SetAddBonusStatsWindow();
        }

        void CreateCharBiographyWindow_OnClose()
        {
            if (!createCharBiographyWindow.Cancelled)
            {
                characterDocument.backStory = createCharBiographyWindow.BackStory;
                characterDocument.biographyEffects = createCharBiographyWindow.PlayerEffects;
                SetAddBonusStatsWindow();
            }
            else
            {
                SetChooseBioWindow();
            }
        }

        void AddBonusStatsWindow_OnClose()
        {
            if (!createCharAddBonusStatsWindow.Cancelled)
            {
                characterDocument.startingStats = createCharAddBonusStatsWindow.StartingStats;
                characterDocument.workingStats = createCharAddBonusStatsWindow.WorkingStats;
                SetAddBonusSkillsWindow();
            }
            else
            {
                SetChooseBioWindow();
            }
        }

        void AddBonusSkillsWindow_OnClose()
        {
            if (!createCharAddBonusSkillsWindow.Cancelled)
            {
                characterDocument.startingSkills = createCharAddBonusSkillsWindow.StartingSkills;
                characterDocument.workingSkills = createCharAddBonusSkillsWindow.WorkingSkills;
                SetSelectReflexesWindow();
            }
            else
            {
                SetAddBonusStatsWindow();
            }
        }

        void ReflexSelectWindow_OnClose()
        {
            if (!createCharReflexSelectWindow.Cancelled)
            {
                characterDocument.reflexes = createCharReflexSelectWindow.PlayerReflexes;
                SetSummaryWindow();
            }
            else
            {
                SetAddBonusSkillsWindow();
            }
        }

        private void SummaryWindow_OnRestart()
        {
            SetRaceSelectWindow();
        }

        void SummaryWindow_OnClose()
        {
            if (!createCharSummaryWindow.Cancelled)
            {
                characterDocument = createCharSummaryWindow.GetUpdatedCharacterDocument();
                StartNewGame();
            }
            else
            {
                SetSelectReflexesWindow();
            }
        }

        #endregion

        #region Game Startup Methods

        void StartNewGame()
        {
            // Assign character document to player entity
            startGameBehaviour.CharacterDocument = characterDocument;

            if (DaggerfallUI.Instance.enableVideos)
            {
                // Create cinematics
                // DaggerfallVidPlayerWindow cinematic1 = (DaggerfallVidPlayerWindow)UIWindowFactory.GetInstanceWithArgs(UIWindowType.VidPlayer, new object[] { uiManager, newGameCinematic1 });
                // DaggerfallVidPlayerWindow cinematic2 = (DaggerfallVidPlayerWindow)UIWindowFactory.GetInstanceWithArgs(UIWindowType.VidPlayer, new object[] { uiManager, newGameCinematic2 });
                // DaggerfallVidPlayerWindow cinematic3 = (DaggerfallVidPlayerWindow)UIWindowFactory.GetInstanceWithArgs(UIWindowType.VidPlayer, new object[] { uiManager, newGameCinematic3 });

                // End of final cinematic will launch game
                // cinematic3.OnVideoFinished += TriggerGame;

                // Push cinematics in reverse order so they play and pop out in correct order
                // uiManager.PushWindow(cinematic3);
                // uiManager.PushWindow(cinematic2);
                // uiManager.PushWindow(cinematic1);
                TriggerGame();
            }
            else
            {
                TriggerGame();
            }
        }

        void TriggerGame()
        {
            startGameBehaviour.StartMethod = StartGameBehaviour.StartMethods.NewCharacter;
        }

        #endregion
    }
}