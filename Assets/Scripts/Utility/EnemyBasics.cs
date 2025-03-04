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

using System;
using System.Collections.Generic;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Items;
using UnityEngine;

namespace DaggerfallWorkshop.Utility
{
    /// <summary>
    /// Static definitions for enemies and their animations.
    /// Remaining data is read from MONSTER.BSA.
    /// </summary>
    public static class EnemyBasics
    {
        #region Enemy Animations

        // Speeds in frames-per-second
        public static int MoveAnimSpeed = 6;
        public static int FlyAnimSpeed = 10;
        public static int PrimaryAttackAnimSpeed = 10;
        public static int HurtAnimSpeed = 4;
        public static int IdleAnimSpeed = 4;
        public static int RangedAttack1AnimSpeed = 10;
        public static int RangedAttack2AnimSpeed = 10;

        // Move animations (double as idle animations for swimming and flying enemies, and enemies without idle animations)
        public static MobileAnimation[] MoveAnims = new MobileAnimation[]
        {
            new MobileAnimation() {Record = 0, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 1, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south-west
            new MobileAnimation() {Record = 2, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing west
            new MobileAnimation() {Record = 3, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing north-west
            new MobileAnimation() {Record = 4, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing north (back facing player)
            new MobileAnimation() {Record = 3, FramePerSecond = MoveAnimSpeed, FlipLeftRight = true},              // Facing north-east
            new MobileAnimation() {Record = 2, FramePerSecond = MoveAnimSpeed, FlipLeftRight = true},              // Facing east
            new MobileAnimation() {Record = 1, FramePerSecond = MoveAnimSpeed, FlipLeftRight = true},              // Facing south-east
        };

        // PrimaryAttack animations
        public static MobileAnimation[] PrimaryAttackAnims = new MobileAnimation[]
        {
            new MobileAnimation() {Record = 5, FramePerSecond = PrimaryAttackAnimSpeed, FlipLeftRight = false},    // Facing south (front facing player)
            new MobileAnimation() {Record = 6, FramePerSecond = PrimaryAttackAnimSpeed, FlipLeftRight = false},    // Facing south-west
            new MobileAnimation() {Record = 7, FramePerSecond = PrimaryAttackAnimSpeed, FlipLeftRight = false},    // Facing west
            new MobileAnimation() {Record = 8, FramePerSecond = PrimaryAttackAnimSpeed, FlipLeftRight = false},    // Facing north-west
            new MobileAnimation() {Record = 9, FramePerSecond = PrimaryAttackAnimSpeed, FlipLeftRight = false},    // Facing north (back facing player)
            new MobileAnimation() {Record = 8, FramePerSecond = PrimaryAttackAnimSpeed, FlipLeftRight = true},     // Facing north-east
            new MobileAnimation() {Record = 7, FramePerSecond = PrimaryAttackAnimSpeed, FlipLeftRight = true},     // Facing east
            new MobileAnimation() {Record = 6, FramePerSecond = PrimaryAttackAnimSpeed, FlipLeftRight = true},     // Facing south-east
        };

        // Hurt animations
        public static MobileAnimation[] HurtAnims = new MobileAnimation[]
        {
            new MobileAnimation() {Record = 10, FramePerSecond = HurtAnimSpeed, FlipLeftRight = false},            // Facing south (front facing player)
            new MobileAnimation() {Record = 11, FramePerSecond = HurtAnimSpeed, FlipLeftRight = false},            // Facing south-west
            new MobileAnimation() {Record = 12, FramePerSecond = HurtAnimSpeed, FlipLeftRight = false},            // Facing west
            new MobileAnimation() {Record = 13, FramePerSecond = HurtAnimSpeed, FlipLeftRight = false},            // Facing north-west
            new MobileAnimation() {Record = 14, FramePerSecond = HurtAnimSpeed, FlipLeftRight = false},            // Facing north (back facing player)
            new MobileAnimation() {Record = 13, FramePerSecond = HurtAnimSpeed, FlipLeftRight = true},             // Facing north-east
            new MobileAnimation() {Record = 12, FramePerSecond = HurtAnimSpeed, FlipLeftRight = true},             // Facing east
            new MobileAnimation() {Record = 11, FramePerSecond = HurtAnimSpeed, FlipLeftRight = true},             // Facing south-east
        };

        // Idle animations (most monsters have a static idle sprite)
        public static MobileAnimation[] IdleAnims = new MobileAnimation[]
        {
            new MobileAnimation() {Record = 15, FramePerSecond = IdleAnimSpeed, FlipLeftRight = false},            // Facing south (front facing player)
            new MobileAnimation() {Record = 16, FramePerSecond = IdleAnimSpeed, FlipLeftRight = false},            // Facing south-west
            new MobileAnimation() {Record = 17, FramePerSecond = IdleAnimSpeed, FlipLeftRight = false},            // Facing west
            new MobileAnimation() {Record = 18, FramePerSecond = IdleAnimSpeed, FlipLeftRight = false},            // Facing north-west
            new MobileAnimation() {Record = 19, FramePerSecond = IdleAnimSpeed, FlipLeftRight = false},            // Facing north (back facing player)
            new MobileAnimation() {Record = 18, FramePerSecond = IdleAnimSpeed, FlipLeftRight = true},             // Facing north-east
            new MobileAnimation() {Record = 17, FramePerSecond = IdleAnimSpeed, FlipLeftRight = true},             // Facing east
            new MobileAnimation() {Record = 16, FramePerSecond = IdleAnimSpeed, FlipLeftRight = true},             // Facing south-east
        };

        // RangedAttack1 animations (humanoid mobiles only)
        public static MobileAnimation[] RangedAttack1Anims = new MobileAnimation[]
        {
            new MobileAnimation() {Record = 20, FramePerSecond = RangedAttack1AnimSpeed, FlipLeftRight = false},   // Facing south (front facing player)
            new MobileAnimation() {Record = 21, FramePerSecond = RangedAttack1AnimSpeed, FlipLeftRight = false},   // Facing south-west
            new MobileAnimation() {Record = 22, FramePerSecond = RangedAttack1AnimSpeed, FlipLeftRight = false},   // Facing west
            new MobileAnimation() {Record = 23, FramePerSecond = RangedAttack1AnimSpeed, FlipLeftRight = false},   // Facing north-west
            new MobileAnimation() {Record = 24, FramePerSecond = RangedAttack1AnimSpeed, FlipLeftRight = false},   // Facing north (back facing player)
            new MobileAnimation() {Record = 23, FramePerSecond = RangedAttack1AnimSpeed, FlipLeftRight = true},    // Facing north-east
            new MobileAnimation() {Record = 22, FramePerSecond = RangedAttack1AnimSpeed, FlipLeftRight = true},    // Facing east
            new MobileAnimation() {Record = 21, FramePerSecond = RangedAttack1AnimSpeed, FlipLeftRight = true},    // Facing south-east
        };

        // RangedAttack2 animations (475, 489, 490 humanoid mobiles only)
        public static MobileAnimation[] RangedAttack2Anims = new MobileAnimation[]
        {
            new MobileAnimation() {Record = 25, FramePerSecond = RangedAttack2AnimSpeed, FlipLeftRight = false},   // Facing south (front facing player)
            new MobileAnimation() {Record = 26, FramePerSecond = RangedAttack2AnimSpeed, FlipLeftRight = false},   // Facing south-west
            new MobileAnimation() {Record = 27, FramePerSecond = RangedAttack2AnimSpeed, FlipLeftRight = false},   // Facing west
            new MobileAnimation() {Record = 28, FramePerSecond = RangedAttack2AnimSpeed, FlipLeftRight = false},   // Facing north-west
            new MobileAnimation() {Record = 29, FramePerSecond = RangedAttack2AnimSpeed, FlipLeftRight = false},   // Facing north (back facing player)
            new MobileAnimation() {Record = 28, FramePerSecond = RangedAttack2AnimSpeed, FlipLeftRight = true},    // Facing north-east
            new MobileAnimation() {Record = 27, FramePerSecond = RangedAttack2AnimSpeed, FlipLeftRight = true},    // Facing east
            new MobileAnimation() {Record = 26, FramePerSecond = RangedAttack2AnimSpeed, FlipLeftRight = true},    // Facing south-east
        };

        // Female thief idle animations
        public static MobileAnimation[] FemaleThiefIdleAnims = new MobileAnimation[]
        {
            new MobileAnimation() {Record = 15, FramePerSecond = IdleAnimSpeed, FlipLeftRight = false},            // Facing south (front facing player)
            new MobileAnimation() {Record = 11, FramePerSecond = IdleAnimSpeed, FlipLeftRight = false},            // Facing south-west
            new MobileAnimation() {Record = 17, FramePerSecond = IdleAnimSpeed, FlipLeftRight = false},            // Facing west
            new MobileAnimation() {Record = 18, FramePerSecond = IdleAnimSpeed, FlipLeftRight = false},            // Facing north-west
            new MobileAnimation() {Record = 19, FramePerSecond = IdleAnimSpeed, FlipLeftRight = false},            // Facing north (back facing player)
            new MobileAnimation() {Record = 18, FramePerSecond = IdleAnimSpeed, FlipLeftRight = true},             // Facing north-east
            new MobileAnimation() {Record = 17, FramePerSecond = IdleAnimSpeed, FlipLeftRight = true},             // Facing east
            new MobileAnimation() {Record = 11, FramePerSecond = IdleAnimSpeed, FlipLeftRight = true},             // Facing south-east
        };

        // Rat idle animations
        public static MobileAnimation[] RatIdleAnims = new MobileAnimation[]
        {
            new MobileAnimation() {Record = 15, FramePerSecond = IdleAnimSpeed, FlipLeftRight = false},            // Facing south (front facing player)
            new MobileAnimation() {Record = 16, FramePerSecond = IdleAnimSpeed, FlipLeftRight = true},             // Facing south-west
            new MobileAnimation() {Record = 17, FramePerSecond = IdleAnimSpeed, FlipLeftRight = true},             // Facing west
            new MobileAnimation() {Record = 18, FramePerSecond = IdleAnimSpeed, FlipLeftRight = true},             // Facing north-west
            new MobileAnimation() {Record = 19, FramePerSecond = IdleAnimSpeed, FlipLeftRight = false},            // Facing north (back facing player)
            new MobileAnimation() {Record = 18, FramePerSecond = IdleAnimSpeed, FlipLeftRight = false},            // Facing north-east
            new MobileAnimation() {Record = 17, FramePerSecond = IdleAnimSpeed, FlipLeftRight = false},            // Facing east
            new MobileAnimation() {Record = 16, FramePerSecond = IdleAnimSpeed, FlipLeftRight = false},            // Facing south-east
        };

        // Wraith and ghost idle/move animations
        public static MobileAnimation[] GhostWraithMoveAnims = new MobileAnimation[]
        {
            new MobileAnimation() {Record = 0, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 1, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south-west
            new MobileAnimation() {Record = 2, FramePerSecond = MoveAnimSpeed, FlipLeftRight = true},              // Facing west
            new MobileAnimation() {Record = 3, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing north-west
            new MobileAnimation() {Record = 4, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing north (back facing player)
            new MobileAnimation() {Record = 3, FramePerSecond = MoveAnimSpeed, FlipLeftRight = true},              // Facing north-east
            new MobileAnimation() {Record = 2, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing east
            new MobileAnimation() {Record = 1, FramePerSecond = MoveAnimSpeed, FlipLeftRight = true},              // Facing south-east
        };

        // Ghost and Wraith attack animations
        public static MobileAnimation[] GhostWraithAttackAnims = new MobileAnimation[]
        {
            new MobileAnimation() {Record = 5, FramePerSecond = PrimaryAttackAnimSpeed, FlipLeftRight = false},    // Facing south (front facing player)
            new MobileAnimation() {Record = 6, FramePerSecond = PrimaryAttackAnimSpeed, FlipLeftRight = false},    // Facing south-west
            new MobileAnimation() {Record = 7, FramePerSecond = PrimaryAttackAnimSpeed, FlipLeftRight = true},     // Facing west
            new MobileAnimation() {Record = 8, FramePerSecond = PrimaryAttackAnimSpeed, FlipLeftRight = false},    // Facing north-west
            new MobileAnimation() {Record = 9, FramePerSecond = PrimaryAttackAnimSpeed, FlipLeftRight = false},    // Facing north (back facing player)
            new MobileAnimation() {Record = 8, FramePerSecond = PrimaryAttackAnimSpeed, FlipLeftRight = true},     // Facing north-east
            new MobileAnimation() {Record = 7, FramePerSecond = PrimaryAttackAnimSpeed, FlipLeftRight = false},    // Facing east
            new MobileAnimation() {Record = 6, FramePerSecond = PrimaryAttackAnimSpeed, FlipLeftRight = true},     // Facing south-east
        };

        // Seducer special animations - has player-facing orientation only
        public static MobileAnimation[] SeducerTransform1Anims = new MobileAnimation[]
        {
            new MobileAnimation() {Record = 23, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 23, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 23, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 23, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 23, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 23, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 23, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 23, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
        };
        public static MobileAnimation[] SeducerTransform2Anims = new MobileAnimation[]
        {
            new MobileAnimation() {Record = 22, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 22, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 22, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 22, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 22, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 22, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 22, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 22, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
        };
        public static MobileAnimation[] SeducerIdleMoveAnims = new MobileAnimation[]
        {
            new MobileAnimation() {Record = 21, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 21, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 21, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 21, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 21, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 21, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 21, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 21, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
        };
        public static MobileAnimation[] SeducerAttackAnims = new MobileAnimation[]
        {
            new MobileAnimation() {Record = 20, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 20, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 20, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 20, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 20, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 20, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 20, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
            new MobileAnimation() {Record = 20, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false},             // Facing south (front facing player)
        };

        // Slaughterfish special idle/move animation - needs to bounce back and forth between frame 0-N rather than loop
        // Move animations (double as idle animations for swimming and flying enemies, and enemies without idle animations)
        public static MobileAnimation[] SlaughterfishMoveAnims = new MobileAnimation[]
        {
            new MobileAnimation() {Record = 0, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false, BounceAnim = true},   // Facing south (front facing player)
            new MobileAnimation() {Record = 1, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false, BounceAnim = true},   // Facing south-west
            new MobileAnimation() {Record = 2, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false, BounceAnim = true},   // Facing west
            new MobileAnimation() {Record = 3, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false, BounceAnim = true},   // Facing north-west
            new MobileAnimation() {Record = 4, FramePerSecond = MoveAnimSpeed, FlipLeftRight = false, BounceAnim = true},   // Facing north (back facing player)
            new MobileAnimation() {Record = 3, FramePerSecond = MoveAnimSpeed, FlipLeftRight = true, BounceAnim = true},    // Facing north-east
            new MobileAnimation() {Record = 2, FramePerSecond = MoveAnimSpeed, FlipLeftRight = true, BounceAnim = true},    // Facing east
            new MobileAnimation() {Record = 1, FramePerSecond = MoveAnimSpeed, FlipLeftRight = true, BounceAnim = true},    // Facing south-east
        };

        #endregion

        #region Enemy Definitions

        // Defines additional data for known enemy types
        // Fills in the blanks where source of data in game files is unknown
        // Suspect at least some of this data is also hard-coded in Daggerfall
        public static MobileEnemy[] Enemies = new MobileEnemy[]
        {
            // Rat
            new MobileEnemy()
            {
                ID = 0,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Animal,
                MaleTexture = 255,
                FemaleTexture = 255,
                CorpseTexture = CorpseTexture(401, 1),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                MoveSound = (int)SoundClips.EnemyRatMove,
                BarkSound = (int)SoundClips.EnemyRatBark,
                AttackSound = (int)SoundClips.EnemyRatAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 1,
                MaxDamage = 4,
                MinHealth = 9,
                MaxHealth = 16,
                Level = 1,
                ArmorValue = 0, // 6
                ParrySounds = false,
                MapChance = 0,
                Weight = 2,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, -1, 3, 4, 5 },
                Team = MobileTeams.Vermin,
            },

            // Imp
            new MobileEnemy()
            {
                ID = 1,
                Behaviour = MobileBehaviour.Flying,
                Affinity = MobileAffinity.Darkness,
                MaleTexture = 256,
                FemaleTexture = 256,
                CorpseTexture = CorpseTexture(406, 5),
                HasIdle = false,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyImpMove,
                BarkSound = (int)SoundClips.EnemyImpBark,
                AttackSound = (int)SoundClips.EnemyImpAttack,
                MinMetalToHit = MaterialTypes.Steel,
                MinDamage = 2,
                MaxDamage = 15,
                MinHealth = 11,
                MaxHealth = 18,
                Level = 2,
                ArmorValue = 2, // 3
                ParrySounds = false,
                MapChance = 1,
                Weight = 40,
                SeesThroughInvisibility = true,
                LootTableKey = "D",
                SoulPts = 1000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, -1, 3, 1 },
                SpellAnimFrames = new int[] { 0, 1, 2, 3, 1 },
                Team = MobileTeams.Magic,
            },

            // Spriggan
            new MobileEnemy()
            {
                ID = 2,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Daylight,
                MaleTexture = 257,
                FemaleTexture = 257,
                CorpseTexture = CorpseTexture(406, 3),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemySprigganMove,
                BarkSound = (int)SoundClips.EnemySprigganBark,
                AttackSound = (int)SoundClips.EnemySprigganAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 1,
                MaxDamage = 10, // 8
                MinDamage2 = 1,
                MaxDamage2 = 8,
                MinDamage3 = 1,
                MaxDamage3 = 10,
                MinHealth = 12,
                MaxHealth = 26,
                Level = 3,
                ArmorValue = 6, // -4
                ParrySounds = false,
                MapChance = 0,
                Weight = 240,
                LootTableKey = "B",
                SoulPts = 1000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, 3, 3 },
                Team = MobileTeams.Nymphs,  // Spriggans
            },

            // Giant Bat
            new MobileEnemy()
            {
                ID = 3,
                Behaviour = MobileBehaviour.Flying,
                Affinity = MobileAffinity.Animal,
                MaleTexture = 258,
                FemaleTexture = 258,
                CorpseTexture = CorpseTexture(401, 0),
                HasIdle = false,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                MoveSound = (int)SoundClips.EnemyGiantBatMove,
                BarkSound = (int)SoundClips.EnemyGiantBatBark,
                AttackSound = (int)SoundClips.EnemyGiantBatAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 2,
                MaxDamage = 12,
                MinHealth = 12,
                MaxHealth = 26,
                Level = 3,
                ArmorValue = 0, //6
                ParrySounds = false,
                MapChance = 0,
                Weight = 80,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3 },
                Team = MobileTeams.Vermin,
            },

            // Grizzly Bear
            new MobileEnemy()
            {
                ID = 4,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Animal,
                MaleTexture = 457, // 259
                FemaleTexture = 457,    // 259
                CorpseTexture = CorpseTexture(401, 2),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                MoveSound = (int)SoundClips.EnemyBearMove,
                BarkSound = (int)SoundClips.EnemyBearBark,
                AttackSound = (int)SoundClips.EnemyBearAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 1,
                MaxDamage = 12, // 8
                MinDamage2 = 1,
                MaxDamage2 = 8,
                MinDamage3 = 1,
                MaxDamage3 = 10,
                MinHealth = 13,
                MaxHealth = 34,
                Level = 4,
                ArmorValue = 3, // 6
                ParrySounds = false,
                MapChance = 0,
                Weight = 1000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, -1, 3, 0 },
                Team = MobileTeams.Bears,
            },

            // Sabertooth Tiger
            new MobileEnemy()
            {
                ID = 5,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Animal,
                MaleTexture = 260,
                FemaleTexture = 260,
                CorpseTexture = CorpseTexture(401, 3),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                MoveSound = (int)SoundClips.EnemyTigerMove,
                BarkSound = (int)SoundClips.EnemyTigerBark,
                AttackSound = (int)SoundClips.EnemyTigerAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 2,
                MaxDamage = 14,
                MinDamage2 = 0, // 1,
                MaxDamage2 = 0, // 10,
                MinDamage3 = 0, // 3,
                MaxDamage3 = 0, // 15,
                MinHealth = 13,
                MaxHealth = 34,
                Level = 4,
                ArmorValue = 1, // 6
                ParrySounds = false,
                MapChance = 0,
                Weight = 1000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, -1, 3, 4, 5 },
                Team = MobileTeams.Tigers,
            },

            // Spider
            new MobileEnemy()
            {
                ID = 6,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Animal,
                MaleTexture = 261,
                FemaleTexture = 261,
                CorpseTexture = CorpseTexture(401, 4),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                MoveSound = (int)SoundClips.EnemySpiderMove,
                BarkSound = (int)SoundClips.EnemySpiderBark,
                AttackSound = (int)SoundClips.EnemySpiderAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 5,
                MaxDamage = 15,
                MinHealth = 13,
                MaxHealth = 34,
                Level = 4,
                ArmorValue = 2, // 5
                ParrySounds = false,
                MapChance = 0,
                Weight = 400,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, 4, 5 },
                Team = MobileTeams.Spiders,
            },

            // Orc
            new MobileEnemy()
            {
                ID = 7,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Darkness,
                MaleTexture = 262,
                FemaleTexture = 262,
                CorpseTexture = CorpseTexture(96, 2),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyOrcMove,
                BarkSound = (int)SoundClips.EnemyOrcBark,
                AttackSound = (int)SoundClips.EnemyOrcAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 1,
                MaxDamage = 6,
                MinHealth = 13,
                MaxHealth = 34,
                Level = 5,
                ArmorValue = 0, // 7
                ParrySounds = true,
                MapChance = 0,
                Weight = 600,
                LootTableKey = "A",
                SoulPts = 1000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, -1, 3, 4, -1, 5, 0 },
                ChanceForAttack2 = 50,
                PrimaryAttackAnimFrames2 = new int[] { 4, -1, 5, 0 },
                Team = MobileTeams.Orcs,
            },

            // Centaur
            new MobileEnemy()
            {
                ID = 8,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Daylight,
                MaleTexture = 263,
                FemaleTexture = 263,
                CorpseTexture = CorpseTexture(406, 0),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyCentaurMove,
                BarkSound = (int)SoundClips.EnemyCentaurBark,
                AttackSound = (int)SoundClips.EnemyCentaurAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 5,
                MaxDamage = 15,
                MinHealth = 14,
                MaxHealth = 46,
                Level = 5,
                ArmorValue = 1, // 6
                ParrySounds = true,
                MapChance = 1,
                Weight = 1200,
                LootTableKey = "C",
                SoulPts = 3000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 1, 1, 2, -1, 3, 3, 4 },
                ChanceForAttack2 = 50,
                PrimaryAttackAnimFrames2 = new int[] { 0, 1, 1, 1, 2, -1, 3, 3, 2, 1, 1, -1, 2, 3, 3, 4 },
                Team = MobileTeams.Centaurs,
            },

            // Werewolf
            new MobileEnemy()
            {
                ID = 9,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Darkness,
                MaleTexture = 264,
                FemaleTexture = 264,
                CorpseTexture = CorpseTexture(96, 5),
                HasIdle = false,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyWerewolfMove,
                BarkSound = (int)SoundClips.EnemyWerewolfBark,
                AttackSound = (int)SoundClips.EnemyWerewolfAttack,
                MinMetalToHit = MaterialTypes.Silver,
                MinDamage = 2,
                MaxDamage = 16,
                MinDamage2 = 0, // 1,
                MaxDamage2 = 0, // 10,
                MinDamage3 = 0, // 2,
                MaxDamage3 = 0, // 12,
                MinHealth = 17,
                MaxHealth = 66,
                Level = 9,
                ArmorValue = 3, // 6
                MapChance = 0,
                ParrySounds = false,
                Weight = 480,
                SoulPts = 1000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, -1, 2 },
                Team = MobileTeams.Werecreatures,
            },

            // Nymph
            new MobileEnemy()
            {
                ID = 10,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Daylight,
                MaleTexture = 265,
                FemaleTexture = 265,
                CorpseTexture = CorpseTexture(406, 2),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyNymphMove,
                BarkSound = (int)SoundClips.EnemyNymphBark,
                AttackSound = (int)SoundClips.EnemyNymphAttack,
                MinMetalToHit = MaterialTypes.Silver,
                MinDamage = 1,
                MaxDamage = 5,
                MinHealth = 15,
                MaxHealth = 50,
                Level = 6,
                ArmorValue = 0, // 0
                ParrySounds = false,
                MapChance = 1,
                Weight = 200,
                LootTableKey = "B", // C
                SoulPts = 10000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, 3, 4, -1, 5 },
                Team = MobileTeams.Nymphs,
            },

            // Slaughterfish
            new MobileEnemy()
            {
                ID = 11,
                Behaviour = MobileBehaviour.Aquatic,
                Affinity = MobileAffinity.Water,
                MaleTexture = 266,
                FemaleTexture = 266,
                CorpseTexture = CorpseTexture(305, 1),
                HasIdle = false,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                MoveSound = (int)SoundClips.EnemyEelMove,
                BarkSound = (int)SoundClips.EnemyEelBark,
                AttackSound = (int)SoundClips.EnemyEelAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 2,
                MaxDamage = 12,
                MinHealth = 15,
                MaxHealth = 50,
                Level = 7,
                ArmorValue = 1, // 6
                ParrySounds = false,
                MapChance = 0,
                Weight = 400,
                PrimaryAttackAnimFrames = new int[] { 0, -1, 1, 2, 3, 4, -1, 5, 0 },
                ChanceForAttack2 = 33,
                PrimaryAttackAnimFrames2 = new int[] { 0, 3, -1, 5, 4, 3, 3, -1, 5, 4, 3, -1, 5, 0 },
                ChanceForAttack3 = 33,
                PrimaryAttackAnimFrames3 = new int[] { 0, 3, -1, 5, 0 },
                Team = MobileTeams.Aquatic,
            },

            // Orc Sergeant
            new MobileEnemy()
            {
                ID = 12,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Darkness,
                MaleTexture = 267,
                FemaleTexture = 267,
                CorpseTexture = CorpseTexture(96, 2),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyOrcSergeantMove,
                BarkSound = (int)SoundClips.EnemyOrcSergeantBark,
                AttackSound = (int)SoundClips.EnemyOrcSergeantAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 5,
                MaxDamage = 15,
                MinHealth = 15,
                MaxHealth = 50,
                Level = 7,
                ArmorValue = 0, // 5
                ParrySounds = true,
                MapChance = 1,
                Weight = 600,
                LootTableKey = "A",
                SoulPts = 1000,
                PrimaryAttackAnimFrames = new int[] { 0, -1, 1, 2, 3, 4, -1, 5, 0 },
                ChanceForAttack2 = 50,
                PrimaryAttackAnimFrames2 = new int[] { 5, 4, 3, -1, 2, 1, 0 },
                Team = MobileTeams.Orcs,
            },

            // Harpy
            new MobileEnemy()
            {
                ID = 13,
                Behaviour = MobileBehaviour.Flying,
                Affinity = MobileAffinity.Daylight,
                MaleTexture = 268,
                FemaleTexture = 268,
                CorpseTexture = CorpseTexture(406, 4),
                HasIdle = false,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyHarpyMove,
                BarkSound = (int)SoundClips.EnemyHarpyBark,
                AttackSound = (int)SoundClips.EnemyHarpyAttack,
                MinMetalToHit = MaterialTypes.Dwarven,
                MinDamage = 5,
                MaxDamage = 15,
                MinHealth = 16,
                MaxHealth = 85,
                Level = 8,
                ArmorValue = 1, // 2
                ParrySounds = false,
                MapChance = 0,
                Weight = 200,
                LootTableKey = "D",
                SoulPts = 3000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, -1, 3 },
                Team = MobileTeams.Harpies,
            },

            // Wereboar
            new MobileEnemy()
            {
                ID = 14,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Darkness,
                MaleTexture = 269,
                FemaleTexture = 269,
                CorpseTexture = CorpseTexture(96, 5),
                HasIdle = false,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyWereboarMove,
                BarkSound = (int)SoundClips.EnemyWereboarBark,
                AttackSound = (int)SoundClips.EnemyWereboarAttack,
                MinMetalToHit = MaterialTypes.Silver,
                MinDamage = 15,
                MaxDamage = 35,
                MinDamage2 = 0, // 2,
                MaxDamage2 = 0, // 12,
                MinDamage3 = 0, // 5,
                MaxDamage3 = 0, // 15,
                MinHealth = 17,
                MaxHealth = 66,
                Level = 11,
                ArmorValue = 5, // 3
                MapChance = 0,
                ParrySounds = false,
                Weight = 560,
                SoulPts = 1000,
                PrimaryAttackAnimFrames = new int[] { 0, -1, 1, 2, 2 },
                Team = MobileTeams.Werecreatures,
            },

            // Skeletal Warrior
            new MobileEnemy()
            {
                ID = 15,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Skeletal,
                MaleTexture = 270,
                FemaleTexture = 270,
                CorpseTexture = CorpseTexture(306, 1),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                BloodIndex = 2,
                MoveSound = (int)SoundClips.EnemySkeletonMove,
                BarkSound = (int)SoundClips.EnemySkeletonBark,
                AttackSound = (int)SoundClips.EnemySkeletonAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 5,
                MaxDamage = 15,
                MinHealth = 17,
                MaxHealth = 66,
                Level = 9,
                ArmorValue = 1, // 0
                ParrySounds = true,
                MapChance = 1,
                Weight = 80,
                SeesThroughInvisibility = true,
                LootTableKey = "H",
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, 3, -1, 4, 5 },
                Team = MobileTeams.Undead,
            },

            // Giant
            new MobileEnemy()
            {
                ID = 16,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Daylight,
                MaleTexture = 271,
                FemaleTexture = 271,
                CorpseTexture = CorpseTexture(406, 1),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyGiantMove,
                BarkSound = (int)SoundClips.EnemyGiantBark,
                AttackSound = (int)SoundClips.EnemyGiantAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 10,
                MaxDamage = 30,
                MinHealth = 18,
                MaxHealth = 74,
                Level = 10,
                ArmorValue = 3, // 3
                ParrySounds = false,
                MapChance = 1,
                LootTableKey = "E", // F
                Weight = 3000,
                SoulPts = 3000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, 4, -1, 5 },
                Team = MobileTeams.Giants,
            },

            // Zombie
            new MobileEnemy()
            {
                ID = 17,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Undead,
                MaleTexture = 272,
                FemaleTexture = 272,
                CorpseTexture = CorpseTexture(306, 4),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyZombieMove,
                BarkSound = (int)SoundClips.EnemyZombieBark,
                AttackSound = (int)SoundClips.EnemyZombieAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 5,  // 15
                MaxDamage = 15, // 50
                MinHealth = 52,
                MaxHealth = 66,
                Level = 10,
                ArmorValue = 1, // 0
                ParrySounds = false,
                MapChance = 1,
                Weight = 4000,
                LootTableKey = "G",
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, -1, 3, 4 },
                ChanceForAttack2 = 50,
                PrimaryAttackAnimFrames2 = new int[] { 0, 2, -1, 3, 4 },
                Team = MobileTeams.Undead,
            },

            // Ghost
            new MobileEnemy()
            {
                ID = 18,
                Behaviour = MobileBehaviour.Spectral,
                Affinity = MobileAffinity.Undead,
                MaleTexture = 273,
                FemaleTexture = 273,
                CorpseTexture = CorpseTexture(306, 0),
                HasIdle = false,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                BloodIndex = 2,
                MoveSound = (int)SoundClips.EnemyGhostMove,
                BarkSound = (int)SoundClips.EnemyGhostBark,
                AttackSound = (int)SoundClips.EnemyGhostAttack,
                MinMetalToHit = MaterialTypes.Silver,
                MinDamage = 10,
                MaxDamage = 35,
                MinHealth = 17,
                MaxHealth = 66,
                Level = 11,
                ArmorValue = 3, // 0
                ParrySounds = false,
                MapChance = 1,
                Weight = 0,
                SeesThroughInvisibility = true,
                LootTableKey = "I",
                NoShadow = true,
                SoulPts = 30000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3 },
                SpellAnimFrames = new int[] { 0, 0, 0, 0, 0, 0 },
                Team = MobileTeams.Undead,
            },

            // Mummy
            new MobileEnemy()
            {
                ID = 19,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Undead,
                MaleTexture = 274,
                FemaleTexture = 274,
                CorpseTexture = CorpseTexture(306, 5),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                BloodIndex = 2,
                MoveSound = (int)SoundClips.EnemyMummyMove,
                BarkSound = (int)SoundClips.EnemyMummyBark,
                AttackSound = (int)SoundClips.EnemyMummyAttack,
                MinMetalToHit = MaterialTypes.Silver,
                MinDamage = 5,
                MaxDamage = 15,
                MinHealth = 17,
                MaxHealth = 66,
                Level = 11,
                ArmorValue = 2, // 2
                ParrySounds = false,
                MapChance = 1,
                Weight = 300,
                SeesThroughInvisibility = true,
                LootTableKey = "J", // E
                SoulPts = 10000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, -1, 3, 4 },
                Team = MobileTeams.Undead,
            },

            // Giant Scorpion
            new MobileEnemy()
            {
                ID = 20,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Animal,
                MaleTexture = 275,
                FemaleTexture = 275,
                CorpseTexture = CorpseTexture(401, 5),
                HasIdle = false,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                MoveSound = (int)SoundClips.EnemyScorpionMove,
                BarkSound = (int)SoundClips.EnemyScorpionBark,
                AttackSound = (int)SoundClips.EnemyScorpionAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 15,
                MaxDamage = 25,
                MinHealth = 18,
                MaxHealth = 74,
                Level = 12,
                ParrySounds = false,
                ArmorValue = 4, // 0
                MapChance = 0,
                Weight = 600,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 3, 2, 1, 0 },
                Team = MobileTeams.Scorpions,
            },

            // Orc Shaman
            new MobileEnemy()
            {
                ID = 21,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Darkness,
                MaleTexture = 276,
                FemaleTexture = 276,
                CorpseTexture = CorpseTexture(96, 2),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyOrcShamanMove,
                BarkSound = (int)SoundClips.EnemyOrcShamanBark,
                AttackSound = (int)SoundClips.EnemyOrcShamanAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 2,
                MaxDamage = 20,
                MinHealth = 18,
                MaxHealth = 74,
                Level = 13,
                ArmorValue = 0, // 7
                ParrySounds = true,
                MapChance = 3,
                Weight = 400,
                LootTableKey = "LR2",
                SoulPts = 3000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 3, 2, 1, 0 },
                ChanceForAttack2 = 20,
                PrimaryAttackAnimFrames2 = new int[] { 0, -1, 4, 5, 0 },
                ChanceForAttack3 = 20,
                PrimaryAttackAnimFrames3 = new int[] { 0, 1, -1, 3, 2, 1, 0, -1, 4, 5, 0 },
                ChanceForAttack4 = 20,
                PrimaryAttackAnimFrames4 = new int[] { 0, 1, -1, 3, 2, -1, 3, 2, 1, 0 }, // Not used in classic. Fight stance used instead.
                ChanceForAttack5 = 20,
                PrimaryAttackAnimFrames5 = new int[] { 0, -1, 4, 5, -1, 4, 5, 0 }, // Not used in classic. Spell animation played instead.
                HasSpellAnimation = true,
                SpellAnimFrames = new int[] { 0, 0, 1, 2, 3, 3, 3 },
                Team = MobileTeams.Orcs,
            },

            // Gargoyle
            new MobileEnemy()
            {
                ID = 22,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Darkness,
                MaleTexture = 277,
                FemaleTexture = 277,
                CorpseTexture = CorpseTexture(96, 1),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyGargoyleMove,
                BarkSound = (int)SoundClips.EnemyGargoyleBark,
                AttackSound = (int)SoundClips.EnemyGargoyleAttack,
                MinMetalToHit = MaterialTypes.Mithril,
                MinDamage = 10,
                MaxDamage = 15,
                MinHealth = 19,
                MaxHealth = 82,
                Level = 14,
                ArmorValue = 5, // 0
                MapChance = 0,
                ParrySounds = false,
                Weight = 300,
                SoulPts = 3000,
                PrimaryAttackAnimFrames = new int[] { 0, 2, 1, 2, 3, -1, 4, 0 },
                Team = MobileTeams.Magic,
            },

            // Wraith
            new MobileEnemy()
            {
                ID = 23,
                Behaviour = MobileBehaviour.Spectral,
                Affinity = MobileAffinity.Undead,
                MaleTexture = 278,
                FemaleTexture = 278,
                CorpseTexture = CorpseTexture(306, 0),
                HasIdle = false,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                BloodIndex = 2,
                MoveSound = (int)SoundClips.EnemyWraithMove,
                BarkSound = (int)SoundClips.EnemyWraithBark,
                AttackSound = (int)SoundClips.EnemyWraithAttack,
                MinMetalToHit = MaterialTypes.Silver,
                MinDamage = 20,
                MaxDamage = 45,
                MinHealth = 30,
                MaxHealth = 90,
                Level = 15,
                ArmorValue = 2, // 0
                ParrySounds = false,
                MapChance = 1,
                Weight = 0,
                SeesThroughInvisibility = true,
                LootTableKey = "I",
                NoShadow = true,
                SoulPts = 30000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, -1, 3 },
                SpellAnimFrames = new int[] { 0, 0, 0, 0, 0 },
                Team = MobileTeams.Undead,
            },

            // Orc Warlord
            new MobileEnemy()
            {
                ID = 24,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Darkness,
                MaleTexture = 279,
                FemaleTexture = 279,
                CorpseTexture = CorpseTexture(96, 2),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyOrcWarlordMove,
                BarkSound = (int)SoundClips.EnemyOrcWarlordBark,
                AttackSound = (int)SoundClips.EnemyOrcWarlordAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 5,
                MaxDamage = 50,
                MinHealth = 20,
                MaxHealth = 90,
                Level = 16,
                ArmorValue = 0, // 0
                ParrySounds = true,
                MapChance = 2,
                Weight = 700,
                LootTableKey = "T",
                SoulPts = 1000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, 4, -1, 5 },
                ChanceForAttack2 = 33,
                PrimaryAttackAnimFrames2 = new int[] { 4, -1, 5, 0 },
                ChanceForAttack3 = 33,
                PrimaryAttackAnimFrames3 = new int[] { 0, 1, -1, 2, 3, 4 -1, 5, 0, 4, -1, 5, 0 },
                Team = MobileTeams.Orcs,
            },

            // Frost Daedra
            new MobileEnemy()
            {
                ID = 25,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Daedra,
                MaleTexture = 280,
                FemaleTexture = 280,
                CorpseTexture = CorpseTexture(400, 3),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyFrostDaedraMove,
                BarkSound = (int)SoundClips.EnemyFrostDaedraBark,
                AttackSound = (int)SoundClips.EnemyFrostDaedraAttack,
                MinMetalToHit = MaterialTypes.Mithril,
                MinDamage = 15, // 50
                MaxDamage = 50, // 100
                MinHealth = 25,
                MaxHealth = 130,
                Level = 17,
                ArmorValue = 8, // -5
                ParrySounds = true,
                MapChance = 0,
                Weight = 800,
                SeesThroughInvisibility = true,
                LootTableKey = "J",
                NoShadow = true,
                GlowColor = new Color(18, 68, 88) * 0.1f,
                SoulPts = 50000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, -1, 4, 5, 0 },
                ChanceForAttack2 = 50,
                PrimaryAttackAnimFrames2 = new int[] { -1, 4, 5, 0 },
                SpellAnimFrames = new int[] { 1, 1, 3, 3 },
                Team = MobileTeams.Daedra,
            },

            // Fire Daedra
            new MobileEnemy()
            {
                ID = 26,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Daedra,
                MaleTexture = 281,
                FemaleTexture = 281,
                CorpseTexture = CorpseTexture(400, 2),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyFireDaedraMove,
                BarkSound = (int)SoundClips.EnemyFireDaedraBark,
                AttackSound = (int)SoundClips.EnemyFireDaedraAttack,
                MinMetalToHit = MaterialTypes.Mithril,
                MinDamage = 15,
                MaxDamage = 50,
                MinHealth = 26,
                MaxHealth = 138,
                Level = 17,
                ArmorValue = 5, // 1
                ParrySounds = true,
                MapChance = 0,
                Weight = 800,
                SeesThroughInvisibility = true,
                LootTableKey = "S", // J
                NoShadow = true,
                GlowColor = new Color(243, 239, 44) * 0.05f,
                SoulPts = 50000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, -1, 4 },
                ChanceForAttack2 = 50,
                PrimaryAttackAnimFrames2 = new int[] { 3, -1, 4 },
                SpellAnimFrames = new int[] { 1, 1, 3, 3 },
                Team = MobileTeams.Daedra,
            },

            // Daedroth
            new MobileEnemy()
            {
                ID = 27,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Daedra,
                MaleTexture = 282,
                FemaleTexture = 282,
                CorpseTexture = CorpseTexture(400, 1),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyLesserDaedraMove,
                BarkSound = (int)SoundClips.EnemyLesserDaedraBark,
                AttackSound = (int)SoundClips.EnemyLesserDaedraAttack,
                MinMetalToHit = MaterialTypes.Mithril,
                MinDamage = 15,
                MaxDamage = 50,
                MinHealth = 27,
                MaxHealth = 146,
                Level = 18,
                ArmorValue = 2, // 1
                ParrySounds = true,
                MapChance = 0,
                Weight = 400,
                SeesThroughInvisibility = true,
                LootTableKey = "S", // E
                SoulPts = 10000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, 4, -1, 5 },
                ChanceForAttack2 = 33,
                PrimaryAttackAnimFrames2 = new int[] { 4, -1, 5, 0 },
                ChanceForAttack3 = 33,
                PrimaryAttackAnimFrames3 = new int[] { 0, 1, -1, 2, 3, 4, -1, 5, 0, 4, -1, 5, 0 },
                SpellAnimFrames = new int[] { 1, 1, 3, 3 },
                Team = MobileTeams.Daedra,
            },

            // Vampire
            new MobileEnemy()
            {
                ID = 28,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Darkness,
                MaleTexture = 283,
                FemaleTexture = 283,
                CorpseTexture = CorpseTexture(96, 3),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyFemaleVampireMove,
                BarkSound = (int)SoundClips.EnemyFemaleVampireBark,
                AttackSound = (int)SoundClips.EnemyFemaleVampireAttack,
                MinMetalToHit = MaterialTypes.Silver,
                MinDamage = 20,
                MaxDamage = 50,
                MinHealth = 28,
                MaxHealth = 154,
                Level = 19,
                ArmorValue = 0, // -2
                ParrySounds = false,
                MapChance = 3,
                Weight = 400,
                SeesThroughInvisibility = true,
                LootTableKey = "Q",
                NoShadow = true,
                SoulPts = 70000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, 3, -1, 4, 5 },
                SpellAnimFrames = new int[] { 1, 1, 5, 5 },
                Team = MobileTeams.Undead,
            },

            // Daedra Seducer
            new MobileEnemy()
            {
                ID = 29,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Daedra,
                MaleTexture = 284,
                FemaleTexture = 284,
                CorpseTexture = CorpseTexture(400, 6),          // Has a winged and unwinged corpse, only using unwinged here
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                HasSeducerTransform1 = true,
                HasSeducerTransform2 = true,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemySeducerMove,
                BarkSound = (int)SoundClips.EnemySeducerBark,
                AttackSound = (int)SoundClips.EnemySeducerAttack,
                MinMetalToHit = MaterialTypes.Mithril,
                MinDamage = 15,
                MaxDamage = 50,
                MinHealth = 27,
                MaxHealth = 146,
                Level = 19,
                ArmorValue = 3, // 1
                ParrySounds = false,
                MapChance = 1,
                Weight = 200,
                SeesThroughInvisibility = true,
                LootTableKey = "S", // Q
                SoulPts = 150000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2 },
                SpellAnimFrames = new int[] { 0, 1, 2 },
                SeducerTransform1Frames = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 },
                SeducerTransform2Frames = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8 },
                Team = MobileTeams.Daedra,
            },

            // Vampire Ancient
            new MobileEnemy()
            {
                ID = 30,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Darkness,
                MaleTexture = 285,
                FemaleTexture = 285,
                CorpseTexture = CorpseTexture(96, 3),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyVampireMove,
                BarkSound = (int)SoundClips.EnemyVampireBark,
                AttackSound = (int)SoundClips.EnemyVampireAttack,
                MinMetalToHit = MaterialTypes.Mithril,
                MinDamage = 20,
                MaxDamage = 60,
                MinHealth = 30,
                MaxHealth = 170,
                Level = 20,
                ArmorValue = 1, // -5
                ParrySounds = false,
                MapChance = 3,
                Weight = 400,
                SeesThroughInvisibility = true,
                LootTableKey = "Q",
                NoShadow = true,
                SoulPts = 100000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, 3, -1, 4, 5 },
                SpellAnimFrames = new int[] { 1, 1, 5, 5 },
                Team = MobileTeams.Undead,
            },

            // Daedra Lord
            new MobileEnemy()
            {
                ID = 31,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Daedra,
                MaleTexture = 286,
                FemaleTexture = 286,
                CorpseTexture = CorpseTexture(400, 4),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyDaedraLordMove,
                BarkSound = (int)SoundClips.EnemyDaedraLordBark,
                AttackSound = (int)SoundClips.EnemyDaedraLordAttack,
                MinMetalToHit = MaterialTypes.Mithril,
                MinDamage = 15,
                MaxDamage = 50,
                MinHealth = 35,
                MaxHealth = 210,
                Level = 20,
                ArmorValue = 5, // -10
                ParrySounds = true,
                MapChance = 0,
                Weight = 1000,
                SeesThroughInvisibility = true,
                LootTableKey = "S",
                SoulPts = 800000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, -1, 4 },
                ChanceForAttack2 = 33,
                PrimaryAttackAnimFrames2 = new int[] { 3, -1, 4, 0, -1, 4, 3, -1, 4, 0, -1, 4, 3 },
                ChanceForAttack3 = 33,
                PrimaryAttackAnimFrames3 = new int[] { 0, 1, -1, 2, 1, 0, 1, -1, 2, 1, 0 },
                SpellAnimFrames = new int[] { 1, 1, 3, 3 },
                Team = MobileTeams.Daedra,
            },

            // Lich
            new MobileEnemy()
            {
                ID = 32,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Undead,
                MaleTexture = 287,
                FemaleTexture = 287,
                CorpseTexture = CorpseTexture(306, 2),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                BloodIndex = 2,
                MoveSound = (int)SoundClips.EnemyLichMove,
                BarkSound = (int)SoundClips.EnemyLichBark,
                AttackSound = (int)SoundClips.EnemyLichAttack,
                MinMetalToHit = MaterialTypes.Mithril,
                MinDamage = 70,
                MaxDamage = 100,
                MinHealth = 30,
                MaxHealth = 170,
                Level = 20,
                ArmorValue = 3, // -10
                ParrySounds = false,
                MapChance = 4,
                Weight = 300,
                SeesThroughInvisibility = true,
                LootTableKey = "J", // S
                SoulPts = 100000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 1, 2, -1, 3, 4, 4 },
                SpellAnimFrames = new int[] { 0, 1, 2, 3, 4 },
                Team = MobileTeams.Undead,
            },

            // Ancient Lich
            new MobileEnemy()
            {
                ID = 33,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Undead,
                MaleTexture = 288,
                FemaleTexture = 288,
                CorpseTexture = CorpseTexture(306, 3),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                BloodIndex = 2,
                MoveSound = (int)SoundClips.EnemyLichKingMove,
                BarkSound = (int)SoundClips.EnemyLichKingBark,
                AttackSound = (int)SoundClips.EnemyLichKingAttack,
                MinMetalToHit = MaterialTypes.Mithril,
                MinDamage = 70,
                MaxDamage = 100,
                MinHealth = 30,
                MaxHealth = 170,
                Level = 21,
                ArmorValue = 5, // -12
                ParrySounds = false,
                MapChance = 4,
                Weight = 300,
                LootTableKey = "S",
                SoulPts = 250000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 1, 2, -1, 3, 4, 4 },
                SpellAnimFrames = new int[] { 0, 1, 2, 3, 4 },
                Team = MobileTeams.Undead,
            },

            // Dragonling
            new MobileEnemy()
            {
                ID = 34,
                Behaviour = MobileBehaviour.Flying,
                Affinity = MobileAffinity.Daylight,
                MaleTexture = 289,
                FemaleTexture = 289,
                CorpseTexture = CorpseTexture(96, 0),
                HasIdle = false,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                MoveSound = (int)SoundClips.EnemyFaeryDragonMove,
                BarkSound = (int)SoundClips.EnemyFaeryDragonBark,
                AttackSound = (int)SoundClips.EnemyFaeryDragonAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 20,
                MaxDamage = 40,
                MinHealth = 27,
                MaxHealth = 146,
                Level = 18,
                ArmorValue = 10, // 6
                ParrySounds = false,
                MapChance = 0,
                Weight = 10000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, -1, 3 },
                Team = MobileTeams.Dragonlings,
            },

            // Fire Atronach
            new MobileEnemy()
            {
                ID = 35,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Golem,
                MaleTexture = 290,
                FemaleTexture = 290,
                CorpseTexture = CorpseTexture(405, 2),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyFireAtronachMove,
                BarkSound = (int)SoundClips.EnemyFireAtronachBark,
                AttackSound = (int)SoundClips.EnemyFireAtronachAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 15,
                MaxDamage = 30,
                MinHealth = 22,
                MaxHealth = 74,
                Level = 13,
                ArmorValue = 3, // 6
                ParrySounds = false,
                MapChance = 0,
                NoShadow = true,
                GlowColor = new Color(243, 150, 44) * 0.05f,
                Weight = 1000,
                SoulPts = 30000,
                PrimaryAttackAnimFrames = new int[] { 0, -1, 1, 2, 3, 4 },
                Team = MobileTeams.Magic,
            },

            // Iron Atronach
            new MobileEnemy()
            {
                ID = 36,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Golem,
                MaleTexture = 291,
                FemaleTexture = 291,
                CorpseTexture = CorpseTexture(405, 1),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyIronAtronachMove,
                BarkSound = (int)SoundClips.EnemyIronAtronachBark,
                AttackSound = (int)SoundClips.EnemyIronAtronachAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 20,
                MaxDamage = 40,
                MinHealth = 23,
                MaxHealth = 82,
                Level = 14,
                ArmorValue = 7, // 6
                ParrySounds = true,
                MapChance = 0,
                Weight = 1000,
                SoulPts = 30000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, -1, 3, 4 },
                Team = MobileTeams.Magic,
            },

            // Flesh Atronach
            new MobileEnemy()
            {
                ID = 37,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Golem,
                MaleTexture = 292,
                FemaleTexture = 292,
                CorpseTexture = CorpseTexture(405, 0),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyFleshAtronachMove,
                BarkSound = (int)SoundClips.EnemyFleshAtronachBark,
                AttackSound = (int)SoundClips.EnemyFleshAtronachAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 15,
                MaxDamage = 25,
                MinHealth = 21,
                MaxHealth = 74,
                Level = 12,
                ArmorValue = 3, // 6
                ParrySounds = false,
                MapChance = 0,
                Weight = 1000,
                SoulPts = 30000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, -1, 3, 4 },
                Team = MobileTeams.Magic,
            },

            // Ice Atronach
            new MobileEnemy()
            {
                ID = 38,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Golem,
                MaleTexture = 293,
                FemaleTexture = 293,
                CorpseTexture = CorpseTexture(405, 3),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyIceAtronachMove,
                BarkSound = (int)SoundClips.EnemyIceAtronachBark,
                AttackSound = (int)SoundClips.EnemyIceAtronachAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 15,
                MaxDamage = 30,
                MinHealth = 22,
                MaxHealth = 74,
                Level = 13,
                ArmorValue = 5, // 6
                ParrySounds = true,
                MapChance = 0,
                Weight = 1000,
                SoulPts = 30000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, -1, 3, 4 },
                ChanceForAttack2 = 50,
                PrimaryAttackAnimFrames2 = new int[] { 0, -1, 3, 4 },
                Team = MobileTeams.Magic,
            },

            // Weights in classic (From offset 0x1BD8D9 in FALL.EXE) only have entries
            // up through Horse. Dragonling, Dreugh and Lamia use nonsense values from
            // the adjacent data. For Daggerfall Unity, using values inferred from
            // other enemy types.

            // Horse (unused, but can appear in merchant-sold soul traps)
            new MobileEnemy()
            {
                ID = 39,
            },

            // Dragonling
            new MobileEnemy()
            {
                ID = 40,
                Behaviour = MobileBehaviour.Flying,
                Affinity = MobileAffinity.Daylight,
                MaleTexture = 295,
                FemaleTexture = 295,
                CorpseTexture = CorpseTexture(96, 0),
                HasIdle = false,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                MoveSound = (int)SoundClips.EnemyFaeryDragonMove,
                BarkSound = (int)SoundClips.EnemyFaeryDragonBark,
                AttackSound = (int)SoundClips.EnemyFaeryDragonAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 5,
                MaxDamage = 15,
                MinHealth = 14,
                MaxHealth = 42,
                Level = 16,
                ArmorValue = 3, // 6
                ParrySounds = false,
                MapChance = 0,
                Weight = 10000, // Using same value as other dragonling
                SoulPts = 500000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3 },
                Team = MobileTeams.Dragonlings,
            },

            // Dreugh
            new MobileEnemy()
            {
                ID = 41,
                Behaviour = MobileBehaviour.Aquatic,
                Affinity = MobileAffinity.Water,
                MaleTexture = 296,
                FemaleTexture = 296,
                CorpseTexture = CorpseTexture(305, 0),
                HasIdle = false,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyDreughMove,
                BarkSound = (int)SoundClips.EnemyDreughBark,
                AttackSound = (int)SoundClips.EnemyDreughAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 12,
                MaxDamage = 24,
                MinHealth = 19,
                MaxHealth = 66,
                Level = 10,
                ArmorValue = 3, // 6
                ParrySounds = false,
                MapChance = 0,
                Weight = 600, // Using same value as orc
                LootTableKey = "R",
                SoulPts = 10000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, 3, -1, 4, 5, -1, 6, 7 },
                ChanceForAttack2 = 33,
                PrimaryAttackAnimFrames2 = new int[] { 0, 1, 2, 3, -1, 4 },
                ChanceForAttack3 = 33,
                PrimaryAttackAnimFrames3 = new int[] { 0, 5, -1, 6, 7 },
                Team = MobileTeams.Aquatic,
            },

            // Lamia
            new MobileEnemy()
            {
                ID = 42,
                Behaviour = MobileBehaviour.Aquatic,
                Affinity = MobileAffinity.Water,
                MaleTexture = 297,
                FemaleTexture = 297,
                CorpseTexture = CorpseTexture(305, 2),
                HasIdle = false,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyLamiaMove,
                BarkSound = (int)SoundClips.EnemyLamiaBark,
                AttackSound = (int)SoundClips.EnemyLamiaAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 10,
                MaxDamage = 20,
                MinHealth = 22,
                MaxHealth = 74,
                Level = 13,
                ArmorValue = 1, // 6
                ParrySounds = false,
                MapChance = 0,
                LootTableKey = "R",
                Weight = 200, // Using same value as nymph
                SoulPts = 10000,
                PrimaryAttackAnimFrames = new int[] { 0, -1, 1, 2, 3, 4, -1, 5, 0 },
                ChanceForAttack2 = 33,
                PrimaryAttackAnimFrames2 = new int[] { 0, 3, -1, 5, 4, 3, 3, -1, 5, 4, 3, -1, 5, 0 },
                ChanceForAttack3 = 33,
                PrimaryAttackAnimFrames3 = new int[] { 0, 3, -1, 5, 0 },
                Team = MobileTeams.Aquatic,
            },

            // Mage
            new MobileEnemy()
            {
                ID = 128,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 486,
                FemaleTexture = 485,
                CorpseTexture = CorpseTexture(380, 1),
                HasIdle = true,
                HasRangedAttack1 = true,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = false,
                MapChance = 3,
                LootTableKey = "LR3",   // U
                CastsMagic = true,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 3, 2, 1, 0, -1, 5, 4, 0 },
                ChanceForAttack2 = 33,
                PrimaryAttackAnimFrames2 = new int[] { 0, 1, -1, 3, 2, 1, 0 },
                ChanceForAttack3 = 33,
                PrimaryAttackAnimFrames3 = new int[] { 0, -1, 5, 4, 0 },
                HasSpellAnimation = true,
                SpellAnimFrames = new int[] { 0, 1, 2, 3, 3 },
                Team = MobileTeams.KnightsAndMages,
            },

            // Spellsword
            new MobileEnemy()
            {
                ID = 129,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 476,
                FemaleTexture = 475,
                CorpseTexture = CorpseTexture(380, 1),
                HasIdle = true,
                HasRangedAttack1 = true,
                HasRangedAttack2 = false,       // Female has RangedAttack2, male variant does not. Setting false for consistency.
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = true,
                MapChance = 1,
                LootTableKey = "P",
                CastsMagic = true,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, 4, 5 },
                ChanceForAttack2 = 33,
                PrimaryAttackAnimFrames2 = new int[] { 5, 4, 3, -1, 2, 1, 0 },
                ChanceForAttack3 = 33,
                PrimaryAttackAnimFrames3 = new int[] { 0, 1, -1, 2, 2, 1, 0 },
                RangedAttackAnimFrames = new int[] { 3, 2, 0, 0, 0, -1, 1, 1, 2, 3 },
                HasSpellAnimation = true,
                SpellAnimFrames = new int[] { 0, 1, 2, 3, 3 },
                Team = MobileTeams.KnightsAndMages,
            },

            // Battlemage
            new MobileEnemy()
            {
                ID = 130,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 490,
                FemaleTexture = 489,
                CorpseTexture = CorpseTexture(380, 1),
                HasIdle = true,
                HasRangedAttack1 = true,
                HasRangedAttack2 = true,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = true,
                MapChance = 2,
                LootTableKey = "LR3",   // U
                CastsMagic = true,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, 4, -1, 5, 0 },
                ChanceForAttack2 = 33,
                PrimaryAttackAnimFrames2 = new int[] { 4, 4, -1, 5, 0, 0 },
                ChanceForAttack3 = 33,
                PrimaryAttackAnimFrames3 = new int[] { 4, -1, 5, 0, 0, 1, -1, 2, 3, 4, -1, 5, 0 },
                RangedAttackAnimFrames = new int[] { 3, 2, 0, 0, 0, -1, 1, 1, 2, 3 },
                HasSpellAnimation = true,
                SpellAnimFrames = new int[] { 0, 1, 2, 3, 3 },
                Team = MobileTeams.KnightsAndMages,
            },

            // Sorcerer
            new MobileEnemy()
            {
                ID = 131,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 476,
                FemaleTexture = 475,
                CorpseTexture = CorpseTexture(380, 1),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = false,
                MapChance = 3,
                LootTableKey = "LR3",   // U
                CastsMagic = true,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, 4, 5 },
                ChanceForAttack2 = 33,
                PrimaryAttackAnimFrames2 = new int[] { 5, 4, 3, -1, 2, 1, 0 },
                HasSpellAnimation = true,
                SpellAnimFrames = new int[] { 0, 1, 2, 3, 3 },
                Team = MobileTeams.KnightsAndMages,
            },

            // Healer
            new MobileEnemy()
            {
                ID = 132,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 486,
                FemaleTexture = 485,
                CorpseTexture = CorpseTexture(380, 1),
                HasIdle = true,
                HasRangedAttack1 = true,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = false,
                MapChance = 1,
                LootTableKey = "LR2",   // U
                CastsMagic = true,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 3, 2, 1, 0, -1, 5, 4, 0 },
                ChanceForAttack2 = 33,
                PrimaryAttackAnimFrames2 = new int[] { 0, 1, -1, 3, 2, 1, 0 },
                ChanceForAttack3 = 33,
                PrimaryAttackAnimFrames3 = new int[] { 0, -1, 5, 4, 0 },
                HasSpellAnimation = true,
                SpellAnimFrames = new int[] { 0, 1, 2, 3, 3 },
                Team = MobileTeams.KnightsAndMages,
            },

            // Nightblade
            new MobileEnemy()
            {
                ID = 133,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 490,
                FemaleTexture = 489,
                CorpseTexture = CorpseTexture(380, 1),
                HasIdle = true,
                HasRangedAttack1 = true,
                HasRangedAttack2 = true,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = true,
                MapChance = 1,
                LootTableKey = "LR3",   // U
                CastsMagic = true,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, 4, -1, 5, 0 },
                ChanceForAttack2 = 33,
                PrimaryAttackAnimFrames2 = new int[] { 4, 4, -1, 5, 0, 0 },
                ChanceForAttack3 = 33,
                PrimaryAttackAnimFrames3 = new int[] { 4, -1, 5, 0, 0, 1, -1, 2, 3, 4, -1, 5, 0 },
                RangedAttackAnimFrames = new int[] { 3, 2, 0, 0, 0, -1, 1, 1, 2, 3 },
                HasSpellAnimation = true,
                SpellAnimFrames = new int[] { 0, 1, 2, 3, 3 },
                Team = MobileTeams.Criminals,
            },

            // Bard
            new MobileEnemy()
            {
                ID = 134,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 482,
                FemaleTexture = 481,
                CorpseTexture = CorpseTexture(380, 1),
                HasIdle = true,
                HasRangedAttack1 = true,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = true,
                MapChance = 2,
                LootTableKey = "O",
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, 4, -1, 5 },
                ChanceForAttack2 = 50,
                PrimaryAttackAnimFrames2 = new int[] { 3, 4, -1, 5, 0 },
                ChanceForAttack3 = 0,
                PrimaryAttackAnimFrames3 = new int[] { 4, -1, 5, 0, 0, 1, -1, 2, 3, 4, -1, 5, 0 },
                RangedAttackAnimFrames = new int[] { 3, 2, 0, 0, 0, -1, 1, 1, 2, 3 },
                Team = MobileTeams.KnightsAndMages,
            },

            // Burglar
            new MobileEnemy()
            {
                ID = 135,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 484,
                FemaleTexture = 483,
                CorpseTexture = CorpseTexture(380, 1),
                HasIdle = true,
                HasRangedAttack1 = true,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = true,
                MapChance = 1,
                LootTableKey = "O",
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, 4, -1, 5, 0 },
                ChanceForAttack2 = 33,
                PrimaryAttackAnimFrames2 = new int[] { 4, 4, -1, 5, 0, 0 },
                ChanceForAttack3 = 33,
                PrimaryAttackAnimFrames3 = new int[] { 4, -1, 5, 0, 0, 1, -1, 2, 3, 4, -1, 5, 0 },
                RangedAttackAnimFrames = new int[] { 3, 2, 0, 0, 0, -1, 1, 1, 2, 3 },
                Team = MobileTeams.Criminals,
            },

            // Rogue
            new MobileEnemy()
            {
                ID = 136,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 488,
                FemaleTexture = 487,
                CorpseTexture = CorpseTexture(380, 1),
                HasIdle = true,
                HasRangedAttack1 = true,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = true,
                MapChance = 1,
                LootTableKey = "O",
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 0, 0, 1, -1, 2, 2, 1, 0 },
                ChanceForAttack2 = 33,
                PrimaryAttackAnimFrames2 = new int[] { 0, 1, -1, 2, 3, 4, 5 },
                ChanceForAttack3 = 33,
                PrimaryAttackAnimFrames3 = new int[] { 5, 5, 3, -1, 2, 1, 0 },
                RangedAttackAnimFrames = new int[] { 3, 2, 0, 0, 0, -1, 1, 1, 2, 3 },
                Team = MobileTeams.Criminals,
            },

            // Acrobat
            new MobileEnemy()
            {
                ID = 137,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 484,
                FemaleTexture = 483,
                CorpseTexture = CorpseTexture(380, 1),
                HasIdle = true,
                HasRangedAttack1 = true,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = true,
                MapChance = 0,
                LootTableKey = "O",
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, 4, -1, 5, 0 },
                ChanceForAttack2 = 33,
                PrimaryAttackAnimFrames2 = new int[] { 4, 4, -1, 5, 0, 0 },
                ChanceForAttack3 = 33,
                PrimaryAttackAnimFrames3 = new int[] { 4, -1, 5, 0, 0, 1, -1, 2, 3, 4, -1, 5, 0 },
                RangedAttackAnimFrames = new int[] { 3, 2, 0, 0, 0, -1, 1, 1, 2, 3 },
                Team = MobileTeams.KnightsAndMages,
            },

            // Thief
            new MobileEnemy()
            {
                ID = 138,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 484,
                FemaleTexture = 483,
                CorpseTexture = CorpseTexture(380, 1),
                HasIdle = true,
                HasRangedAttack1 = true,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = true,
                MapChance = 2,
                LootTableKey = "O",
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, 4, -1, 5, 0 },
                ChanceForAttack2 = 33,
                PrimaryAttackAnimFrames2 = new int[] { 4, 4, -1, 5, 0, 0 },
                ChanceForAttack3 = 33,
                PrimaryAttackAnimFrames3 = new int[] { 4, -1, 5, 0, 0, 1, -1, 2, 3, 4, -1, 5, 0 },
                RangedAttackAnimFrames = new int[] { 3, 2, 0, 0, 0, -1, 1, 1, 2, 3 },
                Team = MobileTeams.Criminals,
            },

            // Assassin
            new MobileEnemy()
            {
                ID = 139,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 480,
                FemaleTexture = 479,
                CorpseTexture = CorpseTexture(380, 1),
                HasIdle = true,
                HasRangedAttack1 = true,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = true,
                MapChance = 0,
                LootTableKey = "O",
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, 4, -1, 5, 0 },
                ChanceForAttack2 = 33,
                PrimaryAttackAnimFrames2 = new int[] { 4, 4, -1, 5, 0, 0 },
                ChanceForAttack3 = 33,
                PrimaryAttackAnimFrames3 = new int[] { 4, -1, 5, 0, 0, 1, -1, 2, 3, 4, -1, 5, 0 },
                RangedAttackAnimFrames = new int[] { 3, 2, 0, 0, 0, -1, 1, 1, 2, 3 },
                Team = MobileTeams.Criminals,
            },

            // Monk
            new MobileEnemy()
            {
                ID = 140,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 1519, // 488
                FemaleTexture = 1520, // 487
                CorpseTexture = CorpseTexture(380, 1),
                HasIdle = true,
                HasRangedAttack1 = true,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = true,
                MapChance = 1,
                LootTableKey = "O", // T
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 0, 0, 1, -1, 2, 2, 1, 0 },
                ChanceForAttack2 = 33,
                PrimaryAttackAnimFrames2 = new int[] { 0, 1, -1, 2, 3, 4, 5 },
                ChanceForAttack3 = 33,
                PrimaryAttackAnimFrames3 = new int[] { 5, 5, 3, -1, 2, 1, 0 },
                RangedAttackAnimFrames = new int[] { 3, 2, 0, 0, 0, -1, 1, 1, 2, 3 },
                Team = MobileTeams.KnightsAndMages,
            },

            // Archer
            new MobileEnemy()
            {
                ID = 141,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 480,
                FemaleTexture = 479,
                CorpseTexture = CorpseTexture(380, 1),
                HasIdle = true,
                HasRangedAttack1 = true,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                PrefersRanged = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = true,
                MapChance = 0,
                LootTableKey = "C",
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, 4, -1, 5, 0 },
                ChanceForAttack2 = 33,
                PrimaryAttackAnimFrames2 = new int[] { 4, 4, -1, 5, 0, 0 },
                ChanceForAttack3 = 33,
                PrimaryAttackAnimFrames3 = new int[] { 4, -1, 5, 0, 0, 1, -1, 2, 3, 4, -1, 5, 0 },
                RangedAttackAnimFrames = new int[] { 3, 2, 0, 0, 0, -1, 1, 1, 2, 3 },
                Team = MobileTeams.KnightsAndMages,
            },

            // Ranger
            new MobileEnemy()
            {
                ID = 142,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 480,
                FemaleTexture = 479,
                CorpseTexture = CorpseTexture(380, 1),
                HasIdle = true,
                HasRangedAttack1 = true,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = true,
                MapChance = 1,
                LootTableKey = "C",
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, 4, -1, 5, 0 },
                ChanceForAttack2 = 33,
                PrimaryAttackAnimFrames2 = new int[] { 4, 4, -1, 5, 0, 0 },
                ChanceForAttack3 = 33,
                PrimaryAttackAnimFrames3 = new int[] { 4, -1, 5, 0, 0, 1, -1, 2, 3, 4, -1, 5, 0 },
                RangedAttackAnimFrames = new int[] { 3, 2, 0, 0, 0, -1, 1, 1, 2, 3 },
                Team = MobileTeams.KnightsAndMages,
            },

            // Barbarian
            new MobileEnemy()
            {
                ID = 143,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 1507, // 482
                FemaleTexture = 1508, // 481
                CorpseTexture = CorpseTexture(380, 1),
                HasIdle = true,
                HasRangedAttack1 = true,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = true,
                MapChance = 0,
                LootTableKey = "LR1",   // T
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, 4, -1, 5 },
                ChanceForAttack2 = 50,
                PrimaryAttackAnimFrames2 = new int[] { 3, 4, -1, 5, 0 },
                ChanceForAttack3 = 0,
                RangedAttackAnimFrames = new int[] { 3, 2, 0, 0, 0, -1, 1, 1, 2, 3 },
                Team = MobileTeams.Criminals,
            },

            // Warrior
            new MobileEnemy()
            {
                ID = 144,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 478,
                FemaleTexture = 477,
                CorpseTexture = CorpseTexture(380, 1),
                HasIdle = true,
                HasRangedAttack1 = true,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = true,
                MapChance = 0,
                LootTableKey = "LR1",   // T
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, -1, 3, 4, 5 },
                ChanceForAttack2 = 50,
                PrimaryAttackAnimFrames2 = new int[] { 4, 5, -1, 3, 2, 1, 0 },
                ChanceForAttack3 = 0,
                RangedAttackAnimFrames = new int[] { 3, 2, 0, 0, 0, -1, 1, 1, 2, 3 },
                Team = MobileTeams.KnightsAndMages,
            },

            // Knight
            new MobileEnemy()
            {
                ID = 145,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 478,
                FemaleTexture = 477,
                CorpseTexture = CorpseTexture(380, 1),
                HasIdle = true,
                HasRangedAttack1 = true,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = true,
                MapChance = 1,
                LootTableKey = "T",
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, -1, 3, 4, 5 },
                ChanceForAttack2 = 50,
                PrimaryAttackAnimFrames2 = new int[] { 4, 5, -1, 3, 2, 1, 0 },
                ChanceForAttack3 = 0,
                RangedAttackAnimFrames = new int[] { 3, 2, 0, 0, 0, -1, 1, 1, 2, 3 },
                Team = MobileTeams.KnightsAndMages,
            },

            // City Watch - The Haltmeister
            new MobileEnemy()
            {
                ID = 146,
                Behaviour = MobileBehaviour.Guard,
                Affinity = MobileAffinity.Human,
                MaleTexture = 399,
                FemaleTexture = 399,
                CorpseTexture = CorpseTexture(380, 1),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.None,
                BarkSound = (int)SoundClips.Halt,
                AttackSound = (int)SoundClips.None,
                ParrySounds = true,
                MapChance = 0,
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, 4 },
                Team = MobileTeams.CityWatch,
            },

            // Goblin
            new MobileEnemy()
            {
                ID = 256,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Darkness,
                MaleTexture = 1600,
                FemaleTexture = 1600,
                CorpseTexture = CorpseTexture(1600, 20),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyVampireMove,
                BarkSound = (int)SoundClips.ArenaGoblin,
                AttackSound = (int)SoundClips.EnemyOrcAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 1,
                MaxDamage = 6,
                MinHealth = 11,
                MaxHealth = 20,
                Level = 2,
                ArmorValue = 0, // 2
                ParrySounds = true,
                MapChance = 0,
                LootTableKey = "A",
                Weight = 200,
                SoulPts = 500,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, 3, -1, 3, 4 },
                Team = MobileTeams.Orcs,
            },

            // Homunculus
            new MobileEnemy()
            {
                ID = 257,
                Behaviour = MobileBehaviour.Flying,
                Affinity = MobileAffinity.Darkness,
                MaleTexture = 1602,
                FemaleTexture = 1602,
                CorpseTexture = CorpseTexture(1602, 15),
                HasIdle = false,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyImpMove,
                BarkSound = (int)SoundClips.ArenaHomunculus,
                AttackSound = (int)SoundClips.EnemyRatAttack,
                MinMetalToHit = MaterialTypes.Dwarven,
                MinDamage = 10,
                MaxDamage = 35,
                MinHealth = 18,
                MaxHealth = 74,
                Level = 12,
                ArmorValue = 4, // 0
                ParrySounds = false,
                MapChance = 0,
                Weight = 80,
                SeesThroughInvisibility = true,
                LootTableKey = "D",
                SoulPts = 30000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, -1, 3, 1 },
                HasSpellAnimation = false,
                SpellAnimFrames = new int[] { 0, 1, 2, 3, 3 },
                Team = MobileTeams.Magic,
            },

            // Lizard Man
            new MobileEnemy()
            {
                ID = 258,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Daylight,
                MaleTexture = 1601,
                FemaleTexture = 1601,
                CorpseTexture = CorpseTexture(1601, 20),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyVampireMove,
                BarkSound = (int)SoundClips.ArenaLizardMan,
                AttackSound = (int)SoundClips.EnemyOrcAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 1,
                MaxDamage = 8,
                MinHealth = 13,
                MaxHealth = 34,
                Level = 4,
                ArmorValue = 0, // 5
                ParrySounds = true,
                MapChance = 0,
                Weight = 400,
                SeesThroughInvisibility = false,
                LootTableKey = "A",
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, 3, 4, 5, -1, 6, 6, 7 },
                Team = MobileTeams.Scorpions,
            },

            // Lizard Warrior
            new MobileEnemy()
            {
                ID = 259,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Daylight,
                MaleTexture = 1607,
                FemaleTexture = 1607,
                CorpseTexture = CorpseTexture(1607, 20),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyVampireMove,
                BarkSound = (int)SoundClips.ArenaLizardMan,
                AttackSound = (int)SoundClips.EnemyOrcAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 6,
                MaxDamage = 18,
                MinHealth = 17,
                MaxHealth = 66,
                Level = 8,
                ArmorValue = 1, // 2
                ParrySounds = true,
                MapChance = 0,
                Weight = 600,
                SeesThroughInvisibility = false,
                LootTableKey = "A",
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, 3, 4, 5, -1, 6, 6, 7 },
                Team = MobileTeams.Scorpions,
            },

            // Bat
            new MobileEnemy()
            {
                ID = 260,
                Behaviour = MobileBehaviour.Flying,
                Affinity = MobileAffinity.Animal,
                MaleTexture = 1603,
                FemaleTexture = 1603,
                CorpseTexture = CorpseTexture(401, 0),
                HasIdle = false,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = false,
                MoveSound = (int)SoundClips.EnemyGiantBatMove,
                BarkSound = (int)SoundClips.EnemyGiantBatBark,
                AttackSound = (int)SoundClips.EnemyGiantBatAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 1,
                MaxDamage = 4,
                MinHealth = 8,
                MaxHealth = 14,
                Level = 1,
                ArmorValue = 0, // 6
                ParrySounds = false,
                MapChance = 0,
                Weight = 40,
                SeesThroughInvisibility = false,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3 },
                Team = MobileTeams.Vermin,
            },

            // Medusa
            new MobileEnemy()
            {
                ID = 261,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Darkness,
                MaleTexture = 1611,
                FemaleTexture = 1611,
                CorpseTexture = CorpseTexture(1611, 25),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemySpiderMove,
                BarkSound = (int)SoundClips.ArenaSkeleton,
                AttackSound = (int)SoundClips.EnemySpiderAttack,
                MinMetalToHit = MaterialTypes.Dwarven,
                MinDamage = 10,
                MaxDamage = 50,
                MinHealth = 30,
                MaxHealth = 90,
                Level = 16,
                ArmorValue = 3, // 0
                ParrySounds = false,
                MapChance = 0,
                Weight = 1200,
                SeesThroughInvisibility = false,
                LootTableKey = "R",
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, 3, 4, 5, 6, -1, 7, 7, 8 },
                HasSpellAnimation = true,
                SpellAnimFrames = new int[] { 0, 1, 2, 3, 3 },
                Team = MobileTeams.Aquatic,
            },

            // Wolf
            new MobileEnemy()
            {
                ID = 262,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Animal,
                MaleTexture = 1612,
                FemaleTexture = 1612,
                CorpseTexture = CorpseTexture(1612, 20),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = false,
                MoveSound = (int)SoundClips.EnemyWerewolfMove,
                BarkSound = (int)SoundClips.EnemyWerewolfBark,
                AttackSound = (int)SoundClips.EnemyWerewolfAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 1,
                MaxDamage = 10,
                MinHealth = 10,
                MaxHealth = 25,
                Level = 5,
                ArmorValue = 1, // 7
                ParrySounds = false,
                MapChance = 0,
                Weight = 300,
                SeesThroughInvisibility = true,
                PrimaryAttackAnimFrames = new int[] { 0, 0, 0, 1, -1, 2, 3, 3 },
                Team = MobileTeams.Werecreatures,
            },

            // Snow Wolf
            new MobileEnemy()
            {
                ID = 263,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Animal,
                MaleTexture = 1615,
                FemaleTexture = 1615,
                CorpseTexture = CorpseTexture(1615, 20),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = false,
                MoveSound = (int)SoundClips.EnemyWerewolfMove,
                BarkSound = (int)SoundClips.EnemyWerewolfBark,
                AttackSound = (int)SoundClips.EnemyWerewolfAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 1,
                MaxDamage = 15,
                MinHealth = 15,
                MaxHealth = 50,
                Level = 10,
                ArmorValue = 2, // 2
                ParrySounds = false,
                MapChance = 0,
                Weight = 350,
                SeesThroughInvisibility = true,
                PrimaryAttackAnimFrames = new int[] { 0, 0, 0, 1, -1, 2, 3, 3 },
                HasSpellAnimation = false,
                Team = MobileTeams.Werecreatures,
            },

            // Hell Hound
            new MobileEnemy()
            {
                ID = 264,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Daedra,
                MaleTexture = 1614,
                FemaleTexture = 1614,
                CorpseTexture = CorpseTexture(1614, 20),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = false,
                MoveSound = (int)SoundClips.EnemyWerewolfMove,
                BarkSound = (int)SoundClips.EnemyWerewolfBark,
                AttackSound = (int)SoundClips.EnemyWerewolfAttack,
                MinMetalToHit = MaterialTypes.Silver,
                MinDamage = 5,
                MaxDamage = 25,
                MinHealth = 20,
                MaxHealth = 75,
                Level = 15,
                ArmorValue = 5, // -2
                ParrySounds = false,
                MapChance = 0,
                GlowColor = new Color(243, 239, 44) * 0.05f,
                Weight = 400,
                SeesThroughInvisibility = true,
                PrimaryAttackAnimFrames = new int[] { 0, 0, 0, 1, -1, 2, 3, 3 },
                HasSpellAnimation = false,
                Team = MobileTeams.Daedra,
            },

            // Grotesque
            new MobileEnemy()
            {
                ID = 265,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Darkness,
                MaleTexture = 1606,
                FemaleTexture = 1606,
                CorpseTexture = CorpseTexture(405, 1),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyIronAtronachMove,
                BarkSound = (int)SoundClips.ArenaStoneGolem,
                AttackSound = (int)SoundClips.EnemyIronAtronachAttack,
                MinMetalToHit = MaterialTypes.Silver,
                MinDamage = 5,
                MaxDamage = 10,
                MinHealth = 17,
                MaxHealth = 66,
                Level = 6,
                ArmorValue = 2, // 2
                ParrySounds = false,
                MapChance = 0,
                Weight = 250,
                SeesThroughInvisibility = false,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 1, 1, 2, 3, -1, 4 },
                Team = MobileTeams.Magic,
            },

            // Skeletal Soldier
            new MobileEnemy()
            {
                ID = 266,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Skeletal,
                MaleTexture = 1605,
                FemaleTexture = 1605,
                CorpseTexture = CorpseTexture(306, 1),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                BloodIndex = 2,
                MoveSound = (int)SoundClips.EnemySkeletonMove,
                BarkSound = (int)SoundClips.EnemySkeletonBark,
                AttackSound = (int)SoundClips.EnemySkeletonAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 2,
                MaxDamage = 12,
                MinHealth = 12,
                MaxHealth = 18,
                Level = 3,
                ArmorValue = 0, // 5
                ParrySounds = true,
                MapChance = 0,
                Weight = 60,
                SeesThroughInvisibility = true,
                LootTableKey = "H",
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, 3, 4, -1, 5 },
                Team = MobileTeams.Undead,
            },

            // Dog
            // ProjectN: put dogs into Knight&Mages team.
            new MobileEnemy()
            {
                ID = 267,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Animal,
                MaleTexture = 1613,
                FemaleTexture = 1613,
                CorpseTexture = CorpseTexture(1613, 20),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = false,
                BloodIndex = 2,
                MoveSound = (int)SoundClips.EnemyWerewolfMove,
                BarkSound = (int)SoundClips.AnimalDog,
                AttackSound = (int)SoundClips.EnemyWerewolfAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 1,
                MaxDamage = 8,
                MinHealth = 12,
                MaxHealth = 18,
                Level = 3,
                ArmorValue = 0, // 6
                ParrySounds = false,
                MapChance = 0,
                Weight = 150,
                SeesThroughInvisibility = true,
                PrimaryAttackAnimFrames = new int[] { 0, 0, 0, 1, -1, 2, 3, 3 },
                Team = MobileTeams.KnightsAndMages,
            },

            // Mountain Nymph
            new MobileEnemy()
            {
                ID = 268,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Daylight,
                MaleTexture = 1604,
                FemaleTexture = 1604,
                CorpseTexture = CorpseTexture(406, 2),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyNymphMove,
                BarkSound = (int)SoundClips.EnemyNymphBark,
                AttackSound = (int)SoundClips.EnemyNymphAttack,
                MinMetalToHit = MaterialTypes.Silver,
                MinDamage = 1,
                MaxDamage = 5,
                MinHealth = 15,
                MaxHealth = 50,
                Level = 6,
                ArmorValue = 0, // 0
                ParrySounds = false,
                MapChance = 1,
                Weight = 200,
                LootTableKey = "B", // C
                SoulPts = 10000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, 3, 4, -1, 5 },
                Team = MobileTeams.Nymphs,
            },

            // Minotaur
            new MobileEnemy()
            {
                ID = 269,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Daylight,
                MaleTexture = 1628,
                FemaleTexture = 1628,
                CorpseTexture = CorpseTexture(1628, 20),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyTigerMove,
                BarkSound = (int)SoundClips.ArenaMinotaur,
                AttackSound = (int)SoundClips.EnemyTigerAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 30,
                MaxDamage = 50,
                MinHealth = 30,
                MaxHealth = 90,
                Level = 15,
                ArmorValue = 5, // 0
                ParrySounds = false,
                MapChance = 3,
                Weight = 1000,
                LootTableKey = "C",
                SoulPts = 20000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, 3, 4, -1, 5, 6 },
                ChanceForAttack2 = 35,
                PrimaryAttackAnimFrames2 = new int[] { 7, 8, 9, 10, -1, 11, 12, 13 },
                Team = MobileTeams.Centaurs,
            },

            // Iron Golem
            new MobileEnemy()
            {
                ID = 270,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Golem,
                MaleTexture = 1629,
                FemaleTexture = 1629,
                CorpseTexture = CorpseTexture(1629, 20),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyIronAtronachMove,
                BarkSound = (int)SoundClips.ArenaIronGolem,
                AttackSound = (int)SoundClips.EnemyIronAtronachAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 10,
                MaxDamage = 20,
                MinHealth = 19,
                MaxHealth = 80,
                Level = 10,
                ArmorValue = 8, // 6
                ParrySounds = false,
                MapChance = 0,
                Weight = 800,
                SoulPts = 20000,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, 3, 4, -1, 5 },
                ChanceForAttack2 = 25,
                PrimaryAttackAnimFrames2 = new int[] {5, 6, 7, -1, 8, 9, 10, -1, 11, 12 },
                ChanceForAttack3 = 35,
                PrimaryAttackAnimFrames3 = new int[] {13, 14, 15, 16, 17, 18, 19, 20, -1, 21, 22 },
                Team = MobileTeams.Magic,
            },

            // Blood Spider
            new MobileEnemy()
            {
                ID = 271,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Animal,
                MaleTexture = 1624,
                FemaleTexture = 1624,
                CorpseTexture = CorpseTexture(1624, 20),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                MoveSound = (int)SoundClips.EnemySpiderMove,
                BarkSound = (int)SoundClips.ArenaSpider,
                AttackSound = (int)SoundClips.EnemySpiderAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 1,
                MaxDamage = 6,
                MinHealth = 15,
                MaxHealth = 50,
                Level = 7,
                ArmorValue = 3, // 3
                ParrySounds = false,
                MapChance = 0,
                Weight = 350,
                PrimaryAttackAnimFrames = new int[] {0, 0, 0, 0, 0, 1, -1, 2},
                Team = MobileTeams.Spiders,
            },

            // Troll
            new MobileEnemy()
            {
                ID = 272,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Darkness,
                MaleTexture = 1618,
                FemaleTexture = 1618,
                CorpseTexture = CorpseTexture(1618, 20),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyFleshAtronachMove,
                BarkSound = (int)SoundClips.ArenaTroll,
                AttackSound = (int)SoundClips.EnemyFleshAtronachAttack,
                MinMetalToHit = MaterialTypes.Silver,
                MinDamage = 10,
                MaxDamage = 30,
                MinHealth = 17,
                MaxHealth = 100,
                Level = 11,
                ArmorValue = 3, // 2
                ParrySounds = false,
                MapChance = 0,
                Weight = 800,
                PrimaryAttackAnimFrames = new int[] { 0, 0, 1, 2, 3, -1, 4 },
                ChanceForAttack2 = 50,
                PrimaryAttackAnimFrames2 = new int[] { 5, 6, 7, -1, 8, 9 },
                Team = MobileTeams.Orcs,
            },

            // Gloom Wraith
            new MobileEnemy()
            {
                ID = 273,
                Behaviour = MobileBehaviour.Spectral,
                Affinity = MobileAffinity.Undead,
                MaleTexture = 1622,
                FemaleTexture = 1622,
                CorpseTexture = CorpseTexture(306, 0),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = false,
                BloodIndex = 2,
                MoveSound = (int)SoundClips.EnemyWraithMove,
                BarkSound = (int)SoundClips.ArenaWraith,
                AttackSound = (int)SoundClips.EnemyWraithAttack,
                MinMetalToHit = MaterialTypes.Mithril,
                MinDamage = 30,
                MaxDamage = 60,
                MinHealth = 30,
                MaxHealth = 150,
                Level = 19,
                ArmorValue = 5, // 2
                ParrySounds = false,
                MapChance = 2,
                Weight = 0,
                SeesThroughInvisibility = true,
                LootTableKey = "I",
                NoShadow = true,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, 3, -1, 4 },
                Team = MobileTeams.Undead,
            },

            // Faded Ghost
            new MobileEnemy()
            {
                ID = 274,
                Behaviour = MobileBehaviour.Spectral,
                Affinity = MobileAffinity.Undead,
                MaleTexture = 1623,
                FemaleTexture = 1623,
                CorpseTexture = CorpseTexture(306, 0),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = false,
                BloodIndex = 2,
                MoveSound = (int)SoundClips.EnemyWraithMove,
                BarkSound = (int)SoundClips.ArenaGhost,
                AttackSound = (int)SoundClips.EnemyWraithAttack,
                MinMetalToHit = MaterialTypes.Silver,
                MinDamage = 5,
                MaxDamage = 20,
                MinHealth = 15,
                MaxHealth = 50,
                Level = 7,
                ArmorValue = 2, // 0
                ParrySounds = false,
                MapChance = 1,
                Weight = 0,
                SeesThroughInvisibility = true,
                LootTableKey = "I",
                NoShadow = true,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, 3, -1, 4 },
                Team = MobileTeams.Undead,
            },

            // King Lysandus
            new MobileEnemy()
            {
                ID = 275,
                Behaviour = MobileBehaviour.Spectral,
                Affinity = MobileAffinity.Undead,
                MaleTexture = 1609,
                FemaleTexture = 1609,
                CorpseTexture = CorpseTexture(306, 0),
                HasIdle = false,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                BloodIndex = 2,
                MoveSound = (int)SoundClips.EnemyWraithMove,
                BarkSound = (int)SoundClips.Vengeance,
                AttackSound = (int)SoundClips.EnemyWraithAttack,
                MinMetalToHit = MaterialTypes.Ebony,
                MinDamage = 60,
                MaxDamage = 100,
                MinHealth = 125,
                MaxHealth = 200,
                Level = 26,
                ArmorValue = 15, // 0
                ParrySounds = false,
                MapChance = 1,
                Weight = 0,
                SeesThroughInvisibility = true,
                LootTableKey = "I",
                CastsMagic = true,
                NoShadow = true,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, -1, 3, 4, 3, -1, 2, 1 },
                ChanceForAttack2 = 35,
                PrimaryAttackAnimFrames2 = new int[] { 0, 4, 3, -1, 2, 1 },
                ChanceForAttack3 = 20,
                PrimaryAttackAnimFrames3 = new int[] { 0, 1, 2, -1, 3, 4, 3, -1, 2, 1, 0, 1, 2, -1, 3, 4 },
                HasSpellAnimation = false,
                Team = MobileTeams.Undead,
            },

            // Fire Daemon
            new MobileEnemy()
            {
                ID = 276,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Daedra,
                MaleTexture = 1627,
                FemaleTexture = 1627,
                CorpseTexture = CorpseTexture(1627, 25),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyFireAtronachMove,
                BarkSound = (int)SoundClips.ArenaFireDaemon,
                AttackSound = (int)SoundClips.EnemyFireAtronachAttack,
                MinMetalToHit = MaterialTypes.Mithril,
                MinDamage = 10,
                MaxDamage = 40,
                MinHealth = 50,
                MaxHealth = 200,
                Level = 22,
                ArmorValue = 12, // -5
                ParrySounds = false,
                MapChance = 1,
                Weight = 1000,
                SeesThroughInvisibility = true,
                LootTableKey = "J",
                CastsMagic = true,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, 3, 4, 5, -1, 6, 7, 8 },
                ChanceForAttack2 = 35,
                PrimaryAttackAnimFrames2 = new int[] { 9, 10, -1, 11, 12, 13, 14, 15 },
                HasSpellAnimation = true,
                Team = MobileTeams.Daedra,
            },

            // Ghoul
            new MobileEnemy()
            {
                ID = 277,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Undead,
                MaleTexture = 1626,
                FemaleTexture = 1626,
                CorpseTexture = CorpseTexture(1626, 20),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyZombieMove,
                BarkSound = (int)SoundClips.ArenaGhoul,
                AttackSound = (int)SoundClips.EnemyZombieAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 5,
                MaxDamage = 20,
                MinHealth = 14,
                MaxHealth = 50,
                Level = 5,
                ArmorValue = 0, // 8
                ParrySounds = false,
                MapChance = 0,
                Weight = 150,
                SeesThroughInvisibility = false,
                LootTableKey = "H",
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, 3, 4, -1, 5, 6, 7, 8 },
                ChanceForAttack2 = 50,
                PrimaryAttackAnimFrames2 = new int[] { 9, 10, 11, 12, -1, 13, 14, 15, 16 },
                Team = MobileTeams.Undead,
            },

            // Boar
            new MobileEnemy()
            {
                ID = 278,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Animal,
                MaleTexture = 1616,
                FemaleTexture = 1616,
                CorpseTexture = CorpseTexture(1616, 20),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = false,
                MoveSound = (int)SoundClips.EnemyWereboarMove,
                BarkSound = (int)SoundClips.AnimalPig,
                AttackSound = (int)SoundClips.EnemyWereboarAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 1,
                MaxDamage = 12,
                MinHealth = 15,
                MaxHealth = 50,
                Level = 5,
                ArmorValue = 3, // 5
                ParrySounds = false,
                MapChance = 0,
                Weight = 500,
                SeesThroughInvisibility = false,
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] {0, 1, 2, 3, -1, 4, 5, 6 },
                Team = MobileTeams.Werecreatures,
            },

            // Land Dreugh
            new MobileEnemy()
            {
                ID = 279,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Animal,
                MaleTexture = 1621,
                FemaleTexture = 1621,
                CorpseTexture = CorpseTexture(1621, 20),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = false,
                MoveSound = (int)SoundClips.EnemySprigganMove,
                BarkSound = (int)SoundClips.EnemyOrcBark,
                AttackSound = (int)SoundClips.EnemyOrcAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 12,
                MaxDamage = 24,
                MinHealth = 17,
                MaxHealth = 66,
                Level = 9,
                ArmorValue = 5, // 0
                ParrySounds = false,
                MapChance = 0,
                Weight = 600,
                SeesThroughInvisibility = false,
                LootTableKey = "R",
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, 3, 4, -1, 5, 6, 7 },
                ChanceForAttack2 = 30,
                PrimaryAttackAnimFrames2 = new int[] { 8, 9, -1, 10, 11, 12, 13 },
                Team = MobileTeams.Aquatic,
            },

            // Mountain Lion
            new MobileEnemy()
            {
                ID = 280,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Animal,
                MaleTexture = 1617,
                FemaleTexture = 1617,
                CorpseTexture = CorpseTexture(1617, 20),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = false,
                MoveSound = (int)SoundClips.EnemyTigerMove,
                BarkSound = (int)SoundClips.EnemyTigerBark,
                AttackSound = (int)SoundClips.EnemyTigerAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 5,
                MaxDamage = 15,
                MinHealth = 16,
                MaxHealth = 66,
                Level = 8,
                ArmorValue = 1, // 3
                ParrySounds = false,
                MapChance = 0,
                Weight = 400,
                SeesThroughInvisibility = false,
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 11, 12, 13, 14, -1, 15, 16, 17 },
                ChanceForAttack2 = 30,
                PrimaryAttackAnimFrames2 = new int[] { 0, 1, 2, 3, 4, 5, -1, 6, 7, 8, 9, 10 },
                Team = MobileTeams.Tigers,
            },

            // Mudcrab
            new MobileEnemy()
            {
                ID = 281,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Animal,
                MaleTexture = 1620,
                FemaleTexture = 1620,
                CorpseTexture = CorpseTexture(1620, 20),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = false,
                MoveSound = (int)SoundClips.EnemyScorpionMove,
                BarkSound = (int)SoundClips.EnemyScorpionBark,
                AttackSound = (int)SoundClips.EnemyScorpionAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 1,
                MaxDamage = 8,
                MinHealth = 11,
                MaxHealth = 24,
                Level = 2,
                ArmorValue = 3, // 5
                ParrySounds = false,
                MapChance = 0,
                Weight = 100,
                SeesThroughInvisibility = false,
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, 3, 4, -1, 5, 6 },
                Team = MobileTeams.Aquatic,
            },

            // Ogre
            new MobileEnemy()
            {
                ID = 282,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Daylight,
                MaleTexture = 1619,
                FemaleTexture = 1619,
                CorpseTexture = CorpseTexture(1619, 20),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyGiantMove,
                BarkSound = (int)SoundClips.EnemyGiantBark,
                AttackSound = (int)SoundClips.EnemyGiantAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 30,
                MaxDamage = 50,
                MinHealth = 18,
                MaxHealth = 80,
                Level = 13,
                ArmorValue = 3, // 2
                ParrySounds = false,
                MapChance = 0,
                Weight = 2000,
                SeesThroughInvisibility = false,
                LootTableKey = "F",
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 13, 14, 15, 16, 17, 18, 19, 20, -1, 21, 22, 23, 24, 25 },
                ChanceForAttack2 = 35,
                PrimaryAttackAnimFrames2 = new int[] { 0, 1, 2, 3, 4, 5, -1, 7, 8, 9, 10, 11, 12, 13 },
                Team = MobileTeams.Giants,
            },

            // Will-o'-wisp
            new MobileEnemy()
            {
                ID = 283,
                Behaviour = MobileBehaviour.Spectral,
                Affinity = MobileAffinity.Daylight,
                MaleTexture = 1610,
                FemaleTexture = 1610,
                CorpseTexture = CorpseTexture(254, 47),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = false,
                BloodIndex = 2,     // ProjectN
                MoveSound = (int)SoundClips.AmbientWindMoan,
                BarkSound = (int)SoundClips.AmbientCreepyBirdLaughs,
                AttackSound = (int)SoundClips.AmbientWindMoan,
                MinMetalToHit = MaterialTypes.Silver,   // None
                MinDamage = 1,
                MaxDamage = 10,
                MinHealth = 17,
                MaxHealth = 66,
                Level = 9,
                ArmorValue = 1, // 4
                ParrySounds = false,
                MapChance = 0,
                Weight = 0,
                SeesThroughInvisibility = false,
                CastsMagic = false,
                NoShadow = true,
                GlowColor = new Color(142, 29, 29) * 0.1f,
                PrimaryAttackAnimFrames = new int[] { 0, -1, 1, 2, 3, 4, 5, 6, 7, 8, 9 },
                Team = MobileTeams.Nymphs,
            },

            // Ice Golem
            new MobileEnemy()
            {
                ID = 284,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Golem,
                MaleTexture = 1625,
                FemaleTexture = 1625,
                CorpseTexture = CorpseTexture(1625, 20),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                BloodIndex = 2,     // 0
                MoveSound = (int)SoundClips.EnemyIceAtronachMove,
                BarkSound = (int)SoundClips.ArenaIceGolem,
                AttackSound = (int)SoundClips.EnemyIceAtronachAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 12,
                MaxDamage = 24,
                MinHealth = 18,
                MaxHealth = 66,
                Level = 9,
                ArmorValue = 5, // 2
                ParrySounds = false,
                MapChance = 0,
                Weight = 800,
                SeesThroughInvisibility = false,
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, 3, -1, 3 },
                Team = MobileTeams.Magic,
            },

            // Dremora Churl
            new MobileEnemy()
            {
                ID = 285,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Daedra,
                MaleTexture = 1708,
                FemaleTexture = 1708,
                CorpseTexture = CorpseTexture(1708, 25),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                BloodIndex = 2,     // 0
                MoveSound = (int)SoundClips.EnemyDaedraLordMove,
                BarkSound = (int)SoundClips.EnemyDaedraLordBark,
                AttackSound = (int)SoundClips.EnemyDaedraLordAttack,
                MinMetalToHit = MaterialTypes.Silver,
                MinDamage = 1,
                MaxDamage = 10,
                MinHealth = 15,
                MaxHealth = 50,
                Level = 6,
                ArmorValue = 0, // 5
                ParrySounds = true, // false
                MapChance = 0,
                Weight = 300,
                SeesThroughInvisibility = true,
                LootTableKey = "S",
                CastsMagic = true,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, 3, -1, 4, 5 },
                HasSpellAnimation = true,
                SpellAnimFrames = new int[] { 0, 1, 2, 3, 3 },
                Team = MobileTeams.Daedra,
            },

            // Stone Golem
            new MobileEnemy()
            {
                ID = 286,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Golem,
                MaleTexture = 1631,
                FemaleTexture = 1631,
                CorpseTexture = CorpseTexture(405, 1),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                BloodIndex = 2,     // 0
                MoveSound = (int)SoundClips.EnemyIronAtronachMove,
                BarkSound = (int)SoundClips.ArenaStoneGolem,
                AttackSound = (int)SoundClips.EnemyIronAtronachAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 5,
                MaxDamage = 15,
                MinHealth = 17,
                MaxHealth = 66,
                Level = 8,
                ArmorValue = 5, // 2
                ParrySounds = false,
                MapChance = 0,
                Weight = 600,
                SeesThroughInvisibility = false,
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, 3, 4, -1, 5, 6, 7, 8, 9, 10 },
                Team = MobileTeams.Magic,
            },

            // Dire Ghoul
            new MobileEnemy()
            {
                ID = 287,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Undead,
                MaleTexture = 1630,
                FemaleTexture = 1630,
                CorpseTexture = CorpseTexture(1630, 20),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyZombieMove,
                BarkSound = (int)SoundClips.ArenaGhoul,
                AttackSound = (int)SoundClips.EnemyZombieAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 15,
                MaxDamage = 35,
                MinHealth = 18,
                MaxHealth = 74,
                Level = 13,
                ArmorValue = 2, // 0
                ParrySounds = false,
                MapChance = 0,
                Weight = 200,
                SeesThroughInvisibility = false,
                LootTableKey = "H",
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, 3, 4, -1, 5, 6, 7, 8 },
                ChanceForAttack2 = 50,
                PrimaryAttackAnimFrames2 = new int[] { 9, 10, 11, 12, -1, 13, 14, 15, 16 },
                Team = MobileTeams.Undead,
            },

            // Scamp
            new MobileEnemy()
            {
                ID = 288,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Daedra,
                MaleTexture = 1703,
                FemaleTexture = 1703,
                CorpseTexture = CorpseTexture(1703, 25),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemySpiderMove,
                BarkSound = (int)SoundClips.BattlespireScampBark,
                AttackSound = (int)SoundClips.BattlespireScampAttack,
                MinMetalToHit = MaterialTypes.Silver,
                MinDamage = 1,
                MaxDamage = 8,
                MinHealth = 10,
                MaxHealth = 25,
                Level = 5,
                ArmorValue = 0, // 2
                ParrySounds = false,
                MapChance = 0,
                LootTableKey = "S",
                Weight = 100,
                SeesThroughInvisibility = true,
                SoulPts = 500,
                CastsMagic = true,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, 3, 4, -1, 5, 6, 7 },
                ChanceForAttack2 = 50,
                PrimaryAttackAnimFrames2 = new int[] { 8, 9, 10, 11, -1, 12, 13, 14 },
                Team = MobileTeams.Daedra,
            },

            // Centurion Sphere
            new MobileEnemy()
            {
                ID = 289,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Golem,
                MaleTexture = 1639,
                FemaleTexture = 1639,
                CorpseTexture = CorpseTexture(1639, 20),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = false,
                BloodIndex = 2,
                MoveSound = (int)SoundClips.ActivateGrind,
                BarkSound = (int)SoundClips.EnemyIronAtronachBark,
                AttackSound = (int)SoundClips.EnemyIronAtronachAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 5,
                MaxDamage = 10,
                MinHealth = 10,
                MaxHealth = 40,
                Level = 5,
                ArmorValue = 2, // 8
                ParrySounds = false,
                MapChance = 0,
                Weight = 500,
                SeesThroughInvisibility = false,
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, 3, 4, -1, 5, 6, 7 },
                ChanceForAttack2 = 50,
                PrimaryAttackAnimFrames2 = new int[] { 8, 9, 10, -1, 11, 12, 13, 14, 15 },
                Team = MobileTeams.Magic,
            },

            // Steam Centurion
            new MobileEnemy()
            {
                ID = 290,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Golem,
                MaleTexture = 1640,
                FemaleTexture = 1640,
                CorpseTexture = CorpseTexture(1640, 20),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = false,
                BloodIndex = 2,
                MoveSound = (int)SoundClips.EnemyIronAtronachMove,
                BarkSound = (int)SoundClips.EnemyIronAtronachBark,
                AttackSound = (int)SoundClips.EnemyIronAtronachAttack,
                MinMetalToHit = MaterialTypes.None,
                MinDamage = 10,
                MaxDamage = 20,
                MinHealth = 19,
                MaxHealth = 80,
                Level = 10,
                ArmorValue = 7, // 0
                ParrySounds = false,
                MapChance = 0,
                Weight = 1000,
                SeesThroughInvisibility = false,
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, -1, 3, 4 },
                ChanceForAttack2 = 50,
                PrimaryAttackAnimFrames2 = new int[] { 5, 6, 7, 8, -1, 9, 10, 11, 12, 13, 14, 15, 16 },
                Team = MobileTeams.Magic,
            },

            // Spy
            new MobileEnemy()
            {
                ID = 383,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 484,
                FemaleTexture = 483,
                CorpseTexture = CorpseTexture(380, 1),
                HasIdle = true,
                HasRangedAttack1 = true,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = true,
                MapChance = 0,
                LootTableKey = "O",
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, 4, -1, 5, 0 },
                ChanceForAttack2 = 33,
                PrimaryAttackAnimFrames2 = new int[] { 4, 4, -1, 5, 0, 0 },
                ChanceForAttack3 = 33,
                PrimaryAttackAnimFrames3 = new int[] { 4, -1, 5, 0, 0, 1, -1, 2, 3, 4, -1, 5, 0 },
                RangedAttackAnimFrames = new int[] { 3, 2, 0, 0, 0, -1, 1, 1, 2, 3 },
                Team = MobileTeams.Random,
            },

            // Druid
            new MobileEnemy()
            {
                ID = 384,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 1509,
                FemaleTexture = 1509,
                CorpseTexture = CorpseTexture(380, 1),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = false,
                MapChance = 3,
                LootTableKey = "LR3",   // U
                CastsMagic = true,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 3, 2, 1, 0, -1, 5, 4, 0 },
                ChanceForAttack2 = 33,
                PrimaryAttackAnimFrames2 = new int[] { 0, 1, -1, 3, 2, 1, 0 },
                ChanceForAttack3 = 33,
                PrimaryAttackAnimFrames3 = new int[] { 0, -1, 5, 4, 0 },
                HasSpellAnimation = true,
                SpellAnimFrames = new int[] { 0, 1, 2, 3, 3 },
                Team = MobileTeams.Bears,
            },

            // Guard
            new MobileEnemy()
            {
                ID = 385,
                Behaviour = MobileBehaviour.Guard,
                Affinity = MobileAffinity.Human,
                MaleTexture = 399,
                FemaleTexture = 399,
                CorpseTexture = CorpseTexture(380, 1),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.None,
                BarkSound = (int)SoundClips.Halt,
                AttackSound = (int)SoundClips.None,
                ParrySounds = true,
                MapChance = 0,
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, 4 },
                Team = MobileTeams.CityWatch,
            },

            // Knight Rider
            new MobileEnemy()
            {
                ID = 386,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 1500,
                FemaleTexture = 1506,
                CorpseTexture = CorpseTexture(1500, 20),
                FemaleCorpseTexture = CorpseTexture(1530, 20),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = false,
                MoveSound = (int)SoundClips.HorseClop,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = false,
                MapChance = 1,
                LootTableKey = "T",
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 1, 2, 3, 4, 5, 6, 7, -1, 0 },
                Team = MobileTeams.KnightsAndMages,
            },

            // Rogue Rider
            new MobileEnemy()
            {
                ID = 387,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 1502,
                FemaleTexture = 1501,
                CorpseTexture = CorpseTexture(1500, 20),
                FemaleCorpseTexture = CorpseTexture(1530, 20),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = false,
                MoveSound = (int)SoundClips.HorseClop,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = false,
                MapChance = 1,
                LootTableKey = "O",
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 1, 2, 3, 4, 5, 6, 7, -1, 0 },
                Team = MobileTeams.Criminals,
            },

            // Necromancer Acolyte
            new MobileEnemy()
            {
                ID = 388,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 1511,
                FemaleTexture = 1511,
                CorpseTexture = CorpseTexture(380, 1),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = false,
                MapChance = 1,
                LootTableKey = "LR3",   // U
                CastsMagic = true,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 3, 2, 1, 0, -1, 5, 4, 0 },
                ChanceForAttack2 = 33,
                PrimaryAttackAnimFrames2 = new int[] { 0, 1, -1, 3, 2, 1, 0 },
                ChanceForAttack3 = 33,
                PrimaryAttackAnimFrames3 = new int[] { 0, -1, 5, 4, 0 },
                HasSpellAnimation = true,
                SpellAnimFrames = new int[] { 0, 1, 2, 3, 3 },
                Team = MobileTeams.Undead,
            },

            // Rogue Druid
            new MobileEnemy()
            {
                ID = 389,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 1512,
                FemaleTexture = 1512,
                CorpseTexture = CorpseTexture(380, 1),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = true,
                MapChance = 3,
                LootTableKey = "O",
                CastsMagic = true,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, 4, -1, 5, 0 },
                ChanceForAttack2 = 33,
                PrimaryAttackAnimFrames2 = new int[] { 4, 4, -1, 5, 0, 0 },
                ChanceForAttack3 = 33,
                PrimaryAttackAnimFrames3 = new int[] { 4, -1, 5, 0, 0, 1, -1, 2, 3, 4, -1, 5, 0 },
                HasSpellAnimation = true,
                SpellAnimFrames = new int[] { 0, 1, 2, 3, 3 },
                Team = MobileTeams.Bears,
            },

            // Bounty Hunter
            new MobileEnemy()
            {
                ID = 390,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 1504,
                FemaleTexture = 1504,
                CorpseTexture = CorpseTexture(380, 1),
                HasIdle = true,
                HasRangedAttack1 = true,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.HorseClop,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = true,
                MapChance = 0,
                LootTableKey = "C",
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, 4, -1, 5 },
                ChanceForAttack2 = 50,
                PrimaryAttackAnimFrames2 = new int[] { 3, 4, -1, 5, 0 },
                RangedAttackAnimFrames = new int[] { 3, 2, 0, 0, 0, -1, 1, 1, 2, 3 },
                Team = MobileTeams.KnightsAndMages,
            },

            // Royal Knight
            new MobileEnemy()
            {
                ID = 391,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 1505,
                FemaleTexture = 1505,
                CorpseTexture = CorpseTexture(380, 1),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = true,
                MapChance = 1,
                LootTableKey = "T",
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, 4, -1, 5 },
                ChanceForAttack2 = 50,
                PrimaryAttackAnimFrames2 = new int[] { 3, 4, -1, 5, 0 },
                RangedAttackAnimFrames = new int[] { 3, 2, 0, 0, 0, -1, 1, 1, 2, 3 },
                Team = MobileTeams.KnightsAndMages,
            },

            // Thief Rider
            new MobileEnemy()
            {
                ID = 392,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 1503,
                FemaleTexture = 1503,
                CorpseTexture = CorpseTexture(1500, 20),
                FemaleCorpseTexture = CorpseTexture(1530, 20),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = true,
                MapChance = 2,
                LootTableKey = "O",
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 1, 2, 3, 4, 5, 6, 7, -1, 0 },
                Team = MobileTeams.Criminals,
            },

            // Necromancer Glaivier
            new MobileEnemy()
            {
                ID = 393,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 1510,
                FemaleTexture = 1510,
                CorpseTexture = CorpseTexture(1510, 25),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = true,
                MapChance = 0,
                LootTableKey = "P",   // T
                CastsMagic = true,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, 3, 4, 5, -1, 6, 7, 8, 9, 10 },
                ChanceForAttack2 = 35,
                PrimaryAttackAnimFrames2 = new int[] { 11, 12, 13, -1, 14, 15, 16, 17, 18, -1, 19 },
                ChanceForAttack3 = 20,
                HasSpellAnimation = true,
                SpellAnimFrames = new int[] { 0, 1, 2, 3, 3 },
                Team = MobileTeams.Undead,
            },

            // Necromancer Assassin
            new MobileEnemy()
            {
                ID = 394,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 1515,
                FemaleTexture = 1515,
                CorpseTexture = CorpseTexture(1515, 30),
                HasIdle = true,
                HasRangedAttack1 = true,
                HasRangedAttack2 = true,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = true,
                MapChance = 0,
                LootTableKey = "O",
                CastsMagic = true,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, 3, 4, -1, 5, 6, 7, 8, 9, 10, -1, 11 },
                ChanceForAttack2 = 50,
                PrimaryAttackAnimFrames2 = new int[] { 12, 13, 14, 15, 16, 17, -1, 18, 19, 20, 21, 22, 23 },
                RangedAttackAnimFrames = new int[] { 3, 2, 0, 0, 0, -1, 1, 1, 2, 3 },
                HasSpellAnimation = true,
                SpellAnimFrames = new int[] { 0, 1, 2, 3, 3 },
                Team = MobileTeams.Undead,
            },

            // Dark Slayer
            new MobileEnemy()
            {
                ID = 395,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 1513,
                FemaleTexture = 1514,
                CorpseTexture = CorpseTexture(1513, 25),
                FemaleCorpseTexture = CorpseTexture(1514, 25),
                HasIdle = true,
                HasRangedAttack1 = true,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = true,
                MapChance = 0,
                LootTableKey = "O",
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, 3, -1, 4, 5, 6, 7, 8, 9 },
                ChanceForAttack2 = 35,
                PrimaryAttackAnimFrames2 = new int[] { 10, 11, -1, 12, 13, 14, 15 },
                ChanceForAttack3 = 35,
                PrimaryAttackAnimFrames3 = new int[] { 16, 17, 18, 19, 20, -1, 21, 22 },
                RangedAttackAnimFrames = new int[] { 3, 2, 0, 0, 0, -1, 1, 1, 2, 3 },
                Team = MobileTeams.Criminals,
            },

            // Witch Defender
            new MobileEnemy()
            {
                ID = 396,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 1516,
                FemaleTexture = 1516,
                CorpseTexture = CorpseTexture(1516, 25),
                HasIdle = true,
                HasRangedAttack1 = true,
                HasRangedAttack2 = false,
                CanOpenDoors = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = true,
                MapChance = 1,
                LootTableKey = "P",
                CastsMagic = true,
                PrimaryAttackAnimFrames = new int[] { 0, 1, 2, 3, -1, 4 },
                ChanceForAttack2 = 33,
                PrimaryAttackAnimFrames2 = new int[] { 5, 6, 7, 8, 9, -1, 10, 11, 12, 13, 14 },
                ChanceForAttack3 = 33,
                PrimaryAttackAnimFrames3 = new int[] { 15, 16, 17, 18, 19, -1, 20, 21, 22, 23 },
                HasSpellAnimation = true,
                SpellAnimFrames = new int[] { 0, 1, 2, 3, 3 },
                Team = MobileTeams.Daedra,
            },

            // Spellsword Rider
            new MobileEnemy()
            {
                ID = 397,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 1531,
                FemaleTexture = 1530,
                CorpseTexture = CorpseTexture(1500, 20),
                FemaleCorpseTexture = CorpseTexture(1530, 20),
                HasIdle = true,
                HasRangedAttack1 = true,
                HasRangedAttack2 = false,
                CanOpenDoors = false,
                MoveSound = (int)SoundClips.HorseClop,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = true,
                MapChance = 1,
                LootTableKey = "P",
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, 4, 5 },
                ChanceForAttack2 = 33,
                PrimaryAttackAnimFrames2 = new int[] { 5, 4, 3, -1, 2, 1, 0 },
                ChanceForAttack3 = 33,
                PrimaryAttackAnimFrames3 = new int[] { 0, 1, -1, 2, 2, 1, 0 },
                HasSpellAnimation = false,
                Team = MobileTeams.KnightsAndMages,
            },

            // Archer Rider
            new MobileEnemy()
            {
                ID = 398,
                Behaviour = MobileBehaviour.General,
                Affinity = MobileAffinity.Human,
                MaleTexture = 1533,
                FemaleTexture = 1532,
                CorpseTexture = CorpseTexture(1500, 20),
                FemaleCorpseTexture = CorpseTexture(1530, 20),
                HasIdle = true,
                HasRangedAttack1 = false,
                HasRangedAttack2 = false,
                CanOpenDoors = false,
                PrefersRanged = true,
                MoveSound = (int)SoundClips.EnemyHumanMove,
                BarkSound = (int)SoundClips.EnemyHumanBark,
                AttackSound = (int)SoundClips.EnemyHumanAttack,
                ParrySounds = true,
                MapChance = 0,
                LootTableKey = "C",
                CastsMagic = false,
                PrimaryAttackAnimFrames = new int[] { 0, 1, -1, 2, 3, 4, -1, 5, 0 },
                ChanceForAttack2 = 33,
                PrimaryAttackAnimFrames2 = new int[] { 4, 4, -1, 5, 0, 0 },
                ChanceForAttack3 = 33,
                PrimaryAttackAnimFrames3 = new int[] { 4, -1, 5, 0, 0, 1, -1, 2, 3, 4, -1, 5, 0 },
                Team = MobileTeams.KnightsAndMages,
            },
        };

        #endregion

        #region Helpers

        public static int CorpseTexture(int archive, int record)
        {
            return ((archive << 16) + record);
        }

        public static void ReverseCorpseTexture(int corpseTexture, out int archive, out int record)
        {
            archive = corpseTexture >> 16;
            record = corpseTexture & 0xffff;
        }

        /// <summary>
        /// Build a dictionary of enemies keyed by ID.
        /// Use this once and store for faster enemy lookups.
        /// </summary>
        /// <returns>Resulting dictionary of mobile enemies.</returns>
        public static Dictionary<int, MobileEnemy> BuildEnemyDict()
        {
            Dictionary<int, MobileEnemy> enemyDict = new Dictionary<int, MobileEnemy>();
            foreach (var enemy in Enemies)
            {
                enemyDict.Add(enemy.ID, enemy);
            }

            return enemyDict;
        }

        /// <summary>
        /// Gets enemy definition based on type.
        /// Runs a brute force search for ID, so use sparingly.
        /// Store a dictionary from GetEnemyDict() for faster lookups.
        /// </summary>
        /// <param name="enemyType">Enemy type to extract definition.</param>
        /// <param name="mobileEnemyOut">Receives details of enemy type.</param>
        /// <returns>True if successful.</returns>
        public static bool GetEnemy(MobileTypes enemyType, out MobileEnemy mobileEnemyOut)
        {
            // Cast type enum to ID.
            // You can add additional IDs to enum to create new enemies.
            int id = (int)enemyType;

            // Search for matching definition in enemy list.
            // Don't forget to add new enemy IDs to Enemies definition array.
            for (int i = 0; i < Enemies.Length; i++)
            {
                if (Enemies[i].ID == id)
                {
                    mobileEnemyOut = Enemies[i];
                    return true;
                }
            }

            // No match found, just return an empty definition
            mobileEnemyOut = new MobileEnemy();
            return false;
        }

        /// <summary>
        /// Gets enemy definition based on name.
        /// Runs a brute force search for ID, so use sparingly.
        /// </summary>
        /// <param name="name">Enemy name to extract definition.</param>
        /// <param name="mobileEnemyOut">Receives details of enemy type if found.</param>
        /// <returns>True if successful.</returns>
        public static bool GetEnemy(string name, out MobileEnemy mobileEnemyOut)
        {
            for (int i = 0; i < Enemies.Length; i++)
            {
                if (0 == string.Compare(TextManager.Instance.GetLocalizedEnemyName(Enemies[i].ID), name, StringComparison.InvariantCultureIgnoreCase))
                {
                    mobileEnemyOut = Enemies[i];
                    return true;
                }
            }

            // No match found, just return an empty definition
            mobileEnemyOut = new MobileEnemy();
            return false;
        }

        #endregion

    }
}
