# M Farm笔记

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

核心移动代码：Horizontal和Vertical组成Vector 2，参与位置移动运算

```c#
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
3. camera添加pixel Prefect组件可以根据像素比例将摄像机的位置调整到最佳距离，然后Cinemachine添加Cinemachine pixel prefect扩展以调用camera的pixel Prefect；根据camera中被pixel Prefect改变的size设置Cinemachine的Lens Ortho Size
4. 调整Cinemachine的deadzone来设置跟随的缓存距离

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



