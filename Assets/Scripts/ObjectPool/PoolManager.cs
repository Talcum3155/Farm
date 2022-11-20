using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Sound.Data;
using Sound.Logic;
using UnityEngine;
using UnityEngine.Pool;
using Utilities;

namespace ObjectPool
{
    public class PoolManager : MonoBehaviour
    {
        //There are many different kinds of Particle Effect
        public List<GameObject> poolPrefabs;

        //Each Object has a ObjectPool
        private readonly List<ObjectPool<GameObject>> _poolEffect = new();
        private readonly Queue<GameObject> _soundPool = new();

        private void Start()
        {
            CreateObjectPool();
            CreateSoundPool();
        }

        private void OnEnable()
        {
            MyEventHandler.InstantiatedParticle += OnInstantiatedParticleEvent;
            MyEventHandler.InitSoundEffect += OnInitSoundEffectEvent;
        }

        private void OnDisable()
        {
            MyEventHandler.InstantiatedParticle -= OnInstantiatedParticleEvent;
            MyEventHandler.InitSoundEffect -= OnInitSoundEffectEvent;
        }

        /// <summary>
        /// Create Object Pool for each Object
        /// in the particle prefab list
        /// </summary>
        private void CreateObjectPool()
        {
            foreach (var prefab in poolPrefabs)
            {
                var parent = new GameObject(prefab.name).transform;
                parent.SetParent(transform);

                _poolEffect.Add(new ObjectPool<GameObject>(
                    () => Instantiate(prefab, parent),
                    e => e.SetActive(true),
                    e => e.SetActive(false),
                    Destroy
                ));
            }
        }

        /// <summary>
        /// Get specified Object from ObjectPool according particle type
        /// </summary>
        /// <param name="effectType"></param>
        /// <param name="position"></param>
        private async void OnInstantiatedParticleEvent(ParticleEffectType effectType, Vector3 position)
        {
            var objPool = effectType switch
            {
                ParticleEffectType.FallenLeaves01 => _poolEffect[0],
                ParticleEffectType.FallenLeaves02 => _poolEffect[1],
                ParticleEffectType.Rock => _poolEffect[2],
                ParticleEffectType.Grass => _poolEffect[3],
                _ => null
            };

            var particleObject = objPool?.Get();
            if (!particleObject)
                throw new NullReferenceException($"{effectType} 的对象池未创建");

            particleObject.transform.position = position;

            //Set inactive after particle active
            await UniTask.Delay(TimeSpan.FromSeconds(1.5f));
            objPool.Release(particleObject);
        }

        /// <summary>
        /// Create Object Pool for sound
        /// </summary>
        private void CreateSoundPool()
        {
            var parent = new GameObject(poolPrefabs[4].name).transform;
            parent.SetParent(transform);

            for (var i = 0; i < 20; i++)
            {
                var o = Instantiate(poolPrefabs[4], parent);
                o.SetActive(false);
                _soundPool.Enqueue(o);
            }
        }

        private GameObject GetSoundPoolObject()
        {
            if (_soundPool.Count < 2)
                CreateObjectPool();
            return _soundPool.Dequeue();
        }

        private async void OnInitSoundEffectEvent(SoundDetails soundDetails)
        {
            var poolObject = GetSoundPoolObject();
            poolObject.SetActive(true);
            poolObject.GetComponent<AmbientSound>().SetSound(soundDetails);
            await UniTask.Delay(TimeSpan.FromSeconds(soundDetails.audioClip.length));
            poolObject.SetActive(false);
            _soundPool.Enqueue(poolObject);
        }
    }
}