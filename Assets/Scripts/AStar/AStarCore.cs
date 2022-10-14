using System.Collections.Generic;
using Map.Logic;
using UnityEngine;
using Utilities;

namespace AStar
{
    public class AStarCore : SingleTon<AStarCore>
    {
        private GridNodes _gridNodes;
        private Node _startNode;
        private Node _targetNode;
        private int _gridWidth;
        private int _gridHeight;
        private int _originX;
        private int _originY;

        //8 points around the currently selected Node 
        private List<Node> _openNodeLists;

        //The final selected points
        private HashSet<Node> _closetNodes;

        private bool _pathFound;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <param name="movementSteps"></param>
        public void BuildPath(string sceneName, Vector2Int startPos, Vector2Int endPos,
            Stack<MovementStep> movementSteps)
        {
            _pathFound = false;

            if (GenerateGridNodes(sceneName, startPos, endPos))
            {
                if (FindShortestPath())
                {
                    UpdatePathOnMovement(sceneName, movementSteps);
                }
            }
        }

        private bool GenerateGridNodes(string sceneName, Vector2Int startPos, Vector2Int endPos)
        {
            if (!GridMapManager.Instance.GetGridDimensions(sceneName, out var gridDimensions, out var gridOrigin))
                return false;

            _gridNodes = new GridNodes(gridDimensions.x, gridDimensions.y);
            _gridWidth = gridDimensions.x;
            _gridHeight = gridDimensions.y;
            _originX = gridOrigin.x;
            _originY = gridOrigin.y;

            _openNodeLists = new List<Node>();
            _closetNodes = new HashSet<Node>();

            //Gird starts from (0,0), so need to subtract the origin coordinates to get real point in grid
            _startNode = _gridNodes.GetGridNode(startPos.x - _originX, startPos.y - _originY);
            _targetNode = _gridNodes.GetGridNode(endPos.x - _originX, endPos.y - _originY);

            Debug.Log(
                $"获取到原点({_originX},{_originY}),地图的长宽({_gridWidth},{_gridHeight}),起始地点: {_startNode.gridPosition},终点: {_targetNode.gridPosition}");

            //Map the tilemap to gridNodes and identify whether the grid is an obstacle
            for (var i = 0; i < _gridWidth; i++)
            for (var j = 0; j < _gridHeight; j++)
            {
                //Get real point in tilemap
                var tilePos = new Vector3Int(i + _originX, j + _originY, 0);
                var tile = GridMapManager.Instance.GetTileInDict(tilePos);

                if (tile == null) continue;
                _gridNodes.GetGridNode(i, j).isObstacle = tile.npcObstacle;
            }

            return true;
        }

        private bool FindShortestPath()
        {
            Debug.Log("寻找最短路径中...");
            _openNodeLists.Add(_startNode);

            while (_openNodeLists.Count > 0)
            {
                var closestNode = _openNodeLists[0];
                _openNodeLists.RemoveAt(0);

                Debug.Log($"选择了最近点{closestNode.gridPosition}");
                //Add the nearest point of the eight surrounding points
                _closetNodes.Add(closestNode);

                if (closestNode == _targetNode)
                {
                    _pathFound = true;
                    break;
                }

                //Calculate the surrounding 8 nodes and add them to openList
                EvaluateNeighbourNodes(closestNode);
                //Sort to find the smallest grid
                _openNodeLists.Sort();
            }

            return _pathFound;
        }

        private void EvaluateNeighbourNodes(Node currentNode)
        {
            var nodeGridPosition = currentNode.gridPosition;
            Debug.Log($"评估点{nodeGridPosition}");

            for (var i = -1; i <= 1; i++)
            for (var j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                    continue;

                var validNeighbourNode = GetValidNeighbourNode(nodeGridPosition.x + i, nodeGridPosition.y + j);
                Debug.Log($"点{nodeGridPosition}的邻居点 {validNeighbourNode}");
                if (validNeighbourNode is null)
                    continue;

                if (!_openNodeLists.Contains(validNeighbourNode))
                {
                    Debug.Log($"计算点{validNeighbourNode}的得分");
                    validNeighbourNode.GCost = currentNode.GCost + GetDistance(currentNode, validNeighbourNode);
                    validNeighbourNode.HCost = GetDistance(validNeighbourNode, _targetNode);
                    //Link parent Node
                    validNeighbourNode.parentNode = currentNode;
                    _openNodeLists.Add(validNeighbourNode);
                }
            }
        }

        /// <summary>
        /// Find valid Node, not obstacle, not selected
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private Node GetValidNeighbourNode(int x, int y)
        {
            if (x >= _gridWidth || y >= _gridHeight || x < 0 || y < 0)
                return null;
            var neighbourNode = _gridNodes.GetGridNode(x, y);
            Debug.Log($"邻居点: {neighbourNode}");

            //The node may bede select before
            if (neighbourNode.isObstacle || _closetNodes.Contains(neighbourNode))
                return null;

            return neighbourNode;
        }

        /// <summary>
        /// return the distance between two points
        /// </summary>
        /// <param name="nodeA"></param>
        /// <param name="nodeB"></param>
        /// <returns></returns>
        private int GetDistance(Node nodeA, Node nodeB)
        {
            var xDistance = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
            var yDistance = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);

            // (xDistance - yDistance): because npc can walk diagonally so needn't walk at right angles route
            if (xDistance > yDistance)
                return 14 * yDistance + 10 * (xDistance - yDistance);
            return 14 * xDistance + 10 * (yDistance - xDistance);
        }

        /// <summary>
        /// Push node path from target point to start point
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="npcMovementStep"></param>
        private void UpdatePathOnMovement(string sceneName, Stack<MovementStep> npcMovementStep)
        {
            var nextNode = _targetNode;
            
            while (nextNode is not null)
            {
                var step = new MovementStep
                {
                    sceneName = sceneName,
                    //Restore the real point in tilemap
                    gridCoordinate = new Vector2Int(nextNode.gridPosition.x + _originX,
                        nextNode.gridPosition.y + _originY)
                };
                npcMovementStep.Push(step);
                nextNode = nextNode.parentNode;
            }
        }
    }
}