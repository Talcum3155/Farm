using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Inventory.Logic;
using UnityEngine;
using Utilities;

namespace Player
{
    public class AnimatorOverride : MonoBehaviour
    {
        private Animator[] _animators;
        public SpriteRenderer holdItemSpriteRenderer;

        [Header("各部位动画的列表")] public List<AnimatorType> animatorTypes;
        private readonly Dictionary<string, Animator> _animatorNameDict = new();

        private void Awake()
        {
            _animators = GetComponentsInChildren<Animator>();
            //获取每个动画控制机所处GO的名称
            foreach (var animator in _animators)
                _animatorNameDict[animator.name] = animator;
        }

        private void OnEnable()
        {
            MyEventHandler.SelectedItem += OnItemSelectedEvent;
            MyEventHandler.HarvestAtPlayerPosition += OnHarvestAtPlayerPositionEvent;
        }

        private void OnDisable()
        {
            MyEventHandler.SelectedItem -= OnItemSelectedEvent;
            MyEventHandler.HarvestAtPlayerPosition -= OnHarvestAtPlayerPositionEvent;
        }

        private void OnItemSelectedEvent(ItemDetails itemDetails, bool isSelected)
        {
            if (!isSelected)
            {
                holdItemSpriteRenderer.enabled = false;
                SwitchAnimator(PartType.None);
                return;
            }

            if (itemDetails.canCarried)
            {
                holdItemSpriteRenderer.enabled = true;
                holdItemSpriteRenderer.sprite = itemDetails.itemOnWorldSprite;
            }

            //TODO: 补充不同种类的动画
            SwitchAnimator(itemDetails.itemType switch
            {
                //这些物品需要举起来
                ItemType.Seed
                    or ItemType.Commodity
                    or ItemType.Furniture
                    => PartType.Carry,
                ItemType.HoeTool
                    => PartType.Hoe,
                ItemType.WaterTool
                    => PartType.Water,
                ItemType.CollectTool
                    => PartType.Collect,
                ItemType.ChopTool
                    => PartType.Chop,
                ItemType.BreakTool
                    => PartType.Break,
                ItemType.ReapTool
                    => PartType.Reap,
                _ => PartType.None
            });
        }

        private void SwitchAnimator(PartType partType)
        {
            //让身体的每个部位的动画都切换成partType对应的动画
            foreach (var animatorType in
                     animatorTypes.Where(animatorType
                         => animatorType.partType == partType))
            {
                _animatorNameDict[animatorType.partName.ToString()].runtimeAnimatorController =
                    animatorType.animatorOverrideController;
            }
        }

        private async void OnHarvestAtPlayerPositionEvent(int itemId, int amount)
        {
            var itemSprite = InventoryManager.Instance.GetItemDetails(itemId).itemOnWorldSprite;
            if (!holdItemSpriteRenderer.enabled)
                await ShowItemInPlayerHead(itemSprite);
        }


        private async UniTask ShowItemInPlayerHead(Sprite itemSprite)
        {
            holdItemSpriteRenderer.sprite = itemSprite;
            holdItemSpriteRenderer.enabled = true;
            await UniTask.Delay(TimeSpan.FromSeconds(1f));
            holdItemSpriteRenderer.enabled = false;
        }
    }
}