using Kitchen;
using KitchenMods;
using UnityEngine;
using HarmonyLib;
using System.Reflection;
using TMPro;
using UnityEngine.InputSystem;
using System;

// Namespace should have "Kitchen" in the beginning
namespace KitchenSmartNoClip
{
    public class SmartNoClip : GenericSystemBase, IModSystem, IModInitializer
    {
        #region Pre
        // KitchenLib Stuff - Keep it for additional information once needed
        public const string MOD_GUID = "aragami.plateup.mods.smartnoclip";
        public const string MOD_NAME = "SmartNoClip";
        public const string MOD_VERSION = "0.1.0";
        public const string MOD_AUTHOR = "Aragami";
        public const string MOD_GAMEVERSION = ">=1.1.8";
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

        private Persistence m_persistence;

        protected override void OnUpdate()
        {

        }

        protected override void Initialise()
        {
            // Initialise gets called every time the game starts and when joining someone else
            if (GameObject.FindObjectOfType<SmartNoClipMono>() != null)
            {
                return;
            }

            m_persistence = new Persistence();

            GameObject thingy = new GameObject("SmartNoClipMono");
            thingy.AddComponent<SmartNoClipMono>();
            //GameObject.DontDestroyOnLoad(thingy); // Is done in class
        }

        public void PostActivate(Mod mod)
        {
            // Early patch to patch InputSource before the first device connects
            m_harmony.PatchAll(Assembly.GetExecutingAssembly());

            LogWarning($"{MOD_GUID} v{MOD_VERSION} in useeee!");
        }

        public static void InputActionsPatch_Action_Started(InputAction.CallbackContext obj)
        {
            SmartNoClipMono.Instance?.ManualNoClipOverride();
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
        public static void LogInfo(Exception _log) { LogInfo(_log.Source + " " + _log.Message); }
        public static void LogWarning(Exception _log) { LogWarning(_log.Source + " " + _log.Message); }
        public static void LogError(Exception _log) { LogError(_log.Source + " " + _log.Message); }

        #endregion
    }
}
