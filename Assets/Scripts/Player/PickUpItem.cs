using System;
using Inventory.Item;
using Inventory.Logic;
using UnityEngine;
using Utilities;

namespace Player
{
    public class PickUpItem : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.CompareTag("WorldItem"))
            {
                var item = col.GetComponent<WorldItem>();
                if (item)
                    InventoryManager.Instance.AddItem(item, true);
                MyEventHandler.CallPlaySoundEffect(SoundName.Pickup);
            }
        }
    }
}