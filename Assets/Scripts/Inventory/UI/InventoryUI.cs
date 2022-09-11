using System;
using System.Collections.Generic;
using Inventory.Logic;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Inventory.UI
{
    public class InventoryUI : MonoBehaviour
    {
        public Image dragImage;

        [SerializeField] private List<BagSlot> bagSlots;
        [SerializeField] private GameObject bag;
        public ItemToolTip itemToolTip;
        private int _activeSlotIndex = -1;

        private void Start()
        {
            for (var i = 0; i < bagSlots.Count; i++)
            {
                bagSlots[i].slotIndex = i;
                bagSlots[i].inventoryUI = this;
            }

            bag.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B))
                SwitchBagOpenStatus();
        }

        private void OnEnable()
        {
            MyEventHandler.UpdateInventoryUI += OnUpdateInventoryUI;
            MyEventHandler.BeforeSceneUnLoad += OnBeforeSceneUnLoad;
        }

        private void OnDisable()
        {
            MyEventHandler.UpdateInventoryUI -= OnUpdateInventoryUI;
            MyEventHandler.BeforeSceneUnLoad -= OnBeforeSceneUnLoad;
        }

        /// <summary>
        /// 根据Inventory的定位，更新对应的库存列表弄UI
        /// </summary>
        /// <param name="location">要更新的格子属于谁</param>
        /// <param name="itemsList">需要更新的格子UI列表</param>
        /// <param name="index">如果物品有索引，就只更新该索引的格子</param>
        private void OnUpdateInventoryUI(InventoryLocation location, List<InventoryItem> itemsList, int index)
        {
            switch (location)
            {
                case InventoryLocation.Bag:
                    if (index != -1)
                    {
                        if (itemsList[index].itemId == 0)
                        {
                            bagSlots[index].UpdateEmptySlot();
                            return;
                        }

                        bagSlots[index].UpdateSlot(InventoryManager.Instance.GetItemDetails(itemsList[index].itemId),
                            itemsList[index].itemAmount);
                        return;
                    }

                    for (var i = 0; i < bagSlots.Count; i++)
                    {
                        //如果此格子有物品就更新
                        if (itemsList[i].itemId > 0)
                            bagSlots[i].UpdateSlot(InventoryManager.Instance.GetItemDetails(itemsList[i].itemId),
                                itemsList[i].itemAmount);
                        //没有就置空
                        else
                            bagSlots[i].UpdateEmptySlot();
                    }

                    break;
                case InventoryLocation.Box:
                    break;
            }
        }

        public void SwitchBagOpenStatus()
            => bag.SetActive(!bag.activeInHierarchy);

        public void SwitchSelectItem(int slotIndex)
        {
            /*
             * 激活槽位等于传参槽位 或 传参是-1 ，禁用当前选择的槽位
             * 如果激活槽位和传参槽位都等于-1，则需要直接范返回防止索引越界
             */
            if (_activeSlotIndex == slotIndex || slotIndex is -1)
            {
                if (_activeSlotIndex is -1)
                    return;
                bagSlots[_activeSlotIndex].IsSelected = false;
                MyEventHandler.CallSelectedItem(bagSlots[_activeSlotIndex].itemDetails, false);
                _activeSlotIndex = -1;
                return;
            }

            if (slotIndex >= bagSlots.Count || slotIndex < 0)
            {
                _activeSlotIndex = -1;
                return;
            }

            if (_activeSlotIndex is not -1)
                bagSlots[_activeSlotIndex].IsSelected = false;
            bagSlots[_activeSlotIndex = slotIndex].IsSelected = true;
            MyEventHandler.CallSelectedItem(bagSlots[_activeSlotIndex].itemDetails, true);
        }

        private void OnBeforeSceneUnLoad()
        {
            //取消举起物品状态和举起物品的动画
            SwitchSelectItem(-1);
        }
    }
}