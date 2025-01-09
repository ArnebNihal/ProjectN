// Project:         Daggerfall Unity
// Copyright:       Copyright (C) 2009-2022 Daggerfall Workshop
// Web Site:        http://www.dfworkshop.net
// License:         MIT License (http://www.opensource.org/licenses/mit-license.php)
// Source Code:     https://github.com/Interkarma/daggerfall-unity
// Original Author: Hazelnut

using DaggerfallConnect;
using DaggerfallWorkshop.Game;
using DaggerfallWorkshop.Game.Guilds;
using DaggerfallWorkshop.Game.MagicAndEffects.MagicEffects;
using DaggerfallWorkshop.Game.UserInterface;
using DaggerfallWorkshop.Game.UserInterfaceWindows;
using System.Collections.Generic;
using UnityEngine;

namespace DaggerfallWorkshop
{
    /// <summary>
    /// Copy/paste of Hazelnut's R&R clickable beds.
    /// </summary>
    public class DaggerfallBed : MonoBehaviour
    {
        void Start()
        {

        }

        public void Rest()
        {
            if (GameManager.Instance.AreEnemiesNearby(true))
            {
                // Raise enemy alert status when monsters nearby
                GameManager.Instance.PlayerEntity.SetEnemyAlert(true);

                // Alert player if monsters nearby
                const int enemiesNearby = 354;
                DaggerfallUI.MessageBox(enemiesNearby);
            }
            else if (GameManager.Instance.PlayerEnterExit.IsPlayerSwimming ||
                     !GameManager.Instance.PlayerMotor.StartRestGroundedCheck())
            {
                const int cannotRestNow = 355;
                DaggerfallUI.MessageBox(cannotRestNow);
            }
            else
            {
                var preventedRestMessage = GameManager.Instance.GetPreventedRestMessage();
                if (preventedRestMessage != null)
                {
                    if (preventedRestMessage != "")
                        DaggerfallUI.MessageBox(preventedRestMessage);
                    else
                    {
                        const int cannotRestNow = 355;
                        DaggerfallUI.MessageBox(cannotRestNow);
                    }
                }
                else
                {
                    RacialOverrideEffect racialOverride = GameManager.Instance.PlayerEffectManager.GetRacialOverrideEffect(); // Allow custom race to block rest (e.g. vampire not sated)
                    if (racialOverride != null && !racialOverride.CheckStartRest(GameManager.Instance.PlayerEntity))
                        return;

                    IUserInterfaceManager uiManager = DaggerfallUI.UIManager;
                    uiManager.PushWindow(UIWindowFactory.GetInstanceWithArgs(UIWindowType.Rest, new object[] { uiManager, true }));
                }
            }
        }
    }
}