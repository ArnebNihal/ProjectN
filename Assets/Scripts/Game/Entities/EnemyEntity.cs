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
using DaggerfallConnect;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallConnect.Save;
using DaggerfallConnect.FallExe;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Items;
using System;

namespace DaggerfallWorkshop.Game.Entity
{
    /// <summary>
    /// The parameters involved in creating an enemy loot pile
    /// </summary>
    public class EnemyLootSpawnedEventArgs : System.EventArgs
    {
        /// <summary>
        /// The Mobile object used for the enemy
        /// </summary>
        public MobileEnemy MobileEnemy { get; set; }

        /// <summary>
        /// The Career template of the enemy
        /// </summary>
        public DFCareer EnemyCareer { get; set; }

        /// <summary>
        /// The collection containing all the items of the loot pile. New items can be added
        /// </summary>
        public ItemCollection Items { get; set; }
    }

    /// <summary>
    /// Implements DaggerfallEntity with properties specific to enemies.
    /// </summary>
    public class EnemyEntity : DaggerfallEntity
    {
        #region Fields

        public static System.EventHandler<EnemyLootSpawnedEventArgs> OnLootSpawned;

        int careerIndex = -1;
        EntityTypes entityType = EntityTypes.None;
        MobileEnemy mobileEnemy;
        bool pickpocketByPlayerAttempted = false;
        int questFoeSpellQueueIndex = -1;
        int questFoeItemQueueIndex = -1;
        bool suppressInfighting = false;

        // From FALL.EXE offset 0x1C0F14
        static byte[] ImpSpells            = { 0x07, 0x0A, 0x1D, 0x2C };
        static byte[] GhostSpells          = { 0x22 };
        static byte[] OrcShamanSpells      = { 0x06, 0x07, 0x16, 0x19, 0x1F };
        static byte[] WraithSpells         = { 0x1C, 0x1F };
        static byte[] FrostDaedraSpells    = { 0x10, 0x14 };
        static byte[] FireDaedraSpells     = { 0x0E, 0x19 };
        static byte[] DaedrothSpells       = { 0x16, 0x17, 0x1F };
        static byte[] VampireSpells        = { 0x33 };
        static byte[] SeducerSpells        = { 0x34, 0x43 };
        static byte[] VampireAncientSpells = { 0x08, 0x32 };
        static byte[] DaedraLordSpells     = { 0x08, 0x0A, 0x0E, 0x3C, 0x43 };
        static byte[] LichSpells           = { 0x08, 0x0A, 0x0E, 0x22, 0x3C };
        static byte[] AncientLichSpells    = { 0x08, 0x0A, 0x0E, 0x1D, 0x1F, 0x22, 0x3C };
        static byte[] HomunculusSpells     = { 0x07, 0x0A, 0x1D, 0x2C };
        static byte[] MedusaSpells         = { 0x23 };
        static byte[] SnowWolfSpells       = { 0x10 };
        static byte[] HellHoundSpells      = { 0x07 };
        static byte[] DremoraSpells        = { 0x08, 0x0E };
        static byte[] ScampSpells          = { 0x1F };
        static byte[][] EnemyClassSpells   = { FrostDaedraSpells, DaedrothSpells, OrcShamanSpells, VampireAncientSpells, DaedraLordSpells, LichSpells, AncientLichSpells };

        #endregion

        #region Properties

        public EntityTypes EntityType
        {
            get { return entityType; }
        }

        public int CareerIndex
        {
            get { return careerIndex; }
        }

        public MobileEnemy MobileEnemy
        {
            get { return mobileEnemy; }
        }

        public bool PickpocketByPlayerAttempted
        {
            get { return (pickpocketByPlayerAttempted); }
            set { pickpocketByPlayerAttempted = value; }
        }

        public int QuestFoeSpellQueueIndex
        {
            get { return questFoeSpellQueueIndex; }
            set { questFoeSpellQueueIndex = value; }
        }

        public int QuestFoeItemQueueIndex
        {
            get { return questFoeItemQueueIndex; }
            set { questFoeItemQueueIndex = value; }
        }

        /// <summary>
        /// Suppress enemy infighting for this entity.
        /// Entity will not target anyone but player and cannot be a target for infighting.
        /// One example of use is Daedra Secuder whose winged sprites have no facing other than directly forward to player.
        /// If Seducer participates in winged infighting their sprite can no longer align properly with controller facing.
        /// Seducer behaviour will disable infighting once they transform into winged variant so enemy combats player only.
        /// </summary>
        public bool SuppressInfighting
        {
            get { return suppressInfighting; }
            set { suppressInfighting = value; }
        }

        public bool SoulTrapActive { get; set; }

        public bool WabbajackActive { get; set; }

        public delegate void EnemyStartingEquipment(PlayerEntity player, EnemyEntity enemyEntity, int variant);
        public static EnemyStartingEquipment AssignEnemyEquipment = DaggerfallUnity.Instance.ItemHelper.AssignEnemyStartingEquipment;

        #endregion

        #region Constructors

        public EnemyEntity(DaggerfallEntityBehaviour entityBehaviour)
            : base(entityBehaviour)
        {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Assigns default entity settings.
        /// </summary>
        public override void SetEntityDefaults()
        {
        }

        /// <summary>
        /// Custom handling of SetHealth() for enemies to support soul trap.
        /// </summary>
        public override int SetHealth(int amount, bool restoreMode = false)
        {
            // Just do base if no soul trap active
            if (!SoulTrapActive)
                return base.SetHealth(amount, restoreMode);

            // Reduce health
            currentHealth = Mathf.Clamp(amount, 0, MaxHealth);
            if (currentHealth <= 0)
            {
                // Attempt soul trap and allow entity to die based on outcome
                if (AttemptSoulTrap())
                {
                    SoulTrapActive = false;
                    return base.SetHealth(amount, restoreMode);
                }
            }

            return currentHealth;
        }

        public override void Update(DaggerfallEntityBehaviour sender)
        {
            base.Update(sender);

            // Despawn city watch when active crime state returns to none
            // This can happen when exiting city area, after fast travel, or via console
            if (entityType == EntityTypes.EnemyClass &&
                careerIndex == (int)MobileTypes.Knight_CityWatch - 128 &&
                GameManager.Instance.PlayerEntity.CrimeCommitted == PlayerEntity.Crimes.None &&
                !GameManager.Instance.PlayerEffectManager.IsTransformedLycanthrope())
            {
                GameObject.Destroy(sender.gameObject);
            }
        }

        /// <summary>
        /// Attempt to trap a soul.
        /// </summary>
        /// <returns>True if entity is allowed to die after trap attempt.</returns>
        bool AttemptSoulTrap()
        {
            // Must have a peered DaggerfallEntityBehaviour and EntityEffectManager
            EntityEffectManager manager = (EntityBehaviour) ? EntityBehaviour.GetComponent<EntityEffectManager>() : null;
            if (!manager)
                return true;

            // Find the soul trap incumbent
            SoulTrap soulTrapEffect = (SoulTrap)manager.FindIncumbentEffect<SoulTrap>();
            if (soulTrapEffect == null)
                return true;

            // Roll chance for trap
            // If trap fails then entity should die as normal without trapping a soul
            // If trap succeeds and player has a free soul gem then entity should die after storing soul
            // If trap succeeds and player has no free soul gems then entity will not die until effect expires or fails
            if (soulTrapEffect.RollTrapChance())
            {
                // Attempt to fill an empty soul trap
                if (SoulTrap.FillEmptyTrapItem((MobileTypes)mobileEnemy.ID))
                {
                    // Trap filled, allow entity to die normally
                    DaggerfallUI.AddHUDText(TextManager.Instance.GetLocalizedText("trapSuccess"), 1.5f);
                    return true;
                }
                else
                {
                    // No empty gems, keep entity tethered to life - player is alerted so they know what's happening
                    currentHealth = 1;
                    DaggerfallUI.AddHUDText(TextManager.Instance.GetLocalizedText("trapNoneEmpty"));
                    return false;
                }
            }
            else
            {
                // Trap failed
                DaggerfallUI.AddHUDText(TextManager.Instance.GetLocalizedText("trapFail"), 1.5f);
                return true;
            }
        }

        public override void ClearConstantEffects()
        {
            base.ClearConstantEffects();
            SoulTrapActive = false;
        }

        /// <summary>
        /// Sets enemy career and prepares entity settings.
        /// </summary>
        public void SetEnemyCareer(MobileEnemy mobileEnemy, EntityTypes entityType)
        {
            // Try custom career first
            career = GetCustomCareerTemplate(mobileEnemy.ID);

            if (career != null)
            {
                // Custom enemy
                careerIndex = mobileEnemy.ID;
                stats.SetPermanentFromCareer(career);

                if (entityType == EntityTypes.EnemyMonster)
                {
                    // Default like a monster
                    level = mobileEnemy.Level;
                    maxHealth = UnityEngine.Random.Range(mobileEnemy.MinHealth, mobileEnemy.MaxHealth + 1);
                    for (int i = 0; i < ArmorValues.Length; i++)
                    {
                        ArmorValues[i] = (sbyte)(mobileEnemy.ArmorValue);
                    }
                }
                else
                {
                    // Default like a class enemy
                    level = GameManager.Instance.PlayerEntity.Level;
                    maxHealth = FormulaHelper.RollEnemyClassMaxHealth(level, career.HitPointsPerLevel);
                }
            }
            else if (entityType == EntityTypes.EnemyMonster)
            {
                careerIndex = mobileEnemy.ID;
                career = GetMonsterCareerTemplate((MonsterCareers)careerIndex);
                stats.SetPermanentFromCareer(career);

                // Enemy monster has predefined level, health and armor values.
                // Armor values can be modified below by equipment.
                level = mobileEnemy.Level;
                maxHealth = UnityEngine.Random.Range(mobileEnemy.MinHealth, mobileEnemy.MaxHealth + 1);
                for (int i = 0; i < ArmorValues.Length; i++)
                {
                    ArmorValues[i] = (sbyte)(mobileEnemy.ArmorValue);
                }
            }
            else if (entityType == EntityTypes.EnemyClass)
            {
                if (mobileEnemy.ID <= ((int)ClassCareers.Knight + 128))
                {
                    careerIndex = mobileEnemy.ID - 128;
                    career = GetClassCareerTemplate((ClassCareers)careerIndex);
                }
                else{
                    careerIndex = GetNewClassCareerIndex(mobileEnemy.ID);
                    career = GetClassCareerTemplate((ClassCareers)careerIndex);
                }
                
                stats.SetPermanentFromCareer(career);

                // Enemy class is levelled to player and uses similar health rules
                // City guards are 3 to 6 levels above the player
                // TODO: make enemy class level based on location + random factor
                PlayerGPS playerGPS = GameManager.Instance.PlayerGPS;
                PlayerEnterExit playerEnterExit = GameManager.Instance.PlayerEnterExit;
                int region = playerGPS.CurrentRegionIndex;
                DFRegion.LocationTypes location = playerGPS.CurrentLocationType;
                DFRegion.DungeonTypes dungeon;
                GovernmentType government = TextManager.Instance.GetCurrentRegionGovernment(region);
                if (playerEnterExit.IsPlayerInsideDungeon)
                    dungeon = playerGPS.CurrentLocation.MapTableData.DungeonType;
                level += FormulaHelper.GovernmentModifier(government);
                level += FormulaHelper.LocationModifier(location);
                if (playerEnterExit.IsPlayerInsideDungeon)
                {
                    dungeon = playerGPS.CurrentLocation.MapTableData.DungeonType;
                    level += FormulaHelper.DungeonModifier(dungeon);
                }
                level = RandomizeLevel(level);
                Debug.Log("career: " + career.Name + ", level: " + level);
                // level = GameManager.Instance.PlayerEntity.Level;
                // if (careerIndex == (int)MobileTypes.Knight_CityWatch - 128)
                // if (careerIndex == (int)MobileTypes.Knight_CityWatch - 128)
                //     level += UnityEngine.Random.Range(3, 7);

                maxHealth = FormulaHelper.RollEnemyClassMaxHealth(level, career.HitPointsPerLevel);
            }
            else
            {
                career = new DFCareer();
                careerIndex = -1;
                return;
            }

            this.mobileEnemy = mobileEnemy;
            this.entityType = entityType;
            name = career.Name;
            minMetalToHit = mobileEnemy.MinMetalToHit;
            if (mobileEnemy.Team == MobileTeams.Random)
                team = (MobileTeams)UnityEngine.Random.Range(0, Enum.GetNames(typeof(MobileTeams)).Length);
            else team = mobileEnemy.Team;

            short skillsLevel = 0;
            // if (skillsLevel > 100)
            // {
            //     skillsLevel = 100;
            // }

            for (int i = 0; i <= DaggerfallSkills.Count; i++)
            {
                skillsLevel = (short)(level * 3 + GetSkillProficiencyBonus(career, i));
                if (skillsLevel > 100)
                {
                    skillsLevel = 100;
                }
                skills.SetPermanentSkillValue(i, skillsLevel);
            }

            // Generate loot table items
            DaggerfallLoot.GenerateItems(mobileEnemy.LootTableKey, items, level);

            // Enemy classes and some monsters use equipment
            // if (careerIndex == (int)MonsterCareers.Orc || careerIndex == (int)MonsterCareers.OrcShaman)
            // {
            //     SetEnemyEquipment(0);
            // }
            // else if (careerIndex == (int)MonsterCareers.Centaur || careerIndex == (int)MonsterCareers.OrcSergeant)
            // {
            //     SetEnemyEquipment(1);
            // }
            // else if (careerIndex == (int)MonsterCareers.OrcWarlord)
            // {
            //     SetEnemyEquipment(2);
            // }
            if (MonsterHasEquipment(careerIndex, out int variant))
            {
                SetEnemyEquipment(variant);
            }
            else if (entityType == EntityTypes.EnemyClass)
            {
                SetEnemyEquipment(UnityEngine.Random.Range(0, 2)); // 0 or 1
            }

            // Assign spell lists
            if (entityType == EntityTypes.EnemyMonster)
            {
                if (careerIndex == (int)MonsterCareers.Imp)
                    SetEnemySpells(ImpSpells);
                else if (careerIndex == (int)MonsterCareers.Ghost)
                    SetEnemySpells(GhostSpells);
                else if (careerIndex == (int)MonsterCareers.OrcShaman)
                    SetEnemySpells(OrcShamanSpells);
                else if (careerIndex == (int)MonsterCareers.Wraith)
                    SetEnemySpells(WraithSpells);
                else if (careerIndex == (int)MonsterCareers.FrostDaedra)
                    SetEnemySpells(FrostDaedraSpells);
                else if (careerIndex == (int)MonsterCareers.FireDaedra)
                    SetEnemySpells(FireDaedraSpells);
                else if (careerIndex == (int)MonsterCareers.Daedroth)
                    SetEnemySpells(DaedrothSpells);
                else if (careerIndex == (int)MonsterCareers.Vampire)
                    SetEnemySpells(VampireSpells);
                else if (careerIndex == (int)MonsterCareers.DaedraSeducer)
                    SetEnemySpells(SeducerSpells);
                else if (careerIndex == (int)MonsterCareers.VampireAncient)
                    SetEnemySpells(VampireAncientSpells);
                else if (careerIndex == (int)MonsterCareers.DaedraLord)
                    SetEnemySpells(DaedraLordSpells);
                else if (careerIndex == (int)MonsterCareers.Lich)
                    SetEnemySpells(LichSpells);
                else if (careerIndex == (int)MonsterCareers.AncientLich)
                    SetEnemySpells(AncientLichSpells);
                else if (careerIndex == (int)MonsterCareers.Homunculus)
                    SetEnemySpells(HomunculusSpells);
                else if (careerIndex == (int)MonsterCareers.Medusa)
                    SetEnemySpells(MedusaSpells);
                else if (careerIndex == (int)MonsterCareers.SnowWolf)
                    SetEnemySpells(SnowWolfSpells);
                else if (careerIndex == (int)MonsterCareers.HellHound)
                    SetEnemySpells(HellHoundSpells);
                else if (careerIndex == (int)MonsterCareers.FireDaemon)
                    SetEnemySpells(FireDaedraSpells);
                else if (careerIndex == (int)MonsterCareers.Dremora)
                    SetEnemySpells(DremoraSpells);
                else if (careerIndex == (int)MonsterCareers.Scamp)
                    SetEnemySpells(ScampSpells);
            }
            else if (entityType == EntityTypes.EnemyClass && (mobileEnemy.CastsMagic))
            {
                int spellListLevel = level / 3;
                if (spellListLevel > 6)
                    spellListLevel = 6;
                SetEnemySpells(EnemyClassSpells[spellListLevel]);
            }

            // Chance of adding map
            DaggerfallLoot.RandomlyAddMap(mobileEnemy.MapChance, items);

            if (!string.IsNullOrEmpty(mobileEnemy.LootTableKey))
            {
                // Chance of adding potion
                DaggerfallLoot.RandomlyAddPotion(3, items);
                // Chance of adding potion recipe
                DaggerfallLoot.RandomlyAddPotionRecipe(2, items);
            }

            OnLootSpawned?.Invoke(this, new EnemyLootSpawnedEventArgs { MobileEnemy = mobileEnemy, EnemyCareer = career, Items = items });

            FillVitalSigns();
        }

        public static int RandomizeLevel(int startingLevel)
        {
            int minLevel = startingLevel - 3;
            int maxLevel = startingLevel + 3;
            int randomRoll = DFRandom.random_range_inclusive(1, 100);
            while (randomRoll <= 5 || randomRoll > 95)
            {                
                if (randomRoll <= 5) minLevel--;
                if (randomRoll > 95) maxLevel++;
                randomRoll = DFRandom.random_range_inclusive(1, 100);
            }
            int finalLevel = DFRandom.random_range_inclusive(minLevel, maxLevel);
            if (finalLevel < 1) finalLevel = 1;
            else if (finalLevel > 100) finalLevel = 100; // Just in case...

            return finalLevel;
        }

        public static bool MonsterHasEquipment(int careerIndex, out int variant)
        {
            variant = -1;
            switch ((MonsterCareers)careerIndex)
            {
                case MonsterCareers.Orc:
                    variant = 0;
                    return true;

                case MonsterCareers.AncientLich:
                case MonsterCareers.DaedraLord:
                case MonsterCareers.DaedraSeducer:
                case MonsterCareers.Daedroth:
                case MonsterCareers.Dremora:
                case MonsterCareers.FireDaedra:
                case MonsterCareers.FireDaemon:
                case MonsterCareers.FrostDaedra:
                case MonsterCareers.Gargoyle:
                case MonsterCareers.Giant:
                case MonsterCareers.Goblin:
                case MonsterCareers.Lich:
                case MonsterCareers.Lizardman:
                case MonsterCareers.LizardWarrior:
                case MonsterCareers.Minotaur:
                case MonsterCareers.Ogre:
                case MonsterCareers.OrcSergeant:
                case MonsterCareers.OrcShaman:
                case MonsterCareers.OrcWarlord:
                case MonsterCareers.SkeletalSoldier:
                case MonsterCareers.SkeletalWarrior:
                case MonsterCareers.Vampire:
                case MonsterCareers.VampireAncient:
                case MonsterCareers.Centaur:
                    variant = 1;
                    return true;

                default:
                    return false;
            }
        }

        public static int GetNewClassCareerIndex(int careerID)
        {
            switch (careerID)
            {
                case (int)ClassCareers.Spy:
                    return 18;
                default:
                    return 19;
            }
        }

        public static int GetSkillProficiencyBonus(DFCareer career, int skill)
        {
            if (career.PrimarySkill1 == (DFCareer.Skills)skill ||
                career.PrimarySkill2 == (DFCareer.Skills)skill ||
                career.PrimarySkill3 == (DFCareer.Skills)skill)
                return 30;
            if (career.MajorSkill1 == (DFCareer.Skills)skill ||
                career.MajorSkill2 == (DFCareer.Skills)skill ||
                career.MajorSkill3 == (DFCareer.Skills)skill)
                return 20;
            if (career.MinorSkill1 == (DFCareer.Skills)skill ||
                career.MinorSkill2 == (DFCareer.Skills)skill ||
                career.MinorSkill3 == (DFCareer.Skills)skill ||
                career.MinorSkill4 == (DFCareer.Skills)skill ||
                career.MinorSkill5 == (DFCareer.Skills)skill ||
                career.MinorSkill6 == (DFCareer.Skills)skill)
                return 10;
            return 0;
        }

        public void SetEnemyEquipment(int variant)
        {
            // Assign the enemies starting equipment.
            AssignEnemyEquipment(GameManager.Instance.PlayerEntity, this, variant);

            // Initialize armor values to 0 (no armor)
            for (int i = 0; i < ArmorValues.Length; i++)
            {
                ArmorValues[i] = 0;
            }
            // Calculate armor values from equipment
            for (int i = (int)Game.Items.EquipSlots.Head; i < (int)Game.Items.EquipSlots.Feet; i++)
            {
                Items.DaggerfallUnityItem item = ItemEquipTable.GetItem((Items.EquipSlots)i);
                if (item != null && item.ItemGroup == Game.Items.ItemGroups.Armor)
                {
                    UpdateEquippedArmorValues(item, true);
                }
            }

            if (entityType == EntityTypes.EnemyClass)
            {
                // Clamp to maximum armor value of 60. In classic this also applies for monsters.
                // Note: Classic sets the value to 60 if it is > 50, which seems like an oversight.
                for (int i = 0; i < ArmorValues.Length; i++)
                {
                    if (ArmorValues[i] > 60)
                    {
                        ArmorValues[i] = 60;
                    }
                }
            }
            else
            {
                // Note: In classic, the above applies for equipment-using monsters as well as enemy classes.
                // The resulting armor values are often 60. Due to the +40 to hit against monsters this makes
                // monsters with equipment very easy to hit, and 60 is a worse value than any value monsters
                // have in their definition. To avoid this, in DF Unity the equipment values are only used if
                // they are better than the value in the definition.
                for (int i = 0; i < ArmorValues.Length; i++)
                {
                    // TOCHECK: isn't this formula doing the opposite of what was intended to?
                    // How I read it, if the equipment has a better value than the standard monster value,
                    // the former is reduced to the standard; how I see it, it should be that, if the equipment
                    // has a lower value than the standard, it should be raised to the standard value.
                    if (ArmorValues[i] > (sbyte)(mobileEnemy.ArmorValue))
                    {
                        ArmorValues[i] = (sbyte)(mobileEnemy.ArmorValue);
                    }
                }
            }
        }

        public void SetEnemySpells(byte[] spellList)
        {
            // Enemies don't follow same rule as player for maximum spell points
            MaxMagicka = 10 * level + 100;
            currentMagicka = MaxMagicka;
            skills.SetPermanentSkillValue(DFCareer.Skills.Destruction, 80);
            skills.SetPermanentSkillValue(DFCareer.Skills.Restoration, 80);
            skills.SetPermanentSkillValue(DFCareer.Skills.Illusion, 80);
            skills.SetPermanentSkillValue(DFCareer.Skills.Alteration, 80);
            skills.SetPermanentSkillValue(DFCareer.Skills.Thaumaturgy, 80);
            skills.SetPermanentSkillValue(DFCareer.Skills.Mysticism, 80);

            // Add spells to enemy from standard list
            foreach (byte spellID in spellList)
            {
                SpellRecord.SpellRecordData spellData;
                GameManager.Instance.EntityEffectBroker.GetClassicSpellRecord(spellID, out spellData);
                if (spellData.index == -1)
                {
                    Debug.LogError("Failed to locate enemy spell in standard spells list.");
                    continue;
                }

                EffectBundleSettings bundle;
                if (!GameManager.Instance.EntityEffectBroker.ClassicSpellRecordDataToEffectBundleSettings(spellData, BundleTypes.Spell, out bundle))
                {
                    Debug.LogError("Failed to create effect bundle for enemy spell: " + spellData.spellName);
                    continue;
                }
                AddSpell(bundle);
            }
        }

        public DFCareer.EnemyGroups GetEnemyGroup()
        {
            switch (careerIndex)
            {
                case (int)MonsterCareers.Rat:
                case (int)MonsterCareers.GiantBat:
                case (int)MonsterCareers.GrizzlyBear:
                case (int)MonsterCareers.SabertoothTiger:
                case (int)MonsterCareers.Spider:
                case (int)MonsterCareers.Slaughterfish:
                case (int)MonsterCareers.GiantScorpion:
                case (int)MonsterCareers.Dragonling:
                case (int)MonsterCareers.Horse_Invalid:             // (grouped as undead in classic)
                case (int)MonsterCareers.Dragonling_Alternate:      // (grouped as undead in classic)
                case (int)MonsterCareers.Bat:
                case (int)MonsterCareers.Wolf:
                case (int)MonsterCareers.SnowWolf:
                case (int)MonsterCareers.Dog:
                case (int)MonsterCareers.BloodSpider:
                case (int)MonsterCareers.Boar:
                case (int)MonsterCareers.MountainLion:
                case (int)MonsterCareers.Mudcrab:
                    return DFCareer.EnemyGroups.Animals;
                case (int)MonsterCareers.Imp:
                case (int)MonsterCareers.Spriggan:
                case (int)MonsterCareers.Orc:
                case (int)MonsterCareers.Centaur:
                case (int)MonsterCareers.Werewolf:
                case (int)MonsterCareers.Nymph:
                case (int)MonsterCareers.OrcSergeant:
                case (int)MonsterCareers.Harpy:
                case (int)MonsterCareers.Wereboar:
                case (int)MonsterCareers.Giant:
                case (int)MonsterCareers.OrcShaman:
                case (int)MonsterCareers.Gargoyle:
                case (int)MonsterCareers.OrcWarlord:
                case (int)MonsterCareers.Dreugh:                    // (grouped as undead in classic)
                case (int)MonsterCareers.Lamia:                     // (grouped as undead in classic)
                case (int)MonsterCareers.Goblin:
                case (int)MonsterCareers.Homunculus:
                case (int)MonsterCareers.Lizardman:
                case (int)MonsterCareers.LizardWarrior:
                case (int)MonsterCareers.Medusa:
                case (int)MonsterCareers.Grotesque:
                case (int)MonsterCareers.MountainNymph:
                case (int)MonsterCareers.Minotaur:
                case (int)MonsterCareers.Troll:
                case (int)MonsterCareers.LandDreugh:
                case (int)MonsterCareers.Ogre:
                    return DFCareer.EnemyGroups.Humanoid;
                case (int)MonsterCareers.SkeletalWarrior:
                case (int)MonsterCareers.Zombie:                    // (grouped as animal in classic)
                case (int)MonsterCareers.Ghost:
                case (int)MonsterCareers.Mummy:
                case (int)MonsterCareers.Wraith:
                case (int)MonsterCareers.Vampire:
                case (int)MonsterCareers.VampireAncient:
                case (int)MonsterCareers.Lich:
                case (int)MonsterCareers.AncientLich:
                case (int)MonsterCareers.SkeletalSoldier:
                case (int)MonsterCareers.GloomWraith:
                case (int)MonsterCareers.FadedGhost:
                case (int)MonsterCareers.KingLysandus:
                case (int)MonsterCareers.Ghoul:
                case (int)MonsterCareers.DireGhoul:
                    return DFCareer.EnemyGroups.Undead;
                case (int)MonsterCareers.FrostDaedra:
                case (int)MonsterCareers.FireDaedra:
                case (int)MonsterCareers.Daedroth:
                case (int)MonsterCareers.DaedraSeducer:
                case (int)MonsterCareers.DaedraLord:
                case (int)MonsterCareers.HellHound:
                case (int)MonsterCareers.FireDaemon:
                case (int)MonsterCareers.Dremora:
                case (int)MonsterCareers.Scamp:
                    return DFCareer.EnemyGroups.Daedra;
                case (int)MonsterCareers.FireAtronach:
                case (int)MonsterCareers.IronAtronach:
                case (int)MonsterCareers.FleshAtronach:
                case (int)MonsterCareers.IceAtronach:
                case (int)MonsterCareers.IronGolem:
                case (int)MonsterCareers.Wisp:
                case (int)MonsterCareers.IceGolem:
                case (int)MonsterCareers.StoneGolem:
                case (int)MonsterCareers.CenturionSphere:
                case (int)MonsterCareers.SteamCenturion:
                    return DFCareer.EnemyGroups.None;

                default:
                    return DFCareer.EnemyGroups.None;
            }
        }

        public DFCareer.Skills GetLanguageSkill()
        {
            if (entityType == EntityTypes.EnemyClass)
            {
                switch (careerIndex)
                {   // BCHG: classic uses Ettiquette for all
                    case (int)ClassCareers.Burglar:
                    case (int)ClassCareers.Rogue:
                    case (int)ClassCareers.Acrobat:
                    case (int)ClassCareers.Thief:
                    case (int)ClassCareers.Assassin:
                    case (int)ClassCareers.Nightblade:
                    case (int)ClassCareers.Druid:
                    case (int)ClassCareers.RogueRider:
                    case (int)ClassCareers.NecroAcolyte:
                    case (int)ClassCareers.RogueDruid:
                    case (int)ClassCareers.BountyHunter:
                    case (int)ClassCareers.ThiefRider:
                    case (int)ClassCareers.NecroGlaive:
                    case (int)ClassCareers.NecroAssassin:
                    case (int)ClassCareers.DarkBrotherhood:
                    case (int)ClassCareers.WitchDefender:
                        return DFCareer.Skills.Streetwise;
                    default:
                        return DFCareer.Skills.Etiquette;
                }
            }

            switch (careerIndex)
            {
                case (int)MonsterCareers.Orc:
                case (int)MonsterCareers.OrcSergeant:
                case (int)MonsterCareers.OrcShaman:
                case (int)MonsterCareers.OrcWarlord:
                case (int)MonsterCareers.Goblin:
                case (int)MonsterCareers.Troll:
                    return DFCareer.Skills.Orcish;

                case (int)MonsterCareers.Harpy:
                    return DFCareer.Skills.Harpy;

                case (int)MonsterCareers.Giant:
                case (int)MonsterCareers.Gargoyle:
                case (int)MonsterCareers.Grotesque:
                case (int)MonsterCareers.Ogre:
                    return DFCareer.Skills.Giantish;

                case (int)MonsterCareers.Dragonling:
                case (int)MonsterCareers.Dragonling_Alternate:
                    return DFCareer.Skills.Dragonish;

                case (int)MonsterCareers.Nymph:
                case (int)MonsterCareers.Lamia:
                case (int)MonsterCareers.Medusa:
                case (int)MonsterCareers.MountainNymph:
                    return DFCareer.Skills.Nymph;

                case (int)MonsterCareers.FrostDaedra:
                case (int)MonsterCareers.FireDaedra:
                case (int)MonsterCareers.Daedroth:
                case (int)MonsterCareers.DaedraSeducer:
                case (int)MonsterCareers.DaedraLord:
                case (int)MonsterCareers.HellHound:
                case (int)MonsterCareers.FireDaemon:
                case (int)MonsterCareers.Dremora:
                case (int)MonsterCareers.Scamp:
                    return DFCareer.Skills.Daedric;

                case (int)MonsterCareers.Spriggan:
                case (int)MonsterCareers.Wisp:
                    return DFCareer.Skills.Spriggan;

                case (int)MonsterCareers.Centaur:
                case (int)MonsterCareers.Minotaur:
                    return DFCareer.Skills.Centaurian;

                case (int)MonsterCareers.Imp:
                case (int)MonsterCareers.Dreugh:
                case (int)MonsterCareers.Homunculus:
                case (int)MonsterCareers.LandDreugh:
                    return DFCareer.Skills.Impish;

                case (int)MonsterCareers.Vampire:
                case (int)MonsterCareers.VampireAncient:
                case (int)MonsterCareers.Lich:
                case (int)MonsterCareers.AncientLich:
                    return DFCareer.Skills.Etiquette;

                default:
                    return DFCareer.Skills.None;
            }
        }

        public int GetWeightInClassicUnits()
        {
            int itemWeightsClassic = (int)(Items.GetWeight() * 4);
            int baseWeight;

            if (entityType == EntityTypes.EnemyMonster)
                baseWeight = mobileEnemy.Weight;
            else if (mobileEnemy.Gender == MobileGender.Female)
                baseWeight = 240;
            else
                baseWeight = 350;

            return itemWeightsClassic + baseWeight;
        }

        #endregion
    }
}
