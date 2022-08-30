using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Inventory.DataSO
{
    [CreateAssetMenu(fileName = "New DataSO",menuName = "Inventory/ItemDataList")]
    public class ItemDataListSo : ScriptableObject
    {
        public List<ItemDetails> itemDetailsList;
    }
}
