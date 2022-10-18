using System.Collections.Generic;
using NPC.Data;
using UnityEngine;
using Utilities;

namespace NPC.Logic
{
    public class NpcManger : SingleTon<NpcManger>
    {
        public SceneRouteDataSo sceneRouteDataSo;
        public List<NpcPosition> npcPositions;

        private readonly Dictionary<string, SceneRoute> _sceneRouteDict = new();

        protected override void Awake()
        {
            base.Awake();
            InitSceneRouteDict();
        }

        /// <summary>
        /// Use from and target scene name as key
        /// and record every path
        /// </summary>
        private void InitSceneRouteDict()
        {
            if (sceneRouteDataSo.sceneRoutes.Count <= 0) return;

            foreach (var route in sceneRouteDataSo.sceneRoutes)
                _sceneRouteDict[route.fromSceneName + route.targetSceneName] = route;
        }

        /// <summary>
        /// Get Scene Route from dictionary
        /// </summary>
        /// <param name="fromSceneName"></param>
        /// <param name="targetSceneName"></param>
        /// <returns></returns>
        public SceneRoute GetSceneRoute(string fromSceneName, string targetSceneName) =>
            _sceneRouteDict[fromSceneName + targetSceneName];
    }
}