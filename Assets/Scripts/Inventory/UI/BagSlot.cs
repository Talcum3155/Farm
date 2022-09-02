using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Inventory.UI
{
    public class BagSlot : MonoBehaviour
    {
        [SerializeField] private Image slotImage;
        [SerializeField] private Image highLightImage;
        [SerializeField] private TextMeshProUGUI amountTmp;
        [SerializeField] private Button button;
        
        [Header("格子类型")] public SlotType slotType;
        public bool isSelected;

        [Header("物品信息")] public ItemDetails itemDetails;
        public int itemAmount;

        public int slotIndex;

        private void Start()
        {
            isSelected = false;
            if (itemDetails.itemID == 0)
            {
                UpdateEmptySlot();
            }
        }

        public void UpdateSlot(ItemDetails details, int amount)
        {
            itemDetails = details;
            slotImage.sprite = details.itemIcon;
            amountTmp.text = amount.ToString();
            slotImage.enabled = true;
            itemAmount = amount;
            button.interactable = true;
        }

        public void UpdateEmptySlot()
        {
            isSelected = false;
            slotImage.enabled = false;
            highLightImage.enabled = false;
            amountTmp.text=string.Empty;
            button.interactable = false;
        }
    }
}