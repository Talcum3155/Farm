using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Utilities;

namespace Inventory.UI
{
    public class ShowItemToolTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private BagSlot bagSlot;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (bagSlot.itemAmount == 0)
                return;

            bagSlot.inventoryUI.itemToolTip.gameObject.SetActive(true);
            bagSlot.inventoryUI.itemToolTip.SetUpItemToolTip(bagSlot.itemDetails, bagSlot.slotType);
            bagSlot.inventoryUI.itemToolTip.transform.position = transform.position + Vector3.up * 60;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (bagSlot.itemAmount == 0)
                return;
            
            bagSlot.inventoryUI.itemToolTip.gameObject.SetActive(false);
        }
    }
}