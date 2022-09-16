using System;
using Map.Logic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities;

namespace Cursor.Logic
{
    public class CursorManager : MonoBehaviour
    {
        public Sprite normal, seed, tool, item;
        private Image _cursorImage;
        private RectTransform _cursorCanvas;

        private Sprite _currentCursorSprite;

        public Sprite CurrentCursorSprite
        {
            private set
            {
                _currentCursorSprite = value;
                _cursorImage.sprite = _currentCursorSprite;
                _cursorImage.color = new Color(1, 1, 1, 1);
            }

            get => _currentCursorSprite;
        }

        private Camera _camera;
        private Grid _currentGrid;

        private Vector3 _mouseWorldPos;
        private Vector3Int _mouseGridPos;

        /*
         * 刚开始时在Grid还未找到之前cursor就已经存在了,
         * 切换场景时，Grid也需要变更成下一个场景的Grid
         * 在这期间调用CheckCursorValid()会因为不存在Grid而报错
         * 所以需要确保Grid在找到之前禁用鼠标
         */
        private bool _cursorUsable;
        private bool _cursorPosValid;

        private ItemDetails _holdingItem; //持有的物品

        private Transform _playerTransform;

        private void OnEnable()
        {
            MyEventHandler.SelectedItem += OnItemSelectedEvent;
            MyEventHandler.AfterSceneLoaded += OnAfterSceneLoaded;
            MyEventHandler.BeforeSceneUnLoad += OnBeforeSceneUnLoadedEvent;
        }

        private void OnDisable()
        {
            MyEventHandler.SelectedItem -= OnItemSelectedEvent;
            MyEventHandler.AfterSceneLoaded -= OnAfterSceneLoaded;
            MyEventHandler.BeforeSceneUnLoad -= OnBeforeSceneUnLoadedEvent;
        }

        private void Start()
        {
            _cursorCanvas = GameObject.FindGameObjectWithTag("CursorCanvas").GetComponent<RectTransform>();
            _cursorImage = _cursorCanvas.GetChild(0).GetComponent<Image>();
            CurrentCursorSprite = normal;

            _camera = Camera.main;
            _playerTransform = FindObjectOfType<Player.Player>().transform;
        }

        private void Update()
        {
            if (_cursorCanvas is null)
                return;

            _cursorImage.transform.position = Input.mousePosition;

            if (InteractWithUI() || !_cursorUsable)
            {
                _cursorImage.sprite = normal;
                return;
            }

            CheckCursorValid();
            CheckPlayerInput();
            _cursorImage.sprite = CurrentCursorSprite;
        }

        /// <summary>
        /// 将鼠标的世界坐标转换成当前瓦片地图的坐标，再检测当前持有的物品能不能对该瓦片产生行为
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void CheckCursorValid()
        {
            if (_holdingItem is null)
                return;

            _mouseWorldPos = _camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,
                -_camera.transform.position.z));
            //将世界坐标转换成当前网格下的网格坐标
            _mouseGridPos = _currentGrid.WorldToCell(_mouseWorldPos);
            var playerGridPos = _currentGrid.WorldToCell(_playerTransform.position);

            //鼠标与玩家的距离大于持有物品的使用范围
            if (Mathf.Abs(_mouseGridPos.x - playerGridPos.x) > 2 || Mathf.Abs(_mouseGridPos.y - playerGridPos.y) > 2)
            {
                SetCursorInvalid();
                return;
            }

            var tile = GridMapManager.Instance.GetTileInDict(_mouseGridPos);
            if (tile is null)
            {
                SetCursorInvalid();
                return;
            }

            switch (_holdingItem.itemType)
            {
                case ItemType.Seed:
                    if (tile.diggable)
                    {
                        SetCursorValid();
                        return;
                    }

                    SetCursorInvalid();
                    break;
                case ItemType.Commodity:
                    if (!tile.dropItem || !_holdingItem.canDropped)
                    {
                        SetCursorInvalid();
                        return;
                    }

                    SetCursorValid();
                    break;
                case ItemType.Furniture:
                    break;
                case ItemType.HoeTool:
                    if (tile.diggable)
                    {
                        SetCursorValid();
                        return;
                    }

                    SetCursorInvalid();
                    break;
                case ItemType.WaterTool:
                    if (tile.daysSinceDug >= 0 && tile.daysSinceWatered is -1)
                    {
                        SetCursorValid();
                        return;
                    }

                    SetCursorInvalid();
                    break;
                case ItemType.ChopTool:
                case ItemType.BreakTool:
                case ItemType.CollectTool:

                    break;
                case ItemType.HarvestableScenery:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetCursorValid()
        {
            _cursorPosValid = true;
            _cursorImage.color = new Color(1, 1, 1, 1);
        }

        private void SetCursorInvalid()
        {
            _cursorPosValid = false;
            _cursorImage.color = new Color(1, 0, 0, 0.5f);
        }

        /// <summary>
        /// 鼠标是否在UI上
        /// IsPointerOverGameObject(),
        /// IsPointerOverGameObject(int pointerId);
        ///     Is the pointer with the given ID over an EventSystem object?
        /// </summary>
        /// <returns></returns>
        private bool InteractWithUI() =>
            EventSystem.current is not null
            && EventSystem.current.IsPointerOverGameObject();

        private void CheckPlayerInput()
        {
            if (Input.GetMouseButtonDown(0) && _cursorPosValid)
            {
                MyEventHandler.CallMouseClickedEvent(_mouseWorldPos, _holdingItem);
            }
        }

        #region 事件绑定

        /// <summary>
        /// 根据当前持有的物品来更改鼠标的状态
        /// </summary>
        /// <param name="details"></param>
        /// <param name="selected"></param>
        private void OnItemSelectedEvent(ItemDetails details, bool selected)
        {
            //取消选择当前持有的物品就让所有状态回归到初始状态
            if (!selected)
            {
                CurrentCursorSprite = normal;
                //只有持有物品的时候才需要就检测鼠标的位置是否合法
                _cursorUsable = false;
                _holdingItem = null;
                return;
            }

            _holdingItem = details;
            _cursorUsable = true;
            CurrentCursorSprite = details.itemType switch
            {
                ItemType.Seed => seed,
                ItemType.Commodity => item,
                ItemType.HoeTool or
                    ItemType.ChopTool or
                    ItemType.ChopTool or
                    ItemType.BreakTool or
                    ItemType.WaterTool or
                    ItemType.CollectTool => tool,
                _ => normal
            };
        }

        private void OnAfterSceneLoaded()
        {
            _currentGrid = FindObjectOfType<Grid>();
            _cursorUsable = true;
        }

        private void OnBeforeSceneUnLoadedEvent()
        {
            _cursorUsable = false;
        }

        #endregion
    }
}