using System;
using Inventory.Logic;
using UnityEngine;
using Utilities;

namespace Inventory.Item
{
    public class WorldItem : MonoBehaviour
    {
        public int itemId;
        public int itemAmount;

        private ItemDetails _itemDetails;
        private BoxCollider2D _boxCollider2D;
        private SpriteRenderer _spriteRenderer;
        
        private void Awake()
        {
            _boxCollider2D = GetComponent<BoxCollider2D>();
            _spriteRenderer = transform.GetChild(0).GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            if (itemId >= 1001)
            {
                Init(itemId);
            }
        }

        public void Init(int id)
        {
            itemId = id;
            _itemDetails = InventoryManager.Instance.GetItemDetails(id);

            if (_itemDetails is null) return;
            var sprite = _itemDetails.itemOnWorldSprite ? _itemDetails.itemOnWorldSprite : _itemDetails.itemIcon;
            _spriteRenderer.sprite = sprite;

            _boxCollider2D.size = new Vector2(sprite.bounds.size.x,sprite.bounds.size.y);
            //锚点在不在中心时，需要让碰撞体的y轴偏移到中心点
            _boxCollider2D.offset = Vector2.up * sprite.bounds.center.y;
        }
    }
}
