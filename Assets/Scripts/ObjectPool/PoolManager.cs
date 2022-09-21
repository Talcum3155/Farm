using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
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

        private void Start()
        {
            CreateObjectPool();
        }

        private void OnEnable()
        {
            MyEventHandler.InstantiatedParticle += OnInstantiatedParticleEvent;
        }

        private void OnDisable()
        {
            MyEventHandler.InstantiatedParticle -= OnInstantiatedParticleEvent;
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
    }
}