using Sound.Data;
using UnityEngine;

namespace Sound.Logic
{
    [RequireComponent(typeof(AudioSource))]
    public class AmbientSound : MonoBehaviour
    {
        [SerializeField] private AudioSource sound;

        /// <summary>
        /// Set sound clip for this GameObject
        /// </summary>
        /// <param name="soundDetails"></param>
        public void SetSound(SoundDetails soundDetails)
            => (sound.clip, sound.volume, sound.pitch)
                = (soundDetails.audioClip,
                    soundDetails.soundVolume,
                    Random.Range(soundDetails.soundPitchMin, soundDetails.soundPitchMax));
    }
}