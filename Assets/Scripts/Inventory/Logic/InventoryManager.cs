using System;
using System.Collections.Generic;
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
        public InventoryBagSo currentBoxBag;
        public BlueprintDataSo blueprintSo;

        [Header("交易")] public int money;

        private readonly Dictionary<string, List<InventoryItem>> _boxDataDict = new();

        public int BoxDataAmount => _boxDataDict.Count;

        private void OnEnable()
        {
            MyEventHandler.DropItem += OnDropItemEvent;
            MyEventHandler.HarvestAtPlayerPosition += OnHarvestAtPlayerPositionEvent;
            MyEventHandler.BuildFurniture += OnBuildFurnitureEvent;
            MyEventHandler.BagBaseOpen += OnBagBaseOpenEvent;
        }

        private void OnDisable()
        {
            MyEventHandler.DropItem -= OnDropItemEvent;
            MyEventHandler.HarvestAtPlayerPosition -= OnHarvestAtPlayerPositionEvent;
            MyEventHandler.BuildFurniture -= OnBuildFurnitureEvent;
            MyEventHandler.BagBaseOpen -= OnBagBaseOpenEvent;
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

        /// <summary>
        /// Get index from bag if own this item.
        /// if there is no this item and
        /// don't need free space index in bag,
        /// it will return -1, otherwise it will
        /// return free space index
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="freeSpaceIndex">whether get free space in bag</param>
        /// <returns></returns>
        private int GetItemIndexInBag(int itemId, bool freeSpaceIndex)
        {
            for (var i = 0; i < playerBagSo.inventoryItems.Count; i++)
            {
                if (playerBagSo.inventoryItems[i].itemId == itemId)
                    return i;
                if (freeSpaceIndex && playerBagSo.inventoryItems[i].itemId < 1000)
                    return i;
            }

            return -1;
        }

        private void AddItemAtIndex(ItemDetails details, int index, int amount)
            =>
                playerBagSo.inventoryItems[index] = new InventoryItem
                {
                    itemId = details.itemID,
                    itemAmount = amount
                };

        /// <summary>
        /// If bag is full, return true
        /// </summary>
        /// <returns></returns>
        public bool CheckBagFull()
            => playerBagSo.inventoryItems.All(inventoryItem => inventoryItem.itemId != 0);

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
        /// Swap Item between two different bags
        /// </summary>
        /// <param name="sourceLocation"></param>
        /// <param name="sourceIndex"></param>
        /// <param name="targetLocation"></param>
        /// <param name="targetIndex"></param>
        public void SwapItem(InventoryLocation sourceLocation, int sourceIndex, InventoryLocation targetLocation,
            int targetIndex)
        {
            var sourceItemList = GetItemList(sourceLocation);
            var targetItemList = GetItemList(targetLocation);

            if (targetIndex >= targetItemList.Count) return;

            var sourceItem = sourceItemList[sourceIndex];
            var targetItem = targetItemList[targetIndex];

            if (targetItem.itemId >= 1000 && sourceItem.itemId != targetItem.itemId)
            {
                sourceItemList[sourceIndex] = targetItem;
                targetItemList[targetIndex] = sourceItem;
            }
            else if (sourceItem.itemId == targetItem.itemId)
            {
                targetItem.itemAmount += sourceItem.itemAmount;
                targetItemList[targetIndex] = targetItem;
                sourceItemList[sourceIndex] = new InventoryItem();
            }
            else
            {
                targetItemList[targetIndex] = sourceItem;
                sourceItemList[sourceIndex] = new InventoryItem();
            }

            MyEventHandler.CallUpdateInventoryUI(sourceLocation, sourceItemList, sourceIndex);
            MyEventHandler.CallUpdateInventoryUI(targetLocation, targetItemList, targetIndex);
        }

        /// <summary>
        /// 移除指定数量的背包物品
        /// </summary>
        /// <param name="itemId"></param>
        /// <param name="removeAmount"></param>
        private void RemoveItem(int itemId, int removeAmount)
        {
            var index = GetItemIndexInBag(itemId, false);
            if (playerBagSo.inventoryItems[index].itemAmount > removeAmount)
            {
                playerBagSo.inventoryItems[index] = new InventoryItem
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

        private void OnDropItemEvent(int itemId, Vector3 pos, ItemType type)
        {
            RemoveItem(itemId, 1);
        }

        private void OnHarvestAtPlayerPositionEvent(int id, int amount)
        {
            var playerBag = playerBagSo.inventoryItems;

            for (var i = 0; i < playerBag.Count; i++)
            {
                //背包有空位，或者已经有该物品
                if (playerBag[i].itemId != 0 && playerBag[i].itemId != id) continue;

                //当背包中没有该物品时，该格子的数量默认为0
                playerBag[i] = new InventoryItem
                    { itemId = id, itemAmount = playerBag[i].itemAmount + amount };
                MyEventHandler.CallUpdateInventoryUI(InventoryLocation.Bag, playerBagSo.inventoryItems, i);
                break;
            }
        }

        private void OnBuildFurnitureEvent(int itemId, Vector3 mousePosition)
        {
            RemoveItem(itemId, 1);
            var blueprint = blueprintSo.GetBlueprintDetails(itemId);
            foreach (var requiredItem in blueprint.resourceItem)
                RemoveItem(requiredItem.itemId, requiredItem.itemAmount);
        }

        private void OnBagBaseOpenEvent(SlotType slotType, InventoryBagSo bagSo)
        {
            currentBoxBag = bagSo;
        }

        #endregion

        public void TradeItem(ItemDetails details, int amount, bool sellTrade)
        {
            var cost = details.itemPrice * amount;
            var indexInBag = GetItemIndexInBag(details.itemID, true);
            Debug.Log($"背包空位置索引：{indexInBag}");

            if (sellTrade) //Sell
            {
                if (playerBagSo.inventoryItems[indexInBag].itemAmount >= amount)
                {
                    RemoveItem(details.itemID, amount);
                    //Total price of sold item
                    cost = (int)(cost * details.sellPercentage);
                    money += cost;
                }
            }
            else if (money - cost >= 0) //Buy
            {
                if (!CheckBagFull())
                {
                    AddItemAtIndex(details, indexInBag, amount);
                    money -= cost;
                }
                else
                    return;
            }

            MyEventHandler.CallUpdateInventoryUI(InventoryLocation.Bag, playerBagSo.inventoryItems, indexInBag);
        }

        /// <summary>
        /// Check the stock of building resource
        /// </summary>
        /// <param name="id">blue print id</param>
        /// <returns></returns>
        public bool CheckStock(int id)
        {
            var blueprintDetails = blueprintSo.GetBlueprintDetails(id);

            foreach (var requireItem in blueprintDetails.resourceItem)
            {
                var itemStock = playerBagSo.GetInventoryItem(requireItem.itemId);
                if (itemStock.itemAmount >= requireItem.itemAmount)
                    continue;
                return false;
            }

            return true;
        }

        private List<InventoryItem> GetItemList(InventoryLocation location)
            => location switch
            {
                InventoryLocation.Bag => playerBagSo.inventoryItems,
                InventoryLocation.Box => currentBoxBag.inventoryItems,
                _ => null
            };

        /// <summary>
        /// Get bag item list from dictionary
        /// </summary>
        /// <param name="key"></param>
        /// <param name="itemList"></param>
        /// <returns></returns>
        public bool GetBoxDataList(string key, out List<InventoryItem> itemList)
            => _boxDataDict.TryGetValue(key, out itemList);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="box"></param>
        public void AddBoxDataToDict(Box box)
        {
            Debug.Log($"{box.name}{box.boxIndex}");
            _boxDataDict[$"{box.name}{box.boxIndex}"] = box.boxBagSo.inventoryItems;
        }
    }
}