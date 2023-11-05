using Kitchen;
using KitchenMods;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using Workshopupdater;
using TMPro;

// Namespace should have "Kitchen" in the beginning
namespace KitchenPrepUiDuringDay
{
    [HarmonyPatch(typeof(ParametersDisplayView), nameof(ParametersDisplayView.UpdateData))]
    public class Patch_Pre_PDVVD
    {
        public static void Prefix(ref ParametersDisplayView.ViewData view_data, ParametersDisplayView __instance)
        {
            ParametersDisplayViewGO = __instance.gameObject
            PrepUiDuringDayMain.IsActuallyNight = view_data.IsNight;
            view_data.IsNight = true;
            if (PrepUiDuringDayMain.IsActuallyNight) // Save total group count
            {
                PrepUiDuringDayMain.LastGroupCount = view_data.ExpectedGroupCount;
                PrepUiDuringDayMain.LastExtraGroupCount = view_data.ExtraGroups;
            }
            ParametersDisplayViewGO = 
        }
    }

    [HarmonyPatch(typeof(ParametersDisplayView), nameof(ParametersDisplayView.UpdateData))]
    public class Patch_Pos_PDVVD
    {
        public static void Postfix(TextMeshPro ___CustomersPerHour, ParametersDisplayView.ViewData view_data)
        {
            if (!PrepUiDuringDayMain.IsActuallyNight) // Usually shows remaining groups to spawn. Override with total from day
            {
                if (view_data.ExtraGroups <= 0)
                    ___CustomersPerHour.text = string.Format("{0}", PrepUiDuringDayMain.LastGroupCount);
                else
                    ___CustomersPerHour.text = string.Format("{0} + {1}", PrepUiDuringDayMain.LastGroupCount, PrepUiDuringDayMain.LastExtraGroupCount);
            }
        }
    }

    public class PrepUiDuringDayMain : GenericSystemBase, IModSystem, IModInitializer
    {
        #region Pre
        // KitchenLib Stuff - Keep it for additional information once needed
        public const string MOD_GUID = "aragami.plateup.mods.prepuiduringday";
        public const string MOD_NAME = "Prep UI During Day";
        public const string MOD_VERSION = "0.1.0";
        public const string MOD_AUTHOR = "Aragami";
        public const string MOD_GAMEVERSION = ">=1.1.7";
        // Game version this mod is designed for in semver
        // e.g. ">=1.1.3" current and all future
        // e.g. ">=1.1.3 <=1.2.3" for all from/until

        // Boolean constant whose value depends on whether you built with DEBUG or RELEASE mode, useful for testing
#if DEBUG
        public const bool DEBUG_MODE = true;
#else
        public const bool DEBUG_MODE = false;
#endif
        #endregion

        #region Harmony

        private static readonly Harmony m_harmony = new(MOD_GUID);

        #endregion

        protected override void OnUpdate()
        {

        }

        public static bool IsActuallyNight; // IsActuallyPrepmode
        public static int LastGroupCount;
        public static int LastExtraGroupCount;

        public static bool ShowPrepUi = true;
        public static GameObject ParametersDisplayViewGO;
        
        public void PostActivate(Mod mod)
        {
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in useeee!");

            m_harmony.PatchAll(Assembly.GetExecutingAssembly());

            GameObject.Instantiate<PrepUiDuringDayMono>().DontDestroyOnLoad();
        }

        public void PreInject() { }

        public void PostInject() { }

        #region Logging
        public static void LogInfo(string _log) { Debug.Log($"[{MOD_NAME}]: " + _log); }
        public static void LogWarning(string _log) { Debug.LogWarning($"[{MOD_NAME}]: " + _log); }
        public static void LogError(string _log) { Debug.LogError($"[{MOD_NAME}]: " + _log); }
        public static void LogInfo(object _log) { LogInfo(_log.ToString()); }
        public static void LogWarning(object _log) { LogWarning(_log.ToString()); }
        public static void LogError(object _log) { LogError(_log.ToString()); }
        #endregion
    }
}
