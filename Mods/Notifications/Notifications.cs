using HarmonyLib;
using Kitchen;
using KitchenMods;
using KitchenSmartNoClip;
using System.Reflection;
using UnityEngine;

namespace Notifications
{
    public class Notifications : GenericSystemBase, IModSystem, IModInitializer
    {
        #region Pre
        public const string MOD_GUID = "aragami.plateup.mods.notifications";
        public const string MOD_NAME = "Notifications";
        public const string MOD_VERSION = "0.1.0";
        public const string MOD_AUTHOR = "Aragami";
        public const string MOD_GAMEVERSION = ">=1.1.4";
#if DEBUG
        public const bool DEBUG_MODE = true;
#else
        public const bool DEBUG_MODE = false;
#endif
        #endregion

        #region Harmony

        //private static readonly Harmony m_harmony = new(MOD_GUID);

        #endregion

        public NotificationView NotificationView;

        protected override void OnUpdate()
        {

        }

        protected override void Initialise()
        {
            // Initialise gets called every time the game starts and when joining someone else
            if (GameObject.FindObjectOfType<NotificationView>() != null)
            {
                return;
            }

            Transform uIContainer = GetUIContainer();
            if (uIContainer != null)
            {
                GameObject thingy = new GameObject("Notifications");
                thingy.transform.SetParent(uIContainer, false);
                GameObject container = new GameObject("Container");
                container.transform.SetParent(thingy.transform, false);
                NotificationView = thingy.AddComponent<NotificationView>();
                NotificationView.Container = container;
                NotificationView.ButtonContainer = container.transform;
            }
        }

        public void PostActivate(Mod mod)
        {
            // Early patch to patch InputSource before the first device connects
            //m_harmony.PatchAll(Assembly.GetExecutingAssembly());

            LogWarning($"{MOD_GUID} v{MOD_VERSION} in useeee!");
        }

        public void PreInject() { }

        public void PostInject() { }

        private static Transform GetUIContainer()
        {
            Transform uiCamera = Camera.main.transform.parent.Find("UI Camera");

            if (uiCamera != null)
            {
                Transform uiContainer = uiCamera.Find("UI Container");
                return uiContainer != null ? uiContainer : null;
            }
            else
            {
                return null;
            }
        }

        #region Logging
        public static void LogInfo(string _log) { Debug.Log($"[{MOD_NAME}] " + _log); }
        public static void LogWarning(string _log) { Debug.LogWarning($"[{MOD_NAME}] " + _log); }
        public static void LogError(string _log) { Debug.LogError($"[{MOD_NAME}] " + _log); }
        public static void LogInfo(object _log) { LogInfo(_log.ToString()); }
        public static void LogWarning(object _log) { LogWarning(_log.ToString()); }
        public static void LogError(object _log) { LogError(_log.ToString()); }
        public static string GetPath(Transform current)
        {
            if (current.parent == null)
                return "/" + current.name;
            return GetPath(current.parent) + "/" + current.name;
        }
        #endregion
    }
}
