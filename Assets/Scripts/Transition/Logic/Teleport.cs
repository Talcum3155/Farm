using System;
using UnityEngine;
using Utilities;

namespace Transition.Logic
{
    public class Teleport : MonoBehaviour
    {
        public string targetSceneName;
        public Vector3 targetScenePosition;

        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.CompareTag("Player"))
            {
                MyEventHandler.CallTransitionScene(targetSceneName, targetScenePosition);
            }
        }
    }
}
