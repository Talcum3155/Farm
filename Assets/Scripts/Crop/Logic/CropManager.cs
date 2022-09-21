using System;
using System.Linq;
using Crop.Data;
using UnityEngine;
using Utilities;

namespace Crop.Logic
{
    public class CropManager : SingleTon<CropManager>
    {
        public CropDetailsSo cropDetailsSo;
        private Transform _cropParent;
        private Grid _currentGrid;
        private Season _currentSeason;

        private void OnEnable()
        {
            MyEventHandler.PlantSeed += OnPlantSeedEvent;
            MyEventHandler.AfterSceneLoaded += OnAfterSceneLoadedEvent;
            MyEventHandler.GameDayEnd += OnGameDayEndEvent;
        }

        private void OnDisable()
        {
            MyEventHandler.PlantSeed -= OnPlantSeedEvent;
            MyEventHandler.AfterSceneLoaded -= OnAfterSceneLoadedEvent;
            MyEventHandler.GameDayEnd -= OnGameDayEndEvent;
        }


        /// <summary>
        /// 根据种子id从种子数据库中获取该种子的具体信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public CropDetails GetCropDetails(int id)
            => cropDetailsSo.cropDetailsList.Find(c => c.seedItemID == id);

        /// <summary>
        /// 种子在当前季节是否可播种
        /// </summary>
        /// <param name="crop"></param>
        /// <returns></returns>
        private bool PlantableInCurrentSeason(CropDetails crop)
            => crop.seasons.Any(s => s == _currentSeason);

        #region 事件绑定

        private void OnAfterSceneLoadedEvent()
        {
            _currentGrid = FindObjectOfType<Grid>();
            _cropParent = GameObject.FindWithTag("CropParent").transform;
        }

        private void OnPlantSeedEvent(int seedId, TileDetails tileDetails)
        {
            var cropDetails = GetCropDetails(seedId);

            //种子在当前季节可耕种且这块瓦片未被播种
            if (_currentGrid is not null && PlantableInCurrentSeason(cropDetails) && tileDetails.seedItemId is -1)
            {
                tileDetails.seedItemId = seedId;
                tileDetails.growthDays = 1;
                //播种后将种子的实例化出来
                DisplayCropPlant(tileDetails, cropDetails);
                return;
            }

            //已经播种过了，这个用在清除地图重新生成上
            DisplayCropPlant(tileDetails, cropDetails);
        }

        private void OnGameDayEndEvent(int day, Season season)
        {
            _currentSeason = season;
        }

        #endregion


        private void DisplayCropPlant(TileDetails tileDetails, CropDetails cropDetails)
        {
            //成长的总阶段数
            var growthStages = cropDetails.growthDays.Length;
            var currentStage = 0;
            var totalGrowthDays = cropDetails.TotalGrowthDays;

            //倒序计算当前种子的成长阶段
            for (var i = growthStages - 1; i >= 0; i--)
            {
                if (tileDetails.growthDays >= totalGrowthDays)
                {
                    currentStage = i;
                    // Debug.Log($"当前阶段：{currentStage}");
                    break;
                }

                totalGrowthDays -= cropDetails.growthDays[i];
            }
            
            var cropper = Instantiate(cropDetails.growthPrefabs[currentStage],
                new Vector3(tileDetails.gridX + 0.5f, tileDetails.gridY + 0.5f, 0),
                Quaternion.identity,
                _cropParent);
            cropper.transform.GetChild(0)
                .GetComponent<SpriteRenderer>()
                .sprite = cropDetails.growthSprites[currentStage];
            var cropperScript = cropper.GetComponent<Cropper>();
            //Each Cropper contains themselves' cropper info and tile info where there are 
            cropperScript.cropDetails = cropDetails;
            cropperScript.tileDetails = tileDetails;
        }
    }
}