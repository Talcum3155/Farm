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

        public ItemDetails GetItemDetails(int id)
            => itemDataListSo.itemDetailsList.Find(i => i.itemID == id);

        public void AddItem(WorldItem item, bool toDestroy)
        {
            var bagFull = true;
            var playerBag = InventoryManager.Instance.playerBagSo.inventoryItems;
            
            
            
            for (var i = 0; i < playerBag.Count; i++)
            {
                //找到了背包中的空格的逻辑
                if (playerBag[i].itemId == 0)
                {
                    bagFull = false;
                    playerBag[i] = new InventoryItem() { itemId = item.itemId, itemAmount = item.itemAmount };
                    break;
                }

                //查找到背包中已经拥有该物品的逻辑
                if (playerBag[i].itemId != item.itemId) continue;
                bagFull = false;
                playerBag[i] = new InventoryItem()
                    { itemId = item.itemId, itemAmount = playerBag[i].itemAmount + item.itemAmount };
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
            => InventoryManager.Instance.playerBagSo.
                inventoryItems.All(inventoryItem => inventoryItem.itemId != 0);
    }
}