using System;
using DG.Tweening;
using Light.Data;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Utilities;

namespace Light.Logic
{
    public class LightControl : MonoBehaviour
    {
        public LightPatternListSo lightData;
        private Light2D _currentLight;
        private LightDetails _currentLightDetails;

        private void Awake()
        {
            _currentLight = GetComponent<Light2D>();
        }

        
        public void ChangeLightShift(Season season, LightShift lightShift, float timeDifference)
        {
            _currentLightDetails = lightData.GetLightDetails(season, lightShift);

            //Set light option to right option by duration remain
            if (timeDifference < Settings.LightDuration)
            {
                var colorOffset = (_currentLightDetails.lightColor - _currentLight.color) / Settings.LightDuration * timeDifference;
                _currentLight.color += colorOffset;

                DOTween.To(() => _currentLight.color, c => _currentLight.color = c, _currentLightDetails.lightColor,
                    Settings.LightDuration - timeDifference);
                DOTween.To(() => _currentLight.intensity, i => _currentLight.intensity = i,
                    _currentLightDetails.lightIntensity, Settings.LightDuration - timeDifference);
            }

            //Directly set light option to right light option
            if (timeDifference > Settings.LightDuration)
            {
                _currentLight.color = _currentLightDetails.lightColor;
                _currentLight.intensity = _currentLightDetails.lightIntensity;
            }
        }
    }
}