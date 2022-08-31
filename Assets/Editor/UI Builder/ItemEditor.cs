using System;
using System.Collections.Generic;
using System.Linq;
using Inventory.DataSO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Utilities;


public class ItemEditor : EditorWindow
{
    private ItemDataListSo _dataBase;
    private List<ItemDetails> _detailsList;

    private VisualTreeAsset _itemRowTemplate;
    private ListView _itemListView; //UI左侧的列表
    private VisualElement _iconPreview;

    private ScrollView _itemDetailsSelection; //显示当前数据的详细信息的滚动视图
    private ItemDetails _activeItem; //当前选中的物品
    private Sprite _defaultIcon;

    private Button _add;
    private Button _remove;

    [MenuItem("Tarowy Tool/ItemEditor")]
    public static void ShowExample()
    {
        var wnd = GetWindow<ItemEditor>();
        wnd.titleContent = new GUIContent("ItemEditor");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        // VisualElement label = new Label("Hello World! From C#");
        // root.Add(label);

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UI Builder/ItemEditor.uxml");
        VisualElement labelFromUXML = visualTree.Instantiate();
        root.Add(labelFromUXML);

        _defaultIcon = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/M Studio/Art/Items/Icons/icon_M.png");

        //从root元素中查找ListView元素
        _itemListView = root.Q<VisualElement>("ItemList").Q<ListView>("ListView");
        //从root元素中查找ScrollView元素
        _itemDetailsSelection = root.Q<ScrollView>("ItemDetails");
        _iconPreview = _itemDetailsSelection.Q<VisualElement>("Icon");

        root.Q<Button>("AddButton").clicked += AddItem;
        root.Q<Button>("DeleteButton").clicked += RemoveItem;
        
        //加载模板数据
        _itemRowTemplate =
            AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Editor/UI Builder/ItemRowTemplate.uxml");

        LoadDataBase();
        //将itemRowTemplate生成到ListView中去
        GenerateListView();
    }

    private void AddItem()
    {
        var itemDetails = new ItemDetails
        {
            itemName = "New Item",
            itemID = 1001 + _detailsList.Count
        };
        _detailsList.Add(itemDetails);
        _itemListView.Rebuild();
    }

    private void RemoveItem()
    {
        _detailsList.Remove(_activeItem);
        _itemListView.Rebuild();
        _itemDetailsSelection.visible = false;
    }

    private void LoadDataBase()
    {
        //从资源文件夹中找到指定的So文件，获取其GUID，GUID是资源文件的的唯一标识符
        var dataArray = AssetDatabase.FindAssets("ItemDataList");

        if (dataArray.Length > 0)
        {
            //根据资源文件的GUID获取文件路径
            var assetPath = AssetDatabase.GUIDToAssetPath(dataArray[0]);
            //根据文件路径实例化该So
            _dataBase = AssetDatabase.LoadAssetAtPath(assetPath, typeof(ItemDataListSo)) as ItemDataListSo;
        }

        _detailsList = _dataBase != null ? _dataBase.itemDetailsList : null;
        //需要标记才能成功保存该数据
        EditorUtility.SetDirty(_dataBase);
    }

    private void GenerateListView()
    {
        VisualElement MakeItem() => _itemRowTemplate.CloneTree();

        void BindItem(VisualElement e, int i)
        {
            if (i >= _detailsList.Count) return;
            e.Q<VisualElement>("ItemIcon").style.backgroundImage = _detailsList[i].itemIcon ? _detailsList[i].itemIcon.texture : _defaultIcon.texture;
            e.Q<Label>("ItemName").text = _detailsList[i].itemName ?? "No Item";
        }

        //限制子物体高度
        _itemListView.fixedItemHeight = 50;
        _itemListView.itemsSource = _detailsList;
        //在列表中产生数据
        _itemListView.makeItem = MakeItem;
        //为列表中每个数据绑定数据
        _itemListView.bindItem = BindItem;

        _itemListView.onSelectionChange += SelectionChange;
        //开始时隐藏ListView
        _itemDetailsSelection.visible = false;
    }

    private void SelectionChange(IEnumerable<object> obj)
    {
        _activeItem = obj.First() as ItemDetails;
        GetItemDetails();
        //有数据被选中时才显示ListView
        _itemDetailsSelection.visible = true;
    }

    private void GetItemDetails()
    {
        //需要标记才能让数据的更改生效
        _itemDetailsSelection.MarkDirtyRepaint();

        _iconPreview.style.backgroundImage = _activeItem.itemIcon ? _activeItem.itemIcon.texture : _defaultIcon.texture;

        var itemId = _itemDetailsSelection.Q<IntegerField>("ItemId");
        itemId.value = _activeItem.itemID;
        itemId.RegisterValueChangedCallback(evt => { _activeItem.itemID = evt.newValue; });

        var itemName = _itemDetailsSelection.Q<TextField>("ItemName");
        itemName.value = _activeItem.itemName;
        itemName.RegisterValueChangedCallback(evt =>
        {
            _activeItem.itemName = evt.newValue;
            _itemListView.Rebuild();
        });

        var itemIcon = _itemDetailsSelection.Q<ObjectField>("ItemIcon");
        itemIcon.value = _activeItem.itemIcon ? _activeItem.itemIcon : _defaultIcon;
        itemIcon.RegisterValueChangedCallback(evt =>
        {
            var newSprite = evt.newValue as Sprite;
            _activeItem.itemIcon = newSprite;
            _iconPreview.style.backgroundImage = newSprite ? newSprite.texture : _defaultIcon.texture;
            _itemListView.Rebuild();
        });

        var itemType = _itemDetailsSelection.Q<EnumField>("ItemType");
        itemType.Init(_activeItem.itemType);
        itemType.value = _activeItem.itemType;
        itemType.RegisterValueChangedCallback(evt =>
        {
            _activeItem.itemType = evt.newValue is ItemType type ? type : ItemType.Seed;
        });

        var itemSprite = _itemDetailsSelection.Q<ObjectField>("ItemSprite");
        itemSprite.value = _activeItem.itemOnWorldSprite;
        itemSprite.RegisterValueChangedCallback(evt =>
        {
            var newSprite = evt.newValue as Sprite;
            _activeItem.itemOnWorldSprite = newSprite;
        });

        var description = _itemDetailsSelection.Q<TextField>("Description");
        description.value = _activeItem.itemDescription;
        description.RegisterValueChangedCallback(evt => { _activeItem.itemDescription = evt.newValue; });

        var itemUseRadius = _itemDetailsSelection.Q<IntegerField>("ItemUseRadius");
        itemUseRadius.value = _activeItem.itemUseRadius;
        itemUseRadius.RegisterValueChangedCallback(evt => { _activeItem.itemUseRadius = evt.newValue; });

        var canPickedUp = _itemDetailsSelection.Q<Toggle>("CanPickedUp");
        canPickedUp.value = _activeItem.canPickedUp;
        canPickedUp.RegisterValueChangedCallback(evt => { _activeItem.canPickedUp = evt.newValue; });

        var canDropped = _itemDetailsSelection.Q<Toggle>("CanDropped");
        canDropped.value = _activeItem.canDropped;
        canDropped.Q<Toggle>("CanDropped").RegisterValueChangedCallback(evt =>
        {
            _activeItem.canDropped = evt.newValue;
        });

        var canCarried = _itemDetailsSelection.Q<Toggle>("CanCarried");
        canCarried.value = _activeItem.canCarried;
        canCarried.RegisterValueChangedCallback(evt => { _activeItem.canCarried = evt.newValue; });

        var itemPrice = _itemDetailsSelection.Q<IntegerField>("ItemPrice");
        itemPrice.value = _activeItem.itemPrice;
        itemPrice.RegisterValueChangedCallback(evt => { _activeItem.itemPrice = evt.newValue; });

        var sellPercentage = _itemDetailsSelection.Q<Slider>("SellPercentage");
        sellPercentage.value = _activeItem.sellPercentage;
        sellPercentage.RegisterValueChangedCallback(evt => { _activeItem.sellPercentage = evt.newValue; });
    }
}