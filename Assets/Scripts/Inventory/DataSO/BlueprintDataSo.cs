using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Inventory.DataSO
{
    [CreateAssetMenu(fileName = "NewBluePrint", menuName = "Inventory/BluePrint")]
    public class BlueprintDataSo : ScriptableObject
    {
        public List<BlueprintDetails> blueprintDataList;

        public BlueprintDetails GetBlueprintDetails(int blueprintId) => blueprintDataList.Find(i => i.bluePrintId == blueprintId);
    }

    [Serializable]
    public class BlueprintDetails
    {
        public int bluePrintId;
        public InventoryItem[] resourceItem = new InventoryItem[3];
        public GameObject buildPrefab;
    }
}
