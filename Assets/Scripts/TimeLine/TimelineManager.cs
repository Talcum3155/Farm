using System;
using UnityEngine;
using UnityEngine.Playables;
using Utilities;

namespace TimeLine
{
    public class TimelineManager : SingleTon<TimelineManager>
    {
        public PlayableDirector startDirector;
        private PlayableDirector _currentDirector;

        private bool _isPause;
        public bool Done { set; get; }

        protected override void Awake()
        {
            base.Awake();
            _currentDirector = startDirector;
        }

        private void OnEnable()
        {
            MyEventHandler.AfterSceneLoaded += OnAfterSceneLoadedEvent;
        }

        private void OnDisable()
        {
            MyEventHandler.AfterSceneLoaded -= OnAfterSceneLoadedEvent;
        }

        private void Update()
        {
            if (_isPause && Input.GetKeyDown(KeyCode.Space) && Done)
            {
                _isPause = false;
                _currentDirector.playableGraph.GetRootPlayable(0).SetSpeed(1d);
            }
        }

        public void PauseTimeline(PlayableDirector director)
        {
            _currentDirector = director;

            _currentDirector.playableGraph.GetRootPlayable(0).SetSpeed(0d);
            _isPause = true;
        }

        private void OnAfterSceneLoadedEvent()
        {
            _currentDirector = FindObjectOfType<PlayableDirector>();
            if (_currentDirector!=null)
                _currentDirector.Play();
        }
    }
}