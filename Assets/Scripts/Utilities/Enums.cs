using System;

namespace Utilities
{
    [Flags]
    public enum ItemType
    {
        Seed,
        Commodity,
        Furniture,

        //工具
        HoeTool,
        ChopTool,
        BreakTool,
        WaterTool,
        CollectTool,

        //可收割的
        HarvestableScenery
    }

    [Flags]
    public enum SlotType
    {
        Bag,
        Shop,
        Box
    }

    [Flags]
    public enum InventoryLocation
    {
        Bag,
        Box
    }
    
    public enum PartName //玩家的哪个部位
    {
        Body,
        Arm,
        Hair,
        Tool
    }
    
    public enum PartType //该部位在干什么
    {
        None,
        Carry,
        Hoe,
        Water,
        Collect,
        Break
    }

    public enum Season
    {
        春天 = 0,
        夏天 = 1,
        秋天 = 2,
        冬天 = 3
    }

    //瓦片地图的每个格子能干什么
    [Flags]
    public enum GridType
    {
        Diggable,
        DropItem,
        PlaceFurniture,
        NpcObstacle
    }
}