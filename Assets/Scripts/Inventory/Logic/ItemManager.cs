using System;
using System.Collections.Generic;
using System.Linq;
using Inventory.Item;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;

namespace Inventory.Logic
{
    public class ItemManager : MonoBehaviour
    {
        public WorldItem itemPrefab;
        public Transform itemParent;
        [SerializeField] private Transform playerTrans;

        private readonly Dictionary<string, List<SceneItem>> _itemsInSceneDict = new();
        private readonly Dictionary<string, List<SceneFurniture>> _furnitureInSceneDict = new();

        private void OnEnable()
        {
            MyEventHandler.InstantiatedItemInScene += OnInstantiatedItemInSceneEvent;
            MyEventHandler.BeforeSceneUnLoad += OnBeforeSceneUnLoad;
            MyEventHandler.AfterSceneLoaded += OnAfterSceneLoaded;
            MyEventHandler.DropItem += OnDropItemEvent;
            MyEventHandler.BuildFurniture += OnBuildFurnitureEvent;
        }

        private void OnDisable()
        {
            MyEventHandler.InstantiatedItemInScene -= OnInstantiatedItemInSceneEvent;
            MyEventHandler.BeforeSceneUnLoad -= OnBeforeSceneUnLoad;
            MyEventHandler.AfterSceneLoaded -= OnAfterSceneLoaded;
            MyEventHandler.DropItem -= OnDropItemEvent;
            MyEventHandler.BuildFurniture -= OnBuildFurnitureEvent;
        }

        private void OnBeforeSceneUnLoad()
        {
            GetAllItemsInScene();
            GetAllSceneFurniture();
        }

        private void OnAfterSceneLoaded()
        {
            FindItemParent();
            RecreateAllItemsInScene();
            RebuildFurniture();
        }

        private void FindItemParent()
            => itemParent = GameObject.FindGameObjectWithTag("ItemParent").transform;

        /// <summary>
        /// 获取ItemParent下所有的子物体，也就是场景中所有的实例化物品
        /// </summary>
        private void GetAllItemsInScene()
        {
            var sceneItems = itemParent
                .GetComponentsInChildren<WorldItem>()
                .Select(worldItem => new SceneItem()
                {
                    itemId = worldItem.itemId,
                    itemPositionInScene = new SerializableVector3(worldItem.transform.position)
                })
                .ToList();
            _itemsInSceneDict[SceneManager.GetActiveScene().name] = sceneItems;
        }

        private void RecreateAllItemsInScene()
        {
            if (_itemsInSceneDict.TryGetValue(SceneManager.GetActiveScene().name, out var sceneItems))
            {
                if (sceneItems is not null)
                {
                    //摧毁所有原来存在物品
                    foreach (var item in itemParent.GetComponentsInChildren<WorldItem>())
                        Destroy(item.gameObject);

                    //将之前保存的物品重新生成到itemParent中去
                    foreach (var item in sceneItems)
                        Instantiate(itemPrefab,
                                item.itemPositionInScene.ToVector3(),
                                quaternion.identity,
                                itemParent)
                            .Init(item.itemId);
                }
            }
        }

        /// <summary>
        /// Add all furniture to dict in current scene,
        /// if the furniture is box, need special operations
        /// </summary>
        private void GetAllSceneFurniture()
            => _furnitureInSceneDict[SceneManager.GetActiveScene().name] = FindObjectsOfType<Furniture>()
                .Select(
                    furniture =>
                        new SceneFurniture
                        {
                            itemId = furniture.itemId,
                            position = new SerializableVector3(furniture.transform.position),
                            boxIndex = furniture.TryGetComponent<Box>(out var box) ? box.boxIndex : default
                        })
                .ToList();

        private void RebuildFurniture()
        {
            if (_furnitureInSceneDict.TryGetValue(SceneManager.GetActiveScene().name, out var currentSceneFurniture))
                currentSceneFurniture.ForEach(
                    furniture =>
                    {
                        Debug.Log($"家具重建{furniture.position}");
                        var blueprint = InventoryManager.Instance.blueprintSo.GetBlueprintDetails(furniture.itemId);

                        var builtItem = Instantiate(blueprint.buildPrefab, furniture.position.ToVector3(),
                            Quaternion.identity, itemParent);

                        if (!builtItem.TryGetComponent<Box>(out var box))
                            return;
                        box.InitBox(furniture.boxIndex);
                    }
                );
        }

        #region 事件绑定

        private void OnInstantiatedItemInSceneEvent(int id, Vector3 pos)
        {
            var item = Instantiate(itemPrefab, pos, Quaternion.identity, itemParent);
            item.itemId = id;
            item.GetComponent<ItemBounce>().InitBounceItem(pos, Vector2.up);
        }

        private void OnDropItemEvent(int id, Vector3 pos, ItemType type)
        {
            //TODO:以下类型不需要抛出
            if (type is ItemType.Seed)
                return;

            var position = playerTrans.position;
            var item = Instantiate(itemPrefab, position, Quaternion.identity, itemParent);
            item.itemId = id;
            item.GetComponent<ItemBounce>().InitBounceItem(pos, (pos - position).normalized);
        }

        private void OnBuildFurnitureEvent(int itemId, Vector3 mousePosition)
        {
            Debug.Log("家具创建");
            var blueprint = InventoryManager.Instance.blueprintSo.GetBlueprintDetails(itemId);
            var furniture = Instantiate(blueprint.buildPrefab, mousePosition, Quaternion.identity, itemParent);

            if (!furniture.TryGetComponent<Box>(out var box))
                return;

            box.boxIndex = InventoryManager.Instance.BoxDataAmount;
            box.InitBox(box.boxIndex);
        }

        #endregion
    }
}