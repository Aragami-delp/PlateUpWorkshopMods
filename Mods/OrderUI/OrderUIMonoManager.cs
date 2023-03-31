using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Kitchen;
using TMPro;
using Unity.Entities;

namespace OrderUI
{
    public class OrderUIMonoManager
    {
        /// <summary>
        /// Disabled initially
        /// </summary>
        private static GameObject m_orderUIScriptPrefab; // Might as well be the OrderUIScript MonoComponent, not the GameObject
        private static OrderUIMonoManager s_instance;

        public static OrderUIMonoManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = new OrderUIMonoManager();
                }
                return s_instance;
            }
        }

        public void NewGroupOrder(Entity _groupEntity)
        {
            DynamicBuffer<CWaitingForItem> cWaitingForItems = World.DefaultGameObjectInjectionWorld.EntityManager.GetBuffer<CWaitingForItem>(_groupEntity);
            Debug.LogError(cWaitingForItems.Length);
            for (int i = 0; i < cWaitingForItems.Length; i++)
            {
                Debug.LogError(cWaitingForItems[i].Item.ToString());
            }
        }

        public void GroupOrderFinished(Entity _groupEntity)
        {

        }

        public OrderUIMonoManager()
        {
            m_orderUIScriptPrefab = Helper.Copy(Kitchen.ViewType.TwitchOrderOption, "OrderUIPrefab", PrepareUIPrefab);
        }
        public static void PrepareUIPrefab(GameObject _prefab)
        {
            _prefab.SetActive(false);
            OrderUIScript newScript = _prefab.AddComponent<OrderUIScript>();
            TwitchOptionsView oldScript = _prefab.GetComponent<TwitchOptionsView>();
            newScript.Init(
                (float)typeof(TwitchOptionsView).GetField("Height", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(oldScript),
                (GameObject)typeof(TwitchOptionsView).GetField("Container", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(oldScript),
                (Renderer)typeof(TwitchOptionsView).GetField("Renderer", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(oldScript),
                (TextMeshPro)typeof(TwitchOptionsView).GetField("Text", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(oldScript),
                (GameObject)typeof(TwitchOptionsView).GetField("ItemPrefab", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(oldScript)
            );

            GameObject.Destroy(oldScript);
        }
    }
}
