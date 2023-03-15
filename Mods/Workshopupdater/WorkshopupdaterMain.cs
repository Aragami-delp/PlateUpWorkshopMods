using Kitchen;
using KitchenMods;
using System.Reflection;
using UnityEngine;
using Steamworks.Ugc;
using System.Collections.Generic;
using HarmonyLib;
using System;
using System.Threading.Tasks;

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

        public static List<Item> updateMods = new List<Item>();
        private static readonly Harmony m_harmony = new Harmony("aragami.plateup.mods.workshopupdater");

        public static bool HasModUpdate => updateMods.Count > 0;

        public void PostActivate(Mod mod)
        {
            // Patch after initializing mods
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");
            RetriveModUpdates();
            PatchHarmony();
        }

        protected override void OnUpdate() { }
        public void PreInject() { }
        public void PostInject() { }

        public static void UpdateAllMods()
        {
            // TODO: unload all mods excpet this one - nvm. the game should close fast enough (hopefully)
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
                LogWarning("Attempting to update mods, but there are no updates!");
            }
        }

        public static void RetriveModUpdates()
        {
            List<Item> result = Task.Run(() => Helper.GetSubscribedModItems()).GetAwaiter().GetResult();

            foreach (Item workshopItem in result)
            {
                //LogError("ID: " + workshopItem.Id + "; UpdateTime: " + workshopItem.Chan + workshopItem.Updated.ToString() +"; Now: " + System.DateTime.UtcNow + "; Created: " + workshopItem.Created.ToString() + "; Title: " + workshopItem.Title + "; Needs update: " + workshopItem.NeedsUpdate.ToString());
                if (workshopItem.NeedsUpdate) // Steam needs about 5min before it finds an update
                {
                    updateMods.Add(workshopItem);
                }
            }
        }

        ///// <summary>
        ///// Gets all mod dependency IDs, that are needed for all given mods, including recurives dependencies
        ///// </summary>
        ///// <param name="_modIDs">Mods IDs use to find dependencies</param>
        ///// <returns>List of mod IDs of dependencies not yet subscribed to</returns>
        //public static async Task<List<Item>> GetAllModDependencies(List<Item> _modIDs)
        //{
        //    Queue<Item> modIDsToCheck = new Queue<Item>(_modIDs);
        //    List<Item> retVal_ModDependencyIDs = new List<Item>();
        //    while (modIDsToCheck.Count > 0)
        //    {
        //        Steamworks.SteamUGC.
        //    }
        //}

        /// <summary>
        /// Gets all mod dependency IDs, that are needed for the given mod
        /// </summary>
        /// <param name="_modID"></param>
        /// <returns></returns>
        //private static async Task<List<Item>> GetModDependencies(Item _modID)
        //{

        //}

        //private static async Task<Item> GetSteamWorkshopItemFromID(Item)
        //{

        //}

        //  http://www.java2s.com/Code/CSharp/Reflection/Getsanassemblybyitsnameifitiscurrentlyloaded.htm
        /// <summary>
        /// Gets an assembly by its name if it is currently loaded
        /// </summary>
        /// <param name="Name">Name of the assembly to return</param>
        /// <returns>The assembly specified if it exists, otherwise it returns null</returns>
        public static System.Reflection.Assembly GetLoadedAssembly(string Name)
        {
            try
            {
                foreach (Assembly TempAssembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (TempAssembly.GetName().Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return TempAssembly;
                    }
                }
                return null;
            }
            catch { throw; }
        }

        public static void PatchHarmony()
        {
            List<Mod> mods = ModPreload.Mods;
            bool isKitchenLibInstalled = false;
            Assembly asmKitchenLib = null;
            foreach (Mod mod in mods)
            {
                // KitchenLib workshop id or name
                if (mod.Name == "2898069883" || mod.Name == "KitchenLib")
                {
                    try
                    {
                        asmKitchenLib = GetLoadedAssembly("KitchenLib-Workshop");
                        isKitchenLibInstalled = true;
                    }
                    catch (Exception _ex)
                    {
                        // KitchenLib local install (hopefully)
                        if (mod.Name == "KitchenLib")
                        {
                            asmKitchenLib = GetLoadedAssembly("KitchenLib");
                            isKitchenLibInstalled = true;
                        }
                    }
                    break;
                }
            }

            MethodInfo original;

            if (isKitchenLibInstalled)
            {
                LogInfo("KitchenLib loaded; Patching KitchenLib Main Menu");
                original = asmKitchenLib.GetType("KitchenLib.Patches.RevisedMainMenu").GetMethod("Setup");
            }
            else
            {
                LogInfo("KitchenLib not found; Patching Vanilla Main Menu");
                original = typeof(StartMainMenu).GetMethod(nameof(StartMainMenu.Setup));
                
            }
            m_harmony.Patch(original, postfix: new HarmonyMethod(typeof(WorkshopupdaterMain).GetMethod(nameof(MainMenu_Postfix))));
        }

        public static void MainMenu_Postfix(object __instance)
        {
            MethodInfo addButton = __instance.GetType().GetMethod("AddButton", BindingFlags.NonPublic | BindingFlags.Instance);
            addButton.Invoke(__instance, new object[] { WorkshopupdaterMain.HasModUpdate ? "UPDATES AVAILABLE" : "All mods up-to-date", (Action<int>)(i => WorkshopupdaterMain.UpdateAllMods()), 0, 1f, 0.2f });
        }

        #region Logging
        // You can remove this, I just prefer a more standardized logging
        public static void LogInfo(string _log) { Debug.Log($"[{MOD_NAME}] " + _log); }
        public static void LogWarning(string _log) { Debug.LogWarning($"[{MOD_NAME}] " + _log); }
        public static void LogError(string _log) { Debug.LogError($"[{MOD_NAME}] " + _log); }
        public static void LogInfo(object _log) { LogInfo(_log.ToString()); }
        public static void LogWarning(object _log) { LogWarning(_log.ToString()); }
        public static void LogError(object _log) { LogError(_log.ToString()); }
        #endregion
    }

}
