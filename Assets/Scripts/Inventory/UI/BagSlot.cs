using System;
using Inventory.Logic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities;

namespace Inventory.UI
{
    public class BagSlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Image slotImage;
        [SerializeField] private Image highLightImage;
        [SerializeField] private TextMeshProUGUI amountTmp;
        [SerializeField] private Button button;
        public InventoryUI inventoryUI;

        [Header("格子类型")] public SlotType slotType;

        [SerializeField] private bool isSelected;

        //设置Select值的时候顺便改变高亮的激活状态
        public bool IsSelected
        {
            set => highLightImage.gameObject.SetActive(isSelected = value);
            get => isSelected;
        }

        [Header("物品信息")] public ItemDetails itemDetails;
        public int itemAmount;

        public int slotIndex;

        private void Start()
        {
            if (itemDetails.itemID == 0)
                UpdateEmptySlot();
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
            highLightImage.gameObject.SetActive(false);
            amountTmp.text = string.Empty;
            button.interactable = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (itemAmount == 0)
                return;

            inventoryUI.SwitchSelectItem(slotIndex);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (itemAmount == 0)
                return;

            inventoryUI.dragImage.gameObject.SetActive(true);
            inventoryUI.dragImage.sprite = itemDetails.itemIcon;
            inventoryUI.SwitchSelectItem(slotIndex);
        }

        public void OnDrag(PointerEventData eventData) =>
            inventoryUI.dragImage.transform.position = eventData.position;

        public void OnEndDrag(PointerEventData eventData)
        {
            //获取拖拽结束时鼠标射线击中的对象
            var target = eventData.pointerCurrentRaycast.gameObject?.GetComponent<BagSlot>();
            Debug.Log(target);
            inventoryUI.dragImage.gameObject.SetActive(false);

            //目标位置是格子
            if (target is not null)
            {
                //此格子类型和目标格子的类型一样，就直接交换位置
                if ((slotType ^ target.slotType) == 0)
                    InventoryManager.Instance.SwapItem(slotIndex, target.slotIndex);

                inventoryUI.SwitchSelectItem(target.slotIndex);
                return;
            }

            //目标位置是地上
            var screenToWorldPoint =
                Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
                    Input.mousePosition.y, -Camera.main.transform.position.z));
            if (itemDetails.canDropped)
            {
                MyEventHandler.CallInstantiatedInScene(itemDetails.itemID,screenToWorldPoint);
            }
        }
    }
}