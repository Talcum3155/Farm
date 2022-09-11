using System;
using System.Collections;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities;
using Utilities.CustomAttribute;

namespace Transition.Logic
{
    public class TransitionManager : MonoBehaviour
    {
        [SceneName] public string startSceneName;

        private CanvasGroup _fadeCanvasGroup;
        private bool _isFading;

        private async void Start()
        {
            _fadeCanvasGroup = FindObjectOfType<CanvasGroup>();
            await LoadSceneSetActive(startSceneName);
            MyEventHandler.CallAfterSceneLoaded();
        }

        private void OnEnable()
            => MyEventHandler.TransitionScene += OnTransitionEvent;

        private void OnDisable()
            => MyEventHandler.TransitionScene -= OnTransitionEvent;

        private async void OnTransitionEvent(string targetSceneName, Vector3 targetScenePosition)
        {
            //防止刚切换完场景玩家时，fade还未结束玩家又想要切换场景
            if (_isFading)
                return;

            //通知订阅的事件的函数即将卸载场景
            MyEventHandler.CallBeforeSceneUnLoaded();
            await StartFade(1);
            await SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene()).ToUniTask();

            MyEventHandler.CallMoveToPosition(targetScenePosition);
            await LoadSceneSetActive(targetSceneName);

            //通知订阅事件的函数场景加载完毕
            MyEventHandler.CallAfterSceneLoaded();
            await StartFade(0);
        }

        /// <summary>
        /// 使用uniTask异步加载场景
        /// </summary>
        /// <param name="sceneName"></param>
        private async UniTask LoadSceneSetActive(string sceneName)
        {
            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive).ToUniTask(
                Progress.Create<float>((f) => { }));
            //将加载的场景设置为激活态，这样SceneManager.GetActiveScene()就能获取该场景的名称
            SceneManager.SetActiveScene(SceneManager.GetSceneAt(SceneManager.sceneCount - 1));
            MyEventHandler.CallAfterSceneLoaded();
        }

        /// <summary>
        /// 切换场景时淡入淡出
        /// </summary>
        /// <param name="targetAlpha"></param>
        private async UniTask StartFade(float targetAlpha)
        {
            _isFading = true;
            _fadeCanvasGroup.blocksRaycasts = true;

            //根据过度时间计算出alpha变化的速度
            var fadeSpeed = Mathf.Abs(_fadeCanvasGroup.alpha - targetAlpha) / Settings.FadeCanvasDuration;

            while (!Mathf.Approximately(_fadeCanvasGroup.alpha, targetAlpha))
            {
                _fadeCanvasGroup.alpha =
                    Mathf.MoveTowards(_fadeCanvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
                //相当于yield return null，等下一帧完成再执行后面的代码
                await UniTask.Yield();
            }

            _fadeCanvasGroup.blocksRaycasts = false;
            _isFading = false;
        }
    }
}