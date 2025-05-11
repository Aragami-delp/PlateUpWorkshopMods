using HarmonyLib;
using Kitchen;
using KitchenMods;
using System.Reflection;
using UnityEngine;

// Namespace should have "Kitchen" in the beginning
namespace KitchenNamesAboveHead
{
    public class NamesAboveHeadFix : GenericSystemBase, IModSystem, IModInitializer
    {
        // GUID must be unique and is recommended to be in reverse domain name notation
        // Mod Name is displayed to the player and listed in the mods menu
        // Mod Version must follow semver notation e.g. "1.2.3"
        public const string MOD_GUID = "aragami.plateup.mods.namesabovehead";
        public const string MOD_NAME = "Names above head fix";
        public const string MOD_VERSION = "0.1.0";
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

        private static readonly Harmony m_harmony = new(MOD_GUID);

        public void PostActivate(KitchenMods.Mod mod)
        {
            // Early patch to patch InputSource before the first device connects
            m_harmony.PatchAll(Assembly.GetExecutingAssembly());

            LogWarning($"{MOD_GUID} v{MOD_VERSION} in useeee!");
        }
        #region Logging
        public static void LogInfo(string _log) { Debug.Log($"[{MOD_NAME}] " + _log); }
        public static void LogWarning(string _log) { Debug.LogWarning($"[{MOD_NAME}] " + _log); }
        public static void LogError(string _log) { Debug.LogError($"[{MOD_NAME}] " + _log); }
        public static void LogInfo(object _log) { LogInfo(_log.ToString()); }
        public static void LogWarning(object _log) { LogWarning(_log.ToString()); }
        public static void LogError(object _log) { LogError(_log.ToString()); }
        #endregion

        public void PreInject() { }
        public void PostInject() { }
        protected override void OnUpdate() { }
    }
}
