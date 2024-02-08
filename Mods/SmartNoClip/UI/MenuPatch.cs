using HarmonyLib;
using Kitchen;
using Kitchen.Modules;
using KitchenData;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities.UniversalDelegates;
using System.Reflection;

namespace KitchenSmartNoClip
{
    [HarmonyPatch(typeof(MainMenu), nameof(MainMenu.Setup))]
    class MenuPatch
    {
        [HarmonyPrefix]
        public static void Setup_AddSmartNoClipMenu(MainMenu __instance)
        {
                MethodInfo m_addButtonMenu = Helper.GetMethod(__instance.GetType(), "AddSubmenuButton");
                m_addButtonMenu.Invoke(__instance, new object[3] { "SmartNoClip", typeof(SmartNoClipOptionsMenu), false });
        }
    }

    [HarmonyPatch(typeof(PlayerPauseView), "SetupMenus")]
    class PausePatch
    {
        [HarmonyPrefix]
        public static void SetupMenus_AddSmartNoClipMenu(PlayerPauseView __instance)
        {
            try
            {
                ModuleList moduleList = (ModuleList)__instance.GetType().GetField("ModuleList", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(__instance);
                MethodInfo mInfo = Helper.GetMethod(__instance.GetType(), "AddMenu");

                mInfo.Invoke(__instance, new object[2] { typeof(SmartNoClipOptionsMenu), new SmartNoClipOptionsMenu(__instance.ButtonContainer, moduleList) });
            }
            catch (Exception e)
            {
                SmartNoClip.LogError(e.InnerException.Message + "\n" + e.StackTrace);
                throw;
            }
        }
    }

    public class SmartNoClipOptionsMenu : Menu<PauseMenuAction>
    {
        //public Option<bool> Option_General_Mod_Active;
        public Option<bool> Option_Active_Prep;
        public Option<bool> Option_Active_Day;
        public Option<bool> Option_Active_HQ;
        public Option<float> Option_Speed_Value;
        public Option<bool> Option_Allow_Players_Outside;

        public SmartNoClipOptionsMenu(Transform container, ModuleList module_list) : base(container, module_list) { }

        public override void Setup(int player_id)
        {
            AddLabel("Active in Prep");
            Add(NewBoolOption("bActive_Prep"));

            AddLabel("Active during Day");
            Add(NewBoolOption("bActive_Day"));

            AddLabel("Active in HQ");
            Add(NewBoolOption("bActive_HQ"));

            AddLabel("Speed during NoClip");
            Add(NewFloatOption("fSpeed_Value", new List<float> { 1f, 1.5f, 2f, 2.5f, 3f }));

            AddLabel("Allow other players out of bounce");
            Add(NewBoolOption("bAllow_Players_Outside"));

            New<SpacerElement>();

            AddButton(Localisation["MENU_BACK_SETTINGS"], (Action<int>)(i => RequestPreviousMenu()));
        }

        private Option<float> NewFloatOption(string _settingsString, List<float> _possibleValues)
        {
            List<string> localization = new() { this.Localisation["SETTING_DISABLED"] };
            for (int i = 1; i < _possibleValues.Count; i++)
            {
                localization.Add(_possibleValues[i].ToString());
            }
            Option<float> enableOption = new Option<float>(
                    _possibleValues
                , Persistence.Instance[_settingsString].FloatValue
                , localization
                , null);
            enableOption.OnChanged += delegate (object _, float value)
            {
                Persistence.Instance[_settingsString].SetValue(value);
                SmartNoClipMono.Instance?.PostConfigUpdated(_settingsString);
            };
            return enableOption;
        }

        private Option<bool> NewBoolOption(string _settingsString)
        {
            Option<bool> boolOption = new Option<bool>(
                    new List<bool> { false, true }
                , Persistence.Instance[_settingsString].BoolValue
                , new List<string> { this.Localisation["SETTING_DISABLED"], this.Localisation["SETTING_ENABLED"] }
                , null);
            boolOption.OnChanged += delegate (object _, bool value)
            {
                Persistence.Instance[_settingsString].SetValue(value);
                SmartNoClipMono.Instance?.PostConfigUpdated(_settingsString);
            };
            return boolOption;
        }
    }
}
