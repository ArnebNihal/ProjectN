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

namespace DaggerfallWorkshop.Game.Entity
{
    //
    // Note: Do not modify these enums as they map directly to native data values.
    //

    /// <summary>
    /// Entity genders.
    /// </summary>
    public enum Genders
    {
        Male,
        Female,
    }

    /// <summary>
    /// Entity races.
    /// </summary>
    public enum Races
    {
        None = -1,
        Breton = 1,
        Redguard = 2,
        Nord = 3,
        DarkElf = 4,
        HighElf = 5,
        WoodElf = 6,
        Khajiit = 7,
        Argonian = 8,
        Imperial = 9,
        Orc = 10,
        Vampire = 51,
        Werewolf = 52,
        Wereboar = 53,
    }

    /// <summary>
    /// Entity class careers.
    /// </summary>
    public enum ClassCareers
    {
        None = -1,
        Mage = 0,
        Spellsword = 1,
        Battlemage = 2,
        Sorcerer = 3,
        Healer = 4,
        Nightblade = 5,
        Bard = 6,
        Burglar = 7,
        Rogue = 8,
        Acrobat = 9,
        Thief = 10,
        Assassin = 11,
        Monk = 12,
        Archer = 13,
        Ranger = 14,
        Barbarian = 15,
        Warrior = 16,
        Knight = 17,

        // ProjectN enemy classes
        Spy = 383,

        // DEX enemy classes
        Druid = 384,
        Guard = 385,
        KnightRider = 386,
        RogueRider = 387,
        NecroAcolyte = 388,
        RogueDruid = 389,
        BountyHunter = 390,
        RoyalKnight = 391,
        ThiefRider = 392,
        NecroGlaive = 393,
        NecroAssassin = 394,
        DarkBrotherhood = 395,
        WitchDefender = 396,
        SpellswordRider = 397,
        ArcherRider = 398,
    }

    /// <summary>
    /// Skills
    /// </summary>
    public enum Skills
    {
        None = -1,
        Medical = 0,
        Etiquette = 1,
        Streetwise = 2,
        Jumping = 3,
        Orcish = 4,
        Harpy = 5,
        Giantish = 6,
        Dragonish = 7,
        Nymph = 8,
        Daedric = 9,
        Spriggan = 10,
        Centaurian = 11,
        Impish = 12,
        Lockpicking = 13,
        Mercantile = 14,
        Pickpocket = 15,
        Stealth = 16,
        Swimming = 17,
        Climbing = 18,
        Backstabbing = 19,
        Dodging = 20,
        Running = 21,
        Destruction = 22,
        Restoration = 23,
        Illusion = 24,
        Alteration = 25,
        Thaumaturgy = 26,
        Mysticism = 27,
        ShortBlade = 28,
        LongBlade = 29,
        HandToHand = 30,
        Axe = 31,
        BluntWeapon = 32,
        Archery = 33,
        CriticalStrike = 34,
        LightArmor = 35,
        MediumArmor = 36,
        HeavyArmor = 37,
        Block = 38,
        Disguise = 39,
        Conjuration = 40,
        Enchant = 41,
        Alchemy = 42
    }

    /// <summary>
    /// Entity monster careers.
    /// </summary>
    public enum MonsterCareers
    {
        None = -1,
        Rat = 0,
        Imp = 1,
        Spriggan = 2,
        GiantBat = 3,
        GrizzlyBear = 4,
        SabertoothTiger = 5,
        Spider = 6,
        Orc = 7,
        Centaur = 8,
        Werewolf = 9,
        Nymph = 10,
        Slaughterfish = 11,
        OrcSergeant = 12,
        Harpy = 13,
        Wereboar = 14,
        SkeletalWarrior = 15,
        Giant = 16,
        Zombie = 17,
        Ghost = 18,
        Mummy = 19,
        GiantScorpion = 20,
        OrcShaman = 21,
        Gargoyle = 22,
        Wraith = 23,
        OrcWarlord = 24,
        FrostDaedra = 25,
        FireDaedra = 26,
        Daedroth = 27,
        Vampire = 28,
        DaedraSeducer = 29,
        VampireAncient = 30,
        DaedraLord = 31,
        Lich = 32,
        AncientLich = 33,
        Dragonling = 34,
        FireAtronach = 35,
        IronAtronach = 36,
        FleshAtronach = 37,
        IceAtronach = 38,
        Horse_Invalid = 39,             // Not used and no matching texture (294 missing). Crashes DF when spawned in-game.
        Dragonling_Alternate = 40,      // Another dragonling. Seems to work fine when spawned in-game.
        Dreugh = 41,
        Lamia = 42,
        Goblin = 256,
        Homunculus = 257,
        Lizardman = 258,
        LizardWarrior = 259,
        Bat = 260,
        Medusa = 261,
        Wolf = 262,
        SnowWolf = 263,
        HellHound = 264,
        Grotesque = 265,
        SkeletalSoldier = 266,
        Dog = 267,
        MountainNymph = 268,
        Minotaur = 269,
        IronGolem = 270,
        BloodSpider = 271,
        Troll = 272,
        GloomWraith = 273,
        FadedGhost = 274,
        KingLysandus = 275,
        FireDaemon = 276,
        Ghoul = 277,
        Boar = 278,
        LandDreugh = 279,
        MountainLion = 280,
        Mudcrab = 281,
        Ogre = 282,
        Wisp = 283,
        IceGolem = 284,
        Dremora = 285,
        StoneGolem = 286,
        DireGhoul = 287,
        Scamp = 288,
        CenturionSphere = 289,
        SteamCenturion = 290
    }

    /// <summary>
    /// Player reflex settings for enemy speed.
    /// </summary>
    public enum PlayerReflexes
    {
        VeryHigh = 0,
        High = 1,
        Average = 2,
        Low = 3,
        VeryLow = 4,
    }

    /// <summary>
    /// Varying visibility type for entities.
    /// </summary>
    [Flags]
    public enum MagicalConcealmentFlags
    {
        None = 0,
        InvisibleNormal = 1,
        InvisibleTrue = 2,
        BlendingNormal = 4,
        BlendingTrue = 8,
        ShadeNormal = 16,
        ShadeTrue = 32,
    }
}
