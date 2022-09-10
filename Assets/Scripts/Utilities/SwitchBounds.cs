using System;
using Cinemachine;
using UnityEngine;

namespace Utilities
{
    public class SwitchBounds : MonoBehaviour
    {
        private void OnEnable()
        {
            MyEventHandler.AfterSceneLoaded += OnSwitchConfinerShapeEvent;
        }

        private void OnDisable()
        {
            MyEventHandler.AfterSceneLoaded -= OnSwitchConfinerShapeEvent;
        }

        /// <summary>
        /// 切换cinemachine的碰撞边界防止摄像机超出地图边界，
        /// 需要cinemachine添加上Confiner组件
        /// </summary>
        private void OnSwitchConfinerShapeEvent()
        {
            var boundGameObject = GameObject.FindGameObjectWithTag("BoundsConfiner").GetComponent<PolygonCollider2D>();
            var cinemachineConfiner = GetComponent<CinemachineConfiner>();

            cinemachineConfiner.m_BoundingShape2D = boundGameObject;
            //切换碰撞边界后不会立即生效，需要清除缓存
            cinemachineConfiner.InvalidatePathCache();
        }
    }
}