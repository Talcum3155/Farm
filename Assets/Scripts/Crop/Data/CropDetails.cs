using System.Linq;
using UnityEngine;
using Utilities;

namespace Crop.Data
{
    [System.Serializable]
    public class CropDetails
    {
        public int seedItemID;
        [Header("不同阶段需要的天数")]
        public int[] growthDays;
        public int TotalGrowthDays => growthDays.Sum();

        [Header("不同生长阶段物品Prefab")]
        public GameObject[] growthPrefabs;
        [Header("不同阶段的图片")]
        public Sprite[] growthSprites;
        [Header("可种植的季节")]
        public Season[] seasons;

        [Space]
        [Header("收割工具")]
        public int[] harvestToolItemID;
        [Header("每种工具使用次数")]
        public int[] requireActionCount;
        [Header("转换新物品ID")]
        public int transferItemID;

        [Space]
        [Header("收割果实信息")]
        public int[] producedItemID;
        [Tooltip("物品生成的最小数量")] public int[] producedMinAmount;
        [Tooltip("物品生成的最大数量")] public int[] producedMaxAmount;
        [Tooltip("生成物品的范围")] public Vector2 spawnRadius;

        [Header("再次生长时间")]
        public int daysToRegrow;
        public int regrowTimes;

        [Header("Options")]
        public bool generateAtPlayerPosition;
        public bool hasAnimation;
        public bool hasParticleEffect;
        //TODO:特效 音效 等

        public ParticleEffectType effectType;
        public Vector3 particlePosition;

        /// <summary>
        /// 工具是否可用来采集该果实
        /// </summary>
        /// <param name="toolId"></param>
        /// <returns></returns>
        public bool CheckToolAvailable(int toolId)
            => harvestToolItemID.Any(t => t == toolId);
        
        /// <summary>
        /// 该工具需要几次才能完成收割
        /// </summary>
        /// <param name="toolId"></param>
        /// <returns></returns>
        public int GetReapCount(int toolId)
        {
            for (var i = 0; i < harvestToolItemID.Length; i++)
            {
                if (harvestToolItemID[i] == toolId)
                    return requireActionCount[i];
            }

            return -1;
        }
    }
}
