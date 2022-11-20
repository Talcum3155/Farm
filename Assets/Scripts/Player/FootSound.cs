using UnityEngine;
using Utilities;

namespace Player
{
    public class FootSound : MonoBehaviour
    {
        public void PlayFootSound()
            => MyEventHandler.CallPlaySoundEffect(SoundName.FootStepSoft);
    }
}