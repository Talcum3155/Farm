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

        private readonly Dictionary<string, List<SceneItem>> _itemsInSceneDictionary = new();

        private void OnEnable()
        {
            MyEventHandler.InstantiatedItemInScene += OnInstantiatedItemInSceneEvent;
            MyEventHandler.BeforeSceneUnLoad += OnBeforeSceneUnLoad;
            MyEventHandler.AfterSceneLoaded += OnAfterSceneLoaded;
            MyEventHandler.DropItem += OnDropItemEvent;
        }

        private void OnDisable()
        {
            MyEventHandler.InstantiatedItemInScene -= OnInstantiatedItemInSceneEvent;
            MyEventHandler.BeforeSceneUnLoad -= OnBeforeSceneUnLoad;
            MyEventHandler.AfterSceneLoaded -= OnAfterSceneLoaded;
            MyEventHandler.DropItem -= OnDropItemEvent;
        }

        private void OnBeforeSceneUnLoad()
        {
            GetAllItemsInScene();
        }

        private void OnAfterSceneLoaded()
        {
            FindItemParent();
            RecreateAllItemsInScene();
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
            _itemsInSceneDictionary[SceneManager.GetActiveScene().name] = sceneItems;
        }

        private void RecreateAllItemsInScene()
        {
            if (_itemsInSceneDictionary.TryGetValue(SceneManager.GetActiveScene().name, out var sceneItems))
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

        #region 事件绑定

        private void OnInstantiatedItemInSceneEvent(int id, Vector3 pos)
        {
            Instantiate(itemPrefab, pos, Quaternion.identity, itemParent).itemId = id;
        }

        private void OnDropItemEvent(int id, Vector3 pos)
        {
            var position = playerTrans.position;
            var item = Instantiate(itemPrefab, position, Quaternion.identity, itemParent);
            item.itemId = id;
            item.GetComponent<ItemBounce>().InitBounceItem(pos, (pos - position).normalized);
        }

        #endregion
    }
}