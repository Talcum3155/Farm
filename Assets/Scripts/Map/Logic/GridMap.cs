using System;
using Map.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utilities;

namespace Map.Logic
{
    [ExecuteInEditMode]
    public class GridMap : MonoBehaviour
    {
        public MapSo currentMapSo;
        public GridType tileMapType;
        public Tilemap mountedGoTilemap;

        private void OnEnable()
        {
            //游戏中的时候该脚本是否正在运行，没在运行说明时编辑器模式，获取tileMap并清除地图的属性列表
            if (Application.IsPlaying(this))
                return;

            mountedGoTilemap = GetComponent<Tilemap>();
            currentMapSo.tilePropertiesInScene.Clear();
        }

        /// <summary>
        /// 将tilemap关闭后就会将绘制的地图属性添加到地图属性列表中
        /// </summary>
        private void OnDisable()
        {
            if (Application.IsPlaying(this))
                return;

            UpdateMapProperties();
            //让unity能应用修改后的
#if UNITY_EDITOR
            if (currentMapSo is not null)
                EditorUtility.SetDirty(currentMapSo);
#endif
            
        }

        private void UpdateMapProperties()
        {
            //压缩该Tilemap，去除外圈没有被画的区域，只留下实际实际绘制的内容
            mountedGoTilemap.CompressBounds();

            if (Application.IsPlaying(this) || currentMapSo is null)
                return;

            //瓦片地图左下角 瓦片地图右下角
            var cellBounds = mountedGoTilemap.cellBounds;
            var (minCoordinate, maxCoordinate)
                = (cellBounds.min, cellBounds.max);
            for (var x = minCoordinate.x; x < maxCoordinate.x; x++)
            {
                for (var y = minCoordinate.y; y < maxCoordinate.y; y++)
                {
                    //由于绘制的瓦片地图不是规则矩形，有可能该位置没有瓦片
                    var tile = mountedGoTilemap.GetTile(new Vector3Int(x, y, 0));

                    if (tile is null)
                        continue;

                    //添加此处绘制的地图到地图属性列表中
                    currentMapSo.tilePropertiesInScene.Add(new TileProperty
                    {
                        coordinate = new Vector2Int(x, y),
                        gridType = tileMapType,
                        boolTypeValue = true
                    });
                }
            }
        }
    }
}