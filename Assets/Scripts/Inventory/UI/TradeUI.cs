using System;
using Inventory.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Inventory.UI
{
    public class TradeUI : MonoBehaviour
    {
        public Image itemImage;
        public TextMeshProUGUI itemName;
        public TMP_InputField inputAmount;
        public Button confirmBtn;
        public Button cancelBtn;

        private ItemDetails _details;
        private bool _sellTrade;

        private void Awake()
        {
            cancelBtn.onClick.AddListener(CancelTrade);
            confirmBtn.onClick.AddListener(TradeItem);
        }

        public void SetupTradeUI(ItemDetails details, bool sell)
        {
            _sellTrade = sell;
            _details = details;
            itemImage.sprite = details.itemIcon;
            itemName.text = details.itemName;
            inputAmount.text = string.Empty;
        }

        private void TradeItem()
        {
            if (int.TryParse(inputAmount.text, out var amount))
            {
                Debug.Log($"输入的数量：{amount}");
                InventoryManager.Instance.TradeItem(_details, amount, _sellTrade);
            }

            CancelTrade();
        }

        private void CancelTrade()
        {
            gameObject.SetActive(false);
        }
    }
}