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

using System;
using DaggerfallConnect.Utility;
using DaggerfallWorkshop;
using System.IO;
using System.Collections.Generic;
using DaggerfallWorkshop.Game.Entity;
using UnityEngine;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.Player;
using DaggerfallWorkshop.Utility;
using Newtonsoft.Json;
using System.Linq;
using DaggerfallWorkshop.Game.UserInterfaceWindows;

namespace DaggerfallConnect.Arena2
{
    public partial class BiogFile
    {
        const int questionCount = 12;
        const int socialGroupCount = 5;
        const int defaultBackstoriesStart = 4116;
        static int[] childGeneric = { 0, 1000, 3000, 16000 };  // Question 2000 is excluded
        static int[] adolescentGeneric = { 3001, 4000, 5000, 5001 };
        static int[] youngadultGeneric = { 5002, 6000, 8000, 17000 };  // Question 7000 is excluded
        static int[] adultGeneric = { 9000, 10000, 11000, 11001, 11002, 11003, 11004, 12000, 13000, 14000, 15000 };

        // Folder names constants
        const string biogSourceFolderName = "BIOGs";

        string questionsStr = string.Empty;
        Question[] questions = new Question[questionCount];
        int[] questionArray = new int[questionCount];
        short[] changedReputations = new short[socialGroupCount];
        List<string> answerEffects = new List<string>();
        CharacterDocument characterDocument;
        public static int backstoryId;
        public BiogText biogText = new BiogText();
        public BiogQuestions[] biogQuestions;

        public class BiogQuestions
        {
            public BiogQuestType QuestType;
            public string[] Question;
            public BiogAnswers[] Answers;
        }

        public class BiogAnswers
        {
            public string[] Answer;
            public string[][] BiogText;
            public BiogAnswerEffect[] Effect;
            public string[] EffectDetails;
        }

        public struct SituationalBiogs
        {
            public string[] Standard;
            public string[] Orphan;
            public string[] WithUncles;
        }

        public class BiogText
        {
            public string[] GeneralBiogs;
            public SituationalBiogs[] QuestionBiogs;    // Is this really needed?
            // public static SituationalBiogs SpellcasterBiogs;
            // public static SituationalBiogs RogueSpellcasterBiogs;
            // public static SituationalBiogs ThiefBiogs;
            // public static SituationalBiogs CarnivalBiogs;
            // public static SituationalBiogs VillageBiogs;
            // public static SituationalBiogs OrcishBiogs;
            public string[] BiogParts;

            public BiogText()
            {

            }

            // public BiogText(List<int> answers, out TextFile.Token[] usedTokens)
            // {
            //     JsonConvert.DeserializeObject<BiogText>(File.ReadAllText(Path.Combine(WorldMaps.mapPath, "BiogText.json")));

            //     List<TextFile.Token> tokenList = new List<TextFile.Token>();
            //     TextFile.Token[] newBiogTokens, originalBiogTokens, biogPartTokens;

            //     // Adding the first newBiogs, since it should be good for every backstory
            //     tokenList.Add(newBiogTokens[0]);

            //     if (answers[5] == 1)    // Just a baby when leaving home
            //         tokenList.Add(newBiogTokens[1]);    // "Or so you were told..."

            //     tokenList.Add(originalBiogTokens[0 + backstoryId * 10]);    // First memories

            //     if (answers[5] == 2)    // A child when leaving home
            //         tokenList.Add(newBiogTokens[2]);    // However your permanence there was short-lived...
                
            //     tokenList.Add(originalBiogTokens[1 + backstoryId * 10]);    // As you grew...

            //     if (answers[5] == 3 || answers[5] == 4) // Adolescent or young adult when leaving home
            //         tokenList.Add(newBiogTokens[3]);

            //     for (int i = 0; i < NewBiogs.Length; i++)
            //     {
            //         string[] textToToken = NewBiogs[i].Split('*');
            //         TextFile.Token[] tokens = new TextFile.Token[textToToken.Length];

            //         for (int j = 0; j < textToToken.Length; j++)
            //         {
            //             tokens[j] = new TextFile.Token(TextFile.Formatting.Text, textToToken[j]);
            //         }
            //         tokenList.AddRange(tokens.ToList());
            //     }
            //     newBiogTokens = tokenList.ToArray();

            //     tokenList = new List<TextFile.Token>();
            //     for (int k = 0; k < OriginalBiogs.Length; k++)
            //     {
            //         string[] textToToken = NewBiogs[k].Split('*');
            //         TextFile.Token[] tokens = new TextFile.Token[textToToken.Length];

            //         for (int l = 0; l < textToToken.Length; l++)
            //         {
            //             tokens[l] = new TextFile.Token(TextFile.Formatting.Text, textToToken[l]);
            //         }
            //         tokenList.AddRange(tokens.ToList());
            //     }
            //     originalBiogTokens = tokenList.ToArray();

            //     tokenList = new List<TextFile.Token>();
            //     for (int m = 0; m < NewBiogs.Length; m++)
            //     {
            //         string[] textToToken = NewBiogs[m].Split('*');
            //         TextFile.Token[] tokens = new TextFile.Token[textToToken.Length];

            //         for (int n = 0; n < textToToken.Length; n++)
            //         {
            //             tokens[n] = new TextFile.Token(TextFile.Formatting.Text, textToToken[n]);
            //         }
            //         tokenList.AddRange(tokens.ToList());
            //     }
            //     biogPartTokens = tokenList.ToArray();

            //     tokenList = new List<TextFile.Token>();
                

            // }
        }

        public static string BIOGSourceFolder
        {
            get { return Path.Combine(Application.streamingAssetsPath, biogSourceFolderName); }
        }

        public BiogFile(CharacterDocument characterDocument)
        {
            // Store reference to character document
            this.characterDocument = characterDocument;

            // Load text file
            string fileName = "BiogQuestions.json";
            biogText = JsonConvert.DeserializeObject<BiogText>(File.ReadAllText(Path.Combine(WorldMaps.mapPath, "BiogText.json")));
            biogQuestions = JsonConvert.DeserializeObject<BiogQuestions[]>(File.ReadAllText(Path.Combine(WorldMaps.mapPath, fileName)));
            // FileProxy txtFile = new FileProxy(Path.Combine(BiogFile.BIOGSourceFolder, fileName), FileUsage.UseDisk, true);
            // questionsStr = System.Text.Encoding.UTF8.GetString(txtFile.GetBytes());

            int[] questionTypes = CountQuestionTypes(this.characterDocument);

            int[] questionAssignment = new int[12];
            for (int h = 0; h < 12; h++)
            {
                questionAssignment[h] = -1;
            }

            PickQuestion(questionTypes, out questionAssignment);

            int questionIndex = -1;
            int subQuestionIndex = -1;

            // Parse text into questions
            // StringReader reader = new StringReader(questionsStr);
            // string curLine = reader.ReadLine();
            for (int i = 0; i < questionCount; i++)
            {
                questions[i] = new Question();

                questionArray[i] = questionAssignment[i];
                questionIndex = questionAssignment[i] / 1000;
                subQuestionIndex = questionAssignment[i] % 1000;

                BiogQuestions currentQuestion = biogQuestions[questionIndex];

                // Here we check if the question has some "macro" to substitute with
                // a certain snippet of text; while writing this, it's just two questions,
                // one in magic and one in combat. It removes the redundant answer, too.
                if (currentQuestion.Question[subQuestionIndex].Contains('%'))
                {
                    DFCareer.Skills magicOut;
                    DFCareer.Skills combatOut;
                    string substitute = SubstituteMacro(currentQuestion.Question[subQuestionIndex], this.characterDocument, out magicOut, out combatOut);
                    currentQuestion.Question[subQuestionIndex] = substitute;
                    List<BiogAnswers> answerList = new List<BiogAnswers>();
                    
                    if (currentQuestion.QuestType == BiogQuestType.Magic)
                    {
                        answerList = currentQuestion.Answers.ToList();
                        answerList.RemoveAt((int)magicOut - (int)DFCareer.MagicSkills.Destruction);
                        currentQuestion.Answers = answerList.ToArray();
                    }
                    else if (currentQuestion.QuestType == BiogQuestType.Combat)
                    {
                        answerList = currentQuestion.Answers.ToList();
                        answerList.RemoveAt(answerList.FindIndex(x => x.EffectDetails[0].StartsWith(((int)combatOut).ToString())));
                        currentQuestion.Answers = answerList.ToArray();
                    }
                }

                if (!currentQuestion.Question[subQuestionIndex].Contains('*'))
                {
                    questions[i].text[0] = currentQuestion.Question[subQuestionIndex];
                    questions[i].text[1] = string.Empty;
                }
                else{
                    questions[i].text = currentQuestion.Question[subQuestionIndex].Split('*');
                }

                Answer ans = new Answer();
                for (int j = 0; j < currentQuestion.Answers.Length; j++)
                {
                    ans = new Answer();
                    int effectIndex;
                    int answerIndex = GetAnswerIndex(questionIndex, subQuestionIndex, out effectIndex);
                    Debug.Log("questionIndex: " + questionIndex + ", subQuestionIndex: " + subQuestionIndex + ", answerIndex: " + answerIndex + ", effectIndex: " + effectIndex);

                    if (answerIndex >= currentQuestion.Answers[j].Answer.Length) answerIndex = 0;
                    if (effectIndex >= currentQuestion.Answers[j].Effect.Length && effectIndex != -1) effectIndex = 0;

                    if (answerIndex >= currentQuestion.Answers[j].Answer.Length)
                        answerIndex = 0;
                    ans.Text = currentQuestion.Answers[j].Answer[answerIndex];
                    if (effectIndex != -1)
                    {
                        if (effectIndex >= currentQuestion.Answers[j].EffectDetails.Length)
                            effectIndex = 0;
                        if (currentQuestion.Answers[j].EffectDetails[effectIndex].Contains('&'))
                        {
                            string[] multEDSplit = currentQuestion.Answers[j].EffectDetails[effectIndex].Split('&');
                            ans.effects.Add((int)currentQuestion.Answers[j].Effect[effectIndex] + "*" + multEDSplit[UnityEngine.Random.Range(0, multEDSplit.Length)]);
                        }
                        else ans.effects.Add((int)currentQuestion.Answers[j].Effect[effectIndex] + "*" + currentQuestion.Answers[j].EffectDetails[0]);
                    }
                    else
                    {
                        for (int k = 0; k < currentQuestion.Answers[j].EffectDetails.Length; k++)
                        {
                            if (currentQuestion.Answers[j].EffectDetails[k].Contains('&'))
                            {
                                string[] multEDSplit = currentQuestion.Answers[j].EffectDetails[k].Split('&');
                                ans.effects.Add((int)currentQuestion.Answers[j].Effect[k] + "*" + multEDSplit[UnityEngine.Random.Range(0, multEDSplit.Length)]);
                            }
                            else ans.effects.Add((int)currentQuestion.Answers[j].Effect[k] + "*" + currentQuestion.Answers[j].EffectDetails[k]);
                        }
                    }
                    questions[i].answers.Add(ans);
                }
                
            }

            // Initialize reputation changes
            for (int i = 0; i < changedReputations.Length; i++)
            {
                changedReputations[i] = 0;
            }

            // Initialize question token lists
            Q1Tokens = new List<int>();
            Q2Tokens = new List<int>();
            Q3Tokens = new List<int>();
            Q4Tokens = new List<int>();
            Q5Tokens = new List<int>();
            Q6Tokens = new List<int>();
            Q7Tokens = new List<int>();
            Q8Tokens = new List<int>();
            Q9Tokens = new List<int>();
            Q10Tokens = new List<int>();
            Q11Tokens = new List<int>();
            Q12Tokens = new List<int>();
        }

        public string SubstituteMacro(string question, CharacterDocument characterDocument, out DFCareer.Skills magicOut, out DFCareer.Skills combatOut)
        {
            string magicSkill = string.Empty;
            string combatSkill = string.Empty;
            magicOut = combatOut = DFCareer.Skills.None;

            DFCareer.Skills skill;

            for (int i = 0; i < 12; i++)
            {
                skill = RotateSkills(i, characterDocument);
                if ((int)skill >= (int)DFCareer.MagicSkills.Destruction &&
                    (int)skill <= (int)DFCareer.MagicSkills.Mysticism &&
                    magicSkill == string.Empty)
                {
                    magicSkill = skill.ToString();
                    magicOut = skill;
                }

                if ((int)skill >= (int)DFCareer.Skills.ShortBlade &&
                    (int)skill <= (int)DFCareer.Skills.Archery &&
                    combatSkill == string.Empty)
                {
                    combatSkill = skill.ToString();
                    combatOut = skill;
                }
            }

            if (question.Contains("[%ms]"))
                return question.Replace("[%ms]", magicSkill);

            if (question.Contains("[%cs]"))
                return question.Replace("[%cs]", combatSkill);

            return string.Empty;
        }

        public DFCareer.Skills RotateSkills(int index, CharacterDocument characterDocument)
        {
            switch (index)
            {
                case 0:
                    return characterDocument.career.PrimarySkill1;
                case 1:
                    return characterDocument.career.PrimarySkill2;
                case 2:
                    return characterDocument.career.PrimarySkill3;
                case 3:
                    return characterDocument.career.MajorSkill1;
                case 4:
                    return characterDocument.career.MajorSkill2;
                case 5:
                    return characterDocument.career.MajorSkill3;
                case 6:
                    return characterDocument.career.MinorSkill1;
                case 7:
                    return characterDocument.career.MinorSkill2;
                case 8:
                    return characterDocument.career.MinorSkill3;
                case 9:
                    return characterDocument.career.MinorSkill4;
                case 10:
                    return characterDocument.career.MinorSkill5;
                case 11:
                    return characterDocument.career.MinorSkill6;
                default:
                    return DFCareer.Skills.None;
            }
        }

        public int GetAnswerIndex(int questionIndex, int subQuestionIndex, out int effectIndex)
        {
            effectIndex = -1;   // if effectIndex is given as -1, every effect is applied
            if (biogQuestions[questionIndex].Answers.Length == 1)   // If there's only one possible answer, return index 0
                return 0;

            switch (questionIndex)
            {
                case 5:
                    switch (subQuestionIndex)
                    {
                        case 0:
                        case 1:
                            return 0;

                        case 2:
                        default:
                            return 1;
                    }

                case 11:
                    switch (subQuestionIndex)
                    {
                        case 0:
                            effectIndex = 0;
                            return 0;

                        case 1:
                            effectIndex = 0;
                            return 1;

                        case 2:
                            effectIndex = 1;
                            return 2;

                        case 3:
                        case 4:
                            effectIndex = 3;
                            return 3;

                        default:
                            return 0;
                    }

                case 16:
                    switch (subQuestionIndex)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 3:
                            effectIndex = 0;
                            return UnityEngine.Random.Range(0, 2);

                        case 4:
                            effectIndex = 1;
                            return 0;

                        case 5:
                            effectIndex = 1;
                            return 1;

                        default:
                            return 0;
                    }

                default:
                    return 0;
            }
        }

        /// <summary>
        /// Count how many questions to select based on skill types
        /// </summary>
        public int[] CountQuestionTypes(CharacterDocument characterDocument)
        {
            int[] questionTypes = { 0, 0, 0, 0, 0};

            questionTypes[GetSkillType(characterDocument.career.PrimarySkill1)] += 100;
            questionTypes[GetSkillType(characterDocument.career.PrimarySkill2)] += 100;
            questionTypes[GetSkillType(characterDocument.career.PrimarySkill3)] +=100;

            questionTypes[GetSkillType(characterDocument.career.MajorSkill1)] +=10;
            questionTypes[GetSkillType(characterDocument.career.MajorSkill2)] +=10;
            questionTypes[GetSkillType(characterDocument.career.MajorSkill3)] +=10;

            questionTypes[GetSkillType(characterDocument.career.MinorSkill1)]++;
            questionTypes[GetSkillType(characterDocument.career.MinorSkill2)]++;
            questionTypes[GetSkillType(characterDocument.career.MinorSkill3)]++;
            questionTypes[GetSkillType(characterDocument.career.MinorSkill4)]++;
            questionTypes[GetSkillType(characterDocument.career.MinorSkill5)]++;
            questionTypes[GetSkillType(characterDocument.career.MinorSkill6)]++;

            return questionTypes;
        }

        public int GetSkillType(DFCareer.Skills skill)
        {
            switch (skill)
            {
                case DFCareer.Skills.Medical:
                case DFCareer.Skills.Etiquette:
                case DFCareer.Skills.Mercantile:
                case DFCareer.Skills.Running:
                case DFCareer.Skills.Streetwise:
                case DFCareer.Skills.Swimming:
                    return (int)BiogQuestType.Generic;
                    break;

                case DFCareer.Skills.Alteration:
                case DFCareer.Skills.Destruction:
                case DFCareer.Skills.Illusion:
                case DFCareer.Skills.Mysticism:
                case DFCareer.Skills.Restoration:
                case DFCareer.Skills.Thaumaturgy:
                    return (int)BiogQuestType.Magic;
                    break;

                case DFCareer.Skills.Archery:
                case DFCareer.Skills.Axe:
                case DFCareer.Skills.Block:
                case DFCareer.Skills.BluntWeapon:
                case DFCareer.Skills.HandToHand:
                case DFCareer.Skills.HeavyArmour:
                case DFCareer.Skills.LongBlade:
                case DFCareer.Skills.MediumArmour:
                case DFCareer.Skills.ShortBlade:
                    return (int)BiogQuestType.Combat;
                    break;

                case DFCareer.Skills.Backstabbing:
                case DFCareer.Skills.Climbing:
                case DFCareer.Skills.CriticalStrike:
                case DFCareer.Skills.Disguise:
                case DFCareer.Skills.Dodging:
                case DFCareer.Skills.Jumping:
                case DFCareer.Skills.LightArmour:
                case DFCareer.Skills.Lockpicking:
                case DFCareer.Skills.Pickpocket:
                case DFCareer.Skills.Stealth:
                    return (int)BiogQuestType.Thievery;
                    break;

                case DFCareer.Skills.Centaurian:
                case DFCareer.Skills.Daedric:
                case DFCareer.Skills.Dragonish:
                case DFCareer.Skills.Giantish:
                case DFCareer.Skills.Harpy:
                case DFCareer.Skills.Impish:
                case DFCareer.Skills.Nymph:
                case DFCareer.Skills.Orcish:
                case DFCareer.Skills.Spriggan:
                    return (int)BiogQuestType.Languages;
                    break;

                default:
                    Debug.Log("Skill not present in this switch!");
                    return -1;                
            }
        }

        public int GetQuestionNumber(int questionValue)
        {
            int valueResult = 0;
            
            valueResult += questionValue / 100;
            questionValue = questionValue % 100;

            valueResult += questionValue / 10;
            questionValue = questionValue % 10;

            valueResult += questionValue;

            return valueResult;
        }

        public void PickQuestion(int[] questionTypes, out int[] questionAssignment)
        {
            questionAssignment = new int[12];

            int magicIndex = Array.FindIndex(biogQuestions, x => x.QuestType == BiogQuestType.Magic);
            int combatIndex = Array.FindIndex(biogQuestions, x => x.QuestType == BiogQuestType.Combat);
            int thieveryIndex = Array.FindIndex(biogQuestions, x => x.QuestType == BiogQuestType.Thievery);
            int languagesIndex = Array.FindIndex(biogQuestions, x => x.QuestType == BiogQuestType.Languages);

            int questionNumber = GetQuestionNumber(questionTypes[(int)BiogQuestType.Magic]);
            for (int mi = 0; mi < questionNumber; mi++)
            {
                switch (mi)
                {
                    case 1:
                        if ((questionTypes[(int)BiogQuestType.Magic] >= 100 && questionTypes[(int)BiogQuestType.Magic] < 200) ||    // Only one Primary magic skill
                            (questionTypes[(int)BiogQuestType.Magic] >= 10 && questionTypes[(int)BiogQuestType.Magic] < 20) ||      // Only one Major magic skill, no Primary
                             questionTypes[(int)BiogQuestType.Magic] == 1)                                                          // Only one Minor magic skill, no Primary nor Major
                        {
                            questionAssignment[9] = magicIndex * 1000 + 0;
                        }
                        else{
                            switch (UnityEngine.Random.Range(0, 2))
                            {
                                case 0:
                                    questionAssignment[6] = magicIndex * 1000 + 3;
                                    break;
                                case 1:
                                    questionAssignment[0] = magicIndex * 1000 + 1;
                                    break;
                            }                            
                        } 
                        break;

                    case 3:
                        switch (UnityEngine.Random.Range(0, 2))
                        {
                            case 0:
                                questionAssignment[3] = magicIndex * 1000 + 2;
                                break;
                            case 1:
                                questionAssignment[7] = (magicIndex + 1) * 1000 + 0;
                                break;
                        }
                        break;
                    
                    case 5:
                        switch (UnityEngine.Random.Range(0, 2))
                        {
                            case 0:
                                questionAssignment[4] = magicIndex * 1000 + 4;
                                break;
                            case 1:
                                questionAssignment[8] = magicIndex * 1000 + 5;
                                break;
                        }
                        break;

                    default:
                        break;
                }
            }

            questionNumber = GetQuestionNumber(questionTypes[(int)BiogQuestType.Combat]);
            for (int ci = 0; ci < questionNumber; ci++)
            {
                switch (ci)
                {
                    case 1:
                        if ((questionTypes[(int)BiogQuestType.Combat] >= 100 && questionTypes[(int)BiogQuestType.Combat] < 200) ||    // Only one Primary combat skill
                            (questionTypes[(int)BiogQuestType.Combat] >= 10 && questionTypes[(int)BiogQuestType.Combat] < 20) ||      // Only one Major combat skill, no Primary
                             questionTypes[(int)BiogQuestType.Combat] == 1)                                                           // Only one Minor combat skill, no Primary nor Major
                        {
                            questionAssignment[GetNextFreeCounter(9, ref questionAssignment)] = (combatIndex + 2) * 1000 + 0;
                        }
                        else{
                            switch (UnityEngine.Random.Range(0, 2))
                            {
                                case 0:
                                    questionAssignment[GetNextFreeCounter(3, ref questionAssignment)] = combatIndex * 1000 + 4;
                                    break;
                                case 1:
                                    questionAssignment[GetNextFreeCounter(6, ref questionAssignment)] = combatIndex * 1000 + 2;
                                    break;
                            }
                        }
                        break;

                    case 3:
                        switch (UnityEngine.Random.Range(0, 2))
                        {
                            case 0:
                                questionAssignment[GetNextFreeCounter(9, ref questionAssignment)] = combatIndex * 1000 + 0;
                                break;
                            case 1:
                                questionAssignment[GetNextFreeCounter(0, ref questionAssignment)] = combatIndex * 1000 + 3;
                                break;
                        }
                        break;

                    case 5:
                        switch (UnityEngine.Random.Range(0, 2))
                        {
                            case 0:
                                questionAssignment[GetNextFreeCounter(0, ref questionAssignment)] = (combatIndex + 1) * 1000 + 0;
                                break;
                            case 1:
                                questionAssignment[GetNextFreeCounter(6, ref questionAssignment)] = combatIndex * 1000 + 1;
                                break;
                        }
                        break;

                    default:
                        break;
                }
            }

            questionNumber = GetQuestionNumber(questionTypes[(int)BiogQuestType.Thievery]);
            for (int ti = 0; ti < questionNumber; ti++)
            {
                switch (ti)
                {
                    case 1:
                        switch (UnityEngine.Random.Range(0, 2))
                        {
                            case 0:
                                questionAssignment[GetNextFreeCounter(9, ref questionAssignment)] = thieveryIndex * 1000 + 3;
                                break;
                            case 1:
                                questionAssignment[GetNextFreeCounter(3, ref questionAssignment)] = thieveryIndex * 1000 + 1;
                                break;
                        }
                        break;

                    case 3:
                        switch (UnityEngine.Random.Range(0, 2))
                        {
                            case 0:
                                questionAssignment[GetNextFreeCounter(3, ref questionAssignment)] = (thieveryIndex + 2) * 1000 + 0;
                                break;
                            case 1:
                                questionAssignment[GetNextFreeCounter(3, ref questionAssignment)] = (thieveryIndex + 3) * 1000 + 0;
                                break;
                        }
                        break;

                    case 5:
                        switch (UnityEngine.Random.Range(0, 2))
                        {
                            case 0:
                                questionAssignment[GetNextFreeCounter(6, ref questionAssignment)] = thieveryIndex * 1000 + 0;
                                break;
                            case 1:
                                questionAssignment[GetNextFreeCounter(9, ref questionAssignment)] = (thieveryIndex + 1) * 1000 + 0;
                                break;
                        }
                        break;

                    case 7:
                        switch (UnityEngine.Random.Range(0, 2))
                        {
                            case 0:
                                questionAssignment[GetNextFreeCounter(9, ref questionAssignment)] = (thieveryIndex + 4) * 1000 + 0;
                                break;
                            case 1:
                                questionAssignment[GetNextFreeCounter(6, ref questionAssignment)] = thieveryIndex * 1000 + 2;
                                break;
                        }
                        break;

                    default:
                        break;
                }
            }

            questionNumber = GetQuestionNumber(questionTypes[(int)BiogQuestType.Languages]);
            for (int li = 0; li < questionNumber; li++)
            {
                switch (li)
                {
                    case 3:
                        questionAssignment[GetNextFreeCounter(0, ref questionAssignment)] = languagesIndex * 1000 + 0;
                        break;

                    case 6:
                        questionAssignment[GetNextFreeCounter(3, ref questionAssignment)] = languagesIndex * 1000 + 1;
                        break;

                    default:
                        break;
                }
            }

            for (int i = 0; i < 12; i++)
            {
                if (questionAssignment[i] == 0)
                {
                    questionAssignment[i] = GetRandomGeneric(i, ref questionAssignment, ref magicIndex);
                }
            }
            
            for (int u = 0; u < 12; u++)
                Debug.Log("Question " + u + ": " + questionAssignment[u]);
        }

        public int GetRandomGeneric(int questionNumber, ref int[] questionAssignment, ref int magicIndex)
        {
            if (!questionAssignment.Contains(2000) &&
                !questionAssignment.Contains(7000) && 
                !questionAssignment.Contains((magicIndex + 1) * 1000 + 0))
            {
                if (questionNumber < 6)
                    return 2000;
                else return 7000;
            }

            int maxGenericIndex = Array.FindLastIndex(biogQuestions, x => x.QuestType == (int)BiogQuestType.Generic) + 1;
            int genChoices = 0;
            int[] genList;

            switch (questionNumber)
            {
                case 0:
                case 1:
                case 2:
                    genList = new int[childGeneric.Length];
                    Array.Copy(childGeneric, genList, childGeneric.Length);
                    break;

                case 3:
                case 4:
                case 5:
                    genList = new int[adolescentGeneric.Length];
                    Array.Copy(adolescentGeneric, genList, adolescentGeneric.Length);
                    break;

                case 6:
                case 7:
                case 8:
                    genList = new int[youngadultGeneric.Length];
                    Array.Copy(youngadultGeneric, genList, youngadultGeneric.Length);
                    break;

                case 9:
                case 10:
                case 11:
                    genList = new int[adultGeneric.Length];
                    Array.Copy(adultGeneric, genList, adultGeneric.Length);
                    break;

                default:
                    genList = new int[0];
                    break;
            }
            genChoices = genList.Length;

            int pickedQuestion = -1;
            int[] questReference = new int[6];
            do{
                pickedQuestion = genList[UnityEngine.Random.Range(0, genChoices)];
                for (int qRef = 0; qRef < questReference.Length; qRef++)
                {
                    questReference[qRef] = pickedQuestion / 1000 * 1000 + qRef;
                    Debug.Log("questReference[qRef]: " + questReference[qRef]);
                }
            }
            while (questionAssignment.Contains(questReference[0]) || questionAssignment.Contains(questReference[1]) ||questionAssignment.Contains(questReference[2]) ||questionAssignment.Contains(questReference[3]) ||questionAssignment.Contains(questReference[4]) ||questionAssignment.Contains(questReference[5]));

            return pickedQuestion;
        }

        public int GetNextFreeCounter(int startingCounter, ref int[] questionAssignment)
        {
            while (questionAssignment[startingCounter] != 0)
            {
                startingCounter++;
                if (startingCounter >= questionAssignment.Length)
                    startingCounter = 0;
            }

            return startingCounter;
        }

        public void DigestRepChanges()
        {
            foreach (string effect in answerEffects)
            {
                if (!effect.StartsWith(((int)BiogAnswerEffect.Reputation).ToString()))
                    continue;
                
                string[] repChange = effect.Split('*');

                if (repChange[1][1] == 'f')
                    continue;

                int amount, id;
                int.TryParse(repChange[1].Split('r')[1], out id);
                Debug.Log("Showing SGroup id: " + id);
                int.TryParse(repChange[2], out amount);
                Debug.Log("Showing rep Change amount: " + amount);

                // string[] tokens = effect.Split(' ');

                // if (effect[0] != 'r'
                //     || effect[1] == 'f'
                //     || tokens.Length < 2
                //     || !int.TryParse(tokens[0].Split('r')[1], out id)
                //     || !int.TryParse(tokens[1], out amount))
                // {
                //     continue;
                // }
                changedReputations[id] += (short)amount;
            }
        }

        public List<string> GenerateBackstory()
        {
            #region Parse answer tokens
            List<int>[] tokenLists = new List<int>[questionCount * 2];
            tokenLists[0] = Q1Tokens;
            tokenLists[1] = Q2Tokens;
            tokenLists[2] = Q3Tokens;
            tokenLists[3] = Q4Tokens;
            tokenLists[4] = Q5Tokens;
            tokenLists[5] = Q6Tokens;
            tokenLists[6] = Q7Tokens;
            tokenLists[7] = Q8Tokens;
            tokenLists[8] = Q9Tokens;
            tokenLists[9] = Q10Tokens;
            tokenLists[10] = Q11Tokens;
            tokenLists[11] = Q12Tokens;

            // Setup tokens for macro handler
            foreach (string effect in answerEffects)
            {
                char prefix = effect[0];

                if (prefix == '#' || prefix == '!' || prefix == '?')
                {
                    int questionInd;
                    string[] effectSplit = effect.Split(' ');
                    string command = effectSplit[0];
                    string index = effectSplit[1];
                    if (!int.TryParse(index, out questionInd))
                    {
                        Debug.LogError("GenerateBackstory: Invalid question index.");
                        continue;
                    }

                    string[] splitStr = command.Split(prefix);
                    if (splitStr.Length > 1)
                    {
                        tokenLists[questionInd].Add(int.Parse(splitStr[1]));
                    }
                }
            }
            #endregion

            // TextFile.Token lastToken = new TextFile.Token();
            // GameManager.Instance.PlayerEntity.BirthRaceTemplate = characterDocument.raceTemplate; // Need correct race set when parsing %ra macro
            List<string> backStory = new List<string>();
            // TextFile.Token[] newBiogs, originalBiogs, biogParts;
            // // BiogText biogText = new BiogText(answers, out newBiogs, out originalBiogs, out biogParts);
            // TextFile.Token[] tokens = DaggerfallUnity.Instance.TextProvider.GetRSCTokens(backstoryId);

            // MacroHelper.ExpandMacros(ref tokens, (IMacroContextProvider)this);

            // foreach (TextFile.Token token in tokens)
            // {
            //     if (token.formatting == TextFile.Formatting.Text)
            //     {
            //         backStory.Add(token.text);
            //     }
            //     else if (token.formatting == TextFile.Formatting.JustifyLeft)
            //     {
            //         if (lastToken.formatting == TextFile.Formatting.JustifyLeft)
            //             backStory.Add("\n");
            //     }
            //     lastToken = token;
            // }

            for (int i = 0; i < questions.Length; i++)
            {
                backStory.Add(biogText.QuestionBiogs[0].Standard[0]);
            }

            return backStory;
        }

        public void AddEffect(string effect, int index)
        {
            Debug.Log("Reached AddEffect, effect: " + effect + ", index: " + index);
            if (effect[0] == '#' || effect[0] == '!' || effect[0] == '?')
            {
                AnswerEffects.Add(effect + " " + index); // Tag text macros with question numbers
            }
            else
            {
                AnswerEffects.Add(effect);
            }
        }

        #region Static Methods

        private static void ApplyPlayerEffect(PlayerEntity playerEntity, string effect)
        {
            string[] tokens = effect.Split('*');
            int parseResult;

            // Modify gold amount
            if (effect.StartsWith(((int)BiogAnswerEffect.GoldPieces).ToString()))
            {

                // Correct GP commands with spaces between the sign and the amount
                // if (tokens.Length > 2)
                // {
                //     tokens[1] = tokens[1] + tokens[2];
                // }
                if (!int.TryParse(tokens[1], out parseResult))
                {
                    Debug.LogError("CreateCharBiography: GP - invalid argument.");
                    return;
                }
                if (tokens[1][0] == '+' || tokens[1][0] != '-')
                {
                    playerEntity.GoldPieces += parseResult;
                }
                else if (tokens[1][0] == '-')
                {
                    playerEntity.GoldPieces -= parseResult;
                    // The player can't carry negative gold pieces
                    playerEntity.GoldPieces = playerEntity.GoldPieces < 0 ? 0 : playerEntity.GoldPieces;
                }
            }
            // Adjust reputation
            else if (effect.StartsWith(((int)BiogAnswerEffect.Reputation).ToString()))
            {
                int id;
                int amount;
                // Faction
                if (effect[3] == 'f')
                {
                    if (!int.TryParse(tokens[1].Split('f')[1], out id) || !int.TryParse(tokens[2], out amount))
                    {
                        Debug.LogError("CreateCharBiography: rf - invalid argument.");
                        return;
                    }
                    playerEntity.FactionData.ChangeReputation(id, amount, true);
                }
                // Social group (Merchants, Commoners, etc.)
                else
                {
                    if (!int.TryParse(tokens[1].Split('r')[1], out id) || !int.TryParse(tokens[2], out amount))
                    {
                        Debug.LogError("CreateCharBiography: r - invalid argument.");
                        return;
                    }
                    playerEntity.SGroupReputations[id] += (short)amount;
                }
            }
            // Add item
            else if (effect.StartsWith(((int)BiogAnswerEffect.Item).ToString()))
            {
                int itemGroup;
                int groupIndex;
                int material;
                if (!int.TryParse(tokens[1], out itemGroup)
                    || !int.TryParse(tokens[2], out groupIndex)
                    || !int.TryParse(tokens[3], out material))
                {
                    Debug.LogError("CreateCharBiography: IT - invalid argument(s).");
                    return;
                }

                DaggerfallUnityItem newItem = null;
                if ((ItemGroups)itemGroup == ItemGroups.Weapons)
                {
                    newItem = ItemBuilder.CreateWeapon((Weapons)Enum.GetValues(typeof(Weapons)).GetValue(groupIndex), (MaterialTypes)material);
                }
                else if ((ItemGroups)itemGroup == ItemGroups.Armor)
                {
                    // Biography commands treat weapon and armor material types the same
                    newItem = ItemBuilder.CreateArmor(playerEntity.Gender, playerEntity.Race, (Armor)Enum.GetValues(typeof(Armor)).GetValue(groupIndex), (ArmorMaterialTypes)material);
                }
                else if ((ItemGroups)itemGroup == ItemGroups.Books)
                {
                    newItem = ItemBuilder.CreateRandomBook();
                }
                else
                {
                    newItem = new DaggerfallUnityItem((ItemGroups)itemGroup, groupIndex);
                }
                playerEntity.Items.AddItem(newItem);
            }
            // Skill modifier effect
            else if (effect.StartsWith(((int)BiogAnswerEffect.Skill).ToString()))
            {
                if (!int.TryParse(tokens[1], out parseResult))
                {
                    Debug.LogError("CreateCharBiography: SKILL - invalid argument.");
                    return;
                }
                short modValue;
                DFCareer.Skills skill = (DFCareer.Skills)parseResult;
                if (short.TryParse(tokens[2], out modValue))
                {
                    short startValue = playerEntity.Skills.GetPermanentSkillValue(skill);
                    playerEntity.Skills.SetPermanentSkillValue(skill, (short)(startValue + modValue));
                }
                else
                {
                    Debug.LogError("CreateCharBiography: Invalid skill adjustment value.");
                }
            }
            // Adjust poison resistance
            else if (effect.StartsWith(((int)BiogAnswerEffect.Modifier).ToString()))
            {
                if (tokens[1].StartsWith("RP"))
                {
                    if (int.TryParse(tokens[2], out parseResult))
                    {
                        playerEntity.BiographyResistPoisonMod = parseResult;
                    }
                    else
                    {
                        Debug.LogError("CreateCharBiography: RP - invalid argument.");
                    }
                }
                // Adjust fatigue
                else if (tokens[1].StartsWith("FT"))
                {
                    if (int.TryParse(tokens[2], out parseResult))
                    {
                        playerEntity.BiographyFatigueMod = parseResult;
                    }
                    else
                    {
                        Debug.LogError("CreateCharBiography: FT - invalid argument.");
                    }
                }
                // Adjust reaction roll
                else if (tokens[1].StartsWith("RR"))
                {
                    if (int.TryParse(tokens[2], out parseResult))
                    {
                        playerEntity.BiographyReactionMod = parseResult;
                    }
                    else
                    {
                        Debug.LogError("CreateCharBiography: RR - invalid argument.");
                    }
                }
                // Adjust disease resistance
                else if (tokens[1].StartsWith("RD"))
                {
                    if (int.TryParse(tokens[2], out parseResult))
                    {
                        playerEntity.BiographyResistDiseaseMod = parseResult;
                    }
                    else
                    {
                        Debug.LogError("CreateCharBiography: RD - invalid argument.");
                    }
                }
                // Adjust magic resistance
                else if (tokens[1].StartsWith("MR"))
                {
                    if (int.TryParse(tokens[2], out parseResult))
                    {
                        playerEntity.BiographyResistMagicMod = parseResult;
                    }
                    else
                    {
                        Debug.LogError("CreateCharBiography: MR - invalid argument.");
                    }
                }
                // Adjust to-hit
                else if (tokens[1].StartsWith("TH"))
                {
                    if (int.TryParse(tokens[2], out parseResult))
                    {
                        playerEntity.BiographyAvoidHitMod = parseResult;
                    }
                    else
                    {
                        Debug.LogError("CreateCharBiography: TH - invalid argument.");
                    }
                }
            }
            else if (effect.StartsWith(((int)BiogAnswerEffect.FriendsAndFoes).ToString()))
            {
                if (tokens[1].StartsWith("AE"))
                {
                    Debug.Log("CreateCharBiography: AE - command unimplemented.");
                }
                else if (tokens[1].StartsWith("AF"))
                {
                    Debug.Log("CreateCharBiography: AF - command unimplemented.");
                }
                else if (tokens[1].StartsWith("AO"))
                {
                    Debug.Log("CreateCharBiography: AO - command unimplemented.");
                }
            }
            // else if (effect[0] == '#' || effect[0] == '!' || effect[0] == '?')
            // {
            //     Debug.Log("CreateCharBiography: Detected biography text command.");
            // }
            else
            {
                Debug.LogError("CreateCharBiography: Invalid command - " + effect);
            }
        }

        public static void ApplyEffects(IEnumerable<string> effects, PlayerEntity playerEntity)
        {
            if (effects == null)
                return;

            foreach (string effect in effects)
            {
                ApplyPlayerEffect(playerEntity, effect);
            }
        }

        public static int[] GetSkillEffects(IEnumerable<string> effects)
        {
            if (effects == null)
                return null;

            int skillCount = Enum.GetNames(typeof(DFCareer.Skills)).Length;
            int[] skills = new int[skillCount];

            // Apply only skill effects
            foreach(string effect in effects)
            {
                string[] tokens = effect.Split(null);
                int parseResult;

                // Skill modifier effect
                if (int.TryParse(tokens[0], out parseResult) && parseResult >= 0 && parseResult < skillCount)
                {
                    short modValue;
                    if (short.TryParse(tokens[1], out modValue))
                    {
                        skills[parseResult] += modValue;
                    }
                    else
                    {
                        Debug.LogError("CreateCharBiography: Invalid skill adjustment value.");
                    }
                }
            }

            return skills;
        }

        // private static ArmorMaterialTypes WeaponToArmorMaterialType(MaterialTypes materialType)
        // {
        //     switch (materialType)
        //     {
        //         case MaterialTypes.Iron:
        //             return ArmorMaterialTypes.Iron;
        //         case MaterialTypes.Steel:
        //             return ArmorMaterialTypes.Steel;
        //         case MaterialTypes.Silver:
        //             return ArmorMaterialTypes.Silver;
        //         case MaterialTypes.Elven:
        //             return ArmorMaterialTypes.Elven;
        //         case MaterialTypes.Dwarven:
        //             return ArmorMaterialTypes.Dwarven;
        //         case MaterialTypes.Mithril:
        //             return ArmorMaterialTypes.Mithril;
        //         case MaterialTypes.Adamantium:
        //             return ArmorMaterialTypes.Adamantium;
        //         case MaterialTypes.Ebony:
        //             return ArmorMaterialTypes.Ebony;
        //         case MaterialTypes.Orcish:
        //             return ArmorMaterialTypes.Orcish;
        //         case MaterialTypes.Daedric:
        //             return ArmorMaterialTypes.Daedric;
        //         default:
        //             return ArmorMaterialTypes.None;
        //     }
        // }

        public static int GetClassAffinityIndex(DFCareer custom, List<DFCareer> classes)
        {
            int highestAffinity = 0;
            int selectedIndex = 0;
            for (int i = 0; i < classes.Count; i++)
            {
                int affinity = 0;
                List<DFCareer.Skills> classSkills = new List<DFCareer.Skills>();
                classSkills.Add(classes[i].PrimarySkill1);
                classSkills.Add(classes[i].PrimarySkill2);
                classSkills.Add(classes[i].PrimarySkill3);
                classSkills.Add(classes[i].MajorSkill1);
                classSkills.Add(classes[i].MajorSkill2);
                classSkills.Add(classes[i].MajorSkill3);
                classSkills.Add(classes[i].MinorSkill1);
                classSkills.Add(classes[i].MinorSkill2);
                classSkills.Add(classes[i].MinorSkill3);
                classSkills.Add(classes[i].MinorSkill4);
                classSkills.Add(classes[i].MinorSkill5);
                classSkills.Add(classes[i].MinorSkill6);
                if (classSkills.Contains(custom.PrimarySkill1)) affinity++;
                if (classSkills.Contains(custom.PrimarySkill2)) affinity++;
                if (classSkills.Contains(custom.PrimarySkill3)) affinity++;
                if (classSkills.Contains(custom.MajorSkill1)) affinity++;
                if (classSkills.Contains(custom.MajorSkill2)) affinity++;
                if (classSkills.Contains(custom.MajorSkill3)) affinity++;
                if (classSkills.Contains(custom.MinorSkill1)) affinity++;
                if (classSkills.Contains(custom.MinorSkill2)) affinity++;
                if (classSkills.Contains(custom.MinorSkill3)) affinity++;
                if (classSkills.Contains(custom.MinorSkill4)) affinity++;
                if (classSkills.Contains(custom.MinorSkill5)) affinity++;
                if (classSkills.Contains(custom.MinorSkill6)) affinity++;
                if (affinity > highestAffinity)
                {
                    highestAffinity = affinity;
                    selectedIndex = i;
                }
            }

            return selectedIndex;
        }

        #endregion

        #region Properties

        public Question[] Questions
        {
            get { return questions; }
        }

        public class Question
        {
            public const int lines = 2;
            //const int maxAnswers = 10;

            public string[] text = new string[lines];
            public List<Answer> answers = new List<Answer>();

            public Question()
            {
                for (int i = 0; i < lines; i++)
                {
                    text[i] = string.Empty;
                }
            }

            public string[] Text
            {
                get { return text; }
            }

            public List<Answer> Answers
            {
                get { return answers; }
            }
        }

        public class Answer
        {
            public string text = string.Empty;
            public List<string> effects = new List<string>();

            public string Text
            {
                get { return text; }
                set { text = value; }
            }

            public List<String> Effects
            {
                get { return effects; }
            }
        }

        public List<string> AnswerEffects
        {
            get { return answerEffects; }
        }

        public List<int> Q1Tokens { get; set; }
        public List<int> Q2Tokens { get; set; }
        public List<int> Q3Tokens { get; set; }
        public List<int> Q4Tokens { get; set; }
        public List<int> Q5Tokens { get; set; }
        public List<int> Q6Tokens { get; set; }
        public List<int> Q7Tokens { get; set; }
        public List<int> Q8Tokens { get; set; }
        public List<int> Q9Tokens { get; set; }
        public List<int> Q10Tokens { get; set; }
        public List<int> Q11Tokens { get; set; }
        public List<int> Q12Tokens { get; set; }

        #endregion
    }
}
