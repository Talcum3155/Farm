using System;
using Crop.Data;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace Crop.Logic
{
    public class Cropper : MonoBehaviour
    {
        public CropDetails cropDetails;
        public TileDetails tileDetails;
        public int harvestActionCount;

        public bool Mature
            => tileDetails.growthDays > cropDetails.TotalGrowthDays;

        public Animator _animator;
        public Transform _player;
        private static readonly int RotateLeft = Animator.StringToHash("RotateLeft");
        private static readonly int RotateRight = Animator.StringToHash("RotateRight");
        private static readonly int FallRight = Animator.StringToHash("FallRight");
        private static readonly int FallLeft = Animator.StringToHash("FallLeft");

        private void Start()
        {
            _player = GameObject.FindWithTag("Player").transform;
            if (!cropDetails.hasAnimation) return;
            _animator = transform.GetChild(0).GetComponentInChildren<Animator>();
        }

        public async void ProcessToolAction(ItemDetails tool)
        {
            var reapCount = cropDetails.GetReapCount(tool.itemID);
            if (reapCount is -1)
                return;
            //If has Animation

            //click counter
            if (harvestActionCount < reapCount)
            {
                harvestActionCount++;

                //there are will be an animator if the crop is a tree
                if (_animator != null) //cannot use Pattern matching
                {
                    _animator.SetTrigger(_player.position.x < transform.position.x ? RotateRight : RotateLeft);
                }

                //play particle effect
                if (cropDetails.hasParticleEffect)
                    MyEventHandler.CallInstantiatedParticle(cropDetails.effectType,
                        transform.position + Vector3.up * 2.5f);

                    //play sound
                return;
            }

            if (harvestActionCount >= reapCount)
            {
                if (cropDetails.generateAtPlayerPosition || !cropDetails.hasAnimation)
                {
                    //generate cropper in Player Head
                    SpawnHarvestItems();
                    return;
                }

                if (_player.position.x < transform.position.x)
                    _animator.SetTrigger(FallRight);
                else
                    _animator.SetTrigger(FallLeft);

                //wait for animation end
                while (!_animator.GetCurrentAnimatorStateInfo(0).IsName("END"))
                {
                    await UniTask.Yield();
                }

                SpawnHarvestItems();
                if (cropDetails.transferItemID > 1000)
                    GenerateTransferItem();
            }
        }

        private void SpawnHarvestItems()
        {
            for (var i = 0; i < cropDetails.producedItemID.Length; i++)
            {
                int generateCount;

                if (cropDetails.producedMaxAmount[i] == cropDetails.producedMinAmount[i])
                    generateCount = cropDetails.producedMinAmount[i];
                else
                    generateCount = Random.Range(
                        cropDetails.producedMinAmount[i],
                        cropDetails.producedMaxAmount[i] + 1);

                if (cropDetails.generateAtPlayerPosition)
                {
                    MyEventHandler.CallHarvestAtPlayerPosition(cropDetails.producedItemID[i], generateCount);
                    continue;
                }

                //The spawned items' position is opposite the player's position
                var dirX = transform.position.x > _player.position.x ? 1 : -1;
                var position = transform.position;
                var spawnPos = new Vector3(
                    position.x
                    + Random.Range(dirX
                        , cropDetails.spawnRadius.x * dirX),
                    position.y
                    + Random.Range(-cropDetails.spawnRadius.y
                        , cropDetails.spawnRadius.y), 0);
                MyEventHandler.CallInstantiatedInScene(cropDetails.producedItemID[i], spawnPos);
            }

            if (tileDetails is null)
                return;

            //The seed can regrow
            if (cropDetails.daysToRegrow > 0 && tileDetails.daysSinceLastHarvest < cropDetails.regrowTimes - 1)
            {
                tileDetails.daysSinceLastHarvest++;
                tileDetails.growthDays
                    = cropDetails.TotalGrowthDays
                      - cropDetails.daysToRegrow;
                MyEventHandler.CallRefreshMap();
                Destroy(gameObject);
                return;
            }

            tileDetails.seedItemId = -1;
            tileDetails.daysSinceDug = -1;
            Destroy(gameObject);
        }

        private void GenerateTransferItem()
        {
            tileDetails.seedItemId = cropDetails.transferItemID;
            tileDetails.daysSinceLastHarvest = -1;
            tileDetails.growthDays = 0;

            MyEventHandler.CallRefreshMap();
        }
    }
}