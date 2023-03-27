using KitchenLib;
using KitchenLib.Event;
using KitchenMods;
using System.Reflection;
using UnityEngine;

// Namespace should have "Kitchen" in the beginning
namespace KitchenOrderUI
{
    public class OrderUI : BaseMod, IModSystem
    {
        public const string MOD_GUID = "aragami.plateup.mods.orderui";
        public const string MOD_NAME = "OrderUI";
        public const string MOD_VERSION = "0.1.0";
        public const string MOD_AUTHOR = "Aragami";
        public const string MOD_GAMEVERSION = ">=1.1.4";

#if DEBUG
        public const bool DEBUG_MODE = true;
#else
        public const bool DEBUG_MODE = false;
#endif

        public OrderUI() : base(MOD_GUID, MOD_NAME, MOD_AUTHOR, MOD_VERSION, MOD_GAMEVERSION, Assembly.GetExecutingAssembly()) { }

        protected override void OnInitialise()
        {
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");
        }

        protected override void OnUpdate()
        {

        }

        protected override void OnPostActivate(KitchenMods.Mod mod)
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
