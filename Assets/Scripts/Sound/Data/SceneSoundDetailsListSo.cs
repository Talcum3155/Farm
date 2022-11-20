using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;
using Utilities.CustomAttribute;

namespace Sound.Data
{
    [CreateAssetMenu(menuName = "Sound/SceneSoundList", fileName = "SceneSoundList_SO")]
    public class SceneSoundDetailsListSo : ScriptableObject
    {
        public List<SceneSoundItem> sceneSoundList;

        public SceneSoundItem GetSceneSoundItem(string sceneName)
            => sceneSoundList.Find(s => s.sceneName.Equals(sceneName));
    }

    [Serializable]
    public class SceneSoundItem
    {
        [SceneName] public string sceneName;
        public SoundName ambient;
        public SoundName music;
    }
}