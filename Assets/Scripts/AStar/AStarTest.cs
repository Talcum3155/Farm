using System.Collections.Generic;
using NPC.Data;
using NPC.Logic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Utilities;
using Utilities.CustomAttribute;

namespace AStar
{
    public class AStarTest : MonoBehaviour
    {
        private AStarCore _aStarCore;

        [Header("Use for Test")] public Vector2Int startPos;
        public Vector2Int finishPos;
        public Tilemap displayMap;
        public TileBase displayTile;
        public bool displayStartAndEnd;

        public bool displayPath;

        private Stack<MovementStep> _movementSteps;

        [Header("测试移动NPC")] public NpcMovement npcMovement;
        public bool moveNpc;
        [SceneName] public string targetScene;
        public Vector2Int targetPos;
        public AnimationClip eventAnimationClip;

        private void Awake()
        {
            _aStarCore = GetComponent<AStarCore>();
            _movementSteps = new Stack<MovementStep>();
        }

        private void Update()
        {
            ShowPathOnGridMap();

            if (moveNpc || Input.GetKeyDown(KeyCode.P))
            {
                moveNpc = false;
                var scheduleDetails = new ScheduleDetails(0, 0, 0, 0, Season.春天, targetScene, targetPos, eventAnimationClip, true);
                npcMovement.BuildPath(scheduleDetails);
            }
        }

        private void ShowPathOnGridMap()
        {
            if (displayMap == null || displayTile == null) return;

            if (displayStartAndEnd)
            {
                displayMap.SetTile((Vector3Int)startPos, displayTile);
                displayMap.SetTile((Vector3Int)finishPos, displayTile);
            }
            else
            {
                displayMap.SetTile((Vector3Int)startPos, null);
                displayMap.SetTile((Vector3Int)finishPos, null);
            }

            if (displayPath)
            {
                var sceneName = SceneManager.GetActiveScene().name;
                _aStarCore.BuildPath(sceneName, startPos, finishPos, _movementSteps);
                Debug.Log("构建路径完毕");

                foreach (var step in _movementSteps)
                    displayMap.SetTile((Vector3Int)step.gridCoordinate, displayTile);
            }
            else
            {
                if (_movementSteps.Count <= 0) return;

                foreach (var step in _movementSteps)
                    displayMap.SetTile((Vector3Int)step.gridCoordinate, null);
            }
        }
    }
}