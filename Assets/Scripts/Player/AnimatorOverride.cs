using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utilities;

namespace Player
{
    public class AnimatorOverride : MonoBehaviour
    {
        private Animator[] _animators;
        public SpriteRenderer holdItemSpriteRenderer;

        [Header("各部位动画的列表")] public List<AnimatorType> animatorTypes;
        private Dictionary<string, Animator> _animatorNameDict = new();

        private void Awake()
        {
            _animators = GetComponentsInChildren<Animator>();
            //获取每个动画控制机所处GO的名称
            foreach (var animator in _animators)
            {
                Debug.Log(animator.name);
                _animatorNameDict[animator.name] = animator;
            }
        }

        private void OnEnable()
        {
            MyEventHandler.SelectedItem += OnItemSelected;
        }

        private void OnDisable()
        {
            MyEventHandler.SelectedItem -= OnItemSelected;
        }

        private void OnItemSelected(ItemDetails itemDetails, bool isSelected)
        {
            var partType = PartType.None;

            if (itemDetails.canCarried && isSelected)
            {
                partType = itemDetails.itemType switch
                {
                    //这些物品需要举起来
                    ItemType.Seed => PartType.Carry,
                    ItemType.Commodity => PartType.Carry,
                    ItemType.Furniture => PartType.Carry,
                    _ => PartType.None
                };
                holdItemSpriteRenderer.enabled = true;
                holdItemSpriteRenderer.sprite = itemDetails.itemOnWorldSprite;
            }
            else
                holdItemSpriteRenderer.enabled = false;

            SwitchAnimator(partType);
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
    }
}