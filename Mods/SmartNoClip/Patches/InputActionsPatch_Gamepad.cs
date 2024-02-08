using HarmonyLib;
using Controllers;
using KitchenData;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities.UniversalDelegates;
using System.Reflection;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;
using Kitchen.Modules;

namespace KitchenSmartNoClip
{
    [HarmonyPatch(typeof(Maps), nameof(Maps.NewGamepad))]
    public class InputActionsPatch_Gamepad
    {
        public const string SmartNoClipInputActionName = "SmartNoClip"; // Not quite sure how efficient this is saved by plateup itself. So dont change this string under any circumstances for this mod. (No errors when this mod is uninstalled.

        [HarmonyPostfix]
        public static void Actions_AddNoClipAction_Gamepad(ref InputActionMap __result)
        {
            __result.AddAction(SmartNoClipInputActionName, InputActionType.Button);
            __result.FindAction(SmartNoClipInputActionName, false).AddBinding("<Gamepad>/select");
            __result.FindAction(SmartNoClipInputActionName, false).started += SmartNoClip.InputActionsPatch_Action_Started;
        }    
    }

    [HarmonyPatch(typeof(Maps), nameof(Maps.NewKeyboard))]
    public class InputActionsPatch_Keyboard
    {
        [HarmonyPostfix]
        public static void Actions_AddNoClipAction_Keyboard(ref InputActionMap __result)
        {
            __result.AddAction(InputActionsPatch_Gamepad.SmartNoClipInputActionName, InputActionType.Button);
            __result.FindAction(InputActionsPatch_Gamepad.SmartNoClipInputActionName, false).AddBinding("<Keyboard>/n");
            __result.FindAction(InputActionsPatch_Gamepad.SmartNoClipInputActionName, false).started += SmartNoClip.InputActionsPatch_Action_Started;
        }
    }

    [HarmonyPatch(typeof(ControlRebindElement), nameof(ControlRebindElement.Setup))]
    public class InputActionsPatch_Rebind
    {
        [HarmonyPostfix]
        public static void Rebind_AddRebind(ControlRebindElement __instance, PanelElement ___Panel, ModuleList ___ModuleList)
        {
            SmartNoClip.LogWarning("Localization error is from mod SmartNoClip. It is not easily avoidable, but it also doesn't cause any problems.");
            __instance.AddRebindOption("NoClip", InputActionsPatch_Gamepad.SmartNoClipInputActionName);
            ___Panel.SetTarget(___ModuleList);
        }
    }
}
