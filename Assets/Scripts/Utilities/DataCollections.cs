using UnityEngine;

namespace Utilities
{
    [System.Serializable]
    public class ItemDetails
    {
        public int itemID;
        public string itemName;
        public ItemType itemType;
        public Sprite itemIcon;
        public Sprite itemOnWorldSprite;
        public string itemDescription;
        public int itemUseRadius; //物品使用范围
        public bool canPickedUp;
        public bool canDropped;
        public bool canCarried;
        public int itemPrice;
        [Range(0, 1)]
        public float sellPercentage; //卖出再买回的损失价格
    }
}
