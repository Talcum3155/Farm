using System;
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
            _cursorImage.sprite = CurrentCursorSprite;
        }

        private void OnItemSelectedEvent(ItemDetails details, bool selected)
        {
            if (!selected)
            {
                CurrentCursorSprite = normal;
                return;
            }

            CurrentCursorSprite = details.itemType switch
            {
                ItemType.Seed => seed,
                ItemType.Commodity => item,
                ItemType.HoeTool => tool,
                ItemType.ChopTool => tool,
                ItemType.BreakTool => tool,
                ItemType.WaterTool => tool,
                ItemType.CollectTool => tool,
                _ => normal
            };
        }

        /// <summary>
        /// 鼠标是否在UI上
        /// IsPointerOverGameObject(),
        /// IsPointerOverGameObject(int pointerId);
        ///     Is the pointer with the given ID over an EventSystem object?
        /// </summary>
        /// <returns></returns>
        private bool InteractWithUI() =>
            EventSystem.current is not null && EventSystem.current.IsPointerOverGameObject();

        private void OnAfterSceneLoaded()
        {
            _currentGrid = FindObjectOfType<Grid>();
            _cursorUsable = true;
        }

        private void CheckCursorValid()
        {
            _mouseWorldPos = _camera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y,
                _camera.transform.position.z));
            //将世界坐标转换成当前网格下的网格坐标
            _mouseGridPos = _currentGrid.WorldToCell(_mouseWorldPos);
            // Debug.Log($"worldPos: {_mouseWorldPos}, gridPos: {_mouseGridPos}");
        }
        
        private void OnBeforeSceneUnLoadedEvent()
        {
            _cursorUsable = false;
        }
    }
}