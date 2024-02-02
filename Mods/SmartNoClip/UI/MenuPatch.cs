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
    [HarmonyPatch(typeof(MainMenu), nameof(MainMenu.Setup))]
    public class MenuPatch
    {
    }
}
