using Kitchen;
using KitchenData;
using KitchenLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace OrderUI
{
    public static class Helper
    {
        private static readonly Dictionary<string, GameObject> PrefabCache = new();

        /// <summary>
        /// Copies a prefab of a given View
        /// </summary>
        /// <param name="viewType">Type of View to copy</param>
        /// <param name="copyName">Name of the new GameObject</param>
        /// <param name="modifier">Action after instantiation</param>
        /// <returns></returns>
        public static GameObject Copy(ViewType viewType, string copyName, Action<GameObject> modifier = null)
        {
            if (!PrefabCache.ContainsKey($"View {(int)viewType} {copyName}"))
            {
                var prefab = OrderUIMain.VanillaAssetDirectory.ViewPrefabs[viewType];
                if (prefab == null)
                {
                    OrderUIMain.LogWarning($"Existing view prefab with view type {(int)viewType} not found.");
                    return null;
                }

                var parent = GameObject.Find("ViewPrefabs");
                if (parent == null)
                {
                    parent = new GameObject("ViewPrefabs");
                    parent.transform.localPosition = Vector3.positiveInfinity;
                    parent.SetActive(false);
                }

                var copy = UnityEngine.Object.Instantiate(prefab, parent.transform);
                copy.name = $"View {(int)viewType} {copyName}";
                if (modifier != null)
                {
                    modifier.Invoke(copy);
                }
                PrefabCache.Add($"View {(int)viewType} {copyName}", copy);
            }

            return PrefabCache[$"View {(int)viewType} {copyName}"];
        }
    }
}
