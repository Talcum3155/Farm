using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace NPC.Data
{
    [CreateAssetMenu(menuName = "NPC/ScenePath",fileName = "New ScenePathList")]
    public class SceneRouteDataSo : ScriptableObject
    {
        public List<SceneRoute> sceneRoutes;
    }
}
