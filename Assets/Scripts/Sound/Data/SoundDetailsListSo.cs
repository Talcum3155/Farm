using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace Sound.Data
{
    [CreateAssetMenu(fileName = "SoundDetailsList_SO",menuName = "Sound/SoundDetailsList")]
    public class SoundDetailsListSo : ScriptableObject
    {
        public List<SoundDetails> soundDetailsList;

        public SoundDetails GetSoundDetails(SoundName soundName)
            => soundDetailsList.Find(s => s.soundName == soundName);
    }

    [Serializable]
    public class SoundDetails
    {
        public SoundName soundName;
        public AudioClip audioClip;
        [Range(0.1f, 1.5f)] public float soundPitchMin;
        [Range(0.1f, 1.5f)] public float soundPitchMax;
        [Range(0.1f, 1.5f)] public float soundVolume;
    }
}
