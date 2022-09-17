using System;
using System.Collections.Generic;
using System.Linq;
using Crop.Logic;
using Map.Data;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Utilities;

namespace Map.Logic
{
    public class GridMapManager : SingleTon<GridMapManager>
    {
        [Header("种地的瓦片信息")] public RuleTile digTile;
        public RuleTile waterTile;
        private Tilemap _digTilemap;
        private Tilemap _waterTilemap;

        [Header("地图信息")] public List<MapSo> mapSos;
        private readonly Dictionary<string, TileDetails> _tileDetailsDict = new();
        private Grid _currentGrid;
        private Season _season;
        private Transform _cropParent;

        private void OnEnable()
        {
            MyEventHandler.ExecuteActionAfterAnimation += OnExecuteActionAfterAnimationEvent;
            MyEventHandler.AfterSceneLoaded += OnAfterSceneLoadedEvent;
            MyEventHandler.GameDayEnd += OnGameDayEndEvent;
        }

        private void OnDisable()
        {
            MyEventHandler.ExecuteActionAfterAnimation -= OnExecuteActionAfterAnimationEvent;
            MyEventHandler.AfterSceneLoaded -= OnAfterSceneLoadedEvent;
            MyEventHandler.GameDayEnd -= OnGameDayEndEvent;
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
                    : new TileDetails
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

        private void SetDugTile(TileDetails details)
            => _digTilemap?.SetTile(new Vector3Int(details.gridX, details.gridY), digTile);

        private void SetWateredTile(TileDetails details)
            => _waterTilemap?.SetTile(new Vector3Int(details.gridX, details.gridY), waterTile);

        #region 事件绑定

        /// <summary>
        /// 人物动画播放完毕或执行到一定进度后触发该事件
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="itemDetails"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void OnExecuteActionAfterAnimationEvent(Vector3 pos, ItemDetails itemDetails)
        {
            var mouseGridPos = _currentGrid.WorldToCell(pos);
            //得到鼠标所位于的瓦片上的详细信息
            var currentTile = GetTileInDict(mouseGridPos);

            if (currentTile is not null)
            {
                switch (itemDetails.itemType)
                {
                    case ItemType.Seed:
                        MyEventHandler.CallPlantSeed(itemDetails.itemID, currentTile);
                        MyEventHandler.CallDropItem(itemDetails.itemID, pos, itemDetails.itemType);
                        break;
                    
                    case ItemType.Commodity:
                        MyEventHandler.CallDropItem(itemDetails.itemID, pos, itemDetails.itemType);
                        break;
                    
                    case ItemType.HoeTool:
                        SetDugTile(currentTile);
                        currentTile.daysSinceDug = 0;
                        currentTile.diggable = false;
                        currentTile.dropItem = false;
                        break;
                    
                    case ItemType.WaterTool:
                        SetWateredTile(currentTile);
                        currentTile.daysSinceWatered = 0;
                        break;
                    
                    case ItemType.ChopTool:
                        break;
                    
                    case ItemType.BreakTool:
                        break;
                    
                    case ItemType.CollectTool:
                        var cropper = GetCropObject(pos);
                        Debug.Log(cropper.cropDetails.seedItemID);
                        break;
                    
                    case ItemType.Furniture:
                        break;
                    
                    case ItemType.HarvestableScenery:
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            //更新地图信息
            UpdateTileDetailSInMap(currentTile);
        }

        private void OnAfterSceneLoadedEvent()
        {
            _currentGrid = FindObjectOfType<Grid>();
            _digTilemap = GameObject.FindWithTag("Dig").GetComponent<Tilemap>();
            _waterTilemap = GameObject.FindWithTag("Watered").GetComponent<Tilemap>();
            _cropParent = GameObject.FindWithTag("CropParent").transform;

            RefreshMap();
        }

        /// <summary>
        /// 每天更新土壤的浇水和挖掘状态
        /// </summary>
        /// <param name="day"></param>
        /// <param name="season"></param>
        private void OnGameDayEndEvent(int day, Season season)
        {
            _season = season;

            foreach (var tile in _tileDetailsDict)
            {
                //每天重置浇水状态
                if (tile.Value.daysSinceWatered > -1)
                    tile.Value.daysSinceWatered = -1;

                if (tile.Value.daysSinceDug > -1)
                    tile.Value.daysSinceDug++;

                //土壤挖的时间超过天且没有播种就让土壤恢复到没有被挖掘的状态
                if (tile.Value.daysSinceDug > 5 && tile.Value.seedItemId is -1)
                {
                    tile.Value.daysSinceDug = -1;
                    tile.Value.diggable = true;
                    tile.Value.growthDays = -1;
                }

                if (tile.Value.seedItemId is not -1)
                {
                    tile.Value.growthDays++;
                }
            }

            RefreshMap();
        }

        #endregion

        /// <summary>
        /// 将土地挖掘或者浇水后更新这块土地的状态
        /// </summary>
        /// <param name="tileDetails"></param>
        private void UpdateTileDetailSInMap(TileDetails tileDetails)
        {
            //TODO: 高性能消耗
            var key = $"{tileDetails.gridX}X{tileDetails.gridY}Y{SceneManager.GetActiveScene().name}";

            if (_tileDetailsDict.ContainsKey(key))
                _tileDetailsDict[key] = tileDetails;
        }

        /// <summary>
        /// 重新绘制当前场景的Dig和Water层
        /// </summary>
        /// <param name="sceneName"></param>
        private void DisplayMap(string sceneName)
        {
            //确定该瓦片是不是本场景的
            foreach (var tileDetails in
                     from tile
                         in _tileDetailsDict
                     let key = tile.Key
                     let tileDetails = tile.Value
                     where key.Contains(sceneName)
                     select tileDetails)
            {
                //如果这块瓦片被挖过或者浇过水就给这块瓦片绘制上挖坑和浇水
                if (tileDetails.daysSinceDug > -1)
                    SetDugTile(tileDetails);
                if (tileDetails.daysSinceWatered > -1)
                    SetWateredTile(tileDetails);
                if (tileDetails.seedItemId > -1)
                {
                    MyEventHandler.CallPlantSeed(tileDetails.seedItemId, tileDetails);
                }
            }
        }

        /// <summary>
        /// 清除Dig和Water层的瓦片，根据场景的名称
        /// 重新将之前保存的瓦片绘制上去
        /// </summary>
        private void RefreshMap()
        {
            _digTilemap?.ClearAllTiles();
            _waterTilemap?.ClearAllTiles();

            for (var i = 0; i < _cropParent.childCount; i++)
                Destroy(_cropParent.GetChild(0).gameObject);

            DisplayMap(SceneManager.GetActiveScene().name);
        }

        /// <summary>
        /// 获取鼠标点击位置的所有碰撞体，从这些碰撞体取得种子的脚本
        /// </summary>
        /// <param name="mouseWorldPos"></param>
        /// <returns></returns>
        private Cropper GetCropObject(Vector3 mouseWorldPos)
        {
            Cropper currentCrop = null;
            foreach (var c in Physics2D.OverlapPointAll(mouseWorldPos))
            {
                if (c.GetComponent<Cropper>())
                    currentCrop = c.GetComponent<Cropper>();
            }

            return currentCrop;
        }
    }
}