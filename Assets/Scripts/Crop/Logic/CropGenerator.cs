using System;
using Map.Logic;
using UnityEngine;
using Utilities;

namespace Crop.Logic
{
    public class CropGenerator : MonoBehaviour
    {
        private Grid _currentGrid;

        public int seedItemId;
        public int growthDays;

        private void Start()
        {
            _currentGrid = FindObjectOfType<Grid>();
        }

        private void OnEnable()
        {
            MyEventHandler.GenerateCrop += OnGenerateCropEvent;
        }

        private void OnDisable()
        {
            MyEventHandler.GenerateCrop -= OnGenerateCropEvent;
        }

        private void OnGenerateCropEvent()
        {
            var cropPosInGrid = _currentGrid.WorldToCell(transform.position);

            if (seedItemId < 0) return;
            
            //Get tileDetails from tile dictionary
            var tile = GridMapManager.Instance.GetTileInDict(cropPosInGrid) ?? new TileDetails();

            tile.seedItemId = seedItemId;
            tile.growthDays = growthDays;
            tile.daysSinceWatered = -1;

            GridMapManager.Instance.UpdateTileDetailSInMap(tile);
        }
    }
}