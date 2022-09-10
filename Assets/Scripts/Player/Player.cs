using System;
using UnityEngine;
using Utilities;

namespace Player
{
    public class Player : MonoBehaviour
    {
        public float moveSpeed;

        private float _inputX;
        private float _inputY;
        public Vector2 _movementInput;
        private bool _controllable;

        private Rigidbody2D _rigidbody2D;
        public Animator[] _animators;
        public bool _isMoving;
        private static readonly int InputX = Animator.StringToHash("InputX");
        private static readonly int InputY = Animator.StringToHash("InputY");
        private static readonly int IsMoving = Animator.StringToHash("IsMoving");

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
        }

        private void OnDisable()
        {
            MyEventHandler.BeforeSceneUnLoad -= OnBeforeSceneUnLoadEvent;
            MyEventHandler.AfterSceneLoaded -= OnAfterSceneLoadEvent;
            MyEventHandler.MoveToPosition -= OnMoveToPositionEvent;
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
                if (!_isMoving) continue;
                animator.SetFloat(InputX, _inputX);
                animator.SetFloat(InputY, _inputY);
            }
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
    }
}