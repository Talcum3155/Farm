using UnityEngine;

namespace AStar
{
    public class GridNodes
    {
        private readonly int _widthOfTotalGrid;
        private readonly int _heightOfTotalGrid;
        private readonly Node[,] _gridNodes;

        public GridNodes(int width, int height)
        {
            _widthOfTotalGrid = width;
            _heightOfTotalGrid = height;

            _gridNodes = new Node[width, height];

            for (var i = 0; i < width; i++)
            for (var j = 0; j < height; j++)
                _gridNodes[i, j] = new Node(new Vector2Int(i, j));
        }

        public Node GetGridNode(int xPos, int yPos)
        {
            if (xPos < _widthOfTotalGrid && yPos < _heightOfTotalGrid)
                return _gridNodes[xPos, yPos];

            Debug.LogError("超出网格范围");
            return null;
        }
    }
}