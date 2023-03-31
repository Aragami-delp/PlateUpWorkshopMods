using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Entities;
using Kitchen;
using TMPro;

namespace OrderUI
{
    public class OrderUIScript : MonoBehaviour
    {
        public Entity CorrespondingGroup;

        private float Height;
        private GameObject Container;
        private Renderer Renderer;
        private TextMeshPro Text;
        private GameObject ItemPrefab;
        private static readonly int Image = Shader.PropertyToID("_Image");

        public void Init(float _height, GameObject _container, Renderer _renderer, TextMeshPro _text, GameObject _itemPrefab)
        {
            Height = _height;
            Container = _container;
            Renderer = _renderer;
            Text = _text;
            ItemPrefab = _itemPrefab;
        }
    }
}
