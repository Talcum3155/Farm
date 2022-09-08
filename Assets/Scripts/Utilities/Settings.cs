using UnityEngine;

namespace Utilities
{
    public static class Settings
    {
        [Header("Translucence")] public const float FadeDuration = 0.35f;
        public const float TargetAlpha = 0.45f;

        public const float SecondThreshold = 0.01f;
        public const int SecondHold = 59; //秒到达59进一
        public const int MinutesHold = 59; //分到达59进一
        public const int HourHold = 23; //时到达23进一
        public const int DayHold = 30; //天到达20进一
        public const int SeasonHold = 3; //季到达20进一
    }
}