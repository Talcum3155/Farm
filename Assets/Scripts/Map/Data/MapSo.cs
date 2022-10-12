using System.Collections.Generic;
using UnityEngine;
using Utilities;
using Utilities.CustomAttribute;

namespace Map.Data
{
    [CreateAssetMenu(menuName = "Map/MapSo", fileName = "New MapData")]
    public class MapSo : ScriptableObject
    {
        [SceneName] public string sceneNameOfMap;
        [Header("地图信息")] public int gridWidth;
        public int gridHeight;

        [Header("左下角原点")] public int originX;
        public int originY;

        public List<TileProperty> tilePropertiesInScene;
    }
}