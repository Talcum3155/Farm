using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

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

        private void Start()
        {
            ShowPathOnGridMap();
        }

        private void Awake()
        {
            _aStarCore = GetComponent<AStarCore>();
            _movementSteps = new Stack<MovementStep>();
        }

        private void Update()
        {
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