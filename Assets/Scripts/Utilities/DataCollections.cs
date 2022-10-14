using System;
using UnityEngine;
using Utilities.CustomAttribute;

namespace Utilities
{
    [System.Serializable]
    public class ItemDetails: IComparable<ItemDetails>
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

        public int CompareTo(ItemDetails other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return itemID.CompareTo(other.itemID);
        }
    }

    [System.Serializable]
    public struct InventoryItem
    {
        public int itemId;
        public int itemAmount;
    }

    [System.Serializable]
    public class AnimatorType
    {
        public PartName partName;
        public PartType partType;
        public AnimatorOverrideController animatorOverrideController;
    }

    [System.Serializable]
    public class SerializableVector3
    {
        public float x, y, z;
        
        public SerializableVector3(Vector3 position)
        {
            x = position.x;
            y = position.y;
            z = position.z;
        }

        public Vector3 ToVector3() => new Vector3(x, y, z);

        public Vector2Int ToVector2Int() => new Vector2Int((int)x, (int)y);
    }

    [System.Serializable]
    public class SceneItem
    {
        public int itemId;
        public SerializableVector3 itemPositionInScene;
    }

    [System.Serializable]
    public class TileProperty
    {
        public Vector2Int coordinate;
        public GridType gridType;
        public bool boolTypeValue;
    }

    [System.Serializable]
    public class TileDetails
    {
        public int gridX, gridY;
        public bool diggable;
        public bool dropItem;
        public bool placeFurniture;
        public bool npcObstacle;
        public int seedItemId = -1;
        public int growthDays = -1;
        public int daysSinceDug = -1; //距离挖坑过了多少天
        public int daysSinceWatered = -1; //距离浇水过了多少天
        //针对可重复收割的农作物，距离收割过了多少天
        public int daysSinceLastHarvest = -1;
    }

    [System.Serializable]
    public class NpcPosition
    {
        public Transform npc;
        public string startScene;
        public Vector3 position;
    }
}
