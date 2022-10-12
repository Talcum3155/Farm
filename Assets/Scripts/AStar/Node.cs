using System;
using UnityEngine;

namespace AStar
{
    public class Node : IComparable<Node>
    {
        public Vector2Int gridPosition;
        //Distance from start
        public int GCost { get; set; } = 0; 
        //Distance from target
        public int HCost { get; set; } = 0;
        //The score of this grid
        public int FCost => GCost + HCost;
        public bool isObstacle;
        public Node parentNode;

        public Node(Vector2Int pos, Node parent = null)
        {
            gridPosition = pos;
            parentNode = parent;
        }

        public int CompareTo(Node other)
        {
            var result = FCost.CompareTo(other.FCost);
            if (result is 0)
                result = HCost.CompareTo(other.HCost);
            return result;
        }
    }
}