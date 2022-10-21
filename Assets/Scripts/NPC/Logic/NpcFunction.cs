using System;
using Inventory.DataSO;
using UnityEngine;
using Utilities;

namespace NPC.Logic
{
    public class NpcFunction : MonoBehaviour
    {
        public InventoryBagSo shopData;
        private bool _opened;

        private void Update()
        {
            if (_opened && Input.GetKeyDown(KeyCode.Escape))
            {
                CloseShop();
            }
        }

        public void OpenShop()
        {
            _opened = true;
            MyEventHandler.CallBagBaseOpen(SlotType.Shop, shopData);
            MyEventHandler.CallUpdateGameState(GameState.Pause);
        }

        public void CloseShop()
        {
            _opened = false;
            MyEventHandler.CallBagBaseClose(SlotType.Shop, shopData);
            MyEventHandler.CallUpdateGameState(GameState.GamePlay);
        }
    }
}
