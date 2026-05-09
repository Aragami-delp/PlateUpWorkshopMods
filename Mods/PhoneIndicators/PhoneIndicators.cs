using HarmonyLib;
using Kitchen;
using KitchenMods;
using System.Reflection;
using UnityEngine;
using TMPro;
using System;
using Unity.Entities;

// Namespace should have "Kitchen" in the beginning
namespace KitchenPhoneIndicators
{
    public class PhoneIndicators : GameSystemBase, IModSystem, IModInitializer
    {
        #region Pre
        // KitchenLib Stuff - Keep it for additional information once needed
        public const string MOD_GUID = "aragami.plateup.mods.phoneindicators";
        public const string MOD_NAME = "PhoneIndicators";
        public const string MOD_VERSION = "0.1.0";
        public const string MOD_AUTHOR = "Aragami";
        public const string MOD_GAMEVERSION = ">=1.2.0";
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

        public static EntityQuery ScheduledCustomers;

        protected override void OnUpdate()
        {

        }

        protected override void Initialise()
        {
            // Initialise gets called every time the game starts and when joining someone else
            if (GameObject.FindObjectOfType<PhoneIndicatorsMono>() != null)
            {
                return;
            }

            GameObject thingy = new GameObject("PhoneIndicatorsMono");
            thingy.AddComponent<PhoneIndicatorsMono>();
            ScheduledCustomers = this.GetEntityQuery((ComponentType)typeof(CScheduledCustomer));
            //GameObject.DontDestroyOnLoad(thingy); // Is done in class
        }

        public void PostActivate(Mod mod)
        {
            // Early patch might not be nessesary
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");

            m_harmony.PatchAll(Assembly.GetExecutingAssembly());
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