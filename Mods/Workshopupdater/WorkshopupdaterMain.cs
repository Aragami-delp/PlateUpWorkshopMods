using Kitchen;
using KitchenMods;
using System.Reflection;
using UnityEngine;
using Steamworks;
using Steamworks.Ugc;
using System.Collections.Generic;
using System.Threading.Tasks;
using HarmonyLib;

namespace Workshopupdater
{
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

        protected override void Initialise()
        {
            base.Initialise();
            // For log file output so the official plateup support staff can identify if/which a mod is being used
            LogError($"{MOD_GUID} v{MOD_VERSION} in use!");
            //SteamUGC.Download(2916090048, true);
        }

        protected override void OnUpdate()
        {

        }

        public static async void UpdateAllMods()
        {
            List<Item> updateMods = new List<Item>();
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
        }

        #region Logging
        // You can remove this, I just prefer a more standardized logging
        public static void LogInfo(string _log) { Debug.Log($"[{MOD_NAME}] " + _log); }
        public static void LogWarning(string _log) { Debug.LogWarning($"[{MOD_NAME}] " + _log); }
        public static void LogError(string _log) { Debug.LogError($"[{MOD_NAME}] " + _log); }
        public static void LogInfo(object _log) { LogInfo(_log.ToString()); }
        public static void LogWarning(object _log) { LogWarning(_log.ToString()); }
        public static void LogError(object _log) { LogError(_log.ToString()); }

        public void PostActivate(Mod mod)
        {
            //TODO: Patch main menu
            LogError("Patch Main menu here");
            UpdateAllMods();
        }

        public void PreInject() { }
        public void PostInject() { }

        #endregion
    }

}
