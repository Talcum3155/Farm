using System;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Transition
{
    public class ItemInterface : MonoBehaviour
    {
        private bool _inAnimation;

        private async void OnTriggerEnter2D(Collider2D col)
        {
            if (_inAnimation) return;
            if (transform.position.x < col.transform.position.x)
            {
                await RotateLeft();
                return;
            }

            await RotateRight();
        }

        private async void OnTriggerExit2D(Collider2D other)
        {
            if (_inAnimation) return;
            if (transform.position.x > other.transform.position.x)
            {
                await RotateLeft();
                return;
            }

            await RotateRight();
        }

        private async UniTask RotateLeft()
        {
            _inAnimation = true;

            for (var i = 0; i < 4; i++)
            {
                transform.GetChild(0).Rotate(0, 0, 2);
                await UniTask.Delay(TimeSpan.FromSeconds(0.04f));
            }

            for (var i = 0; i < 5; i++)
            {
                transform.GetChild(0).Rotate(0, 0, -2);
                await UniTask.Delay(TimeSpan.FromSeconds(0.04f));
            }

            transform.GetChild(0).Rotate(0, 0, 2);
            await UniTask.Delay(TimeSpan.FromSeconds(0.04f));

            _inAnimation = false;
        }

        private async UniTask RotateRight()
        {
            _inAnimation = true;

            for (var i = 0; i < 4; i++)
            {
                transform.GetChild(0).Rotate(0, 0, -2);
                await UniTask.Delay(TimeSpan.FromSeconds(0.04f));
            }

            for (var i = 0; i < 5; i++)
            {
                transform.GetChild(0).Rotate(0, 0, 2);
                await UniTask.Delay(TimeSpan.FromSeconds(0.04f));
            }

            transform.GetChild(0).Rotate(0, 0, -2);
            await UniTask.Delay(TimeSpan.FromSeconds(0.04f));

            _inAnimation = false;
        }
    }
}