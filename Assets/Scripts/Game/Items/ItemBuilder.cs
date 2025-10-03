    // Project:         Daggerfall Unity
// Copyright:       Copyright (C) 2009-2022 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Gavin Clayton (interkarma@dfworkshop.net)
// Contributors: InconsolableCellist
//
// Notes:
//

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallConnect.FallExe;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Utility.AssetInjection;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallConnect;
using DaggerfallConnect.Utility;

namespace DaggerfallWorkshop.Game.Items
{
    /// <summary>
    /// Generates new items for various game systems.
    /// This helper still under development.
    /// </summary>
    public static class ItemBuilder
    {
        #region Data

        public const int firstFemaleArchive = 245;
        public const int firstMaleArchive = 249;
        private const int chooseAtRandom = -1;

        // This array is used to pick random material values.
        // The array is traversed, subtracting each value from a sum until the sum is less than the next value.
        // Steel through Daedric, or Iron if sum is less than the first value.
        public static readonly byte[] materialsByModifier = { 64, 128, 10, 21, 13, 8, 5, 3, 2, 5 };

        // Weight multipliers by material type. Iron through Daedric. Weight is baseWeight * value / 4.
        // ProjectN: modified every value - still to playtest intensively.
        static readonly short[] weightMultipliersByMaterial = { 0, 5, 4, 4, 2, 3, 7, 3, 6, 2, 12, 1 };

        // Value multipliers by material type. Iron through Daedric. Value is baseValue * ( 3 * value).
        static readonly short[] valueMultipliersByMaterial = { 0, 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 20 };

        // Condition multipliers by material type. Iron through Daedric. MaxCondition is baseMaxCondition * value / 4.
        // ProjectN: modifiied every value - still to playtest intensively.
        static readonly short[] conditionMultipliersByMaterial = { 0, 4, 6, 4, 6, 12, 16, 7, 20, 12, 16, 5 };

        // Enchantment point/gold value data for item powers
        static readonly int[] extraSpellPtsEnchantPts = { 0x1F4, 0x1F4, 0x1F4, 0x1F4, 0xC8, 0xC8, 0xC8, 0x2BC, 0x320, 0x384, 0x3E8 };
        static readonly int[] potentVsEnchantPts = { 0x320, 0x384, 0x3E8, 0x4B0 };
        static readonly int[] regensHealthEnchantPts = { 0x0FA0, 0x0BB8, 0x0BB8 };
        static readonly int[] vampiricEffectEnchantPts = { 0x7D0, 0x3E8 };
        static readonly int[] increasedWeightAllowanceEnchantPts = { 0x190, 0x258 };
        static readonly int[] improvesTalentsEnchantPts = { 0x1F4, 0x258, 0x258 };
        static readonly int[] goodRepWithEnchantPts = { 0x3E8, 0x3E8, 0x3E8, 0x3E8, 0x3E8, 0x1388 };
        static readonly int[][] enchantmentPtsForItemPowerArrays = { null, null, null, extraSpellPtsEnchantPts, potentVsEnchantPts, regensHealthEnchantPts,
                                                                    vampiricEffectEnchantPts, increasedWeightAllowanceEnchantPts, null, null, null, null, null,
                                                                    improvesTalentsEnchantPts, goodRepWithEnchantPts};
        static readonly ushort[] enchantmentPointCostsForNonParamTypes = { 0, 0x0F448, 0x0F63C, 0x0FF9C, 0x0FD44, 0, 0, 0, 0x384, 0x5DC, 0x384, 0x64, 0x2BC };

        public static int newArmorArchive = 0;

        public enum BodyMorphology
        {
            Argonian = 0,
            Elf = 1,
            Human = 2,
            Khajiit = 3,
        }

        /// <summary>
        /// Every clothing dye.
        /// </summary>
        public static DyeColors[] clothingDyes = {
            DyeColors.Blue,
            DyeColors.Grey,
            DyeColors.Red,
            DyeColors.DarkBrown,
            DyeColors.Purple,
            DyeColors.LightBrown,
            DyeColors.White,
            DyeColors.Aquamarine,
            DyeColors.Yellow,
            DyeColors.Green,
            DyeColors.Olive,
            DyeColors.Amber,
            DyeColors.DarkGrey
        };

        /// <summary>
        /// Every metal dye.
        /// </summary>
        public static DyeColors[] metalDyes = {
            DyeColors.Iron,
            DyeColors.Steel,
            DyeColors.Silver,
            DyeColors.Elven,
            DyeColors.Glass,
            DyeColors.Dwarven,
            DyeColors.Orcish,
            DyeColors.Mithril,
            DyeColors.Adamantium,
            DyeColors.Ebony,
            DyeColors.Daedric
        };

        /// <summary>
        /// Cheap clothing dye.
        /// </summary>
        public static DyeColors[] cheapClothingDyes = {
            DyeColors.Grey,
            DyeColors.DarkBrown,
            DyeColors.LightBrown,
            DyeColors.White,
            DyeColors.Olive,
        };

        /// <summary>
        /// Fancy clothing dye.
        /// </summary>
        public static DyeColors[] fancyClothingDyes = {
            DyeColors.Blue,
            DyeColors.Red,
            DyeColors.Purple,
            DyeColors.Aquamarine,
            DyeColors.Yellow,
            DyeColors.Green,
            DyeColors.Amber,
            DyeColors.DarkGrey
        };

        public static DyeColors[] workClothingDyes = {
            DyeColors.Grey,
            DyeColors.DarkBrown,
            DyeColors.LightBrown,
            DyeColors.DarkGrey,
            DyeColors.Leather
        };

        #endregion

        #region Public Methods

        public static DyeColors RandomClothingDye()
        {
            return clothingDyes[UnityEngine.Random.Range(0, clothingDyes.Length)];
        }

        public static DyeColors RandomMetalDye()
        {
            return metalDyes[UnityEngine.Random.Range(0, metalDyes.Length)];
        }

        public static DyeColors RandomCheapClothingDye()
        {
            return cheapClothingDyes[UnityEngine.Random.Range(0, cheapClothingDyes.Length)];
        }

        public static DyeColors RandomFancyClothingDye()
        {
            return fancyClothingDyes[UnityEngine.Random.Range(0, fancyClothingDyes.Length)];
        }

        public static DyeColors RandomWorkClothingDye()
        {
            return workClothingDyes[UnityEngine.Random.Range(0, workClothingDyes.Length)];
        }

        /// <summary>
        /// Creates a generic item from group and template index.
        /// </summary>
        /// <param name="itemGroup">Item group.</param>
        /// <param name="templateIndex">Template index.</param>
        /// <returns>DaggerfallUnityItem.</returns>
        public static DaggerfallUnityItem CreateItem(ItemGroups itemGroup, int templateIndex)
        {
            // Handle custom items
            if (templateIndex > ItemHelper.LastDFTemplate)
            {
                // Allow custom item classes to be instantiated when registered
                Type itemClassType;
                if (DaggerfallUnity.Instance.ItemHelper.GetCustomItemClass(templateIndex, out itemClassType))
                    return (DaggerfallUnityItem)Activator.CreateInstance(itemClassType);
                else
                    return new DaggerfallUnityItem(itemGroup, templateIndex);
            }

            // Create classic item
            int groupIndex = DaggerfallUnity.Instance.ItemHelper.GetGroupIndex(itemGroup, templateIndex);
            if (groupIndex == -1)
            {
                Debug.LogErrorFormat("ItemBuilder.CreateItem() encountered an item with an invalid GroupIndex. Check you're passing 'template index' matching a value in ItemEnums - e.g. (int)Weapons.Dagger NOT a 'group index' (e.g. 0).");
                return null;
            }
            DaggerfallUnityItem newItem = new DaggerfallUnityItem(itemGroup, groupIndex);

            return newItem;
        }

        /// <summary>
        /// Super generic item creator: the less is passed, the more random the generation gets.
        /// I added some bool to limit the creation to functional items (e.g.: no furnitures, horses, etc.)
        /// </summary>
        public static DaggerfallUnityItem CreateRandomItem(ItemGroups itemGroup = ItemGroups.None, int minTempIndex = -1, int maxTempIndex = -1, bool filterCrazyStuff = true)
        {
            if (itemGroup == ItemGroups.None)
            {
                do{
                    itemGroup = (ItemGroups)UnityEngine.Random.Range(0, Enum.GetNames(typeof(ItemGroups)).Length);
                }
                while (filterCrazyStuff && (itemGroup == ItemGroups.Artifacts ||
                                            itemGroup == ItemGroups.Currency ||
                                            itemGroup == ItemGroups.Deeds ||
                                            itemGroup == ItemGroups.Furniture ||
                                            itemGroup == ItemGroups.MagicItems ||
                                            itemGroup == ItemGroups.MiscItems ||
                                            itemGroup == ItemGroups.QuestItems ||
                                            itemGroup == ItemGroups.Transportation ||
                                            itemGroup == ItemGroups.UselessItems1));    // This could be removed from here when some use is given to these items
            }

            int[] enumArray = (int[])DaggerfallUnity.Instance.ItemHelper.GetEnumArray(itemGroup);            
            if (minTempIndex < enumArray[0]) minTempIndex = enumArray[0];
            if (maxTempIndex >= enumArray.Length || maxTempIndex == -1) maxTempIndex = enumArray.Length - 1;

            DaggerfallUnityItem newItem = new DaggerfallUnityItem(itemGroup, UnityEngine.Random.Range(minTempIndex, (maxTempIndex + 1)));

            return newItem;
        }

        /// <summary>
        /// Generates men's clothing.
        /// </summary>
        /// <param name="item">Item type to generate.</param>
        /// <param name="race">Race of player.</param>
        /// <param name="variant">Variant to use. If not set, a random variant will be selected.</param>
        /// <param name="dye">Dye to use</param>
        /// <returns>DaggerfallUnityItem.</returns>
        public static DaggerfallUnityItem CreateMensClothing(MensClothing item, Races race, int qualityMod = 0, int variant = -1, DyeColors dye = DyeColors.Blue)
        {
            // Create item
            int groupIndex = DaggerfallUnity.Instance.ItemHelper.GetGroupIndex(ItemGroups.MensClothing, (int)item);
            DaggerfallUnityItem newItem = new DaggerfallUnityItem(ItemGroups.MensClothing, groupIndex);

            // Random variant
            if (variant < 0)
                variant = UnityEngine.Random.Range(0, newItem.ItemTemplate.variants);

            // Set race, variant, dye
            SetRace(newItem, race);
            SetClothQuality(newItem, qualityMod, out ClothCraftsmanship clothCraftsmanship);
            SetVariant(newItem, variant);
            // newItem.dyeColor = dye;

            return newItem;
        }

        /// <summary>
        /// Generates women's clothing.
        /// </summary>
        /// <param name="item">Item type to generate.</param>
        /// <param name="race">Race of player.</param>
        /// <param name="variant">Variant to use. If not set, a random variant will be selected.</param>
        /// <param name="dye">Dye to use</param>
        /// <returns>DaggerfallUnityItem.</returns>
        public static DaggerfallUnityItem CreateWomensClothing(WomensClothing item, Races race, int qualityMod = 0, int variant = -1, DyeColors dye = DyeColors.Blue)
        {
            // Create item
            int groupIndex = DaggerfallUnity.Instance.ItemHelper.GetGroupIndex(ItemGroups.WomensClothing, (int)item);
            DaggerfallUnityItem newItem = new DaggerfallUnityItem(ItemGroups.WomensClothing, groupIndex);

            // Random variant
            if (variant < 0)
                variant = UnityEngine.Random.Range(0, newItem.ItemTemplate.variants);

            // Set race, variant, dye
            SetRace(newItem, race);
            SetClothQuality(newItem, qualityMod, out ClothCraftsmanship clothCraftsmanship);
            SetVariant(newItem, variant);
            // newItem.dyeColor = dye;

            return newItem;
        }

        /// <summary>
        /// Creates a new item of random clothing.
        /// </summary>
        /// <param name="gender">Gender of player</param>
        /// <returns>DaggerfallUnityItem.</returns>
        public static DaggerfallUnityItem CreateRandomClothing(Genders gender, Races race, int qualityMod = 0)
        {
            // Create random clothing by gender, including any custom items registered as clothes
            ItemGroups genderClothingGroup = (gender == Genders.Male) ? ItemGroups.MensClothing : ItemGroups.WomensClothing;

            ItemHelper itemHelper = DaggerfallUnity.Instance.ItemHelper;
            Array enumArray = itemHelper.GetEnumArray(genderClothingGroup);
            int[] customItemTemplates = itemHelper.GetCustomItemsForGroup(genderClothingGroup);

            int groupIndex = UnityEngine.Random.Range(0, enumArray.Length + customItemTemplates.Length);
            DaggerfallUnityItem newItem;
            if (groupIndex < enumArray.Length)
                newItem = new DaggerfallUnityItem(genderClothingGroup, groupIndex);
            else
                newItem = CreateItem(genderClothingGroup, customItemTemplates[groupIndex - enumArray.Length]);

            SetRace(newItem, race);
            SetClothQuality(newItem, qualityMod, out ClothCraftsmanship clothCraftsmanship);

            // In ProjectN, the colour is chosen with the variant
            // SetClothColors(newItem, clothCraftsmanship);
            // newItem.dyeColor = RandomClothingDye();

            // Random variant
            SetVariant(newItem, UnityEngine.Random.Range(0, newItem.TotalVariants));

            return newItem;
        }

        /// <summary>
        /// Creates a new book.
        /// </summary>
        /// <param name="fileName">The name of the books resource.</param>
        /// <returns>An instance of the book item or null.</returns>
        public static DaggerfallUnityItem CreateBook(string fileName)
        {
            if (!Path.HasExtension(fileName))
                fileName += ".TXT";

            var entry = BookReplacement.BookMappingEntries.Values.FirstOrDefault(x => x.Name.Equals(fileName, StringComparison.Ordinal));
            if (entry.ID != 0)
                return CreateBook(entry.ID);

            int id;
            if (fileName.Length == 12 && fileName.StartsWith("BOK") && int.TryParse(fileName.Substring(3, 5), out id))
                return CreateBook(id);

            return null;
        }

        /// <summary>
        /// Creates a new book.
        /// </summary>
        /// <param name="id">The numeric id of book resource.</param>
        /// <returns>An instance of the book item or null.</returns>
        public static DaggerfallUnityItem CreateBook(int id)
        {
            var bookFile = new BookFile();

            string name = GameManager.Instance.ItemHelper.GetBookFileName(id);
            if (!BookReplacement.TryImportBook(name, bookFile) &&
                !bookFile.OpenBook(DaggerfallUnity.Instance.Arena2Path, name))
                return null;

            return new DaggerfallUnityItem(ItemGroups.Books, 0)
            {
                message = id,
                value = bookFile.Price
            };
        }

        /// <summary>
        /// Creates a new random book
        /// </summary>
        /// <returns>DaggerfallUnityItem.</returns>
        public static DaggerfallUnityItem CreateRandomBook()
        {
            Array enumArray = DaggerfallUnity.Instance.ItemHelper.GetEnumArray(ItemGroups.Books);
            DaggerfallUnityItem book = new DaggerfallUnityItem(ItemGroups.Books, Array.IndexOf(enumArray, Books.Book0));
            book.message = DaggerfallUnity.Instance.ItemHelper.GetRandomBookID();
            book.CurrentVariant = UnityEngine.Random.Range(0, book.TotalVariants);
            // Update item value for this book.
            BookFile bookFile = new BookFile();
            string name = GameManager.Instance.ItemHelper.GetBookFileName(book.message);
            if (!BookReplacement.TryImportBook(name, bookFile))
                bookFile.OpenBook(DaggerfallUnity.Instance.Arena2Path, name);
            book.value = bookFile.Price;
            return book;
        }

        /// <summary>
        /// Creates a new random religious item.
        /// </summary>
        /// <returns>DaggerfallUnityItem.</returns>
        public static DaggerfallUnityItem CreateRandomReligiousItem()
        {
            Array enumArray = DaggerfallUnity.Instance.ItemHelper.GetEnumArray(ItemGroups.ReligiousItems);
            int groupIndex = UnityEngine.Random.Range(0, enumArray.Length);
            DaggerfallUnityItem newItem = new DaggerfallUnityItem(ItemGroups.ReligiousItems, groupIndex);

            return newItem;
        }

        public static DaggerfallUnityItem CreateRandomlyFilledSoulTrap()
        {
            // Create a trapped soul type and filter invalid creatures
            MobileTypes soul = MobileTypes.None;
            while (soul == MobileTypes.None)
            {
                MobileTypes randomSoul = (MobileTypes)UnityEngine.Random.Range((int)MobileTypes.Rat, (int)MobileTypes.Lamia + 1);
                if (randomSoul == MobileTypes.Horse_Invalid ||
                    randomSoul == MobileTypes.Dragonling)       // NOTE: Dragonling (34) is soulless, only soul of Dragonling_Alternate (40) from B0B70Y16 has a soul
                    continue;
                else
                    soul = randomSoul;
            }

            // Generate item
            DaggerfallUnityItem newItem = CreateItem(ItemGroups.MiscItems, (int)MiscItems.Soul_trap);
            newItem.TrappedSoulType = soul;
            MobileEnemy mobileEnemy = GameObjectHelper.EnemyDict[(int)soul];
            newItem.value = 5000 + mobileEnemy.SoulPts;

            return newItem;
        }

        /// <summary>
        /// Creates a new random gem.
        /// </summary>
        /// <returns>DaggerfallUnityItem.</returns>
        public static DaggerfallUnityItem CreateRandomGem()
        {
            Array enumArray = DaggerfallUnity.Instance.ItemHelper.GetEnumArray(ItemGroups.Gems);
            int groupIndex = UnityEngine.Random.Range(0, enumArray.Length);
            DaggerfallUnityItem newItem = new DaggerfallUnityItem(ItemGroups.Gems, groupIndex);

            return newItem;
        }

        /// <summary>
        /// Creates a new random jewellery.
        /// </summary>
        /// <returns>DaggerfallUnityItem.</returns>
        public static DaggerfallUnityItem CreateRandomJewellery()
        {
            Array enumArray = DaggerfallUnity.Instance.ItemHelper.GetEnumArray(ItemGroups.Jewellery);
            int groupIndex = UnityEngine.Random.Range(0, enumArray.Length);
            DaggerfallUnityItem newItem = new DaggerfallUnityItem(ItemGroups.Jewellery, groupIndex);

            return newItem;
        }

        /// <summary>
        /// Creates a new random drug.
        /// </summary>
        /// <returns>DaggerfallUnityItem.</returns>
        public static DaggerfallUnityItem CreateRandomDrug()
        {
            Array enumArray = DaggerfallUnity.Instance.ItemHelper.GetEnumArray(ItemGroups.Drugs);
            int groupIndex = UnityEngine.Random.Range(0, enumArray.Length);
            DaggerfallUnityItem newItem = new DaggerfallUnityItem(ItemGroups.Drugs, groupIndex);

            return newItem;
        }

        /// <summary>
        /// Generates a weapon.
        /// </summary>
        /// <param name="weapon"></param>
        /// <param name="material">Ignored for arrows</param>
        /// <returns></returns>
        public static DaggerfallUnityItem CreateWeapon(Weapons weapon, MaterialTypes material)
        {
            // Create item
            int groupIndex = DaggerfallUnity.Instance.ItemHelper.GetGroupIndex(ItemGroups.Weapons, (int)weapon);
            DaggerfallUnityItem newItem = new DaggerfallUnityItem(ItemGroups.Weapons, groupIndex);

            if (weapon == Weapons.Arrow)
            {   // Handle arrows
                newItem.stackCount = UnityEngine.Random.Range(1, 20 + 1);
                newItem.currentCondition = 0; // not sure if this is necessary, but classic does it
            }

            ApplyWeaponMaterial(newItem, material);
            int variant = GetWeaponVariant(newItem, material);
            SetVariant(newItem, variant);
            
            return newItem;
        }

        public static int GetWeaponVariant(DaggerfallUnityItem weapon, MaterialTypes material)
        {
            // For the moment, there's no Glass Exotic variant
            if (material == MaterialTypes.Glass || WeaponHasNoVariants(weapon.TemplateIndex))
                return 0;

            if (!GameManager.HasInstance)
                return 0;

            PlayerGPS playerGPS = GameManager.Instance.PlayerGPS;
            int chances = 1;

            if (IsFromThisArea(weapon))
                chances = 900;
            else if (playerGPS.IsPlayerInTown(true) && playerGPS.CurrentLocation.Exterior.ExteriorData.PortTownAndUnknown != 0)
                chances = 500;

            Debug.Log("Getting weapon variant");
            if (DFRandom.random_range_inclusive(1, 1000) <= chances)
                return 1;

            return 0;
        }

        public static bool IsFromThisArea(DaggerfallUnityItem weapon)
        {
            PlayerGPS playerGPS = GameManager.Instance.PlayerGPS;

            // At the moment, it's not possible to visit a place where daedric exotic weapons are "common"
            if (weapon.nativeMaterialValue == (int)MaterialTypes.Daedric)
                return false;

            if (weapon.nativeMaterialValue == (int)MaterialTypes.Iron)
            {
                if (IsFromUpperCraglorn(playerGPS.CurrentRegionIndex) &&
                   (weapon.TemplateIndex == (int)Weapons.Claymore ||
                    weapon.TemplateIndex == (int)Weapons.Warhammer ||
                    weapon.TemplateIndex == (int)Weapons.War_Axe))
                    return true;
            }

            if (weapon.nativeMaterialValue == (int)MaterialTypes.Orcish)
            {
                if (playerGPS.CurrentRegionIndex == Array.IndexOf(WorldData.WorldSetting.RegionNames, "Orsinium Area") &&
                   (weapon.TemplateIndex == (int)Weapons.Dagger ||
                    weapon.TemplateIndex == (int)Weapons.Tanto ||
                    weapon.TemplateIndex == (int)Weapons.Staff ||
                    weapon.TemplateIndex == (int)Weapons.Shortsword ||
                    weapon.TemplateIndex == (int)Weapons.Wakazashi ||
                    weapon.TemplateIndex == (int)Weapons.Longsword ||
                    weapon.TemplateIndex == (int)Weapons.Katana ||
                    weapon.TemplateIndex == (int)Weapons.Dai_Katana))
                    return true;
                if (IsFromHollowWastes(playerGPS.CurrentRegionIndex) &&
                   (weapon.TemplateIndex == (int)Weapons.Broadsword ||
                    weapon.TemplateIndex == (int)Weapons.Saber ||
                    weapon.TemplateIndex == (int)Weapons.Claymore ||
                    weapon.TemplateIndex == (int)Weapons.Mace ||
                    weapon.TemplateIndex == (int)Weapons.Flail))
                    return true;
                if (IsFromValusMountains(playerGPS.CurrentRegionIndex) &&
                   (weapon.TemplateIndex == (int)Weapons.Warhammer ||
                    weapon.TemplateIndex == (int)Weapons.Battle_Axe ||
                    weapon.TemplateIndex == (int)Weapons.War_Axe))                
                    return true;
                if (IsFromGreenshade(playerGPS.CurrentRegionIndex) &&
                    weapon.TemplateIndex == (int)Weapons.Short_Bow)
                    return true;
                if (IsFromGrathwood(playerGPS.CurrentRegionIndex) &&
                    weapon.TemplateIndex == (int)Weapons.Long_Bow)
                    return true;
            }

            return RandomWeaponOrigin(playerGPS.CurrentRegionIndex, weapon);
        }

        public static bool RandomWeaponOrigin(int regionIndex, DaggerfallUnityItem weapon)
        {
            int factor = weapon.TemplateIndex * weapon.nativeMaterialValue * 7;
            int regionNumber = WorldData.WorldSetting.RegionNames.Length;
            while (factor >= regionNumber)
            {
                factor -= regionNumber;
            }
            return regionIndex == factor;
        }

        public static int RandomWeaponOrigin(DaggerfallUnityItem weapon)
        {
            int factor = weapon.TemplateIndex * weapon.nativeMaterialValue * 7;
            int regionNumber = WorldData.WorldSetting.RegionNames.Length;
            while (factor >= regionNumber)
            {
                factor -= regionNumber;
            }
            return factor;
        }

        /// <summary>
        /// Checks if the passed region index belongs to a region in the Hollow Wastes area
        /// (https://en.uesp.net/wiki/Lore:Hollow_Wastes)
        /// </summary>
        public static bool IsFromHollowWastes(int regionIndex)
        {
            switch (regionIndex)
            {
                case 11:    // Dak'fron
                case 20:    // Sentinel
                case 47:    // Ayasofya
                case 48:    // Tigonus
                case 54:    // Santaki
                case 55:    // Antiphyllos
                case 56:    // Bergama
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks if the passed region index belongs to a region in the Valus Mountains area
        /// (https://en.uesp.net/wiki/Lore:Valus_Mountains)
        /// </summary>
        public static bool IsFromValusMountains(int regionIndex)
        {
            switch (regionIndex)
            {
                case 153:   // Kragenmoor
                case 154:   // Andrethis
                case 351:   // Cheydinhal
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks if the passed region index belongs to a region in the Grathwood area
        /// (https://en.uesp.net/wiki/Lore:Grahtwood)
        /// </summary>
        public static bool IsFromGrathwood(int regionIndex)
        {
            switch (regionIndex)
            {
                case 244:   // Tarlain Heights
                case 245:   // Cormount
                case 246:   // Stonesquare
                case 247:   // Southpoint
                case 251:   // Haven
                case 299:   // Greenhall
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks if the passed region index belongs to a region in the Greenshade area
        /// (https://en.uesp.net/wiki/Lore:Greenshade)
        /// </summary>
        public static bool IsFromGreenshade(int regionIndex)
        {
            switch (regionIndex)
            {
                case 239:   // Woodheart
                case 240:   // Vullen Haven
                case 241:   // Longhaven
                case 242:   // Marbruk Field
                case 243:   // Greenheart
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks if the passed region index belongs to a region in the Upper Craglorn area
        /// (https://en.uesp.net/wiki/Lore:Craglorn)
        /// </summary>
        public static bool IsFromUpperCraglorn(int regionIndex)
        {
            switch (regionIndex)
            {
                case 104:   // Belkarth
                case 105:   // Dragon Gate
                case 106:   // Dragonstar
                    return true;

                default:
                    return false;
            }
        }

        public static bool WeaponHasNoVariants(int weaponIndex)
        {
            switch (weaponIndex)
            {
                case 288:   // Hatchet
                case 289:   // Flail
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Creates random weapon.
        /// </summary>
        /// <returns>DaggerfallUnityItem</returns>
        public static DaggerfallUnityItem CreateRandomWeapon(int luck, int levelModifier = 1, bool isTownGuard = false)
        {
            // Create a random weapon type, including any custom items registered as weapons
            ItemHelper itemHelper = DaggerfallUnity.Instance.ItemHelper;
            Array enumArray = itemHelper.GetEnumArray(ItemGroups.Weapons);
            int[] customItemTemplates = itemHelper.GetCustomItemsForGroup(ItemGroups.Weapons);

            int groupIndex = UnityEngine.Random.Range(0, enumArray.Length + customItemTemplates.Length);
            DaggerfallUnityItem newItem;
            if (groupIndex < enumArray.Length)
                newItem = new DaggerfallUnityItem(ItemGroups.Weapons, groupIndex);
            else
                newItem = CreateItem(ItemGroups.Weapons, customItemTemplates[groupIndex - enumArray.Length]);
 
            // Random weapon material
            MaterialTypes material = FormulaHelper.RandomMaterial(luck, levelModifier, isTownGuard);

            if (groupIndex == 18)
            {   // Handle arrows
                newItem.stackCount = UnityEngine.Random.Range(1, 20 + 1);
                newItem.currentCondition = 0; // not sure if this is necessary, but classic does it
            }
            
            ApplyWeaponMaterial(newItem, material);
            int variant = GetWeaponVariant(newItem, material);
            SetVariant(newItem, variant);
            
            return newItem;
        }

        /// <summary>Set material and adjust weapon stats accordingly</summary>
        public static void ApplyWeaponMaterial(DaggerfallUnityItem weapon, MaterialTypes material)
        {
            weapon.nativeMaterialValue = (int)material;
            weapon = SetItemPropertiesByMaterial(weapon, (int)material);
            weapon.dyeColor = DaggerfallUnity.Instance.ItemHelper.GetWeaponDyeColor(material);

            // Female characters use archive - 1 (i.e. 233 rather than 234) for weapons
            if (GameManager.Instance.PlayerEntity.Gender == Genders.Female)
            {
                if (!GameManager.Instance.ItemHelper.IsNewWeapon(weapon) || weapon.TemplateIndex == (int)Weapons.Crossbow)
                    weapon.PlayerTextureArchive -= 1;
            }
        }

        /// <summary>
        /// Generates armour.
        /// </summary>
        /// <param name="gender">Gender armor is created for.</param>
        /// <param name="race">Race armor is created for.</param>
        /// <param name="armor">Type of armor item to create.</param>
        /// <param name="armorType"> Base type of armour.</param>
        /// <param name="material">Material of armor.</param>
        /// <param name="variant">Visual variant of armor. If -1, a random variant is chosen.</param>
        /// <returns>DaggerfallUnityItem</returns>
        public static DaggerfallUnityItem CreateArmor(Genders gender, Races race, Armor armor, ArmorMaterialTypes material, int variant = -1)
        {
            // Create item
            int groupIndex = DaggerfallUnity.Instance.ItemHelper.GetGroupIndex(ItemGroups.Armor, (int)armor);
            Debug.Log("Creating armour with group index " + groupIndex);
            DaggerfallUnityItem newItem = new DaggerfallUnityItem(ItemGroups.Armor, groupIndex);

            ApplyArmorSettings(newItem, gender, race, material, variant);

            return newItem;
        }

        /// <summary>
        /// Creates random armor.
        /// </summary>
        /// <param name="luck">Luck stat for material type.</param>
        /// <param name="gender">Gender armor is created for.</param>
        /// <param name="race">Race armor is created for.</param>
        /// <returns>DaggerfallUnityItem</returns>
        public static DaggerfallUnityItem CreateRandomArmor(int luck, Genders gender, Races race, int levelModifier = 1, bool isTownGuard = false)
        {
            // Create a random armor type, including any custom items registered as armor
            ItemHelper itemHelper = DaggerfallUnity.Instance.ItemHelper;
            Array enumArray = itemHelper.GetEnumArray(ItemGroups.Armor);
            // int[] customItemTemplates = itemHelper.GetCustomItemsForGroup(ItemGroups.Armor);

            int groupIndex = GetArmorIndex(enumArray.Length);
            Debug.Log("enumArray.Length: " + enumArray.Length + ", groupIndex: " + groupIndex);
            DaggerfallUnityItem newItem;
            // if (groupIndex < enumArray.Length)
            newItem = new DaggerfallUnityItem(ItemGroups.Armor, groupIndex);
            // else
            //     newItem = CreateItem(ItemGroups.Armor, customItemTemplates[groupIndex - enumArray.Length]);

            ApplyArmorSettings(newItem, gender, race, FormulaHelper.RandomArmorMaterial((Armor)enumArray.GetValue(groupIndex), luck, levelModifier, isTownGuard));

            return newItem;
        }

        // Here we try to keep vanilla armor type proportions (Plate 1/10, Chain 2/10, Leather 7/10)
        // despite the new armor types. Works in tandem with chances in RandomArmorMaterial.
        public static int GetArmorIndex(int arrayLength)
        {
            int diceRoll = UnityEngine.Random.Range(0, 245);
            if (diceRoll < 35)
                return diceRoll % 7;
            if (diceRoll < 70)
                return diceRoll % 5 + 11;
            if (diceRoll < 210)
                return diceRoll % 7 + 16;
            // Shields
            if (diceRoll < 228)
                return 7;
            if (diceRoll < 238)
                return 8;
            if (diceRoll < 243)
                return 9;
            else return 10;
        }

        /// <summary>Set gender, body morphology and material of armor</summary>
        public static void ApplyArmorSettings(DaggerfallUnityItem armor, Genders gender, Races race, ArmorMaterialTypes material, int variant = 0)
        {
            // Keep original archive in case a new type armor will be generated.
            Debug.Log("armor.PlayerTextureArchive: " + armor.PlayerTextureArchive);
            newArmorArchive = armor.PlayerTextureArchive;

            // Adjust for gender
            if (gender == Genders.Female)
                armor.PlayerTextureArchive = firstFemaleArchive;
            else
                armor.PlayerTextureArchive = firstMaleArchive;

            // Adjust for body morphology
            SetRace(armor, race);

            // Adjust material
            ApplyArmorMaterial(armor, material);

            if (IsNewArmor(armor))
            {
                armor.PlayerTextureArchive = newArmorArchive;

                if (gender == Genders.Male)
                    armor.PlayerTextureArchive += 4;

                if (armor.IsOfTemplate(ItemGroups.Armor, (int)Armor.Helmet))
                {
                    if ((gender == Genders.Male && (race == Races.Khajiit || race == Races.DarkElf || race == Races.HighElf || race == Races.WoodElf)) ||
                        gender == Genders.Female && race == Races.Argonian)
                        armor.PlayerTextureArchive--;
                }
                Debug.Log("Second passage - armor.PlayerTextureArchive: " + armor.PlayerTextureArchive);
            }

            // Adjust for variant
            if (variant >= 0)
                SetVariant(armor, variant);
            else
                RandomizeArmorVariant(armor);
        }

        /// <summary>Set material and adjust armor stats accordingly</summary>
        public static void ApplyArmorMaterial(DaggerfallUnityItem armor, ArmorMaterialTypes material)
        {
            armor.nativeMaterialValue = (int)material;

            if (armor.nativeMaterialValue == (int)ArmorMaterialTypes.Leather) // Managing standard leather
            {
                armor.weightInKg /= 2;
            }
            else if (armor.nativeMaterialValue == (int)ArmorMaterialTypes.Fur)
            {
                armor.weightInKg -= 2;
            }
            else if (armor.nativeMaterialValue >= (int)ArmorMaterialTypes.Chain)    // Managing standard chain
            {
                armor.value *= 2;
            }
            
            if (GetArmorMaterialType((int)material) >= MaterialTypes.Iron)
            {
                Debug.Log("materia: " + material + ", Armor material type: " + GetArmorMaterialType((int)material));
                armor = SetItemPropertiesByMaterial(armor, (int)material);
            }

            Debug.Log("armor.PlayerTextureArchive: " + armor.PlayerTextureArchive);

            armor.dyeColor = DaggerfallUnity.Instance.ItemHelper.GetArmorDyeColor(material);
        }

        public static bool IsNewArmor(DaggerfallUnityItem armor)
        {
            // ArmorMaterialTypes armorType = GetArmorMaterialType(armor.nativeMaterialValue);

            if ((armor.nativeMaterialValue >= (int)ArmorMaterialTypes.LeatherIron && armor.nativeMaterialValue <= (int)ArmorMaterialTypes.LeatherDaedric) ||
                 armor.nativeMaterialValue == (int)ArmorMaterialTypes.Fur ||
                 newArmorArchive >= 10000)
                return true;
            
            return false;
        }

        /// <summary>
        /// Creates random magic item in same manner as classic.
        /// </summary>
        /// <returns>DaggerfallUnityItem</returns>
        public static DaggerfallUnityItem CreateRandomMagicItem(int playerLuck, Genders gender, Races race, int levelModifier = 1)
        {
            return CreateRegularMagicItem(chooseAtRandom, playerLuck, gender, race, levelModifier);
        }

        /// <summary>
        /// Create a regular non-artifact magic item.
        /// </summary>
        /// <param name="chosenItem">An integer index of the item to create, or -1 for a random one.</param>
        /// <param name="playerLuck">The player luck live stat.</param>
        /// <param name="gender">The gender to create an item for.</param>
        /// <param name="race">The race to create an item for.</param>
        /// <returns>DaggerfallUnityItem</returns>
        /// <exception cref="Exception">When a base item cannot be created.</exception>
        public static DaggerfallUnityItem CreateRegularMagicItem(int chosenItem, int playerLuck, Genders gender, Races race, int levelModifier = 1)
        {
            byte[] itemGroups0 = { 2, 3, 6, 10, 12, 14, 25 };
            byte[] itemGroups1 = { 2, 3, 6, 12, 25 };

            DaggerfallUnityItem newItem = null;

            // Get the list of magic item templates read from MAGIC.DEF
            MagicItemTemplate[] magicItems = DaggerfallUnity.Instance.ItemHelper.MagicItemTemplates;

            // Reduce the list to only the regular magic items.
            MagicItemTemplate[] regularMagicItems = magicItems.Where(template => template.type == MagicItemTypes.RegularMagicItem).ToArray();
            if (chosenItem > regularMagicItems.Length)
                throw new Exception(string.Format("Magic item subclass {0} does not exist", chosenItem));

            // Pick a random one if needed.
            if (chosenItem == chooseAtRandom)
            {
                chosenItem = UnityEngine.Random.Range(0, regularMagicItems.Length);
            }

            // Get the chosen template
            MagicItemTemplate magicItem = regularMagicItems[chosenItem];

            // Get the item group. The possible groups are determined by the 33rd byte (magicItem.group) of the MAGIC.DEF template being used.
            ItemGroups group = 0;
            if (magicItem.group == 0)
                group = (ItemGroups)itemGroups0[UnityEngine.Random.Range(0, 7)];
            else if (magicItem.group == 1)
                group = (ItemGroups)itemGroups1[UnityEngine.Random.Range(0, 5)];
            else if (magicItem.group == 2)
                group = ItemGroups.Weapons;

            // Create the base item
            if (group == ItemGroups.Weapons)
            {
                newItem = CreateRandomWeapon(playerLuck, levelModifier);

                // No arrows as enchanted items
                while (newItem.GroupIndex == 18)
                    newItem = CreateRandomWeapon(playerLuck, levelModifier);
            }
            else if (group == ItemGroups.Armor)
                newItem = CreateRandomArmor(playerLuck, gender, race, levelModifier);
            else if (group == ItemGroups.MensClothing || group == ItemGroups.WomensClothing)
                newItem = CreateRandomClothing(gender, race);
            else if (group == ItemGroups.ReligiousItems)
                newItem = CreateRandomReligiousItem();
            else if (group == ItemGroups.Gems)
                newItem = CreateRandomGem();
            else // Only other possibility is jewellery
                newItem = CreateRandomJewellery();

            if (newItem == null)
                throw new Exception("CreateRegularMagicItem() failed to create an item.");

            // Replace the regular item name with the magic item name
            newItem.shortName = magicItem.name;

            // Add the enchantments
            newItem.legacyMagic = new DaggerfallEnchantment[magicItem.enchantments.Length];
            for (int i = 0; i < magicItem.enchantments.Length; ++i)
                newItem.legacyMagic[i] = magicItem.enchantments[i];

            // Set the condition/magic uses
            newItem.maxCondition = magicItem.uses;
            newItem.currentCondition = magicItem.uses;

            // Set the value of the item. This is determined by the enchantment point cost/spell-casting cost
            // of the enchantments on the item.
            int value = 0;
            for (int i = 0; i < magicItem.enchantments.Length; ++i)
            {
                if (magicItem.enchantments[i].type != EnchantmentTypes.None
                    && magicItem.enchantments[i].type < EnchantmentTypes.ItemDeteriorates)
                {
                    switch (magicItem.enchantments[i].type)
                    {
                        case EnchantmentTypes.CastWhenUsed:
                        case EnchantmentTypes.CastWhenHeld:
                        case EnchantmentTypes.CastWhenStrikes:
                            // Enchantments that cast a spell. The parameter is the spell index in SPELLS.STD.
                            value += Formulas.FormulaHelper.GetSpellEnchantPtCost(magicItem.enchantments[i].param);
                            break;
                        case EnchantmentTypes.RepairsObjects:
                        case EnchantmentTypes.AbsorbsSpells:
                        case EnchantmentTypes.EnhancesSkill:
                        case EnchantmentTypes.FeatherWeight:
                        case EnchantmentTypes.StrengthensArmor:
                            // Enchantments that provide an effect that has no parameters
                            value += enchantmentPointCostsForNonParamTypes[(int)magicItem.enchantments[i].type];
                            break;
                        case EnchantmentTypes.SoulBound:
                            // Bound soul
                            MobileEnemy mobileEnemy = GameObjectHelper.EnemyDict[magicItem.enchantments[i].param];
                            value += mobileEnemy.SoulPts; // TODO: Not sure about this. Should be negative? Needs to be tested.
                            break;
                        default:
                            // Enchantments that provide a non-spell effect with a parameter (parameter = when effect applies, what enemies are affected, etc.)
                            value += enchantmentPtsForItemPowerArrays[(int)magicItem.enchantments[i].type][magicItem.enchantments[i].param];
                            break;
                    }
                }
            }

            newItem.value = value;

            return newItem;
        }

        /// <summary>
        /// Sets properties for a weapon or piece of armor based on its material.
        /// </summary>
        /// <param name="item">Item to have its properties modified.</param>
        /// <param name="material">Material to use to apply properties.</param>
        /// <returns>DaggerfallUnityItem</returns>
        public static DaggerfallUnityItem SetItemPropertiesByMaterial(DaggerfallUnityItem item, int nativeMaterialValue)
        {
            int multiplier = 1;
            MaterialTypes material = MaterialTypes.Iron;
            Debug.Log("nativeMaterialValue: " + nativeMaterialValue);

            if (item.ItemGroup == ItemGroups.Weapons)
            {
                multiplier = 4;
                material = (MaterialTypes)nativeMaterialValue;
            }
            else
            {
                ArmorTypes armor = GetArmorType(nativeMaterialValue);
                material = GetArmorMaterialType(nativeMaterialValue);
                switch (armor)
                {
                    case ArmorTypes.Leather:
                        multiplier = 2;
                        break;
                    case ArmorTypes.Chain:
                        multiplier = 6;
                        break;
                    case ArmorTypes.Plate:
                        multiplier = 10;
                        break;
                    default:
                        break;
                }
            }

            Debug.Log("(int)GetArmorMaterialType(item.nativeMaterialValue): " + (int)GetArmorMaterialType((int)material));
            item.value *= multiplier * valueMultipliersByMaterial[(int)GetArmorMaterialType((int)material)];
            item.weightInKg += (CalculateWeightForMaterial(item, GetArmorMaterialType((int)material)) - item.weightInKg) * multiplier / 6;
            item.maxCondition = item.maxCondition * conditionMultipliersByMaterial[(int)GetArmorMaterialType((int)material)] / 4;
            item.currentCondition = item.maxCondition;

            return item;
        }

        static float CalculateWeightForMaterial(DaggerfallUnityItem item, MaterialTypes material)
        {
            int quarterKgs = (int)(item.weightInKg * 4);
            Debug.Log("(int)material: " + (int)material);
            float matQuarterKgs = (float)(quarterKgs * weightMultipliersByMaterial[(int)material]) / 4;
            return Mathf.Round(matQuarterKgs) / 4;
        }

        /// <summary>
        /// Creates a random ingredient from any of the ingredient groups.
        /// Passing a non-ingredient group will return null.
        /// </summary>
        /// <param name="ingredientGroup">Ingredient group.</param>
        /// <returns>DaggerfallUnityItem</returns>
        public static DaggerfallUnityItem CreateRandomIngredient(ItemGroups ingredientGroup)
        {
            int groupIndex;
            Array enumArray;
            switch (ingredientGroup)
            {
                case ItemGroups.CreatureIngredients1:
                case ItemGroups.CreatureIngredients2:
                case ItemGroups.CreatureIngredients3:
                case ItemGroups.MetalIngredients:
                case ItemGroups.MiscellaneousIngredients1:
                case ItemGroups.MiscellaneousIngredients2:
                case ItemGroups.PlantIngredients1:
                case ItemGroups.PlantIngredients2:
                    enumArray = DaggerfallUnity.Instance.ItemHelper.GetEnumArray(ingredientGroup);
                    groupIndex = UnityEngine.Random.Range(0, enumArray.Length);
                    break;
                default:
                    return null;
            }

            // Create item
            DaggerfallUnityItem newItem = new DaggerfallUnityItem(ingredientGroup, groupIndex);

            return newItem;
        }

        /// <summary>
        /// Creates a random ingredient from a random ingredient group.
        /// </summary>
        /// <returns>DaggerfallUnityItem</returns>
        public static DaggerfallUnityItem CreateRandomIngredient()
        {
            // Randomise ingredient group
            ItemGroups itemGroup;
            int group = UnityEngine.Random.Range(0, 8);
            Array enumArray;
            switch (group)
            {
                case 0:
                    itemGroup = ItemGroups.CreatureIngredients1;
                    break;
                case 1:
                    itemGroup = ItemGroups.CreatureIngredients2;
                    break;
                case 2:
                    itemGroup = ItemGroups.CreatureIngredients3;
                    break;
                case 3:
                    itemGroup = ItemGroups.MetalIngredients;
                    break;
                case 4:
                    itemGroup = ItemGroups.MiscellaneousIngredients1;
                    break;
                case 5:
                    itemGroup = ItemGroups.MiscellaneousIngredients2;
                    break;
                case 6:
                    itemGroup = ItemGroups.PlantIngredients1;
                    break;
                case 7:
                    itemGroup = ItemGroups.PlantIngredients2;
                    break;
                default:
                    return null;
            }

            // Randomise ingredient within group
            enumArray = DaggerfallUnity.Instance.ItemHelper.GetEnumArray(itemGroup);
            int groupIndex = UnityEngine.Random.Range(0, enumArray.Length);

            // Create item
            DaggerfallUnityItem newItem = new DaggerfallUnityItem(itemGroup, groupIndex);

            return newItem;
        }

        /// <summary>
        /// Creates a potion.
        /// </summary>
        /// <param name="recipe">Recipe index for the potion</param>
        /// <returns>Potion DaggerfallUnityItem</returns>
        public static DaggerfallUnityItem CreatePotion(int recipeKey, int stackSize = 1)
        {
            return new DaggerfallUnityItem(ItemGroups.UselessItems1, 1) { PotionRecipeKey = recipeKey, stackCount = stackSize };
        }

        /// <summary>
        /// Creates a random potion from all registered recipes.
        /// </summary>
        /// <returns>Potion DaggerfallUnityItem</returns>
        public static DaggerfallUnityItem CreateRandomPotion(int stackSize = 1)
        {
            List<int> recipeKeys = GameManager.Instance.EntityEffectBroker.GetPotionRecipeKeys();
            int recipeIdx = UnityEngine.Random.Range(0, recipeKeys.Count);
            return CreatePotion(recipeKeys[recipeIdx]);
        }

        /// <summary>
        /// Creates a random (classic) potion
        /// </summary>
        /// <returns>Potion DaggerfallUnityItem</returns>
        public static DaggerfallUnityItem CreateRandomClassicPotion()
        {
            int recipeIdx = UnityEngine.Random.Range(0, MagicAndEffects.PotionRecipe.classicRecipeKeys.Length);
            return CreatePotion(MagicAndEffects.PotionRecipe.classicRecipeKeys[recipeIdx]);
        }

        /// <summary>
        /// Generates gold pieces.
        /// </summary>
        /// <param name="amount">Total number of gold pieces in stack.</param>
        /// <returns></returns>
        public static DaggerfallUnityItem CreateGoldPieces(int amount)
        {
            DaggerfallUnityItem newItem = CreateItem(ItemGroups.Currency, (int)Currency.Gold_pieces);
            newItem.stackCount = amount;
            newItem.value = 1;

            return newItem;
        }

        /// <summary>
        /// Sets a random variant of clothing item.
        /// </summary>
        /// <param name="item">Item to randomize variant.</param>
        public static void RandomizeClothingVariant(DaggerfallUnityItem item)
        {
            int totalVariants = item.ItemTemplate.variants;
            SetVariant(item, UnityEngine.Random.Range(0, totalVariants));
        }

        /// <summary>
        /// Sets a random variant of armor item.
        /// </summary>
        /// <param name="item">Item to randomize variant.</param>
        public static void RandomizeArmorVariant(DaggerfallUnityItem item)
        {
            int variant = 0;

            // We only need to pick randomly where there is more than one possible variant. Otherwise we can just pass in 0 to SetVariant and
            // the correct variant will still be chosen.
            if (item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Cuirass))
            {
                variant = UnityEngine.Random.Range(1, 4);
            }
            // else if (item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Jerkin))
            // {
            //     if (item.nativeMaterialValue == (int)ArmorMaterialTypes.Fur)
            //         variant = 11;
            //     else
            //         variant = (int)GetArmorMaterialType(item.nativeMaterialValue);
            // }
            else if (item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Greaves))
            {
                if (item.nativeMaterialValue == (int)ArmorMaterialTypes.Leather)
                    variant = UnityEngine.Random.Range(0, 2);
                else if (item.nativeMaterialValue >= (int)ArmorMaterialTypes.PlateIron)
                    variant = UnityEngine.Random.Range(2, 6);
            }
            // else if (item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Cuisse))
            // {
            //     if (item.nativeMaterialValue == (int)ArmorMaterialTypes.Fur)
            //         variant = 11;
            //     else
            //         variant = (int)GetArmorMaterialType(item.nativeMaterialValue);
            // }
            else if (item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Left_Pauldron) || item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Right_Pauldron))
            {
                if (item.nativeMaterialValue >= (int)ArmorMaterialTypes.PlateIron)
                    variant = UnityEngine.Random.Range(1, 4);
            }
            // else if (item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Left_Vambrace) || item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Right_Vambrace))
            // {
            //     if (item.nativeMaterialValue == (int)ArmorMaterialTypes.Fur)
            //         variant = 11;
            //     else
            //         variant = (int)GetArmorMaterialType(item.nativeMaterialValue);
            // }
            else if (item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Boots) && (item.nativeMaterialValue >= (int)ArmorMaterialTypes.PlateIron))
            {
                variant = UnityEngine.Random.Range(1, 3);
            }
            else if (item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Helm))
            {
                variant = UnityEngine.Random.Range(0, item.ItemTemplate.variants);
            }
            // else if (item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Light_Boots))
            // {
            //     variant = UnityEngine.Random.Range(0, 2);
            // }
            // else if (item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Gloves))
            // {
            //     variant = UnityEngine.Random.Range(0, 2);
            // }
            SetVariant(item, variant);
        }

        /// <summary>
        /// If no location is passed, a map from a random settlement location is generated.
        /// Take into account that, although maps closer to the player current position are
        /// going to be generated much more frequently, the randomisation can get some pretty
        /// crazy results.
        /// </summary>
        /// <param name="location">Location to generate a town map of.</param>
        /// <param name="exoticism">Value to roll to get a map from "far away".</param>
        /// <param name="anyRegion">If a random map can be created of locations in other regions in this province.</param>
        /// <param name="anyProvince">If a random map can be created of locations in other provinces in this continent.</param>
        /// <param name="anyContinent">If a random map can be created of locations in other continents (on this planet).</param>
        public static DaggerfallUnityItem CreateTownMap(int exoticism = 100, bool anyRegion = false, bool anyProvince = false, bool anyContinent = false, DFLocation location = new DFLocation())
        {
            DaggerfallUnityItem townMap = new DaggerfallUnityItem(ItemGroups.Maps, 1);
            int diceRoll = Dice100.Roll();

            if (location.Exterior.ExteriorData.BlockNames == null && diceRoll < exoticism)
            {
                PlayerGPS playerGPS = GameManager.Instance.PlayerGPS;
                ProvinceNames pickedProvince = playerGPS.GetProvinceFromRegion();
                int pickedRegion = playerGPS.CurrentPoliticIndex;
                (int, int) currentTile = playerGPS.CurrentTile;
                List<(int, int)> locIdxs = new List<(int, int)>();

                if (anyRegion)
                {
                    diceRoll = Dice100.Roll();
                    if (diceRoll >= exoticism && anyProvince)
                    {
                        diceRoll = Dice100.Roll();
                        if (diceRoll >= exoticism && anyContinent)
                        {
                            // TODO: expand this
                        }
                        else{
                            pickedProvince = (ProvinceNames)UnityEngine.Random.Range(1, Enum.GetNames(typeof(ProvinceNames)).Length + 1);
                            townMap.value *= 10;
                        }
                    }
                    pickedRegion = WorldData.WorldSetting.regionInProvince[(int)pickedProvince][UnityEngine.Random.Range(0, WorldData.WorldSetting.regionInProvince[(int)pickedProvince].Length)];
                    townMap.value *= 5;
                }

                if (diceRoll > 50) // town map from the same region OR exotic map
                {
                    Debug.Log("Trying to create town map from " + WorldData.WorldSetting.RegionNames[pickedRegion]);
                    for (int t = 0; t < WorldMaps.regionTiles[pickedRegion].Count; t++)
                    {
                        DFRegion tile = WorldMaps.ConvertWorldMapsToDFRegion(WorldMaps.regionTiles[pickedRegion][t], true);
                        for (int r = 0; r < tile.LocationCount; r++)
                        {
                            if (tile.MapTable[r].LocationType == DFRegion.LocationTypes.TownCity ||
                                tile.MapTable[r].LocationType == DFRegion.LocationTypes.TownHamlet ||
                                tile.MapTable[r].LocationType == DFRegion.LocationTypes.TownVillage)
                                locIdxs.Add((MapsFile.MapPixelToTile(MapsFile.GetPixelFromPixelID(tile.MapTable[r].MapId)), r));                            
                        }
                    }
                }
                else{   // town map from the same geographical area (9x9 tiles)
                    Debug.Log("Trying to create local town map.");
                    for (int gX = 0; gX < 3; gX++)
                    {
                        for (int gY = 0; gY < 3; gY++)
                        {
                            DFRegion localTile = WorldMaps.ConvertWorldMapsToDFRegion(gY * 3 + gX);
                            for (int gr = 0; gr < localTile.LocationCount; gr++)
                            {
                                if (localTile.MapTable[gr].LocationType == DFRegion.LocationTypes.TownCity ||
                                    localTile.MapTable[gr].LocationType == DFRegion.LocationTypes.TownHamlet ||
                                    localTile.MapTable[gr].LocationType == DFRegion.LocationTypes.TownVillage)
                                    locIdxs.Add((MapsFile.MapPixelToTile(MapsFile.GetPixelFromPixelID(localTile.MapTable[gr].MapId)), gr));
                            }
                        }
                    }
                }

                int locIndex = UnityEngine.Random.Range(0, locIdxs.Count);
                MapSummary locSummary;
                if (locIdxs.Count < 1)
                    return null;
                DFRegion finalTile = WorldMaps.ConvertWorldMapsToDFRegion(locIdxs[locIndex].Item1, true);
                DFPosition pos = MapsFile.GetPixelFromPixelID(finalTile.MapTable[locIdxs[locIndex].Item2].MapId);
                townMap.message = locIdxs[locIndex].Item1 * 10000 + locIdxs[locIndex].Item2;
                townMap.shortName = townMap.shortName + " " + finalTile.MapNames[locIdxs[locIndex].Item2] + " (" + WorldData.WorldSetting.RegionNames[PoliticData.GetAbsPoliticValue(pos.X, pos.Y)] + ")";
            }
            else{
                townMap.message = location.AbsTileIndex * 10000 + location.LocationIndex;
                townMap.shortName = townMap.shortName + " " + location.Name + " (" + location.RegionName + ")";
            }
            Debug.Log("townMap.ItemName: " + townMap.ItemName);

            return townMap;
        }

        public static DaggerfallUnityItem CreateLocationMap(int exoticism = 100, bool anyRegion = false, bool anyProvince = false, bool anyContinent = false, DFLocation location = new DFLocation(), DFRegion.LocationTypes locType = DFRegion.LocationTypes.None)
        {
            DaggerfallUnityItem locationMap = new DaggerfallUnityItem(ItemGroups.Maps, 0);
            int variant = 0;

            if (location.Exterior.ExteriorData.BlockNames == null)
            {
                int diceRoll;
                PlayerGPS playerGPS = GameManager.Instance.PlayerGPS;
                ProvinceNames pickedProvince = playerGPS.GetProvinceFromRegion();
                int pickedRegion = playerGPS.CurrentPoliticIndex;
                (int, int) currentTile = playerGPS.CurrentTile;
                List<(int, int)> locIdxs = new List<(int, int)>();

                if (locType == DFRegion.LocationTypes.None)
                {
                    diceRoll = Dice100.Roll();

                    if (diceRoll <= 30)
                        locType = DFRegion.LocationTypes.TownCity;
                    else if (diceRoll <= 40)
                        locType = DFRegion.LocationTypes.TownHamlet;
                    else if (diceRoll <= 45)
                        locType = DFRegion.LocationTypes.DungeonLabyrinth;
                    else if (diceRoll <= 55)
                        locType = DFRegion.LocationTypes.DungeonKeep;
                    else if (diceRoll <= 70)
                        locType = DFRegion.LocationTypes.DungeonRuin;
                    else if (diceRoll <= 80)
                        locType = DFRegion.LocationTypes.ReligionTemple;
                    else if (diceRoll <= 98)
                        locType = DFRegion.LocationTypes.ReligionCult;
                    else if (diceRoll <= 100)
                        locType = DFRegion.LocationTypes.Coven;
                }

                switch (locType)
                {
                    case DFRegion.LocationTypes.DungeonRuin:
                    case DFRegion.LocationTypes.DungeonKeep:
                    case DFRegion.LocationTypes.DungeonLabyrinth:
                        locationMap.value *= (10 - (int)locType);
                        variant = 3;
                        break;
                    case DFRegion.LocationTypes.ReligionTemple:
                    case DFRegion.LocationTypes.ReligionCult:
                        locationMap.value /= 2;
                        variant = 1;
                        break;
                    case DFRegion.LocationTypes.Coven:
                        locationMap.value *= 100000;
                        variant = 2;
                        break;
                    case DFRegion.LocationTypes.TownCity:
                    case DFRegion.LocationTypes.TownHamlet:
                    default:
                        break;
                }

                diceRoll = Dice100.Roll();

                if (diceRoll >= exoticism && anyRegion)
                {
                    diceRoll = Dice100.Roll();
                    if (diceRoll >= exoticism && anyProvince)
                    {
                        diceRoll = Dice100.Roll();
                        if (diceRoll >= exoticism && anyContinent)
                        {
                            // TODO: expand this
                        }
                        else{
                            pickedProvince = (ProvinceNames)UnityEngine.Random.Range(1, Enum.GetNames(typeof(ProvinceNames)).Length + 1);
                            locationMap.value *= 10;
                        }
                    }
                    pickedRegion = WorldData.WorldSetting.regionInProvince[(int)pickedProvince][UnityEngine.Random.Range(0, WorldData.WorldSetting.regionInProvince[(int)pickedProvince].Length)];
                    locationMap.value *= 5;
                }

                if (diceRoll > 50) // map from the same region OR exotic map
                {
                    for (int t = 0; t < WorldMaps.regionTiles[pickedRegion].Count; t++)
                    {
                        DFRegion tile = WorldMaps.ConvertWorldMapsToDFRegion(WorldMaps.regionTiles[pickedRegion][t], true);
                        for (int r = 0; r < tile.LocationCount; r++)
                        {
                            if (tile.MapTable[r].LocationType == locType)
                                locIdxs.Add((MapsFile.MapPixelToTile(MapsFile.GetPixelFromPixelID(tile.MapTable[r].MapId)), r));                            
                        }
                    }
                }
                else{   // map from the same geographical area (9x9 tiles)
                    for (int gX = 0; gX < 3; gX++)
                    {
                        for (int gY = 0; gY < 3; gY++)
                        {
                            DFRegion localTile = WorldMaps.ConvertWorldMapsToDFRegion(gY * 3 + gX);
                            for (int gr = 0; gr < localTile.LocationCount; gr++)
                            {
                                if (localTile.MapTable[gr].LocationType == locType)
                                    locIdxs.Add((MapsFile.MapPixelToTile(MapsFile.GetPixelFromPixelID(localTile.MapTable[gr].MapId)), gr));
                            }
                        }
                    }
                }

                if (locIdxs.Count == 0)
                {
                    Debug.Log("No suitable location found.");
                    return null;
                }

                int locIndex = UnityEngine.Random.Range(0, locIdxs.Count);
                MapSummary locSummary;
                DFRegion finalTile = WorldMaps.ConvertWorldMapsToDFRegion(locIdxs[locIndex].Item1, true);
                DFPosition pos = MapsFile.GetPixelFromPixelID(finalTile.MapTable[locIdxs[locIndex].Item2].MapId);
                locationMap.message = locIdxs[locIndex].Item1 * 10000 + locIdxs[locIndex].Item2;
                locationMap.shortName = locationMap.shortName + " " + finalTile.MapNames[locIdxs[locIndex].Item2] + " (" + WorldData.WorldSetting.RegionNames[PoliticData.GetAbsPoliticValue(pos.X, pos.Y)] + ")";
            }
            else
            {
                locationMap.message = location.AbsTileIndex * 10000 + location.LocationIndex;
                locationMap.shortName = locationMap.shortName + " " + location.Name + " (" + location.RegionName + ")";
            }
            SetVariant(locationMap, variant);
            return locationMap;            
        }

        #endregion

        #region Static Utility Methods

        public static void SetRace(DaggerfallUnityItem item, Races race)
        {
            int offset = (int)GetBodyMorphology(race);
            item.PlayerTextureArchive += offset;
        }

        public static void SetClothQuality(DaggerfallUnityItem item, int quality, out ClothCraftsmanship clothCraftsmanship)
        {
            int[] tier = new int[4];
            clothCraftsmanship = ClothCraftsmanship.Cheap;
            for (int i = 0; i < 4; i++)
            {
                tier[i] = UnityEngine.Random.Range(0, 100) + quality;
                if (tier[i] >= 80)
                    clothCraftsmanship++;
            }

            // Calling "Tall Sandals" by their name
            if ((item.TemplateIndex == (int)WomensClothing.Sandals ||
                 item.TemplateIndex == (int)MensClothing.Sandals) &&
                 item.CurrentVariant == 0)
                item.shortName = "Tall " + item.shortName;

            item.craftsmanship = clothCraftsmanship;
            switch (clothCraftsmanship)
            {
                case ClothCraftsmanship.Cheap:
                    item.shortName = "Cheap " + item.shortName;
                    break;
                case ClothCraftsmanship.Normal:
                    item.value *= 2;
                    item.maxCondition *= 3;
                    item.enchantmentPoints *= 2;
                    break;
                case ClothCraftsmanship.Fancy:
                    item.shortName = "Fancy " + item.shortName;
                    item.value *= 5;
                    item.maxCondition *= 2;
                    item.enchantmentPoints *= 5;
                    break;
                case ClothCraftsmanship.Extravagant:
                    item.shortName = "Extravagant " + item.shortName;
                    item.value *= 10;
                    item.maxCondition *= 2;
                    item.enchantmentPoints *= 7;
                    break;
                case ClothCraftsmanship.Exquisite:
                    item.shortName = "Exquisite " + item.shortName;
                    item.value *= 20;
                    item.maxCondition *= 3;
                    item.enchantmentPoints *= 10;
                    break;
            }
        }

        public static void SetClothDyeData(DaggerfallUnityItem item)
        {
            if (item.ItemGroup == ItemGroups.WomensClothing)
            {
                if (item.TemplateIndex == (int)WomensClothing.Brassier ||
                    item.TemplateIndex == (int)WomensClothing.Peasant_blouse ||
                   (item.TemplateIndex == (int)WomensClothing.Casual_pants && item.CurrentVariant == 4) ||
                    item.TemplateIndex == (int)WomensClothing.Casual_cloak ||
                   (item.TemplateIndex == (int)WomensClothing.Evening_gown && item.CurrentVariant == 0) ||
                    item.TemplateIndex == (int)WomensClothing.Loincloth ||
                    item.TemplateIndex == (int)WomensClothing.Plain_robes ||
                    item.TemplateIndex == (int)WomensClothing.Priestess_robes ||
                    item.TemplateIndex == (int)WomensClothing.Open_tunic ||
                   (item.TemplateIndex == (int)WomensClothing.Tights && item.CurrentVariant == 0))
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.BasicClothing };
                    item.dyeLevel = 1;
                }
                else if (item.TemplateIndex == (int)WomensClothing.Formal_brassier &&
                        (item.CurrentVariant == 0 || item.CurrentVariant == 3))
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.BasicClothing, DyeTargets.YellowClothing };
                    item.dyeLevel = 2;
                }
                else if (item.TemplateIndex == (int)WomensClothing.Formal_brassier && item.CurrentVariant == 1)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.SteelClothing };
                    item.dyeLevel = 1;
                }
                else if ((item.TemplateIndex == (int)WomensClothing.Formal_brassier && item.CurrentVariant == 2) ||
                         (item.TemplateIndex == (int)WomensClothing.Casual_pants && item.CurrentVariant == 3))
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.LightBrownClothing };
                    item.dyeLevel = 1;
                }
                else if (item.TemplateIndex == (int)WomensClothing.Formal_brassier && item.CurrentVariant == 4)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.RedClothing };
                    item.dyeLevel = 1;
                }
                else if (item.TemplateIndex == (int)WomensClothing.Eodoric ||
                        (item.TemplateIndex == (int)WomensClothing.Formal_eodoric && item.CurrentVariant != 0))
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.YellowClothing };
                    item.dyeLevel = 1;
                }
                else if (item.TemplateIndex == (int)WomensClothing.Shoes ||
                         item.TemplateIndex == (int)WomensClothing.Tall_boots ||
                         item.TemplateIndex == (int)WomensClothing.Boots ||
                        (item.TemplateIndex == (int)WomensClothing.Casual_pants && item.CurrentVariant < 3) ||
                        (item.TemplateIndex == (int)WomensClothing.Tights && item.CurrentVariant == 1))
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.LeatherClothing };
                    item.dyeLevel = 1;
                }
                else if (item.TemplateIndex == (int)WomensClothing.Sandals)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.RedClothing };
                    item.dyeLevel = 1;
                }
                else if (item.TemplateIndex == (int)WomensClothing.Casual_pants && item.CurrentVariant == 3)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.LightBrownClothing };
                    item.dyeLevel = 1;
                }
                else if (item.TemplateIndex == (int)WomensClothing.Formal_cloak ||
                         item.TemplateIndex == (int)WomensClothing.Evening_gown && item.CurrentVariant == 1)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.BasicClothing, DyeTargets.LightBrownClothing };
                    item.dyeLevel = 2;
                }
                else if (item.TemplateIndex == (int)WomensClothing.Khajiit_suit)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.BlackClothing };
                    item.dyeLevel = 1;
                }
                else if (item.TemplateIndex == (int)WomensClothing.Formal_eodoric && item.CurrentVariant == 0)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.YellowClothing, DyeTargets.BasicClothing };
                    item.dyeLevel = 2;
                }
                else if (item.TemplateIndex == (int)WomensClothing.Day_gown && item.CurrentVariant == 0)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.BasicClothing, DyeTargets.PurpleClothing, DyeTargets.BlackClothing };
                    item.dyeLevel = 3;
                }
                else if (item.TemplateIndex == (int)WomensClothing.Day_gown && item.CurrentVariant == 1)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.RedClothing, DyeTargets.PurpleClothing, DyeTargets.BlackClothing };
                    item.dyeLevel = 3;
                }
                else if (item.TemplateIndex == (int)WomensClothing.Casual_dress ||
                         item.TemplateIndex == (int)WomensClothing.Strapless_dress ||
                         item.TemplateIndex == (int)WomensClothing.Long_skirt ||
                         item.TemplateIndex == (int)WomensClothing.Short_shirt_unchangeable ||
                         item.TemplateIndex == (int)WomensClothing.Long_shirt_unchangeable)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.BasicClothing, DyeTargets.LightBrownClothing, DyeTargets.YellowClothing };
                    item.dyeLevel = 3;
                }
                else if (item.TemplateIndex == (int)WomensClothing.Short_shirt ||
                         item.TemplateIndex == (int)WomensClothing.Long_shirt ||
                         item.TemplateIndex == (int)WomensClothing.Short_shirt_closed ||
                         item.TemplateIndex == (int)WomensClothing.Long_shirt_closed)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.BasicClothing, DyeTargets.GreenClothing };
                    item.dyeLevel = 2;
                }
                else if (item.TemplateIndex == (int)WomensClothing.Short_shirt_belt ||
                         item.TemplateIndex == (int)WomensClothing.Long_shirt_belt ||
                         item.TemplateIndex == (int)WomensClothing.Short_shirt_closed_belt ||
                         item.TemplateIndex == (int)WomensClothing.Long_shirt_closed_belt)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.BasicClothing, DyeTargets.LightBrownClothing, DyeTargets.YellowClothing, DyeTargets.GreenClothing };
                    item.dyeLevel = 4;
                }
                else if (item.TemplateIndex == (int)WomensClothing.Short_shirt_sash ||
                         item.TemplateIndex == (int)WomensClothing.Long_shirt_sash ||
                         item.TemplateIndex == (int)WomensClothing.Short_shirt_closed_sash ||
                         item.TemplateIndex == (int)WomensClothing.Long_shirt_closed_sash)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.BasicClothing, DyeTargets.RedClothing, DyeTargets.GreenClothing };
                    item.dyeLevel = 3;
                }
                else if (item.TemplateIndex == (int)WomensClothing.Wrap)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.BasicClothing, DyeTargets.RedClothing };
                    item.dyeLevel = 2;
                }
                else if (item.TemplateIndex == (int)WomensClothing.Vest && item.CurrentVariant == 0)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.GreenClothing };
                    item.dyeLevel = 1;
                }
                else if (item.TemplateIndex == (int)WomensClothing.Vest && item.CurrentVariant == 1)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.RedClothing };
                    item.dyeLevel = 1;
                }
                else return;
            }
            else
            {
                if ((item.TemplateIndex == (int)MensClothing.Straps && item.CurrentVariant != 3) ||
                     item.TemplateIndex == (int)MensClothing.Challenger_Straps ||
                     item.TemplateIndex == (int)MensClothing.Champion_straps)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.LeatherClothing, DyeTargets.YellowClothing };
                    item.dyeLevel = 2;
                }
                else if (item.TemplateIndex == (int)MensClothing.Armbands ||
                         item.TemplateIndex == (int)MensClothing.Fancy_Armbands ||
                         item.TemplateIndex == (int)MensClothing.Eodoric)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.YellowClothing };
                    item.dyeLevel = 1;
                }
                else if (item.TemplateIndex == (int)MensClothing.Kimono)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.DarkBrownClothing, DyeTargets.LightBrownClothing, DyeTargets.YellowClothing };
                    item.dyeLevel = 3;
                }
                else if (item.TemplateIndex == (int)MensClothing.Sash ||
                         item.TemplateIndex == (int)MensClothing.Sandals)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.RedClothing };
                    item.dyeLevel = 1;
                }
                else if (item.TemplateIndex == (int)MensClothing.Shoes ||
                         item.TemplateIndex == (int)MensClothing.Tall_Boots ||
                         item.TemplateIndex == (int)MensClothing.Boots ||
                        (item.TemplateIndex == (int)MensClothing.Casual_pants && item.CurrentVariant != 3) ||
                        (item.TemplateIndex == (int)MensClothing.Breeches && item.CurrentVariant == 0))
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.LeatherClothing };
                    item.dyeLevel = 1;
                }
                else if ((item.TemplateIndex == (int)MensClothing.Casual_pants && item.CurrentVariant == 3) ||
                         (item.TemplateIndex == (int)MensClothing.Breeches && item.CurrentVariant == 1))
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.LightBrownClothing };
                    item.dyeLevel = 1;
                }
                else if (item.TemplateIndex == (int)MensClothing.Short_skirt ||
                         item.TemplateIndex == (int)MensClothing.Casual_cloak ||
                         item.TemplateIndex == (int)MensClothing.Loincloth ||
                         item.TemplateIndex == (int)MensClothing.Plain_robes ||
                         item.TemplateIndex == (int)MensClothing.Priest_robes ||
                         item.TemplateIndex == (int)MensClothing.Open_Tunic)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.BasicClothing };
                    item.dyeLevel = 1;
                }
                else if (item.TemplateIndex == (int)MensClothing.Formal_cloak ||
                         item.TemplateIndex == (int)MensClothing.Dwynnen_surcoat)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.BasicClothing, DyeTargets.LightBrownClothing };
                    item.dyeLevel = 2;
                }
                else if (item.TemplateIndex == (int)MensClothing.Khajiit_suit)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.BlackClothing };
                    item.dyeLevel = 1;
                }
                else if (item.TemplateIndex == (int)MensClothing.Short_tunic ||
                         item.TemplateIndex == (int)MensClothing.Short_tunic_fit ||
                         item.TemplateIndex == (int)MensClothing.Short_shirt ||
                         item.TemplateIndex == (int)MensClothing.Long_shirt ||
                         item.TemplateIndex == (int)MensClothing.Short_shirt_closed_top ||
                         item.TemplateIndex == (int)MensClothing.Long_shirt_closed_top)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.BasicClothing, DyeTargets.GreenClothing };
                    item.dyeLevel = 2;
                }
                else if (item.TemplateIndex == (int)MensClothing.Reversible_tunic)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.BasicClothing, DyeTargets.YellowClothing };
                    item.dyeLevel = 2;
                }
                else if (item.TemplateIndex == (int)MensClothing.Toga ||
                         item.TemplateIndex == (int)MensClothing.Wrap)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.BasicClothing, DyeTargets.RedClothing };
                    item.dyeLevel = 2;
                }
                else if (item.TemplateIndex == (int)MensClothing.Formal_tunic)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.YellowClothing, DyeTargets.BasicClothing, DyeTargets.RedClothing };
                    item.dyeLevel = 3;
                }
                else if (item.TemplateIndex == (int)MensClothing.Short_shirt_with_belt ||
                         item.TemplateIndex == (int)MensClothing.Long_shirt_with_belt ||
                         item.TemplateIndex == (int)MensClothing.Short_shirt_closed_top2 ||
                         item.TemplateIndex == (int)MensClothing.Long_shirt_closed_top2)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.BasicClothing, DyeTargets.LightBrownClothing, DyeTargets.YellowClothing, DyeTargets.GreenClothing };
                    item.dyeLevel = 4;
                }
                else if (item.TemplateIndex == (int)MensClothing.Short_shirt_with_sash ||
                         item.TemplateIndex == (int)MensClothing.Long_shirt_with_sash ||
                         item.TemplateIndex == (int)MensClothing.Short_shirt_closed_top3 ||
                         item.TemplateIndex == (int)MensClothing.Long_shirt_closed_top3)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.BasicClothing, DyeTargets.RedClothing, DyeTargets.GreenClothing };
                    item.dyeLevel = 3;
                }
                else if (item.TemplateIndex == (int)MensClothing.Long_Skirt ||
                         item.TemplateIndex == (int)MensClothing.Short_shirt_unchangeable ||
                         item.TemplateIndex == (int)MensClothing.Long_shirt_unchangeable)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.BasicClothing, DyeTargets.LightBrownClothing, DyeTargets.YellowClothing };
                    item.dyeLevel = 3;
                }
                else if (item.TemplateIndex == (int)MensClothing.Anticlere_Surcoat)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.GreenClothing, DyeTargets.BasicClothing, DyeTargets.LightBrownClothing, DyeTargets.YellowClothing };
                    item.dyeLevel = 4;
                }
                else if (item.TemplateIndex == (int)MensClothing.Vest)
                {
                    item.dyeTargets = new DyeTargets[] { DyeTargets.AdamantiumClothing, DyeTargets.YellowClothing };
                    item.dyeLevel = 2;
                }
            }
        }

        public static void SetClothColors(DaggerfallUnityItem item, ClothCraftsmanship clothCraftsmanship)
        {
            if (item.TemplateIndex == (int)WomensClothing.Brassier ||
               (item.TemplateIndex == (int)WomensClothing.Formal_brassier && item.CurrentVariant == 2) ||
                item.TemplateIndex == (int)WomensClothing.Casual_cloak ||
                item.TemplateIndex == (int)WomensClothing.Loincloth ||
                item.TemplateIndex == (int)WomensClothing.Vest ||
                item.TemplateIndex == (int)MensClothing.Sash ||
                item.TemplateIndex == (int)MensClothing.Short_skirt ||
                item.TemplateIndex == (int)MensClothing.Casual_cloak ||
                item.TemplateIndex == (int)MensClothing.Loincloth)
            {
                switch (clothCraftsmanship)
                {
                    case ClothCraftsmanship.Cheap:
                        item.dyeColor = RandomCheapClothingDye();
                        break;
                    case ClothCraftsmanship.Normal:
                        item.dyeColor = RandomWorkClothingDye();
                        break;
                    case ClothCraftsmanship.Fancy:
                        item.dyeColor = RandomFancyClothingDye();
                        break;
                    case ClothCraftsmanship.Extravagant:
                    case ClothCraftsmanship.Exquisite:
                        item.dyeColor = RandomMetalDye();
                        break;
                }
            }
            else if (item.TemplateIndex == (int)WomensClothing.Formal_brassier &&
                    (item.CurrentVariant == 0 || item.CurrentVariant == 3))
            {
                switch (clothCraftsmanship)
                {
                    case ClothCraftsmanship.Cheap:
                        item.dyeColor = RandomCheapClothingDye();
                        item.additionalColors[0] = RandomWorkClothingDye();
                        break;
                    case ClothCraftsmanship.Normal:
                        item.dyeColor = RandomWorkClothingDye();
                        item.additionalColors[0] = RandomClothingDye();
                        break;
                    case ClothCraftsmanship.Fancy:
                        item.dyeColor = RandomFancyClothingDye();
                        item.additionalColors[0] = RandomClothingDye();
                        break;
                    case ClothCraftsmanship.Extravagant:
                        item.dyeColor = RandomMetalDye();
                        item.additionalColors[0] = RandomClothingDye();
                        break;
                    case ClothCraftsmanship.Exquisite:
                        item.dyeColor = RandomMetalDye();
                        item.additionalColors[0] = RandomMetalDye();
                        break;
                }
            }
            else if (item.TemplateIndex == (int)WomensClothing.Formal_brassier && item.CurrentVariant == 1)
            {
                switch (clothCraftsmanship)
                {
                    case ClothCraftsmanship.Cheap:
                        item.dyeColor = DyeColors.Iron;
                        break;
                    case ClothCraftsmanship.Normal:
                        item.dyeColor = DyeColors.Steel;
                        break;
                    case ClothCraftsmanship.Fancy:
                        item.dyeColor = RandomClothingDye();
                        break;
                    case ClothCraftsmanship.Extravagant:
                    case ClothCraftsmanship.Exquisite:
                        item.dyeColor = RandomMetalDye();
                        break;
                }
            }
            else if (item.TemplateIndex == (int)WomensClothing.Peasant_blouse ||
                     item.TemplateIndex == (int)WomensClothing.Khajiit_suit ||
                    (item.TemplateIndex == (int)WomensClothing.Evening_gown && item.CurrentVariant == 0) ||
                     item.TemplateIndex == (int)WomensClothing.Plain_robes ||
                     item.TemplateIndex == (int)WomensClothing.Priestess_robes ||
                     item.TemplateIndex == (int)WomensClothing.Open_tunic ||
                    (item.TemplateIndex == (int)WomensClothing.Tights && item.CurrentVariant == 0) ||
                     item.TemplateIndex == (int)MensClothing.Khajiit_suit ||
                     item.TemplateIndex == (int)MensClothing.Plain_robes ||
                     item.TemplateIndex == (int)MensClothing.Priest_robes ||
                     item.TemplateIndex == (int)MensClothing.Open_Tunic)
            {
                switch (clothCraftsmanship)
                {
                    case ClothCraftsmanship.Cheap:
                    case ClothCraftsmanship.Normal:
                        item.dyeColor = RandomCheapClothingDye();
                        break;
                    case ClothCraftsmanship.Fancy:
                        item.dyeColor = RandomClothingDye();
                        break;
                    case ClothCraftsmanship.Extravagant:
                        item.dyeColor = RandomFancyClothingDye();
                        break;
                    case ClothCraftsmanship.Exquisite:
                        item.dyeColor = RandomMetalDye();
                        break;
                }
            }
            else if (item.TemplateIndex == (int)WomensClothing.Eodoric ||
                    (item.TemplateIndex == (int)WomensClothing.Formal_eodoric && (item.CurrentVariant == 1 || item.CurrentVariant == 2)) ||
                     item.TemplateIndex == (int)MensClothing.Armbands ||
                     item.TemplateIndex == (int)MensClothing.Fancy_Armbands ||
                     item.TemplateIndex == (int)MensClothing.Eodoric)
            {
                switch (clothCraftsmanship)
                {
                    case ClothCraftsmanship.Cheap:
                        item.dyeColor = RandomClothingDye();
                        break;
                    case ClothCraftsmanship.Normal:
                        item.dyeColor = DyeColors.Yellow;
                        break;
                    case ClothCraftsmanship.Fancy:
                    case ClothCraftsmanship.Extravagant:
                    case ClothCraftsmanship.Exquisite:
                        item.dyeColor = RandomMetalDye();
                        break;
                }
            }
            else if (item.TemplateIndex == (int)WomensClothing.Shoes ||
                     item.TemplateIndex == (int)WomensClothing.Tall_boots ||
                     item.TemplateIndex == (int)WomensClothing.Boots ||
                     item.TemplateIndex == (int)WomensClothing.Sandals ||
                     item.TemplateIndex == (int)MensClothing.Shoes ||
                     item.TemplateIndex == (int)MensClothing.Tall_Boots ||
                     item.TemplateIndex == (int)MensClothing.Boots ||
                     item.TemplateIndex == (int)MensClothing.Sandals)
            {
                switch (clothCraftsmanship)
                {
                    case ClothCraftsmanship.Cheap:
                        item.dyeColor = DyeColors.LightBrown;
                        break;
                    case ClothCraftsmanship.Normal:
                        item.dyeColor = DyeColors.Leather;
                        break;
                    case ClothCraftsmanship.Fancy:
                        item.dyeColor = DyeColors.DarkBrown;
                        break;
                    case ClothCraftsmanship.Extravagant:
                        item.dyeColor = RandomClothingDye();
                        break;
                    case ClothCraftsmanship.Exquisite:
                        item.dyeColor = RandomMetalDye();
                        break;
                }
            }
            else if (item.TemplateIndex == (int)WomensClothing.Casual_pants ||
                    (item.TemplateIndex == (int)WomensClothing.Tights && item.CurrentVariant == 1) ||
                     item.TemplateIndex == (int)MensClothing.Casual_pants)
            {
                switch (clothCraftsmanship)
                {
                    case ClothCraftsmanship.Cheap:
                        item.dyeColor = DyeColors.LightBrown;
                        break;
                    case ClothCraftsmanship.Normal:
                        item.dyeColor = DyeColors.Leather;
                        break;
                    case ClothCraftsmanship.Fancy:
                        item.dyeColor = RandomCheapClothingDye();
                        break;
                    case ClothCraftsmanship.Extravagant:
                        item.dyeColor = RandomFancyClothingDye();
                        break;
                    case ClothCraftsmanship.Exquisite:
                        item.dyeColor = RandomMetalDye();
                        break;
                }
            }
            else if (item.TemplateIndex == (int)WomensClothing.Formal_cloak ||
                     item.TemplateIndex == (int)MensClothing.Formal_cloak ||
                     item.TemplateIndex == (int)MensClothing.Dwynnen_surcoat)
            {
                switch (clothCraftsmanship)
                {
                    case ClothCraftsmanship.Cheap:
                        item.dyeColor = RandomCheapClothingDye();
                        item.additionalColors[0] = DyeColors.LightBrown;
                        break;
                    case ClothCraftsmanship.Normal:
                        item.dyeColor = RandomCheapClothingDye();
                        item.additionalColors[0] = RandomCheapClothingDye();
                        break;
                    case ClothCraftsmanship.Fancy:
                        item.dyeColor = RandomFancyClothingDye();
                        item.additionalColors[0] = RandomCheapClothingDye();
                        break;
                    case ClothCraftsmanship.Extravagant:
                        item.dyeColor = RandomFancyClothingDye();
                        item.additionalColors[0] = RandomFancyClothingDye();
                        break;
                    case ClothCraftsmanship.Exquisite:
                        item.dyeColor = RandomMetalDye();
                        item.additionalColors[0] = RandomClothingDye();
                        break;
                }
            }
            else if (item.TemplateIndex == (int)WomensClothing.Formal_eodoric && item.CurrentVariant == 0)
            {
                switch (clothCraftsmanship)
                {
                    case ClothCraftsmanship.Cheap:
                        item.dyeColor = RandomClothingDye();
                        item.additionalColors[0] = RandomCheapClothingDye();
                        break;
                    case ClothCraftsmanship.Normal:
                        item.dyeColor = DyeColors.Yellow;
                        item.additionalColors[0] = RandomCheapClothingDye();
                        break;
                    case ClothCraftsmanship.Fancy:
                        item.dyeColor = DyeColors.Yellow;
                        item.additionalColors[0] = RandomFancyClothingDye();
                        break;
                    case ClothCraftsmanship.Extravagant:
                        item.dyeColor = DyeColors.Yellow;
                        item.additionalColors[0] = RandomMetalDye();
                        break;
                    case ClothCraftsmanship.Exquisite:
                        item.dyeColor = RandomMetalDye();
                        item.additionalColors[0] = RandomMetalDye();
                        break;
                }
            }
            else if (item.TemplateIndex == (int)WomensClothing.Evening_gown && item.CurrentVariant == 1)
            {
                switch (clothCraftsmanship)
                {
                    case ClothCraftsmanship.Cheap:
                        item.dyeColor = RandomCheapClothingDye();
                        item.additionalColors[0] = DyeColors.LightBrown;
                        break;
                    case ClothCraftsmanship.Normal:
                        item.dyeColor = RandomCheapClothingDye();
                        item.additionalColors[0] = RandomClothingDye();
                        break;
                    case ClothCraftsmanship.Fancy:
                        item.dyeColor = RandomFancyClothingDye();
                        item.additionalColors[0] = RandomClothingDye();
                        break;
                    case ClothCraftsmanship.Extravagant:
                        item.dyeColor = RandomFancyClothingDye();
                        item.additionalColors[0] = RandomMetalDye();
                        break;
                    case ClothCraftsmanship.Exquisite:
                        item.dyeColor = RandomMetalDye();
                        item.additionalColors[0] = RandomClothingDye();
                        break;
                }
            }
            else if (item.TemplateIndex == (int)WomensClothing.Day_gown)
            {
                switch (clothCraftsmanship)
                {
                    case ClothCraftsmanship.Cheap:
                        item.dyeColor = RandomCheapClothingDye();
                        item.additionalColors[0] = RandomCheapClothingDye();
                        item.additionalColors[1] = RandomCheapClothingDye();
                        break;
                    case ClothCraftsmanship.Normal:
                        item.dyeColor = RandomClothingDye();
                        item.additionalColors[0] = RandomCheapClothingDye();
                        item.additionalColors[1] = DyeColors.DarkGrey;
                        break;
                    case ClothCraftsmanship.Fancy:
                        item.dyeColor = RandomClothingDye();
                        item.additionalColors[0] = RandomFancyClothingDye();
                        item.additionalColors[1] = DyeColors.DarkGrey;
                        break;
                    case ClothCraftsmanship.Extravagant:
                        item.dyeColor = RandomFancyClothingDye();
                        item.additionalColors[0] = RandomFancyClothingDye();
                        item.additionalColors[1] = RandomFancyClothingDye();
                        break;
                    case ClothCraftsmanship.Exquisite:
                        item.dyeColor = RandomMetalDye();
                        item.additionalColors[0] = RandomFancyClothingDye();
                        item.additionalColors[1] = RandomClothingDye();
                        break;
                }
            }
            else if (item.TemplateIndex == (int)WomensClothing.Casual_dress ||
                     item.TemplateIndex == (int)WomensClothing.Strapless_dress ||
                     item.TemplateIndex == (int)WomensClothing.Long_skirt ||
                     item.TemplateIndex == (int)WomensClothing.Short_shirt_unchangeable ||
                     item.TemplateIndex == (int)WomensClothing.Long_shirt_unchangeable ||
                     item.TemplateIndex == (int)MensClothing.Long_Skirt ||
                     item.TemplateIndex == (int)MensClothing.Short_shirt_unchangeable ||
                     item.TemplateIndex == (int)MensClothing.Long_shirt_unchangeable)
            {
                switch (clothCraftsmanship)
                {
                    case ClothCraftsmanship.Cheap:
                        item.dyeColor = RandomCheapClothingDye();
                        item.additionalColors[0] = DyeColors.LightBrown;
                        item.additionalColors[1] = DyeColors.Iron;
                        break;
                    case ClothCraftsmanship.Normal:
                        item.dyeColor = RandomCheapClothingDye();
                        item.additionalColors[0] = RandomClothingDye();
                        item.additionalColors[1] = DyeColors.Yellow;
                        break;
                    case ClothCraftsmanship.Fancy:
                        item.dyeColor = RandomFancyClothingDye();
                        item.additionalColors[0] = RandomClothingDye();
                        item.additionalColors[1] = RandomMetalDye();
                        break;
                    case ClothCraftsmanship.Extravagant:
                        item.dyeColor = RandomFancyClothingDye();
                        item.additionalColors[0] = RandomMetalDye();
                        item.additionalColors[1] = RandomClothingDye();
                        break;
                    case ClothCraftsmanship.Exquisite:
                        item.dyeColor = RandomMetalDye();
                        item.additionalColors[0] = RandomClothingDye();
                        item.additionalColors[1] = RandomMetalDye();
                        break;
                }
            }
            else if (item.TemplateIndex == (int)WomensClothing.Short_shirt ||
                     item.TemplateIndex == (int)WomensClothing.Long_shirt ||
                     item.TemplateIndex == (int)WomensClothing.Short_shirt_closed ||
                     item.TemplateIndex == (int)WomensClothing.Long_shirt_closed ||
                     item.TemplateIndex == (int)WomensClothing.Wrap ||
                     item.TemplateIndex == (int)MensClothing.Short_tunic ||
                     item.TemplateIndex == (int)MensClothing.Short_tunic_fit ||
                     item.TemplateIndex == (int)MensClothing.Toga ||
                     item.TemplateIndex == (int)MensClothing.Short_shirt ||
                     item.TemplateIndex == (int)MensClothing.Short_shirt_closed_top ||
                     item.TemplateIndex == (int)MensClothing.Long_shirt_closed_top ||
                     item.TemplateIndex == (int)MensClothing.Wrap)
            {
                switch (clothCraftsmanship)
                {
                    case ClothCraftsmanship.Cheap:
                        item.dyeColor = RandomCheapClothingDye();
                        item.additionalColors[0] = RandomCheapClothingDye();
                        break;
                    case ClothCraftsmanship.Normal:
                        item.dyeColor = RandomCheapClothingDye();
                        item.additionalColors[0] = RandomClothingDye();
                        break;
                    case ClothCraftsmanship.Fancy:
                        item.dyeColor = RandomFancyClothingDye();
                        item.additionalColors[0] = RandomClothingDye();
                        break;
                    case ClothCraftsmanship.Extravagant:
                        item.dyeColor = RandomFancyClothingDye();
                        item.additionalColors[0] = RandomFancyClothingDye();
                        break;
                    case ClothCraftsmanship.Exquisite:
                        item.dyeColor = RandomMetalDye();
                        item.additionalColors[0] = RandomMetalDye();
                        break;
                }
            }
            else if (item.TemplateIndex == (int)WomensClothing.Short_shirt_belt ||
                     item.TemplateIndex == (int)WomensClothing.Long_shirt_belt ||
                     item.TemplateIndex == (int)WomensClothing.Short_shirt_closed_belt ||
                     item.TemplateIndex == (int)WomensClothing.Long_shirt_closed_belt ||
                     item.TemplateIndex == (int)MensClothing.Short_shirt_with_belt ||
                     item.TemplateIndex == (int)MensClothing.Short_shirt_closed_top2 ||
                     item.TemplateIndex == (int)MensClothing.Long_shirt_closed_top2)
            {
                switch (clothCraftsmanship)
                {
                    case ClothCraftsmanship.Cheap:
                        item.dyeColor = RandomCheapClothingDye();
                        item.additionalColors[0] = DyeColors.LightBrown;
                        item.additionalColors[1] = DyeColors.Iron;
                        item.additionalColors[2] = RandomCheapClothingDye();
                        break;
                    case ClothCraftsmanship.Normal:
                        item.dyeColor = RandomCheapClothingDye();
                        item.additionalColors[0] = RandomClothingDye();
                        item.additionalColors[1] = DyeColors.Yellow;
                        item.additionalColors[2] = RandomClothingDye();
                        break;
                    case ClothCraftsmanship.Fancy:
                        item.dyeColor = RandomFancyClothingDye();
                        item.additionalColors[0] = RandomClothingDye();
                        item.additionalColors[1] = RandomMetalDye();
                        item.additionalColors[2] = RandomClothingDye();
                        break;
                    case ClothCraftsmanship.Extravagant:
                        item.dyeColor = RandomFancyClothingDye();
                        item.additionalColors[0] = RandomMetalDye();
                        item.additionalColors[1] = RandomClothingDye();
                        item.additionalColors[2] = RandomFancyClothingDye();
                        break;
                    case ClothCraftsmanship.Exquisite:
                        item.dyeColor = RandomMetalDye();
                        item.additionalColors[0] = RandomClothingDye();
                        item.additionalColors[1] = RandomMetalDye();
                        item.additionalColors[2] = RandomMetalDye();
                        break;
                }
            }
            else if (item.TemplateIndex == (int)WomensClothing.Short_shirt_sash ||
                     item.TemplateIndex == (int)WomensClothing.Long_shirt_sash ||
                     item.TemplateIndex == (int)WomensClothing.Short_shirt_closed_sash ||
                     item.TemplateIndex == (int)WomensClothing.Long_shirt_closed_sash ||
                     item.TemplateIndex == (int)MensClothing.Short_shirt_with_sash ||
                     item.TemplateIndex == (int)MensClothing.Short_shirt_closed_top3 ||
                     item.TemplateIndex == (int)MensClothing.Long_shirt_closed_top3)
            {
                switch (clothCraftsmanship)
                {
                    case ClothCraftsmanship.Cheap:
                        item.dyeColor = RandomCheapClothingDye();
                        item.additionalColors[0] = DyeColors.LightBrown;
                        item.additionalColors[1] = RandomCheapClothingDye();
                        break;
                    case ClothCraftsmanship.Normal:
                        item.dyeColor = RandomCheapClothingDye();
                        item.additionalColors[0] = RandomClothingDye();
                        item.additionalColors[1] = RandomClothingDye();
                        break;
                    case ClothCraftsmanship.Fancy:
                        item.dyeColor = RandomFancyClothingDye();
                        item.additionalColors[0] = RandomClothingDye();
                        item.additionalColors[1] = RandomClothingDye();
                        break;
                    case ClothCraftsmanship.Extravagant:
                        item.dyeColor = RandomFancyClothingDye();
                        item.additionalColors[0] = RandomMetalDye();
                        item.additionalColors[1] = RandomFancyClothingDye();
                        break;
                    case ClothCraftsmanship.Exquisite:
                        item.dyeColor = RandomMetalDye();
                        item.additionalColors[0] = RandomClothingDye();
                        item.additionalColors[1] = RandomMetalDye();
                        break;
                }
            }
            else if ((item.TemplateIndex == (int)MensClothing.Straps && item.CurrentVariant < 3) ||
                      item.TemplateIndex == (int)MensClothing.Champion_straps ||
                      item.TemplateIndex == (int)MensClothing.Challenger_Straps)
            {
                switch (clothCraftsmanship)
                {
                    case ClothCraftsmanship.Cheap:
                        item.dyeColor = DyeColors.LightBrown;
                        item.additionalColors[0] = DyeColors.Iron;
                        break;
                    case ClothCraftsmanship.Normal:
                        item.dyeColor = DyeColors.Leather;
                        item.additionalColors[0] = DyeColors.Yellow;
                        break;
                    case ClothCraftsmanship.Fancy:
                        item.dyeColor = RandomCheapClothingDye();
                        item.additionalColors[0] = RandomMetalDye();
                        break;
                    case ClothCraftsmanship.Extravagant:
                        item.dyeColor = RandomFancyClothingDye();
                        item.additionalColors[0] = RandomMetalDye();
                        break;
                    case ClothCraftsmanship.Exquisite:
                        item.dyeColor = RandomMetalDye();
                        item.additionalColors[0] = RandomClothingDye();
                        break;
                }
            }
            else if ((item.TemplateIndex == (int)MensClothing.Straps && item.CurrentVariant == 3) ||
                      item.TemplateIndex == (int)MensClothing.Vest)
            {
                switch (clothCraftsmanship)
                {
                    case ClothCraftsmanship.Cheap:
                        item.dyeColor = RandomCheapClothingDye();
                        item.additionalColors[0] = DyeColors.Iron;
                        break;
                    case ClothCraftsmanship.Normal:
                        item.dyeColor = DyeColors.DarkGrey;
                        item.additionalColors[0] = DyeColors.Yellow;
                        break;
                    case ClothCraftsmanship.Fancy:
                        item.dyeColor = RandomClothingDye();
                        item.additionalColors[0] = RandomMetalDye();
                        break;
                    case ClothCraftsmanship.Extravagant:
                        item.dyeColor = RandomFancyClothingDye();
                        item.additionalColors[0] = RandomMetalDye();
                        break;
                    case ClothCraftsmanship.Exquisite:
                        item.dyeColor = RandomMetalDye();
                        item.additionalColors[0] = RandomClothingDye();
                        break;
                }
            }
            else if (item.TemplateIndex == (int)MensClothing.Kimono)
            {
                switch (clothCraftsmanship)
                {
                    case ClothCraftsmanship.Cheap:
                        item.dyeColor = RandomCheapClothingDye();
                        item.additionalColors[0] = DyeColors.LightBrown;
                        item.additionalColors[1] = DyeColors.Iron;
                        break;
                    case ClothCraftsmanship.Normal:
                        item.dyeColor = RandomWorkClothingDye();
                        item.additionalColors[0] = RandomCheapClothingDye();
                        item.additionalColors[1] = DyeColors.Yellow;
                        break;
                    case ClothCraftsmanship.Fancy:
                        item.dyeColor = RandomFancyClothingDye();
                        item.additionalColors[0] = RandomClothingDye();
                        item.additionalColors[1] = RandomMetalDye();
                        break;
                    case ClothCraftsmanship.Extravagant:
                        item.dyeColor = RandomClothingDye();
                        item.additionalColors[0] = RandomFancyClothingDye();
                        item.additionalColors[1] = RandomMetalDye();
                        break;
                    case ClothCraftsmanship.Exquisite:
                        item.dyeColor = RandomMetalDye();
                        item.additionalColors[0] = RandomMetalDye();
                        item.additionalColors[1] = RandomClothingDye();
                        break;
                }
            }
            else if (item.TemplateIndex == (int)MensClothing.Reversible_tunic)
            {
                switch (clothCraftsmanship)
                {
                    case ClothCraftsmanship.Cheap:
                        item.dyeColor = RandomCheapClothingDye();
                        item.additionalColors[0] = RandomWorkClothingDye();
                        break;
                    case ClothCraftsmanship.Normal:
                        item.dyeColor = RandomWorkClothingDye();
                        item.additionalColors[0] = RandomFancyClothingDye();
                        break;
                    case ClothCraftsmanship.Fancy:
                        item.dyeColor = RandomFancyClothingDye();
                        item.additionalColors[0] = RandomMetalDye();
                        break;
                    case ClothCraftsmanship.Extravagant:
                        item.dyeColor = RandomMetalDye();
                        item.additionalColors[0] = RandomFancyClothingDye();
                        break;
                    case ClothCraftsmanship.Exquisite:
                        item.dyeColor = RandomMetalDye();
                        item.additionalColors[0] = RandomMetalDye();
                        break;
                }
            }
            else if (item.TemplateIndex == (int)MensClothing.Formal_tunic)
            {
                switch (clothCraftsmanship)
                {
                    case ClothCraftsmanship.Cheap:
                        item.dyeColor = RandomWorkClothingDye();
                        item.additionalColors[0] = RandomCheapClothingDye();
                        item.additionalColors[1] = RandomClothingDye();
                        break;
                    case ClothCraftsmanship.Normal:
                        item.dyeColor = RandomClothingDye();
                        item.additionalColors[0] = RandomWorkClothingDye();
                        item.additionalColors[1] = RandomClothingDye();
                        break;
                    case ClothCraftsmanship.Fancy:
                        item.dyeColor = RandomFancyClothingDye();
                        item.additionalColors[0] = RandomClothingDye();
                        item.additionalColors[1] = RandomMetalDye();
                        break;
                    case ClothCraftsmanship.Extravagant:
                        item.dyeColor = RandomFancyClothingDye();
                        item.additionalColors[0] = RandomFancyClothingDye();
                        item.additionalColors[1] = RandomMetalDye();
                        break;
                    case ClothCraftsmanship.Exquisite:
                        item.dyeColor = RandomMetalDye();
                        item.additionalColors[0] = RandomMetalDye();
                        item.additionalColors[1] = RandomClothingDye();
                        break;
                }
            }
            else if (item.TemplateIndex == (int)MensClothing.Anticlere_Surcoat)
            {
                switch (clothCraftsmanship)
                {
                    case ClothCraftsmanship.Cheap:
                        item.dyeColor = RandomCheapClothingDye();
                        item.additionalColors[0] = RandomWorkClothingDye();
                        item.additionalColors[1] = DyeColors.LightBrown;
                        item.additionalColors[2] = DyeColors.Iron;
                        break;
                    case ClothCraftsmanship.Normal:
                        item.dyeColor = RandomWorkClothingDye();
                        item.additionalColors[0] = RandomCheapClothingDye();
                        item.additionalColors[1] = RandomClothingDye();
                        item.additionalColors[2] = DyeColors.Yellow;
                        break;
                    case ClothCraftsmanship.Fancy:
                        item.dyeColor = RandomFancyClothingDye();
                        item.additionalColors[0] = RandomClothingDye();
                        item.additionalColors[1] = RandomClothingDye();
                        item.additionalColors[2] = RandomMetalDye();
                        break;
                    case ClothCraftsmanship.Extravagant:
                        item.dyeColor = RandomFancyClothingDye();
                        item.additionalColors[0] = RandomMetalDye();
                        item.additionalColors[1] = RandomFancyClothingDye();
                        item.additionalColors[2] = RandomClothingDye();
                        break;
                    case ClothCraftsmanship.Exquisite:
                        item.dyeColor = RandomMetalDye();
                        item.additionalColors[0] = RandomMetalDye();
                        item.additionalColors[1] = RandomClothingDye();
                        item.additionalColors[2] = RandomMetalDye();
                        break;
                }
            }
            else item.dyeColor = RandomClothingDye();
        }

        public static void SetVariant(DaggerfallUnityItem item, int variant)
        {
            // Range check
            int totalVariants = item.ItemTemplate.variants;
            if (variant < 0 || variant >= totalVariants)
                return;

            // Clamp to appropriate variant based on material family
            if (item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Cuirass))
            {
                if (item.nativeMaterialValue == (int)ArmorMaterialTypes.Leather)
                    variant = 0;
                else if (item.nativeMaterialValue == (int)ArmorMaterialTypes.Chain)
                    variant = 4;
                else
                    variant = Mathf.Clamp(variant, 1, 3);
            }
            else if (item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Hauberk))
            {
                variant = 0;
            }
            else if (item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Jerkin))
            {
                if (item.nativeMaterialValue == (int)ArmorMaterialTypes.Fur)
                    variant = 12;
                else
                    variant = (int)GetArmorMaterialType(item.nativeMaterialValue);
            }
            else if (item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Greaves))
            {
                if (item.nativeMaterialValue == (int)ArmorMaterialTypes.Leather)
                    variant = Mathf.Clamp(variant, 0, 1);
                else if (item.nativeMaterialValue == (int)ArmorMaterialTypes.Chain)
                    variant = 6;
                else
                    variant = Mathf.Clamp(variant, 2, 5);
            }
            else if (item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Chausses))
            {
                variant = 0;
            }
            else if (item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Cuisse))
            {
                if (item.nativeMaterialValue == (int)ArmorMaterialTypes.Fur)
                    variant = 12;
                else
                    variant = (int)GetArmorMaterialType(item.nativeMaterialValue);
            }
            else if (item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Left_Pauldron) || item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Right_Pauldron))
            {
                if (item.nativeMaterialValue == (int)ArmorMaterialTypes.Leather)
                    variant = 0;
                else if (item.nativeMaterialValue == (int)ArmorMaterialTypes.Chain)
                    variant = 4;
                else
                    variant = Mathf.Clamp(variant, 1, 3);
            }
            else if (item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Left_Spaulder) || item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Right_Spaulder))
            {
                variant = 0;
            }
            else if (item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Left_Vambrace) || item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Right_Vambrace))
            {
                if (item.nativeMaterialValue == (int)ArmorMaterialTypes.Fur)
                    variant = 12;
                else
                    variant = (int)GetArmorMaterialType(item.nativeMaterialValue);
            }
            else if (item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Gauntlets))
            {
                if (item.nativeMaterialValue == (int)ArmorMaterialTypes.Leather)
                    variant = 0;
                else
                    variant = 1;
            }
            else if (item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Gloves))
            {
               if (item.nativeMaterialValue == (int)ArmorMaterialTypes.Fur)
                    variant = 12;
                else
                    variant = (int)GetArmorMaterialType(item.nativeMaterialValue);
            }
            else if (item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Boots))
            {
                if (item.nativeMaterialValue == (int)ArmorMaterialTypes.Leather)
                    variant = 0;
                else
                    variant = Mathf.Clamp(variant, 1, 2);
            }
            else if (item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Sollerets))
            {
                variant = 0;
            }
            else if (item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Light_Boots))
            {
                if (item.nativeMaterialValue == (int)ArmorMaterialTypes.Fur)
                    variant = 12;
                else
                    variant = (int)GetArmorMaterialType(item.nativeMaterialValue);
            }
            else if (item.IsOfTemplate(ItemGroups.Armor, (int)Armor.Helmet))
            {
                if (item.nativeMaterialValue == (int)ArmorMaterialTypes.Fur)
                    variant = 12;
                else
                    variant = (int)GetArmorMaterialType(item.nativeMaterialValue);
            }
            // Women's clothing variants
            else if (item.ItemGroup == ItemGroups.WomensClothing ||
                     item.ItemGroup == ItemGroups.MensClothing)
            {
                SetClothDyeData(item);
                SetClothColors(item, item.craftsmanship);
            }
            else if (item.ItemGroup == ItemGroups.Weapons && variant != 0)
            {
                string prefix = GetExoticWeaponPrefix(item);
                item.shortName = prefix + " " + item.shortName;
                item.value *= 10;
                item.maxCondition = item.maxCondition * 15 / 10;
                item.enchantmentPoints = item.enchantmentPoints * 15 / 10;
            }

            // Store variant
            item.CurrentVariant = variant;
        }

        public static DaggerfallUnityItem GetItemCraftsmanshipStats(DaggerfallUnityItem item, ClothCraftsmanship craftsmanship)
        {
            switch (craftsmanship)
            {
                case ClothCraftsmanship.Normal:
                    item.value *= 2;
                    item.maxCondition *= 3;
                    item.enchantmentPoints *= 2;
                    return item;
                case ClothCraftsmanship.Fancy:
                    item.value *= 5;
                    item.maxCondition *= 2;
                    item.enchantmentPoints *= 5;
                    return item;
                case ClothCraftsmanship.Extravagant:
                    item.value *= 10;
                    item.maxCondition *= 2;
                    item.enchantmentPoints *= 7;
                    return item;
                case ClothCraftsmanship.Exquisite:
                    item.value *= 20;
                    item.maxCondition *= 3;
                    item.enchantmentPoints *= 10;
                    return item;
                case ClothCraftsmanship.Cheap:
                default:
                    return item;
            }
        }

        public static string GetExoticWeaponPrefix(DaggerfallUnityItem weapon)
        {
            if (weapon.nativeMaterialValue == (int)MaterialTypes.Daedric)
            {
                switch (weapon.TemplateIndex)
                {
                    case (int)Weapons.Dagger:
                        return "Deadlands'";
                    case (int)Weapons.Tanto:
                        return "Mirrormoor's";
                    case (int)Weapons.Staff:
                        return "Apocrypha's";
                    case (int)Weapons.Shortsword:
                        return "Shivering Isle's";
                    case (int)Weapons.Wakazashi:
                        return "Void's";
                    case (int)Weapons.Broadsword:
                        return "Pit's";
                    case (int)Weapons.Saber:
                        return "Moonshadow's";
                    case (int)Weapons.Longsword:
                        return "Auroran's";
                    case (int)Weapons.Katana:
                        return "Spiral Skein's";
                    case (int)Weapons.Claymore:
                        return "Attribution's";
                    case (int)Weapons.Dai_Katana:
                        return "Revelry's";
                    case (int)Weapons.Mace:
                        return "Ashpit's";
                    case (int)Weapons.Flail:
                        return "Quagmire's";
                    case (int)Weapons.Warhammer:
                        return "Coldharbour's";
                    case (int)Weapons.Battle_Axe:
                        return "Regret's";
                    case (int)Weapons.War_Axe:
                        return "Oblivion's";
                    case (int)Weapons.Short_Bow:
                        return "Hunting Ground's";
                    case (int)Weapons.Long_Bow:
                        return "Evergloam's";
                }
            }
            if (weapon.nativeMaterialValue == (int)MaterialTypes.Orcish)
            {
                switch (weapon.TemplateIndex)
                {
                    case (int)Weapons.Dagger:
                    case (int)Weapons.Tanto:
                    case (int)Weapons.Staff:
                    case (int)Weapons.Shortsword:
                    case (int)Weapons.Wakazashi:
                    case (int)Weapons.Longsword:
                    case (int)Weapons.Katana:
                    case (int)Weapons.Dai_Katana:
                        return "Orsinium's";
                    case (int)Weapons.Broadsword:
                    case (int)Weapons.Saber:
                    case (int)Weapons.Claymore:
                    case (int)Weapons.Mace:
                    case (int)Weapons.Flail:
                        return "Hollow Wastes'";
                    case (int)Weapons.Warhammer:
                    case (int)Weapons.Battle_Axe:
                    case (int)Weapons.War_Axe:
                        return "Valus'";
                    case (int)Weapons.Short_Bow:
                        return "Greenshade's";
                    case (int)Weapons.Long_Bow:
                        return "Grathwood's";
                }                    
            }
            if (weapon.nativeMaterialValue == (int)MaterialTypes.Iron)
            {
                if (weapon.TemplateIndex == (int)Weapons.Claymore ||
                    weapon.TemplateIndex == (int)Weapons.Warhammer ||
                    weapon.TemplateIndex == (int)Weapons.War_Axe)
                    return "Craglorn's";
            }

            string randomPrefix = WorldData.WorldSetting.RegionNames[RandomWeaponOrigin(weapon)];

            if (randomPrefix.EndsWith("s")) randomPrefix += "'";
            else randomPrefix += "'s";

            return randomPrefix;
        }

        public static BodyMorphology GetBodyMorphology(Races race)
        {
            switch (race)
            {
                case Races.Argonian:
                    return BodyMorphology.Argonian;

                case Races.DarkElf:
                case Races.HighElf:
                case Races.WoodElf:
                    return BodyMorphology.Elf;

                case Races.Breton:
                case Races.Nord:
                case Races.Redguard:
                case Races.Imperial:
                case Races.Orc:
                    return BodyMorphology.Human;

                case Races.Khajiit:
                    return BodyMorphology.Khajiit;

                default:
                    throw new Exception("GetBodyMorphology() encountered unsupported race value.");
            }
        }

        public static ArmorTypes GetArmorType(int nativeMaterialValue)
        {
            return (ArmorTypes)(nativeMaterialValue / 100 * 100);
        }

        public static MaterialTypes GetArmorMaterialType(int nativeMaterialValue)
        {
            if (nativeMaterialValue == 0)
                return MaterialTypes.Leather;
            Debug.Log(nativeMaterialValue + " (nativeMaterialValue) % 0x0010 = " + (nativeMaterialValue % 0x0010));
            return (MaterialTypes)(nativeMaterialValue % 0x0010);
        }

        #endregion
    }
}