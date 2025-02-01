// Project:         Climates & Calories mod for Daggerfall Unity (http://www.dfworkshop.net)
// Copyright:       Copyright (C) 2020 Ralzar
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Author:          Ralzar

using DaggerfallWorkshop;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Entity;
using DaggerfallWorkshop.Game.Formulas;
using DaggerfallWorkshop.Game.Items;
using DaggerfallWorkshop.Game.MagicAndEffects;
using DaggerfallWorkshop.Game.Serialization;
using DaggerfallWorkshop.Game.Utility;

namespace DaggerfallWorkshop.Game
{
    /// <summary>
    /// Abstract class for all food items common behaviour
    /// </summary>
    public abstract class AbstractItemFood : DaggerfallUnityItem
    {
        public AbstractItemFood(ItemGroups itemGroup, int templateIndex) : base(itemGroup, templateIndex)
        {
            message = (int)FoodStatus.Fresh;
        }

        public abstract uint GetCalories();

        // public int FoodStatus
        // {
        //     get { return message; }
        //     set { message = value; }
        // }

        public virtual string GetFoodStatus()
        {
            switch (message)
            {
                case (int)FoodStatus.Stale:
                    return "Smelly ";
                case (int)FoodStatus.Mouldy:
                    return "Mouldy ";
                case (int)FoodStatus.Rotten:
                    return "Rotten ";
                case (int)FoodStatus.Putrid:
                    return "Putrid ";
                default:
                    return "";
            }
        }

        // Use template world archive for fresh/stale food, or [template index] inventory texture archive for other states
        public override int InventoryTextureArchive
        {
            get
            {
                if (message == (int)FoodStatus.Fresh || message == (int)FoodStatus.Stale)
                    return WorldTextureArchive;
                else
                    return InventoryTextureArchive;
            }
        }

        // Use template world record for fresh food, or status for other states
        public override int InventoryTextureRecord
        {
            get
            {
                switch (message)
                {
                    case (int)FoodStatus.Fresh:
                    case (int)FoodStatus.Stale:
                    default:
                        return WorldTextureRecord;
                    case (int)FoodStatus.Mouldy:
                        return 0;
                    case (int)FoodStatus.Rotten:
                        return 1;
                    case (int)FoodStatus.Putrid:
                        return 1;
                }
            }
        }

        public bool RotFood()
        {
            if (message < (int)FoodStatus.Putrid)
            {
                message++;
                shortName = GetFoodStatus() + ItemTemplate.name;
                value = 0;
                return false;
            }
            return true;
        }

        public override bool UseItem(ItemCollection collection)
        {
            PlayerEntity playerEntity = GameManager.Instance.PlayerEntity;
            uint gameMinutes = DaggerfallUnity.Instance.WorldTime.DaggerfallDateTime.ToClassicDaggerfallTime();
            uint hunger = gameMinutes - playerEntity.LastTimePlayerAteOrDrankAtTavern;
            uint cals = GetCalories() / ((uint)message + 1);
            string feel = "invigorated";

            if (message == (int)FoodStatus.Putrid)
            {
                DaggerfallUI.MessageBox(string.Format("This {0} is too disgusting to force down.", shortName));
            }
            else if (hunger >= cals)
            {
                if (hunger > cals + 240)
                {
                    playerEntity.LastTimePlayerAteOrDrankAtTavern = gameMinutes - 240;
                }
                playerEntity.LastTimePlayerAteOrDrankAtTavern += cals;

                collection.RemoveItem(this);
                DaggerfallUI.MessageBox(string.Format("You eat the {0}.", shortName));

                if (message > (int)FoodStatus.Stale || (TemplateIndex == ItemRawMeat.templateIndex || TemplateIndex == ItemRawFish.templateIndex))
                {
                    feel = TemplateIndex == ItemRawMeat.templateIndex ? "nauseated" : "invigorated";
                    bool unlucky = Dice100.SuccessRoll(playerEntity.Stats.LiveLuck);
                    if (unlucky || (TemplateIndex == ItemRawMeat.templateIndex || TemplateIndex == ItemRawFish.templateIndex))
                    {
                        feel = "disgusted";
                        Diseases[] diseaseListA = { Diseases.StomachRot };
                        Diseases[] diseaseListB = { Diseases.StomachRot, Diseases.SwampRot, Diseases.BloodRot, Diseases.Cholera, Diseases.YellowFever };
                        if (message > (int)FoodStatus.Mouldy)
                            FormulaHelper.InflictDisease(playerEntity as DaggerfallEntity, playerEntity as DaggerfallEntity, diseaseListB);
                        else
                            FormulaHelper.InflictDisease(playerEntity as DaggerfallEntity, playerEntity as DaggerfallEntity, diseaseListA);
                    }
                }
                

                DaggerfallUI.AddHUDText("You feel " + feel + " by the meal.");
            }
            else
            {
                DaggerfallUI.MessageBox(string.Format("You are not hungry enough to eat the {0} right now.", shortName));
            }
            return true;
        }

        public void FoodOnHUD()
        {
            if (ClimateCalories.isVampire)
            {
                DaggerfallUI.AddHUDText("The meal does nothing for you.");
            }
            else
            {
                DaggerfallUI.AddHUDText("You feel invigorated by the meal.");
            }
        }
    }

    //Apple
    public class ItemApple : AbstractItemFood
    {
        public const int templateIndex = 306;

        public ItemApple() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 60;
        }

        public override string GetFoodStatus()
        {
            switch (message)
            {
                case (int)FoodStatus.Stale:
                    return "Soft ";
                default:
                    return base.GetFoodStatus();
            }
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemApple).ToString();
            return data;
        }
    }

    //Orange
    public class ItemOrange : AbstractItemFood
    {
        public const int templateIndex = 307;

        public ItemOrange() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 60;
        }

        public override string GetFoodStatus()
        {
            switch (message)
            {
                case (int)FoodStatus.Stale:
                    return "Soft ";
                default:
                    return base.GetFoodStatus();
            }
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemOrange).ToString();
            return data;
        }
    }

    //Bread
    public class ItemBread : AbstractItemFood
    {
        public const int templateIndex = 308;

        public ItemBread() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 180;
        }

        public override string GetFoodStatus()
        {
            switch (message)
            {
                case (int)FoodStatus.Stale:
                    return "Stale ";
                default:
                    return base.GetFoodStatus();
            }
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemBread).ToString();
            return data;
        }
    }

    //Fish
    public class ItemRawFish : AbstractItemFood
    {
        public const int templateIndex = 309;

        public ItemRawFish() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 90;
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemRawFish).ToString();
            return data;
        }
    }

    //Salted Fish
    public class ItemCookedFish : AbstractItemFood
    {
        public const int templateIndex = 310;

        public ItemCookedFish() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 200;
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemCookedFish).ToString();
            return data;
        }
    }

    //Meat
    public class ItemMeat : AbstractItemFood
    {
        public const int templateIndex = 311;

        public ItemMeat() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 240;
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemMeat).ToString();
            return data;
        }
    }

    //Raw Meat
    public class ItemRawMeat : AbstractItemFood
    {
        public const int templateIndex = 312;

        public ItemRawMeat() : base(ItemGroups.UselessItems2, templateIndex)
        {
        }

        public override uint GetCalories()
        {
            return 100;
        }

        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemRawMeat).ToString();
            return data;
        }
    }

    public class ItemRations : DaggerfallUnityItem
    {
        public const int templateIndex = ClimateCalories.templateIndex_Rations;

        public ItemRations() : base(ItemGroups.UselessItems2, templateIndex)
        {
            stackCount = UnityEngine.Random.Range(2, 10);
        }

        public override bool IsStackable()
        {
            return true;
        }

        public override bool UseItem(ItemCollection collection)
        {
            DaggerfallUI.MessageBox(string.Format("When too hungry, you will eat some rations."));
            return true;
        }


        public override ItemData_v1 GetSaveData()
        {
            ItemData_v1 data = base.GetSaveData();
            data.className = typeof(ItemRations).ToString();
            return data;
        }
    }

}

