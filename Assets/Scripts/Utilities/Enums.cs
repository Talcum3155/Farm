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
}