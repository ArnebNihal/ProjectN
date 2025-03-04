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
using DaggerfallConnect;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Game.Utility;

namespace DaggerfallWorkshop.Utility
{
    #region Encounter Tables

    /// <summary>
    /// Static definitions for random encounters based on dungeon type, from FALL.EXE.
    /// All lists from classic have 20 entries.
    /// These are generally ordered from low-level through to high-level encounters.
    /// </summary>
    public static class RandomEncounters
    {
        public static RandomEncounterTable[] EncounterTables = new RandomEncounterTable[]
        {
            // Crypt - Index0
            new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.Crypt,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Rat,
                    MobileTypes.Bat,                // MobileTypes.SkeletalWarrior,
                    MobileTypes.SkeletalSoldier,    // MobileTypes.GiantBat,
                    MobileTypes.Spider,             // MobileTypes.Rat,
                    MobileTypes.Ghoul,              // MobileTypes.SkeletalWarrior,
                    MobileTypes.SkeletalSoldier,    // MobileTypes.GiantBat,
                    MobileTypes.FadedGhost,         // MobileTypes.Mummy,
                    MobileTypes.Ghoul,              // MobileTypes.SkeletalWarrior,
                    MobileTypes.SkeletalWarrior,    // MobileTypes.Spider,
                    MobileTypes.Zombie,             // MobileTypes.Zombie,
                    MobileTypes.Mummy,              // MobileTypes.Ghost,
                    MobileTypes.Ghost,              // MobileTypes.Zombie,
                    MobileTypes.DireGhoul,          // MobileTypes.Zombie,
                    MobileTypes.SkeletalWarrior,    // MobileTypes.Ghost,
                    MobileTypes.Wraith,             // MobileTypes.Ghost,
                    MobileTypes.Medusa,             // MobileTypes.Wraith,
                    MobileTypes.Vampire,            // MobileTypes.Wraith,
                    MobileTypes.GloomWraith,        // MobileTypes.Vampire,
                    MobileTypes.VampireAncient,
                    MobileTypes.GloomWraith,        // MobileTypes.Lich,
                },
            },

            // Orc Stronghold - Index1
            new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.OrcStronghold,
                Enemies = new MobileTypes[]
                {                
                    MobileTypes.Bat,                
                    MobileTypes.Goblin,
                    MobileTypes.GiantBat,
                    MobileTypes.Spider,
                    MobileTypes.Orc,
                    MobileTypes.Boar,
                    MobileTypes.BloodSpider,
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.OrcSergeant,
                    MobileTypes.Giant,
                    MobileTypes.Troll,
                    MobileTypes.Ogre,
                    MobileTypes.OrcShaman,
                    MobileTypes.GiantScorpion,
                    MobileTypes.OrcWarlord,
                    MobileTypes.OrcShaman,
                    MobileTypes.Daedroth,
                    MobileTypes.GiantScorpion,
                    MobileTypes.OrcShaman,
                    MobileTypes.OrcWarlord
                },
            },

            // Human Stronghold - Index2
            new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.HumanStronghold,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Rat,
                    MobileTypes.Warrior,
                    MobileTypes.Archer,
                    MobileTypes.Mage,
                    MobileTypes.GiantBat,
                    MobileTypes.Battlemage,
                    MobileTypes.Spellsword,
                    MobileTypes.Warrior,
                    MobileTypes.Archer,
                    MobileTypes.Knight,
                    MobileTypes.Monk,
                    MobileTypes.Mage,
                    MobileTypes.Spellsword,
                    MobileTypes.Battlemage,
                    MobileTypes.Archer,
                    MobileTypes.Knight,
                    MobileTypes.Warrior,
                    MobileTypes.Monk,
                    MobileTypes.Battlemage,
                    MobileTypes.Warrior,
                    MobileTypes.Spy,
                },
            },

            // Prison - Index3
            new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.Prison,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Rat,
                    MobileTypes.Guard,
                    MobileTypes.Burglar,
                    MobileTypes.Rat,
                    MobileTypes.Guard,
                    MobileTypes.GiantBat,
                    MobileTypes.Thief,
                    MobileTypes.Burglar,
                    MobileTypes.Guard,
                    MobileTypes.Nightblade,
                    MobileTypes.Rogue,
                    MobileTypes.Thief,
                    MobileTypes.Guard,
                    MobileTypes.Barbarian,
                    MobileTypes.Nightblade,
                    MobileTypes.Thief,
                    MobileTypes.Burglar,
                    MobileTypes.Guard,
                    MobileTypes.BountyHunter,
                    MobileTypes.Assassin
                },
            },

            // Desecrated Temple - Index4
            new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.DesecratedTemple,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Bat,
                    MobileTypes.Healer,
                    MobileTypes.SkeletalSoldier,
                    MobileTypes.Scamp,
                    MobileTypes.Ghoul,
                    MobileTypes.Monk,
                    MobileTypes.Dremora,
                    MobileTypes.FadedGhost,
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.Healer,
                    MobileTypes.Ghost,
                    MobileTypes.Mummy,
                    MobileTypes.Wraith,
                    MobileTypes.Monk,
                    MobileTypes.FrostDaedra,
                    MobileTypes.FireDaedra,
                    MobileTypes.Daedroth,
                    MobileTypes.DaedraSeducer,
                    MobileTypes.Knight,
                    MobileTypes.DaedraLord,
                },
            },

            // Mine - Index5
            new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.Mine,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Rat,
                    MobileTypes.Bat,
                    MobileTypes.Burglar,
                    MobileTypes.Spider,
                    MobileTypes.CenturionSphere,
                    MobileTypes.Thief,
                    MobileTypes.BloodSpider,
                    MobileTypes.StoneGolem,
                    MobileTypes.IronGolem,
                    MobileTypes.Giant,
                    MobileTypes.Rogue,
                    MobileTypes.SteamCenturion,
                    MobileTypes.Thief,
                    MobileTypes.Ogre,
                    MobileTypes.IronAtronach,
                    MobileTypes.FireAtronach,
                    MobileTypes.Thief,
                    MobileTypes.FireDaedra,
                    MobileTypes.Nightblade,
                    MobileTypes.FireDaemon,
                },
            },

            // Natural Cave - Index6
            new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.NaturalCave,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Mudcrab,
                    MobileTypes.Bat,
                    MobileTypes.Spriggan,
                    MobileTypes.GrizzlyBear,
                    MobileTypes.Wolf,
                    MobileTypes.Nymph,
                    MobileTypes.Wisp,
                    MobileTypes.MountainLion,
                    MobileTypes.SnowWolf,
                    MobileTypes.LandDreugh,
                    MobileTypes.Troll,
                    MobileTypes.Wisp,
                    MobileTypes.Ranger,
                    MobileTypes.Gargoyle,
                    MobileTypes.Minotaur,
                    MobileTypes.Monk,
                    MobileTypes.Medusa,
                    MobileTypes.Barbarian,
                    MobileTypes.Dragonling,
                    MobileTypes.Lich,
                },
            },

            // Coven - Index7
            new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.Coven,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Bat,
                    MobileTypes.Imp,
                    MobileTypes.GiantBat,
                    MobileTypes.WitchDefender,
                    MobileTypes.Scamp,
                    MobileTypes.Nymph,
                    MobileTypes.Harpy,
                    MobileTypes.Dremora,
                    MobileTypes.WitchDefender,
                    MobileTypes.StoneGolem,
                    MobileTypes.IronGolem,
                    MobileTypes.Homunculus,
                    MobileTypes.WitchDefender,
                    MobileTypes.FleshAtronach,
                    MobileTypes.FireAtronach,
                    MobileTypes.FrostDaedra,
                    MobileTypes.WitchDefender,
                    MobileTypes.Daedroth,
                    MobileTypes.DaedraSeducer,
                    MobileTypes.DaedraLord,
                },
            },

            // Vampire Haunt - Index8
            new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.VampireHaunt,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Bat,
                    MobileTypes.GiantBat,
                    MobileTypes.SkeletalSoldier,
                    MobileTypes.Spider,
                    MobileTypes.Wolf,
                    MobileTypes.Nymph,
                    MobileTypes.BloodSpider,
                    MobileTypes.Werewolf,
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.SnowWolf,
                    MobileTypes.Ghost,
                    MobileTypes.Mummy,
                    MobileTypes.Vampire,
                    MobileTypes.Wraith,
                    MobileTypes.Vampire,
                    MobileTypes.GloomWraith,
                    MobileTypes.VampireAncient,
                    MobileTypes.Vampire,
                    MobileTypes.VampireAncient,
                    MobileTypes.VampireAncient,
                },
            },

            // Laboratory - Index9
            new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.Laboratory,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Imp,
                    MobileTypes.Mage,
                    MobileTypes.Sorcerer,
                    MobileTypes.Battlemage,
                    MobileTypes.Grotesque,
                    MobileTypes.Mage,
                    MobileTypes.StoneGolem,
                    MobileTypes.Sorcerer,
                    MobileTypes.IceGolem,
                    MobileTypes.IronGolem,
                    MobileTypes.Homunculus,
                    MobileTypes.FleshAtronach,
                    MobileTypes.IceAtronach,
                    MobileTypes.Mage,
                    MobileTypes.IronAtronach,
                    MobileTypes.Sorcerer,
                    MobileTypes.Gargoyle,
                    MobileTypes.Battlemage,
                    MobileTypes.Lich,
                    MobileTypes.AncientLich,
                },
            },

            // Harpy Nest - Index10
            new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.HarpyNest,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Rat,
                    MobileTypes.Bat,
                    MobileTypes.Spriggan,
                    MobileTypes.Spider,
                    MobileTypes.Nymph,
                    MobileTypes.Harpy,
                    MobileTypes.BloodSpider,
                    MobileTypes.Harpy,
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.Harpy,
                    MobileTypes.GiantScorpion,
                    MobileTypes.OrcShaman,
                    MobileTypes.Harpy,
                    MobileTypes.Rogue,
                    MobileTypes.Ranger,
                    MobileTypes.Harpy,
                    MobileTypes.Nightblade,
                    MobileTypes.Ranger,
                    MobileTypes.Rogue,
                    MobileTypes.Ranger,
                },
            },

            // Ruined Castle - Index11
            new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.RuinedCastle,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Rat,
                    MobileTypes.Dog,
                    MobileTypes.Warrior,
                    MobileTypes.Orc,
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.Spider,
                    MobileTypes.OrcSergeant,
                    MobileTypes.Werewolf,
                    MobileTypes.Knight,
                    MobileTypes.Wereboar,
                    MobileTypes.Zombie,
                    MobileTypes.Giant,
                    MobileTypes.Knight,
                    MobileTypes.Ghost,
                    MobileTypes.Wraith,
                    MobileTypes.Warrior,
                    MobileTypes.Knight,
                    MobileTypes.Lich,
                    MobileTypes.AncientLich,
                    MobileTypes.VampireAncient,
                },
            },

            // Spider Nest - Index12
            new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.SpiderNest,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Rat,
                    MobileTypes.Goblin,
                    MobileTypes.Spriggan,
                    MobileTypes.Lizardman,
                    MobileTypes.Spider,
                    MobileTypes.Nymph,
                    MobileTypes.BloodSpider,
                    MobileTypes.LizardWarrior,
                    MobileTypes.Harpy,
                    MobileTypes.Assassin,
                    MobileTypes.BloodSpider,
                    MobileTypes.GiantScorpion,
                    MobileTypes.Ghost,
                    MobileTypes.Mummy,
                    MobileTypes.Assassin,
                    MobileTypes.Wraith,
                    MobileTypes.Medusa,
                    MobileTypes.DaedraSeducer,
                    MobileTypes.GloomWraith,
                    MobileTypes.Assassin,
                },
            },

            // Giant Stronghold - Index13
            new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.GiantStronghold,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.GiantBat,
                    MobileTypes.GrizzlyBear,
                    MobileTypes.SabertoothTiger,
                    MobileTypes.Orc,
                    MobileTypes.Giant,
                    MobileTypes.Grotesque,
                    MobileTypes.Boar,
                    MobileTypes.Giant,
                    MobileTypes.StoneGolem,
                    MobileTypes.Giant,
                    MobileTypes.Troll,
                    MobileTypes.GiantScorpion,
                    MobileTypes.Ogre,
                    MobileTypes.Giant,
                    MobileTypes.Minotaur,
                    MobileTypes.FireDaedra,
                    MobileTypes.Daedroth,
                    MobileTypes.Giant,
                    MobileTypes.Dragonling,
                    MobileTypes.FrostDaedra,
                },
            },

            // Dragon's Den - Index14
            new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.DragonsDen,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Bat,
                    MobileTypes.Goblin,
                    MobileTypes.GiantBat,
                    MobileTypes.Spider,
                    MobileTypes.Burglar,
                    MobileTypes.Centaur,
                    MobileTypes.Grotesque,
                    MobileTypes.Harpy,
                    MobileTypes.Knight,
                    MobileTypes.Sorcerer,
                    MobileTypes.Nymph,
                    MobileTypes.FireAtronach,
                    MobileTypes.Mage,
                    MobileTypes.Gargoyle,
                    MobileTypes.Dragonling,
                    MobileTypes.Ranger,
                    MobileTypes.FireDaedra,
                    MobileTypes.Knight,
                    MobileTypes.Dragonling,
                    MobileTypes.DaedraLord,
                },
            },

            // Barbarian Stronghold - Index15
            new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.BarbarianStronghold,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Rat,
                    MobileTypes.Barbarian,
                    MobileTypes.Thief,
                    MobileTypes.Rogue,
                    MobileTypes.GrizzlyBear,
                    MobileTypes.Centaur,
                    MobileTypes.Boar,
                    MobileTypes.Druid,
                    MobileTypes.Barbarian,
                    MobileTypes.Werewolf,
                    MobileTypes.Rogue,
                    MobileTypes.Wereboar,
                    MobileTypes.OrcShaman,
                    MobileTypes.Warrior,
                    MobileTypes.Minotaur,
                    MobileTypes.Druid,
                    MobileTypes.Rogue,
                    MobileTypes.Barbarian,
                    MobileTypes.Nightblade,
                    MobileTypes.Assassin,
                },
            },

            // Volcanic Caves - Index16
            new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.VolcanicCaves,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Goblin,
                    MobileTypes.Imp,
                    MobileTypes.SabertoothTiger,
                    MobileTypes.Lizardman,
                    MobileTypes.Orc,
                    MobileTypes.Scamp,
                    MobileTypes.OrcSergeant,
                    MobileTypes.LizardWarrior,
                    MobileTypes.StoneGolem,
                    MobileTypes.Giant,
                    MobileTypes.IronGolem,
                    MobileTypes.GiantScorpion,
                    MobileTypes.FireAtronach,
                    MobileTypes.OrcShaman,
                    MobileTypes.IronAtronach,
                    MobileTypes.DaedraSeducer,
                    MobileTypes.FireDaedra,
                    MobileTypes.Daedroth,
                    MobileTypes.DaedraLord,
                    MobileTypes.FireDaemon,
                },
            },

            // Scorpion Nest - Index17
            new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.ScorpionNest,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Rat,
                    MobileTypes.GiantBat,
                    MobileTypes.SkeletalSoldier,
                    MobileTypes.Lizardman,
                    MobileTypes.Ghoul,
                    MobileTypes.Grotesque,
                    MobileTypes.Nymph,
                    MobileTypes.LizardWarrior,
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.Zombie,
                    MobileTypes.GiantScorpion,
                    MobileTypes.Gargoyle,
                    MobileTypes.DireGhoul,
                    MobileTypes.GiantScorpion,
                    MobileTypes.Assassin,
                    MobileTypes.Medusa,
                    MobileTypes.DaedraSeducer,
                    MobileTypes.GiantScorpion,
                    MobileTypes.Daedroth,
                    MobileTypes.DaedraLord,
                },
            },

            // Cemetery - Index18
            new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Rat,
                    MobileTypes.Rat,
                    MobileTypes.GiantBat,
                    MobileTypes.Rat,
                    MobileTypes.Thief,
                    MobileTypes.GiantBat,
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.GiantBat,
                    MobileTypes.Spider,
                    MobileTypes.Archer,
                    MobileTypes.Spider,
                    MobileTypes.Imp,
                    MobileTypes.Spider,
                    MobileTypes.Imp,
                    MobileTypes.Zombie,
                    MobileTypes.Mummy,
                    MobileTypes.Bard,
                    MobileTypes.Rogue,
                    MobileTypes.Rat,
                    MobileTypes.Rat,
                },
            },

            /*
            // Cemetery - DF Unity version
            new RandomEncounterTable()
            {
                DungeonType = DFRegion.DungeonTypes.Cemetery,
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Rat,
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.GiantBat,
                    MobileTypes.Mummy,
                    MobileTypes.Spider,
                    MobileTypes.Zombie,
                    MobileTypes.Ghost,
                    MobileTypes.Wraith,
                    MobileTypes.Vampire,
                    MobileTypes.VampireAncient,
                    MobileTypes.Lich,
                },
            },*/

            // Underwater - Index19
            new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Mudcrab,
                    MobileTypes.Mudcrab,
                    MobileTypes.SkeletalSoldier,
                    MobileTypes.Slaughterfish,
                    MobileTypes.Mudcrab,
                    MobileTypes.Ghoul,
                    MobileTypes.Slaughterfish,
                    MobileTypes.Dreugh,
                    MobileTypes.Mudcrab,
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.Dreugh,
                    MobileTypes.Slaughterfish,
                    MobileTypes.Lamia,
                    MobileTypes.Dreugh,
                    MobileTypes.Wraith,
                    MobileTypes.Slaughterfish,
                    MobileTypes.IceAtronach,
                    MobileTypes.Lamia,
                    MobileTypes.Dreugh,
                    MobileTypes.GloomWraith,
                },
            },

            // Desert, in location, night - Index20
            new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Rat,
                    MobileTypes.Dog,
                    MobileTypes.Burglar,
                    MobileTypes.Dog,
                    MobileTypes.Thief,
                    MobileTypes.Dog,
                    MobileTypes.Rogue,
                    MobileTypes.Burglar,
                    MobileTypes.Nightblade,
                    MobileTypes.GiantScorpion,
                    MobileTypes.Assassin,
                    MobileTypes.Rogue,
                    MobileTypes.Burglar,
                    MobileTypes.GiantScorpion,
                    MobileTypes.Barbarian,
                    MobileTypes.Thief,
                    MobileTypes.Burglar,
                    MobileTypes.Nightblade,
                    MobileTypes.Vampire,
                    MobileTypes.Assassin,
                },
            },

            // Desert, not in location, day - Index21
            new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Rat,
                    MobileTypes.Goblin,
                    MobileTypes.Lizardman,
                    MobileTypes.SabertoothTiger,
                    MobileTypes.Orc,
                    MobileTypes.RogueRider,
                    MobileTypes.Harpy,
                    MobileTypes.ThiefRider,
                    MobileTypes.Giant,
                    MobileTypes.GiantScorpion,
                    MobileTypes.RogueRider,
                    MobileTypes.OrcShaman,
                    MobileTypes.ThiefRider,
                    MobileTypes.GiantScorpion,
                    MobileTypes.Minotaur,
                    MobileTypes.OrcWarlord,
                    MobileTypes.HellHound,
                    MobileTypes.RogueRider,
                    MobileTypes.OrcWarlord,
                    MobileTypes.ThiefRider,
                },
            },

            // Desert, not in location, night - Index22
            new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Bat,
                    MobileTypes.GiantBat,
                    MobileTypes.SabertoothTiger,
                    MobileTypes.Spider,
                    MobileTypes.Orc,
                    MobileTypes.Nightblade,
                    MobileTypes.Assassin,
                    MobileTypes.Werewolf,
                    MobileTypes.BloodSpider,
                    MobileTypes.Wereboar,
                    MobileTypes.Mummy,
                    MobileTypes.GiantScorpion,
                    MobileTypes.FireAtronach,
                    MobileTypes.OrcWarlord,
                    MobileTypes.IronAtronach,
                    MobileTypes.Barbarian,
                    MobileTypes.Nightblade,
                    MobileTypes.Vampire,
                    MobileTypes.Nightblade,
                    MobileTypes.Dragonling,
                },
            },

            // Mountain, in location, night - Index23
            new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Rat,
                    MobileTypes.Thief,
                    MobileTypes.Dog,
                    MobileTypes.Burglar,
                    MobileTypes.GrizzlyBear,
                    MobileTypes.Wolf,
                    MobileTypes.FadedGhost,
                    MobileTypes.Thief,
                    MobileTypes.Werewolf,
                    MobileTypes.SnowWolf,
                    MobileTypes.Burglar,
                    MobileTypes.Rogue,
                    MobileTypes.Nightblade,
                    MobileTypes.Barbarian,
                    MobileTypes.Assassin,
                    MobileTypes.Thief,
                    MobileTypes.Burglar,
                    MobileTypes.Rogue,
                    MobileTypes.Nightblade,
                    MobileTypes.Assassin,
                },
            },

            // Mountain, not in location, day - Index24
            new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.GrizzlyBear,
                    MobileTypes.Spriggan,
                    MobileTypes.RogueRider,
                    MobileTypes.Orc,
                    MobileTypes.Nymph,
                    MobileTypes.Harpy,
                    MobileTypes.OrcSergeant,
                    MobileTypes.MountainLion,
                    MobileTypes.Wisp,
                    MobileTypes.LandDreugh,
                    MobileTypes.Giant,
                    MobileTypes.OrcShaman,
                    MobileTypes.ThiefRider,
                    MobileTypes.IronAtronach,
                    MobileTypes.RogueRider,
                    MobileTypes.OrcWarlord,
                    MobileTypes.Druid,
                    MobileTypes.RogueDruid,
                    MobileTypes.ThiefRider,
                    MobileTypes.RogueRider,
                },
            },

            // Mountain, not in location, night - Index25
            new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Bat,
                    MobileTypes.GrizzlyBear,
                    MobileTypes.GiantBat,
                    MobileTypes.Centaur,
                    MobileTypes.Wolf,
                    MobileTypes.Assassin,
                    MobileTypes.Ranger,
                    MobileTypes.Werewolf,
                    MobileTypes.OrcSergeant,
                    MobileTypes.Giant,
                    MobileTypes.Ghost,
                    MobileTypes.OrcShaman,
                    MobileTypes.Sorcerer,
                    MobileTypes.Gargoyle,
                    MobileTypes.Battlemage,
                    MobileTypes.Wraith,
                    MobileTypes.Spellsword,
                    MobileTypes.Dragonling,
                    MobileTypes.Ranger,
                    MobileTypes.Knight,
                },
            },

            // Rainforest, in location, night - Index26
            new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Rat,
                    MobileTypes.Thief,
                    MobileTypes.Dog,
                    MobileTypes.GiantBat,
                    MobileTypes.Nightblade,
                    MobileTypes.GiantBat,
                    MobileTypes.Harpy,
                    MobileTypes.Rogue,
                    MobileTypes.Bard,
                    MobileTypes.Barbarian,
                    MobileTypes.Wereboar,
                    MobileTypes.Thief,
                    MobileTypes.Warrior,
                    MobileTypes.Archer,
                    MobileTypes.Assassin,
                    MobileTypes.Battlemage,
                    MobileTypes.Nightblade,
                    MobileTypes.Vampire,
                    MobileTypes.Warrior,
                    MobileTypes.Knight,
                },
            },

            // Rainforest, not in location, day - Index27
            new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Rat,
                    MobileTypes.GrizzlyBear,
                    MobileTypes.SabertoothTiger,
                    MobileTypes.Spider,
                    MobileTypes.Thief,
                    MobileTypes.Harpy,
                    MobileTypes.Rogue,
                    MobileTypes.Wisp,
                    MobileTypes.Ranger,
                    MobileTypes.Giant,
                    MobileTypes.Warrior,
                    MobileTypes.Knight,
                    MobileTypes.Thief,
                    MobileTypes.Barbarian,
                    MobileTypes.Ranger,
                    MobileTypes.ArcherRider,
                    MobileTypes.Acrobat,
                    MobileTypes.Bard,
                    MobileTypes.Mage,
                    MobileTypes.SpellswordRider,
                },
            },

            // Rainforest, not in location, night - Index28
            new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Bat,
                    MobileTypes.GiantBat,
                    MobileTypes.SabertoothTiger,
                    MobileTypes.Spider,
                    MobileTypes.Spellsword,
                    MobileTypes.Rogue,
                    MobileTypes.Wolf,
                    MobileTypes.Harpy,
                    MobileTypes.Werewolf,
                    MobileTypes.Giant,
                    MobileTypes.Ghost,
                    MobileTypes.Wereboar,
                    MobileTypes.Ogre,
                    MobileTypes.HellHound,
                    MobileTypes.Wraith,
                    MobileTypes.Assassin,
                    MobileTypes.Daedroth,
                    MobileTypes.Nightblade,
                    MobileTypes.Warrior,
                    MobileTypes.Battlemage,
                },
            },

            // Subtropical, in location, night - Index29
            // TODO: I think this table isn't balanced as it should be.
            new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Rat,
                    MobileTypes.Burglar,
                    MobileTypes.Thief,
                    MobileTypes.Monk,
                    MobileTypes.Sorcerer,
                    MobileTypes.Bard,
                    MobileTypes.Rogue,
                    MobileTypes.Warrior,
                    MobileTypes.Nightblade,
                    MobileTypes.Thief,
                    MobileTypes.Acrobat,
                    MobileTypes.Ranger,
                    MobileTypes.Archer,
                    MobileTypes.Vampire,
                    MobileTypes.Barbarian,
                    MobileTypes.Warrior,
                    MobileTypes.Mage,
                    MobileTypes.Spellsword,
                    MobileTypes.Assassin,
                    MobileTypes.Knight,
                },
            },

            // Subtropical, not in location, day - Index30
            new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Rat,
                    MobileTypes.Imp,
                    MobileTypes.GrizzlyBear,
                    MobileTypes.SabertoothTiger,
                    MobileTypes.Centaur,
                    MobileTypes.Nymph,
                    MobileTypes.BloodSpider,
                    MobileTypes.Giant,
                    MobileTypes.Thief,
                    MobileTypes.GiantScorpion,
                    MobileTypes.Warrior,
                    MobileTypes.Sorcerer,
                    MobileTypes.Nightblade,
                    MobileTypes.Rogue,
                    MobileTypes.Knight,
                    MobileTypes.Battlemage,
                    MobileTypes.Dragonling,
                    MobileTypes.Thief,
                    MobileTypes.Barbarian,
                    MobileTypes.Nightblade,
                },
            },

            // Subtropical, not in location, night - Index31
            new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.GiantBat,
                    MobileTypes.GrizzlyBear,
                    MobileTypes.SabertoothTiger,
                    MobileTypes.Spider,
                    MobileTypes.Warrior,
                    MobileTypes.Nightblade,
                    MobileTypes.Harpy,
                    MobileTypes.Werewolf,
                    MobileTypes.Zombie,
                    MobileTypes.Wereboar,
                    MobileTypes.Ghost,
                    MobileTypes.GiantScorpion,
                    MobileTypes.Barbarian,
                    MobileTypes.Assassin,
                    MobileTypes.Battlemage,
                    MobileTypes.Bard,
                    MobileTypes.Nightblade,
                    MobileTypes.Warrior,
                    MobileTypes.Lich,
                    MobileTypes.Assassin,
                },
            },

            // Swamp/woodlands, in location, night - Index32
            new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Rat,
                    MobileTypes.GiantBat,
                    MobileTypes.Burglar,
                    MobileTypes.Bard,
                    MobileTypes.Rogue,
                    MobileTypes.Archer,
                    MobileTypes.Warrior,
                    MobileTypes.Battlemage,
                    MobileTypes.Werewolf,
                    MobileTypes.Acrobat,
                    MobileTypes.Wereboar,
                    MobileTypes.Burglar,
                    MobileTypes.Monk,
                    MobileTypes.Rogue,
                    MobileTypes.Nightblade,
                    MobileTypes.Assassin,
                    MobileTypes.Thief,
                    MobileTypes.Vampire,
                    MobileTypes.Knight,
                    MobileTypes.Battlemage,
                },
            },

            // Swamp/woodlands, not in location, day - Index33
            new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Rat,
                    MobileTypes.GrizzlyBear,
                    MobileTypes.Spriggan,
                    MobileTypes.Orc,
                    MobileTypes.Centaur,
                    MobileTypes.Nymph,
                    MobileTypes.OrcSergeant,
                    MobileTypes.Wisp,
                    MobileTypes.Giant,
                    MobileTypes.Wereboar,
                    MobileTypes.OrcShaman,
                    MobileTypes.Bard,
                    MobileTypes.Barbarian,
                    MobileTypes.OrcWarlord,
                    MobileTypes.Battlemage,
                    MobileTypes.Assassin,
                    MobileTypes.Ranger,
                    MobileTypes.Rogue,
                    MobileTypes.Dragonling,
                    MobileTypes.RogueRider,
                },
            },

            // Swamp/woodlands, not in location, night - Index34
            new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.GiantBat,
                    MobileTypes.GrizzlyBear,
                    MobileTypes.Spider,
                    MobileTypes.Zombie,
                    MobileTypes.Wolf,
                    MobileTypes.Nightblade,
                    MobileTypes.Knight,
                    MobileTypes.Wisp,
                    MobileTypes.Werewolf,
                    MobileTypes.Giant,
                    MobileTypes.Wereboar,
                    MobileTypes.Gargoyle,
                    MobileTypes.Spellsword,
                    MobileTypes.Monk,
                    MobileTypes.Bard,
                    MobileTypes.Rogue,
                    MobileTypes.Ranger,
                    MobileTypes.Nightblade,
                    MobileTypes.Dragonling,
                    MobileTypes.Sorcerer,
                },
            },

            // Haunted woodlands, in location, night - Index35
            new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.GiantBat,
                    MobileTypes.Nightblade,
                    MobileTypes.Burglar,
                    MobileTypes.SkeletalSoldier,
                    MobileTypes.Rogue,
                    MobileTypes.Monk,
                    MobileTypes.FadedGhost,
                    MobileTypes.Barbarian,
                    MobileTypes.Ghost,
                    MobileTypes.Sorcerer,
                    MobileTypes.Warrior,
                    MobileTypes.Assassin,
                    MobileTypes.Wraith,
                    MobileTypes.Knight,
                    MobileTypes.Battlemage,
                    MobileTypes.Thief,
                    MobileTypes.Rogue,
                    MobileTypes.Vampire,
                    MobileTypes.GloomWraith,
                    MobileTypes.VampireAncient,
                },
            },

            // Haunted woodlands, not in location, day - Index36
            new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Imp,
                    MobileTypes.GrizzlyBear,
                    MobileTypes.Spriggan,
                    MobileTypes.Spider,
                    MobileTypes.Centaur,
                    MobileTypes.Nymph,
                    MobileTypes.Wisp,
                    MobileTypes.Harpy,
                    MobileTypes.Giant,
                    MobileTypes.Sorcerer,
                    MobileTypes.Homunculus,
                    MobileTypes.Gargoyle,
                    MobileTypes.Knight,
                    MobileTypes.Ogre,
                    MobileTypes.IronAtronach,
                    MobileTypes.Battlemage,
                    MobileTypes.Assassin,
                    MobileTypes.ArcherRider,
                    MobileTypes.Dragonling,
                    MobileTypes.Knight,
                },
            },

            // Haunted woodlands, not in location, night - Index37
            new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.GiantBat,
                    MobileTypes.GrizzlyBear,
                    MobileTypes.Scamp,
                    MobileTypes.SkeletalSoldier,
                    MobileTypes.FadedGhost,
                    MobileTypes.Werewolf,
                    MobileTypes.SkeletalWarrior,
                    MobileTypes.Zombie,
                    MobileTypes.Giant,
                    MobileTypes.FleshAtronach,
                    MobileTypes.Ghost,
                    MobileTypes.FireAtronach,
                    MobileTypes.IceAtronach,
                    MobileTypes.IronAtronach,
                    MobileTypes.Wraith,
                    MobileTypes.FrostDaedra,
                    MobileTypes.Daedroth,
                    MobileTypes.Vampire,
                    MobileTypes.VampireAncient,
                    MobileTypes.Lich,
                },
            },

            // Unused - Index38
            new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Rat,
                    MobileTypes.Rat,
                    MobileTypes.Thief,
                    MobileTypes.Rat,
                    MobileTypes.Rat,
                    MobileTypes.GiantBat,
                    MobileTypes.GiantBat,
                    MobileTypes.Zombie,
                    MobileTypes.Ghost,
                    MobileTypes.Rat,
                    MobileTypes.Assassin,
                    MobileTypes.GiantBat,
                    MobileTypes.Rogue,
                    MobileTypes.Rat,
                    MobileTypes.GiantBat,
                    MobileTypes.Rat,
                    MobileTypes.GiantBat,
                    MobileTypes.Rat,
                    MobileTypes.Rat,
                    MobileTypes.Vampire,
                },
            },

            // Default building - Index39
            new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Thief,
                    MobileTypes.Warrior,
                    MobileTypes.Burglar,
                    MobileTypes.Warrior,
                    MobileTypes.Burglar,
                    MobileTypes.Bard,
                    MobileTypes.Warrior,
                    MobileTypes.Acrobat,
                    MobileTypes.Burglar,
                    MobileTypes.Rogue,
                    MobileTypes.Thief,
                    MobileTypes.Warrior,
                    MobileTypes.Burglar,
                    MobileTypes.Nightblade,
                    MobileTypes.Rogue,
                    MobileTypes.Warrior,
                    MobileTypes.Burglar,
                    MobileTypes.Thief,
                    MobileTypes.Warrior,
                    MobileTypes.Rogue,
                    MobileTypes.Spy
                },
            },

            // Guildhall - Index40
            new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Mage,
                    MobileTypes.Imp,
                    MobileTypes.Battlemage,
                    MobileTypes.Healer,
                    MobileTypes.Nightblade,
                    MobileTypes.Spellsword,
                    MobileTypes.Mage,
                    MobileTypes.Sorcerer,
                    MobileTypes.Battlemage,
                    MobileTypes.Healer,
                    MobileTypes.FireAtronach,
                    MobileTypes.Spellsword,
                    MobileTypes.Mage,
                    MobileTypes.Sorcerer,
                    MobileTypes.Battlemage,
                    MobileTypes.Nightblade,
                    MobileTypes.Spellsword,
                    MobileTypes.Mage,
                    MobileTypes.DaedraSeducer,
                    MobileTypes.Battlemage,
                    MobileTypes.Spy
                },
            },

            // Temple - Index41
            new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Monk,
                    MobileTypes.Monk,
                    MobileTypes.Knight,
                    MobileTypes.Monk,
                    MobileTypes.Monk,
                    MobileTypes.Monk,
                    MobileTypes.Monk,
                    MobileTypes.Knight,
                    MobileTypes.Monk,
                    MobileTypes.Monk,
                    MobileTypes.Monk,
                    MobileTypes.Monk,
                    MobileTypes.Monk,
                    MobileTypes.Knight,
                    MobileTypes.Monk,
                    MobileTypes.Monk,
                    MobileTypes.Monk,
                    MobileTypes.Monk,
                    MobileTypes.Monk,
                    MobileTypes.Knight,
                },
            },

            // Palace, House1 - Index42
            new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Warrior,
                    MobileTypes.Archer,
                    MobileTypes.Bard,
                    MobileTypes.Spellsword,
                    MobileTypes.Knight,
                    MobileTypes.Warrior,
                    MobileTypes.Knight,
                    MobileTypes.Warrior,
                    MobileTypes.Archer,
                    MobileTypes.Bard,
                    MobileTypes.Spellsword,
                    MobileTypes.Knight,
                    MobileTypes.Warrior,
                    MobileTypes.Knight,
                    MobileTypes.Warrior,
                    MobileTypes.Archer,
                    MobileTypes.Spellsword,
                    MobileTypes.Knight,
                    MobileTypes.Warrior,
                    MobileTypes.Knight,
                    MobileTypes.Spy
                },
            },

            // House2 - Index43
            new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Bard,
                    MobileTypes.Warrior,
                    MobileTypes.Rogue,
                    MobileTypes.Thief,
                    MobileTypes.Warrior,
                    MobileTypes.Spellsword,
                    MobileTypes.Burglar,
                    MobileTypes.Rogue,
                    MobileTypes.Monk,
                    MobileTypes.Mage,
                    MobileTypes.Nightblade,
                    MobileTypes.Acrobat,
                    MobileTypes.Warrior,
                    MobileTypes.Bard,
                    MobileTypes.Healer,
                    MobileTypes.Sorcerer,
                    MobileTypes.Thief,
                    MobileTypes.Vampire,
                    MobileTypes.Rogue,
                    MobileTypes.Warrior,
                },
            },

            // House3 - Index44
            new RandomEncounterTable()
            {
                Enemies = new MobileTypes[]
                {
                    MobileTypes.Rat,
                    MobileTypes.Rat,
                    MobileTypes.Burglar,
                    MobileTypes.Thief,
                    MobileTypes.Bard,
                    MobileTypes.Monk,
                    MobileTypes.Bard,
                    MobileTypes.Healer,
                    MobileTypes.Rogue,
                    MobileTypes.Monk,
                    MobileTypes.Burglar,
                    MobileTypes.Thief,
                    MobileTypes.Bard,
                    MobileTypes.Rogue,
                    MobileTypes.Ranger,
                    MobileTypes.Burglar,
                    MobileTypes.Thief,
                    MobileTypes.Bard,
                    MobileTypes.Monk,
                    MobileTypes.Rogue,
                },
            },
        };
        #endregion

        #region Public methods

        // Enemy selection method from classic. Returns an enemy ID based on environment and player level.
        public static MobileTypes ChooseRandomEnemy(bool chooseUnderWaterEnemy)
        {
            int encounterTableIndex = 0;
            Game.PlayerEnterExit playerEnterExit = Game.GameManager.Instance.PlayerEnterExit;
            PlayerGPS playerGPS = Game.GameManager.Instance.PlayerGPS;

            if (!playerEnterExit || !playerGPS)
                return MobileTypes.None;

            if (chooseUnderWaterEnemy)
                encounterTableIndex = 19;
            else if (playerEnterExit.IsPlayerInsideDungeon)
                encounterTableIndex = ((int)playerEnterExit.Dungeon.Summary.DungeonType);
            else if (playerEnterExit.IsPlayerInsideBuilding)
            {
                DFLocation.BuildingTypes buildingType = playerEnterExit.BuildingType;

                if (buildingType == DFLocation.BuildingTypes.GuildHall)
                    encounterTableIndex = 40;
                else if (buildingType == DFLocation.BuildingTypes.Temple)
                    encounterTableIndex = 41;
                else if (buildingType == DFLocation.BuildingTypes.Palace
                    || buildingType == DFLocation.BuildingTypes.House1)
                    encounterTableIndex = 42;
                else if (buildingType == DFLocation.BuildingTypes.House2)
                    encounterTableIndex = 43;
                else if (buildingType == DFLocation.BuildingTypes.House3)
                    encounterTableIndex = 44;
                else
                    encounterTableIndex = 39;
            }
            else
            {
                int climate = playerGPS.CurrentClimateIndex;
                bool isDay = DaggerfallUnity.Instance.WorldTime.Now.IsDay;

                if (playerGPS.IsPlayerInLocationRect)
                {
                    bool isActiveTime = DaggerfallUnity.Instance.WorldTime.Now.SettlementIsActive;
                    if (isActiveTime)
                        return MobileTypes.None;

                    // Player in location rectangle, night
                    switch (climate)
                    {
                        case (int)MapsFile.Climates.Desert:
                        case (int)MapsFile.Climates.Desert2:
                            encounterTableIndex = 20;
                            break;
                        case (int)MapsFile.Climates.Mountain:
                            encounterTableIndex = 23;
                            break;
                        case (int)MapsFile.Climates.Rainforest:
                            encounterTableIndex = 26;
                            break;
                        case (int)MapsFile.Climates.Subtropical:
                            encounterTableIndex = 29;
                            break;
                        case (int)MapsFile.Climates.Swamp:
                        case (int)MapsFile.Climates.MountainWoods:
                        case (int)MapsFile.Climates.Woodlands:
                        case (int)MapsFile.Climates.Maquis:
                            encounterTableIndex = 32;
                            break;
                        case (int)MapsFile.Climates.HauntedWoodlands:
                            encounterTableIndex = 35;
                            break;

                        default:
                            return MobileTypes.None;
                    }
                }
                else
                {
                    if (isDay)
                    {
                        // Player not in location rectangle, day
                        switch (climate)
                        {
                            case (int)MapsFile.Climates.Desert:
                            case (int)MapsFile.Climates.Desert2:
                                encounterTableIndex = 21;
                                break;
                            case (int)MapsFile.Climates.Mountain:
                                encounterTableIndex = 24;
                                break;
                            case (int)MapsFile.Climates.Rainforest:
                                encounterTableIndex = 27;
                                break;
                            case (int)MapsFile.Climates.Subtropical:
                                encounterTableIndex = 30;
                                break;
                            case (int)MapsFile.Climates.Swamp:
                            case (int)MapsFile.Climates.MountainWoods:
                            case (int)MapsFile.Climates.Woodlands:
                            case (int)MapsFile.Climates.Maquis:
                                encounterTableIndex = 33;
                                break;
                            case (int)MapsFile.Climates.HauntedWoodlands:
                                encounterTableIndex = 36;
                                break;

                            default:
                                return MobileTypes.None;
                        }
                    }
                    else
                    {
                        // Player not in location rectangle, night
                        switch (climate)
                        {
                            case (int)MapsFile.Climates.Desert:
                            case (int)MapsFile.Climates.Desert2:
                                encounterTableIndex = 22;
                                break;
                            case (int)MapsFile.Climates.Mountain:
                                encounterTableIndex = 25;
                                break;
                            case (int)MapsFile.Climates.Rainforest:
                                encounterTableIndex = 28;
                                break;
                            case (int)MapsFile.Climates.Subtropical:
                                encounterTableIndex = 31;
                                break;
                            case (int)MapsFile.Climates.Swamp:
                            case (int)MapsFile.Climates.MountainWoods:
                            case (int)MapsFile.Climates.Woodlands:
                            case (int)MapsFile.Climates.Maquis:
                                encounterTableIndex = 34;
                                break;
                            case (int)MapsFile.Climates.HauntedWoodlands:
                                encounterTableIndex = 37;
                                break;

                            default:
                                return MobileTypes.None;
                        }
                    }
                }
            }

            int roll = Dice100.Roll();
            int playerLevel = Game.GameManager.Instance.PlayerEntity.Level;
            int min;
            int max;

            // Random/player level based adjustments from classic. These assume enemy lists of length 20.
            if (roll > 80)
            {
                if (roll > 95)
                {
                    if (playerLevel <= 5)
                    {
                        min = 0;
                        max = playerLevel + 2;
                    }
                    else
                    {
                        min = 0;
                        max = 19;
                    }
                }
                else
                {
                    min = 0;
                    max = playerLevel + 1;
                }
            }
            else
            {
                min = playerLevel - 3;
                max = playerLevel + 3;
            }
            if (min < 0)
            {
                min = 0;
                max = 5;
            }
            if (max > 19)
            {
                min = 14;
                max = 19;
            }

            RandomEncounterTable encounterTable = EncounterTables[encounterTableIndex];

            // Adding a check here (not in classic) for lists of shorter length than 20
            if (max + 1 > encounterTable.Enemies.Length)
            {
                max = encounterTable.Enemies.Length - 1;
                if (max >= 5)
                    min = max - 5;
                else
                    min = UnityEngine.Random.Range(0, max);
            }

            return encounterTable.Enemies[UnityEngine.Random.Range(min, max + 1)];
        }
    }
    #endregion
}
