using ProceduralDungeon;
using UnityEngine;
using System.Collections.Generic;

namespace BinarySpacePartitioning
{
    public class BSPTree
    {
        private BSPNode m_rootNode;

        public BSPTree(IntVector2 mapSize, IntVector2 minNodeSize, Vector2 minRoomSizeRatio)
        {
            m_rootNode = new BSPNode(0, new IntRect(0, 0, mapSize.x, mapSize.z), null, minNodeSize, minRoomSizeRatio);
        }

        public void Split()
        {
            m_rootNode.Split();
        }

        public List<BSPNode> GetAllLeafNodes()
        {
            List<BSPNode> nodes = new List<BSPNode>();
            m_rootNode.GetLeafNodes(ref nodes);
            return nodes;
        }

        public List<BSPNode> GetNodesByLevel(int level)
        {
            List<BSPNode> nodes = new List<BSPNode>();
            m_rootNode.GetNodesByLevel(ref nodes, level);
            return nodes;
        }

        public List<BSPNode> GetAllParentNodes()
        {
            List<BSPNode> nodes = new List<BSPNode>();
            m_rootNode.GetParentNodes(ref nodes);
            return nodes;

        }
    }
}