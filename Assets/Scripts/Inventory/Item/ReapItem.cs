using Crop.Data;
using Crop.Logic;
using UnityEngine;
using Utilities;

namespace Inventory.Item
{
    public class ReapItem : MonoBehaviour
    {
        private CropDetails _cropDetails;

        private Transform _player;

        public void InitCropData(int id)
        {
            _cropDetails = CropManager.Instance.GetCropDetails(id);
            _player = FindObjectOfType<Player.Player>().transform;
        }
        
        public void SpawnHarvestItems()
        {
            for (var i = 0; i < _cropDetails.producedItemID.Length; i++)
            {
                int generateCount;

                if (_cropDetails.producedMaxAmount[i] == _cropDetails.producedMinAmount[i])
                    generateCount = _cropDetails.producedMinAmount[i];
                else
                    generateCount = Random.Range(
                        _cropDetails.producedMinAmount[i],
                        _cropDetails.producedMaxAmount[i] + 1);

                if (_cropDetails.generateAtPlayerPosition)
                {
                    MyEventHandler.CallHarvestAtPlayerPosition(_cropDetails.producedItemID[i], generateCount);
                    continue;
                }

                //The spawned items' position is opposite the player's position
                var dirX = transform.position.x > _player.position.x ? 1 : -1;
                var position = transform.position;
                var spawnPos = new Vector3(
                    position.x
                    + Random.Range(dirX
                        , _cropDetails.spawnRadius.x * dirX),
                    position.y
                    + Random.Range(-_cropDetails.spawnRadius.y
                        , _cropDetails.spawnRadius.y), 0);
                MyEventHandler.CallInstantiatedInScene(_cropDetails.producedItemID[i], spawnPos);
            }
            
            Destroy(gameObject);
        }
    }
}
