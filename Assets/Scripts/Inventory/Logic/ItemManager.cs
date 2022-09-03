using System;
using Inventory.Item;
using UnityEngine;
using Utilities;

namespace Inventory.Logic
{
    public class ItemManager : MonoBehaviour
    {
        public WorldItem itemPrefab;
        public Transform itemParent;

        private void Start()
        {
            itemParent = GameObject.FindGameObjectWithTag("ItemParent").transform;
        }

        private void OnEnable()
        {
            MyEventHandler.InstantiatedItemInScene += OnInstantiatedItemInScene;
        }

        private void OnDisable()
        {
            MyEventHandler.InstantiatedItemInScene -= OnInstantiatedItemInScene;
        }

        private void OnInstantiatedItemInScene(int id, Vector3 pos)
        {
            Instantiate(itemPrefab, pos, Quaternion.identity, itemParent).itemId = id;
        }
    }
}