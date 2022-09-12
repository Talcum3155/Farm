using System;
using System.Collections.Generic;
using Map.Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;

namespace Map.Logic
{
    public class GridMapManager : SingleTon<GridMapManager>
    {
        [Header("地图信息")] public List<MapSo> mapSos;
        private readonly Dictionary<string, TileDetails> _tileDetailsDict = new();
        private Grid _currentGrid;

        private void OnEnable()
        {
            MyEventHandler.ExecuteActionAfterAnimation += OnExecuteActionAfterAnimationEvent;
            MyEventHandler.AfterSceneLoaded += OnAfterSceneLoadedEvent;
        }

        private void OnDisable()
        {
            MyEventHandler.ExecuteActionAfterAnimation -= OnExecuteActionAfterAnimationEvent;
            MyEventHandler.AfterSceneLoaded -= OnAfterSceneLoadedEvent;
        }

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
                var key = $"{tileProperty.coordinate.x}X{tileProperty.coordinate.y}Y{map.sceneNameOfMap}";
                
                var details = _tileDetailsDict.ContainsKey(key)
                    ? _tileDetailsDict[key]
                    : new TileDetails()
                    {
                        gridX = tileProperty.coordinate.x,
                        gridY = tileProperty.coordinate.y,
                    };

                switch (tileProperty.gridType)
                {
                    case GridType.Diggable:
                        details.diggable = tileProperty.boolTypeValue;
                        break;
                    case GridType.DropItem:
                        details.dropItem = tileProperty.boolTypeValue;
                        break;
                    case GridType.PlaceFurniture:
                        details.placeFurniture = tileProperty.boolTypeValue;
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

        public TileDetails GetTileInDict(Vector3Int mouseGridPos)
        {
            var key = $"{mouseGridPos.x}X{mouseGridPos.y}Y{SceneManager.GetActiveScene().name}";
            return _tileDetailsDict.ContainsKey(key) ? _tileDetailsDict[key] : null;
        }

        #region 事件绑定

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="itemDetails"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void OnExecuteActionAfterAnimationEvent(Vector3 pos, ItemDetails itemDetails)
        {
            var mouseGridPos = _currentGrid.WorldToCell(pos);
            var currentTile = GetTileInDict(mouseGridPos);

            if (currentTile is not null)
            {
                switch (itemDetails.itemType)
                {
                    case ItemType.Seed:
                        break;
                    case ItemType.Commodity:
                        MyEventHandler.CallDropItem(itemDetails.itemID,pos);
                        break;
                    case ItemType.Furniture:
                        break;
                    case ItemType.HoeTool:
                    case ItemType.ChopTool:
                    case ItemType.BreakTool:
                    case ItemType.WaterTool:
                    case ItemType.CollectTool:
                        break;
                    case ItemType.HarvestableScenery:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
        
        private void OnAfterSceneLoadedEvent()
        {
            _currentGrid = FindObjectOfType<Grid>();
        }

        #endregion
    }
}