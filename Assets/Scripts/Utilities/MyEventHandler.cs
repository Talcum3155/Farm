using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public static class MyEventHandler
    {
        /// <summary>
        /// 有索引时，更新列表对应索引的值，没有索引，更新全表
        /// </summary>
        public static event Action<InventoryLocation, List<InventoryItem>, int> UpdateInventoryUI;

        public static void CallUpdateInventoryUI(InventoryLocation location, List<InventoryItem> list, int index)
            => UpdateInventoryUI?.Invoke(location, list, index);

        /// <summary>
        /// 根据物品ID和拖拽位置在地图上生成物品
        /// </summary>
        public static event Action<int, Vector3> InstantiatedItemInScene;

        public static void CallInstantiatedInScene(int id, Vector3 pos) => InstantiatedItemInScene?.Invoke(id, pos);


        /// <summary>
        /// 将东西扔到地面上的事件
        /// </summary>
        public static event Action<int, Vector3, ItemType> DropItem;

        public static void CallDropItem(int itemId, Vector3 instantiatePos, ItemType type)
            => DropItem?.Invoke(itemId, instantiatePos, type);

        /// <summary>
        /// 背包中选择物品后触发该事件，内容包含物品详细信息以及是否被选中
        /// </summary>
        public static event Action<ItemDetails, bool> SelectedItem;

        public static void CallSelectedItem(ItemDetails item, bool isSelected) =>
            SelectedItem?.Invoke(item, isSelected);

        /// <summary>
        /// 根据名称加载场景以及移动到加载后的场景的对应位置
        /// </summary>
        public static event Action<string, Vector3> TransitionScene;

        public static void CallTransitionScene(string targetSceneName, Vector3 targetScenePosition)
            => TransitionScene?.Invoke(targetSceneName, targetScenePosition);

        /// <summary>
        /// 在场景卸载前执行的方法
        /// </summary>
        public static event Action BeforeSceneUnLoad;

        public static void CallBeforeSceneUnLoaded()
            => BeforeSceneUnLoad?.Invoke();

        /// <summary>
        /// 在场景加载后执行的方法
        /// </summary>
        public static event Action AfterSceneLoaded;

        public static void CallAfterSceneLoaded()
            => AfterSceneLoaded?.Invoke();

        /// <summary>
        /// 将玩家移动到对应位置
        /// </summary>
        public static event Action<Vector3> MoveToPosition;

        public static void CallMoveToPosition(Vector3 targetPosition)
            => MoveToPosition?.Invoke(targetPosition);

        /// <summary>
        /// Trigger when click on the ground or tree
        /// </summary>
        public static event Action<Vector3, Vector3, ItemDetails> MouseClickedEvent;

        public static void CallMouseClickedEvent(Vector3 mousePos, Vector3 treePos, ItemDetails itemDetail)
            => MouseClickedEvent?.Invoke(mousePos, treePos, itemDetail);

        /// <summary>
        /// 某些动画执行完后触发该事件
        /// </summary>
        public static event Action<Vector3, ItemDetails> ExecuteActionAfterAnimation;

        public static void CallExecuteActionAfterAnimation(Vector3 pos, ItemDetails itemDetails)
            => ExecuteActionAfterAnimation?.Invoke(pos, itemDetails);

        #region Time about

        /// <summary>
        /// 每天结束后触发事件
        /// </summary>
        public static event Action<int, Season> GameDayEnd;

        public static void CallGameDayEnd(int day, Season season)
            => GameDayEnd?.Invoke(day, season);

        /// <summary>
        /// 更新时间UI的秒和分,，由于时分秒是连在一起的，单独更新小时不太方便，所以这里也加上了小时的参数
        /// </summary>
        public static event Action<int, int, int> UpdateTime;

        public static void CallUpdateTime(int second, int minutes, int hour)
            => UpdateTime?.Invoke(second, minutes, hour);

        /// <summary>
        /// Touch off every minute, used in schedule event of npc 
        /// </summary>
        public static event Action<int, int, int, Season> GameMinuteEnd;

        public static void CallGameMinuteEnd(int minute, int hour, int day, Season season)
        {
            GameMinuteEnd?.Invoke(minute, hour, day, season);
        }

        /// <summary>
        /// 更新时间UI的小时，由于更新小时既需要更新时间槽也需要更新天色图，
        /// 所以单独抽离出来，防止同时又要重复更新其他值而浪费性能
        /// </summary>
        public static event Action<int> UpdateHour;

        public static void CallUpdateHour(int hour)
            => UpdateHour?.Invoke(hour);

        /// <summary>
        /// 更新时间UI的日期
        /// </summary>
        public static event Action<int, int, Season, int> UpdateDate;

        public static void CallUpdateDate(int day, int month, Season season, int year)
            => UpdateDate?.Invoke(day, month, season, year);

        #endregion

        /// <summary>
        /// 播种后触发该事件
        /// </summary>
        public static event Action<int, TileDetails> PlantSeed;

        public static void CallPlantSeed(int seedId, TileDetails tile)
            => PlantSeed?.Invoke(seedId, tile);

        /// <summary>
        /// display cropper sprite in player's head
        /// </summary>
        public static event Action<int, int> HarvestAtPlayerPosition;

        public static void CallHarvestAtPlayerPosition(int id, int amount)
            => HarvestAtPlayerPosition?.Invoke(id, amount);

        /// <summary>
        /// Refresh map after reap cropper
        /// </summary>
        public static event Action RefreshMap;

        public static void CallRefreshMap()
            => RefreshMap?.Invoke();

        /// <summary>
        /// Generate particle effect at specified position
        /// </summary>
        public static event Action<ParticleEffectType, Vector3> InstantiatedParticle;

        public static void CallInstantiatedParticle(ParticleEffectType particle, Vector3 position)
            => InstantiatedParticle?.Invoke(particle, position);

        /// <summary>
        /// Generate crop after scene loaded
        /// </summary>
        public static event Action GenerateCrop;

        public static void CallGenerateCrop()
            => GenerateCrop?.Invoke();
    }
}