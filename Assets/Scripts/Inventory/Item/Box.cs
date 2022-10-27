using System;
using Inventory.DataSO;
using Inventory.Logic;
using UnityEngine;
using Utilities;

namespace Inventory.Item
{
    public class Box : MonoBehaviour
    {
        public InventoryBagSo boxBagTemplate;
        public InventoryBagSo boxBagSo;

        public GameObject mouseIcon;
        private bool _canOpen;
        private bool _opened;

        public int boxIndex;

        private void Awake()
        {
            mouseIcon = transform.GetChild(0).gameObject;
        }

        private void OnEnable()
        {
            if (boxBagSo == null)
                boxBagSo = Instantiate(boxBagTemplate);
        }

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (!col.CompareTag("Player")) return;
            _canOpen = true;
            mouseIcon.SetActive(true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;
            _canOpen = false;
            mouseIcon.SetActive(false);
        }

        private void Update()
        {
            if (!_opened && _canOpen && Input.GetMouseButtonDown(1))
            {
                MyEventHandler.CallBagBaseOpen(SlotType.Box, boxBagSo);
                _opened = true;
            }

            if ((!_canOpen && _opened) || Input.GetKeyDown(KeyCode.Escape))
            {
                MyEventHandler.CallBagBaseClose(SlotType.Box, boxBagSo);
                _opened = false;
            }
        }

        /// <summary>
        /// if the box is built the first time,
        /// it will add itself to dict.
        /// otherwise, it will read the itemList data
        /// from dict by key
        /// </summary>
        /// <param name="index"></param>
        public void InitBox(int index)
        {
            boxIndex = index;
            if (InventoryManager.Instance.GetBoxDataList($"{name}{index}", out var itemList))
            {
                boxBagSo.inventoryItems = itemList;
                return;
            }

            InventoryManager.Instance.AddBoxDataToDict(this);
        }
    }
}