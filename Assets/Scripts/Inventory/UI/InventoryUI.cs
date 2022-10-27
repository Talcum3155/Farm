using System;
using System.Collections.Generic;
using System.Linq;
using Inventory.DataSO;
using Inventory.Logic;
using TMPro;
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

        [Header("交易UI")] public TradeUI tradeUI;
        public TextMeshProUGUI moneyText;

        private int _activeSlotIndex = -1;

        [Header("通用背包")] [SerializeField] private GameObject bagBase;
        public GameObject shopSlotPrefab;
        public GameObject boxSlotPrefab;
        [SerializeField] private List<BagSlot> baseBagSlots;

        private void Start()
        {
            for (var i = 0; i < bagSlots.Count; i++)
            {
                bagSlots[i].slotIndex = i;
                bagSlots[i].inventoryUI = this;
            }

            bag.SetActive(false);
            moneyText.text = InventoryManager.Instance.money.ToString();
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
            MyEventHandler.BagBaseOpen += OnBaseBagOpenEvent;
            MyEventHandler.BagBaseClose += OnBaseBagCloseEvent;
            MyEventHandler.ShowTradeUI += OnShowTradeUIEvent;
        }

        private void OnDisable()
        {
            MyEventHandler.UpdateInventoryUI -= OnUpdateInventoryUI;
            MyEventHandler.BeforeSceneUnLoad -= OnBeforeSceneUnLoad;
            MyEventHandler.BagBaseOpen -= OnBaseBagOpenEvent;
            MyEventHandler.BagBaseClose -= OnBaseBagCloseEvent;
            MyEventHandler.ShowTradeUI -= OnShowTradeUIEvent;
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
                    moneyText.text = InventoryManager.Instance.money.ToString();
                    if (index != -1)
                    {
                        //此背包槽位已经空了
                        if (itemsList[index].itemId == 0)
                        {
                            bagSlots[index].UpdateEmptySlot();
                            _activeSlotIndex = -1;
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
                    for (var i = 0; i < baseBagSlots.Count; i++)
                    {
                        if (itemsList[i].itemAmount > 0)
                        {
                            var details = InventoryManager.Instance.GetItemDetails(itemsList[i].itemId);
                            baseBagSlots[i].UpdateSlot(details, itemsList[i].itemAmount);
                            continue;
                        }

                        baseBagSlots[i].UpdateEmptySlot();
                    }

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

            /*
             * _activeSlotIndex不是-1，说明持有物品，需要先将该物品卸下。
             * 卸下时不仅要取消高亮，如果持有物是工具，
             * 还需要将工具的动画状态机还原回默认状态
             */
            if (_activeSlotIndex is not -1)
            {
                bagSlots[_activeSlotIndex].IsSelected = false;
                MyEventHandler.CallSelectedItem(null, false);
            }

            bagSlots[_activeSlotIndex = slotIndex].IsSelected = true;
            MyEventHandler.CallSelectedItem(bagSlots[_activeSlotIndex].itemDetails, true);
        }

        private void OnBeforeSceneUnLoad()
        {
            //取消举起物品状态和举起物品的动画
            SwitchSelectItem(-1);
        }

        private void OnBaseBagOpenEvent(SlotType slotType, InventoryBagSo bagSo)
        {
            var prefab = slotType switch
            {
                SlotType.Shop => shopSlotPrefab,
                SlotType.Box => boxSlotPrefab,
                _ => null
            };

            bagBase.SetActive(true);
            var i = 0;
            baseBagSlots = new List<BagSlot>(bagSo.inventoryItems.Select((s) =>
            {
                var slot = Instantiate(prefab, bagBase.transform.GetChild(1)).GetComponent<BagSlot>();
                slot.inventoryUI = this;
                slot.slotIndex = i++;
                return slot;
            }));

            LayoutRebuilder.ForceRebuildLayoutImmediate(bagBase.GetComponent<RectTransform>());

            if (slotType == SlotType.Shop)
            {
                bag.GetComponent<RectTransform>().pivot = new Vector2(-0.75f, 0.5f);
                bag.SetActive(true);
            }

            OnUpdateInventoryUI(InventoryLocation.Box, bagSo.inventoryItems, -1);
        }

        private void OnBaseBagCloseEvent(SlotType slotType, InventoryBagSo bagSo)
        {
            bagBase.SetActive(false);

            itemToolTip.gameObject.SetActive(false);
            SwitchSelectItem(-1);

            foreach (var slot in baseBagSlots)
                Destroy(slot.gameObject);
            baseBagSlots.Clear();

            if (slotType == SlotType.Shop)
            {
                bag.GetComponent<RectTransform>().pivot = new Vector2(0.5f, 0.5f);
                bag.SetActive(false);
            }
        }

        private void OnShowTradeUIEvent(ItemDetails details, bool sell)
        {
            tradeUI.gameObject.SetActive(true);
            tradeUI.SetupTradeUI(details, sell);
        }
    }
}