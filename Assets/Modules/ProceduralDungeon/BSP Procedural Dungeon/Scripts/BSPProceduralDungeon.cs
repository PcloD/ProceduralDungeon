using UnityEngine;
using BinarySpacePartitioning;
using System.Collections.Generic;

namespace ProceduralDungeon
{
    public class BSPProceduralDungeon : MonoBehaviour
    {
        [SerializeField] private IntVector2 m_mapSize = new IntVector2(10, 10);
        [SerializeField] private int m_splitIteration = 1;
        [SerializeField] private IntVector2 m_minNodeSize = new IntVector2(3, 3);
        [SerializeField] private Vector2 m_minRoomSizeRatio = new Vector2(0.45f, 0.45f);

        [Header("Gizmos")]
        [SerializeField] private bool m_drawGrid;
        [SerializeField] private bool m_drawBSP;
        [SerializeField] private bool m_drawRoom;
        [SerializeField] private bool m_drawCorridor;

        private BSPTree m_tree;
        private int m_currentInteration;
        private List<BSPNode> m_leafNodes;
        private List<BSPNode> m_parentNodes;

        private void Update()
        {
            if(Input.GetKey(KeyCode.Space))
            {
                Generate();
            }
        }

        [InspectorMethod]
        public void Generate()
        {
            Initialize();

            for (int i = 0; i < m_splitIteration; i++)
            {
                Split();
            }

            GenerateRoom();
            GenerateCorridor();
        }

        [InspectorMethod]
        private void Initialize()
        {
            m_tree = new BSPTree(m_mapSize, m_minNodeSize, m_minRoomSizeRatio);
            m_currentInteration = 0;
            m_leafNodes = m_tree.GetAllLeafNodes();

            if(m_parentNodes != null)
            {
                m_parentNodes.Clear();
            }
        }

        [InspectorMethod]
        private void Split()
        {
            if(m_tree == null)
            {
                return;
            }

            m_currentInteration++;
            m_tree.Split();
            m_leafNodes = m_tree.GetAllLeafNodes();
            if (m_parentNodes != null)
            {
                m_parentNodes.Clear();
            }
        }

        [InspectorMethod]
        private void GenerateRoom()
        {
            if(m_leafNodes == null || m_leafNodes.Count == 0)
            {
                return;
            }

            for (int i = 0; i < m_leafNodes.Count; i++)
            {
                m_leafNodes[i].GenerateRoomRect();
            }

            if (m_parentNodes != null)
            {
                m_parentNodes.Clear();
            }
        }

        [InspectorMethod]
        private void GenerateCorridor()
        {
            if (m_leafNodes == null || m_leafNodes.Count == 0)
            {
                return;
            }

            List<BSPNode> levelNodes = null;
            for (int i = m_currentInteration; i >= 0; i--)
            {
                levelNodes = m_tree.GetNodesByLevel(i);
                for (int j = 0; j < levelNodes.Count; j++)
                {
                    levelNodes[j].GenerateCorridorRect();
                }
            }

            m_parentNodes = m_tree.GetAllParentNodes();
        }

        private void OnDrawGizmos()
        {
            DrawGrid();
            DrawBSP();
            DrawRoom();
            DrawCorridor();
        }

        private void DrawGrid()
        {
            if (!m_drawGrid)
            {
                return;
            }

            Vector3 left;
            Vector3 right;
            for (int z = 0; z <= m_mapSize.z; z++)
            {
                left = new Vector3(0, 0, z);
                right = new Vector3(m_mapSize.x, 0, z);

                Gizmos.DrawLine(left, right);
            }

            Vector3 bottom;
            Vector3 up;
            for (int x = 0; x <= m_mapSize.x; x++)
            {
                bottom = new Vector3(x, 0, 0);
                up = new Vector3(x, 0, m_mapSize.z);

                Gizmos.DrawLine(bottom, up);
            }
        }

        private void DrawBSP()
        {
            if (!m_drawBSP)
            {
                return;
            }

            if(m_leafNodes == null || m_leafNodes.Count == 0)
            {
                return;
            }

            Gizmos.color = Color.green;
            for(int i = 0; i < m_leafNodes.Count; i++)
            {
                DrawRect(m_leafNodes[i].Rect);
            }
        }

        private void DrawRoom()
        {
            if (!m_drawRoom)
            {
                return;
            }

            if (m_leafNodes == null || m_leafNodes.Count == 0)
            {
                return;
            }

            Gizmos.color = Color.blue;
            for (int i = 0; i < m_leafNodes.Count; i++)
            {
                DrawRect(m_leafNodes[i].RoomRect);
            }
        }

        private void DrawCorridor()
        {
            if (!m_drawCorridor)
            {
                return;
            }

            if (m_parentNodes == null || m_parentNodes.Count == 0)
            {
                return;
            }

            Gizmos.color = Color.red;
            for (int i = 0; i < m_parentNodes.Count; i++)
            {
                DrawRect(m_parentNodes[i].CorridorRect);
            }
        }

        private void DrawRect(IntRect rect)
        {
            Gizmos.DrawLine(new Vector3(rect.x, 0, rect.y), new Vector3(rect.x + rect.width, 0, rect.y));
            Gizmos.DrawLine(new Vector3(rect.x, 0, rect.y + rect.height), new Vector3(rect.x + rect.width, 0, rect.y + rect.height));
            Gizmos.DrawLine(new Vector3(rect.x, 0, rect.y), new Vector3(rect.x, 0, rect.y + rect.height));
            Gizmos.DrawLine(new Vector3(rect.x + rect.width, 0, rect.y), new Vector3(rect.x + rect.width, 0, rect.y + rect.height));
        }
    }
}