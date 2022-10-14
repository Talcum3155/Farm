using System.Collections.Generic;
using UnityEngine;

namespace NPC.Data
{
    [CreateAssetMenu(menuName = "NPC/NPC Schedule",fileName = "NewScheduleList")]
    public class ScheduleDetailsSo : ScriptableObject
    {
        public List<ScheduleDetails> scheduleList;
    }
}
