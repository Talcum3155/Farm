using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace TimeSystem.UI
{
    public class TimeUI : MonoBehaviour
    {
        [SerializeField] private RectTransform timeRotation;
        [SerializeField] private Image[] timeBlocks;
        [SerializeField] private TextMeshProUGUI gameDate;
        [SerializeField] private TextMeshProUGUI gameTime;
        [SerializeField] private Image seasonImage;
        public Sprite[] seasonSprites;

        private void OnEnable()
        {
            MyEventHandler.UpdateTime += OnUpdateTimeEvent;
            MyEventHandler.UpdateDate += OnUpdateDateEvent;
            MyEventHandler.UpdateHour += OnUpdateHourEvent;
        }

        private void OnDisable()
        {
            MyEventHandler.UpdateTime -= OnUpdateTimeEvent;
            MyEventHandler.UpdateDate -= OnUpdateDateEvent;
            MyEventHandler.UpdateHour -= OnUpdateHourEvent;
        }

        /// <summary>
        /// 更新时间
        /// </summary>
        /// <param name="second"></param>
        /// <param name="minutes"></param>
        /// <param name="hour"></param>
        private void OnUpdateTimeEvent(int second, int minutes, int hour)
            => gameTime.text = $"{hour:00} : {minutes:00} : {second:00}";


        /// <summary>
        /// 更新日期
        /// </summary>
        /// <param name="day"></param>
        /// <param name="month"></param>
        /// <param name="season"></param>
        /// <param name="year"></param>s
        private void OnUpdateDateEvent(int day, int month, Season season, int year)
        {
            gameDate.text = $"{year:0000}年{month:00}月{day:00}日";
            seasonImage.sprite = seasonSprites[(int)season];
        }

        /// <summary>
        /// 更新时间槽、天色图轮替
        /// </summary>
        /// <param name="hour"></param>
        private void OnUpdateHourEvent(int hour)
        {
            UpdateTimeSlot(hour);
            DayNightImageRotate(hour);
        }

        private void UpdateTimeSlot(int hour)
        {
            //零晨时，重置所有时间槽位
            if (hour == 0)
            {
                foreach (var timeBlock in timeBlocks)
                    timeBlock.DOColor(new Color(1, 1, 1, 0), 0.5f);
                return;
            }

            var index = hour / 4;

            var colorPercent = (hour % 4) / 4f;
            /*
             * 将之前所有的槽位都设为不透明是为了防止刚进入游戏时，只会刷新当前的槽位，之前的槽位还是处于透明状态
             */
            for (var i = 0; i < index; i++)
                timeBlocks[i].DOColor(new Color(1, 1, 1, 1), 0.5f);

            var targetColor = new Color(1, 1, 1, 1 * colorPercent);
            timeBlocks[index].DOColor(targetColor, 1f);
        }

        private void DayNightImageRotate(int hour)
        {
            var vector3 = new Vector3(0, 0, hour * 15 - 90);
            timeRotation.DORotate(vector3, 1f, RotateMode.Fast);
        }
    }
}