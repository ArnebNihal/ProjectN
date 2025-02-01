// Project:         Climates & Calories mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2020 Ralzar
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Ralzar

using DaggerfallConnect;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Utility;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.Utility.ModSupport;
using DaggerfallWorkshop.Game.Utility.ModSupport.ModSettings;
using UnityEngine;
using System;
using DaggerfallWorkshop;
using DaggerfallConnect.Arena2;
using DaggerfallWorkshop.Utility;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using DaggerfallWorkshop.Game.Serialization;
using System.Collections.Generic;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallConnect.Utility;

namespace DaggerfallWorkshop.Game
{
    public class Climates
    {

        static PlayerEnterExit playerEnterExit = GameManager.Instance.PlayerEnterExit;
        static PlayerGPS playerGPS = GameManager.Instance.PlayerGPS;
        static PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
        static EntityEffectManager playerEffectManager = playerEntity.EntityBehaviour.GetComponent<EntityEffectManager>();
        static DaggerfallUnity dfUnity = DaggerfallUnity.Instance;

        static public bool gotDrink = true;
        static private int offSet = -5; //used to make small adjustments to the mod. Negative numbers makes the character freeze more easily.
        static public int baseNatTemp = 0;
        static public int natTemp = 0;
        static public int armorTemp = 0;
        static private int charTemp = 0;
        static public int pureClothTemp = 0;
        static public int natCharTemp = 0;
        static public int totalTemp = 0;
        static public int absTemp = 0;
        static public bool cloak = true;
        static public bool hood = true;

        public static void TemperatureCalculator()
        {
            gotDrink = WaterToDrink();
            baseNatTemp = Climate() + Season() + DayNight() + Weather();
            natTemp = Resist(baseNatTemp);
            armorTemp = Armors(baseNatTemp);
            pureClothTemp = Clothes(natTemp);
            charTemp = Resist(RaceTemp() + pureClothTemp + armorTemp - Water()) + offSet;
            natCharTemp = Resist(baseNatTemp + RaceTemp()) + offSet;
            totalTemp = ItemTemp(Dungeon(natTemp) + charTemp);
            absTemp = Mathf.Abs(totalTemp);
            cloak = Cloak();
            hood = HoodUp();
            Hunger.rations = Hunger.RationsToEat();

            AdviceText.AdviceDataUpdate();
        }

        static bool WaterToDrink()
        {
            List<DaggerfallUnityItem> skins = GameManager.Instance.PlayerEntity.Items.SearchItems(ItemGroups.UselessItems2, ClimateCalories.templateIndex_Waterskin);
            foreach (DaggerfallUnityItem skin in skins)
            {
                if (skin.weightInKg < 2 && (GameManager.Instance.PlayerEnterExit.IsPlayerSubmerged || ClimateCalories.playerIsWading || playerEnterExit.BuildingType == DFLocation.BuildingTypes.Temple))
                {
                    ClimateCalories.RefillWater(10, true);
                }
                if (skin.weightInKg > 0.1)
                {
                    return true;
                }
            }
            return false;
        }

        // ProjectN: added Maquis to the available climate types.
        static int Climate()
        {
            switch (playerGPS.CurrentClimateIndex)
            {
                case (int)MapsFile.Climates.Desert2:
                    return 50;
                case (int)MapsFile.Climates.Desert:
                    return 40;
                case (int)MapsFile.Climates.Subtropical:
                    return 30;
                case (int)MapsFile.Climates.Rainforest:
                    return 20;
                case (int)MapsFile.Climates.Maquis:
                case (int)MapsFile.Climates.Swamp:
                    return 10;
                case (int)MapsFile.Climates.Woodlands:
                    return -10;
                case (int)MapsFile.Climates.HauntedWoodlands:
                    return -20;
                case (int)MapsFile.Climates.MountainWoods:
                    return -30;
                case (int)MapsFile.Climates.Mountain:
                    return -40;
            }
            return 0;
        }

        // ProjectN: changed from Month() to Season(), now it uses the actual Season Value
        // to approximate as best as it can the actual temperature of the location.
        static int Season()
        {
            switch (DaggerfallUnity.Instance.WorldTime.Now.ActualSeasonValue)
            {
                //Spring
                case DaggerfallDateTime.Seasons.SpringEarly:
                    return -5;
                case DaggerfallDateTime.Seasons.Spring:
                    return 0;
                case DaggerfallDateTime.Seasons.SpringLate:
                    return 5;
                //Summer
                case DaggerfallDateTime.Seasons.SummerEarly:
                    return 10;
                case DaggerfallDateTime.Seasons.Summer:
                    return 20;
                case DaggerfallDateTime.Seasons.SummerLate:
                    return +15;
                //Fall
                case DaggerfallDateTime.Seasons.FallEarly:
                    return 0;
                case DaggerfallDateTime.Seasons.Fall:
                    return -5;
                case DaggerfallDateTime.Seasons.FallLate:
                    return -10;
                //Winter
                case DaggerfallDateTime.Seasons.WinterEarly:
                    return -15;
                case DaggerfallDateTime.Seasons.Winter:
                    return -20;
                case DaggerfallDateTime.Seasons.WinterLate:
                    return -15;
            }
            return 0;
        }

        static int DayNight()
        {
            WorldTime worldTime = DaggerfallUnity.Instance.WorldTime;
            int clock = worldTime.Now.MinuteOfDay;

            // ProjectN: in the original C&C, "night" started at 16, so 2 hours before actual night;
            // modifying to 1 hour before actual night for now, since at certain latitudes and during
            // winter, some days could be too short for it. Same for "night end": setting it to 30 minutes
            // after night ends instead of 1 hour.
            // ProjectN: proceeded to add Maquis in the available climate types.
            if ((clock >= (worldTime.Now.DuskHour - 60) || 
                 clock <= (worldTime.Now.DawnHour + 30)) && 
                !playerEnterExit.IsPlayerInsideDungeon)
            {
                int climate = playerGPS.CurrentClimateIndex;
                int night = 1;

                switch (climate)
                {
                    case (int)MapsFile.Climates.Desert2:
                        night = 4;
                        break;
                    case (int)MapsFile.Climates.Desert:
                        night = 3;
                        break;
                    case (int)MapsFile.Climates.Rainforest:
                    case (int)MapsFile.Climates.Subtropical:
                    case (int)MapsFile.Climates.Swamp:
                    case (int)MapsFile.Climates.Woodlands:
                    case (int)MapsFile.Climates.HauntedWoodlands:
                    case (int)MapsFile.Climates.MountainWoods:
                    case (int)MapsFile.Climates.Maquis:
                        night = 1;
                        break;
                    case (int)MapsFile.Climates.Mountain:
                        night = 2;
                        break;
                }
                // ProjectN: similar changes are applied here: I halved all the first/after
                // used by C&C. MUCH test required.
                if ((clock >= (worldTime.Now.DuskHour - 60) && clock <= (worldTime.Now.DuskHour + 30)) || 
                    (clock >= (worldTime.Now.DawnHour - 60) && clock <= (worldTime.Now.DawnHour + 30)))
                { return -10 * night; }
                else
                { return -20 * night; }
            }
            return 0;
        }

        static int Weather()
        {
            int temp = 0;
            int wetWeather = 0;
            if (!playerEnterExit.IsPlayerInsideDungeon && !playerEnterExit.IsPlayerInsideBuilding)
            {
                bool isRaining = GameManager.Instance.WeatherManager.IsRaining;
                bool isOvercast = GameManager.Instance.WeatherManager.IsOvercast;
                bool isStorming = GameManager.Instance.WeatherManager.IsStorming;
                bool isSnowing = GameManager.Instance.WeatherManager.IsSnowing;

                if (isRaining)
                {
                    temp -= 10;
                    if (cloak && hood)
                    {
                        wetWeather = 0;
                    }
                    else if (cloak && !hood)
                    {
                        wetWeather = 1;
                    }
                    else
                    { wetWeather = 3; }
                }
                else if (isStorming)
                {
                    temp -= 15;
                    if (cloak && hood)
                    {
                        wetWeather = 1;
                    }
                    else if (cloak && !hood)
                    {
                        wetWeather = 2;
                    }
                    else
                    { wetWeather = 5; }                  
                }
                else if (isSnowing)
                {
                    temp -= 10;
                    if (cloak)
                    {
                        wetWeather = 0;
                    }
                    else
                    {
                        wetWeather = 1;
                    }
                }
                else if (isOvercast)
                {
                    temp -= 8;
                }
                else if (ClimateCalories.isVampire && playerEnterExit.IsPlayerInSunlight)
                {
                    int heat = Resist(Climate() + Season() + DayNight());
                    if (heat > 0 && DaggerfallUnity.Instance.WorldTime.Now.IsDay && !hood)
                    {
                        playerEntity.DecreaseHealth(heat / 5);
                    }
                }
            }
            ClimateCalories.wetWeather = wetWeather;
            return temp;
        }

        // Resist adjusts the number (usually NatTemp or CharTemp) for class resistances.
        // ProjectN: added values for Climate Survival/Uncomfortable. Test required.
        static int Resist(int temp)
        {
            int resFire = playerEntity.Resistances.LiveFire;
            int resFrost = playerEntity.Resistances.LiveFrost;
            DFCareer.Elements element = DFCareer.Elements.Frost;
            if (temp > 0) { element = DFCareer.Elements.Fire; }

            if (GameManager.Instance.PlayerEffectManager.HasLycanthropy())
            {
                if (playerEntity.IsInBeastForm)
                {
                    resFrost += 100;
                    resFire += 80;
                }
                else
                {
                    resFrost += 10;
                    resFire += 10;
                }
            }
            else if (ClimateCalories.isVampire)
            {
                resFrost += 25;
            }
            if (temp < 0)
            {
                if (playerEntity.RaceTemplate.CriticalWeaknessFlags == DFCareer.EffectFlags.Frost) { resFrost -= 50; }
                else if (playerEntity.RaceTemplate.LowToleranceFlags == DFCareer.EffectFlags.Frost) { resFrost -= 25; }
                else if (playerEntity.RaceTemplate.ResistanceFlags == DFCareer.EffectFlags.Frost) { resFrost += 25; }
                else if (playerEntity.RaceTemplate.ImmunityFlags == DFCareer.EffectFlags.Frost) { resFrost += 50; }

                if (playerGPS.CheckSurvivalSkillPresence(playerEntity.Career.ClimateSurvival)) { resFrost += 25; }
                else if (playerGPS.CheckSurvivalSkillPresence(playerEntity.Career.UncomfortableClimate)) { resFrost -= 25; }

                if (playerEntity.HasResistanceFlag(element)) { resFrost += 50; }

                temp = Mathf.Min(temp + resFrost, 0);
            }
            else
            {
                if (playerEntity.RaceTemplate.CriticalWeaknessFlags == DFCareer.EffectFlags.Fire) { resFire -= 50; }
                else if (playerEntity.RaceTemplate.LowToleranceFlags == DFCareer.EffectFlags.Fire) { resFire -= 25; }
                else if (playerEntity.RaceTemplate.ResistanceFlags == DFCareer.EffectFlags.Fire) { resFire += 25; }
                else if (playerEntity.RaceTemplate.ImmunityFlags == DFCareer.EffectFlags.Fire) { resFire += 50; }

                if (playerGPS.CheckSurvivalSkillPresence(playerEntity.Career.ClimateSurvival)) { resFire += 25; }
                else if (playerGPS.CheckSurvivalSkillPresence(playerEntity.Career.UncomfortableClimate)) { resFire -= 25; }

                if (playerEntity.HasResistanceFlag(element)) { resFire += 50; }

                temp = Mathf.Max(temp - resFire, 0);
            }
            return temp;
        }

        static Dictionary<EquipSlots, int[]> armorTemps = new Dictionary<EquipSlots, int[]>()
        {
            // slot -> { leatherT, furT, chainT, defaultT, chainM, defaultM }
            { EquipSlots.ChestArmor, new int[] { 3, 10, 1, 3, 1, 4 } },
            { EquipSlots.LegsArmor,  new int[] { 2, 8, 1, 2, 1, 3 } },
            { EquipSlots.Head,       new int[] { 1, 3, 2, 1, 1, 1 } },
            { EquipSlots.LeftArm,    new int[] { 1, 4, 2, 1, 0, 1 } },
            { EquipSlots.RightArm,   new int[] { 1, 4, 2, 1, 0, 1 } },
            { EquipSlots.Gloves,     new int[] { 1, 2, 0, 0, 0, 0 } },
            // Hands & Feet??
        };
        enum ArmorTempsIdx { LeathTempIdx, FurTempIdx, MailTempIdx, DefTempIdx, MailMetalIdx, DefMetalIdx }

        static int Armors(int natTemp)
        {
            int temp = 0;
            int metal = 0;
            foreach (EquipSlots slot in armorTemps.Keys)
            {
                DaggerfallUnityItem item = playerEntity.ItemEquipTable.GetItem(slot);
                if (item != null)
                {
                    int[] tempVals = armorTemps[slot];
                    switch (ItemBuilder.GetArmorType(item.NativeMaterialValue))
                    {
                        case ArmorTypes.Fur:
                            temp += tempVals[(int)ArmorTempsIdx.FurTempIdx];
                            break;
                        case ArmorTypes.Leather:
                            temp += tempVals[(int)ArmorTempsIdx.LeathTempIdx];
                            break;
                        case ArmorTypes.Chain:
                            temp += tempVals[(int)ArmorTempsIdx.MailTempIdx];
                            metal += tempVals[(int)ArmorTempsIdx.MailMetalIdx];
                            break;
                        default:
                            temp += tempVals[(int)ArmorTempsIdx.DefTempIdx];
                            metal += tempVals[(int)ArmorTempsIdx.DefMetalIdx];
                            break;
                    }
                }
            }
            int metalTemp = (metal * natTemp) / 20;
            if (metalTemp > 0 && playerEnterExit.IsPlayerInSunlight && !ArmorCovered())
            {
                temp += metalTemp;
                if (ClimateCalories.txtCount > ClimateCalories.txtIntervals && metalTemp > 5) { DaggerfallUI.AddHUDText("Your armor is starting to heat up."); }
            }
            else if (metalTemp < 0)
            {
                temp += (metalTemp + 1) / 2;
                if (ClimateCalories.txtCount > ClimateCalories.txtIntervals && temp < 0) { DaggerfallUI.AddHUDText("Your armor is getting cold."); }
            }
            if (temp > 0) { temp = Mathf.Max(temp - ClimateCalories.wetCount, 0); }

            return temp;
        }


        static public bool ArmorCovered()
        {
            DaggerfallUnityItem chestCloth = playerEntity.ItemEquipTable.GetItem(EquipSlots.ChestClothes);
            if (cloak)
            {
                return true;
            }
            if (chestCloth != null)
            {
                switch (chestCloth.TemplateIndex)
                {
                    case (int)MensClothing.Priest_robes:
                    case (int)MensClothing.Plain_robes:
                    case (int)WomensClothing.Evening_gown:
                    case (int)WomensClothing.Casual_dress:
                    case (int)WomensClothing.Priestess_robes:
                    case (int)WomensClothing.Plain_robes:
                    case (int)WomensClothing.Day_gown:
                        return true;
                }
            }
            return false;
        }


        static int Clothes(int natTemp)
        {
            DaggerfallUnityItem chestCloth = playerEntity.ItemEquipTable.GetItem(EquipSlots.ChestClothes);
            DaggerfallUnityItem feetCloth = playerEntity.ItemEquipTable.GetItem(EquipSlots.Feet);
            DaggerfallUnityItem legsCloth = playerEntity.ItemEquipTable.GetItem(EquipSlots.LegsClothes);
            DaggerfallUnityItem cloak1 = playerEntity.ItemEquipTable.GetItem(EquipSlots.Cloak1);
            DaggerfallUnityItem cloak2 = playerEntity.ItemEquipTable.GetItem(EquipSlots.Cloak2);
            int chest = 0;
            int feet = 0;
            int legs = 0;
            int cloak = 0;
            int temp = 0;

            if (chestCloth != null)
            {
                switch (chestCloth.TemplateIndex)
                {
                    case (int)MensClothing.Straps:
                    case (int)MensClothing.Armbands:
                    case (int)MensClothing.Fancy_Armbands:
                    case (int)MensClothing.Champion_straps:
                    case (int)MensClothing.Sash:
                    case (int)MensClothing.Challenger_Straps:
                    case (int)MensClothing.Eodoric:
                    case (int)MensClothing.Vest:
                    case (int)WomensClothing.Brassier:
                    case (int)WomensClothing.Formal_brassier:
                    case (int)WomensClothing.Eodoric:
                    case (int)WomensClothing.Formal_eodoric:
                    case (int)WomensClothing.Vest:
                        chest = 1;
                        break;
                    case (int)MensClothing.Short_shirt:
                    case (int)MensClothing.Short_shirt_with_belt:
                    case (int)WomensClothing.Short_shirt:
                    case (int)WomensClothing.Short_shirt_belt:
                        chest = 5;
                        break;
                    case (int)MensClothing.Short_tunic:
                    case (int)MensClothing.Toga:
                    case (int)MensClothing.Short_shirt_closed_top:
                    case (int)MensClothing.Short_shirt_closed_top2:
                    case (int)MensClothing.Short_shirt_unchangeable:
                    case (int)MensClothing.Long_shirt:
                    case (int)MensClothing.Long_shirt_with_belt:
                    case (int)MensClothing.Long_shirt_unchangeable:
                    case (int)WomensClothing.Short_shirt_closed:
                    case (int)WomensClothing.Short_shirt_closed_belt:
                    case (int)WomensClothing.Short_shirt_unchangeable:
                    case (int)WomensClothing.Long_shirt:
                    case (int)WomensClothing.Long_shirt_belt:
                    case (int)WomensClothing.Long_shirt_unchangeable:
                    case (int)WomensClothing.Peasant_blouse:
                    case (int)WomensClothing.Strapless_dress:
                        chest = 8;
                        break;
                    case (int)MensClothing.Open_Tunic:
                    case (int)MensClothing.Long_shirt_closed_top:
                    case (int)MensClothing.Long_shirt_closed_top2:
                    case (int)MensClothing.Kimono:
                    case (int)WomensClothing.Evening_gown:
                    case (int)WomensClothing.Casual_dress:
                    case (int)WomensClothing.Long_shirt_closed:
                    case (int)WomensClothing.Open_tunic:
                        chest = 10;
                        break;
                    case (int)MensClothing.Priest_robes:
                    case (int)MensClothing.Anticlere_Surcoat:
                    case (int)MensClothing.Formal_tunic:
                    case (int)MensClothing.Reversible_tunic:
                    case (int)MensClothing.Dwynnen_surcoat:
                    case (int)MensClothing.Plain_robes:
                    case (int)WomensClothing.Priestess_robes:
                    case (int)WomensClothing.Plain_robes:
                    case (int)WomensClothing.Long_shirt_closed_belt:
                    case (int)WomensClothing.Day_gown:
                        chest = 12;
                        break;
                }
            }

            if (feetCloth != null)
            {
                switch (feetCloth.TemplateIndex)
                {
                    case (int)MensClothing.Sandals:
                    case (int)WomensClothing.Sandals:
                        feet = 0;
                        break;
                    case (int)MensClothing.Shoes:
                    case (int)WomensClothing.Shoes:
                        feet = 2;
                        break;
                    case (int)MensClothing.Tall_Boots:
                    case (int)WomensClothing.Tall_boots:
                        feet = 4;
                        break;
                    case (int)MensClothing.Boots:
                    case (int)WomensClothing.Boots:
                        if (feetCloth.CurrentVariant == 0 || feetCloth.CurrentVariant == 2)
                        {
                            feet = 4;
                        }
                        else
                        {
                            feet = 0;
                        }
                        break;
                    case (int)Armor.Boots:
                        if (ItemBuilder.GetArmorType(feetCloth.NativeMaterialValue) == ArmorTypes.Leather)
                            feet = 2;
                        else feet = 0;
                        break;
                    case (int)Armor.Sollerets:
                        feet = 2;
                        break;
                    case (int)Armor.Light_Boots:
                        if (ItemBuilder.GetArmorType(feetCloth.NativeMaterialValue) == ArmorTypes.Fur)
                            feet = 6;
                        else feet = 4;
                        break;
                    default:
                        feet = 4;
                        break;
                }

            }
            if (legsCloth != null)
            {
                switch (legsCloth.TemplateIndex)
                {
                    case (int)MensClothing.Loincloth:
                    case (int)WomensClothing.Loincloth:
                        legs = 1;
                        break;
                    case (int)MensClothing.Khajiit_suit:
                    case (int)WomensClothing.Khajiit_suit:
                        legs = 2;
                        break;
                    case (int)MensClothing.Wrap:
                    case (int)MensClothing.Short_skirt:
                    case (int)WomensClothing.Tights:
                    case (int)WomensClothing.Wrap:
                        legs = 4;
                        break;
                    case (int)MensClothing.Long_Skirt:
                    case (int)WomensClothing.Long_skirt:
                        legs = 8;
                        break;
                    case (int)MensClothing.Casual_pants:
                    case (int)MensClothing.Breeches:
                    case (int)WomensClothing.Casual_pants:
                        legs = 10;
                        break;
                }
            }
            if (cloak1 != null)
            {
                int cloak1int = 0;
                switch (cloak1.CurrentVariant)
                {
                    case 0: //closed, hood down
                        cloak1int = 4;
                        break;
                    case 1: //closed, hood up
                        cloak1int = 5;
                        break;
                    case 2: //one shoulder, hood up
                        cloak1int = 3;
                        break;
                    case 3: //one shoulder, hood down
                        cloak1int = 2;
                        break;
                    case 4: //open, hood down
                        cloak1int = 1;
                        break;
                    case 5: //open, hood up
                        cloak1int = 2;
                        break;
                }
                switch (cloak1.TemplateIndex)
                {
                    case (int)MensClothing.Casual_cloak:
                    case (int)WomensClothing.Casual_cloak:
                        cloak += cloak1int;
                        break;
                    case (int)MensClothing.Formal_cloak:
                    case (int)WomensClothing.Formal_cloak:
                        cloak += (cloak1int * 3);
                        break;
                }

            }
            if (cloak2 != null)
            {
                int cloak2int = 0;
                switch (cloak2.CurrentVariant)
                {
                    case 0: //closed, hood down
                        cloak2int = 4;
                        break;
                    case 1: //closed, hood up
                        cloak2int = 5;
                        break;
                    case 2: //one shoulder, hood up
                        cloak2int = 3;
                        break;
                    case 3: //one shoulder, hood down
                        cloak2int = 2;
                        break;
                    case 4: //open, hood down
                        cloak2int = 1;
                        break;
                    case 5: //open, hood up
                        cloak2int = 2;
                        break;
                }
                switch (cloak2.TemplateIndex)
                {
                    case (int)MensClothing.Casual_cloak:
                    case (int)WomensClothing.Casual_cloak:
                        cloak += cloak2int;
                        break;
                    case (int)MensClothing.Formal_cloak:
                    case (int)WomensClothing.Formal_cloak:
                        cloak += (cloak2int * 3);
                        break;
                }
            }
            pureClothTemp = chest + feet + legs + cloak;
            temp = Mathf.Max(pureClothTemp - ClimateCalories.wetCount, 0);
            if (natTemp > 30 && playerEnterExit.IsPlayerInSunlight && hood)
            { temp -= 10; }
            return temp;
        }

        static int RaceTemp()
        {
            switch (playerEntity.BirthRaceTemplate.ID)
            {
                case (int)Races.Nord:
                    return 5;
                case (int)Races.Breton:
                case (int)Races.Orc:
                    return 5;
                case (int)Races.HighElf:
                case (int)Races.WoodElf:
                case (int)Races.Imperial:
                    return 0;
                case (int)Races.Khajiit:
                case (int)Races.DarkElf:
                case (int)Races.Redguard:
                    return -5;
                case (int)Races.Argonian:
                    return -10;
            }
            return 0;
        }

        static int Water()
        {
            int temp = 0;
            ClimateCalories.wetEnvironment = 0;
            if (GameManager.Instance.PlayerEnterExit.IsPlayerSubmerged) { ClimateCalories.wetEnvironment = 300; }
            if (ClimateCalories.playerIsWading) { ClimateCalories.wetEnvironment += 50; }
            if (ClimateCalories.wetCount > 0)
            {
                if (ClimateCalories.wetCount > 300)
                {
                    ClimateCalories.wetCount = 300;
                }
                temp = (ClimateCalories.wetCount / 20);
            }
            return temp;
        }

        // Adjust temperature for waterskin ( or barrel of grog when added) in inventory.
        static int ItemTemp(int charNatTemp)
        {
            if (charNatTemp > 9 && gotDrink && !GameManager.Instance.PlayerEffectManager.HasVampirism())
            {
                charNatTemp -= 20;
                charNatTemp = Mathf.Max(charNatTemp, 0);
            }
            return charNatTemp;
        }

        static bool Cloak()
        {
            DaggerfallUnityItem cloak1 = playerEntity.ItemEquipTable.GetItem(EquipSlots.Cloak1);
            DaggerfallUnityItem cloak2 = playerEntity.ItemEquipTable.GetItem(EquipSlots.Cloak2);

            if (cloak1 != null || cloak2 != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        static bool HoodUp()
        {
            DaggerfallUnityItem cloak1 = playerEntity.ItemEquipTable.GetItem(EquipSlots.Cloak1);
            DaggerfallUnityItem cloak2 = playerEntity.ItemEquipTable.GetItem(EquipSlots.Cloak2);
            DaggerfallUnityItem chestCloth = playerEntity.ItemEquipTable.GetItem(EquipSlots.ChestClothes);
            bool up = false;
            if (cloak1 != null)
            {
                switch (cloak1.CurrentVariant)
                {
                    case 0:
                    case 3:
                    case 4:
                        up = false;
                        break;
                    case 1:
                    case 2:
                    case 5:
                        up = true;
                        break;
                }
            }
            if (cloak2 != null && !up)
            {
                switch (cloak2.CurrentVariant)
                {
                    case 0:
                    case 3:
                    case 4:
                        up = false;
                        break;
                    case 1:
                    case 2:
                    case 5:
                        up = true;
                        break;
                }
            }
            if (chestCloth != null && !up)
            {
                switch (chestCloth.TemplateIndex)
                {
                    case (int)MensClothing.Plain_robes:
                    case (int)WomensClothing.Plain_robes:
                        switch (chestCloth.CurrentVariant)
                        {
                            case 0:
                                up = false;
                                break;
                            case 1:
                                up = true;
                                break;
                        }
                        break;
                }
            }
            return up;
        }

        // If inside dungeon, the temperature effects is decreased.
        static public int Dungeon(int natTemp)
        {
            if (playerEnterExit.IsPlayerInsideDungeon || playerEnterExit.IsPlayerInsideDungeonCastle || playerEnterExit.IsPlayerInsideSpecialArea)
            {
                if (natTemp > -20)
                {
                    natTemp = Mathf.Max((natTemp / 2) - 30, -20);
                }
                else
                {
                    natTemp = Mathf.Min((natTemp / 2) + 30, -20);
                }
            }
            return natTemp;
        }

    }
}
