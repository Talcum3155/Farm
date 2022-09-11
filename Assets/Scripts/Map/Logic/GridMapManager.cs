using System;
using System.Collections.Generic;
using Map.Data;
using UnityEngine;
using Utilities;

namespace Map.Logic
{
    public class GridMapManager : MonoBehaviour
    {
        [Header("地图信息")] public List<MapSo> mapSos;
        private Dictionary<string, TileDetails> _tileDetailsDict = new();

        private void Start()
        {
            foreach (var mapSo in mapSos)
                InitTileDetailsDict(mapSo);
        }

        private void InitTileDetailsDict(MapSo map)
        {
            if (map is null)
                return;

            foreach (var tileProperty in map.tilePropertiesInScene)
            {
                var details = new TileDetails()
                {
                    gridX = tileProperty.coordinate.x,
                    gridY = tileProperty.coordinate.y,
                };

                var key = $"{details.gridX}X{details.gridY}Y{map.sceneNameOfMap}";

                switch (tileProperty.gridType)
                {
                    case GridType.Diggable:
                        details.diggable = tileProperty.boolTypeValue;
                        break;
                    case GridType.DropItem:
                        details.dropItem = tileProperty.boolTypeValue;
                        break;
                    case GridType.PlaceFurniture:
                        details.placeFurniture= tileProperty.boolTypeValue;
                        break;
                    case GridType.NpcObstacle:
                        details.npcObstacle = tileProperty.boolTypeValue;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                _tileDetailsDict[key] = details;
            }
        }
    }
}