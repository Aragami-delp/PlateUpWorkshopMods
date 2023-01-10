using Kitchen;
using KitchenLib;
using KitchenMods;
using System.Reflection;
using System.Collections;
using UnityEngine;
using Controllers;
using KitchenData;
using HarmonyLib;
using Kitchen.Modules;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System;
using System.Linq;
using TMPro;
// Namespace should have "Kitchen" in the beginning
namespace KitchenFullKeyboardRebind
{
    public class Mod : BaseMod
    {
        public const string MOD_GUID = "aragami.plateup.mods.fullkeyboardrebind";
        public const string MOD_NAME = "FullKeyboardRebind";
        public const string MOD_VERSION = "0.1.0";
        public const string MOD_AUTHOR = "Aragami";
        public const string MOD_GAMEVERSION = ">=1.1.2";
        // Game version this mod is designed for in semver
        // e.g. ">=1.1.1" current and all future
        // e.g. ">=1.1.1 <=1.2.3" for all from/until

        public Mod() : base(MOD_GUID, MOD_NAME, MOD_AUTHOR, MOD_VERSION, MOD_GAMEVERSION, Assembly.GetExecutingAssembly()) { }

        protected override void Initialise()
        {
            base.Initialise();
            // For log file output so the official plateup support staff can identify if/which a mod is being used
            LogWarning($"{MOD_GUID} v{MOD_VERSION} in use!");
        }

        protected override void OnUpdate()
        {

        }

        #region Logging
        // You can remove this, I just prefer a more standardized logging
        public static void LogInfo(string _log) { Debug.Log($"{MOD_NAME} " + _log); }
        public static void LogWarning(string _log) { Debug.LogWarning($"{MOD_NAME} " + _log); }
        public static void LogError(string _log) { Debug.LogError($"{MOD_NAME} " + _log); }
        public static void LogInfo(object _log) { LogInfo(_log.ToString()); }
        public static void LogWarning(object _log) { LogWarning(_log.ToString()); }
        public static void LogError(object _log) { LogError(_log.ToString()); }
        #endregion

        [HarmonyPatch(typeof(ControlRebindElement), nameof(ControlRebindElement.Setup))]
        class ControlRebindElement_Setup_Patch
        {

            [HarmonyPostfix]
            static void Postfix(ControlRebindElement __instance, int player)
            {
                if (InputSourceIdentifier.DefaultInputSource.GetCurrentController(player) == ControllerType.Keyboard)
                {
                    __instance.AddRebindOption("Movement Up", "Movement_Up");
                    __instance.AddRebindOption("Movement Left", "Movement_Left");
                    __instance.AddRebindOption("Movement Down", "Movement_Down");
                    __instance.AddRebindOption("Movement Right", "Movement_Right");
                }
            }
        }

        static public void StartInteractiveRebind(InputAction _action, int _bindingIndex, Action<RebindResult> _callback)
        {
            //yield return new WaitForSeconds(.1f);
            _action.Disable();
            var m_RebindOperation = _action.PerformInteractiveRebinding(_bindingIndex)
                       // To avoid accidental input from mouse motion
                       .WithControlsExcluding("<Mouse>")
                       .WithControlsExcluding("Mouse")
                       .WithCancelingThrough("<Keyboard>/escape")
                       .OnMatchWaitForAnother(0.2f)
                       .Start().OnCancel(op =>
                       {
                           _action.Enable();
                           _callback?.Invoke(RebindResult.Cancelled);
                       }).OnComplete(op =>
                       {
                           _action.Enable();
                           _callback?.Invoke(RebindResult.Success);
                       });
        }

        [HarmonyPatch(typeof(ControlRebindElement), "StartRebind")]
        class ControlRebindElement_StartRebind_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(ControlRebindElement __instance, string action, ModuleList ___ModuleList, LabelElement ___RebindMessage, PanelElement ___Panel)
            {
                String[] strParts = action.Split('_');
                if (strParts.Length == 2 && strParts[0] == "Movement")
                {
                    InputAction movementAction = null;
                    foreach (var foundAction in InputSystem.ListEnabledActions())
                    {
                        if (foundAction.name == "Movement")
                        {
                            movementAction = foundAction;
                        }
                    }
                    int bindingIndex = -1;
                    switch (strParts[1])
                    {
                        case "Up":
                            bindingIndex = 1;
                            break;
                        case "Down":
                            bindingIndex = 2;
                            break;
                        case "Left":
                            bindingIndex = 3;
                            break;
                        case "Right":
                            bindingIndex = 4;
                            break;
                    }

                    #region Original
                    foreach (ModuleInstance module1 in ___ModuleList.Modules)
                    {
                        if (module1.Module is RemapElement module)
                            module.gameObject.SetActive(false);
                    }
                    ___RebindMessage.gameObject.SetActive(true);
                    ___RebindMessage.SetLabel(GameData.Main.GlobalLocalisation["REBIND_NOW"]);
                    if (___Panel.isActiveAndEnabled)
                        ___Panel.SetTarget((IModule)___RebindMessage);
                    #endregion

                    #region TriggerRebind
                    StartInteractiveRebind(movementAction, bindingIndex, (Action<RebindResult>)(result =>
                    {
                        switch (result)
                        {
                            case RebindResult.Success:
                                // Not in first mod version - Saving can be done later - overwrite would be better anyways
                                break;
                            case RebindResult.Fail:
                                // Not in first mod version - Restart binding process
                                return;
                            case RebindResult.RejectedInUse:
                                ___RebindMessage.SetLabel(GameData.Main.GlobalLocalisation["REBIND_IN_USE"]);
                                return;
                        }
                        MethodInfo endRebind = __instance.GetType().GetMethod("EndRebind", BindingFlags.NonPublic | BindingFlags.Instance);
                        endRebind.Invoke(__instance, new object[0] { });
                    }));
                    #endregion
                    return false; // Skip original and other prefixes
                }
                return true; // Do original
            }
        }

        static public string GetBindingNameByActionIndex(InputAction _action, int _bindingIndex)
        {
            string[] strArray = _action.bindings[_bindingIndex].effectivePath.Split('/');
            return strArray[strArray.Length - 1];
        }

        [HarmonyPatch(typeof(InputSource), nameof(InputSource.GetBindingName))]
        class InputSource_GetBindingName_Patch
        {
            [HarmonyPrefix]
            static bool Prefix(InputSource __instance, int player, string action_name, ref string __result, Dictionary<int, PlayerData> ___Players)
            {
                String[] strParts = action_name.Split('_');
                if (strParts.Length == 2 && strParts[0] == "Movement")
                {
                    PlayerData playerData;
                    if (!___Players.TryGetValue(player, out playerData))
                    {
                        __result = "?";
                        return false;
                    }
                    InputAction action = playerData.InputData.Map.FindAction(strParts[0], false);
                    int bindingIndex = -1;
                    switch (strParts[1])
                    {
                        case "Up":
                            bindingIndex = 1;
                            break;
                        case "Down":
                            bindingIndex = 2;
                            break;
                        case "Left":
                            bindingIndex = 3;
                            break;
                        case "Right":
                            bindingIndex = 4;
                            break;
                    }
                    __result = action == null ? "?" : GetBindingNameByActionIndex(action, bindingIndex);
                    return false; // Skip original and other prefixes
                }
                return true; // Do original
            }
        }

        [HarmonyPatch(typeof(RemapElement), "HandleBindingChange")]
        class RemapElement_HandleBindingChange_Patch
        {
            [HarmonyPrefix]
            static void Prefix(RemapElement __instance, string ___Action, string s)
            {
                Debug.LogError("___Action: " + ___Action);
                Debug.LogError("s: " + s);
                //String[] strParts = ___Action.Split('_');
                //if (strParts.Length == 2 && strParts[0] == "Movement")
                //{
                //    return false; // Skip original and other prefixes
                //}
                //return true; // Do original
            }
        }
    }
}
