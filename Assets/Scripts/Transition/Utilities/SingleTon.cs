using System;
using UnityEngine;

namespace Utilities
{
    public class SingleTon<T> : MonoBehaviour where T : SingleTon<T>, new()
    {
        public static T Instance { private set; get; }
        
        protected virtual void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }

            Instance = (T)this;
        }

        public bool IsInitialized => Instance is null;

        private void OnDestroy()
        {
            if (Instance == this)
                Destroy(gameObject);
        }
    }
}