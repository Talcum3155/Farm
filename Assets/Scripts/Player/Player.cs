using UnityEngine;

namespace Player
{
    public class Player : MonoBehaviour
    {
        public float moveSpeed;

        private float _inputX;
        private float _inputY;
        private Vector2 _movementInput;

        private Rigidbody2D _rigidbody2D;

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            GetInput();
        }

        private void FixedUpdate()
        {
            MoveMoment();
        }

        private void GetInput()
        {
            _inputX = Input.GetAxis("Horizontal");
            _inputY = Input.GetAxis("Vertical");

            // if (_inputX != 0 && _inputY != 0)
            // {
            //     _inputX *= 0.6f;
            //     _inputY *= 0.6f;
            // }

            _movementInput = new Vector2(_inputX, _inputY);
        }

        private void MoveMoment() =>
            _rigidbody2D.MovePosition(
                _rigidbody2D.position 
                + _movementInput * (moveSpeed * Time.deltaTime));
    }
}