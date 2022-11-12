# M Farm笔记

## 游戏部分

### 第一节 展示

### 第二节 创建项目

1. 创建项目，从Pack Manager中删除不必要的扩展以提升速度。
2. scene中按下空格可弹出显示或隐藏组件功能的窗口

### 第三节 导入

1. 导入素材进行切割，可以通过图片编辑器的右上角将当前的参数设置为preset以一次性应用到所有图片
2. Sprite Editor里Type使用Grid By Cell Size，以20为像素精度切割，Method使用Smart可以忽略不包含图片的格子

### 第四节 创建Player

1. 调整头、身体、手图片的pivot为底部，然后Sprite Sort Point选择pivot，将三个部位组装成一个完整的身体，父物体使用Sorting Group可以将整个GO当成一个sorting layer的整体
2. Project Settings-Graphic-Camera Settings可以更改渲染轴，俯视角为x=0,y=1,z=0，这样当sorting layer一样时，y坐标小的会显示在前面

### 第五节 基本移动

&ensp;&ensp;核心移动代码：Horizontal和Vertical组成Vector 2，参与位置移动运算

```csharp
private void MoveMoment() =>
            _rigidbody2D.MovePosition(
                _rigidbody2D.position 
                + _movementInput * (moveSpeed * Time.deltaTime));
```

### 第六节 创建地图结构

1. 创建Tilemap，需要从Unity仓库中导入
2. 将Tilemap移动到另一个场景，再将该场景拖拽到Hierarchy中即可同时加载两个场景
3. Tile Palette上方工具按钮空白处右键最后一个选项可以进入设置，能将翻转功能添加进去

### 第七节 绘制地图

1. Sprite Editor里将切分的地图块改名可以快速查找到该地图快
2. 文件夹右键2D-Tiles-Rule Tile可以创建规则地图
3. Tile Palette下方左上角可以选择Random Brush，能选择几块地图随机刷上去

### 第八节 摄像机跟随

1. unity仓库安装Cinemachine便可在Hierarchy中添加Cinemachine
2. 2D场景有缝隙，可以文件夹中右键2D-Sprite-Sprite Atlas添加图集，再将包含地图的文件夹拖拽进去，使用Pack Preview打包图集
3. camera添加pixel Prefect组件可以根据像素比例将摄像机的位置调整到最佳距离，然后Cinemachine添加Cinemachine pixel
   prefect扩展以调用camera的pixel Prefect；根据camera中被pixel Prefect改变的size设置Cinemachine的Lens Ortho Size
4. 调整Cinemachine的dead zone来设置跟随的缓存距离

### 第九节 碰撞层和景观树

1. 单独用一个Tilemap层来绘制碰撞层
2. sprite Editor中可以更改physics shape来改变该图片用作Tilemap的Composite碰撞体时的碰撞体积
3. 用创建人物的方法创建一棵树，树的树干需要改变pivot让图层能以正确的顺序渲染

### 第十节 摄像机边界

1. Cinemachine添加Cinemachine Confiner扩展，将碰撞边界的GO拖拽给Confiner以限制摄像机距离，碰撞边界GO使用Polygon Collider2D碰撞体来包裹整个地图，需要设置为trigger
2. 由于碰撞边界GO不能跨场景拖拽，所以需要用代码在更换场景时来寻找碰撞边界GO，热更换碰撞边界会导致刷新不及时，所以需要手动清除缓存

```c#
private void SwitchConfinerShape()
        {
            var boundGameObject = GameObject.FindGameObjectWithTag("BoundsConfiner").GetComponent<PolygonCollider2D>();
            var cinemachineConfiner = GetComponent<CinemachineConfiner>();

            cinemachineConfiner.m_BoundingShape2D = boundGameObject;
            //切换碰撞边界后不会立即生效，需要清除缓存
            cinemachineConfiner.InvalidatePathCache();
        }
```

### 第十一节 实现景观物体遮挡半透明

1. 给树的根部添加碰撞体，其余的部分添加触发体，被player触发后修改sprite Renderer的透明度

2. 使用dotween来线性修改透明度

   ```csharp
   public void FadeIn()
           {
               _leaves.DOColor(new Color(1, 1, 1, 1), Settings.FadeDuration);
               _trunk.DOColor(new Color(1, 1, 1, 1), Settings.FadeDuration);
           }
   
           public void FadeOut()
           {
               _leaves.DOColor(new Color(1, 1, 1, Settings.TargetAlpha), Settings.FadeDuration);
               _trunk.DOColor(new Color(1, 1, 1, Settings.TargetAlpha), Settings.FadeDuration);
           }
   ```

3. 创建Settings来储存全局使用的变量，方便修改

### 第十二节 背包数据初始化

1. 单独创建一个类来保存各种数据和各种枚举数据
2. 使用ItemDataSo来创建物品数据

## Editor编写

### 第十三节 使用 UI Toolkit 和 UI Builder 制作物品编辑器

1. 右键UI ToolKit-Editor Window创建三个编辑代码ItemEditor

2. 从Window-UIToolKit可以打开创建的编辑器

3. 编辑器ItemEditor代码的C#版本里可以编辑代码，通过特性可以更改编辑器所处的位置

   ```csharp
   前：[MenuItem("Window/UI Toolkit/ItemEditor")]
   后：[MenuItem("Tarowy Tool/ItemEditor")]
   ```

4. 双击ItemEditor代码的uxml可以打开UI Builder来编辑工具

### 第十四节 创建 ListView 中的 ItemTemplate

1. 右键UI ToolKit-UI Document可以创建一个UI Builder文件以制作UI模板，在代码中可以调用这个模板，将其添加到List中
2. window-UI ToolKit-Samples可以看到编辑器工具UI的样例

### 第十五节  生成 ListView 列表

1. itemEditor的C#版本里添加读取数据的代码，在初始化UI后读取So的数据

   ```csharp
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
   ```

2. 在代码中加载上一节创建的模板，将So的数据填充进去，再将其添加到编辑器ListView中

   ```csharp
    private void GenerateListView()
    {
        VisualElement MakeItem() => _itemRowTemplate.CloneTree();

        void BindItem(VisualElement e, int i)
        {
            if (i >= _detailsList.Count) return;
            e.Q<VisualElement>("ItemIcon").style.backgroundImage = _detailsList[i].itemIcon?.texture;
            e.Q<Label>("ItemName").text = _detailsList[i].itemName ?? "No Item";
        }

        _itemListView.itemsSource = _detailsList;
        //在列表中产生数据
        _itemListView.makeItem = MakeItem;
        //为列表中每个数据绑定数据
        _itemListView.bindItem = BindItem;
    }
   ```
   
### 第十六节 绑定 Editor Window 中的参数变量

1. 分别获取So的详细数据和ScrollView元素的数据，将So的数据显示到ScrollView中 
2. 使用`RegisterValueChangedCallback`侦测UI中数据的变动并将对应的数据也改变

   ```csharp
      var itemId = _itemDetailsSelection.Q<IntegerField>("ItemId");
        itemId.value = _activeItem.itemID;
        itemId.RegisterValueChangedCallback(evt => { _activeItem.itemID = evt.newValue; });
   ```

3. 如果So数据中的图片没有数据，则需要将编辑器工具的图片设置为默认值，否则会报错无法运行

### 第十七节 实现 ListView 添加删除同步信息功能

1. 直接从root元素中查找按钮，然后订阅事件

   ```csharp
   root.Q<Button>("AddButton").clicked += AddItem;
   root.Q<Button>("DeleteButton").clicked += RemoveItem;
   ```
## 继续游戏部分

### 第十八节 填写物品数据库

### 第十九节 创建 InventoryManager 和 Item

1. 创建物品数据库单例以便其他代码调用
2. 创建一个**itemBase**的GO来显示**itemBase**对应ID的物品图片，对于不同的**Sprite**的大小以及其锚点的位置需要用代码将触发体修改成不同尺寸，`sprite.bounds.center`获取bounds相对锚点的坐标

   ```csharp
   private void Init(int id)
        {
            itemId = id;
            _itemDetails = InventoryManager.Instance.GetItemDetails(id);

            if (_itemDetails is null) return;
            var sprite = _itemDetails.itemOnWorldSprite ? _itemDetails.itemOnWorldSprite : _itemDetails.itemIcon;
            _spriteRenderer.sprite = sprite;

            _boxCollider2D.size = new Vector2(sprite.bounds.size.x,sprite.bounds.size.y);
            //锚点在不在中心时，需要让碰撞体的y轴偏移到中心点
            _boxCollider2D.offset = Vector2.up * sprite.bounds.center.y;
        }
    }
   ```
   
3. Game窗口右上角可以设置**Play Unforced**，这样运行游戏时不会使焦点切换到Game窗口中

### 第二十节 拾取物品基本逻辑

1. 靠近物品且物品是**CanPickedUp**的就将其捡起来

### 第二十一节 实现背包的数据结构

1. 创建玩家背包物品数据库
2. 捡到物品后将其添加到玩家的背包中，需要考虑背包已经有该物品以及背包是否还有空格的问题

### 第二十二节 实现背包检查和添加物品

1. 背包有空位就在空位添加物品，如果已经有该物品就直接让该物品数量加上捡取的物品的数量

### 第二十三节 制作 Action Bar UI

1. 创建一个新的**scene**，在其中创建**Canvas**，**640 * 380**的size
2. 使用**Layout Group**排列ActionBar上的Slot，对于**Layout Group**中不想被其管束的元素可以为其加上**Layout Element**

### 第二十四节 制作人物背包内的UI

1. 使用**Grid Layout Group**排列背包内的格子

### 第二十五节 SlotUI 根据数据显示图片和数量

1. **Button**的**Navigation**可以在使用键盘操控的时候从一个格子切换到另一个格子，点击**Visible**可以看见切换的线条
2. **Unity**编辑器拖拽组件比`Awake()`中获取更快，对私有变量使用`[SerializeField]`特性可以使其显示在**Unity**的编辑器中
3. 为**Slot**创建枚举，定义其是什么类型的**Slot**

### 第二十六节 背包UI显示

1. 将背包的格子和PlayerBag的数据一一匹配
2. 使用静态事件订阅修改UI的方法，以便其他函数调用

   ```csharp
   public static event Action<InventoryLocation, List<InventoryItem>, int> UpdateInventoryUI;

   public static void CallUpdateInventoryUI(InventoryLocation location, List<InventoryItem> list, int index)
            => UpdateInventoryUI?.Invoke(location, list, index);
   ```
   
### 第二十七节 控制背包打开和关闭

1. `bag.activeInHierarchy`可以获取GO在Hierarchy面板中的激活状态，可以用这个来控制背包的关闭与打开

### 第二十八节 背包物品选择高亮显示和动画

1. 选中一个格子时激活高亮GO，其他格子的高亮需要关闭，`IPointerClickHandler`可以检测鼠标的点击
2. 使高亮成GO为一个动画

### 第二十九节 创建 DragItem 实现物品拖拽跟随显示

1. 在Inventory的**Canvas**创建一个新的**Canvas**，**Pixel Prefect**选择none，inherit会让该**Canvas**继承上一个**Canvas**，**Override Sorting**设置比UI画布高一层以覆盖，不需要**Canvas Scaler**
2. 在拖拽的画布中创建一个Image用来存放被拖拽的Slot物品的Image，同样需要关闭其射线
3. 拖拽开始事件：`IBeginDragHandler`，拖拽中事件：`IDragHandler`，拖拽结束事件：`IEndDragHandler`
4. 关闭BagSlot所有子物体和Tmp的**RayCast Target**，以便鼠标的射线能击中对应的格子以便获取格子的数据

### 第三十节 实现拖拽物品交换数据和在地图上生成物品

1. 依靠`IEndDragHandler`结束事件的eventHandler参数获取拖拽的终点是否是格子以及具体是哪个格子
2. 格子类型不同需要进行转换，考虑目标格子是否是空格子
3. 如果拖拽的目标是地上，需要将屏幕坐标转换为世界坐标，在对应坐标处生成该物品

### 第三十一节 制作 ItemTooltip 的 UI

1. 创建一个ItemTooltip的GO用来显示物品的信息，为其添加**Vertical Layout Group**以规划垂直布局，勾选**Control Child Size**的width限制元素宽度，添加**Content Size Fitter**并选择**Vertical Fit**为**Preferred Size**以自适应高度
2. 创建Top(显示名字和类型)、Middle(显示描述)、Bottom(显示价格)的子元素，Middle中的描述需要添加**Content Size Fitter**并选择**Vertical Fit**为**Preferred Size**来适应不同物品的描述字数
3. Top的**Vertical Layout Group**控制子元素的高度和宽度来让子元素贴紧
4. Middle也需要勾选**Vertical Layout Group**的控制宽高以及添加**Content Size Fitter**，这样子元素边长后自己也会跟着变长
5. Bottom的文字可以添加**Shadow**组件来添加阴影，这个文字需要用text(legacy)。如果用TMP，设置其阴影会导致所有的TMP都被一起设置

### 第三十二节 实现根据物品详情显示 ItemTooltip

1. 让ItemDetail的数据分别对应上ItemTooltip的UI
2. 种子、家具、图纸才需要显示价格，对于处于不同类型格子中的物品也需要区分显示卖价和买价
3. 为BagSlot单独创建一个Script来根据鼠标的位置显示ItemTooltip，不需要显示ItemTooltip的BagSlot可以不挂载该组件，使用`IPointerEnterHandler`,`IPointerExitHandler`来判断鼠标是否移入
4. ItemDetail的Description文本的长度改变后不能即使刷新**Content Size Fitter**来自适应高度，需要使用`LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>())`来强制刷新布局

### 第三十三节 制作 Player 的动画

1. 创建一个包含空动画的基地动画状态机，动画状态机中创建两个混合树，**Idle**用来控制静止动画，**WalkAndRun**用来控制奔跑和走路的切换
2. **WalkAndRun**里再创建四个混合树，用来控制上下左右动画的切换，当切换到左右/上下动画时，再根据具体的x/y轴的输入来控制奔跑和行走
3. 手、足、身体分别创建一个**Animation Override Controller**以空动画状态机为基地的动画状态机来分别控制各部位的动画

### 第三十四节 实现选中物品触发举起动画

1. 创建一个**Arm_Hold**的**Animation Override Controller**，用Hold动画填充，当举起物品时就将动画机切换成这个
2. 创建**PartName**和**PartType**的枚举，一个**PartName**和一个**PartType**共同决定了是哪个Override动画状态机，创建一个字典映射动画控制机所处GO的名称到具体的动画控制机
3. 需要标记当前选的是哪个物品，如果该物品能举起，就启动举起物品的动画状态机

### 第三十五节 绘制房子和可以被砍伐的树

1. 将房子添加到**Palette**，房子的前半部分被人物遮挡，后半部分遮挡人物，所以需要处于不同层级
2. 能砍伐的树木由上半部分和下班部分拼接而成，为树的整体添加**Sorting Group**
3. 添加半透明脚本

### 第三十六节 构建游戏的时间系统

1. 创建一个Manger来更新时间，在Settings里添加阈值，当时间超过阈值时就产生进位

### 第三十七节 时间系统 UI 制作

1. 右上角添加状态UI，使用将天色图片旋转的方式更替天色图，为天色图的父物体添加一个**Mask**组件，就可以只显示天色图的其中一片图
2. 状态UI内有六个格子，每个格子代表四个小时。在状态UI下方添加日期和时间的UI，以及代表季节的图片
3. 最右上角的地方添加设置按钮，对设置按钮的图片勾选上Read/Write，这样点击图片的空白处就不会触发按钮

### 第三十八节 代码链接 UI 实现时间日期对应转换

1. 使用两个事件更新日期，一个事件更新不怎么频繁刷新的日历，一个事件更新频繁刷新的时间
2. 使用**Dotween**控制天色图的旋转

### 第三十九节 第二场景的绘制指南

1. 绘制 Collision 层碰撞
2. 添加 Bounds 设置摄像机边界
3. 创建 ItemParent 并设置 Tag 
4. 别忘了把**Sprite Sort Point**设置为**Pivot**否则透视关系会不太对
### 第四十节 创建 TransitionManager 控制人物场景切换

1. 引入**UniTask**使用`SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive).ToUniTask()`转换成UniTask以异步加载场景

   ```csharp
   private async UniTask LoadSceneSetActive(string sceneName)
        {
            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive).ToUniTask(
                Progress.Create<float>((f) => { Debug.Log($"加载进度 {f}"); }));
            //将加载的场景设置为激活态，这样SceneManager.GetActiveScene()就能获取该场景的名称
            SceneManager.SetActiveScene(SceneManager.GetSceneAt(SceneManager.sceneCount - 1));
        }
   ```
   
2. 创建一个触发体，当玩家触碰时，会传送到触发体上标记的场景以及对应的位置

### 第四十一节 实现人物跨场景移动以及场景加载前后事件

1. 创建两个事件，分别在卸载场景前、加载场景后执行事件，以及一个让玩家移动到另一个对应场景的事件
2. 改变镜头边界、 **ItemManager**中寻找**ItemParent**的函数都需要在加载另一个场景之后执行一次
3. 如果出场景前举着物品，物品栏也处于高亮，那么在切换场景后，需要卸下物品且取消高亮

### 第四十二节 制作 [SceneName] Attribute 特性

1. 创建一个**SceneNameAttribute**和一个**SceneNameDrawer**类，一个用来标记切换场景类中的场景名称字段，一个用来给被标记的字段填充场景名称

   ```csharp
    private void GetSceneNameArray(SerializedProperty property)
        {
            var scenes = EditorBuildSettings.scenes;
            SceneNames = new GUIContent[scenes.Length];

            for (var i = 0; i < SceneNames.Length; i++)
            {
                //从场景路径分割出路径和场景名称
                var splitPath = scenes[i].path.Split(_splitSeparators, StringSplitOptions.RemoveEmptyEntries);
                //如果有场景名称就获取场景，如果场景被删除了就返回(Deleted Scene)
                SceneNames[i] = new GUIContent(splitPath.Length is > 0 ? splitPath[^1] : "(Deleted Scene)");
            }

            if (SceneNames.Length is 0)
                SceneNames = new[] { new GUIContent("Check Your Build Settings") };

            SceneIndex = 0;
            /*
             * 如果字段本来就有值，检测这个值在不在场景列表里，这个行为是为了检测场景名称有没有拼错
             * 如果字段没有值，将0索引的值赋值到字段中
             */
            if (!string.IsNullOrEmpty(property.stringValue))
            {
                for (var i = 0; i < SceneNames.Length; i++)
                {
                    if (!SceneNames[i].text.Equals(property.stringValue))
                        continue;
                    SceneIndex = i;
                    break;
                }
            }
            property.stringValue = SceneNames[SceneIndex].text;
        }
    }
   ```
   
### 第四十三节 场景切换淡入淡出和动态 UI 显示

1. 创建一个覆盖全屏幕的image，添加**Canvas Group**使用异步更改alpha值来控制淡入淡出
2. 根据过度时间计算出alpha变化的速度，由于是alpha是浮点数，可能会有微小误差，所以使用`Mathf.Approximately(a,b)`来判断两数是否非常接近
3. 当切换场景时，如果fade还未结束就不能让玩家再次切换场景

### 第四十四节 保存和加载场景中的物品

1. 创建一个`SerializableVector3`和`SceneItem`储存物品的ID和位置信息
2. 创建一个字典`Dictionary<场景名称,List<SceneItem>>>`，每次退出场景时遍历场景中的itemParent的子物体，将其保存到该场景名称下的字典中，进入场景时再根据场景名称读取出列表将其重新生成

### 第四十五节 设置鼠标指针根据物品调整

1. 如果直接将Sprite设置成Cursor类型，会导致该图片无法修改，如果要把Cursor做成动画就更复杂了。所以另一种方法是创建一个Canvas，让鼠标的图片跟随鼠标移动
2. 鼠标图片的pivot不一致，可以使用图片编辑工具将鼠标的图片移到同一个像素区域内，让它们以同一个像素坐标开始
3. 选择工具、种子、商品后鼠标会变成不同的类型，取消选择后鼠标变回默认图片。需要注意鼠标在UI上时，无论当前选择是什么鼠标都得变回默认图片，这个需要依靠**EventSystem**来实现

### 第四十六节 构建地图信息系统

1. 规定地图上哪些地方能挖坑，哪些地方能干什么。可以为**Grid**挂载**Grid Information**来实现，但是这样有些限制，最好还是自己写逻辑。
2. 新创建一个**tileMap**名为GridProperties用来设定被框画的范围的地图属性。类**TileProperty**用来储存格子的坐标、这个格子可以干什么。创建一个地图的SO包含**SceneName**和`List<TileProperty>`用来储存不同场景中每个格子的作用
3. 类**Grip Map**挂载在GridProperties的每个tileMap层上，当在不同的tileMap层上绘制时，会根据tileMap层的GirdType来将绘制的网格加入到`List<TileProperty>`中，记录每个网格能干什么
4. **Grip Map**需要使用`[ExecuteInEditMode]`让该脚本只有处于编辑器模式下才能运行，这样编辑器里一关闭**Grip Map**，该tileMap层网格的属性就会被重新添加到`List<TileProperty>`中

   ```csharp
   private void UpdateMapProperties()
        {
            //压缩该Tilemap，去除外圈没有被画的区域，只留下实际实际绘制的内容
            mountedGoTilemap.CompressBounds();
            if (Application.IsPlaying(this) || currentMapSo is null) return;
            //瓦片地图左下角 瓦片地图右下角
            var cellBounds = mountedGoTilemap.cellBounds;
            var (minCoordinate, maxCoordinate) = (cellBounds.min, cellBounds.max);
            for (var x = minCoordinate.x; x < maxCoordinate.x; x++)
                for (var y = minCoordinate.y; y < maxCoordinate.y; y++)
                {
                    //由于绘制的瓦片地图不是规则矩形，有可能该位置没有瓦片
                    var tile = mountedGoTilemap.GetTile(new Vector3Int(x, y, 0));
                    if (tile is null) continue;
                    //添加此处绘制的地图到地图属性列表中
                    currentMapSo.tilePropertiesInScene.Add(new TileProperty()
                    {
                        coordinate = new Vector2Int(x, y),
                        gridType = tileMapType,
                        boolTypeValue = true
                    });
                }
        }
   ```

### 第四十七节 生成地图数据

1. 类**TileDetails**储存格子的坐标以及各种信息，**GripMapManager**中根据格子坐标生成的名称来对应上每个**TileDetails**
2. 利用**Grid**中的实例化方法`currentGrid.WorldToCell(_mouseWorldPos);`将鼠标的位置转换成对应瓦片地图的网格坐标

### 第四十八节 设置鼠标可用状

1. 当持有物品时，需要检测鼠标所处的位置能不能让该物品产生行为，根据行为与否改变鼠标的可用与不可用状态
2. 物品的距离属性可以派上用场了，如果鼠标的位置距离玩家过远，强制无法产生行为

### 第四十九节 实现鼠标选中物品后的场景点击事件流程

1. 选中可抛出的物品后点击世界地图可以将物品丢出去，需要保证物品数量会减少，且丢光物品之后取消高亮

### 第五十节 制作可以扔出来的物品

1. **BounceItemBase**具有一个子物体显示是什么物品，另一个子物体是一个影子图片
2. 当玩家点击地面时，会在玩家脚底生成该GO，将其显示图片的子物体的位置更改到头顶。影子才是该物体的本体，子物体的图片逐渐向影子靠拢，即向y做匀速运动，而影子则向抛出的目标位置移动，做x方向的匀速运动，子物体图片和影子应该同时到达目标点

   ```csharp
   private void Bounce()
        {
            _landed = itemTrans.position.y <= transform.position.y;
            if (Vector3.Distance(_targetPos, transform.position) > 0.1f)
                //水平方向根据玩家的的扔出速度改变位置
                transform.position += (Vector3)_throwDirection * (throwAcceleration * Time.deltaTime);
            if (!_landed)
            {
                //竖直方向根据速度改变位置
                itemTrans.position += Vector3.down * (_landSpeed * Time.deltaTime);
                return;
            }
            coll.enabled = true;
            //落地后删除脚本节省性能
            Destroy(this);
        }
   ```
   
### 第五十一节 实现挖坑和浇水的地图更改变化

1. 创建一个挖过的土壤和浇水后的土壤的规则瓦片地图，使其能随挖坑大小变化形状
2. 点击地图后使用`TileMap.SetTile()`来根据点击的Grid位置切换地图上该处的挖过或浇水的规则瓦片
3. 需要同时改变鼠标在不同持有物下的可用状态以及动画

### 第五十二节 制作人物使用工具的动画和流程

1. 为**BaseController**添加一个使用工具的混合树，根据鼠标点击的方位来切换人物的转向并使用工具，所有继承**BaseController**的动画状态机也会出现刚添加的使用工具的混合树的动画槽位
2. 混合树播放完毕后直接过渡到**Exit**，动画会重新从**Entry**开始执行
3. 制作每个部位的使用工具的动画和浇水的动画

### 第五十三节 （Map）随着时间变化刷新地图显示内容

1. 每次挖掘或者浇水后都需要更新这块瓦片的状态
2. 每天结束后都需要更新已挖掘和已浇水瓦片的天数信息，并重绘**Dig**和**Water**层的瓦片

### 第五十四节 （Crop）种子数据库制作

1. 单独创建一个类包含种子的各种信息，再使用SO填写数据

### 第五十五 （Crop）制作 CropManager 实现撒种子的事件

1. **CropManger**用来管理种子的信息
2. **CursorManager**和**GridMapManager**中添加种子的判断逻辑，需要判断该瓦片是否被挖掘过以及是否已经有种子了
3. 播种时需要判断当前季节是否能播种

### 第五十六节 （Crop）种子成长过程

1. **GridMapManager**中的每日时间在每天结束后刷新已经播种过种子的瓦片的种子天数
2. 为种子添加**Crop**脚本来单独控制
3. **GridMapManager**中每天摧毁所有瓦片的种子对象并重新创建

### 第五十七节 （Crop）实现菜篮子收割庄稼的行为

1. **CursorManager**中补充收集工具的逻辑，将**CropManger**设置为单例，用其中的方法判断瓦片的种子是否为空且种子的已生长日期是否大于种子的生长日期
2. 使用`Physics2D.OverlapPointAll(mouseWorldPos)`API可以获取鼠标位置的所有碰撞体，从碰撞体中获得**Cropper**的脚本从而获取种子的详细信息
3. 制作使用收集工具的动画

### 第五十八节 （Crop）实现收割庄稼产生果实

1. 果实成熟后判断当前工具是否可用
2. 从随机生成最大值最小值之间的果实，可以将得到的物品图片显示在玩家的头上告诉玩家得到了什么

### 第五十九节 （Crop）实现农作物的重复收割

1. 如果当前植物可收割，收割后将上一次采集日期的参数++，并减去重生日期使其回到之前的生长阶段

### 第六十节 （Crop）制作可砍伐的树木摇晃和倒下动画

1. 为可砍伐树木添加***Cropper**脚本使其变成一个庄稼物品，最后一个阶段的预制体需要替换成可砍伐树木的预制体。不需要在玩家头顶生成图片，勾选动画和粒子效果
2. 添加斧头砍树的动画

### 第六十一节 （Crop）实现斧子砍树的功能

1. 由于树有多个碰撞体，最底部的碰撞体的GO不具有**Cropper**脚本，所以当鼠标位于瓦片之上时会导致获取不到**Cropper**而报错。需要重写判断逻辑，获取不到**Cropper**时禁用鼠标
2. 只有鼠标位于玩家上方玩家就会向上砍树，而树的可点击范围比较大，会导致玩家反方向砍树。可以在砍树的时候将树的坐标传递到**MouseClickedEvent**里，相对树的坐标来计算转向

### 第六十二节 （Crop）随机生成收割物品和转化的实现

1. 使用`_animator.GetCurrentAnimatorStateInfo(0).IsName("END")`检测当前动画，当砍倒树木后播放的END动画播放完毕就生成树桩，树桩会穿模所以需要单独创建一个树桩的预制体来充当种子
2. 生成的树桩依然可以作为树木被砍伐。树木或木桩被砍伐且动画播放完毕后生成物品，可为生成的物品添加下坠效果

### 第六十三节 工具栏按钮快捷键

1. 按下键盘上的1~9可以开启对应背包

### 第六十四节 使用 Particle System 制作树叶凋落特效

### 第六十五节 使用 Unity 最新 ObjectPool API 制作对象池

1. Unity的2021版自带对象池
2. 将树叶掉落的粒子效果存入对象池，当砍树时利用事件生成粒子效果到树木的位置

### 第六十六节 （Crop）实现树木、石头、稻草在场景里的预先生成方法

1. 将预先制作的农作物放置到场景中，需要编写一个**CropGenerator**脚本在游戏开始时将作物的信息写入到作物所在的土地中
2. 在场景加载后执行脚本，**CropManger**会根据预制作物的信息在土地上生成作物
3. 场景重复加载会导致预制作物回到预设的状态，需要建立一个字典来判断该场景是否第一次加载
4. 添加石头的预制物，完成动画等工作

### 第六十七节 （Crop）制作石头和稻草的粒子特效

1. 制作粒子特效
2. 多个杂草会堆叠在同一个土地上，所以不能用**CropGenerator**来让土地记录种子信息，需要把杂草的脚本改成Item，当成item来对待

### 第六十八节 （Crop）实现割草的全部流程及稻草的互动摇晃

1. 制作各个部位的收割动画
2. 创建**ReapItem**脚本用来初始化杂草的信息，如果杂草的**WorldItem**脚本检测到该物品是杂草就会为其添加上该脚本
3. 使用`Physics2D.OverlapCircleNonAlloc(mousePosition, tool.itemUseRadius, collider2Ds)`来检测半径内的物体，这个函数不会产生太多垃圾回收。挥出镰刀后将半径范围内的杂草割除
4. 人物穿过杂草后杂草会摇晃产生互动效果

## AStar 部分

### 第六十九节 

1. 创建AStar算法所需的节点信息，包含目标距离，起点距离以及得分
2. 为**MapSO**添加地图格子信息，由于瓦片的坐标会是负数，但数组索引不能是负数，所以需要以瓦片地图的左下角为原点

### 第七十节 （AStar）根据每个地图信息生成节点数据

### 第七十一节 （AStar）核心功能评估周围节点得到最短路径

### 第七十二节 （AStar）测试实现在真实游戏地图上显示最短路径

## 游戏部分

### 第七十三节 创建 NPC 基本信息并实现根据场景切换显示

1. 创建NPC的动画，NPC具有一个**EventAnimationClip**动画用来播放事件动画，在任何状态下都可以进入，播放完毕或者达到一定的时间后强制退出该动画
2. 创建NPC脚本，包含当前场景、目标场景、当前格子、目标格子等信息
3. 创建**NpcManager**，包含所有NPC的信息

### 第七十四节 NPC 的 Schedule 数据制作和路径生成

1. 创建**ScheduleDetails**用来记录NPC的日程，什么时候触发什么事件
2. **NpcMovements**里建立每一步的动作类**MovementsStep**，包含了这一步的时间、坐标、季节、场景。

### 第七十五节 利用 AStar 实现 NPC 的移动

1. 利用AStar算法获取移动步骤，再为每一步附加上时间信息，如果还有时间移动到下一步就直接正常走过去，如果没有时间就传送过去

### 第七十六节 加入 NPC 动画及真实的 Schedule 触发

1. 完成NPC移动的动画，NPC到达指定地点后可以停顿一段时间再做出行为
2. 规划时间轴列表，每分钟呼叫NPC检查时间表看当前时间下有没有需要执行的任务

### 第七十七节 跨场景路地图的径数据及生成

1. **ScenePath**包含了不同场景的传送地点，如果某一项值大于**Settings**中的最大地图尺寸，说明应该按照NPC时间表来寻路
2. 跨场景寻路的思路是 ，从当前场景走到传送地点的路径加上另一个场景传送地点到目标地点的路径
3. 注意别寻到死路去了，会导致游戏卡顿

### 第七十八节 修正 CropGenerator 和 AStar 地图节点生成

## Dialogue部分

### 第七十九节 （Dialogue）制作对话的 UI

### 第八十节  （Dialogue）创建对话数据实现对话逻辑

1. 创建对话列表，将对话按照反方向压入栈中，每次对话弹出对话条目即可

## 游戏部分

### 第八十一节 创建 NPCFunction 和 通用 UI 实现对话后打开商店

1. 制作NPC使用的背包UI
2. 创建事件来广播格子类型和背包数据的SO
3. 打开背包时根据商店的SO显示物品，商店背包的格子代码和人物的有所不同

### 第八十二节 创建交易窗口 UI 并实现拖拽交易打开交易窗口

1. 增加一个事件**CallBagBaseClose**用来处理关闭交易窗口的逻辑
2. 增加事件**CallUpdateGameState**在对话时禁用玩家的移动功能
3. 创建交易的UI，当从商店的格子拖拽物体到背包时判定为买东西，此时弹出交易的窗口

### 第八十三节 实现买卖交易的完整流程

1. 填写交易数量并提交后，查找背包的空闲位置或已经拥有的该物品的位置，并更新**Inventory**的UI，钱不够或者背包已满就直接取消交易

### 第八十四节 建造图纸数据及 ItemTooltip 显示资源物品

1. 创建蓝图的SO数据
2. 在**ItemTooltip**上添加一个UI显示图纸建造所需要的材料
3. 鼠标移动到图纸上时会显示需要的材料和数量

### 第八十五节 完成建造的流程和逻辑

1. 创建一个用来建造的**Build Cursor**，当选中图纸时就激活该Cursor，像自制的Cursor一样跟随鼠标的移动
2. 判断鼠标所在瓦片能不能放置家具，如果能放置且背包里的材料足够就生成家具并从背包扣除对应的材料数量

### 第八十六节 实现切换场景保存和读取场景中的建造物品

1. 创建一个**Furniture**类挂载到家具上，用来保存该家具的id和再场景中的位置
2. 创建**Box**类保存箱子数据，每个箱子都克隆一份新的**BoxTemplate**SO数据

### 第八十七节 实现箱子储物空间的保存和数据交换

1. 将背包的物品拖拽到箱子后需要同时交换两者的数据
2. 使用字典的方式保存场景不同位置箱子的列表数据

## 2D Light

### 第八十八节 升级到 URP 并创建灯光数据结构

1. 升级项目到URP
2. 创建灯光的Details，包含光色、光强的信息
3. 创建**LightControl**脚本控制物体灯光的变化

### 第八十九节 实现跟随游戏时间触发切换场景光效（昼夜交替）

1. **LightControl**包含了设置灯光的方法
2. 根据时间差值来计算当前应该把灯光设置成哪个LightShift，如果时间差值大于过渡时间就直接将灯光设成目标值，如果小于过渡时间就根据余下的过渡时间逐渐设置
