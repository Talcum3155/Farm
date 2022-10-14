using UnityEngine;

namespace Utilities
{
    public static class Settings
    {
        public const float ItemFadeDuration = 0.35f;
        public const float TargetAlpha = 0.45f;

        public const float SecondThreshold = 0.01f;
        public const int SecondHold = 59; //秒到达59进一
        public const int MinutesHold = 59; //分到达59进一
        public const int HourHold = 23; //时到达23进一
        public const int DayHold = 30; //天到达20进一
        public const int SeasonHold = 3; //季到达20进一

        public const float FadeCanvasDuration = 1.5f;

        public const float GridCellSize = 1f;
        public const float GridCellDiagonalSize = 1.41f;
        // 20*20 pixel per 1unit => 1*1 pixel per 0.05 unit
        public const float PixelSize = 0.05f;

        //Wait a while to play animation
        public const float AnimationBreakTime = 5f; 
    }
}