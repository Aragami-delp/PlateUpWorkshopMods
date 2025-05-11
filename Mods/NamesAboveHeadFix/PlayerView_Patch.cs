using ExitGames.Client.Photon.StructWrapping;
using HarmonyLib;
using Kitchen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;

namespace KitchenNamesAboveHead
{
    [HarmonyPatch(typeof(PlayerIdentificationComponent))]
    public class PlayerIdentificationComponent_Patch
    {
        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        public static bool Update_PrefixPatch(PlayerIdentificationComponent __instance)
        {
            Traverse.Create(__instance).Field("UseNameLabel").SetValue(true);
            return true;
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        public static void Update_PostfixPatch(PlayerIdentificationComponent __instance)
        {
            Traverse traverse = Traverse.Create(__instance);
            TextMeshPro textMeshPro = traverse.Field("NameLabel").GetValue<TextMeshPro>();
            int playerid = traverse.Field("PlayerID").GetValue<int>();
            textMeshPro.text = Players.Main.Get(playerid).Name;
            traverse.Field("NameLabel").SetValue(textMeshPro);
        }
    }
}
