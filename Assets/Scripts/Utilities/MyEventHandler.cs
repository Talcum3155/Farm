using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public static class MyEventHandler
    {
        public static event Action<InventoryLocation, List<InventoryItem>, int> UpdateInventoryUI;

        public static void CallUpdateInventoryUI(InventoryLocation location, List<InventoryItem> list, int index)
            => UpdateInventoryUI?.Invoke(location, list, index);
    }
}