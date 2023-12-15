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
    public class PrepUiDuringDayMono : MonoBehaviour
    {
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.I))
            {
                PrepUiDuringDayMain.ShowUiDuringDay = !PrepUiDuringDayMain.ShowUiDuringDay;
                PrepUiDuringDayMain.ShowPrepUi();
                PrepUiDuringDayMain.LogInfo("ShowUi: " + PrepUiDuringDayMain.ShowUiDuringDay);
            }
        }
    }
}