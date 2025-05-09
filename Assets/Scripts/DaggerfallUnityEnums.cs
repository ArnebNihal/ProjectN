// Project:         Daggerfall Unity
// Copyright:       Copyright (C) 2009-2022 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Gavin Clayton (interkarma@dfworkshop.net)
// Contributors:    Arneb
// 
// Notes:
//

using System;

namespace DaggerfallWorkshop
{
    /// <summary>
    /// Supported alpha texture formats for texure reader.
    /// </summary>
    public enum SupportedAlphaTextureFormats
    {
        RGBA444,
        ARGB444,
        RGBA32,
        ARGB32,
    }

    /// <summary>
    /// Types of billboard flats in Daggerfall blocks.
    /// </summary>
    public enum FlatTypes
    {
        Decoration,                             // Decorative flats in RMB and RDB blocks
        NPC,                                    // Non-player characters
        Editor,                                 // Editor/marker flats (TEXTURE.199)
        Animal,                                 // Animated animal flats (TEXTURE.201)
        Light,                                  // Light-source flats (TEXTURE.210)
        Nature,                                 // Climate nature flats (TEXTURE.500-TEXTURE.511)
    }

    /// <summary>
    /// Sub-types of editor flats.
    /// </summary>
    public enum EditorFlatTypes
    {
        Other,                                  // Unused sub-types
        Enter,                                  // Entrance point for dungeons
        Start,                                  // Starting point for cities after fast travel
        FixedMobile,                            // Fixed mobile enemy (same every load)
        RandomMobile,                           // Random mobile enemy (based on dungeon ecology and player level)
        RandomTreasure,                         // Random treasure pile
    }

    /// <summary>
    /// Location climate usage options.
    /// </summary>
    public enum LocationClimateUse
    {
        Disabled,                               // Don't use climate swaps
        UseLocation,                            // Use location climate settings
        Custom,                                 // Use custom climate settings
    }

    /// <summary>
    /// Government types. I put them here because I don't know where else.
    /// Probably the wrong place...
    /// </summary>
    public enum GovernmentType
    {
        None = 0,
        Kingdom = 1,
        Duchy = 2,
        March = 3,
        County = 4,
        Barony = 5,
        Fiefdom = 6,
        Empire = 7,
    }

    public enum WorldAreaNames
    {
        Tamriel = 1,
        Akavir,
        Aldmeris,
        Atmora,
        Lyg,
        Pyandonea,
        Yokuda,
        Isles
    }

    /// <summary>
    /// Province names. These will be used in the near future, I guess.
    /// </summary>
    public enum ProvinceNames
    {
        HighRock = 1,
        Hammerfell = 2,
        Skyrim = 3,
        Morrowind = 4,
        Cyrodiil = 5,
        Sumurset = 6,
        Valenwood = 7,
        Elsweyr = 8,
        BlackMarsh = 9
    }

    /// <summary>
    /// Levels of Encumbrance
    /// </summary>
    public enum EncumbranceLevels
    {
        Unencumbered = 0,
        Lightly_Encumbered = 1,
        Slightly_Burdened = 2,
        Burdened = 3,
        Stressed = 4,
        Overburdened = 5
    }

    /// <summary>
    /// Base types for climate-aware texture sets.
    /// </summary>
    public enum ClimateBases
    {
        Desert,
        Mountain,
        Temperate,
        Swamp,
        Maquis
    }

    /// <summary>
    /// Character age ranges. At the moment, these are used only
    /// during character creation, but maybe later they will find other uses.
    /// </summary>
    public enum AgeRanges
    {
        Infant = 1,
        Child,
        Adolescent,
        YoungAdult,
        Adult,
        OldEnough

    }

    /// <summary>
    /// What each biog section in BiogText.json "OriginalBiogs" refers to.
    /// Not sure if these will be used at all, at the moment they're there for, well, reference.
    /// </summary>
    public enum BiogReference
    {
        EarliestMemories = 0,
        GrowingUp = 1,
        TharnTalk = 2,
        TwentiethYear = 3
    }

    public enum BiogQuestType
    {
        Generic = 0,
        Magic = 1,
        Combat = 2,
        Thievery = 3,
        Languages = 4
    }

    public enum BiogAnswerEffect
    {
        GoldPieces = 0,
        Reputation = 1,
        Item = 2,
        Skill = 3,
        Modifier = 4,
        FriendsAndFoes = 5
    }

    /// <summary>
    /// Climate season modifiers.
    /// </summary>
    public enum ClimateSeason
    {
        Summer,
        Winter,
        Rain,
    }

    /// <summary>
    /// Window textures modifiers.
    /// </summary>
    public enum WindowStyle
    {
        Disabled,
        Day,
        Night,
        Fog,
        Custom,
    }

    /// <summary>
    /// Weather texture modifiers.
    /// </summary>
    public enum WeatherStyle
    {
        Normal = 0,
        Rain1 = 4,
        Rain2 = 5,
        Snow1 = 6,
        Snow2 = 7,
    }

    /// <summary>
    /// Texture sets for nature flats.
    /// Note: Snow sets are only assigned by climate processing.
    /// </summary>
    public enum ClimateNatureSets
    {
        RainForest,              // TEXTURE.500
        SubTropical,             // TEXTURE.501
        Swamp,                   // TEXTURE.502
        Desert,                  // TEXTURE.503
        TemperateWoodland,       // TEXTURE.504
        //SnowWoodland,          // TEXTURE.505
        WoodlandHills,           // TEXTURE.506
        //SnowWoodlandHills,     // TEXTURE.507
        HauntedWoodlands,        // TEXTURE.508
        //SnowHauntedWoodlands,  // TEXTURE.509
        Mountains,               // TEXTURE.510
        //SnowMountains,         // TEXTURE.511
        Maquis,                  // TEXTURE.1030
    }

    public enum DungeonTextureUse
    {
        /// <summary>Don't change dungeon textures.</summary>
        Disabled,
        /// <summary>Use dungeon location textures. Partially implemented.</summary>
        UseLocation_PartiallyImplemented,
        /// <summary>Use custom dungeon texture.</summary>
        Custom,
    }

    /// <summary>
    /// A list of mobile enemy types with ID range 0-42 (monsters) and 128-146 (humanoids).
    /// Do not extend this enum.
    /// </summary>
    public enum MobileTypes
    {
        // Monster IDs are 0-42
        Rat,
        Imp,
        Spriggan,
        GiantBat,
        GrizzlyBear,
        SabertoothTiger,
        Spider,
        Orc,
        Centaur,
        Werewolf,
        Nymph,
        Slaughterfish,
        OrcSergeant,
        Harpy,
        Wereboar,
        SkeletalWarrior,
        Giant,
        Zombie,
        Ghost,
        Mummy,
        GiantScorpion,
        OrcShaman,
        Gargoyle,
        Wraith,
        OrcWarlord,
        FrostDaedra,
        FireDaedra,
        Daedroth,
        Vampire,
        DaedraSeducer,
        VampireAncient,
        DaedraLord,
        Lich,
        AncientLich,
        Dragonling,
        FireAtronach,
        IronAtronach,
        FleshAtronach,
        IceAtronach,
        Horse_Invalid,              // Not used and no matching texture (294 missing). Crashes DF when spawned in-game.
        Dragonling_Alternate,       // Another dragonling. Seems to work fine when spawned in-game.
        Dreugh,
        Lamia,

        // Humanoid IDs are 128-146
        Mage = 128,
        Spellsword,
        Battlemage,
        Sorcerer,
        Healer,
        Nightblade,
        Bard,
        Burglar,
        Rogue,
        Acrobat,
        Thief,
        Assassin,
        Monk,
        Archer,
        Ranger,
        Barbarian,
        Warrior,
        Knight,
        Knight_CityWatch,           // Just called Knight in-game, but renamed CityWatch here for uniqueness. HALT!

        // DEX monsters
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
        SteamCenturion = 290,

        // ProjectN Classes
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

        // No enemy type
        None = (int)0xffff,
    }

    /// <summary>
    /// Mobile animation states.
    /// </summary>
    public enum MobileStates
    {
        Move,                   // Records 0-4      (Flying and swimming mobs also uses this animation set for idle)
        PrimaryAttack,          // Records 5-9      (Usually a melee attack animation)
        Hurt,                   // Records 10-14    (Mob has been struck)
        Idle,                   // Records 15-19    (Frost and ice Daedra have animated idle states)
        RangedAttack1,          // Records 20-24    (Bow attack)
        Spell,                  // Records 20-24 or, if absent, copy of PrimaryAttack
        RangedAttack2,          // Records 25-29    (Bow attack on 475, 489, 490 only, absent on other humanoids)
        SeducerTransform1,      // Record 23        (Crouch and grow wings)
        SeducerTransform2,      // Record 22        (Stand and spread wings)
    }

    /// <summary>
    /// Mobile enemy behaviour groups.
    /// </summary>
    public enum MobileBehaviour
    {
        General,            // General ground-based enemy
        Flying,             // Flying enemy
        Aquatic,            // Water-only enemy
        Spectral,           // Ghosts with flight and transparent effect
        Guard,              // City Watch - HALT!
    }

    /// <summary>
    /// Mobile affinity for resists/weaknesses, grouping, etc.
    /// This could be extended into a set of flags for multi-affinity creatures.
    /// </summary>
    public enum MobileAffinity
    {
        None,               // No special affinity
        Daylight,           // Daylight creatures (centaur, giant, nymph, spriggan, harpy, dragonling)
        Darkness,           // Darkness creatures (imp, gargoyle, orc, vampires, werecreatures)
        Undead,             // Undead monsters (skeleton, liches, zombie, mummy, ghosts)
        Animal,             // Animals (bat, rat, bear, tiger, spider, scorpion)
        Daedra,             // Daedra (daedroth, fire, frost, lord, seducer)
        Golem,              // Golems (flesh, fire, frost, iron)
        Water,              // Water creatures (dreugh, slaughterfish, lamia)
        Skeletal,           // Skeletal creatures (skeleton soldier and warrior)
        Human,              // A human creature
    }

    /// <summary>
    /// Mobile teams.
    /// </summary>
    public enum MobileTeams
    {
        Random = -1,
        PlayerEnemy,
        PlayerAlly,
        Vermin,
        Spriggans,
        Bears,
        Tigers,
        Spiders,
        Orcs,
        Centaurs,
        Werecreatures,
        Nymphs,
        Aquatic,
        Harpies,
        Undead,
        Giants,
        Scorpions,
        Magic,
        Daedra,
        Dragonlings,
        KnightsAndMages,
        Criminals,
        CityWatch,
    }

    /// <summary>
    /// Mobile gender.
    /// All monsters have an unspecified gender and no male/female variations.
    /// When specifying gender for humanoids, a value of unspecified will randomly choose between male/female.
    /// </summary>
    public enum MobileGender
    {
        Unspecified,
        Female,
        Male,
    }

    /// <summary>
    /// Reaction settings for mobiles.
    /// </summary>
    public enum MobileReactions
    {
        Hostile,            // Immediately hostile
        Passive,            // Not hostile unless attacked
        Custom,             // Reaction controlled elsewhere
    }

    /// <summary>
    /// Basic combat flags for mobiles.
    /// Every mobile has a basic melee attack available.
    /// This can be extended to create more diverse foes with
    /// a wider range of behaviours.
    /// </summary>
    public enum MobileCombatFlags
    {
        Ranged = 1,         // Ranged weapon available
        Spells = 2,         // Spellcasting available
    }

    /// <summary>
    /// Modes for user when activating central object.
    /// </summary>
    public enum PlayerActivateModes
    {
        Steal,
        Grab,
        Info,
        Talk,
    }

    /// <summary>
    /// Door types found around locations.
    /// </summary>
    public enum DoorTypes
    {
        None,                   // No door type detected
        Building,               // General building doors for both enctrance and exit
        DungeonEntrance,        // Enter a dungeon
        DungeonExit,            // Exit a dungeon
    }

    /// <summary>
    /// Various metal types in Daggerfall.
    /// This is a Daggerfall Unity enum.
    /// For enum matching native data see Items/ItemEnums.cs.
    /// </summary>
    public enum MetalTypes
    {
        // ProjectN: moved Orcish to be on par with Mithril
        Leather = -1,
        None,
        Iron,
        Steel,
        Silver,
        Elven,
        Glass,
        Dwarven,
        Orcish,
        Mithril,
        Adamantium,
        Ebony,
        Daedric
    }

    /// <summary>
    /// Available dye colours for armour, weapons, and clothing.
    /// Values match known Daggerfall colour indices.
    /// May change at a later date with new research.
    /// </summary>
    public enum DyeColors
    {
        None = -1,
        // Clothing dyes
        Blue = 0,
        Grey = 1,
        Red = 2,
        DarkBrown = 3,
        Purple = 4,
        LightBrown = 5,
        White = 6,
        Aquamarine = 7,
        Yellow = 8,
        Green = 9,
        Olive = 10,
        Amber = 11,
        DarkGrey = 12,

        // 10-14 Unknown or not observed

        // Weapon and armour dyes
        Iron = 15,
        Steel = 16,
        Leather = 17,
        Unchanged = 18,
        SilverOrElven = 18, // This enum kept for compatibility with older saves
        Silver = 18,
        Elven = 19,
        Glass = 20,
        Dwarven = 21,
        Mithril = 22,
        Adamantium = 23,
        Ebony = 24,
        Orcish = 25,
        Daedric = 26        
    }

    /// <summary>
    /// Supported targets for dye changes.
    /// </summary>
    public enum DyeTargets
    {
        BasicClothing,
        WeaponsAndArmor,
        SteelClothing,      // Formal Brassiere 4
        YellowClothing,     // Edoric
        LeatherClothing,    // Shoes and boots
        RedClothing,        // Sandals, Vest and others
        BlackClothing,      // Khajiit Suit
        GreenClothing,      // Vest
        LightBrownClothing, // Casual Pants 19 and Formal Cloak
        PurpleClothing,     // Just a third level for dresses 39 and 40
        DarkBrownClothing   // Kimono
    }

    public enum ClothCraftsmanship
    {
        None = -1,
        Cheap,
        Normal,
        Fancy,
        Extravagant,
        Exquisite
    }

    /// <summary>
    /// Generic weapon types in Daggerfall.
    /// </summary>
    public enum WeaponTypes
    {
        None = -1,
        LongBlade,
        LongBlade_Magic,
        Staff,
        Staff_Magic,
        Dagger,
        Dagger_Magic,
        Mace,
        Mace_Magic,
        Flail,
        Flail_Magic,
        Warhammer,
        Warhammer_Magic,
        Battleaxe,
        Battleaxe_Magic,
        Bow,
        Crossbow,
        Melee,
        Werecreature,
        DaggerExotic,
        DaggerExotic_Magic,
        TantoExotic,
        TantoExotic_Magic,
        StaffExotic,
        StaffExotic_Magic,
        ShortswordExotic,
        ShortswordExotic_Magic,
        WakazashiExotic,
        WakazashiExotic_Magic,
        BroadswordExotic,
        BroadswordExotic_Magic,
        SaberExotic,
        SaberExotic_Magic,
        LongswordExotic,
        LongswordExotic_Magic,
        KatanaExotic,
        KatanaExotic_Magic,
        ClaymoreExotic,
        ClaymoreExotic_Magic,
        Dai_KatanaExotic,
        Dai_KatanaExotic_Magic,
        MaceExotic,
        MaceExotic_Magic,
        FlailExotic,
        FlailExotic_Magic,
        WarhammerExotic,
        WarhammerExotic_Magic,
        Battle_AxeExotic,
        Battle_AxeExotic_Magic,
        War_AxeExotic,
        War_AxeExotic_Magic,
        Short_BowExotic,
        Short_BowExotic_Magic,
        Long_BowExotic,
        Long_BowExotic_Magic,
        CrossbowExotic,
        CrossbowExotic_Magic
    }

    /// <summary>
    /// Defines how a weapon is aligned.
    /// </summary>
    public enum WeaponAlignment
    {
        Left,
        Center,
        Right,
    }

    /// <summary>
    /// Weapon animation states.
    /// </summary>
    public enum WeaponStates
    {
        Idle,               // Record 0
        StrikeDown,         // Record 1
        StrikeDownLeft,     // Record 2
        StrikeLeft,         // Record 3
        StrikeRight,        // Record 4
        StrikeDownRight,    // Record 5
        StrikeUp,           // Record 6
    }

    /// <summary>
    /// Quick AudioSource presets for DaggerfallAudioSource.
    /// These make minor changes to peer AudioSource component.
    /// </summary>
    public enum AudioPresets
    {
        None,                       // No changes to AudioSource
        OnDemand,                   // PlayOnAwake=false, Loop=false
        LoopOnAwake,                // PlayOnAwake=true, Loop=true
        LoopOnDemand,               // PlayOnAwake=false, Loop=true
        LoopIfPlayerNear,           // PlayOnAwake=true, Loop=true, distanceCheck=true
        PlayRandomlyIfPlayerNear,   // PlayOnAwake=false, Loop=false, distanceCheck=true, playRandomly=true
    }

    /// <summary>
    /// States for action doors and other objects.
    /// </summary>
    public enum ActionState
    {
        Start,
        PlayingForward,
        PlayingReverse,
        End,
    }

    /// <summary>
    /// Defines various types of living entities in the world.
    /// </summary>
    public enum EntityTypes
    {
        None,
        Player,
        CivilianNPC,
        StaticNPC,
        EnemyMonster,
        EnemyClass,
    }

    /// <summary>
    /// Supported ImageReader file types.
    /// </summary>
    public enum ImageTypes
    {
        None,
        TEXTURE,
        IMG,
        CIF,
        RCI,
        CFA,
        BSS,
        GFX,
    }

    /// <summary>
    /// Defines character's hands.
    /// </summary>
    public enum CharacterHands
    {
        None,
        Left,
        Right,
        Both,
    }

    /// <summary>
    /// Defines how an item is held.
    /// </summary>
    public enum ItemHands
    {
        None,               // Item is not held in the hands
        Either,             // Can wield in either left or right hand (off-hand equip image available)
        Both,               // Can wield in both hands only
        LeftOnly,           // Can wield in left hand only (e.g. shields)
        RightOnly,          // Can wield in right hand only
    }

    /// <summary>
    /// Various container images for inventory management.
    /// Not sure if all of these are used.
    /// May change at a later date.
    /// </summary>
    public enum InventoryContainerImages
    {
        Corpse1,
        Corpse2,
        Ground,
        Wagon,
        Shelves,
        Chest,
        Merchant,
        Anvil,
        Magic,
        Backpack,
        Corpse3,
    }

    /// <summary>
    /// Supported loot containers in world.
    /// </summary>
    public enum LootContainerTypes
    {
        Nothing,
        RandomTreasure,
        CorpseMarker,
        DroppedLoot,
        ShopShelves,
        HouseContainers
    }

    /// <summary>
    /// Controls how weapons are held in the player's hands.
    /// </summary>
    public enum Handedness
    {
        DrawRight,      // Classic Daggerfall behaviour
        DrawLeft,       // Same as classic, but drawn in left hand
        DrawByHand,     // Draws based on hand equipped
        DuelWield,      // Draws based on hand plus dual-wield
    }

    /// <summary>
    /// Basic context of world object, such as dropped loot.
    /// </summary>
    public enum WorldContext
    {
        Nothing,
        Exterior,
        Interior,
        Dungeon,
    }

    /// <summary>
    /// Text macros used by various systems (quest messages, items, stats, etc.).
    /// Macro output depends on the characters wrapping symbol name.
    /// Not all objects support all macro types.
    /// </summary>
    public enum MacroTypes
    {
        None,
        NameMacro1,         // _symbol_    - replaced with name of symbol itself, such as person, house, or business
        NameMacro2,         // __symbol_   - replaced with name of city where symbol is found
        NameMacro3,         // ___symbol_  - replaced with name of place, such as dungeon name or house where symbol is found
        NameMacro4,         // ____symbol_ - replaced with name of region where symbol is found
        DetailsMacro,       // =symbol_    - replaced with detail based on target symbol type (e.g. days remaining on a clock, player class, enemy name)
        FactionMacro,       // ==symbol_   - replaced with faction of target symbol, such as an NPC faction
        ContextMacro,       // %symbol     - replaced with output based on context (e.g. pronoun macros relate back to previous NPC/foe symbol in source text)
        BindingMacro,       // =#symbol_   - replaced with current keybind for symbol action (Daggerfall Unity only)
    }

    /// <summary>
    /// Types of sites player can be sent to for quests.
    /// </summary>
    public enum SiteTypes
    {
        None,
        Town,
        Dungeon,
        Building,
    }

    /// <summary>
    /// Types of markers in a location from TEXTURE.199.
    /// Only used for quest resource placement right now.
    /// Will be expanded later to include additional marker types (rest, ladders, etc.)
    /// </summary>
    public enum MarkerTypes
    {
        None = -1,
        QuestSpawn = 11,        // Quest spawn marker (Foe/Person resources)
        QuestItem = 18,         // Quest item marker (Item resource)
    }

    /// <summary>
    /// Marker preference when allocating quest resources to a static marker index.
    /// </summary>
    public enum MarkerPreference
    {
        Default,                // Assign Foe/Person to specified questmarker index and Item to specified itemmarker index
        UseQuestMarker,         // Assign Foe/Person/Item to specified questmarker index
        AnyMarker,              // Assign Foe/Person/Item randomly from combined questmarker and itemmarker pool
    }

    /// <summary>
    /// Phases of the moons.
    /// </summary>
    public enum LunarPhases
    {
        None = -1,
        New = 0,
        OneWax = 1,
        HalfWax = 2,
        ThreeWax = 3,
        Full = 4,
        ThreeWane = 5,
        HalfWane = 6,
        OneWane = 7,
    }

    /// <summary>
    /// Lycanthropy variants.
    /// </summary>
    public enum LycanthropyTypes
    {
        None = 0,
        Werewolf = 1,
        Wereboar = 2,
    }

    /// <summary>
    /// Vampire clans.
    /// </summary>
    public enum VampireClans
    {
        None = 0,
        Vraseth = 150,
        Haarvenu = 151,
        Thrafey = 152,
        Lyrezi = 153,
        Montalion = 154,
        Khulari = 155,
        Garlythi = 156,
        Anthotis = 157,
        Selenu = 158,
    }

    /// <summary>
    /// Regions across the game world.
    /// Values maps to zero-based region index.
    /// Not all regions used in game.
    /// </summary>
    public enum DaggerfallRegions
    {
        AlikrDesert = 0,
        DragontailMountains = 1,
        GlenpointFoothills = 2,
        DaggerfallBluffs = 3,
        YeorthBurrowland = 4,
        Dwynnen = 5,
        RavennianForest = 6,
        Devilrock = 7,
        MaleknaForest = 8,
        IsleOfBalfiera = 9,
        Bantha = 10,
        Dakfron = 11,
        IslandsInTheWesternIlliacBay = 12,
        TamarilynPoint = 13,
        LainlynCliffs = 14,
        BjoulsaeRiver = 15,
        WrothgarianMountains = 16,
        Daggerfall = 17,
        Glenpoint = 18,
        Betony = 19,
        Sentinel = 20,
        Anticlere = 21,
        Lainlyn = 22,
        Wayrest = 23,
        GenTemHighRockVillage = 24,
        GenRaiHammerfellVillage = 25,
        OrsiniumArea = 26,
        SkeffingtonWood = 27,
        HammerfellBayCoast = 28,
        HammerfellSeaCoast = 29,
        HighRockBayCoast = 30,
        HighRockSeaCoast = 31,
        Northmoor = 32,
        Menevia = 33,
        Alcaire = 34,
        Koegria = 35,
        Bhoriane = 36,
        Kambria = 37,
        Phrygias = 38,
        Urvaius = 39,
        Ykalon = 40,
        Daenia = 41,
        Shalgora = 42,
        AbibonGora = 43,
        Kairou = 44,
        Pothago = 45,
        Myrkwasa = 46,
        Ayasofya = 47,
        Tigonus = 48,
        Kozanset = 49,
        Satakalaam = 50,
        Totambu = 51,
        Mournoth = 52,
        Ephesus = 53,
        Santaki = 54,
        Antiphyllos = 55,
        Bergama = 56,
        Gavaudon = 57,
        Tulune = 58,
        GlenumbraMoors = 59,
        IlessanHills = 60,
        Cybiades = 61,
    }

    /// <summary>
    /// State of smaller dungeons setting to be serialized with quest data.
    /// </summary>
    public enum QuestSmallerDungeonsState
    {
        NotSet,
        Disabled,
        Enabled,
    }

    /// <summary>
    /// Quick way to reference a text collection.
    /// The current value of collection name is read from appropriate field in scene TextManager singleton.
    /// </summary>
    public enum TextCollections
    {
        Internal,
        TextRSC,
        TextFlats,
        TextQuests,
        TextLocations,
    }

    /// <summary>
    /// Antialiasing methods supported by core.
    /// </summary>
    public enum AntiAliasingMethods
    {
        None = 0,   // No anti-aliasing
        FXAA = 1,   // Fast approximate anti-aliasing (FXAA)
        SMAA = 2,   // Subpixel morphilogical anti-aliasing (SMAA)
        TAA = 3,    // Temporal anti-aliasing (TAA)
    }

    /// <summary>
    /// Retro Mode supported aspect ratio corrections.
    /// </summary>
    public enum RetroModeAspects
    {
        Off = 0,                // No aspect correction in retro mode
        FourThree = 1,          // 4:3 aspect correction in retro mode
        SixteenTen = 2,         // 16:10 aspect correction in retro mode
    }

    /// <summary>
    /// Core game effects settings groups for deploying some or all settings.
    /// </summary>
    [Flags]
    public enum CoreGameEffectSettingsGroups
    {
        Nothing = 0,
        Antialiasing = 1,
        AmbientOcclusion = 2,
        Bloom = 4,
        MotionBlur = 8,
        Vignette = 16,
        DepthOfField = 32,
        Dither = 64,
        ColorBoost = 128,
        RetroMode = 256,
        Reserved512 = 512,
        Everything = 0xffff,
    }
}
