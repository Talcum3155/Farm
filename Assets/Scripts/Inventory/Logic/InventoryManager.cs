using System;
using System.Linq;
using Inventory.DataSO;
using Inventory.Item;
using UnityEngine;
using Utilities;

namespace Inventory.Logic
{
    public class InventoryManager : SingleTon<InventoryManager>
    {
        public ItemDataListSo itemDataListSo;
        public InventoryBagSo playerBagSo;

        private void OnEnable()
        {
            MyEventHandler.DropItem += OnDropItemEvent;
        }

        private void OnDisable()
        {
            MyEventHandler.DropItem -= OnDropItemEvent;
        }

        private void Start()
        {
            MyEventHandler.CallUpdateInventoryUI(InventoryLocation.Bag, playerBagSo.inventoryItems, -1);
        }

        public ItemDetails GetItemDetails(int id)
            => itemDataListSo.itemDetailsList.Find(i => i.itemID == id);

        /// <summary>
        /// 将捡取的世界物品添加到背包中，
        /// 考虑背包中是否已经有该物品以及背包中是否有空格
        /// </summary>
        /// <param name="item">处于世界中的物品的详细数据</param>
        /// <param name="toDestroy">是否摧毁该物品</param>
        public void AddItem(WorldItem item, bool toDestroy)
        {
            var bagFull = true;
            var playerBag = playerBagSo.inventoryItems;

            for (var i = 0; i < playerBag.Count; i++)
            {
                //找到了背包中的空格或者背包中已经拥有该物品
                if (playerBag[i].itemId != 0
                    && playerBag[i].itemId != item.itemId) continue;

                bagFull = false;
                //当背包中没有该物品时，该格子的数量默认为0
                playerBag[i] = new InventoryItem()
                    { itemId = item.itemId, itemAmount = playerBag[i].itemAmount + item.itemAmount };
                MyEventHandler.CallUpdateInventoryUI(InventoryLocation.Bag, playerBagSo.inventoryItems, i);
                break;
            }

            if (!bagFull && toDestroy)
                Destroy(item.gameObject);
        }

        private int GetItemIndexInBag(int itemId)
        {
            for (var i = 0; i < playerBagSo.inventoryItems.Count; i++)
                if (playerBagSo.inventoryItems[i].itemId == itemId)
                    return i;

            return -1;
        }

        public bool CheckBagSpace()
            => InventoryManager.Instance.playerBagSo.inventoryItems.All(inventoryItem => inventoryItem.itemId != 0);

        /// <summary>
        /// 使用元组交换两个索引的位置
        /// </summary>
        /// <param name="formIndex">从何处交换</param>
        /// <param name="targetIndex">需要和谁交换</param>
        public void SwapItem(int formIndex, int targetIndex)
        {
            (playerBagSo.inventoryItems[formIndex], playerBagSo.inventoryItems[targetIndex]) = (
                playerBagSo.inventoryItems[targetIndex], playerBagSo.inventoryItems[formIndex]);
            MyEventHandler.CallUpdateInventoryUI(InventoryLocation.Bag, playerBagSo.inventoryItems, formIndex);
            MyEventHandler.CallUpdateInventoryUI(InventoryLocation.Bag, playerBagSo.inventoryItems, targetIndex);
        }

        /// <summary>
        /// 移除指定数量的背包物品
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="removeAmount"></param>
        private void RemoveItem(int itemId, int removeAmount)
        {
            var index = GetItemIndexInBag(itemId);
            Debug.Log($"索引{index}");
            if (playerBagSo.inventoryItems[index].itemAmount > removeAmount)
            {
                playerBagSo.inventoryItems[index] = new InventoryItem()
                {
                    itemId = itemId,
                    itemAmount = playerBagSo.inventoryItems[index].itemAmount - removeAmount
                };
                MyEventHandler.CallUpdateInventoryUI(InventoryLocation.Bag, playerBagSo.inventoryItems, index);
                return;
            }

            playerBagSo.inventoryItems[index] = new InventoryItem();
            //让动画回归默认状态
            MyEventHandler.CallSelectedItem(null, false);
            MyEventHandler.CallUpdateInventoryUI(InventoryLocation.Bag, playerBagSo.inventoryItems, index);
        }

        #region 事件绑定

        private void OnDropItemEvent(int itemId, Vector3 pos)
        {
            Debug.Log($"减少 {itemId}");
            RemoveItem(itemId, 1);
        }

        #endregion
    }
}