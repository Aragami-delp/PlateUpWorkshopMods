using Kitchen;
using KitchenMods;
using System.Reflection;
using UnityEngine;
using Steamworks;
using Steamworks.Ugc;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;
using System;
using KitchenLib;

namespace Workshopupdater
{
    [HarmonyPatch(typeof(StartMainMenu), nameof(StartMainMenu.Setup))]
    public class StartMainMenu_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(StartMainMenu __instance)
        {
            MethodInfo addButton = __instance.GetType().GetMethod("AddButton", BindingFlags.NonPublic | BindingFlags.Instance);
            addButton.Invoke(__instance, new object[] { WorkshopupdaterMain.HasModUpdate ? "UPDATES AVAILABLE" : "All mods up-to-date", (Action<int>)(i => WorkshopupdaterMain.UpdateAllMods()), 0, 1f, 0.2f });
        }
    }

    [HarmonyPatch(typeof(RevisedMainMenu), nameof(RevisedMainMenu.Setup))]
    public class RevisedMainMenu_Patch
    {
        [HarmonyPostfix]
        public static void Postfix(RevisedMainMenu __instance)
        {
            MethodInfo addButton = __instance.GetType().GetMethod("AddButton", BindingFlags.NonPublic | BindingFlags.Instance);
            addButton.Invoke(__instance, new object[] { WorkshopupdaterMain.HasModUpdate ? "UPDATES AVAILABLE" : "All mods up-to-date", (Action<int>)(i => WorkshopupdaterMain.UpdateAllMods()), 0, 1f, 0.2f });
        }
    }

    public class WorkshopupdaterMain : GenericSystemBase, IModSystem, IModInitializer
    {
        // guid must be unique and is recommended to be in reverse domain name notation
        // mod name that is displayed to the player and listed in the mods menu
        // mod version must follow semver e.g. "1.2.3"
        public const string MOD_GUID = "aragami.plateup.mods.workshopupdater";
        public const string MOD_NAME = "Workshopupdater";
        public const string MOD_VERSION = "0.1.0";
        public const string MOD_AUTHOR = "Aragami";
        public const string MOD_GAMEVERSION = ">=1.1.2";
        // Game version this mod is designed for in semver
        // e.g. ">=1.1.1" current and all future
        // e.g. ">=1.1.1 <=1.2.3" for all from/until

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
        }

        private static readonly Harmony m_harmony = new Harmony("aragami.plateup.mods.workshopupdater");

        protected override void Initialise()
        {
            base.Initialise();
            //SteamUGC.Download(2916090048, true);
        }

        protected override void OnUpdate()
        {

        }

        public static List<Item> updateMods = new List<Item>();

        public static bool HasModUpdate => updateMods.Count > 0;

        public static void UpdateAllMods()
        {
            // TODO: unload all mods excpet this one - nvm
            if (updateMods.Count > 0)
            {
                foreach (Item updateMod in updateMods)
                {
                    LogInfo("Updating Mod: " + updateMod.Id + " " + updateMod.Title);
                    updateMod.Download(false);
                }
                // Close game immediatly after forcing mod updates
                LogInfo("Exiting game for workshop updates");
                Session.SoftExit();
            }
            else
            {
                LogWarning("Attempting to update mods, but there are not updates!");
            }
        }

        public static async void RetriveModUpdates()
        {
            Query subedItemsQuery = Query.ItemsReadyToUse.WhereUserSubscribed(SteamClient.SteamId.AccountId);
            ResultPage? ugcResult = await subedItemsQuery.GetPageAsync(1); // TODO: multiple pages? - how many?
            if (ugcResult != null)
            {
                foreach (Item workshopItem in ugcResult.Value.Entries)
                {
                    //LogError("ID: " + workshopItem.Id + "; UpdateTime: " + workshopItem.Chan + workshopItem.Updated.ToString() +"; Now: " + System.DateTime.UtcNow + "; Created: " + workshopItem.Created.ToString() + "; Title: " + workshopItem.Title + "; Needs update: " + workshopItem.NeedsUpdate.ToString());
                    if (workshopItem.NeedsUpdate) // Maybe also all mods that got updated with the last day? Since Steam sometimes doesnt want to find updates
                    {
                        updateMods.Add(workshopItem);
                    }
                }
            }
        }

        #region Logging
        // You can remove this, I just prefer a more standardized logging
        public static void LogInfo(string _log) { Debug.Log($"[{MOD_NAME}] " + _log); }
        public static void LogWarning(string _log) { Debug.LogWarning($"[{MOD_NAME}] " + _log); }
        public static void LogError(string _log) { Debug.LogError($"[{MOD_NAME}] " + _log); }
        public static void LogInfo(object _log) { LogInfo(_log.ToString()); }
        public static void LogWarning(object _log) { LogWarning(_log.ToString()); }
        public static void LogError(object _log) { LogError(_log.ToString()); }

        public static void PatchHarmony()
        {
            List<Mod> mods = ModPreload.Mods;
            bool isKitchenLibInstalled = false;
            foreach (Mod mod in mods)
            {
                // KitchenLib workshop id or name
                if (mod.Name == "2898069883" || mod.Name == "KitchenLib")
                {
                    isKitchenLibInstalled = true;
                    break;
                }
            }

            MethodInfo original;
            MethodInfo postfix;

            if (isKitchenLibInstalled)
            {
                original = typeof(RevisedMainMenu).GetMethod(nameof(RevisedMainMenu.Setup));
                postfix = typeof(RevisedMainMenu_Patch).GetMethod(nameof(RevisedMainMenu_Patch.Postfix));
            }
            else
            {
                original = typeof(StartMainMenu).GetMethod(nameof(StartMainMenu.Setup));
                postfix = typeof(StartMainMenu_Patch).GetMethod(nameof(StartMainMenu_Patch.Postfix));
            }
            m_harmony.Patch(original, postfix: new HarmonyMethod(postfix));
        }

        public void PostActivate(Mod mod)
        {
            //TODO: Patch main menu
            LogError($"{MOD_GUID} v{MOD_VERSION} in use!");
            LogError("Patch Main menu here");
            RetriveModUpdates();
            // Patch after initializing mods
            PatchHarmony();
        }

        public void PreInject() { }
        public void PostInject() { }

        #endregion
    }

}
