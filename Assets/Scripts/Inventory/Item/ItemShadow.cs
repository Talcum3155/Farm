using System;
using UnityEngine;

namespace Inventory.Item
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ItemShadow : MonoBehaviour
    {
        [SerializeField]private SpriteRenderer itemSprite;
        [SerializeField] SpriteRenderer shadowSprite;

        private void Start()
        {
            shadowSprite.sprite = itemSprite.sprite;
            shadowSprite.color = new Color(0, 0, 0, 0.3f);
        }
    }
}
