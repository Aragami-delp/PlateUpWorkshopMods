using Kitchen;
using KitchenMods;
using UnityEngine;

namespace PhoneIndicators
{
    public class PhoneIndicators : GameSystemBase, IModSystem
    {
        public const string MOD_GUID = "aragami.plateup.mods.phoneindicators";
        public const string MOD_NAME = "PhoneIndicators";
        public const string MOD_VERSION = "0.1.0";
        public const string MOD_AUTHOR = "Aragami";

        protected override void Initialise()
        {
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");
        }

        protected override void OnUpdate()
        {
        }
        
        #region Logging
        internal static void LogInfo(string _log) { Debug.Log($"[{MOD_NAME}] " + _log); }
        internal static void LogWarning(string _log) { Debug.LogWarning($"[{MOD_NAME}] " + _log); }
        internal static void LogError(string _log) { Debug.LogError($"[{MOD_NAME}] " + _log); }
        #endregion
    }
}