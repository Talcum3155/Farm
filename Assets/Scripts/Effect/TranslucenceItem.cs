using System;
using DG.Tweening;
using UnityEngine;
using Utilities;

namespace Effect
{
    public class TranslucenceItem : MonoBehaviour
    {
        private SpriteRenderer _leaves;
        private SpriteRenderer _trunk;

        private void Awake()
        {
            _leaves = transform.GetChild(0).GetComponent<SpriteRenderer>();
            _trunk = transform.GetChild(1).GetComponent<SpriteRenderer>();
        }

        /// <summary>
        /// 逐渐恢复颜色
        /// </summary>
        public void FadeIn()
        {
            _leaves.DOColor(new Color(1, 1, 1, 1), Settings.ItemFadeDuration);
            _trunk.DOColor(new Color(1, 1, 1, 1), Settings.ItemFadeDuration);
        }

        /// <summary>
        /// 逐渐半透明
        /// </summary>
        public void FadeOut()
        {
            _leaves.DOColor(new Color(1, 1, 1, Settings.TargetAlpha), Settings.ItemFadeDuration);
            _trunk.DOColor(new Color(1, 1, 1, Settings.TargetAlpha), Settings.ItemFadeDuration);
        }
            

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.CompareTag("Player"))
            {
                FadeOut();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                FadeIn();
            }
        }
    }
}