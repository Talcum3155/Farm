using System;
using UnityEngine;
using Utilities;

namespace NPC.Data
{
    [System.Serializable]
    public class ScheduleDetails : IComparable<ScheduleDetails>
    {
        public int hour, minute, day;

        //Execute first with lower priority
        public int priority;
        public Season season;
        public string targetScene;
        public Vector2Int targetGridPosition;
        public AnimationClip animationClip;
        public bool interactable;

        public int Time => hour * 100 + minute;

        public ScheduleDetails(int hour, int minute, int day, int priority, Season season, string targetScene,
            Vector2Int targetGridPosition, AnimationClip animationClip, bool interactable)
        {
            this.hour = hour;
            this.minute = minute;
            this.day = day;
            this.priority = priority;
            this.season = season;
            this.targetScene = targetScene;
            this.targetGridPosition = targetGridPosition;
            this.animationClip = animationClip;
            this.interactable = interactable;
        }

        public int CompareTo(ScheduleDetails other)
        {
            if (Time == other.Time)
            {
                if (priority > other.priority)
                    return 1;

                return -1;
            }

            if (Time > other.Time)
                return 1;

            if (Time < other.Time)
                return -1;

            return 0;
        }
    }
}