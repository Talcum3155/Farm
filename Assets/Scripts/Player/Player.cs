using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Utilities;

namespace Player
{
    public class Player : MonoBehaviour
    {
        public float moveSpeed;

        public float _inputX;
        public float _inputY;
        public Vector2 _movementInput;
        private bool _controllable;

        private Rigidbody2D _rigidbody2D;
        public Animator[] _animators;
        public bool _isMoving;
        public float mouseX;
        public float mouseY;
        public bool usingTool;
        private static readonly int InputX = Animator.StringToHash("InputX");
        private static readonly int InputY = Animator.StringToHash("InputY");
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");
        private static readonly int MouseX = Animator.StringToHash("MouseX");
        private static readonly int MouseY = Animator.StringToHash("MouseY");
        private static readonly int UseTool = Animator.StringToHash("UseTool");

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _animators = GetComponentsInChildren<Animator>();
        }

        private void OnEnable()
        {
            MyEventHandler.BeforeSceneUnLoad += OnBeforeSceneUnLoadEvent;
            MyEventHandler.AfterSceneLoaded += OnAfterSceneLoadEvent;
            MyEventHandler.MoveToPosition += OnMoveToPositionEvent;
            MyEventHandler.MouseClickedEvent += OnMouseClickedEvent;
        }

        private void OnDisable()
        {
            MyEventHandler.BeforeSceneUnLoad -= OnBeforeSceneUnLoadEvent;
            MyEventHandler.AfterSceneLoaded -= OnAfterSceneLoadEvent;
            MyEventHandler.MoveToPosition -= OnMoveToPositionEvent;
            MyEventHandler.MouseClickedEvent -= OnMouseClickedEvent;
        }

        private void Update()
        {
            if (_controllable)
                GetInput();
            else
                //玩家失去控制权时，需要强制停止人物的移动和动画
                _isMoving = false;
            SwitchAnimation();
        }

        private void FixedUpdate()
        {
            if (_controllable)
                MoveMoment();
        }

        private void GetInput()
        {
            _inputX = Input.GetAxis("Horizontal");
            _inputY = Input.GetAxis("Vertical");

            if (_inputX != 0 && _inputY != 0)
            {
                _inputX *= 0.6f;
                _inputY *= 0.6f;
            }

            if (Input.GetKey(KeyCode.LeftShift))
            {
                _inputX *= 0.5f;
                _inputY *= 0.5f;
            }

            _movementInput = new Vector2(_inputX, _inputY);
            _isMoving = _movementInput != Vector2.zero;
        }

        private void MoveMoment() =>
            _rigidbody2D.MovePosition(
                _rigidbody2D.position
                + _movementInput * (moveSpeed * Time.deltaTime));

        private void SwitchAnimation()
        {
            foreach (var animator in _animators)
            {
                //需要放在if外，否则不移动时，x和y没及时归零导致动画一直处于移动状态
                animator.SetBool(IsMoving, _isMoving);
                
                /*
                 * 根据鼠标的方位改变工具的动画方向，
                 * 工具动画播放完毕后会过渡到Exit，
                 * 动画会从Entry开始重新播放动画。
                 * 由于工具动画需要触发才能执行，
                 * 所以mouseXY值即使改变也不会影响到当前动画
                 */
                animator.SetFloat(MouseX, mouseX);
                animator.SetFloat(MouseY, mouseY);
                
                if (!_isMoving) continue;
                animator.SetFloat(InputX, _inputX);
                animator.SetFloat(InputY, _inputY);
            }
        }

        #region 事件绑定

        private async void OnMouseClickedEvent(Vector3 mousePos, ItemDetails itemDetails)
        {
            //这些物品种类不需要动画，直接就可以执行，如果不是说明该物品是工具会执行工具动画
            if (itemDetails.itemType is ItemType.Seed or ItemType.Commodity or ItemType.Furniture)
            {
                MyEventHandler.CallExecuteActionAfterAnimation(mousePos, itemDetails);
                return;
            }

            var position = transform.position;
            mouseX = mousePos.x - position.x;
            mouseY = mousePos.y - position.y;
            if (Mathf.Abs(mouseX) > Mathf.Abs(mouseY))
                mouseY = 0;
            else
                mouseX = 0;

            await UseToolAsync(mousePos, itemDetails);
        }

        /// <summary>
        /// 卸载场景前禁用人物移动
        /// </summary>
        private void OnBeforeSceneUnLoadEvent() => _controllable = false;

        /// <summary>
        /// 加载场景后启用人物移动
        /// </summary>
        private void OnAfterSceneLoadEvent() => _controllable = true;

        /// <summary>
        /// 将玩家移动到指定位置
        /// </summary>
        /// <param name="position"></param>
        private void OnMoveToPositionEvent(Vector3 position) => transform.position = position;

        #endregion
        
        /// <summary>
        /// 根据鼠标相对人物的位置播放不同的使用工具的动画
        /// </summary>
        /// <param name="mousePos"></param>
        /// <param name="details"></param>
        private async UniTask UseToolAsync(Vector3 mousePos, ItemDetails details)
        {
            usingTool = true;
            //播放工具动画时禁止移动，防止干扰工具动画
            _controllable = false;
            //确保上面的状态改变了，等待一帧后再执行后面的代码
            await UniTask.Yield();

            foreach (var animator in _animators)
            {
                //依次触发每个部位的工具动画
                animator.SetTrigger(UseTool);
                
                //覆盖之前输入的XY轴的值，防止动画结束后人物又转回原来的方向
                animator.SetFloat(InputX, mouseX);
                animator.SetFloat(InputY, mouseY);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(0.45f));
            MyEventHandler.CallExecuteActionAfterAnimation(mousePos, details);
            await UniTask.Delay(TimeSpan.FromSeconds(0.25f));

            usingTool = false;
            _controllable = true;
        }
    }
}