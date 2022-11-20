using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sound.Data;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Utilities;
using Random = UnityEngine.Random;

namespace Sound.Logic
{
    public class AudioManager : SingleTon<AudioManager>
    {
        public SoundDetailsListSo soundDetailsList;
        public SceneSoundDetailsListSo sceneSoundDetailsList;

        public AudioSource ambientSource;
        public AudioSource gameSource;

        private static float MusicStartSecond => Random.Range(5f, 15f);
        private UniTask _soundTask = UniTask.CompletedTask;
        private CancellationTokenSource _cts;

        [Header("Mixer")] public AudioMixer audioMixer;
        public float musicTransitionSecond = 8f;

        [Header("Snapshots")] public AudioMixerSnapshot normalSnapshot;
        public AudioMixerSnapshot ambientSnapshot;
        public AudioMixerSnapshot muteSnapshot;

        private void OnEnable()
        {
            MyEventHandler.AfterSceneLoaded += OnAfterSceneLoadedEvent;
            MyEventHandler.PlaySoundEffect += OnPlaySoundEffectEvent;
        }

        private void OnDisable()
        {
            MyEventHandler.AfterSceneLoaded -= OnAfterSceneLoadedEvent;
            MyEventHandler.PlaySoundEffect -= OnPlaySoundEffectEvent;
        }

        private void OnPlaySoundEffectEvent(SoundName soundName)
        {
            var soundDetails = soundDetailsList.GetSoundDetails(soundName);
            if (soundDetails != null)
                MyEventHandler.CallInitSoundEffect(soundDetails);
        }

        private void OnAfterSceneLoadedEvent()
        {
            var sceneSound = sceneSoundDetailsList.GetSceneSoundItem(SceneManager.GetActiveScene().name);
            if (sceneSound == null)
                return;

            var ambient = soundDetailsList.GetSoundDetails(sceneSound.ambient);
            var music = soundDetailsList.GetSoundDetails(sceneSound.music);

            if (_soundTask.Status != UniTaskStatus.Succeeded)
            {
                _cts.Cancel();
            }

            _cts = new CancellationTokenSource();
            _soundTask = PlayerSoundAsync(music, ambient);
        }

        /// <summary>
        /// Wait for a random time to
        /// switch to the BGM of the corresponding scene
        /// </summary>
        /// <param name="music"></param>
        /// <param name="ambient"></param>
        private async UniTask PlayerSoundAsync(SoundDetails music, SoundDetails ambient)
        {
            if (music == null || ambient == null)
                await UniTask.CompletedTask;

            PlayAmbientClip(ambient, 1f);
            await UniTask.Delay(TimeSpan.FromSeconds(MusicStartSecond), false, PlayerLoopTiming.Update, _cts.Token);
            if (_cts.IsCancellationRequested)
                await UniTask.CompletedTask;
            PlayMusicClip(music, musicTransitionSecond);
        }

        /// <summary>
        /// Play BGM
        /// </summary>
        /// <param name="soundDetails"></param>
        /// <param name="transitionTime"></param>
        private void PlayMusicClip(SoundDetails soundDetails, float transitionTime)
        {
            audioMixer.SetFloat("MusicVolume", ConvertSoundVolume(soundDetails.soundVolume));
            gameSource.clip = soundDetails.audioClip;
            if (gameSource.isActiveAndEnabled)
                gameSource.Play();

            normalSnapshot.TransitionTo(transitionTime);
        }

        /// <summary>
        /// Play Ambient Sound
        /// </summary>
        /// <param name="soundDetails"></param>
        /// <param name="transitionTime"></param>
        private void PlayAmbientClip(SoundDetails soundDetails, float transitionTime)
        {
            audioMixer.SetFloat("AmbientVolume", ConvertSoundVolume(soundDetails.soundVolume));
            ambientSource.clip = soundDetails.audioClip;
            if (ambientSource.isActiveAndEnabled)
                ambientSource.Play();

            ambientSnapshot.TransitionTo(transitionTime);
        }

        private float ConvertSoundVolume(float amount)
            => amount * 100 - 80;
    }
}