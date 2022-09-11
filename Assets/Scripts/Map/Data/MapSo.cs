using System.Collections.Generic;
using UnityEngine;
using Utilities;
using Utilities.CustomAttribute;

namespace Map.Data
{
    [CreateAssetMenu(menuName = "Map/MapSo",fileName = "New MapData")]
    public class MapSo : ScriptableObject
    {
        [SceneName] public string sceneNameOfMap;
        public List<TileProperty> tilePropertiesInScene;
    }
}
