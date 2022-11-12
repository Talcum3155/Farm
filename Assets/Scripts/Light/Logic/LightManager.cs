using System;
using UnityEngine;
using Utilities;

namespace Light.Logic
{
    public class LightManager : MonoBehaviour
    {
        private LightControl[] _sceneLights;
        private LightShift _currentLightShift;
        private Season _currentSeason;

        private float _timeDifference;

        private void OnEnable()
        {
            MyEventHandler.AfterSceneLoaded += OnAfterSceneLoadedEvent;
            MyEventHandler.LightShiftChange += OnLightShiftChangeEvent;
        }

        private void OnDisable()
        {
            MyEventHandler.AfterSceneLoaded -= OnAfterSceneLoadedEvent;
            MyEventHandler.LightShiftChange -= OnLightShiftChangeEvent;
        }

        private void OnAfterSceneLoadedEvent()
        {
            _sceneLights = FindObjectsOfType<LightControl>();

            foreach (var sceneLight in _sceneLights)
            {
                //Switch to the right light that match the time
                sceneLight.ChangeLightShift(_currentSeason, _currentLightShift, _timeDifference);
            }
        }

        private void OnLightShiftChangeEvent(Season season, LightShift lightShift, float timeDifference)
        {
            _currentSeason = season;
            _timeDifference = timeDifference;

            if (_currentLightShift == lightShift) 
                return;
            _currentLightShift = lightShift;

            foreach (var sceneLight in _sceneLights)
            {
                sceneLight.ChangeLightShift(_currentSeason, _currentLightShift, _timeDifference);
            }
        }
    }
}