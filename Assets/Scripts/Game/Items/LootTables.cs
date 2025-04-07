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
using DaggerfallWorkshop.Game.Entity;
using System.Collections.Generic;
using DaggerfallWorkshop.Game.Utility;

namespace DaggerfallWorkshop.Game.Items
{
    /// <summary>
    /// The parameters involved in creating a loot pile from a loot table.
    /// </summary>
    public class TabledLootSpawnedEventArgs : System.EventArgs
    {
        /// <summary>
        /// The index of the location.
        /// For Dungeons, this corresponds to DFRegion.DungeonTypes.
        /// For Interiors, this corresponds to DFRegion.LocationTypes.
        /// </summary>
        public int LocationIndex { get; set; }

        /// <summary>
        /// The Key used to spawn the loot pile (ex: "K" for Crypt loot).
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The collection containing all the items of the loot pile. New items can be added.
        /// </summary>
        public ItemCollection Items { get; set; }
    }

    /// <summary>
    /// Built-in loot tables.
    /// Currently just for testing during early implementation.
    /// These approximate the loot tables on page 156 of Daggerfall Chronicles but are
    /// different in several ways (e.g. Chronicles uses "WP" for both "Warm Plant" and "Weapon").
    /// May diverge substantially over time during testing and future implementation.
    /// </summary>
    public static class LootTables
    {
        // When a Loot pile is generated from a loot table
        public static System.EventHandler<TabledLootSpawnedEventArgs> OnLootSpawned;

        /// <summary>
        /// Default loot table chance matrices.
        /// Note: Temporary implementation. Will eventually be moved to an external file and loaded as keyed dict.
        /// Note: Many loot tables are defined with a lower chance for magic items in FALL.EXE's tables than is
        /// shown in Daggerfall Chronicles.
        /// These are shown below in the order "Key", "Chronicles chance", "FALL.EXE chance".
        /// E, 5, 3
        /// F, 2, 1
        /// G, 3, 1
        /// H, 2, 1
        /// I, 10, 2
        /// J, 20, 3
        /// K, 5, 3
        /// L, 2, 1
        /// N, 2, 1
        /// O, 3, 2
        /// P, 3, 2
        /// Q, 10, 3
        /// R, 5, 2
        /// S, 20, 3
        /// T, 3, 1
        /// U, 3, 2
        /// </summary>
        public static LootChanceMatrix[] DefaultLootTables = {
            new LootChanceMatrix() {key = "-",   MinGold = 0,   MaxGold = 0,    P1 = 0, P2 = 0, C1 = 0, C2 = 0, C3 = 0, M1 = 0, AM = 0,  WP = 0,  MI = 0, CL = 0, BK = 0, M2 = 0, RL = 0 }, //None
            new LootChanceMatrix() {key = "A",   MinGold = 0,   MaxGold = 5,    P1 = 0, P2 = 0, C1 = 1, C2 = 1, C3 = 1, M1 = 0, AM = 1,  WP = 5,  MI = 1, CL = 5, BK = 0, M2 = 1, RL = 0 }, //Orcs
            // Chronicles says B has 10 for Warm Plant and Misc. Monster, but in FALL.EXE it is Temperate Plant and Warm Plant.
            new LootChanceMatrix() {key = "B",   MinGold = 0,   MaxGold = 0,    P1 = 5, P2 = 5, C1 = 0, C2 = 0, C3 = 0, M1 = 0, AM = 0,  WP = 0,  MI = 0, CL = 0, BK = 0, M2 = 0, RL = 0 }, //Nature
            new LootChanceMatrix() {key = "C",   MinGold = 0,   MaxGold = 10,   P1 = 5, P2 = 5, C1 = 3, C2 = 3, C3 = 3, M1 = 3, AM = 10, WP = 20, MI = 1, CL = 50,BK = 2, M2 = 2, RL = 1 }, //Rangers
            new LootChanceMatrix() {key = "D",   MinGold = 0,   MaxGold = 0,    P1 = 2, P2 = 2, C1 = 2, C2 = 2, C3 = 2, M1 = 2, AM = 0,  WP = 0,  MI = 0, CL = 0, BK = 0, M2 = 0, RL = 0 }, //Harpy
            new LootChanceMatrix() {key = "E",   MinGold = 0,   MaxGold = 10,   P1 = 2, P2 = 2, C1 = 5, C2 = 5, C3 = 5, M1 = 2, AM = 0,  WP = 5,  MI = 0, CL = 0, BK = 0, M2 = 1, RL = 2 }, //Giant
            new LootChanceMatrix() {key = "F",   MinGold = 2,   MaxGold = 15,   P1 = 2, P2 = 2, C1 = 5, C2 = 5, C3 = 5, M1 = 2, AM = 80, WP = 70, MI = 2, CL = 0, BK = 0, M2 = 3, RL = 10 },//Giant Loot
            new LootChanceMatrix() {key = "G",   MinGold = 0,   MaxGold = 8,    P1 = 0, P2 = 0, C1 = 0, C2 = 0, C3 = 0, M1 = 0, AM = 10, WP = 1,  MI = 1, CL = 10,BK = 0, M2 = 3, RL = 5 }, //Undead naked
            new LootChanceMatrix() {key = "H",   MinGold = 0,   MaxGold = 5,    P1 = 0, P2 = 0, C1 = 0, C2 = 0, C3 = 0, M1 = 0, AM = 5,  WP = 30, MI = 1, CL = 2, BK = 1, M2 = 0, RL = 5 }, //Undead armed
            // Chronicles is missing "I" but lists its data in table "J." All the tables from here are off by one compared to Chronicles.
            new LootChanceMatrix() {key = "I",   MinGold = 0,   MaxGold = 0,    P1 = 0, P2 = 0, C1 = 0, C2 = 0, C3 = 0, M1 = 0, AM = 0,  WP = 0,  MI = 1, CL = 0, BK = 0, M2 = 0, RL = 5 }, //Undead spirits
            new LootChanceMatrix() {key = "J",   MinGold = 10,  MaxGold = 40,   P1 = 3, P2 = 3, C1 = 3, C2 = 3, C3 = 3, M1 = 5, AM = 1,  WP = 1,  MI = 5, CL = 5, BK = 5, M2 = 2, RL = 10 },//Undead bosses
            new LootChanceMatrix() {key = "K",   MinGold = 10,  MaxGold = 20,   P1 = 3, P2 = 3, C1 = 3, C2 = 3, C3 = 3, M1 = 3, AM = 70, WP = 50, MI = 3, CL = 0, BK = 2, M2 = 2, RL = 80 },//Undead Loot
            new LootChanceMatrix() {key = "L",   MinGold = 0,   MaxGold = 10,   P1 = 0, P2 = 0, C1 = 3, C2 = 3, C3 = 3, M1 = 3, AM = 70, WP = 50, MI = 2, CL = 20,BK = 0, M2 = 3, RL = 3 }, //Nest Loot
            new LootChanceMatrix() {key = "M",   MinGold = 0,   MaxGold = 7,    P1 = 1, P2 = 1, C1 = 1, C2 = 1, C3 = 1, M1 = 2, AM = 80, WP = 40, MI = 2, CL = 15,BK = 2, M2 = 3, RL = 1 }, //Cave Loot
            new LootChanceMatrix() {key = "N",   MinGold = 1,   MaxGold = 40,   P1 = 5, P2 = 5, C1 = 5, C2 = 5, C3 = 5, M1 = 5, AM = 90, WP = 60, MI = 2, CL = 20,BK = 5, M2 = 2, RL = 5 }, //Castle Loot
            new LootChanceMatrix() {key = "O",   MinGold = 0,   MaxGold = 30,   P1 = 1, P2 = 1, C1 = 1, C2 = 1, C3 = 1, M1 = 1, AM = 5,  WP = 10, MI = 1, CL = 60,BK = 0, M2 = 0, RL = 0 }, //Rogues
            new LootChanceMatrix() {key = "P",   MinGold = 2,   MaxGold = 10,   P1 = 5, P2 = 2, C1 = 2, C2 = 2, C3 = 2, M1 = 2, AM = 10, WP = 10, MI = 2, CL = 50,BK = 9, M2 = 2, RL = 0 }, //Spellsword
            new LootChanceMatrix() {key = "Q",   MinGold = 10,  MaxGold = 40,   P1 = 2, P2 = 2, C1 = 4, C2 = 4, C3 = 4, M1 = 2, AM = 0,  WP = 0,  MI = 2, CL = 70,BK = 5, M2 = 3, RL = 5 }, //Vampires
            new LootChanceMatrix() {key = "R",   MinGold = 2,   MaxGold = 10,   P1 = 0, P2 = 0, C1 = 3, C2 = 3, C3 = 3, M1 = 5, AM = 0,  WP = 0,  MI = 1, CL = 0, BK = 0, M2 = 2, RL = 0 }, //Water monsters
            new LootChanceMatrix() {key = "S",   MinGold = 30,  MaxGold = 70,   P1 = 5, P2 = 5, C1 = 5, C2 = 5, C3 = 5, M1 = 15,AM = 0,  WP = 0,  MI = 8, CL = 5, BK = 5, M2 = 2, RL = 10 },//Dragon Loot, Daedras
            new LootChanceMatrix() {key = "T",   MinGold = 10,  MaxGold = 50,   P1 = 0, P2 = 0, C1 = 0, C2 = 0, C3 = 0, M1 = 0, AM = 60, WP = 40, MI = 0, CL = 50,BK = 10,M2 = 0, RL = 10}, //Knight Orc Warlord
            new LootChanceMatrix() {key = "U",   MinGold = 0,   MaxGold = 40,   P1 = 5, P2 = 5, C1 = 5, C2 = 5, C3 = 5, M1 = 10,AM = 20, WP = 20, MI = 4, CL = 20,BK = 90,M2 = 5, RL = 70 },//Laboratory Loot
            new LootChanceMatrix() {key = "LR1", MinGold = 0,   MaxGold = 20,   P1 = 0, P2 = 0, C1 = 0, C2 = 0, C3 = 0, M1 = 0, AM = 40, WP = 50, MI = 1, CL = 50,BK = 0, M2 = 0, RL = 5 }, //Warrior, Barbarian
            new LootChanceMatrix() {key = "LR2", MinGold = 0,   MaxGold = 5,    P1 = 3, P2 = 3, C1 = 1, C2 = 1, C3 = 3, M1 = 3, AM = 0,  WP = 20, MI = 1, CL = 60,BK = 10,M2 = 3, RL = 0 }, //Healer, Orc Shaman
            new LootChanceMatrix() {key = "LR3", MinGold = 0,   MaxGold = 30,   P1 = 3, P2 = 3, C1 = 1, C2 = 1, C3 = 1, M1 = 2, AM = 0,  WP = 20, MI = 1, CL = 95,BK = 45,M2 = 2, RL = 10 },//Spellcasters
        };

        /// <summary>
        /// Gets loot matrix by key.
        /// Note: Temporary implementation. Will eventually be moved to an external file and loaded as keyed dict.
        /// </summary>
        /// <param name="key">Key of matrix to get.</param>
        /// <returns>LootChanceMatrix.</returns>
        public static LootChanceMatrix GetMatrix(string key)
        {
            for (int i = 0; i < DefaultLootTables.Length; i++)
            {
                if (DefaultLootTables[i].key == key)
                    return DefaultLootTables[i];
            }

            return DefaultLootTables[0];
        }

        public static bool GenerateLoot(DaggerfallLoot loot, int locationIndex)
        {
            string[] lootTableKeys = {
            "K", // Crypt
            "N", // Orc Stronghold
            "N", // Human Stronghold
            "N", // Prison
            "K", // Desecrated Temple
            "M", // Mine
            "M", // Natural Cave
            "Q", // Coven
            "K", // Vampire Haunt
            "U", // Laboratory
            "D", // Harpy Nest
            "N", // Ruined Castle
            "L", // Spider Nest
            "F", // Giant Stronghold
            "S", // Dragon's Den
            "N", // Barbarian Stronghold
            "M", // Volcanic Caves
            "L", // Scorpion Nest
            "N", // Cemetery
            };

            // Get loot table key
            if (locationIndex < lootTableKeys.Length)
            {
                DaggerfallLoot.GenerateItems(lootTableKeys[locationIndex], loot.Items);

                // Randomly add map
                char key = lootTableKeys[locationIndex][0];
                int alphabetIndex = key - 64;

                if (alphabetIndex >= 10 && alphabetIndex <= 15) // between keys J and O
                {
                    int[] mapChances = { 2, 1, 1, 2, 2, 15 };
                    int mapChance = mapChances[alphabetIndex - 10];
                    DaggerfallLoot.RandomlyAddMap(mapChance, loot.Items);
                    DaggerfallLoot.RandomlyAddPotion(4, loot.Items);
                    DaggerfallLoot.RandomlyAddPotionRecipe(2, loot.Items);
                }

                OnLootSpawned?.Invoke(null, new TabledLootSpawnedEventArgs { LocationIndex = locationIndex, Key = lootTableKeys[locationIndex], Items = loot.Items });

                return true;
            }
            return false;
        }

        /// <summary>
        /// Generates an array of items based on loot chance matrix.
        /// </summary>
        /// <param name="matrix">Loot chance matrix.</param>
        /// <param name="playerEntity">Player entity.</param>
        /// <param name="levelModifer">Bonus to loot based on area/monster level/etc...
        /// <returns>DaggerfallUnityItem array.</returns>
        public static DaggerfallUnityItem[] GenerateRandomLoot(LootChanceMatrix matrix, PlayerEntity playerEntity, int levelModifier = 1)
        {
            // Notes: The first part of the DF Chronicles explanation of how loot is generated does not match the released game.
            // It says the chance for each item category is the matrix amount * the level of the NPC. Actual behavior in the
            // released game is (matrix amount * PC level) for the first 4 item categories (temperate plants, warm plants,
            // miscellaneous monster, warm monster), and just matrix amount for the categories after that.
            // The second part of the DF Chronicles explanation (roll repeatedly for items from a category, each time at halved
            // chance), matches the game.
            // In classic, since a 0-99 roll is compared to whether it is less or greater than item chance,
            // even a 0% chance category has a 1/100 chance to appear, and the chance values are effectively
            // 1 higher than what the loot tables show.
            // ProjectN: substituting player level with luck modifier
            // ProjectN: added a modifier to initial conditions of random loot (as per R&R:I)
            float chance;
            int luck = playerEntity.Stats.GetLiveStatValue(DaggerfallConnect.DFCareer.Stats.Luck);
            List<DaggerfallUnityItem> items = new List<DaggerfallUnityItem>();
            DaggerfallUnityItem item = null;

            // Reseed random
            Random.InitState(items.GetHashCode());

            // Random gold
            // ProjectN: eliminating player level from calculation
            int goldCount = Random.Range(matrix.MinGold, matrix.MaxGold + levelModifier);
            if (goldCount > 0)
            {
                items.Add(ItemBuilder.CreateGoldPieces(goldCount));
            }

            // Random weapon
            chance = matrix.WP;
            while (Dice100.SuccessRoll((int)chance))
            {
                item = ItemBuilder.CreateRandomWeapon(luck, levelModifier);
                item.currentCondition = (int)(item.maxCondition * UnityEngine.Random.Range(0.2f, 0.75f));
                items.Add(item);
                chance *= 0.5f;
            }

            // Random armor
            chance = matrix.AM;
            while (Dice100.SuccessRoll((int)chance))
            {
                item = ItemBuilder.CreateRandomArmor(luck, playerEntity.Gender, playerEntity.Race, levelModifier);
                item.currentCondition = (int)(item.maxCondition * UnityEngine.Random.Range(0.2f, 0.75f));
                items.Add(item);
                chance *= 0.5f;
            }

            // Random ingredients
            int luckMod = (luck - 50) / 5;
            RandomIngredient(matrix.C1 * luckMod, ItemGroups.CreatureIngredients1, items);
            RandomIngredient(matrix.C2 * luckMod, ItemGroups.CreatureIngredients2, items);
            RandomIngredient(matrix.C3, ItemGroups.CreatureIngredients3, items);
            RandomIngredient(matrix.P1 * luckMod, ItemGroups.PlantIngredients1, items);
            RandomIngredient(matrix.P2 * luckMod, ItemGroups.PlantIngredients2, items);
            RandomIngredient(matrix.M1, ItemGroups.MiscellaneousIngredients1, items);
            RandomIngredient(matrix.M2, ItemGroups.MiscellaneousIngredients2, items);

            // Random magic item
            chance = matrix.MI;
            while (Dice100.SuccessRoll((int)chance))
            {
                item = ItemBuilder.CreateRandomMagicItem(luck, playerEntity.Gender, playerEntity.Race);
                item.currentCondition = (int)(item.maxCondition * UnityEngine.Random.Range(0.2f, 0.75f));
                items.Add(item);
                chance *= 0.5f;
            }

            // Random clothes
            chance = matrix.CL;
            while (Dice100.SuccessRoll((int)chance))
            {
                item = ItemBuilder.CreateRandomClothing(playerEntity.Gender, playerEntity.Race, levelModifier);
                item.currentCondition = (int)(item.maxCondition * UnityEngine.Random.Range(0.2f, 0.75f));
                items.Add(item);
                chance *= 0.5f;
            }

            // Random books
            chance = matrix.BK;
            while (Dice100.SuccessRoll((int)chance))
            {
                item = ItemBuilder.CreateRandomBook();
                item.currentCondition = (int)(item.maxCondition * UnityEngine.Random.Range(0.2f, 0.75f));
                items.Add(item);
                chance *= 0.5f;
            }

            // Random religious item
            chance = matrix.RL;
            while (Dice100.SuccessRoll((int)chance))
            {
                item = ItemBuilder.CreateRandomReligiousItem();
                item.currentCondition = (int)(item.maxCondition * UnityEngine.Random.Range(0.2f, 0.75f));
                items.Add(item);
                chance *= 0.5f;
            }

            return items.ToArray();
        }

        #region Private Methods

        static void RandomIngredient(float chance, ItemGroups ingredientGroup, List<DaggerfallUnityItem> targetItems)
        {
            while (Dice100.SuccessRoll((int)chance))
            {
                targetItems.Add(ItemBuilder.CreateRandomIngredient(ingredientGroup));
                chance *= 0.5f;
            }
        }

        #endregion
    }
}
