using System;
using UnityEngine;
using Utilities;

namespace TimeSystem.Logic
{
    public class TimeManager : SingleTon<TimeManager>
    {
        private int _gameSecond, _gameMinutes, _gameHour, _gameDay, _gameMonth, _gameYear;
        [SerializeField] private Season gameSeason = Season.春天;
        [SerializeField] private int monthInSeason = 3;

        public bool gameClockPause;
        private float _tikTime;

        private float _timeDifference;

        public TimeSpan GameTime => new TimeSpan(_gameHour, _gameMinutes, _gameSecond);
        
        private void Start()
        {
            ResetTime();
            MyEventHandler.CallUpdateTime(_gameSecond, _gameMinutes, _gameHour);
            MyEventHandler.CallUpdateHour(_gameHour);
            MyEventHandler.CallUpdateDate(_gameDay, _gameMonth, gameSeason, _gameYear);
            MyEventHandler.CallLightShiftChange(gameSeason, GetCurrentLightShift(), _timeDifference);
        }

        private void OnEnable()
        {
            MyEventHandler.UpdateGameState += UpdateGameStateEvent;
        }

        private void OnDisable()
        {
            MyEventHandler.UpdateGameState -= UpdateGameStateEvent;
        }
        
        private void UpdateGameStateEvent(GameState state)
        {
            gameClockPause = state == GameState.Pause;
        }

        private void Update()
        {
            if (gameClockPause) return;

            _tikTime += Time.deltaTime;
            //如果计时器超过了阈值，就刷新游戏内时间
            if (!(_tikTime >= Settings.SecondThreshold)) return;
            _tikTime -= Settings.SecondThreshold;
            UpdateSecond();

            if (Input.GetKey(KeyCode.T))
            {
                for (var i = 0; i < 100; i++)
                    UpdateSecond();
            }

            if (Input.GetKey(KeyCode.Y))
            {
                UpdateDay();
            }
        }

        private void UpdateSecond()
        {
            _gameSecond++;
            if (_gameSecond > Settings.SecondHold)
            {
                _gameSecond = 0;
                UpdateMinutes();
            }

            MyEventHandler.CallUpdateTime(_gameSecond, _gameMinutes, _gameHour);
        }

        private void UpdateMinutes()
        {
            _gameMinutes++;
            if (_gameMinutes > Settings.MinutesHold)
            {
                _gameMinutes = 0;
                UpdateHour();
            }

            MyEventHandler.CallGameMinuteEnd(_gameMinutes, _gameHour, _gameDay, gameSeason);
            //Judge the light whether needs to be changed
            MyEventHandler.CallLightShiftChange(gameSeason, GetCurrentLightShift(), _timeDifference);
        }

        private void UpdateHour()
        {
            _gameHour++;
            if (_gameHour > Settings.HourHold)
            {
                _gameHour = 0;
                UpdateDay();
            }

            MyEventHandler.CallUpdateHour(_gameHour);
        }

        private void UpdateDay()
        {
            _gameDay++;
            if (_gameDay > Settings.DayHold)
            {
                _gameDay = 1;
                UpdateMonth();
            }

            MyEventHandler.CallUpdateDate(_gameDay, _gameMonth, gameSeason, _gameYear);
            MyEventHandler.CallGameDayEnd(_gameDay, gameSeason);
        }

        private void UpdateMonth()
        {
            _gameMonth++;
            if (_gameMonth > 12)
            {
                _gameMonth = 1;
                UpdateMonthInSeason();
            }
        }

        private void UpdateMonthInSeason()
        {
            monthInSeason--;
            if (monthInSeason == 0)
            {
                monthInSeason = 3;
                UpdateSeason();
            }
        }

        private void UpdateSeason()
        {
            var seasonNumber = (int)gameSeason;
            seasonNumber++;
            if (seasonNumber > Settings.SeasonHold)
            {
                seasonNumber = 0;
                UpdateYear();
            }

            gameSeason = (Season)seasonNumber;
        }

        private void UpdateYear()
        {
            _gameYear++;
            if (_gameYear > 9999)
                _gameYear = 2022;
        }

        public void ResetTime()
        {
            _gameSecond = 0;
            _gameMinutes = 0;
            _gameHour = 7;
            _gameDay = 1;
            _gameMonth = 1;
            _gameYear = 2022;
            gameSeason = Season.春天;
        }

        private LightShift GetCurrentLightShift()
        {
            if (GameTime >= Settings.MorningTime && GameTime < Settings.NightTime)
            {
                _timeDifference = (float)(GameTime - Settings.MorningTime).TotalMinutes;
                return LightShift.Morning;
            }

            if (GameTime < Settings.MorningTime || GameTime >= Settings.NightTime)
            {
                _timeDifference = Mathf.Abs((float)(GameTime - Settings.NightTime).TotalMinutes);
                return LightShift.Night;
            }

            return LightShift.Morning;
        }

        public string GetFormatTime() =>
            $"{_gameYear:0000}年 {_gameMonth:00}月 {_gameDay:00}日 " +
            $"{gameSeason} {_gameHour:00} : {_gameMinutes:00} : {_gameSecond:00}";
    }
}