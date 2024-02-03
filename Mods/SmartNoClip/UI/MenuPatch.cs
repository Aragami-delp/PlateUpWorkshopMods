using HarmonyLib;
using Kitchen;
using Kitchen.Modules;
using KitchenData;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KitchenSmartNoClip
{
    //[HarmonyPatch(typeof(MainMenu), nameof(MainMenu.Setup))]
    //public class MenuPatch
    //{
    //}

    //public Option<bool> EnableOption;
    //// Default player set while ghost mode is activated
    //public Option<float> SpeedOption;

    //public PrepGhostOptionsMenu(Transform container, ModuleList module_list) : base(container, module_list) { }

    //public override void Setup(int player_id)
    //{
    //    EnableOption = GetEnableOption();
    //    this.AddLabel("Ghost Mode");
    //    Add(EnableOption);

    //    SpeedOption = GetSpeedOption();
    //    this.AddLabel("Ghost Speed");
    //    Add(SpeedOption);

    //    AddButton(Localisation["MENU_BACK_SETTINGS"], (Action<int>)(i => RequestPreviousMenu()));
    //}
}
