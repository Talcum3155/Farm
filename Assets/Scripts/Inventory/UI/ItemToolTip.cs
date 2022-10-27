using System;
using Inventory.DataSO;
using Inventory.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Inventory.UI
{
    public class ItemToolTip : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemTypeText;
        [SerializeField] private TextMeshProUGUI itemDescriptionText;
        [SerializeField] private Text itemPriceText;
        [SerializeField] private GameObject bottomPart;

        [Header("建造")] [SerializeField] public GameObject resourcePanel;
        [SerializeField] private Image[] materials;

        public void SetUpItemToolTip(ItemDetails itemDetails, SlotType slotType)
        {
            itemNameText.text = itemDetails.itemName;
            itemTypeText.text = GetTypeName(itemDetails.itemType);
            itemDescriptionText.text = itemDetails.itemDescription;

            //如果itemDetails的类型是种子、家具、商品就需要显示价格WS
            if (itemDetails.itemType is ItemType.Seed or ItemType.Commodity or ItemType.Furniture)
            {
                // Debug.Log(GetTypeName(itemDetails.itemType));
                bottomPart.SetActive(true);
                //如果是背包里的物品就显示卖价
                itemPriceText.text = slotType == SlotType.Bag
                    ? ((int)(itemDetails.sellPercentage * itemDetails.itemPrice)).ToString()
                    : itemDetails.itemPrice.ToString();
                //强制刷新布局
                LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
                return;
            }

            bottomPart.SetActive(false);
            LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
        }

        private static string GetTypeName(ItemType itemType)
        {
            return itemType switch
            {
                ItemType.Seed => "种子",
                ItemType.Commodity => "商品",
                ItemType.Furniture => "家具",
                ItemType.HoeTool => "工具",
                ItemType.ChopTool => "工具",
                ItemType.BreakTool => "工具",
                ItemType.WaterTool => "工具",
                ItemType.CollectTool => "工具",
                _ => "无"
            };
        }

        public void SetupResourcePanel(int blueprintId)
        {
            var blueprint = InventoryManager.Instance.blueprintSo.GetBlueprintDetails(blueprintId);
            for (var i = 0; i < materials.Length; i++)
            {
                if (i < blueprint.resourceItem.Length)
                {
                    var blueprintItem = blueprint.resourceItem[i];
                    materials[i].gameObject.SetActive(true);
                    materials[i].sprite = InventoryManager.Instance.GetItemDetails(blueprintItem.itemId).itemIcon;
                    materials[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text =
                        blueprintItem.itemAmount.ToString();
                    continue;
                }

                materials[i].gameObject.SetActive(false);
            }
        }
    }
}