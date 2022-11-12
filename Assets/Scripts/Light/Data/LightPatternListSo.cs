using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Light.Data
{
    [CreateAssetMenu(fileName = "LightPattern_So", menuName = "Light/Light Pattern")]
    public class LightPatternListSo : ScriptableObject
    {
        public List<LightDetails> lightPatternList = new();

        /// <summary>
        /// Find LightDetails By season
        /// and lightShift as primary key 
        /// </summary>
        /// <param name="season"></param>
        /// <param name="lightShift"></param>
        /// <returns></returns>
        public LightDetails GetLightDetails(Season season, LightShift lightShift)
            => lightPatternList.Find(l => l.season == season && l.lightShift == lightShift);
    }

    [Serializable]
    public class LightDetails
    {
        public Season season;
        public LightShift lightShift;
        public Color lightColor;
        public float lightIntensity;
    }
}