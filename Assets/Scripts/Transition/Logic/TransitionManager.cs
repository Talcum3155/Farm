using System;
using System.Collections;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;

namespace Transition.Logic
{
    public class TransitionManager : MonoBehaviour
    {
        public string startSceneName;

        private async void Start()
        {
            await LoadSceneSetActive(startSceneName);
        }

        private void OnEnable()
            => MyEventHandler.TransitionScene += OnTransitionEvent;

        private void OnDisable()
            => MyEventHandler.TransitionScene -= OnTransitionEvent;

        private async void OnTransitionEvent(string targetSceneName, Vector3 targetScenePosition)
        {
            await SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene()).ToUniTask();
            await LoadSceneSetActive(targetSceneName);
            
        }

        /// <summary>
        /// 使用uniTask异步加载场景
        /// </summary>
        /// <param name="sceneName"></param>
        private async UniTask LoadSceneSetActive(string sceneName)
        {
            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive).ToUniTask(
                Progress.Create<float>((f) => { Debug.Log($"加载进度 {f}"); }));
            //将加载的场景设置为激活态，这样SceneManager.GetActiveScene()就能获取该场景的名称
            SceneManager.SetActiveScene(SceneManager.GetSceneAt(SceneManager.sceneCount - 1));
        }
    }
}