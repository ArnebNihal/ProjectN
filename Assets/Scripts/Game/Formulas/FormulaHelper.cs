// Project:         Daggerfall Unity
// Copyright:       Copyright (C) 2009-2022 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Gavin Clayton (interkarma@dfworkshop.net)
// Contributors:    Allofich, Hazelnut, ifkopifko, Numidium, TheLacus
// 
// Notes:
//

using UnityEngine;
using System;
using DaggerfallConnect;
using DaggerfallWorkshop.Game.Guilds;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using System.Collections.Generic;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Utility;
using DaggerfallConnect.Save;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Banking;
using System.Linq;

namespace DaggerfallWorkshop.Game.Formulas
{
    /// <summary>
    /// Common formulas used throughout game.
    /// Where the exact formula is unknown, a "best effort" approximation will be used.
    /// Most formula can be overridden by registering a new method matching the appropriate delegate signature.
    /// Other signatures can be added upon demand.
    /// </summary>
    public static class FormulaHelper
    {
        private struct FormulaOverride
        {
            internal readonly Delegate Formula;
            internal readonly Mod Provider;

            internal FormulaOverride(Delegate formula, Mod provider)
            {
                Formula = formula;
                Provider = provider;
            }
        }

        readonly static Dictionary<string, FormulaOverride> overrides = new Dictionary<string, FormulaOverride>();

        public static float specialInfectionChance = 0.6f;

        // Approximation of classic frame updates
        public const int classicFrameUpdate = 980;
        public const int weaponArmorMultiplier = 50;

        // ProjectN: values for unleveled material generation
        public const int rareMaterial = 100;    // Is 100 for experimenting with metals. It was 1000 before.
        public const int ebonyChance = 10;
        public const int adamantiumChance = ebonyChance + 20;
        public const int mithrilOrcishRelativeChance = 2;
        public const int dwarvenChance = 3;
        public const int glassChance = dwarvenChance + 3;
        public const int elvenChance =  glassChance + 3;
        public const int silverChance = elvenChance + 3;
        public const int steelChance = (rareMaterial - silverChance) / 10 * 4;

        /// <summary>Struct for return values of formula that affect damage and to-hit chance.</summary>
        public struct ToHitAndDamageMods
        {
            public int damageMod;
            public int toHitMod;
        }

        #region Basic Formulas

        public static int DamageModifier(int strength)
        {
            Func<int, int> del;
            if (TryGetOverride("DamageModifier", out del))
                return del(strength);

            return (int)Mathf.Floor((float)(strength - 50) / 5f);
        }

        public static int MaxEncumbrance(int strength)
        {
            Func<int, int> del;
            if (TryGetOverride("MaxEncumbrance", out del))
                return del(strength);

            return (int)Mathf.Floor((float)strength * 1.5f);
        }

        public static int SpellPoints(int intelligence, float multiplier)
        {
            Func<int, float, int> del;
            if (TryGetOverride("SpellPoints", out del))
                return del(intelligence, multiplier);

            return (int)Mathf.Floor((float)intelligence * multiplier);
        }

        public static int MagicResist(int willpower)
        {
            Func<int, int> del;
            if (TryGetOverride("MagicResist", out del))
                return del(willpower);

            return (int)Mathf.Floor((float)willpower / 10f);
        }

        public static int ToHitModifier(int agility)
        {
            Func<int, int> del;
            if (TryGetOverride("ToHitModifier", out del))
                return del(agility);

            // ProjectN: doubled AGI toHit bonus and malus, DFU was AGI / 10 - 5;
            return (int)Mathf.Floor((float)agility / 5f) - 5;
        }

        public static int HitPointsModifier(int endurance)
        {
            Func<int, int> del;
            if (TryGetOverride("HitPointsModifier", out del))
                return del(endurance);

            return (int)Mathf.Floor((float)endurance / 10f) - 5;
        }

        public static int HealingRateModifier(int endurance)
        {
            Func<int, int> del;
            if (TryGetOverride("HealingRateModifier", out del))
                return del(endurance);

            // Original Daggerfall seems to have a bug where negative endurance modifiers on healing rate
            // are applied as modifier + 1. Not recreating that here.
            return (int)Mathf.Floor((float)endurance / 10f) - 5;
        }

        public static int MaxStatValue()
        {
            Func<int> del;
            if (TryGetOverride("MaxStatValue", out del))
                return del();
            else
                return 100;
        }

        public static int BonusPool()
        {
            Func<int> del;
            if (TryGetOverride("BonusPool", out del))
                return del();

            const int minBonusPool = 4;        // The minimum number of free points to allocate on level up
            const int maxBonusPool = 6;        // The maximum number of free points to allocate on level up

            // Roll bonus pool for player to distribute
            // Using maxBonusPool + 1 for inclusive range
            UnityEngine.Random.InitState(Time.frameCount);
            return UnityEngine.Random.Range(minBonusPool, maxBonusPool + 1);
        }

        #endregion

        #region Player

        // Generates player health based on level and career hit points per level
        public static int RollMaxHealth(PlayerEntity player)
        {
            Func<PlayerEntity, int> del;
            if (TryGetOverride("RollMaxHealth", out del))
                return del(player);

            const int baseHealth = 25;
            int maxHealth = baseHealth + player.Career.HitPointsPerLevel;

            for (int i = 1; i < player.Level; i++)
            {
                maxHealth += CalculateHitPointsPerLevelUp(player);
            }

            return maxHealth;
        }

        // Calculate how much health the player should recover per hour of rest
        public static int CalculateHealthRecoveryRate(PlayerEntity player)
        {
            Func<PlayerEntity, int> del;
            if (TryGetOverride("CalculateHealthRecoveryRate", out del))
                return del(player);

            short medical = player.Skills.GetLiveSkillValue(DFCareer.Skills.Medical);
            int endurance = player.Stats.LiveEndurance;
            int maxHealth = player.MaxHealth;
            PlayerEnterExit playerEnterExit;
            playerEnterExit = GameManager.Instance.PlayerGPS.GetComponent<PlayerEnterExit>();
            DFCareer.RapidHealingFlags rapidHealingFlags = player.Career.RapidHealing;

            short addToMedical = 60;

            if (rapidHealingFlags == DFCareer.RapidHealingFlags.Always)
                addToMedical = 100;
            else if (DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.IsDay && !playerEnterExit.IsPlayerInside)
            {
                if (rapidHealingFlags == DFCareer.RapidHealingFlags.InLight)
                    addToMedical = 100;
            }
            else if (rapidHealingFlags == DFCareer.RapidHealingFlags.InDarkness)
                addToMedical = 100;

            medical += addToMedical;

            return Mathf.Max((int)Mathf.Floor(HealingRateModifier(endurance) + medical * maxHealth / 1000), 1);
        }

        // Calculate how much fatigue the player should recover per hour of rest
        public static int CalculateFatigueRecoveryRate(int maxFatigue)
        {
            Func<int, int> del;
            if (TryGetOverride("CalculateFatigueRecoveryRate", out del))
                return del(maxFatigue);

            return Mathf.Max((int)Mathf.Floor(maxFatigue / 8), 1);
        }

        // Calculate how many spell points the player should recover per hour of rest
        public static int CalculateSpellPointRecoveryRate(PlayerEntity player)
        {
            Func<PlayerEntity, int> del;
            if (TryGetOverride("CalculateSpellPointRecoveryRate", out del))
                return del(player);

            if (player.Career.NoRegenSpellPoints)
                return 0;

            return Mathf.Max((int)Mathf.Floor(player.MaxMagicka / 8), 1);
        }

        // Calculate chance of successfully lockpicking a door in an interior (an animating door). If this is higher than a random number between 0 and 100 (inclusive), the lockpicking succeeds.
        // ProjectN: removing player level from the equation. It will be substituted by lockpicking tools.
        public static int CalculateInteriorLockpickingChance(int lockvalue, int lockpickingSkill)
        {
            int chance = (lockpickingSkill - 5 * lockvalue);
            return Mathf.Clamp(chance, 5, 95);
        }

        // Calculate chance of successfully lockpicking a door in an exterior (a door that leads to an interior). If this is higher than a random number between 0 and 100 (inclusive), the lockpicking succeeds.
        public static int CalculateExteriorLockpickingChance(int lockvalue, int lockpickingSkill)
        {
            int chance = lockpickingSkill - (5 * lockvalue);
            return Mathf.Clamp(chance, 5, 95);
        }

        // Calculate chance of successfully pickpocketing a target
        // ProjectN: Added LUC and AGI to the calculation, removed player and target level
        public static int CalculatePickpocketingChance(PlayerEntity player, EnemyEntity target)
        {
            int chance = player.Skills.GetLiveSkillValue(DFCareer.Skills.Pickpocket) + ((player.Stats.LiveLuck / 10 - 5) - (target.Stats.LiveLuck / 10 - 5));
            // If target is an enemy mobile, apply level modifier.
            if (target != null)
            {
                chance += ((player.Stats.LiveAgility - 50) / 5) - ((target.Stats.LiveAgility - 50) / 5);
            }
            return Mathf.Clamp(chance, 5, 95);
        }

        // Calculate chance of being caught shoplifting items
        // ProjectN: Added LUC to the calculation
        public static int CalculateShopliftingChance(PlayerEntity player, int shopQuality, int weightAndNumItems)
        {
            Func<PlayerEntity, int, int, int> del;
            if (TryGetOverride("CalculateShopliftingChance", out del))
                return del(player, shopQuality, weightAndNumItems);

            int chance = 100 - (player.Skills.GetLiveSkillValue(DFCareer.Skills.Pickpocket) + (player.Stats.LiveLuck / 10 - 5));
            chance += shopQuality + weightAndNumItems;
            return Mathf.Clamp(chance, 5, 95);
        }
        
        // Calculate chance of stealth skill hiding the user.
        // ProjectN: Added LUC to the calculation
        public static int CalculateStealthChance(float distanceToTarget, DaggerfallEntityBehaviour target)
        {
            Func<float, DaggerfallEntityBehaviour, int> del;
            if (TryGetOverride("CalculateStealthChance", out del))
                return del(distanceToTarget, target);

            int chance = 2 * ((int)(distanceToTarget / MeshReader.GlobalScale) * (target.Entity.Skills.GetLiveSkillValue(DFCareer.Skills.Stealth) >> 10) + (target.Entity.Stats.LiveLuck / 10 - 5));
            return chance;
        }

        // Calculate chance of successfully climbing - checked repeatedly while climbing
        public static int CalculateClimbingChance(PlayerEntity player, int basePercentSuccess)
        {
            // Fail to climb if weapon not sheathed.
            if (!GameManager.Instance.WeaponManager.Sheathed && GameManager.Instance.WeaponManager.ScreenWeapon.WeaponType != WeaponTypes.Melee)
            {
                DaggerfallUI.SetMidScreenText(TextManager.Instance.GetLocalizedText("noClimbHoldingWeapon"), 1f);
                return 0;
            }

            int skill = player.Skills.GetLiveSkillValue(DFCareer.Skills.Climbing);
            int luck = player.Stats.GetLiveStatValue(DFCareer.Stats.Luck);
            if (player.Race == Races.Khajiit)
                skill += 30;

            // Climbing effect states "target can climb twice as well" - doubling effective skill after racial applied
            if (player.IsEnhancedClimbing)
                skill *= 2;

            // Clamp skill range
            skill = Mathf.Clamp(skill, 5, 95);
            float luckFactor = Mathf.Lerp(0, 10, luck * 0.01f);

            // Skill Check
            int chance = (int) (Mathf.Lerp(basePercentSuccess, 100, skill * .01f) + luckFactor);

            return chance;
        }

        // Calculate how many uses a skill needs before its value will rise.
        public static int CalculateSkillUsesForAdvancement(int skillValue, int skillAdvancementMultiplier, float careerAdvancementMultiplier, int level, float governingAttributeMultiplier)
        {
            Func<int, int, float, int, int> del;
            if (TryGetOverride("CalculateSkillUsesForAdvancement", out del))
                return del(skillValue, skillAdvancementMultiplier, careerAdvancementMultiplier, level);

            double levelMod = Math.Pow(1.04, level);
            return (int)Math.Floor((skillValue * skillAdvancementMultiplier * careerAdvancementMultiplier * governingAttributeMultiplier * levelMod * 2 / 5) + 1);
        }

        // Calculate the multiplier that influences skill uses for advancement
        // that's based on the comparison between the skill level and its
        // governing attribute.
        public static float CalculateGoverningAttributeMultiplier(short skillValue, int statValue)
        {
            return (float)(1.0f - ((statValue - skillValue) * 0.005));
        }

        // Calculate player level.
        public static int CalculatePlayerLevel(int startingLevelUpSkillsSum, int currentLevelUpSkillsSum)
        {
            Func<int, int, int> del;
            if (TryGetOverride("CalculatePlayerLevel", out del))
                return del(startingLevelUpSkillsSum, currentLevelUpSkillsSum);

            return (int)Mathf.Floor((currentLevelUpSkillsSum - startingLevelUpSkillsSum + 28) / 15);
        }

        // Calculate hit points player gains per level.
        public static int CalculateHitPointsPerLevelUp(PlayerEntity player)
        {
            Func<PlayerEntity, int> del;
            if (TryGetOverride("CalculateHitPointsPerLevelUp", out del))
                return del(player);

            int minRoll = player.Career.HitPointsPerLevel / 2;
            int maxRoll = player.Career.HitPointsPerLevel;
            DFRandom.Seed = (uint)Time.renderedFrameCount;
            int addHitPoints = DFRandom.random_range_inclusive(minRoll, maxRoll);
            addHitPoints += HitPointsModifier(player.Stats.PermanentEndurance);
            if (addHitPoints < 1)
                addHitPoints = 1;
            return addHitPoints;
        }

        // Calculate whether the player is successful at pacifying an enemy.
        public static bool CalculateEnemyPacification(PlayerEntity player, DFCareer.Skills languageSkill)
        {
            Func<PlayerEntity, DFCareer.Skills, bool> del;
            if (TryGetOverride("CalculateEnemyPacification", out del))
                return del(player, languageSkill);

            double chance = 0;
            if (languageSkill == DFCareer.Skills.Etiquette ||
                languageSkill == DFCareer.Skills.Streetwise)
            {
                chance += player.Skills.GetLiveSkillValue(languageSkill) / 10;
                chance += player.Stats.LivePersonality / 5;
            }
            else
            {
                chance += player.Skills.GetLiveSkillValue(languageSkill);
                chance += player.Stats.LivePersonality / 10;
            }
            chance += GameManager.Instance.WeaponManager.Sheathed ? 10 : -25;

            // Add chance from Comprehend Languages effect if present
            ComprehendLanguages languagesEffect = (ComprehendLanguages)GameManager.Instance.PlayerEffectManager.FindIncumbentEffect<ComprehendLanguages>();
            if (languagesEffect != null)
                chance += languagesEffect.ChanceValue();

            int roll = UnityEngine.Random.Range(0, 200);
            bool success = (roll < chance);
            //if (success)
            //    player.TallySkill(languageSkill, 1);
            //else if (languageSkill != DFCareer.Skills.Etiquette && languageSkill != DFCareer.Skills.Streetwise)
            //    player.TallySkill(languageSkill, 1);

            Debug.LogFormat("Pacification {3} using {0} skill: chance= {1}  roll= {2}", languageSkill, chance, roll, success ? "success" : "failure");
            return success;
        }

        // Calculate whether the player is blessed when donating to a Temple.
        public static int CalculateTempleBlessing(int donationAmount, int deityRep)
        {
            return 1;   // TODO Amount of stat boost, guessing what this formula might need...
        }

        // Gets vampire clan based on region
        public static VampireClans GetVampireClan(int regionIndex)
        {
            FactionFile.FactionData factionData;
            GameManager.Instance.PlayerEntity.FactionData.GetRegionFaction(regionIndex, out factionData);
            switch ((FactionFile.FactionIDs) factionData.vam)
            {
                case FactionFile.FactionIDs.The_Vraseth:
                    return VampireClans.Vraseth;
                case FactionFile.FactionIDs.The_Haarvenu:
                    return VampireClans.Haarvenu;
                case FactionFile.FactionIDs.The_Thrafey:
                    return VampireClans.Thrafey;
                case FactionFile.FactionIDs.The_Lyrezi:
                    return VampireClans.Lyrezi;
                case FactionFile.FactionIDs.The_Montalion:
                    return VampireClans.Montalion;
                case FactionFile.FactionIDs.The_Khulari:
                    return VampireClans.Khulari;
                case FactionFile.FactionIDs.The_Garlythi:
                    return VampireClans.Garlythi;
                case FactionFile.FactionIDs.The_Anthotis:
                    return VampireClans.Anthotis;
                case FactionFile.FactionIDs.The_Selenu:
                    return VampireClans.Selenu;
            }

            // The Lyrezi are the default like in classic
            return VampireClans.Lyrezi;
        }

        #endregion

        #region Combat & Damage

        public static int CalculateHandToHandMinDamage(int handToHandSkill)
        {
            Func<int, int> del;
            if (TryGetOverride("CalculateHandToHandMinDamage", out del))
                return del(handToHandSkill);

            return (handToHandSkill / 10) + 1;
        }

        public static int CalculateHandToHandMaxDamage(int handToHandSkill)
        {
            Func<int, int> del;
            if (TryGetOverride("CalculateHandToHandMaxDamage", out del))
                return del(handToHandSkill);

            // Daggerfall Chronicles table lists hand-to-hand skills of 80 and above (45 through 79 are omitted)
            // as if they give max damage of (handToHandSkill / 5) + 2, but the hand-to-hand damage display in the character sheet
            // in classic Daggerfall shows it as continuing to be (handToHandSkill / 5) + 1
            return (handToHandSkill / 5) + 1;
        }

        public static int CalculateWeaponMinDamage(Weapons weapon)
        {
            Func<Weapons, int> del;
            if (TryGetOverride("CalculateWeaponMinDamage", out del))
                return del(weapon);

            switch (weapon)
            {
                case Weapons.Dagger:
                case Weapons.Tanto:
                case Weapons.Wakazashi:
                case Weapons.Saber:
                case Weapons.Katana:
                case Weapons.Dai_Katana:
                    return 1;
                case Weapons.Shortsword:
                case Weapons.Broadsword:
                case Weapons.Longsword:
                case Weapons.Claymore:
                case Weapons.Staff:
                case Weapons.ArchersAxe:
                    return 2;
                case Weapons.Battle_Axe:
                case Weapons.War_Axe:
                    return 3;
                case Weapons.LightFlail:
                case Weapons.Flail:
                case Weapons.Short_Bow:
                case Weapons.Long_Bow:
                    return 4;
                case Weapons.Mace:
                case Weapons.Warhammer:
                    return 5;
                default:
                    return 0;
            }
        }

        public static int CalculateWeaponMaxDamage(Weapons weapon)
        {
            Func<Weapons, int> del;
            if (TryGetOverride("CalculateWeaponMaxDamage", out del))
                return del(weapon);

            switch (weapon)
            {
                case Weapons.Dagger:
                    return 6;
                case Weapons.Tanto:
                    return 7;
                case Weapons.Shortsword:
                case Weapons.Staff:
                    return 8;
                case Weapons.Wakazashi:
                case Weapons.ArchersAxe:
                case Weapons.LightFlail:
                    return 10;
                case Weapons.Saber:
                case Weapons.Battle_Axe:
                case Weapons.Mace:
                    return 12;
                case Weapons.Flail:
                    return 14;
                case Weapons.Broadsword:
                    return 15;
                case Weapons.Longsword:
                case Weapons.Katana:
                case Weapons.War_Axe:
                case Weapons.Short_Bow:
                    return 16;
                case Weapons.Warhammer:
                case Weapons.Long_Bow:
                    return 18;
                case Weapons.Claymore:
                case Weapons.Dai_Katana:
                    return 19;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Calculate the damage caused by an attack.
        /// </summary>
        /// <param name="attacker">Attacking entity</param>
        /// <param name="target">Target entity</param>
        /// <param name="isEnemyFacingAwayFromPlayer">Whether enemy is facing away from player, used for backstabbing</param>
        /// <param name="weaponAnimTime">Time the weapon animation lasted before the attack in ms, used for bow drawing </param>
        /// <param name="weapon">The weapon item being used</param>
        /// <returns>Damage inflicted to target, can be 0 for a miss or ineffective hit</returns>
        public static int CalculateAttackDamage(DaggerfallEntity attacker, DaggerfallEntity target, bool isEnemyFacingAwayFromPlayer, int weaponAnimTime, DaggerfallUnityItem weapon, DaggerfallUnityItem arrow = null)
        {
            if (attacker == null || target == null)
                return 0;

            Func<DaggerfallEntity, DaggerfallEntity, bool, int, DaggerfallUnityItem, int> del;
            if (TryGetOverride("CalculateAttackDamage", out del))
                return del(attacker, target, isEnemyFacingAwayFromPlayer, weaponAnimTime, weapon);

            int metalMultiplier = 10;
            int damageModifiers = 0;
            int damage = 0;
            int shieldDamage = 0;
            bool blockSuccessful = false;
            int chanceToHitMod = 0;
            int backstabChance = 0;
            int criticalStrikeChance = 0;
            bool backstabSuccessful = false;
            bool criticalStrikeSuccessful = false;
            PlayerEntity player = GameManager.Instance.PlayerEntity;
            short skillID = 0;

            // Choose whether weapon-wielding enemies use their weapons or weaponless attacks.
            // In classic, weapon-wielding enemies use the damage values of their weapons
            // instead of their weaponless values.
            // For some enemies this gives lower damage than similar-tier monsters
            // and the weaponless values seems more appropriate, so here
            // enemies will choose to use their weaponless attack if it is more damaging.
            EnemyEntity AIAttacker = attacker as EnemyEntity;
            if (AIAttacker != null && weapon != null)
            {
                int weaponAverage = (weapon.GetBaseDamageMin() + weapon.GetBaseDamageMax()) / 2;
                int noWeaponAverage = (AIAttacker.MobileEnemy.MinDamage + AIAttacker.MobileEnemy.MaxDamage) / 2;

                if (noWeaponAverage > weaponAverage)
                {
                    // Use hand-to-hand
                    weapon = null;
                }
            }

            if (weapon != null)
            {
                // If the attacker is using a weapon, check if the material is high enough to damage the target
                // ProjectN: if material is below the minimum requirement to hurt a monster, damage is divided by the
                // distance between material used and reqired material; silver halves this penalty (this could be modified later on).
                // ProjectN: when attacking with a bow, it's the arrow metal that set the attacking material, not the bow's.
                if (arrow != null)
                {
                    if (target.MinMetalToHit > (MaterialTypes)arrow.NativeMaterialValue)
                    {
                        if (attacker == player)
                        {
                            DaggerfallUI.Instance.PopupMessage(TextManager.Instance.GetLocalizedText("arrowMaterialNotAdequate"));
                        }
                        metalMultiplier -= (int)target.MinMetalToHit - arrow.NativeMaterialValue;
                    }
                    // Get weapon skill used
                    skillID = weapon.GetWeaponSkillIDAsShort();
                }
                else
                {
                    if (target.MinMetalToHit > (MaterialTypes)weapon.NativeMaterialValue)
                    {
                        if (attacker == player)
                        {
                            DaggerfallUI.Instance.PopupMessage(TextManager.Instance.GetLocalizedText("materialNotAdequate"));
                        }
                        metalMultiplier -= (int)target.MinMetalToHit - weapon.NativeMaterialValue;
                    }
                    // Get weapon skill used
                    skillID = weapon.GetWeaponSkillIDAsShort();
                }
            }
            else
            {
                skillID = (short)DFCareer.Skills.HandToHand;
            }

            chanceToHitMod = attacker.Skills.GetLiveSkillValue(skillID);

            criticalStrikeChance = CalculateCriticalStrikeChance(attacker);

            if (attacker == player)
            {
                // Apply swing modifiers
                ToHitAndDamageMods swingMods = CalculateSwingModifiers(GameManager.Instance.WeaponManager.ScreenWeapon);
                damageModifiers += swingMods.damageMod;
                chanceToHitMod += swingMods.toHitMod;

                // Apply proficiency modifiers
                ToHitAndDamageMods proficiencyMods = CalculateProficiencyModifiers(attacker, weapon);
                damageModifiers += proficiencyMods.damageMod;
                chanceToHitMod += proficiencyMods.toHitMod;

                // Apply racial bonuses
                ToHitAndDamageMods racialMods = CalculateRacialModifiers(attacker, weapon, player);
                damageModifiers += racialMods.damageMod;
                chanceToHitMod += racialMods.toHitMod;

                backstabChance = CalculateBackstabChance(player, null, isEnemyFacingAwayFromPlayer, out backstabSuccessful);
                chanceToHitMod += backstabChance;
            }

            // Choose struck body part
            int struckBodyPart = CalculateStruckBodyPart();

            // Calculate chance to hit mod based on the combination of weapon/creature type
            // UPDATE: I don't like this anymore, suspending it for now
            // chanceToHitMod += CalculateWeaponTargetRelation(attacker, target, struckBodyPart, weapon);

            // Get damage for weaponless attacks
            if (skillID == (short)DFCareer.Skills.HandToHand)
            {
                if (attacker == player || (AIAttacker != null && AIAttacker.EntityType == EntityTypes.EnemyClass))
                {
                    if (CalculateSuccessfulHit(attacker, target, chanceToHitMod, struckBodyPart))
                    {
                        damage = CalculateHandToHandAttackDamage(attacker, target, damageModifiers, attacker == player);

                        damage = CalculateBackstabDamage(damage, backstabChance, weapon);

                        damage = CalculateCriticalStrikeDamage(attacker, target, criticalStrikeChance, weapon, damage, out criticalStrikeSuccessful);

                        if (!backstabSuccessful && !criticalStrikeSuccessful && (target.ItemEquipTable.GetItem(EquipSlots.LeftHand) != null && target.ItemEquipTable.GetItem(EquipSlots.LeftHand).IsShield))
                            damage = CalculateBlock(attacker, target, weapon, damage, (BodyParts)struckBodyPart, out blockSuccessful, out shieldDamage);

                        if (damage > 0)
                            damage = CalculateArmorProtection(damage, target, struckBodyPart, weapon, criticalStrikeSuccessful);

                        if (damage > 0 && metalMultiplier < 10)
                            damage = damage * metalMultiplier / 10;
                    }
                }
                else if (AIAttacker != null) // attacker is a monster
                {
                    // Handle multiple attacks by AI
                    int minBaseDamage = 0;
                    int maxBaseDamage = 0;
                    int attackNumber = 0;
                    while (attackNumber < 3) // Classic supports up to 5 attacks but no monster has more than 3
                    {
                        if (attackNumber == 0)
                        {
                            minBaseDamage = AIAttacker.MobileEnemy.MinDamage;
                            maxBaseDamage = AIAttacker.MobileEnemy.MaxDamage;
                        }
                        else if (attackNumber == 1)
                        {
                            minBaseDamage = AIAttacker.MobileEnemy.MinDamage2;
                            maxBaseDamage = AIAttacker.MobileEnemy.MaxDamage2;
                        }
                        else if (attackNumber == 2)
                        {
                            minBaseDamage = AIAttacker.MobileEnemy.MinDamage3;
                            maxBaseDamage = AIAttacker.MobileEnemy.MaxDamage3;
                        }

                        int reflexesChance = 50 - (10 * ((int)player.Reflexes - 2));

                        int hitDamage = 0;
                        if (DFRandom.rand() % 100 < reflexesChance && minBaseDamage > 0 && CalculateSuccessfulHit(attacker, target, chanceToHitMod, struckBodyPart))
                        {
                            hitDamage = UnityEngine.Random.Range(minBaseDamage, maxBaseDamage + 1);
                            // Apply special monster attack effects
                            if (hitDamage > 0)
                                OnMonsterHit(AIAttacker, target, hitDamage);

                            damage += hitDamage;
                        }

                        // Apply bonus damage only when monster has actually hit, or they will accumulate bonus damage even for missed attacks and zero-damage attacks
                        if (hitDamage > 0)
                            damage += GetBonusOrPenaltyByEnemyType(attacker, target);

                        if (hitDamage > 0)
                            damage = CalculateCriticalStrikeDamage(attacker, target, criticalStrikeChance, weapon, damage, out criticalStrikeSuccessful);
                        
                        if (!criticalStrikeSuccessful && hitDamage > 0 && target.ItemEquipTable.GetItem(EquipSlots.LeftHand) != null && target.ItemEquipTable.GetItem(EquipSlots.LeftHand).IsShield)
                            damage = CalculateBlock(attacker, target, weapon, damage, (BodyParts)struckBodyPart, out blockSuccessful, out shieldDamage);

                        if (damage > 0)
                            damage = CalculateArmorProtection(damage, target, struckBodyPart, weapon, criticalStrikeSuccessful);

                        if (metalMultiplier < 10)
                            damage = damage * metalMultiplier / 10;

                        ++attackNumber;
                    }
                }
            }
            // Handle weapon attacks
            else if (weapon != null)
            {
                // Apply weapon material modifier.
                chanceToHitMod += CalculateWeaponToHit(weapon);

                // Mod hook for adjusting final hit chance mod and adding new elements to calculation. (no-op in DFU)
                chanceToHitMod = AdjustArcheryHitChanceMod(attacker, target, chanceToHitMod, weaponAnimTime, weapon);

                if (CalculateSuccessfulHit(attacker, target, chanceToHitMod, struckBodyPart))
                {
                    damage = CalculateWeaponAttackDamage(attacker, target, damageModifiers, weaponAnimTime, weapon, arrow);

                    damage = CalculateBackstabDamage(damage, backstabChance, weapon);

                    damage = CalculateCriticalStrikeDamage(attacker, target, criticalStrikeChance, weapon, damage, out criticalStrikeSuccessful);

                    if (!backstabSuccessful && !criticalStrikeSuccessful && (target.ItemEquipTable.GetItem(EquipSlots.LeftHand) != null && target.ItemEquipTable.GetItem(EquipSlots.LeftHand).IsShield))
                        damage = CalculateBlock(attacker, target, weapon, damage, (BodyParts)struckBodyPart, out blockSuccessful, out shieldDamage);

                    if (damage > 0)
                        damage = CalculateArmorProtection(damage, target, struckBodyPart, weapon, criticalStrikeSuccessful);

                    if (damage > 0 && metalMultiplier < 10)
                        damage = damage * metalMultiplier / 10;
                }

                // Handle poisoned weapons
                if (damage > 0 && weapon.poisonType != Poisons.None)
                {
                    InflictPoison(attacker, target, weapon.poisonType, false);
                    weapon.poisonType = Poisons.None;
                }
            }

            damage = Mathf.Max(0, damage);

            DamageEquipment(attacker, target, damage, weapon, struckBodyPart, blockSuccessful, shieldDamage);

            // Apply Ring of Namira effect
            if (target == player)
            {
                DaggerfallUnityItem[] equippedItems = target.ItemEquipTable.EquipTable;
                DaggerfallUnityItem item = null;
                if (equippedItems.Length != 0)
                {
                    if (IsRingOfNamira(equippedItems[(int)EquipSlots.Ring0]) || IsRingOfNamira(equippedItems[(int)EquipSlots.Ring1]))
                    {
                        IEntityEffect effectTemplate = GameManager.Instance.EntityEffectBroker.GetEffectTemplate(RingOfNamiraEffect.EffectKey);
                        effectTemplate.EnchantmentPayloadCallback(EnchantmentPayloadFlags.None,
                            targetEntity: AIAttacker.EntityBehaviour,
                            sourceItem: item,
                            sourceDamage: damage);
                    }
                }
            }
            //Debug.LogFormat("Damage {0} applied, animTime={1}  ({2})", damage, weaponAnimTime, GameManager.Instance.WeaponManager.ScreenWeapon.WeaponState);

            return damage;
        }

        private static bool IsRingOfNamira(DaggerfallUnityItem item)
        {
            return item != null && item.ContainsEnchantment(DaggerfallConnect.FallExe.EnchantmentTypes.SpecialArtifactEffect, (int)ArtifactsSubTypes.Ring_of_Namira);
        }

        public static int CalculateBlock(DaggerfallEntity attacker, DaggerfallEntity target, DaggerfallUnityItem weapon, int damage, BodyParts struckBodyPart, out bool blockSuccessful, out int reducedDamage)
        {
            reducedDamage = 0;
            blockSuccessful = false;

            DaggerfallUnityItem shield = target.ItemEquipTable.GetItem(EquipSlots.LeftHand);
            if (shield != null && !shield.IsShield)
                return damage;

            int baseBlockChance = target.Skills.GetLiveSkillValue(DFCareer.Skills.Block) * 70 / 100;
            if (shield.GetShieldProtectedBodyParts().Contains(struckBodyPart))
            {
                if (Dice100.SuccessRoll(baseBlockChance))
                {
                    reducedDamage = damage;
                    damage = 0;
                    blockSuccessful = true;
                    if (target is EnemyEntity)
                    {
                        DaggerfallUI.Instance.PopupMessage(TextManager.Instance.GetLocalizedEnemyName((target as EnemyEntity).MobileEnemy.ID) + " " + TextManager.Instance.GetLocalizedText("enemyShieldBlocked") + " " + shield.LongName);
                    }
                    else{ 
                        // Tally player's block skill
                        GameManager.Instance.PlayerEntity.TallySkill(DFCareer.Skills.Block, 1);
                        DaggerfallUI.Instance.PopupMessage(TextManager.Instance.GetLocalizedText("playerShieldBlocked") + " " + shield.LongName);
                    }
                }
            }
            else
            {
                if (Dice100.SuccessRoll(baseBlockChance / 2))
                {
                    reducedDamage = damage * (baseBlockChance / 2) / 100;
                    damage -= reducedDamage;
                    blockSuccessful = true;
                    if (target is EnemyEntity)
                    {
                        DaggerfallUI.Instance.PopupMessage(TextManager.Instance.GetLocalizedEnemyName((target as EnemyEntity).MobileEnemy.ID) + " " + TextManager.Instance.GetLocalizedText("enemyShieldDeflect"));
                    }
                    else{
                        // Tally player's block skill
                        GameManager.Instance.PlayerEntity.TallySkill(DFCareer.Skills.Block, 1);
                        DaggerfallUI.Instance.PopupMessage(TextManager.Instance.GetLocalizedText("playerShieldDeflect"));
                    } 
                }
            }
            return damage;
        }

        public static int CalculateArmorProtection(int damage, DaggerfallEntity target, int struckBodyPart, DaggerfallUnityItem weapon, bool criticalStrikeSuccessful)
        {
            int armorValue = 0;
            int csArmorModifier = 1;
            if (criticalStrikeSuccessful)
                csArmorModifier = GetCSArmorModifier(weapon);
            if (struckBodyPart <= target.ArmorValues.Length)
            {
                armorValue = target.ArmorValues[struckBodyPart] + target.IncreasedArmorValueModifier + target.DecreasedArmorValueModifier;
                Debug.Log("target.ArmorValues[struckBodyPart]: " + target.ArmorValues[struckBodyPart] + ", target.IncreasedArmorValueModifier: " + target.IncreasedArmorValueModifier + ", target.DecreasedArmorValueModifier: " + ", armorValue: " + armorValue);
            }
            armorValue = armorValue * csArmorModifier / 10;

            DaggerfallUnityItem armor = target.ItemEquipTable.GetItem(BodyPartsToEquipSlots((BodyParts)struckBodyPart, true));

            if (armor != null)
            {
                ArmorTypes armorType = ItemBuilder.GetArmorType(armor.NativeMaterialValue);

                if (armorType == ArmorTypes.Leather || armorType == ArmorTypes.Fur)
                {
                    if (weapon.GetWeaponSkillID() == DFCareer.Skills.BluntWeapon)
                        armorValue += armorValue * weaponArmorMultiplier / 100;
                    else if (weapon.GetWeaponSkillID() == DFCareer.Skills.LongBlade)
                        armorValue = armorValue * weaponArmorMultiplier / 100;
                    GameManager.Instance.PlayerEntity.TallySkill(DFCareer.Skills.LightArmor, 1);
                }
                else if (armorType == ArmorTypes.Chain)
                {
                    if (weapon.GetWeaponSkillID() == DFCareer.Skills.ShortBlade ||
                        weapon.GetWeaponSkillID() == DFCareer.Skills.HandToHand)
                        armorValue += armorValue * weaponArmorMultiplier / 100;
                    else if (weapon.GetWeaponSkillID() == DFCareer.Skills.Axe)
                        armorValue = armorValue * weaponArmorMultiplier / 100;
                    GameManager.Instance.PlayerEntity.TallySkill(DFCareer.Skills.MediumArmor, 1);
                }
                else if (armorType == ArmorTypes.Plate)
                {
                    if (weapon.GetWeaponSkillID() == DFCareer.Skills.ShortBlade ||
                        weapon.GetWeaponSkillID() == DFCareer.Skills.HandToHand ||
                        weapon.GetWeaponSkillID() == DFCareer.Skills.LongBlade)
                        armorValue += armorValue * weaponArmorMultiplier / 100;
                    else if (weapon.GetWeaponSkillID() == DFCareer.Skills.BluntWeapon)
                        armorValue = armorValue * weaponArmorMultiplier / 100;
                    GameManager.Instance.PlayerEntity.TallySkill(DFCareer.Skills.HeavyArmor, 1);
                }
            }
            else{
                // Unarmored doesn't modify damage, since it doesn't seem to make much sense.
                // But many things I did, didn't make sense. This is still open to change.
            }

            Debug.Log("damage: " + damage + ", struckBodyPart: " + (BodyParts)struckBodyPart + ", armorValue: " + armorValue + ", final damage: " + (damage - armorValue));

            return (damage - armorValue);
        }

        public static int GetCSArmorModifier(DaggerfallUnityItem weapon)
        {
            if (weapon == null)
                return 10;
            
            DFCareer.Skills weaponType = weapon.GetWeaponSkillID();
            switch (weaponType)
            {
                case DFCareer.Skills.ShortBlade:
                    return 0;
                case DFCareer.Skills.LongBlade:
                case DFCareer.Skills.Archery:
                    return 5;
                case DFCareer.Skills.Axe:
                    return 7;
                default:
                    return 10;
            }
        }

        public static EquipSlots BodyPartsToEquipSlots(BodyParts bodyPart, bool isArmor)
        {
            if (isArmor)
                return BodyPartsToArmorEquipSlots(bodyPart);
            else return BodyPartsToClothesEquipSlots(bodyPart);
        }

        public static EquipSlots BodyPartsToArmorEquipSlots(BodyParts armorPart)
        {
            switch (armorPart)
            {
                case BodyParts.Head:
                    return EquipSlots.Head;
                case BodyParts.RightArm:
                    return EquipSlots.RightArm;
                case BodyParts.LeftArm:
                    return EquipSlots.LeftArm;
                case BodyParts.Chest:
                    return EquipSlots.ChestArmor;
                case BodyParts.Hands:
                    return EquipSlots.Gloves;
                case BodyParts.Legs:
                    return EquipSlots.LegsArmor;
                case BodyParts.Feet:
                    return EquipSlots.Feet;
                default:
                    return EquipSlots.None;
            }
        }

        public static EquipSlots BodyPartsToClothesEquipSlots(BodyParts clothesPart)
        {
            switch (clothesPart)
            {
                case BodyParts.Head:
                    return EquipSlots.Head;
                case BodyParts.RightArm:
                case BodyParts.LeftArm:
                case BodyParts.Chest:
                    return EquipSlots.ChestClothes;
                case BodyParts.Hands:
                    return EquipSlots.Gloves;
                case BodyParts.Legs:
                    return EquipSlots.LegsClothes;
                case BodyParts.Feet:
                    return EquipSlots.Feet;
                default:
                    return EquipSlots.None;
            }
        }

        public static int CalculateWeaponAttackDamage(DaggerfallEntity attacker, DaggerfallEntity target, int damageModifier, int weaponAnimTime, DaggerfallUnityItem weapon, DaggerfallUnityItem arrow = null)
        {
            Func<DaggerfallEntity, DaggerfallEntity, int, int, DaggerfallUnityItem, int> del;
            if (TryGetOverride("CalculateWeaponAttackDamage", out del))
                return del(attacker, target, damageModifier, weaponAnimTime, weapon);

            int damage = UnityEngine.Random.Range(weapon.GetBaseDamageMin(), weapon.GetBaseDamageMax() + 1) + damageModifier;

            if (target != GameManager.Instance.PlayerEntity)
            {
                if ((target as EnemyEntity).GetEnemyGroup() == DFCareer.EnemyGroups.Undead)
                {
                    if ((target as EnemyEntity).CareerIndex == (int)MonsterCareers.AncientLich ||
                        (target as EnemyEntity).CareerIndex == (int)MonsterCareers.Lich ||
                        (target as EnemyEntity).CareerIndex == (int)MonsterCareers.SkeletalWarrior)
                    // Apply edged-weapon damage modifier for skeletal types
                    if (weapon.GetWeaponSkillID() != DFCareer.Skills.BluntWeapon)
                    {
                        damage /= 2;
                    }
                    else if ((target as EnemyEntity).CareerIndex != (int)MonsterCareers.Vampire &&
                             (target as EnemyEntity).CareerIndex != (int)MonsterCareers.VampireAncient &&
                             (target as EnemyEntity).CareerIndex != (int)MonsterCareers.Ghost &&
                             (target as EnemyEntity).CareerIndex != (int)MonsterCareers.Wraith)
                    // Other types of undead aside from vampires and ghosts get half dmage from
                    // not-blunt, not-axe weapons
                    if (weapon.GetWeaponSkillID() != DFCareer.Skills.BluntWeapon &&
                        weapon.GetWeaponSkillID() != DFCareer.Skills.Axe)
                    {
                        damage /= 2;
                    }


                    // Apply silver weapon damage modifier for Skeletal Warrior
                    // Arena applies a silver weapon damage bonus for undead enemies, which is probably where this comes from.
                    // ProjectN: going back to Arena, now all undead gets double damage from silver weapons
                    if (weapon.NativeMaterialValue == (int)MaterialTypes.Silver)
                        damage *= 2;
                }
            }
            // TODO: Apply strength bonus from Mace of Molag Bal

            // Apply strength modifier
            damage += DamageModifier(attacker.Stats.LiveStrength);

            // Apply material modifier.
            // The in-game display in Daggerfall of weapon damages with material modifiers is incorrect. The material modifier is half of what the display suggests.
            // ProjectN: missile weapons get the average between the weapon and the arrow material damage modifier
            if (arrow != null)
                damage += (weapon.GetWeaponMaterialModifier() + arrow.GetWeaponMaterialModifier()) / 2;
            else damage += weapon.GetWeaponMaterialModifier();

            damage = AdjustArcheryAttackDamage(attacker, target, damage, weaponAnimTime, weapon);

            damage += GetBonusOrPenaltyByEnemyType(attacker, target);
            
            if (damage < 1)
                damage = 0;

            return damage;
        }

        public static int CalculateHandToHandAttackDamage(DaggerfallEntity attacker, DaggerfallEntity target, int damageModifier, bool player)
        {
            Func<DaggerfallEntity, DaggerfallEntity, int, int> del;
            if (TryGetOverride("CalculateHandToHandAttackDamage", out del))
                return del(attacker, target, damageModifier);

            int minBaseDamage = CalculateHandToHandMinDamage(attacker.Skills.GetLiveSkillValue(DFCareer.Skills.HandToHand));
            int maxBaseDamage = CalculateHandToHandMaxDamage(attacker.Skills.GetLiveSkillValue(DFCareer.Skills.HandToHand));
            int damage = UnityEngine.Random.Range(minBaseDamage, maxBaseDamage + 1);

            // Apply damage modifiers.
            damage += damageModifier;

            // Apply strength modifier for players. It is not applied in classic despite what the in-game description for the Strength attribute says.
            if (player)
                damage += DamageModifier(attacker.Stats.LiveStrength);

            damage += GetBonusOrPenaltyByEnemyType(attacker, target);

            return damage;
        }

        /// <summary>
        /// Calculates whether an attack on a target is successful or not.
        /// </summary>
        public static bool CalculateSuccessfulHit(DaggerfallEntity attacker, DaggerfallEntity target, int chanceToHitMod, int struckBodyPart)
        {
            if (attacker == null || target == null)
                return false;

            Func<DaggerfallEntity, DaggerfallEntity, int, int, bool> del;
            if (TryGetOverride("CalculateSuccessfulHit", out del))
                return del(attacker, target, chanceToHitMod, struckBodyPart);

            int chanceToHit = chanceToHitMod;
            Debug.Log("1. chanceToHit: " + chanceToHit);

            // Get armor value for struck body part
            // ProjectN: Armor Value isn't a factor in chanceToHit anymore, but it's important
            // to leave the bonus to hit it confers, or everything else will fall out of place
            // chanceToHit += CalculateArmorToHit(target, struckBodyPart);
            // chanceToHit += 100;
            Debug.Log("2. chanceToHit: " + chanceToHit);

            // Apply adrenaline rush modifiers.
            chanceToHit += CalculateAdrenalineRushToHit(attacker, target);

            // Apply enchantment modifier
            chanceToHit += attacker.ChanceToHitModifier;

            // Apply stat differential modifiers. (default: luck and agility)
            chanceToHit += CalculateStatsToHit(attacker, target);

            // Apply skill modifiers. (default: dodge and crit strike)
            // ProjectN: critical strike isn't a bonus to hit anymore, but modifies damage dealt.
            chanceToHit += CalculateSkillsToHit(attacker, target);
            

            // Apply monster modifier and biography adjustments.
            chanceToHit += CalculateAdjustmentsToHit(attacker, target);

            Debug.Log("3. chanceToHit: " + chanceToHit);
            chanceToHit = Mathf.Clamp(chanceToHit, 3, 97);

            return Dice100.SuccessRoll(chanceToHit);
        }

        public static int CalculateWeaponTargetRelation(DaggerfallEntity attacker, DaggerfallEntity target, int struckBodyPart, DaggerfallUnityItem weapon)
        {
            if (target is EnemyEntity)
            {
                var enemyTarget = target as EnemyEntity;
                if (enemyTarget.MobileEnemy.Affinity == MobileAffinity.Human)
                {
                    if (weapon.GetWeaponSkillID() == DFCareer.Skills.LongBlade)
                        return +10;
                    else return 0;
                }
                else if (enemyTarget.GetEnemyGroup() == DFCareer.EnemyGroups.Undead)
                {
                    if (weapon.GetWeaponSkillID() == DFCareer.Skills.ShortBlade ||
                        weapon.GetWeaponSkillID() == DFCareer.Skills.LongBlade ||
                        weapon.GetWeaponSkillID() == DFCareer.Skills.Archery)
                        return -10;
                    else if (weapon.GetWeaponSkillID() == DFCareer.Skills.BluntWeapon)
                        return +10;
                    else return 0;
                }
                else if (enemyTarget.GetEnemyGroup() == DFCareer.EnemyGroups.Animals)
                {
                    if (weapon.GetWeaponSkillID() == DFCareer.Skills.Axe ||
                        weapon.GetWeaponSkillID() == DFCareer.Skills.Archery)
                        return +10;
                    else return 0;
                }
            }
            else if (target is PlayerEntity)
            {
                if (GameManager.Instance.PlayerEffectManager.HasVampirism()) // Vampires are undead, therefore use undead modifier
                {
                    if (weapon.GetWeaponSkillID() == DFCareer.Skills.ShortBlade ||
                        weapon.GetWeaponSkillID() == DFCareer.Skills.LongBlade ||
                        weapon.GetWeaponSkillID() == DFCareer.Skills.Archery)
                        return -10;
                    else if (weapon.GetWeaponSkillID() == DFCareer.Skills.BluntWeapon)
                        return +10;
                    else return 0;
                }
                else
                {
                    // Player is assumed humanoid
                    if (weapon.GetWeaponSkillID() == DFCareer.Skills.LongBlade)
                        return +10;
                    else return 0;
                }
            }

            return 0;
        }

        public static float GetMeleeWeaponAnimTime(PlayerEntity player, WeaponTypes weaponType, ItemHands weaponHands)
        {
            EquipSlots weaponSlot = GameManager.Instance.WeaponManager.UsingRightHand ? EquipSlots.RightHand : EquipSlots.LeftHand;
            DaggerfallUnityItem weapon = player.ItemEquipTable.GetItem(weaponSlot);
            int adjustedSpeed = 0;
            float weaponWeight = 0f;
            float SpeedReductionFactor = 3.4f;

            if (weaponType == WeaponTypes.Melee || weapon == null)
            {
                adjustedSpeed = player.Stats.LiveSpeed;
            }
            else
            {
                weaponWeight = weapon.ItemTemplate.baseWeight;
                int strWeightPerc = 150 - player.Stats.LiveStrength;
                float adjustedWeight = strWeightPerc * weaponWeight / 100;
                float speedReductionPerc = adjustedWeight * SpeedReductionFactor;
                int playerSpeed = Mathf.Min(player.Stats.LiveSpeed, 98);    // Cap speed at 98%

                adjustedSpeed = (int)(playerSpeed - (playerSpeed * speedReductionPerc / 90));
            }
            float frameSpeed = 3 * (115 - adjustedSpeed);

#if UNITY_EDITOR
            Debug.LogFormat("anim= {0}ms/frame, speed={1} strength={2} weight={3} adjustedSpeed={4}", frameSpeed / FormulaHelper.classicFrameUpdate, player.Stats.LiveSpeed, player.Stats.LiveStrength, weaponWeight, adjustedSpeed);
#endif
            return frameSpeed / FormulaHelper.classicFrameUpdate;
        }

        public static float GetBowCooldownTime(PlayerEntity player)
        {
            Func<PlayerEntity, float> del;
            if (TryGetOverride("GetBowCooldownTime", out del))
                return del(player);

            float cooldown = 10 * (100 - player.Stats.LiveSpeed) + 800;
            return cooldown / classicFrameUpdate;
        }

        public static int CalculateCasterLevel(DaggerfallEntity caster, IEntityEffect effect)
        {
            Func<DaggerfallEntity, IEntityEffect, int> del;
            if (TryGetOverride("CalculateCasterLevel", out del))
                return del(caster, effect);

            return caster != null ? caster.Level : 1;
        }

        #endregion

        #region Combat & Damage: component sub-formula

        public static int CalculateStruckBodyPart()
        {
            Func<int> del;
            if (TryGetOverride("CalculateStruckBodyPart", out del))
                return del();

            int[] bodyParts = { 0, 0, 1, 1, 1, 2, 2, 2, 3, 3, 3, 3, 4, 4, 4, 4, 5, 5, 5, 6 };
            return bodyParts[UnityEngine.Random.Range(0, bodyParts.Length)];
        }

        public static ToHitAndDamageMods CalculateSwingModifiers(FPSWeapon onscreenWeapon)
        {
            Func<FPSWeapon, ToHitAndDamageMods> del;
            if (TryGetOverride("CalculateSwingModifiers", out del))
                return del(onscreenWeapon);

            ToHitAndDamageMods mods = new ToHitAndDamageMods();
            if (onscreenWeapon != null && onscreenWeapon.SpecificWeapon.GetWeaponSkillID() != DFCareer.Skills.ShortBlade)
            {
                // The Daggerfall manual groups diagonal slashes to the left and right as if they are the same, but they are different.
                // Classic does not apply swing modifiers to unarmed attacks.
                if (onscreenWeapon.WeaponState == WeaponStates.StrikeUp)
                {
                    mods.damageMod = -4;
                    mods.toHitMod = 10;
                }
                if (onscreenWeapon.WeaponState == WeaponStates.StrikeDownRight)
                {
                    mods.damageMod = -2;
                    mods.toHitMod = 5;
                }
                if (onscreenWeapon.WeaponState == WeaponStates.StrikeDownLeft)
                {
                    mods.damageMod = 2;
                    mods.toHitMod = -5;
                }
                if (onscreenWeapon.WeaponState == WeaponStates.StrikeDown)
                {
                    mods.damageMod = 4;
                    mods.toHitMod = -10;
                }
            }
            else{
                if (onscreenWeapon.WeaponState == WeaponStates.StrikeUp)
                {
                    mods.damageMod = 4;
                    mods.toHitMod = -10;
                }
                if (onscreenWeapon.WeaponState == WeaponStates.StrikeRight)
                {
                    mods.damageMod = -4;
                    mods.toHitMod = 10;
                }
                if (onscreenWeapon.WeaponState == WeaponStates.StrikeDownLeft)
                {
                    mods.damageMod = 2;
                    mods.toHitMod = -5;
                }
                if (onscreenWeapon.WeaponState == WeaponStates.StrikeDown)
                {
                    mods.damageMod = -2;
                    mods.toHitMod = 5;
                }
            }
            return mods;
        }

        public static ToHitAndDamageMods CalculateProficiencyModifiers(DaggerfallEntity attacker, DaggerfallUnityItem weapon)
        {
            Func<DaggerfallEntity, DaggerfallUnityItem, ToHitAndDamageMods> del;
            if (TryGetOverride("CalculateProficiencyModifiers", out del))
                return del(attacker, weapon);

            ToHitAndDamageMods mods = new ToHitAndDamageMods();
            if (weapon != null)
            {
                // Apply weapon proficiency
                if (((int)attacker.Career.ExpertProficiencies & weapon.GetWeaponSkillUsed()) != 0)
                {
                    mods.damageMod = (attacker.Level / 3) + 1;
                    mods.toHitMod = attacker.Level;
                }
            }
            // Apply hand-to-hand proficiency. Hand-to-hand proficiency is not applied in classic.
            else if (((int)attacker.Career.ExpertProficiencies & (int)DFCareer.ProficiencyFlags.HandToHand) != 0)
            {
                mods.damageMod = (attacker.Level / 3) + 1;
                mods.toHitMod = attacker.Level;
            }
            return mods;
        }

        public static ToHitAndDamageMods CalculateRacialModifiers(DaggerfallEntity attacker, DaggerfallUnityItem weapon, PlayerEntity player)
        {
            Func<DaggerfallEntity, DaggerfallUnityItem, PlayerEntity, ToHitAndDamageMods> del;
            if (TryGetOverride("CalculateRacialModifiers", out del))
                return del(attacker, weapon, player);

            ToHitAndDamageMods mods = new ToHitAndDamageMods();
            if (weapon != null)
            {
                if (player.RaceTemplate.ID == (int)Races.DarkElf)
                {
                    mods.damageMod = attacker.Level / 4;
                    mods.toHitMod = attacker.Level / 4;
                }
                else if (weapon.GetWeaponSkillIDAsShort() == (short)DFCareer.Skills.Archery)
                {
                    if (player.RaceTemplate.ID == (int)Races.WoodElf)
                    {
                        mods.damageMod = attacker.Level / 3;
                        mods.toHitMod = attacker.Level / 3;
                    }
                }
                else if (player.RaceTemplate.ID == (int)Races.Redguard)
                {
                    mods.damageMod = attacker.Level / 3;
                    mods.toHitMod = attacker.Level / 3;
                }
            }
            return mods;
        }

        public static int CalculateBackstabChance(PlayerEntity player, DaggerfallEntity target, bool isEnemyFacingAwayFromPlayer, out bool backstabSuccessful)
        {
            backstabSuccessful = false;

            if (isEnemyFacingAwayFromPlayer)
            {
                player.TallySkill(DFCareer.Skills.Backstabbing, 1);
                backstabSuccessful = true;
                return player.Skills.GetLiveSkillValue(DFCareer.Skills.Backstabbing);
            }
            return 0;
        }

        public static int CalculateBackstabDamage(int damage, int backstabbingLevel, DaggerfallUnityItem weapon)
        {
            Func<int, int, int> del;
            if (TryGetOverride("CalculateBackstabDamage", out del))
                return del(damage, backstabbingLevel);

            if (backstabbingLevel > 1 && Dice100.SuccessRoll(backstabbingLevel))
            {
                damage *= GetBackstabDamageMultiplier(weapon);
                string backstabMessage = TextManager.Instance.GetLocalizedText("successfulBackstab");
                DaggerfallUI.Instance.PopupMessage(backstabMessage);
            }
            return damage;
        }

        public static int GetBackstabDamageMultiplier(DaggerfallUnityItem weapon)
        {
            if (weapon.GetWeaponSkillID() == DFCareer.Skills.Archery)
                return 3;
            if (weapon.GetItemHands() == ItemHands.Both)
                return 2;
            if (weapon.GetWeaponSkillID() == DFCareer.Skills.ShortBlade)
                return 5;
            if (weapon.GetWeaponSkillID() == DFCareer.Skills.LongBlade)
                return 3;
            if (weapon.GetWeaponSkillID() == DFCareer.Skills.Axe)
                return 3;
            if (weapon.GetWeaponSkillID() == DFCareer.Skills.BluntWeapon)
                return 2;
            if (weapon.GetWeaponSkillID() == DFCareer.Skills.HandToHand)
                return 2;
            return 1;
        }

        public static int CalculateCriticalStrikeChance(DaggerfallEntity attacker)
        {
            return attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / 3;
        }

        public static int CalculateCriticalStrikeDamage(DaggerfallEntity attacker, DaggerfallEntity target, int criticalStrikeChance, DaggerfallUnityItem weapon, int damageMod, out bool criticalStrikeSuccessful)
        {
            criticalStrikeSuccessful = false;
            if (Dice100.SuccessRoll(criticalStrikeChance))
            {
                criticalStrikeSuccessful = true;

                if (weapon.GetWeaponSkillID() == DFCareer.Skills.LongBlade)
                    damageMod = damageMod * 13 / 10;
                else if (weapon.GetWeaponSkillID() == DFCareer.Skills.Axe ||
                         weapon.GetWeaponSkillID() == DFCareer.Skills.Archery)
                    damageMod = damageMod * 15 / 10;
                else if (weapon.GetWeaponSkillID() == DFCareer.Skills.BluntWeapon)
                    damageMod = damageMod * 2;

                if (target is EnemyEntity)
                {
                    DaggerfallUI.Instance.PopupMessage(TextManager.Instance.GetLocalizedEnemyName((target as EnemyEntity).MobileEnemy.ID) + " " + TextManager.Instance.GetLocalizedText("enemyCriticalStrike"));
                }
                else
                {
                    // Tally player's CS skill
                    GameManager.Instance.PlayerEntity.TallySkill(DFCareer.Skills.CriticalStrike, 1);
                    DaggerfallUI.Instance.PopupMessage(TextManager.Instance.GetLocalizedText("playerCriticalStrike"));
                } 
            }

            return damageMod;
        }

        public static int GetBonusOrPenaltyByEnemyType(DaggerfallEntity attacker, DaggerfallEntity target)
        {
            Func<DaggerfallEntity, DaggerfallEntity, int> del;
            if (TryGetOverride("GetBonusOrPenaltyByEnemyType", out del))
                return del(attacker, target);

            if (attacker == null || target == null)
                return 0;

            int damage = 0;
            // Apply bonus or penalty by opponent type.
            // In classic this is broken and only works if the attack is done with a weapon that has the maximum number of enchantments.
            if (target is EnemyEntity)
            {
                var enemyTarget = target as EnemyEntity;
                if (enemyTarget.MobileEnemy.Affinity == MobileAffinity.Human)
                {
                    if (((int)attacker.Career.HumanoidAttackModifier & (int)DFCareer.AttackModifier.Bonus) != 0)
                        damage += attacker.Level;
                    if (((int)attacker.Career.HumanoidAttackModifier & (int)DFCareer.AttackModifier.Phobia) != 0)
                        damage -= attacker.Level;
                }
                else if (enemyTarget.GetEnemyGroup() == DFCareer.EnemyGroups.Undead)
                {
                    if (((int)attacker.Career.UndeadAttackModifier & (int)DFCareer.AttackModifier.Bonus) != 0)
                        damage += attacker.Level;
                    if (((int)attacker.Career.UndeadAttackModifier & (int)DFCareer.AttackModifier.Phobia) != 0)
                        damage -= attacker.Level;
                }
                else if (enemyTarget.GetEnemyGroup() == DFCareer.EnemyGroups.Daedra)
                {
                    if (((int)attacker.Career.DaedraAttackModifier & (int)DFCareer.AttackModifier.Bonus) != 0)
                        damage += attacker.Level;
                    if (((int)attacker.Career.DaedraAttackModifier & (int)DFCareer.AttackModifier.Phobia) != 0)
                        damage -= attacker.Level;
                }
                else if (enemyTarget.GetEnemyGroup() == DFCareer.EnemyGroups.Animals)
                {
                    if (((int)attacker.Career.AnimalsAttackModifier & (int)DFCareer.AttackModifier.Bonus) != 0)
                        damage += attacker.Level;
                    if (((int)attacker.Career.AnimalsAttackModifier & (int)DFCareer.AttackModifier.Phobia) != 0)
                        damage -= attacker.Level;
                }
            }
            else if (target is PlayerEntity)
            {
                if (GameManager.Instance.PlayerEffectManager.HasVampirism()) // Vampires are undead, therefore use undead modifier
                {
                    if (((int)attacker.Career.UndeadAttackModifier & (int)DFCareer.AttackModifier.Bonus) != 0)
                        damage += attacker.Level;
                    if (((int)attacker.Career.UndeadAttackModifier & (int)DFCareer.AttackModifier.Phobia) != 0)
                        damage -= attacker.Level;
                }
                else
                {
                    // Player is assumed humanoid
                    if (((int)attacker.Career.HumanoidAttackModifier & (int)DFCareer.AttackModifier.Bonus) != 0)
                        damage += attacker.Level;
                    if (((int)attacker.Career.HumanoidAttackModifier & (int)DFCareer.AttackModifier.Phobia) != 0)
                        damage -= attacker.Level;
                }
            }

            return damage;
        }

        public static int AdjustArcheryHitChanceMod(DaggerfallEntity attacker, DaggerfallEntity target, int hitChanceMod, int weaponAnimTime, DaggerfallUnityItem weapon)
        {
            if (weaponAnimTime > 0 && (weapon.TemplateIndex == (int)Weapons.Short_Bow || weapon.TemplateIndex == (int)Weapons.Long_Bow))
            {
                int adjustedHitChanceMod = hitChanceMod;
                if (weaponAnimTime < 200)
                    adjustedHitChanceMod -= 40;
                else if (weaponAnimTime < 500)
                    adjustedHitChanceMod -= 10;
                else if (weaponAnimTime < 1000)
                    adjustedHitChanceMod = hitChanceMod;
                else if (weaponAnimTime < 2000)
                    adjustedHitChanceMod += 10;
                else if (weaponAnimTime > 5000)
                    adjustedHitChanceMod -= 10;
                else if (weaponAnimTime > 8000)
                    adjustedHitChanceMod -= 20;

#if UNITY_EDITOR
                Debug.LogFormat("Adjusted Weapon HitChanceMod for bow drawing from {0} to {1} (t={2}ms)", hitChanceMod, adjustedHitChanceMod, weaponAnimTime);
#endif
                return adjustedHitChanceMod;
            }
            return hitChanceMod;
        }

        public static int AdjustArcheryAttackDamage(DaggerfallEntity attacker, DaggerfallEntity target, int damage, int weaponAnimTime, DaggerfallUnityItem weapon)
        {
            if (weaponAnimTime > 0 && (weapon.TemplateIndex == (int)Weapons.Short_Bow || weapon.TemplateIndex == (int)Weapons.Long_Bow))
            {
                double adjustedDamage = damage;
                if (weaponAnimTime < 800)
                    adjustedDamage *= (double)weaponAnimTime / 800;
                else if (weaponAnimTime < 5000)
                    adjustedDamage = damage;
                else if (weaponAnimTime < 6000)
                    adjustedDamage *= 0.85;
                else if (weaponAnimTime < 8000)
                    adjustedDamage *= 0.75;
                else if (weaponAnimTime < 9000)
                    adjustedDamage *= 0.5;
                else if (weaponAnimTime >= 9000)
                    adjustedDamage *= 0.25;

#if UNITY_EDITOR
                Debug.LogFormat("Adjusted Weapon Damage for bow drawing from {0} to {1} (t={2}ms)", damage, (int)adjustedDamage, weaponAnimTime);
#endif
                return (int)adjustedDamage;
            }
            return damage;
        }

        /// <summary>
        /// Allocate any equipment damage from a strike, and reduce item condition.
        /// </summary>
        public static void DamageEquipment(DaggerfallEntity attacker, DaggerfallEntity target, int damage, DaggerfallUnityItem weapon, int struckBodyPart, bool blockSuccessful, int shieldDamage = 0)
        {
            Func<DaggerfallEntity, DaggerfallEntity, int, DaggerfallUnityItem, int, bool> del;
            if (TryGetOverride("DamageEquipment", out del))
                if (del(attacker, target, damage, weapon, struckBodyPart))
                    return; // Only return if override returns true

            // If damage was done by a weapon, damage the weapon and armor of the hit body part.
            // In classic, shields are never damaged, only armor specific to the hitbody part is.
            // Here, if an equipped shield covers the hit body part, it takes damage instead.
            // ProjectN: the shield gets damaged if it partially or completely deflects a hit, 
            // armor if some damage pass the block or block fails.
            // The types of weapon, armor and material influence how much damage is delivered to
            // the item.

            MaterialTypes weaponMaterial = 0;
            MaterialTypes armorMaterial = 0;
            MaterialTypes shieldMaterial = 0;
            DaggerfallUnityItem armor = target.ItemEquipTable.GetItem(BodyPartsToEquipSlots((BodyParts)struckBodyPart, true));
            DaggerfallUnityItem shield = null;
            if (blockSuccessful)
                shield = target.ItemEquipTable.GetItem(EquipSlots.LeftHand);

            if (weapon != null)
                weaponMaterial = (MaterialTypes)weapon.NativeMaterialValue;
            if (armor != null)
                armorMaterial = ItemBuilder.GetArmorMaterialType(target.ItemEquipTable.GetItem(BodyPartsToEquipSlots((BodyParts)struckBodyPart, true)).NativeMaterialValue);
            if (blockSuccessful)
                shieldMaterial = ItemBuilder.GetArmorMaterialType(target.ItemEquipTable.GetItem(EquipSlots.LeftHand).NativeMaterialValue);
            if (weapon != null && (damage > 0 || shieldDamage > 0))
            {
                int weaponDamageMultiplier = 1;
                // TODO: If attacker is AI, apply Ring of Namira effect
                if (damage > 0)
                {   
                    if (armor != null)
                    {
                        weaponDamageMultiplier = armorMaterial - weaponMaterial;
                        if (ItemBuilder.GetArmorType(armor.NativeMaterialValue) != ArmorTypes.Chain &&
                            ItemBuilder.GetArmorType(armor.NativeMaterialValue) != ArmorTypes.Plate)
                            weaponDamageMultiplier /= 2;
                        if (weaponDamageMultiplier < 1) weaponDamageMultiplier = 1;
                        ApplyConditionDamageThroughPhysicalHit(weapon, attacker, (damage * weaponDamageMultiplier), true);
                    }
                    else{
                        ApplyConditionDamageThroughPhysicalHit(weapon, attacker, damage, true);
                    }
                }
                if (shieldDamage > 0)
                {
                    if (shield != null)
                    {
                        weaponDamageMultiplier = shieldMaterial - weaponMaterial;
                        if (ItemBuilder.GetArmorType(shield.NativeMaterialValue) != ArmorTypes.Chain &&
                            ItemBuilder.GetArmorType(shield.NativeMaterialValue) != ArmorTypes.Plate)
                            weaponDamageMultiplier /= 2;
                        if (weaponDamageMultiplier < 1) weaponDamageMultiplier = 1;
                        ApplyConditionDamageThroughPhysicalHit(weapon, attacker, (damage * weaponDamageMultiplier), true);
                    }
                    else{
                        ApplyConditionDamageThroughPhysicalHit(weapon, attacker, damage);
                    }
                    
                }

                // if (shield != null)
                // {
                //     BodyParts[] protectedBodyParts = shield.GetShieldProtectedBodyParts();

                //     for (int i = 0; (i < protectedBodyParts.Length) && !shieldTakesDamage; i++)
                //     {
                //         if (protectedBodyParts[i] == (BodyParts)struckBodyPart)
                //             shieldTakesDamage = true;
                //     }
                // }               
            }

            if (blockSuccessful)
            {
                if (shield != null)
                {
                    int shieldDamageMultiplier = weaponMaterial - shieldMaterial;
                    if (shieldDamageMultiplier < 1) shieldDamageMultiplier = 1;
                    ApplyConditionDamageThroughPhysicalHit(shield, target, (shieldDamage * shieldDamageMultiplier), false, weapon);
                }
                else{
                    ApplyConditionDamageThroughPhysicalHit(shield, target, shieldDamage, false, weapon);
                }
            }

            if (damage > 0 && armor != null)
            {
                int armorDamageMultiplier = weaponMaterial - armorMaterial;
                if (armorDamageMultiplier < 1) armorDamageMultiplier = 1;
                ApplyConditionDamageThroughPhysicalHit(armor, target, (damage * armorDamageMultiplier), false, weapon);
            }

            // ProjectN: if there's no armor covering that body part, damage clothes instead.
            if (damage > 0 && armor == null)
            {
                DaggerfallUnityItem cloth = target.ItemEquipTable.GetItem(BodyPartsToEquipSlots((BodyParts)struckBodyPart, false));
                if (cloth != null)
                    ApplyConditionDamageThroughPhysicalHit(cloth, target, damage, false, weapon);
            }
        }

        /// <summary>
        /// Applies condition damage to an item based on physical hit damage.
        /// </summary>
        public static void ApplyConditionDamageThroughPhysicalHit(DaggerfallUnityItem item, DaggerfallEntity owner, int damage, bool isWeapon = false, DaggerfallUnityItem weapon = null)
        {            
            int amount = 0;
            int resilienceFactor = 100;

            if (isWeapon)
            {
                resilienceFactor = GetWeaponResilience(item);
                // amount = (10 * damage + 50) / 100;
                amount = damage / resilienceFactor;
            }
            else{
                resilienceFactor = GetWeaponResilience(weapon);
                amount = damage * resilienceFactor;
            }
                
            if (amount == 0) amount = 1;

            if (item.IsEnchanted && owner is PlayerEntity)
                item.LowerCondition(amount, owner, (owner as PlayerEntity).Items);      // Lower condition and trigger removal for magic items (will not be removed if AllowMagicRepairs enabled)
            else
                item.LowerCondition(amount, owner);                                     // Lower condition of mundane item and do not remove if it breaks
        }

        public static int GetWeaponResilience(DaggerfallUnityItem weapon)
        {
            switch (weapon.GetWeaponSkillID())
            {
                case DFCareer.Skills.ShortBlade:
                case DFCareer.Skills.LongBlade:
                    return 5;
                case DFCareer.Skills.Axe:
                    return 7;
                case DFCareer.Skills.BluntWeapon:
                case DFCareer.Skills.HandToHand:
                default:
                    return 10;
            }
        }

        public static int CalculateWeaponToHit(DaggerfallUnityItem weapon)
        {
            Func<DaggerfallUnityItem, int> del;
            if (TryGetOverride("CalculateWeaponToHit", out del))
                return del(weapon);

            // ProjectN: I'm removing the "x10" multiplier to material bonus (and malus) to hit;
            return weapon.GetWeaponMaterialModifier();
        }

        public static int CalculateArmorToHit(DaggerfallEntity target, int struckBodyPart)
        {
            Func<DaggerfallEntity, int, int> del;
            if (TryGetOverride("CalculateArmorToHit", out del))
                return del(target, struckBodyPart);

            int armorValue = 0;
            if (struckBodyPart <= target.ArmorValues.Length)
            {
                armorValue = target.ArmorValues[struckBodyPart] + target.IncreasedArmorValueModifier + target.DecreasedArmorValueModifier;
            }
            return armorValue;
        }

        public static int CalculateAdrenalineRushToHit(DaggerfallEntity attacker, DaggerfallEntity target)
        {
            Func<DaggerfallEntity, DaggerfallEntity, int> del;
            if (TryGetOverride("CalculateAdrenalineRushToHit", out del))
                return del(attacker, target);

            const int adrenalineRushModifier = 5;
            const int improvedAdrenalineRushModifier = 8;

            int chanceToHitMod = 0;
            if (attacker.Career.AdrenalineRush && attacker.CurrentHealth < (attacker.MaxHealth / 8))
            {
                chanceToHitMod += (attacker.ImprovedAdrenalineRush) ? improvedAdrenalineRushModifier : adrenalineRushModifier;
            }

            if (target.Career.AdrenalineRush && target.CurrentHealth < (target.MaxHealth / 8))
            {
                chanceToHitMod -= (target.ImprovedAdrenalineRush) ? improvedAdrenalineRushModifier : adrenalineRushModifier;
            }
            return chanceToHitMod;
        }

        public static int CalculateStatsToHit(DaggerfallEntity attacker, DaggerfallEntity target)
        {
            Func<DaggerfallEntity, DaggerfallEntity, int> del;
            if (TryGetOverride("CalculateStatDiffsToHit", out del))
                return del(attacker, target);

            int chanceToHitMod = 0;

            // Apply luck modifier.
            chanceToHitMod += (attacker.Stats.LiveLuck - target.Stats.LiveLuck) / 10;

            // Apply agility modifier.
            // ProjectN: doubling AGI bonus/malus here, 'cos I think it should have more importance.
            chanceToHitMod += (attacker.Stats.LiveAgility - target.Stats.LiveAgility) / 5;  // And not "/ 10" as it was before.

            return chanceToHitMod;
        }

        public static int CalculateSkillsToHit(DaggerfallEntity attacker, DaggerfallEntity target)
        {
            Func<DaggerfallEntity, DaggerfallEntity, int> del;
            if (TryGetOverride("CalculateSkillsToHit", out del))
                return del(attacker, target);

            int chanceToHitMod = 0;
            int dodgeMalus = CalculateDodgeMalus(target);

            // Apply dodging modifier.
            // This modifier is bugged in classic and the attacker's dodging skill is used rather than the target's.
            // DF Chronicles says the dodging calculation is (dodging / 10), but it actually seems to be (dodging / 4).
            // ProjectN: dodge chance is a percentage that gets multiplied by a "dodge malus", calculated on encumbrance,
            // armor type equipped and armor skill.
            Debug.Log("dodge chance: " + (target.Skills.GetLiveSkillValue(DFCareer.Skills.Dodging) * dodgeMalus / 100));
            chanceToHitMod -= target.Skills.GetLiveSkillValue(DFCareer.Skills.Dodging) * dodgeMalus / 100;

            // Apply critical strike modifier.
            // ProjectN: Critical Strike isn't a bonus to hit anymore, but modifies damage dealt.
            // if (Dice100.SuccessRoll(attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike)))
            // {
            //     chanceToHitMod += attacker.Skills.GetLiveSkillValue(DFCareer.Skills.CriticalStrike) / 10;
            // }

            return chanceToHitMod;
        }

        public static int CalculateDodgeMalus(DaggerfallEntity target)
        {
            int dodgeMalus = 100;
            int feetArmor, legsArmor, chestArmor, shield;

            dodgeMalus -= target.EncumbranceLevel * 10;

            if (!target.ItemEquipTable.IsSlotOpen(EquipSlots.Feet))
            {
                feetArmor = target.ItemEquipTable.EquipTable[(int)EquipSlots.Feet].NativeMaterialValue;
                ArmorTypes feetArmorType = ItemBuilder.GetArmorType(feetArmor);
                if (feetArmorType == ArmorTypes.Leather || feetArmorType == ArmorTypes.Fur)
                    dodgeMalus -= 5 - ((target.Skills.GetLiveSkillValue(DFCareer.Skills.LightArmor) + 10) / 25);
                else if (feetArmorType == ArmorTypes.Chain)
                    dodgeMalus -= 10 - ((target.Skills.GetLiveSkillValue(DFCareer.Skills.MediumArmor) + 10) / 25);
                else dodgeMalus -= 15;
            }
            if (!target.ItemEquipTable.IsSlotOpen(EquipSlots.LegsArmor))
            {
                legsArmor = target.ItemEquipTable.EquipTable[(int)EquipSlots.LegsArmor].NativeMaterialValue;
                ArmorTypes legsArmorType = ItemBuilder.GetArmorType(legsArmor);
                if (legsArmorType == ArmorTypes.Leather || legsArmorType == ArmorTypes.Fur)
                    dodgeMalus -= 5 - ((target.Skills.GetLiveSkillValue(DFCareer.Skills.LightArmor) + 10) / 25);
                else if (legsArmorType == ArmorTypes.Chain)
                    dodgeMalus -= 10 - ((target.Skills.GetLiveSkillValue(DFCareer.Skills.MediumArmor) + 10) / 25);
                else dodgeMalus -= 15;
            }
            if (!target.ItemEquipTable.IsSlotOpen(EquipSlots.ChestArmor))
            {
                chestArmor = target.ItemEquipTable.EquipTable[(int)EquipSlots.ChestArmor].NativeMaterialValue;
                ArmorTypes chestArmorType = ItemBuilder.GetArmorType(chestArmor);
                if (chestArmorType == ArmorTypes.Leather || chestArmorType == ArmorTypes.Fur)
                    dodgeMalus -= 5 - ((target.Skills.GetLiveSkillValue(DFCareer.Skills.LightArmor) + 10) / 25);
                else if (chestArmorType == ArmorTypes.Chain)
                    dodgeMalus -= 10 - ((target.Skills.GetLiveSkillValue(DFCareer.Skills.MediumArmor) + 10) / 25);
                else dodgeMalus -= 15;
            }
            if (!target.ItemEquipTable.IsSlotOpen(EquipSlots.LeftHand) && target.ItemEquipTable.EquipTable[(int)EquipSlots.LeftHand].IsShield)
            {
                if (target.ItemEquipTable.EquipTable[(int)EquipSlots.LeftHand].IsOfTemplate(ItemGroups.Armor, (int)Armor.Round_Shield))
                    dodgeMalus -= 5;
                else if (target.ItemEquipTable.EquipTable[(int)EquipSlots.LeftHand].IsOfTemplate(ItemGroups.Armor, (int)Armor.Kite_Shield))
                    dodgeMalus -= 10;
                else if (target.ItemEquipTable.EquipTable[(int)EquipSlots.LeftHand].IsOfTemplate(ItemGroups.Armor, (int)Armor.Tower_Shield))
                    dodgeMalus -=15;
            }
            if (dodgeMalus < 5)
                dodgeMalus = 5;

            Debug.Log("dodgeMalus: " + dodgeMalus);
            return dodgeMalus;
        }

        public static int CalculateAdjustmentsToHit(DaggerfallEntity attacker, DaggerfallEntity target)
        {
            Func<DaggerfallEntity, DaggerfallEntity, int> del;
            if (TryGetOverride("CalculateAdjustmentsToHit", out del))
                return del(attacker, target);

            PlayerEntity player = GameManager.Instance.PlayerEntity;
            EnemyEntity AITarget = target as EnemyEntity;

            int chanceToHitMod = 0;

            // Apply hit mod from character biography
            if (target == player)
            {
                chanceToHitMod -= player.BiographyAvoidHitMod;
            }

            // Apply monster modifier.
            // if ((target != player) && (AITarget.EntityType == EntityTypes.EnemyMonster))
            // {
            //     chanceToHitMod += 40;
            // }

            // DF Chronicles says -60 is applied at the end, but it actually seems to be -50.
            chanceToHitMod += 50;

            return chanceToHitMod;
        }

        #endregion

        #region Effects and Resistances

        /// <summary>
        /// Execute special monster attack effects.
        /// </summary>
        /// <param name="attacker">Attacking entity</param>
        /// <param name="target">Target entity</param>
        /// <param name="damage">Damage done by the hit</param>
        public static void OnMonsterHit(EnemyEntity attacker, DaggerfallEntity target, int damage)
        {
            Action<EnemyEntity, DaggerfallEntity, int> del;
            if (TryGetOverride("OnMonsterHit", out del))
            {
                del(attacker, target, damage);
                return;
            }

            Diseases[] diseaseListA = { Diseases.Plague };
            Diseases[] diseaseListB = { Diseases.Plague, Diseases.StomachRot, Diseases.BrainFever };
            Diseases[] diseaseListC = {
                Diseases.Plague, Diseases.YellowFever, Diseases.StomachRot, Diseases.Consumption,
                Diseases.BrainFever, Diseases.SwampRot, Diseases.Cholera, Diseases.Leprosy, Diseases.RedDeath,
                Diseases.TyphoidFever, Diseases.Dementia
            };
            float random;
            switch (attacker.CareerIndex)
            {
                case (int)MonsterCareers.Rat:
                    // In classic rat can only give plague (diseaseListA), but DF Chronicles says plague, stomach rot and brain fever (diseaseListB).
                    // Don't know which was intended. Using B since it has more variety.
                    if (Dice100.SuccessRoll(5))
                        InflictDisease(attacker, target, diseaseListB);
                    break;
                case (int)MonsterCareers.GiantBat:
                    // Classic uses 2% chance, but DF Chronicles says 5% chance. Not sure which was intended.
                    if (Dice100.SuccessRoll(2))
                        InflictDisease(attacker, target, diseaseListB);
                    break;
                case (int)MonsterCareers.Spider:
                case (int)MonsterCareers.GiantScorpion:
                    EntityEffectManager targetEffectManager = target.EntityBehaviour.GetComponent<EntityEffectManager>();
                    if (targetEffectManager.FindIncumbentEffect<Paralyze>() == null)
                    {
                        SpellRecord.SpellRecordData spellData;
                        GameManager.Instance.EntityEffectBroker.GetClassicSpellRecord(66, out spellData);
                        EffectBundleSettings bundle;
                        GameManager.Instance.EntityEffectBroker.ClassicSpellRecordDataToEffectBundleSettings(spellData, BundleTypes.Spell, out bundle);
                        EntityEffectBundle spell = new EntityEffectBundle(bundle, attacker.EntityBehaviour);
                        EntityEffectManager attackerEffectManager = attacker.EntityBehaviour.GetComponent<EntityEffectManager>();
                        attackerEffectManager.SetReadySpell(spell, true);
                    }
                    break;
                case (int)MonsterCareers.Werewolf:
                    random = UnityEngine.Random.Range(0f, 100f);
                    if (random <= specialInfectionChance && target.EntityBehaviour.EntityType == EntityTypes.Player)
                    {
                        // Werewolf
                        EntityEffectBundle bundle = GameManager.Instance.PlayerEffectManager.CreateLycanthropyDisease(LycanthropyTypes.Werewolf);
                        GameManager.Instance.PlayerEffectManager.AssignBundle(bundle, AssignBundleFlags.SpecialInfection);
                        Debug.Log("Player infected by werewolf.");
                    }
                    break;
                case (int)MonsterCareers.Nymph:
                    FatigueDamage(attacker, target, damage);
                    break;
                case (int)MonsterCareers.Wereboar:
                    random = UnityEngine.Random.Range(0f, 100f);
                    if (random <= specialInfectionChance && target.EntityBehaviour.EntityType == EntityTypes.Player)
                    {
                        // Wereboar
                        EntityEffectBundle bundle = GameManager.Instance.PlayerEffectManager.CreateLycanthropyDisease(LycanthropyTypes.Wereboar);
                        GameManager.Instance.PlayerEffectManager.AssignBundle(bundle, AssignBundleFlags.SpecialInfection);
                        Debug.Log("Player infected by wereboar.");
                    }
                    break;
                case (int)MonsterCareers.Zombie:
                    // Nothing in classic. DF Chronicles says 2% chance of disease, which seems like it was probably intended.
                    // Diseases listed in DF Chronicles match those of mummy (except missing cholera, probably a mistake)
                    if (Dice100.SuccessRoll(2))
                        InflictDisease(attacker, target, diseaseListC);
                    break;
                case (int)MonsterCareers.Mummy:
                    if (Dice100.SuccessRoll(5))
                        InflictDisease(attacker, target, diseaseListC);
                    break;
                case (int)MonsterCareers.Vampire:
                case (int)MonsterCareers.VampireAncient:
                    random = UnityEngine.Random.Range(0f, 100f);
                    if (random <= specialInfectionChance && target.EntityBehaviour.EntityType == EntityTypes.Player)
                    {
                        // Inflict stage one vampirism disease
                        EntityEffectBundle bundle = GameManager.Instance.PlayerEffectManager.CreateVampirismDisease();
                        GameManager.Instance.PlayerEffectManager.AssignBundle(bundle, AssignBundleFlags.SpecialInfection);
                        Debug.Log("Player infected by vampire.");
                    }
                    else if (random <= 2.0f)
                    {
                        InflictDisease(attacker, target, diseaseListA);
                    }
                    break;
                case (int)MonsterCareers.Lamia:
                    // Nothing in classic, but DF Chronicles says 2 pts of fatigue damage per health damage
                    FatigueDamage(attacker, target, damage);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Inflict a classic poison onto entity.
        /// </summary>
        /// <param name="attacker">Source entity. Can be the same as target</param>
        /// <param name="target">Target entity</param>
        /// <param name="poisonType">Classic poison type</param>
        /// <param name="bypassResistance">Whether it should bypass resistances</param>
        public static void InflictPoison(DaggerfallEntity attacker, DaggerfallEntity target, Poisons poisonType, bool bypassResistance)
        {
            Action<DaggerfallEntity, DaggerfallEntity, Poisons, bool> del;
            if(TryGetOverride("InflictPoison", out del))
            {
                del(attacker, target, poisonType, bypassResistance);
                return;
            }

            // Target must have an entity behaviour and effect manager
            EntityEffectManager effectManager = null;
            if (target.EntityBehaviour != null)
            {
                effectManager = target.EntityBehaviour.GetComponent<EntityEffectManager>();
                if (effectManager == null)
                    return;
            }
            else
            {
                return;
            }

            // Note: In classic, AI characters' immunity to poison is ignored, although the level 1 check below still gives rats immunity
            DFCareer.Tolerance toleranceFlags = target.Career.Poison;
            if (toleranceFlags == DFCareer.Tolerance.Immune)
                return;

            // Handle player with racial resistance to poison
            if (target is PlayerEntity)
            {
                RaceTemplate raceTemplate = (target as PlayerEntity).GetLiveRaceTemplate();
                if ((raceTemplate.ImmunityFlags & DFCareer.EffectFlags.Poison) == DFCareer.EffectFlags.Poison)
                    return;
            }

            if (bypassResistance || SavingThrow(DFCareer.Elements.DiseaseOrPoison, DFCareer.EffectFlags.Poison, target, 0) != 0)
            {
                // ProjectN: removing this limit because doesn't make much sense.
                // if (target.Level != 1)
                // {
                    // Infect target
                    EntityEffectBundle bundle = effectManager.CreatePoison(poisonType);
                    effectManager.AssignBundle(bundle, AssignBundleFlags.BypassSavingThrows);
                // }
            }
            else
            {
                Debug.LogFormat("Poison resisted by {0}.", target.EntityBehaviour.name);
            }
        }

        public static DFCareer.ToleranceFlags GetToleranceFlag(DFCareer.Tolerance tolerance)
        {
            DFCareer.ToleranceFlags flag = DFCareer.ToleranceFlags.Normal;
            switch (tolerance)
            {
                case DFCareer.Tolerance.Immune:
                    flag = DFCareer.ToleranceFlags.Immune;
                    break;
                case DFCareer.Tolerance.Resistant:
                    flag = DFCareer.ToleranceFlags.Resistant;
                    break;
                case DFCareer.Tolerance.LowTolerance:
                    flag = DFCareer.ToleranceFlags.LowTolerance;
                    break;
                case DFCareer.Tolerance.CriticalWeakness:
                    flag = DFCareer.ToleranceFlags.CriticalWeakness;
                    break;
            }

            return flag;
        }

        public static int SavingThrow(DFCareer.Elements elementType, DFCareer.EffectFlags effectFlags, DaggerfallEntity target, int modifier)
        {
            Func<DFCareer.Elements, DFCareer.EffectFlags, DaggerfallEntity, int, int> del;
            if (TryGetOverride("SavingThrow", out del))
                return del(elementType, effectFlags, target, modifier);

            // Handle resistances granted by magical effects
            if (target.HasResistanceFlag(elementType))
            {
                int chance = target.GetResistanceChance(elementType);
                if (Dice100.SuccessRoll(chance))
                    return 0;
            }

            // Magic effect resistances did not stop the effect. Try with career flags and biography modifiers
            int savingThrow = 50;
            DFCareer.ToleranceFlags toleranceFlags = DFCareer.ToleranceFlags.Normal;
            int biographyMod = 0;

            PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
            if ((effectFlags & DFCareer.EffectFlags.Paralysis) != 0)
            {
                toleranceFlags |= GetToleranceFlag(target.Career.Paralysis);
                // Innate immunity if high elf. Start with 100 saving throw, but can be modified by
                // tolerance flags. Note this differs from classic, where high elves have 100% immunity
                // regardless of tolerance flags.
                if (target == playerEntity && playerEntity.Race == Races.HighElf)
                    savingThrow = 100;
            }
            if ((effectFlags & DFCareer.EffectFlags.Magic) != 0)
            {
                toleranceFlags |= GetToleranceFlag(target.Career.Magic);
                if (target == playerEntity)
                    biographyMod += playerEntity.BiographyResistMagicMod;
            }
            if ((effectFlags & DFCareer.EffectFlags.Poison) != 0)
            {
                toleranceFlags |= GetToleranceFlag(target.Career.Poison);
                if (target == playerEntity)
                    biographyMod += playerEntity.BiographyResistPoisonMod;
            }
            if ((effectFlags & DFCareer.EffectFlags.Fire) != 0)
                toleranceFlags |= GetToleranceFlag(target.Career.Fire);
            if ((effectFlags & DFCareer.EffectFlags.Frost) != 0)
                toleranceFlags |= GetToleranceFlag(target.Career.Frost);
            if ((effectFlags & DFCareer.EffectFlags.Shock) != 0)
                toleranceFlags |= GetToleranceFlag(target.Career.Shock);
            if ((effectFlags & DFCareer.EffectFlags.Disease) != 0)
            {
                toleranceFlags |= GetToleranceFlag(target.Career.Disease);
                if (target == playerEntity)
                    biographyMod += playerEntity.BiographyResistDiseaseMod;
            }

            // Note: Differing from classic implementation here. In classic
            // immune grants always 100% resistance and critical weakness is
            // always 0% resistance if there is no immunity. Here we are using
            // a method that allows mixing different tolerance flags, getting
            // rid of related exploits when creating a character class.
            if ((toleranceFlags & DFCareer.ToleranceFlags.Immune) != 0)
                savingThrow += 50;
            if ((toleranceFlags & DFCareer.ToleranceFlags.CriticalWeakness) != 0)
                savingThrow -= 50;
            if ((toleranceFlags & DFCareer.ToleranceFlags.LowTolerance) != 0)
                savingThrow -= 25;
            if ((toleranceFlags & DFCareer.ToleranceFlags.Resistant) != 0)
                savingThrow += 25;

            savingThrow += biographyMod + modifier;
            if (elementType == DFCareer.Elements.Frost && target == playerEntity && playerEntity.Race == Races.Nord)
                savingThrow += 30;
            else if (elementType == DFCareer.Elements.Magic && target == playerEntity && playerEntity.Race == Races.Breton)
                savingThrow += 30;

            // Handle perfect immunity of 100% or greater
            // Otherwise clamping to 5-95 allows a perfectly immune character to sometimes receive incoming payload
            // This doesn't seem to match immunity intent or player expectations from classic
            if (savingThrow >= 100)
                return 0;

            // Increase saving throw by MagicResist, equal to LiveWillpower / 10 (rounded down)
            savingThrow += target.MagicResist;

            savingThrow = Mathf.Clamp(savingThrow, 5, 95);

            int percentDamageOrDuration = 100;
            int roll = Dice100.Roll();

            if (roll <= savingThrow)
            {
                // Percent damage/duration is prorated at within 20 of failed roll, as described in DF Chronicles
                if (savingThrow - 20 <= roll)
                    percentDamageOrDuration = 100 - 5 * (savingThrow - roll);
                else
                    percentDamageOrDuration = 0;
            }

            return Mathf.Clamp(percentDamageOrDuration, 0, 100);
        }

        public static int SavingThrow(IEntityEffect sourceEffect, DaggerfallEntity target)
        {
            Func<IEntityEffect, DaggerfallEntity, int> del;
            if (TryGetOverride("SavingThrowSpellEffect", out del))
                return del(sourceEffect, target);

            if (sourceEffect == null || sourceEffect.ParentBundle == null)
                return 100;

            DFCareer.EffectFlags effectFlags = GetEffectFlags(sourceEffect);
            DFCareer.Elements elementType = GetElementType(sourceEffect);
            int modifier = GetResistanceModifier(effectFlags, target);

            return SavingThrow(elementType, effectFlags, target, modifier);
        }

        public static int ModifyEffectAmount(IEntityEffect sourceEffect, DaggerfallEntity target, int amount)
        {
            if (sourceEffect == null || sourceEffect.ParentBundle == null)
                return amount;

            int percentDamageOrDuration = SavingThrow(sourceEffect, target);
            float percent = percentDamageOrDuration / 100f;

            return (int)(amount * percent);
        }

        /// <summary>
        /// Gets DFCareer.EffectFlags from an effect.
        /// Note: If effect is not instanced by a bundle then it will not have an element type.
        /// </summary>
        /// <param name="effect">Source effect.</param>
        /// <returns>DFCareer.EffectFlags.</returns>
        public static DFCareer.EffectFlags GetEffectFlags(IEntityEffect effect)
        {
            DFCareer.EffectFlags result = DFCareer.EffectFlags.None;

            // Paralysis/Disease
            if (effect is Paralyze)
                result |= DFCareer.EffectFlags.Paralysis;
            if (effect is DiseaseEffect)
                result |= DFCareer.EffectFlags.Disease;

            // Elemental
            switch (effect.ParentBundle.elementType)
            {
                case ElementTypes.Fire:
                    result |= DFCareer.EffectFlags.Fire;
                    break;
                case ElementTypes.Cold:
                    result |= DFCareer.EffectFlags.Frost;
                    break;
                case ElementTypes.Poison:
                    result |= DFCareer.EffectFlags.Poison;
                    break;
                case ElementTypes.Shock:
                    result |= DFCareer.EffectFlags.Shock;
                    break;
                case ElementTypes.Magic:
                    result |= DFCareer.EffectFlags.Magic;
                    break;
            }

            return result;
        }

        /// <summary>
        /// Gets a resistance element based on effect element.
        /// </summary>
        /// <param name="effect">Source effect.</param>
        /// <returns>DFCareer.Elements</returns>
        public static DFCareer.Elements GetElementType(IEntityEffect effect)
        {
            // Always return magic for non-elemental (i.e. magic-only) effects
            if (effect.Properties.AllowedElements == ElementTypes.Magic)
                return DFCareer.Elements.Magic;

            // Otherwise return element selected by parent spell bundle
            switch (effect.ParentBundle.elementType)
            {
                case ElementTypes.Fire:
                    return DFCareer.Elements.Fire;
                case ElementTypes.Cold:
                    return DFCareer.Elements.Frost;
                case ElementTypes.Poison:
                    return DFCareer.Elements.DiseaseOrPoison;
                case ElementTypes.Shock:
                    return DFCareer.Elements.Shock;
                case ElementTypes.Magic:
                    return DFCareer.Elements.Magic;
                default:
                    return DFCareer.Elements.None;
            }
        }

        public static int GetResistanceModifier(DFCareer.EffectFlags effectFlags, DaggerfallEntity target)
        {
            int result = 0;

            // Will only read best matching resistance modifier from flags - priority is given to disease/poison over elemental
            // Note disease/poison resistance are both the same here for purposes of saving throw
            if ((effectFlags & DFCareer.EffectFlags.Disease) == DFCareer.EffectFlags.Disease || (effectFlags & DFCareer.EffectFlags.Poison) == DFCareer.EffectFlags.Poison)
                result = target.Resistances.LiveDiseaseOrPoison;
            else if ((effectFlags & DFCareer.EffectFlags.Fire) == DFCareer.EffectFlags.Fire)
                result = target.Resistances.LiveFire;
            else if ((effectFlags & DFCareer.EffectFlags.Frost) == DFCareer.EffectFlags.Frost)
                result = target.Resistances.LiveFrost;
            else if ((effectFlags & DFCareer.EffectFlags.Shock) == DFCareer.EffectFlags.Shock)
                result = target.Resistances.LiveShock;
            else if ((effectFlags & DFCareer.EffectFlags.Magic) == DFCareer.EffectFlags.Magic)
                result = target.Resistances.LiveMagic;

            return result;
        }

        /// <summary>
        /// Inflict a classic disease onto player.
        /// </summary>
        /// <param name="attacker">Source entity. Can be the same as target</param>
        /// <param name="target">Target entity - must be player.</param>
        /// <param name="diseaseList">Array of disease indices matching Diseases enum.</param>
        public static void InflictDisease(DaggerfallEntity attacker, DaggerfallEntity target, Diseases[] diseaseList)
        {
            Action<DaggerfallEntity, DaggerfallEntity, Diseases[]> del;
            if (TryGetOverride("InflictDisease", out del))
            {
                del(attacker, target, diseaseList);
                return;
            }

            // Must have a valid disease list
            if (diseaseList == null || diseaseList.Length == 0 || target.EntityBehaviour.EntityType != EntityTypes.Player)
                return;

            // Only allow player to catch a disease this way
            PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
            if (target != playerEntity)
                return;

            // Player cannot catch diseases at level 1 in classic. Maybe to keep new players from dying at the start of the game.
            // ProjectN: removing this limit because doesn't make much sense.
            // if (playerEntity.Level != 1)
            // {
                // Return if disease resisted
                if (SavingThrow(DFCareer.Elements.DiseaseOrPoison, DFCareer.EffectFlags.Disease, target, 0) == 0)
                    return;

                // Select a random disease from disease array and validate range
                int diseaseIndex = UnityEngine.Random.Range(0, diseaseList.Length);

                // Infect player
                Diseases diseaseType = diseaseList[diseaseIndex];
                EntityEffectBundle bundle = GameManager.Instance.PlayerEffectManager.CreateDisease(diseaseType);
                GameManager.Instance.PlayerEffectManager.AssignBundle(bundle, AssignBundleFlags.BypassSavingThrows);

                Debug.LogFormat("Infected player with disease {0}", diseaseType.ToString());
            // }
        }

        public static void FatigueDamage(EnemyEntity attacker, DaggerfallEntity target, int damage)
        {
            Action<EnemyEntity, DaggerfallEntity, int> del;
            if (TryGetOverride("FatigueDamage", out del))
            {
                del(attacker, target, damage);
                return;
            }

            // In classic, nymphs do 10-30 fatigue damage per hit, and lamias don't do any.
            // DF Chronicles says nymphs have "Energy Leech", which is a spell in
            // the game and not what they use, and for lamias "Every 1 pt of health damage = 2 pts of fatigue damage".
            // Lamia health damage is 5-15. Multiplying this by 2 may be where 10-30 came from. Nymph health damage is 1-5.
            // Not sure what was intended here, but using the "Every 1 pt of health damage = 2 pts of fatigue damage"
            // seems sensible, since the fatigue damage will scale to the health damage and lamias are a higher level opponent
            // than nymphs and will do more fatigue damage.
            target.SetFatigue(target.CurrentFatigue - ((damage * 2) * 64));

            // TODO: When nymphs drain the player's fatigue level to 0, the player is supposed to fall asleep for 14 days
            // and then wake up, according to DF Chronicles. This doesn't work correctly in classic. Classic does advance
            // time 14 days but the player dies like normal because of the "collapse from exhaustion near monsters = die" code.
        }

        #endregion

        #region Enemies

        // Generates health for enemy classes based on level and class
        public static int RollEnemyClassMaxHealth(int level, int hitPointsPerLevel)
        {
            Func<int, int, int> del;
            if (TryGetOverride("RollEnemyClassMaxHealth", out del))
                return del(level, hitPointsPerLevel);

            const int baseHealth = 10;
            int maxHealth = baseHealth;

            for (int i = 0; i < level; i++)
            {
                maxHealth += UnityEngine.Random.Range(1, hitPointsPerLevel + 1);
            }
            return maxHealth;
        }

        /// <summary>
        /// Roll for random spawn in location area at night.
        /// </summary>
        /// <returns>0 to generate a spawn. >0 to not generate a spawn.</returns>
        public static int RollRandomSpawn_LocationNight()
        {
            Func<int> del;
            if (TryGetOverride("RollRandomSpawn_LocationNight", out del))
                return del();
            else
                return UnityEngine.Random.Range(0, 24);
        }

        /// <summary>
        /// Roll for random spawn in wilderness during daylight hours.
        /// </summary>
        /// <returns>0 to generate a spawn. >0 to not generate a spawn.</returns>
        public static int RollRandomSpawn_WildernessDay()
        {
            Func<int> del;
            if (TryGetOverride("RollRandomSpawn_WildernessDay", out del))
                return del();
            else
                return UnityEngine.Random.Range(0, 36);
        }

        /// <summary>
        /// Roll for random spawn in wilderness at night.
        /// </summary>
        /// <returns>0 to generate a spawn. >0 to not generate a spawn.</returns>
        public static int RollRandomSpawn_WildernessNight()
        {
            Func<int> del;
            if (TryGetOverride("RollRandomSpawn_WildernessNight", out del))
                return del();
            else
                return UnityEngine.Random.Range(0, 24);
        }

        /// <summary>
        /// Roll for random spawn in dungeons.
        /// </summary>
        /// <returns>0 to generate a spawn. >0 to not generate a spawn.</returns>
        public static int RollRandomSpawn_Dungeon()
        {
            Func<int> del;
            if (TryGetOverride("RollRandomSpawn_Dungeon", out del))
                return del();
            else if (GameManager.Instance.PlayerEntity.EnemyAlertActive)
                return UnityEngine.Random.Range(0, 36);

            return 1; // >0 is do not generate a spawn
        }

        #endregion

        #region Holidays & Conversation

        public static int GetHolidayId(uint gameMinutes, int regionIndex)
        {
            // Gives which regions celebrate which holidays.
            // Values are region IDs, index is holiday ID. 0xFF means all regions celebrate the holiday.
            byte[] regionIndexCelebratingHoliday = { 0xFF, 0x19, 0x01, 0xFF, 0x1D, 0x05, 0x19, 0x06, 0x3C, 0xFF, 0x29, 0x1A,
                0xFF, 0x02, 0x19, 0x01, 0x0E, 0x12, 0x14, 0xFF, 0xFF, 0x1C, 0x21, 0x1F, 0x2C, 0xFF, 0x12,
                0x23, 0xFF, 0x38, 0xFF, 0x01, 0x30, 0x29, 0x0B, 0x16, 0xFF, 0xFF, 0x11, 0x17, 0x14, 0x01,
                0xFF, 0x13, 0xFF, 0x33, 0x3C, 0x2E, 0xFF, 0xFF, 0x01, 0x2D, 0x18 };

            // Gives the day of the year that holidays are celebrated on.
            // Value are days of the year, index is holiday ID.
            short[] holidayDaysOfYear = { 0x01, 0x02, 0x0C, 0x0F, 0x10, 0x12, 0x20, 0x23, 0x26, 0x2E, 0x39, 0x3A,
                0x43, 0x45, 0x55, 0x56, 0x5B, 0x67, 0x6E, 0x76, 0x7F, 0x81, 0x8C, 0x96, 0x97, 0xA6, 0xAD,
                0xAE, 0xBE, 0xC0, 0xC8, 0xD1, 0xD4, 0xDD, 0xE0, 0xE7, 0xED, 0xF3, 0xF6, 0xFC, 0x103, 0x113,
                0x11B, 0x125, 0x12C, 0x12F, 0x134, 0x13E, 0x140, 0x159, 0x15C, 0x162, 0x163 };

            int holidayID = 0;
            uint dayOfYear = gameMinutes % 518400 / 1440 + 1;
            if (dayOfYear <= 355)
            {
                while (holidayID < 53)
                {
                    if ((regionIndexCelebratingHoliday[holidayID] == 0xFF || regionIndexCelebratingHoliday[holidayID] == regionIndex + 1)
                        && dayOfYear == holidayDaysOfYear[holidayID])
                    {
                        return holidayID + 1;
                    }
                    ++holidayID;
                }
            }

            // Not a holiday
            return 0;
        }

        #endregion

        #region Commerce

        public static int CalculateRoomCost(int daysToRent)
        {
            Func<int, int> del;
            if (TryGetOverride("CalculateRoomCost", out del))
                return del(daysToRent);

            int cost = 0;
            int dayOfYear = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.DayOfYear;
            if (dayOfYear <= 46 && dayOfYear + daysToRent > 46)
                cost = 7 * (daysToRent - 1);  // No charge for Heart's Day
            else
                cost = 7 * daysToRent;

            if (cost == 0) // Only renting for Heart's Day
                DaggerfallUI.MessageBox(TextManager.Instance.GetLocalizedText("roomFreeDueToHeartsDay"));

            return cost;
        }

        /// <summary>
        /// Calculate the cost of something in a given shop.
        /// </summary>
        /// <param name="baseValue">Base value</param>
        /// <param name="shopQuality">Shop quality 0-20</param>
        /// <param name="conditionPercentage">Condition of item as a percentage, -1 indicates condition not applicable</param>
        /// <returns>Shop specific cost</returns>
        public static int CalculateCost(int baseValue, int shopQuality, int conditionPercentage = -1)
        {
            float conditionMod = (conditionPercentage == -1) ? 1f : Mathf.Max((float)conditionPercentage / 100, 0.2f);
            int cost = (int)(baseValue * conditionMod);

            if (cost < 1)
                cost = 1;

            cost = ApplyRegionalPriceAdjustment(cost);
            cost = 2 * (cost * (shopQuality - 10) / 100 + cost);

            return cost;
        }

        // ProjectN: applying R&R:I formula
        public static int CalculateItemRepairCost(int baseItemValue, int shopQuality, int condition, int max, IGuild guild)
        {
            Func<int, int, int, int, IGuild, int> del;
            if (TryGetOverride("CalculateItemRepairCost", out del))
                return del(baseItemValue, shopQuality, condition, max, guild);

            // Don't cost already repaired item
            if (condition == max)
                return 0;

            float repairCostScaleFactor = 0.6f;
            float conditionFactor = repairCostScaleFactor * (max - condition) / max;
            int cost = Mathf.Max((int)(baseItemValue * conditionFactor), 1);

            cost = CalculateCost(cost, shopQuality);

            // int cost = 10 * baseItemValue / 100;

            // if (cost < 1)
            //     cost = 1;

            if (guild != null)
                cost = guild.ReducedRepairCost(cost);

            return cost;
        }

        public static int CalculateItemRepairTime(int condition, int max)
        {
            Func<int, int, int> del;
            if (TryGetOverride("CalculateItemRepairTime", out del))
                return del(condition, max);

            int damage = max - condition;
            int repairTime = (damage * DaggerfallDateTime.SecondsPerDay / 1000);
            return Mathf.Max(repairTime, DaggerfallDateTime.SecondsPerDay);
        }

        public static int CalculateItemIdentifyCost(int baseItemValue, IGuild guild)
        {
            Func<int, IGuild, int> del;
            if (TryGetOverride("CalculateItemIdentifyCost", out del))
                return del(baseItemValue, guild);

            // Free on Witches Festival
            uint minutes = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();
            PlayerGPS gps = GameManager.Instance.PlayerGPS;
            if (gps.HasCurrentLocation)
            {
                int holidayId = GetHolidayId(minutes, gps.CurrentRegionIndex);
                if (holidayId == (int)DFLocation.Holidays.Witches_Festival)
                    return 0;
            }
            int cost = (25 * baseItemValue) >> 8;

            if (guild != null)
                cost = guild.ReducedIdentifyCost(cost);

            return cost;
        }

        public static int CalculateDaedraSummoningCost(int npcRep)
        {
            Func<int, int> del;
            if (TryGetOverride("CalculateDaedraSummoningCost", out del))
                return del(npcRep);

            return 200000 - (npcRep * 1000);
        }

        public static int CalculateDaedraSummoningChance(int daedraRep, int bonus)
        {
            Func<int, int, int> del;
            if (TryGetOverride("CalculateDaedraSummoningChance", out del))
                return del(daedraRep, bonus);

            return 30 + daedraRep + bonus;
        }

        public static int CalculateTradePrice(int cost, int shopQuality, bool selling)
        {
            Func<int, int, bool, int> del;
            if (TryGetOverride("CalculateTradePrice", out del))
                return del(cost, shopQuality, selling);

            PlayerEntity player = GameManager.Instance.PlayerEntity;
            int merchant_mercantile_level = 5 * (shopQuality - 10) + 50;
            int merchant_personality_level = 5 * (shopQuality - 10) + 50;

            int delta_mercantile;
            int delta_personality;
            int amount = 0;

            if (selling)
            {
                delta_mercantile = (((100 - merchant_mercantile_level) << 8) / 200 + 128) * (((player.Skills.GetLiveSkillValue(DFCareer.Skills.Mercantile)) << 8) / 200 + 128) >> 8;
                delta_personality = (((100 - merchant_personality_level) << 8) / 200 + 128) * ((player.Stats.LivePersonality << 8) / 200 + 128) >> 8;
                amount = ((((179 * delta_mercantile) >> 8) + ((51 * delta_personality) >> 8)) * cost) >> 8;
            }
            else // buying
            {
                delta_mercantile = ((merchant_mercantile_level << 8) / 200 + 128) * (((100 - (player.Skills.GetLiveSkillValue(DFCareer.Skills.Mercantile))) << 8) / 200 + 128) >> 8;
                delta_personality = ((merchant_personality_level << 8) / 200 + 128) * (((100 - player.Stats.LivePersonality) << 8) / 200 + 128) >> 8 << 6;
                amount = ((((192 * delta_mercantile) >> 8) + (delta_personality >> 8)) * cost) >> 8;
            }

            return amount;
        }

        public static int CalculateMaxBankLoan()
        {
            Func<int> del;
            if (TryGetOverride("CalculateMaxBankLoan", out del))
                return del();

            //unoffical wiki says max possible loan is 1,100,000 but testing indicates otherwise
            //rep. doesn't seem to effect cap, it's just level * 50k
            // ProjectN TODO: make legal/bankers/whatever reputation influence this
            return GameManager.Instance.PlayerEntity.Level * DaggerfallBankManager.loanMaxPerLevel;
        }

        public static int CalculateBankLoanRepayment(int amount, int regionIndex)
        {
            Func<int, int, int> del;
            if (TryGetOverride("CalculateBankLoanRepayment", out del))
                return del(amount, regionIndex);

            return (int)(amount + amount * .1);
        }

        public static int ApplyRegionalPriceAdjustment(int cost)
        {
            Func<int, int> del;
            if (TryGetOverride("ApplyRegionalPriceAdjustment", out del))
                return del(cost);

            int adjustedCost;
            int currentRegionIndex;
            PlayerEntity player = GameManager.Instance.PlayerEntity;
            PlayerGPS gps = GameManager.Instance.PlayerGPS;
            if (gps.HasCurrentLocation)
                currentRegionIndex = gps.CurrentRegionIndex;
            else
                return cost;

            adjustedCost = cost * player.RegionData[currentRegionIndex].PriceAdjustment / 1000;
            if (adjustedCost < 1)
                adjustedCost = 1;
            return adjustedCost;
        }

        public static void RandomizeInitialRegionalPrices(ref PlayerEntity.RegionDataRecord[] regionData)
        {
            for (int i = 0; i < regionData.Length; i++)
                regionData[i].PriceAdjustment = (ushort)(UnityEngine.Random.Range(0, 500 + 1) + 750);
        }

        public static void UpdateRegionalPrices(ref PlayerEntity.RegionDataRecord[] regionData, int times)
        {
            PlayerEntity player = GameManager.Instance.PlayerEntity;
            FactionFile.FactionData merchantsFaction;
            if (!player.FactionData.GetFactionData((int)FactionFile.FactionIDs.The_Merchants, out merchantsFaction))
                return;

            for (int i = 0; i < regionData.Length; ++i)
            {
                FactionFile.FactionData regionFaction;
                if (player.FactionData.FindFactionByTypeAndRegion(7, i, out regionFaction))
                {
                    for (int j = 0; j < times; ++j)
                    {
                        int chanceOfPriceRise = ((merchantsFaction.power) - (regionFaction.power)) / 5
                            + 50 - (regionData[i].PriceAdjustment - 1000) / 25;
                        if (Dice100.FailedRoll(chanceOfPriceRise))
                            regionData[i].PriceAdjustment = (ushort)(49 * regionData[i].PriceAdjustment / 50);
                        else
                            regionData[i].PriceAdjustment = (ushort)(51 * regionData[i].PriceAdjustment / 50);

                        regionData[i].PriceAdjustment = (ushort)Mathf.Clamp(regionData[i].PriceAdjustment, 250, 4000);
                        if (regionData[i].PriceAdjustment <= 2000)
                        {
                            if (regionData[i].PriceAdjustment >= 500)
                            {
                                player.TurnOffConditionFlag(i, PlayerEntity.RegionDataFlags.PricesHigh);
                                player.TurnOffConditionFlag(i, PlayerEntity.RegionDataFlags.PricesLow);
                            }
                            else
                                player.TurnOnConditionFlag(i, PlayerEntity.RegionDataFlags.PricesLow);
                        }
                        else
                            player.TurnOnConditionFlag(i, PlayerEntity.RegionDataFlags.PricesHigh);
                    }
                }
            }
        }

        #endregion

        #region Items

        public static bool IsItemStackable(DaggerfallUnityItem item)
        {
            Func<DaggerfallUnityItem, bool> del;
            if (TryGetOverride("IsItemStackable", out del))
                if (del(item))
                    return true; // Only return if override returns true

            if (item.IsIngredient || item.IsPotion || (item.ItemGroup == ItemGroups.Books) ||
                item.IsOfTemplate(ItemGroups.Currency, (int)Currency.Gold_pieces) ||
                item.IsOfTemplate(ItemGroups.Weapons, (int)Weapons.Arrow) ||
                item.IsOfTemplate(ItemGroups.UselessItems2, (int)UselessItems2.Bandage) ||
                item.IsOfTemplate(ItemGroups.UselessItems2, (int)UselessItems2.Oil))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Gets a random material based on player level.
        /// Note, this is called by default RandomArmorMaterial function.
        /// </summary>
        /// <param name="isTownGuard">Town guards don't get material above steel.</param>
        /// <param name="luckMod">Player's luck when loot or shops, enemy's luck when it's its equipment.</param>
        /// <returns>MaterialTypes value of material selected.</returns>
        // ProjectN: making the material randomisation unleveled.
        public static MaterialTypes RandomMaterial(int luck, bool isTownGuard = false, int region = -1, GovernmentType government = GovernmentType.None, DFRegion.LocationTypes location = DFRegion.LocationTypes.None, DFRegion.DungeonTypes dungeon = DFRegion.DungeonTypes.NoDungeon)
        {
            int luckMod = (luck - 50) / 5;
            int matRoll = UnityEngine.Random.Range(0, rareMaterial);
            int matRollModified = matRoll + luckMod;

            PlayerGPS playerGPS = GameManager.Instance.PlayerGPS;
            PlayerEnterExit playerEnterExit = GameManager.Instance.PlayerEnterExit;
            if (region == -1)
                region = playerGPS.CurrentRegionIndex;
            government = TextManager.Instance.GetCurrentRegionGovernment(region);
            if (location == DFRegion.LocationTypes.None)
                location = playerGPS.CurrentLocationType;
            if (playerEnterExit.IsPlayerInsideDungeon)
                dungeon = playerGPS.CurrentLocation.MapTableData.DungeonType;

            matRollModified += GovernmentModifier(government);
            matRollModified += LocationModifier(location);
            matRollModified += DungeonModifier(dungeon);

            if (playerEnterExit.IsPlayerInsideOpenShop ||
               (playerEnterExit.IsPlayerInsideClosedShop))
            {
                matRollModified += playerEnterExit.Interior.BuildingData.Quality / 4;
            }
            
            MaterialTypes material;
            if (matRoll == (rareMaterial - 1)) material = GetRareMaterial(luckMod);
            else material = GetMaterialType(matRollModified, luckMod);

            if (isTownGuard && material > MaterialTypes.Steel)
                material = MaterialTypes.Steel;

            Debug.Log("Material randomised: " + material);
            return material;
        }

        public static MaterialTypes GetMaterialType(int matRollModified, int luckMod)
        {
            MaterialTypes material = MaterialTypes.Iron; // initialize to iron

            if (matRollModified >= rareMaterial)
                material = GetRareMaterial(luckMod);
            else if (matRollModified >= (rareMaterial - dwarvenChance))
                material = MaterialTypes.Dwarven;
            else if (matRollModified >= (rareMaterial - glassChance))
                material = MaterialTypes.Glass;
            else if (matRollModified >= (rareMaterial - elvenChance))
                material = MaterialTypes.Elven;
            else if (matRollModified >= (rareMaterial - silverChance))
                material = MaterialTypes.Silver;
            else if (matRollModified >= (rareMaterial - steelChance))
                material = MaterialTypes.Steel;

            return material;
        }

        public static int GovernmentModifier(GovernmentType government)
        {
            switch (government)
            {
                case GovernmentType.Fiefdom:
                    return 1;
                case GovernmentType.Barony:
                    return 2;
                case GovernmentType.County:
                    return 3;
                case GovernmentType.March:
                    return 4;
                case GovernmentType.Duchy:
                    return 5;
                case GovernmentType.Kingdom:
                    return 6;
                case GovernmentType.Empire:
                    return 7;
                default:
                    return 0;
            }
        }

            public static int LocationModifier(DFRegion.LocationTypes location)
            {
                switch (location)
                {
                    case DFRegion.LocationTypes.TownCity:
                    case DFRegion.LocationTypes.DungeonRuin:
                        return 5;
                    case DFRegion.LocationTypes.TownHamlet:
                    case DFRegion.LocationTypes.HomeWealthy:
                    case DFRegion.LocationTypes.Graveyard:
                    case DFRegion.LocationTypes.HiddenLocation:
                        return 3;
                    case DFRegion.LocationTypes.TownVillage:
                    case DFRegion.LocationTypes.ReligionCult:
                        return 1;
                    case DFRegion.LocationTypes.HomeFarms:
                    case DFRegion.LocationTypes.ReligionTemple:
                    case DFRegion.LocationTypes.Tavern:
                    case DFRegion.LocationTypes.HomePoor:
                        return 0;
                    case DFRegion.LocationTypes.DungeonLabyrinth:
                        return 10;
                    case DFRegion.LocationTypes.DungeonKeep:
                    case DFRegion.LocationTypes.Coven:
                        return 8;
                    default:
                        return 0;
                }
            }

            public static int DungeonModifier(DFRegion.DungeonTypes dungeon)
            {
                switch (dungeon)
                {
                    case DFRegion.DungeonTypes.NaturalCave:
                    case DFRegion.DungeonTypes.Cemetery:
                    case DFRegion.DungeonTypes.RuinedCastle:
                        return 0;                
                    case DFRegion.DungeonTypes.SpiderNest:
                    case DFRegion.DungeonTypes.ScorpionNest:
                    case DFRegion.DungeonTypes.Prison:
                    case DFRegion.DungeonTypes.Mine:
                        return 3;
                    case DFRegion.DungeonTypes.BarbarianStronghold:
                    case DFRegion.DungeonTypes.OrcStronghold:
                    case DFRegion.DungeonTypes.HumanStronghold:
                        return 6;
                    case DFRegion.DungeonTypes.Crypt:
                    case DFRegion.DungeonTypes.GiantStronghold:
                    case DFRegion.DungeonTypes.VampireHaunt:
                    case DFRegion.DungeonTypes.Laboratory:
                        return 9;
                    case DFRegion.DungeonTypes.DragonsDen:
                    case DFRegion.DungeonTypes.Coven:
                    case DFRegion.DungeonTypes.DesecratedTemple:
                    case DFRegion.DungeonTypes.VolcanicCaves:
                        return 12;
                    default:
                        return 0;
                }
            }

        public static MaterialTypes GetRareMaterial(int luck)
        {
            int matRoll = UnityEngine.Random.Range(0, rareMaterial);

            if (matRoll == (rareMaterial - 1) || matRoll + luck >= rareMaterial)
                return MaterialTypes.Daedric;
            else if (matRoll + luck >= (rareMaterial - ebonyChance))
                return MaterialTypes.Ebony;
            else if (matRoll + luck >= (rareMaterial - adamantiumChance))
                return MaterialTypes.Adamantium;
            else if (matRoll % 2 == 0)
                return MaterialTypes.Mithril;
            else return MaterialTypes.Orcish;            
        }

        /// <summary>
        /// Gets a random armor material based on player level.
        /// </summary>
        /// <returns>ArmorMaterialTypes value of material selected.</returns>
        // ProjectN: player level is no longer a factor in material randomisation.
        public static ArmorMaterialTypes RandomArmorMaterial(Armor armor, int luck, bool isTownGuard = false)
        {
            // Random armor material
            int roll = Dice100.Roll();
            MaterialTypes armorMaterial = FormulaHelper.RandomMaterial(luck);

            // TODO: I need to write some formula for Armor (and item in general) that have to be harder to find
            // in certain areas of the world. Fur is one of those: it should be easier to find in cold areas, while
            // very scarce in warm and hot climates. For now I just let it random.
            if (armor >= Armor.Cuirass && armor <= Armor.Tower_Shield)
            {
                if (roll >= 20)
                {
                    if (roll >= 40)
                    {
                        return (ArmorMaterialTypes)(0x0200 + armorMaterial);
                    }
                    else
                    {
                        return ArmorMaterialTypes.Chain;
                    }
                }
                else
                {
                    return ArmorMaterialTypes.Leather;
                }
            }
            else if (armor >= Armor.Hauberk && armor <= Armor.Sollerets)
            {
                if (roll >= 20)
                    return (ArmorMaterialTypes)(0x0100 + armorMaterial);
                else return ArmorMaterialTypes.Chain;
            }
            else // if (armorValue >= Armor.Jerkin && armorValue <= Armor.RightVambrace)
            {
                if (roll >= 40)
                    return (ArmorMaterialTypes)(0x0000 + armorMaterial);
                else if (roll >= 20)
                    return ArmorMaterialTypes.Fur;
                else return ArmorMaterialTypes.Leather;
            }
        }

        #endregion

        #region Spell Costs

        /// <summary>
        /// A structure containing both the gold and spell point cost of either a single effect, or an entire spell
        /// </summary>
        public struct SpellCost
        {
            public int goldCost;
            public int spellPointCost;

            public void Deconstruct(out int gcost, out int spcost)
            {
                gcost = goldCost;
                spcost = spellPointCost;
            }
        }

        /// <summary>
        /// Performs complete gold and spellpoint costs for an array of effects.
        /// Also calculates multipliers for target type.
        /// </summary>
        /// <param name="effectEntries">EffectEntry array for spell.</param>
        /// <param name="targetType">Target type of spell.</param>
        /// <param name="totalGoldCostOut">Total gold cost out.</param>
        /// <param name="totalSpellPointCostOut">Total spellpoint cost out.</param>
        /// <param name="casterEntity">Caster entity. Assumed to be player if null.</param>
        /// <param name="minimumCastingCost">Spell point always costs minimum (e.g. from vampirism). Do not set true for reflection/absorption cost calculations.</param>
        public static SpellCost CalculateTotalEffectCosts(EffectEntry[] effectEntries, TargetTypes targetType, DaggerfallEntity casterEntity = null, bool minimumCastingCost = false)
        {
            Func<EffectEntry[], TargetTypes, DaggerfallEntity, bool, SpellCost> del;
            if (TryGetOverride("CalculateTotalEffectCosts", out del))
                return del(effectEntries, targetType, casterEntity, minimumCastingCost);

            const int castCostFloor = 5;

            SpellCost totalCost;
            totalCost.goldCost = 0;
            totalCost.spellPointCost = 0;

            // Must have effect entries
            if (effectEntries == null || effectEntries.Length == 0)
                return totalCost;

            // Add costs for each active effect slot
            for (int i = 0; i < effectEntries.Length; i++)
            {
                if (string.IsNullOrEmpty(effectEntries[i].Key))
                    continue;

                (int goldCost, int spellPointCost) = CalculateEffectCosts(effectEntries[i], casterEntity);
                totalCost.goldCost += goldCost;
                totalCost.spellPointCost += spellPointCost;
            }

            // Multipliers for target type
            totalCost.goldCost = ApplyTargetCostMultiplier(totalCost.goldCost, targetType);
            totalCost.spellPointCost = ApplyTargetCostMultiplier(totalCost.spellPointCost, targetType);

            // Set vampire spell cost
            if (minimumCastingCost)
                totalCost.spellPointCost = castCostFloor;

            // Enforce minimum
            if (totalCost.spellPointCost < castCostFloor)
                totalCost.spellPointCost = castCostFloor;

            return totalCost;
        }

        /// <summary>
        /// Calculate effect costs from an EffectEntry.
        /// </summary>
        public static SpellCost CalculateEffectCosts(EffectEntry effectEntry, DaggerfallEntity casterEntity = null)
        {
            // Get effect template
            IEntityEffect effectTemplate = GameManager.Instance.EntityEffectBroker.GetEffectTemplate(effectEntry.Key);
            if (effectTemplate == null)
                return new SpellCost { goldCost = 0, spellPointCost = 0 };

            return CalculateEffectCosts(effectTemplate, effectEntry.Settings, casterEntity);
        }

        /// <summary>
        /// Calculates effect costs from an IEntityEffect and custom settings.
        /// </summary>
        public static SpellCost CalculateEffectCosts(IEntityEffect effect, EffectSettings settings, DaggerfallEntity casterEntity = null)
        {
            Func<IEntityEffect, EffectSettings, DaggerfallEntity, SpellCost> del;
            if(TryGetOverride("CalculateEffectCosts", out del))
                return del(effect, settings, casterEntity);

            bool activeComponents = false;            

            // Get related skill
            int skillValue = 0;
            if (casterEntity == null)
            {
                // From player
                skillValue = GameManager.Instance.PlayerEntity.Skills.GetLiveSkillValue((DFCareer.Skills)effect.Properties.MagicSkill);
            }
            else
            {
                // From another entity
                skillValue = casterEntity.Skills.GetLiveSkillValue((DFCareer.Skills)effect.Properties.MagicSkill);
            }

            // Duration costs
            int durationGoldCost = 0;
            if (effect.Properties.SupportDuration)
            {
                activeComponents = true;
                durationGoldCost = GetEffectComponentCosts(
                    effect.Properties.DurationCosts,
                    settings.DurationBase,
                    settings.DurationPlus,
                    settings.DurationPerLevel,
                    skillValue);

                //Debug.LogFormat("Duration: gold {0} spellpoints {1}", durationGoldCost, durationSpellPointCost);
            }

            // Chance costs
            int chanceGoldCost = 0;
            if (effect.Properties.SupportChance)
            {
                activeComponents = true;
                chanceGoldCost = GetEffectComponentCosts(
                    effect.Properties.ChanceCosts,
                    settings.ChanceBase,
                    settings.ChancePlus,
                    settings.ChancePerLevel,
                    skillValue);

                //Debug.LogFormat("Chance: gold {0} spellpoints {1}", chanceGoldCost, chanceSpellPointCost);
            }

            // Magnitude costs
            int magnitudeGoldCost = 0;
            if (effect.Properties.SupportMagnitude)
            {
                activeComponents = true;
                int magnitudeBase = (settings.MagnitudeBaseMax + settings.MagnitudeBaseMin) / 2;
                int magnitudePlus = (settings.MagnitudePlusMax + settings.MagnitudePlusMin) / 2;
                magnitudeGoldCost = GetEffectComponentCosts(
                    effect.Properties.MagnitudeCosts,
                    magnitudeBase,
                    magnitudePlus,
                    settings.MagnitudePerLevel,
                    skillValue);

                //Debug.LogFormat("Magnitude: gold {0} spellpoints {1}", magnitudeGoldCost, magnitudeSpellPointCost);
            }

            // If there are no active components (e.g. Teleport) then fudge some costs
            // This gives the same casting cost outcome as classic and supplies a reasonable gold cost
            // Note: Classic does not assign a gold cost when a zero-component effect is the only effect present, which seems like a bug
            int fudgeGoldCost = 0;
            if (!activeComponents)
                fudgeGoldCost = GetEffectComponentCosts(BaseEntityEffect.MakeEffectCosts(60, 100, 160), 1, 1, 1, skillValue);

            // Add gold costs together and calculate spellpoint cost from the result
            SpellCost effectCost;
            effectCost.goldCost = durationGoldCost + chanceGoldCost + magnitudeGoldCost + fudgeGoldCost;
            effectCost.spellPointCost = effectCost.goldCost * (110 - skillValue) / 400;

            //Debug.LogFormat("Costs: gold {0} spellpoints {1}", finalGoldCost, finalSpellPointCost);
            return effectCost;
        }

        public static int ApplyTargetCostMultiplier(int cost, TargetTypes targetType)
        {
            Func<int, TargetTypes, int> del;
            if (TryGetOverride("ApplyTargetCostMultiplier", out del))
                return del(cost, targetType);

            switch (targetType)
            {
                default:
                case TargetTypes.CasterOnly:                // x1.0
                case TargetTypes.ByTouch:
                    // These do not change costs, just including here for completeness
                    break;
                case TargetTypes.SingleTargetAtRange:       // x1.5
                    cost = (int)(cost * 1.5f);
                    break;
                case TargetTypes.AreaAroundCaster:          // x2.0
                    cost = (int)(cost * 2.0f);
                    break;
                case TargetTypes.AreaAtRange:               // x2.5
                    cost = (int)(cost * 2.5f);
                    break;
            }

            return cost;
        }

        static int GetEffectComponentCosts(
            EffectCosts costs,
            int starting,
            int increase,
            int perLevel,
            int skillValue)
        {
            //Calculate effect gold cost, spellpoint cost is calculated from gold cost after adding up for duration, chance and magnitude
            return trunc(costs.OffsetGold + costs.CostA * starting + costs.CostB * trunc(increase / perLevel));
        }

        /// <summary>
        /// Reversed from classic. Calculates enchantment point/gold value for a spell being attached to an item.
        /// </summary>
        /// <param name="spellIndex">Index of spell in SPELLS.STD.</param>
        public static int GetSpellEnchantPtCost(int spellIndex)
        {
            List<SpellRecord.SpellRecordData> spells = DaggerfallSpellReader.ReadSpellsFile();
            int cost = 0;

            foreach (SpellRecord.SpellRecordData spell in spells)
            {
                if (spell.index == spellIndex)
                {
                    cost = 10 * CalculateCastingCost(spell);
                    break;
                }
            }

            return cost;
        }

        /// <summary>
        /// Reversed from classic. Calculates cost of casting a spell. This cost is also used
        /// to lower item condition when equipping an item whith a "Cast when held" effect.
        /// For now this is only being used for enchanted items, because there is other code for entity-cast spells.
        /// </summary>
        /// <param name="spell">Spell record read from SPELLS.STD.</param>
        /// <param name="enchantingItem">True if the method is used from the magic item maker.</param>
        public static int CalculateCastingCost(SpellRecord.SpellRecordData spell, bool enchantingItem= true)
        {
            Func<SpellRecord.SpellRecordData, bool, int> del;
            if (TryGetOverride("CalculateCastingCost", out del))
                return del(spell, enchantingItem);

            // Indices into effect settings array for each effect and its subtypes
            byte[] effectIndices = {    0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Paralysis
                                        0x01, 0x02, 0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Continuous Damage
                                        0x04, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Create Item
                                        0x05, 0x05, 0x06, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Cure
                                        0x07, 0x07, 0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Damage
                                        0x08, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Disintegrate
                                        0x09, 0x08, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Dispel
                                        0x0A, 0x0A, 0x0A, 0x0A, 0x0A, 0x0A, 0x0A, 0x0A, 0x00, 0x00, 0x00, 0x00, // Drain
                                        0x0B, 0x0B, 0x0B, 0x0B, 0x0B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Elemental Resistance
                                        0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x0C, 0x00, 0x00, 0x00, 0x00, // Fortify Attribute
                                        0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x0D, 0x07, 0x0E, 0x00, 0x00, // Heal
                                        0x0F, 0x0F, 0x0F, 0x0F, 0x0F, 0x0F, 0x0F, 0x0F, 0x0F, 0x0F, 0x00, 0x00, // Transfer
                                        0x26, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Soul Trap
                                        0x10, 0x11, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Invisibility
                                        0x12, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Levitate
                                        0x13, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Light
                                        0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Lock
                                        0x28, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Open
                                        0x15, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Regenerate
                                        0x16, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Silence
                                        0x17, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Spell Absorption
                                        0x17, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Spell Reflection
                                        0x16, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Spell Resistance
                                        0x18, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Chameleon
                                        0x18, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Shadow
                                        0x14, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Slowfall
                                        0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Climbing
                                        0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Jumping
                                        0x19, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Free Action
                                        0x1A, 0x1A, 0x1A, 0x1A, 0x1A, 0x1A, 0x1A, 0x00, 0x00, 0x00, 0x00, 0x00, // Lycanthropy/Polymorph
                                        0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Water Breathing
                                        0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Water Walking
                                        0x1B, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Dimunition
                                        0x1A, 0x1C, 0x1C, 0x1D, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Calm
                                        0x1E, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Charm
                                        0x1F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Shield
                                        0x27, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Telekinesis
                                        0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Astral Travel
                                        0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Etherealness
                                        0x21, 0x21, 0x22, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Detect
                                        0x23, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Identify
                                        0x24, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Wizard Sight
                                        0x07, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Darkness
                                        0x25, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Recall
                                        0x29, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Comprehend Languages
                                        0x2A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Intensify Fire
                                        0x2A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Diminish Fire
                                        0x2A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Wall of Stone?
                                        0x2A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Wall of Fire?
                                        0x2A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, // Wall of Frost?
                                        0x2A, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }; // Wall of Poison?

            // These are coefficients for each effect type and subtype. They affect casting cost, enchantment point cost and magic item worth.
            // There are 4 coefficients, used together with duration, chance and magnitude settings.
            // Which one they are used with depends on which "settings type" the effect is classified as.
            ushort[] effectCoefficients = {
                                        0x07, 0x19, 0x07, 0x19, // Paralysis / Cure Magic?
                                        0x07, 0x02, 0x0A, 0x07, // Continuous Damage - Health
                                        0x05, 0x02, 0x0A, 0x07, // Continuous Damage - Stamina / Climbing / Jumping / Water Breathing / Water Walking
                                        0x0A, 0x02, 0x0A, 0x07, // Continuous Damage - Spell Points
                                        0x0F, 0x1E, 0x00, 0x00, // Create Item
                                        0x02, 0x19, 0x00, 0x00, // Cure Disease / Cure Poison
                                        0x05, 0x23, 0x00, 0x00, // Cure Paralysis
                                        0x05, 0x07, 0x00, 0x00, // Damage Health / Damage Stamina / Damage Spell Points / Heal Health / Darkness
                                        0x14, 0x23, 0x00, 0x00, // Disintegrate / Dispel Undead
                                        0x1E, 0x2D, 0x00, 0x00, // Dispel Magic / Dispel Daedra
                                        0x04, 0x19, 0x02, 0x19, // Drain Attribute
                                        0x19, 0x19, 0x02, 0x19, // Elemental Resistance
                                        0x07, 0x19, 0x0A, 0x1E, // Fortify Attribute
                                        0x0A, 0x07, 0x00, 0x00, // Heal Attribute
                                        0x02, 0x07, 0x00, 0x00, // Heal Stamina
                                        0x05, 0x05, 0x0F, 0x19, // Transfer
                                        0x0A, 0x1E, 0x00, 0x00, // Invisibility
                                        0x0F, 0x23, 0x00, 0x00, // True Invisibility
                                        0x0F, 0x19, 0x00, 0x00, // Levitate
                                        0x02, 0x0A, 0x00, 0x00, // Light
                                        0x05, 0x19, 0x07, 0x1E, // Lock / Slowfall
                                        0x19, 0x05, 0x02, 0x02, // Regenerate
                                        0x05, 0x19, 0x05, 0x19, // Silence / Spell Resistance
                                        0x07, 0x23, 0x07, 0x23, // Spell Absorption / Spell Reflection
                                        0x05, 0x14, 0x00, 0x00, // Chameleon / Shadow
                                        0x05, 0x05, 0x00, 0x00, // Free Action
                                        0x0F, 0x19, 0x0F, 0x19, // Lycanthropy / Polymorph / Calm Animal
                                        0x0A, 0x14, 0x14, 0x28, // Diminution
                                        0x0A, 0x05, 0x14, 0x23, // Calm Undead / Calm Humanoid
                                        0x07, 0x02, 0x0F, 0x1E, // Calm Daedra? (Unused)
                                        0x05, 0x02, 0x0A, 0x0F, // Charm
                                        0x07, 0x02, 0x14, 0x0F, // Shield
                                        0x23, 0x02, 0x0A, 0x19, // Astral Travel / Etherealness
                                        0x05, 0x02, 0x14, 0x1E, // Detect Magic / Detect Enemy
                                        0x05, 0x02, 0x0F, 0x19, // Detect Treasure
                                        0x05, 0x02, 0x0A, 0x19, // Identify
                                        0x07, 0x0C, 0x05, 0x05, // Wizard Sight
                                        0x23, 0x2D, 0x00, 0x00, // Recall
                                        0x0F, 0x11, 0x0A, 0x11, // Soul Trap
                                        0x14, 0x11, 0x19, 0x23, // Telekinesis
                                        0x05, 0x19, 0x00, 0x00, // Open
                                        0x0F, 0x11, 0x0A, 0x11, // Comprehend Languages
                                        0x0F, 0x0F, 0x05, 0x05 }; // Intensify Fire / Diminish Fire / Wall of --

            // Used to know which Magic School an effect belongs to
            byte[] effectMagicSchools = { 0, 2, 3, 1, 2, 2, 3, 2, 0, 1,
                                          1, 2, 3, 5, 4, 5, 3, 3, 1, 3,
                                          1, 4, 4, 5, 5, 0, 1, 0, 0, 5,
                                          0, 4, 0, 4, 4, 0, 3, 3, 0, 4,
                                          4, 4, 5, 3, 3, 0, 0, 4, 4, 4,
                                          4 };

            // Used to get the skill corresponding to each of the above magic school
            DFCareer.Skills[] magicSkills = { DFCareer.Skills.Alteration,
                                              DFCareer.Skills.Restoration,
                                              DFCareer.Skills.Destruction,
                                              DFCareer.Skills.Mysticism,
                                              DFCareer.Skills.Thaumaturgy,
                                              DFCareer.Skills.Illusion };

            // All effects have one of 6 types for their settings depending on which settings (duration, chance, magnitude)
            // they use, which determine how the coefficient values are used with their data to determine spell casting
            // cost /magic item value/enchantment point cost.
            // There is also a 7th type that is supported in classic (see below) but no effect is defined to use it.
            byte[] settingsTypes = { 1, 2, 3, 4, 5, 4, 4, 2, 1, 6,
                                     5, 6, 1, 3, 3, 3, 1, 4, 2, 1,
                                     1, 1, 1, 3, 3, 3, 3, 3, 3, 1,
                                     3, 3, 1, 1, 1, 2, 2, 1, 1, 1,
                                     1, 1, 3, 4, 1, 2, 2, 2, 2, 2,
                                     2 };

            // Modifiers for casting ranges
            byte[] rangeTypeModifiers = { 2, 2, 3, 4, 5 };

            int cost = 0;
            int skill = 50; // 50 is used for item enchantments
            
            for (int i = 0; i < 3; ++i)
            {
                if (i < spell.effects.Length && spell.effects[i].type != -1)
                {
                    // Get the coefficients applied to settings for this effect and copy them into the temporary variable
                    ushort[] coefficientsForThisEffect = new ushort[4];

                    if (spell.effects[i].subType == -1) // No subtype
                    {
                        Array.Copy(effectCoefficients, 4 * effectIndices[12 * spell.effects[i].type], coefficientsForThisEffect, 0, 4);
                    }
                    else // Subtype exists
                    {
                        Array.Copy(effectCoefficients, 4 * effectIndices[12 * spell.effects[i].type + spell.effects[i].subType], coefficientsForThisEffect, 0, 4);
                    }

                    if (!enchantingItem)
                    {
                        // If not using the item maker, then player skill corresponding to the effect magic school must be used
                        skill = GameManager.Instance.PlayerEntity.Skills.GetLiveSkillValue(magicSkills[effectMagicSchools[spell.effects[i].type]]);
                    }

                    // Add to the cost based on this effect's settings
                    cost += getCostFromSettings(settingsTypes[spell.effects[i].type], i, spell, coefficientsForThisEffect) * (110 - skill) / 100;
                }
            }

            cost = cost * rangeTypeModifiers[spell.rangeType] >> 1;
            if (cost < 5)
                cost = 5;

            return cost;
        }

        /// <summary>
        /// Reversed from classic. Used wih calculating cost of casting a spell.
        /// This uses the spell's settings for chance, duration and magnitude together with coefficients for that effect
        /// to get the cost of the effect, before the range type modifier is applied.
        /// </summary>
        public static int getCostFromSettings(int settingsType, int effectNumber, SpellRecord.SpellRecordData spellData, ushort[] coefficients)
        {
            int cost = 0;

            switch (settingsType)
            {
                case 1:
                    // Coefficients used with:
                    // 0 = durationBase, 1 = durationMod / durationPerLevel, 2 = chanceBase, 3 = chanceMod / chancePerLevel
                    cost =    coefficients[0] * spellData.effects[effectNumber].durationBase
                            + spellData.effects[effectNumber].durationMod / spellData.effects[effectNumber].durationPerLevel * coefficients[1]
                            + coefficients[2] * spellData.effects[effectNumber].chanceBase
                            + spellData.effects[effectNumber].chanceMod / spellData.effects[effectNumber].chancePerLevel * coefficients[3];
                    break;
                case 2:
                    // Coefficients used with:
                    // 0 = durationBase, 1 = durationMod / durationPerLevel, 2 = (magnitudeBaseHigh + magnitudeBaseLow) / 2, 3 = (magnitudeLevelBase + magnitudeLevelHigh) / 2 / magnitudePerLevel
                    cost =    coefficients[0] * spellData.effects[effectNumber].durationBase
                            + spellData.effects[effectNumber].durationMod / spellData.effects[effectNumber].durationPerLevel * coefficients[1]
                            + (spellData.effects[effectNumber].magnitudeBaseHigh + spellData.effects[effectNumber].magnitudeBaseLow) / 2 * coefficients[2]
                            + (spellData.effects[effectNumber].magnitudeLevelBase + spellData.effects[effectNumber].magnitudeLevelHigh) / 2 / spellData.effects[effectNumber].magnitudePerLevel * coefficients[3];
                    break;
                case 3:
                    // Coefficients used with:
                    // 0 = durationBase, 1 = durationMod / durationPerLevel
                    cost =    coefficients[0] * spellData.effects[effectNumber].durationBase
                            + spellData.effects[effectNumber].durationMod / spellData.effects[effectNumber].durationPerLevel * coefficients[1];
                    break;
                case 4:
                    // Coefficients used with:
                    // 0 = chanceBase, 1 = chanceMod / chancePerLevel
                    cost =    coefficients[0] * spellData.effects[effectNumber].chanceBase
                            + spellData.effects[effectNumber].chanceMod / spellData.effects[effectNumber].chancePerLevel * coefficients[1];
                    break;
                case 5:
                    // Coefficients used with:
                    // 0 = (magnitudeBaseHigh + magnitudeBaseLow) / 2, 1 = (magnitudeLevelBase + magnitudeLevelHigh) / 2 / magnitudePerLevel
                    cost =    coefficients[0] * ((spellData.effects[effectNumber].magnitudeBaseHigh + spellData.effects[effectNumber].magnitudeBaseLow) / 2)
                            + (spellData.effects[effectNumber].magnitudeLevelBase + spellData.effects[effectNumber].magnitudeLevelHigh) / 2 / spellData.effects[effectNumber].magnitudePerLevel * coefficients[1];
                    break;
                case 6:
                    // Coefficients used with:
                    // 0 = durationBase, 1 = durationMod / durationPerLevel, 2 = (magnitudeBaseHigh + magnitudeBaseLow) / 2, 3 = (magnitudeLevelBase + magnitudeLevelHigh) / 2 / magnitudePerLevel
                    cost =    coefficients[0] * spellData.effects[effectNumber].durationBase
                            + coefficients[1] * spellData.effects[effectNumber].durationMod / spellData.effects[effectNumber].durationPerLevel
                            + ((spellData.effects[effectNumber].magnitudeBaseHigh + spellData.effects[effectNumber].magnitudeBaseLow) / 2) * coefficients[2]
                            + coefficients[3] / spellData.effects[effectNumber].magnitudePerLevel * ((spellData.effects[effectNumber].magnitudeLevelBase + spellData.effects[effectNumber].magnitudeLevelHigh) / 2);
                    break;
                case 7: // Not used
                    // Coefficients used with:
                    // 0 = (magnitudeBaseHigh + magnitudeBaseLow) / 2, 1 = (magnitudeLevelBase + magnitudeLevelHigh) / 2 / magnitudePerLevel * durationBase / durationMod
                    cost =    (spellData.effects[effectNumber].magnitudeBaseHigh + spellData.effects[effectNumber].magnitudeBaseLow) / 2 * coefficients[0]
                            + coefficients[1] * (spellData.effects[effectNumber].magnitudeLevelBase + spellData.effects[effectNumber].magnitudeLevelHigh) / 2 / spellData.effects[effectNumber].magnitudePerLevel * spellData.effects[effectNumber].durationBase / spellData.effects[effectNumber].durationMod;
                    break;
            }
            return cost;
        }

        // Just makes formulas more readable
        static int trunc(double value) { return (int)Math.Truncate(value); }

        #endregion

        #region Enchanting

        /// <summary>
        /// Gets the maximum enchantment capacity for any item.
        /// </summary>
        /// <param name="item">Source item.</param>
        /// <returns>Item max enchantment power.</returns>
        public static int GetItemEnchantmentPower(DaggerfallUnityItem item)
        {
            Func<DaggerfallUnityItem, int> del;
            if (TryGetOverride("GetItemEnchantmentPower", out del))
                return del(item);

            if (item == null)
                throw new Exception("GetItemEnchantmentPower: item is null");

            float multiplier = 0f;
            if (item.ItemGroup == ItemGroups.Weapons)
                multiplier = GetWeaponEnchantmentMultiplier((MaterialTypes)item.NativeMaterialValue);
            else if (item.ItemGroup == ItemGroups.Armor)
                multiplier = GetArmorEnchantmentMultiplier((ArmorMaterialTypes)item.NativeMaterialValue);

            // Final enchantment power is basePower + basePower*multiplier (rounded down)
            int basePower = item.ItemTemplate.enchantmentPoints;
            return basePower + Mathf.FloorToInt(basePower * multiplier);
        }

        public static float GetWeaponEnchantmentMultiplier(MaterialTypes weaponMaterial)
        {
            // UESP lists regular material power progression in weapon matrix: https://en.uesp.net/wiki/Daggerfall:Enchantment_Power#Weapons
            // Enchantment power values for staves are inaccurate in UESP weapon matrix (confirmed in classic)
            // The below yields correct enchantment power for staves matching classic
            // ProjectN: modified enchantement multipliers;
            // TODO: these are the same as Armor modifiers, the two methods should be unified.
            switch(weaponMaterial)
            {
                case MaterialTypes.Iron:           // Iron is -25% from base
                    return -0.25f;
                default:
                case MaterialTypes.Base:           // Base/Steel/Orcish all use base enchantment power
                case MaterialTypes.Steel:
                case MaterialTypes.Orcish:
                    return 0f;
                case MaterialTypes.Dwarven:        // Dwarven is +25% from base
                    return 0.25f;
                case MaterialTypes.Elven:          // Elven is +50% from base
                    return 0.5f;
                case MaterialTypes.Silver:         // Silver is +100% from base                
                case MaterialTypes.Ebony:          // Ebony is +100% from base
                case MaterialTypes.Daedric:        // Daedric is +100% from base
                    return 1.0f;
                case MaterialTypes.Glass:          // Glass is + 125% from base
                    return 1.25f;
                case MaterialTypes.Adamantium:     // Adamantium is +150% from base
                    return 1.5f;
                case MaterialTypes.Mithril:        // Mithril is +200% from base
                    return 2.0f;
            }
        }

        public static float GetArmorEnchantmentMultiplier(ArmorMaterialTypes armorMaterial)
        {
            // UESP lists highly variable material power progression in Armor matrix: https://en.uesp.net/wiki/Daggerfall:Enchantment_Power#Armor
            // This indicates certain Armor types don't follow the same general material progression patterns for enchantment point multipliers
            // Yet to confirm this in classic - but not entirely confident in accuracy of UESP information here either
            // For now using consistent progression for enchantment point multipliers and can improve later if required
            // ProjectN: modified enchantement multipliers;
            MaterialTypes materialType = ItemBuilder.GetArmorMaterialType((int)armorMaterial);
            switch (materialType)
            {
                case MaterialTypes.Iron:           // Iron is -25% from base
                    return -0.25f;
                default:
                case MaterialTypes.Base:           // Base/Steel/Orcish all use base enchantment power
                case MaterialTypes.Steel:
                case MaterialTypes.Orcish:
                    return 0f;
                case MaterialTypes.Dwarven:        // Dwarven is +25% from base
                    return 0.25f;
                case MaterialTypes.Elven:          // Elven is +50% from base
                    return 0.5f;
                case MaterialTypes.Silver:         // Silver is +100% from base                
                case MaterialTypes.Ebony:          // Ebony is +100% from base
                case MaterialTypes.Daedric:        // Daedric is +100% from base
                    return 1.0f;
                case MaterialTypes.Glass:          // Glass is + 125% from base
                    return 1.25f;
                case MaterialTypes.Adamantium:     // Adamantium is +150% from base
                    return 1.5f;
                case MaterialTypes.Mithril:        // Mithril is +200% from base
                    return 2.0f;
            }
        }

        #endregion

        #region Formula Overrides

        /// <summary>
        /// Registers an override for a formula using a generic `System.Func{T}` callback
        /// with the same signature as the method it overrides
        /// (i.e. `RegisterOverride{Func{int, int, float}}("FormulaName", (a, b) => (float)a / b);`).
        /// The invocation will fail if signature is not correct, meaning if the delegate
        /// is not one of the variation of `Func` with the expected arguments.
        /// </summary>
        /// <param name="provider">The mod that provides this override; used to enforce load order.</param>
        /// <param name="formulaName">The name of the method that provides the formula.</param>
        /// <param name="formula">A callback that implements the formula.</param>
        /// <typeparam name="TDelegate">One of the available generic Func delegates.</typeparam>
        /// <exception cref="ArgumentNullException">`formulaName` or `formula` is null.</exception>
        /// <exception cref="InvalidCastException">Type is not a delegate.</exception>
        public static void RegisterOverride<TDelegate>(Mod provider, string formulaName, TDelegate formula)
            where TDelegate : class
        {
            if (formulaName == null)
                throw new ArgumentNullException("formulaName");

            if (formula == null)
                throw new ArgumentNullException("formula");

            var del = formula as Delegate;
            if (del == null)
                throw new InvalidCastException("formula is not a delegate.");

            FormulaOverride formulaOverride;
            if (!overrides.TryGetValue(formulaName, out formulaOverride) || formulaOverride.Provider.LoadPriority < provider.LoadPriority)
                overrides[formulaName] = new FormulaOverride(del, provider);
        }

        /// <summary>
        /// Gets an override for a formula.
        /// </summary>
        /// <param name="formulaName">The name of the method that provides the formula.</param>
        /// <param name="formula">A callback that implements the formula.</param>
        /// <typeparam name="TDelegate">One of the available generic Func delegates.</typeparam>
        /// <returns>True if formula is found.</returns>
        private static bool TryGetOverride<TDelegate>(string formulaName, out TDelegate formula)
            where TDelegate : class
        {
            FormulaOverride formulaOverride;
            if (overrides.TryGetValue(formulaName, out formulaOverride))
            {
                if ((formula = formulaOverride.Formula as TDelegate) != null)
                    return true;

                const string errorMessage = "Removed override for formula {0} provided by {1} because signature doesn't match (expected {2} and got {3}).";
                Debug.LogErrorFormat(errorMessage, formulaName, formulaOverride.Provider.Title, typeof(TDelegate), formulaOverride.Formula.GetType());
                overrides.Remove(formulaName);
            }

            formula = default(TDelegate);
            return false;
        }

        #endregion
    }
}
