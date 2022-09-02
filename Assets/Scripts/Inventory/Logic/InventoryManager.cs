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

        public bool GetItemIndex(int itemId, out int index)
        {
            index = -1;
            var bagFull = true;
            var playerBag = InventoryManager.Instance.playerBagSo.inventoryItems;
            for (var i = 0; i < playerBag.Count; i++)
            {
                //找到了背包中的空格，返回空格索引
                if (playerBag[i].itemId == 0)
                {
                    bagFull = false;
                    index = i;
                    break;
                }

                //查找到背包中已经拥有该物品，返回该物品索引
                if (playerBag[i].itemId != itemId) continue;
                bagFull = false;
                index = i;
                break;
            }

            return bagFull;
        }

        public bool CheckBagSpace()
            => InventoryManager.Instance.playerBagSo.inventoryItems.All(inventoryItem => inventoryItem.itemId != 0);
    }
}