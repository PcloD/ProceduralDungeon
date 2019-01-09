using UnityEngine;
using BinarySpacePartitioning;
using System.Collections.Generic;

namespace ProceduralDungeon
{
    public class BSPProceduralDungeon : BaseProceduralDungeon
    {
        [Header("Objects")]
        [SerializeField] private GameObject[] m_roomGroundObjs;
        [SerializeField] private GameObject[] m_roomWallObjs;
        [SerializeField] private GameObject[] m_roadGroundObjs;
        [SerializeField] private GameObject[] m_roadWallObjs;
        [SerializeField] private GameObject[] m_pillarObjs;

        [Header("Settings")]
        [SerializeField] private IntVector2 m_mapSize = new IntVector2(10, 10);
        [SerializeField] private int m_splitIteration = 1;
        [SerializeField] private IntVector2 m_minBSPSize = new IntVector2(3, 3);
        [SerializeField] private Vector2 m_minRoomSizeRatio = new Vector2(0.45f, 0.45f);
        [SerializeField] private bool m_generateObjects;

        [Header("Gizmos")]
        [SerializeField] private bool m_drawGrid;
        [SerializeField] private bool m_drawBSP;
        [SerializeField] private bool m_drawRoom;
        [SerializeField] private bool m_drawCorridor;

        private BSPTree m_tree;
        private List<BSPNode> m_leafNodes;
        private List<BSPNode> m_parentNodes;

        private RoomObjectGenerator m_roomObjectGenerator;
        private RoadObjectGenerator m_roadObjectGenerator;
        private PillarObjectGenerator m_pillarObjectGenerator;

        protected override void Destroy()
        {
            if (m_roomObjectGenerator != null)
            {
                m_roomObjectGenerator.Destroy();
            }

            if (m_roadObjectGenerator != null)
            {
                m_roadObjectGenerator.Destroy();
            }

            if (m_pillarObjectGenerator != null)
            {
                m_pillarObjectGenerator.Destroy();
            }
        }

        protected override void StartGenerate()
        {
            m_tree = new BSPTree(m_mapSize, m_minBSPSize, m_minRoomSizeRatio);

            for (int i = 0; i < m_splitIteration; i++)
            {
                m_tree.Split();
            }

            m_leafNodes = m_tree.GetAllLeafNodes();
            for (int i = 0; i < m_leafNodes.Count; i++)
            {
                m_leafNodes[i].GenerateRoomRect();
            }

            List<BSPNode> levelNodes = null;
            for (int i = m_splitIteration; i >= 0; i--)
            {
                levelNodes = m_tree.GetNodesByLevel(i);
                for (int j = 0; j < levelNodes.Count; j++)
                {
                    levelNodes[j].GenerateCorridorRect();
                }
            }

            m_parentNodes = m_tree.GetAllParentNodes();

            if(m_generateObjects)
            {
                List<Room> rooms = new List<Room>();
                for(int i = 0; i < m_leafNodes.Count; i++)
                {
                    rooms.Add(new Room(m_leafNodes[i].RoomRect));
                }

                List<Road> roads = new List<Road>();
                for (int i = 0; i < m_parentNodes.Count; i++)
                {
                    if (m_parentNodes[i].Corridor == null)
                    {
                        continue;
                    }

                    roads.Add(new Road(m_parentNodes[i].Corridor));
                }

                ProceduralDungeonConnectHelper helper = new ProceduralDungeonConnectHelper(rooms.ToArray(), roads.ToArray());
                m_roomObjectGenerator = new RoomObjectGenerator(helper.Rooms, m_roomGroundObjs, m_roomWallObjs);
                //m_roadObjectGenerator = new RoadObjectGenerator(roads.ToArray(), m_roadGroundObjs, m_roadWallObjs);

                //List<Wall> walls = new List<Wall>();
                //walls.AddRange(m_roomObjectGenerator.WallList);
                //walls.AddRange(m_roadObjectGenerator.WallList);
                //m_pillarObjectGenerator = new PillarObjectGenerator(m_mapSize, walls.ToArray(), m_pillarObjs);
            }
        }

        public override Vector3 GetRandomPosition()
        {
            IntRect roomRect = GetRandomNode().RoomRect;
            return new Vector3(roomRect.center.x, 0, roomRect.center.y);
        }

        private BSPNode GetRandomNode()
        {
            return m_leafNodes[Random.Range(0, m_leafNodes.Count)];
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
                if(m_parentNodes[i].Corridor == null)
                {
                    continue;
                }

                DrawRect(m_parentNodes[i].Corridor.Rect);
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