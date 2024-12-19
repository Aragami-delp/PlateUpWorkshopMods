using Kitchen;
using KitchenMods;
using UnityEngine;
using System.Collections.Generic;
using Steamworks.Data;
using System.Threading.Tasks;
using Steamworks.Ugc;
using System.Linq;
using System.Windows.Forms;
using System;

// Namespace should have "Kitchen" in the beginning
namespace KitchenDependencyChecker
{
    public class DependencyCheckerMain : GenericSystemBase, IModSystem, IModInitializer
    {
        // GUID must be unique and is recommended to be in reverse domain name notation
        // Mod Name is displayed to the player and listed in the mods menu
        // Mod Version must follow semver notation e.g. "1.2.3"
        public const string MOD_GUID = "aragami.plateup.mods.dependencyChecker";
        public const string MOD_NAME = "Dependency Checker";
        public const string MOD_VERSION = "1.0.0";
        public const string MOD_AUTHOR = "Aragami";
        public const string MOD_GAMEVERSION = ">=1.1.4";
        // Game version this mod is designed for in semver
        // e.g. ">=1.1.3" current and all future
        // e.g. ">=1.1.3 <=1.2.3" for all from/until

        // Boolean constant whose value depends on whether you built with DEBUG or RELEASE mode, useful for testing
#if DEBUG
        public const bool DEBUG_MODE = true;
#else
        public const bool DEBUG_MODE = false;
#endif


        public void PostActivate(Mod mod)
        {
            //if (!firstStart) { return; }
            if (UnityEngine.Application.platform != RuntimePlatform.WindowsPlayer && UnityEngine.Application.platform != RuntimePlatform.WindowsEditor)
                LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");

            Task.Run(async () =>
            {
                try
                {
                    await HandleModActivationAsync(mod);
                }
                catch (Exception ex)
                {
                    LogError($"Error in mod activation: {ex.Message}");
                }
            });
        }

        private async Task HandleModActivationAsync(Mod mod)
        {
            //HashSet<PublishedFileId> lastItems = Helper.LoadLastModList();
            List<Item> currentItems = await Helper.GetInstalledModItems();
            //List<Item> newItems = currentItems.Where(item => !lastItems.Contains(item.Id)).ToList(); // Should get all new items // Check all for dependency change whenever a single mod is added/removed

            //if (!lastItems.ToHashSet().SetEquals(currentItems.Select(x => x.Id)))
            {
                HashSet<PublishedFileId> deps = new();
                try
                {
                    deps = await Helper.GetAllModDependencies(currentItems);
                    LogError("DepsCount" + deps.Count);
                }
                catch (Exception e)
                {
                    LogError("ErrorHere");
                }
                if (deps.Count > 0)
                {
                    foreach (PublishedFileId depId in deps.ToHashSet())
                    {
                        foreach (Item item in currentItems)
                        {
                            if (item.Id == depId)
                            {
                                deps.Remove(depId);
                            }
                        }
                    }
                    List<Item> dependencyItems = await Helper.GetModItems(deps);

                    if (dependencyItems.Count > 0)
                        ShowDependencyDialog(dependencyItems);
                    //else
                    //    Helper.SaveCurrentModList(currentItems);
                }
                //else
                //    Helper.SaveCurrentModList(currentItems);
            }
            //else
            //    Helper.SaveCurrentModList(currentItems);
        }

        private void ShowDependencyDialog(List<Item> _missingItems)
        {
            string messageStart = "Some of your mods are missing dependencies in order to work properly. Missing Mods:";
            string messageBody = string.Empty;
            foreach (Item missingItem in _missingItems)
            {
                messageBody += "\n" + missingItem.Title;
            }
            string messageEnd = "\n\nPress \"Yes\" to close the game and install these automatically.\nPress \"No\" to close the game.\nPress \"Cancel\" to start anyways.";
            bool startGame = false;
            DialogResult result = MessageBox.Show(messageStart + messageBody + messageEnd, "DependencyChecker", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Error);
            if (result == DialogResult.Yes)
            {
                //Helper.SaveCurrentModList();
                Task.Run(async () => await Helper.InstallItems(_missingItems)).GetAwaiter().GetResult();
                startGame = false;
            }
            else if (result == DialogResult.No)
            {
                startGame = false;
            }
            else if (result == DialogResult.Cancel)
            {
                startGame = true;
            }
            if (!startGame) { Session.SoftExit(); }
        }

        public void PreInject()
        {

        }

        public void PostInject()
        {

        }

        protected override void OnUpdate()
        {

        }

        #region Logging
        public static void LogInfo(string _log) { Debug.Log($"[{MOD_NAME}] " + _log); }
        public static void LogWarning(string _log) { Debug.LogWarning($"[{MOD_NAME}] " + _log); }
        public static void LogError(string _log) { Debug.LogError($"[{MOD_NAME}] " + _log); }
        public static void LogInfo(object _log) { LogInfo(_log.ToString()); }
        public static void LogWarning(object _log) { LogWarning(_log.ToString()); }
        public static void LogError(object _log) { LogError(_log.ToString()); }
        #endregion
    }
}
