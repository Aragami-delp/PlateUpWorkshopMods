using HarmonyLib;
using Kitchen;
using UnityEngine;

namespace KitchenPhoneIndicators
{
    [HarmonyPatch(typeof(PlayerView), "Update")]
    public class PhoneIndicatorsPatch_PlayerView_Update
    {
        /// <summary>
        /// Called every frame
        /// </summary>
        [HarmonyPrefix]
        public static void Update_CheckPrepState()
        {
            PhoneIndicatorsMono.Instance.PlayerView_Update_Prefix();
        }
    }
}
