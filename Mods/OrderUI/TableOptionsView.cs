using Kitchen;
using KitchenData;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities.CodeGeneratedJobForEach;
using Unity.Entities;
using Unity.Profiling;
using UnityEngine;

namespace OrderUI
{
    public class TableOptionsView : UpdatableObjectView<TwitchOptionsView.ViewData>
    {
        [SerializeField]
        [Header("Configuration")]
        private float Height;
        [Header("References")]
        [SerializeField]
        private GameObject Container;
        [SerializeField]
        private Renderer Renderer;
        [SerializeField]
        private TextMeshPro Text;
        [SerializeField]
        private GameObject ItemPrefab;
        [Header("State")]
        private TwitchOptionsView.ViewData Data;
        private static readonly int Image = Shader.PropertyToID("_Image");

        protected override void UpdateData(TwitchOptionsView.ViewData view_data)
        {
            Vector3 localPosition = this.Container.transform.localPosition;
            localPosition.y = this.Height * (float)view_data.Index;
            this.Container.transform.localPosition = localPosition;
            if ((UnityEngine.Object)this.Renderer == (UnityEngine.Object)null)
                return;
            this.Text.text = string.Format("!order {0}", (object)view_data.Index);
            this.Renderer.gameObject.SetActive(true);
            this.Renderer.material.SetTexture(TableOptionsView.Image, (Texture)PrefabSnapshot.GetFoodSnapshot(this.ItemPrefab, new ItemView.ViewData()
            {
                ItemID = view_data.ItemID,
                Components = view_data.ItemComponents
            }));
        }

        [MessagePackObject(false)]
        public struct ViewData : IViewData, IViewResponseData, IViewData.ICheckForChanges<TableOptionsView.ViewData>
        {
            [Key(0)]
            public int ItemID;
            [Key(1)]
            public ItemList ItemComponents;
            [Key(2)]
            public int Index;

            public bool IsChangedFrom(TableOptionsView.ViewData check) => this.ItemID != check.ItemID || !this.ItemComponents.IsEquivalent(check.ItemComponents) || this.Index != check.Index;
        }
    }
}
