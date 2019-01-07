using UnityEngine;

namespace ProceduralDungeon
{
    public class RoomBasedProceduralDungeon : MonoBehaviour
    {
        [SerializeField] private GameObject m_roomGroundObj;
        [SerializeField] private GameObject m_roomWallObj;
        [SerializeField] private GameObject m_roadGroundObj;
        [SerializeField] private GameObject m_roadWallObj;

        [SerializeField] private IntVector2 m_mapSize;
        [SerializeField] private int m_totalRoomCount;
        [SerializeField] private int m_selectRoomCount;
        [SerializeField] private IntVector2 m_minRoomSize;
        [SerializeField] private IntVector2 m_maxRoomSize;

        [SerializeField] private bool m_drawMap;
        [SerializeField] private bool m_drawRoom;
        [SerializeField] private bool m_drawSelectRoom;
        [SerializeField] private bool m_drawSelectRoomCenterBias;
        [SerializeField] private bool m_drawDelaunayTriangulation;
        [SerializeField] private bool m_drawSpanningTree;
        [SerializeField] private bool m_drawRoads;

        private RoomGenerator m_roomGenerator;
        private RoadGenerator m_roadGenerator;
        private RoomObjectGenerator m_roomObjectGenerator;
        private RoadObjectGenerator m_roadObjectGenerator;

        [InspectorMethod]
        private void Generate()
        {
            if(m_roomObjectGenerator != null)
            {
                m_roomObjectGenerator.Destroy();
            }

            if (m_roadObjectGenerator != null)
            {
                m_roadObjectGenerator.Destroy();
            }

            m_roomGenerator = new RoomGenerator(m_mapSize, m_totalRoomCount, m_selectRoomCount, m_minRoomSize, m_maxRoomSize);
            m_roadGenerator = new RoadGenerator(m_mapSize, m_roomGenerator.Rooms);
            m_roomObjectGenerator = new RoomObjectGenerator(m_roomGenerator.SelectRooms, m_roomGroundObj, m_roomWallObj);
            m_roadObjectGenerator = new RoadObjectGenerator(m_roadGenerator.Roads, m_roadGroundObj, m_roadWallObj);
        }

        private void OnDrawGizmos()
        {
            DrawMap();
            DrawRoom();
            DrawSelectRoom();
            DrawDelaunayTriangulation();
            DrawSpanningTree();
            DrawRoads();
        }

        private void DrawMap()
        {
            if (!m_drawMap)
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

        private void DrawRoom()
        {
            if (!m_drawRoom)
            {
                return;
            }

            if (m_roomGenerator == null || m_roomGenerator.Rooms == null || m_roomGenerator.Rooms.Length == 0)
            {
                return;
            }

            Gizmos.color = Color.red;

            Room cacheRoom;
            Vector3 leftUp;
            Vector3 rightUp;
            Vector3 leftBottom;
            Vector3 rightBottom;

            for (int i = 0; i < m_roomGenerator.Rooms.Length; i++)
            {
                cacheRoom = m_roomGenerator.Rooms[i];
                if (cacheRoom == null)
                {
                    continue;
                }

                leftUp = new Vector3(cacheRoom.MinBorder.x, cacheRoom.MaxBorder.z);
                rightUp = new Vector3(cacheRoom.MaxBorder.x, cacheRoom.MaxBorder.z);
                leftBottom = new Vector3(cacheRoom.MinBorder.x, cacheRoom.MinBorder.z);
                rightBottom = new Vector3(cacheRoom.MaxBorder.x, cacheRoom.MaxBorder.z);

                Gizmos.DrawSphere(cacheRoom.Center, 0.1f);
                Gizmos.DrawLine(leftUp, rightUp);
                Gizmos.DrawLine(rightUp, rightBottom);
                Gizmos.DrawLine(rightBottom, leftBottom);
                Gizmos.DrawLine(leftBottom, leftUp);
            }
        }

        private void DrawSelectRoom()
        {
            if (!m_drawSelectRoom)
            {
                return;
            }

            if (m_roomGenerator == null || m_roomGenerator.SelectRooms == null || m_roomGenerator.SelectRooms.Length == 0)
            {
                return;
            }

            Gizmos.color = Color.green;

            Room cacheRoom;
            Vector3 leftUp;
            Vector3 rightUp;
            Vector3 leftBottom;
            Vector3 rightBottom;

            for (int i = 0; i < m_roomGenerator.SelectRooms.Length; i++)
            {
                cacheRoom = m_roomGenerator.SelectRooms[i];
                leftUp = cacheRoom.Center + new Vector3(-cacheRoom.Size.x / 2, 0, cacheRoom.Size.z / 2);
                rightUp = cacheRoom.Center + new Vector3(cacheRoom.Size.x / 2, 0, cacheRoom.Size.z / 2);
                leftBottom = cacheRoom.Center + new Vector3(-cacheRoom.Size.x / 2, 0, -cacheRoom.Size.z / 2);
                rightBottom = cacheRoom.Center + new Vector3(cacheRoom.Size.x / 2, 0, -cacheRoom.Size.z / 2);

                Gizmos.DrawSphere(cacheRoom.Center, 0.1f);
                Gizmos.DrawLine(leftUp, rightUp);
                Gizmos.DrawLine(rightUp, rightBottom);
                Gizmos.DrawLine(rightBottom, leftBottom);
                Gizmos.DrawLine(leftBottom, leftUp);
            }

            if (m_drawSelectRoomCenterBias)
            {
                Gizmos.color = Color.red;
                for (int i = 0; i < m_roomGenerator.SelectRooms.Length; i++)
                {
                    cacheRoom = m_roomGenerator.SelectRooms[i];
                    Gizmos.DrawSphere(cacheRoom.Center + cacheRoom.CenterBias, 0.1f);
                }
            }
        }

        private void DrawDelaunayTriangulation()
        {
            if (!m_drawDelaunayTriangulation)
            {
                return;
            }

            if (m_roadGenerator == null ||
                m_roadGenerator.VoronoiDiagram == null ||
                m_roadGenerator.VoronoiDiagram.Edges == null ||
                m_roadGenerator.VoronoiDiagram.Edges.Count == 0)
            {
                return;
            }

            Gizmos.color = Color.red;
            for (int i = 0; i < m_roadGenerator.VoronoiDiagram.Edges.Count; i++)
            {
                Gizmos.DrawLine(m_roadGenerator.VoronoiDiagram.Edges[i].LeftSite, m_roadGenerator.VoronoiDiagram.Edges[i].RightSite);
            }
        }

        private void DrawSpanningTree()
        {
            if (!m_drawSpanningTree)
            {
                return;
            }

            if (m_roadGenerator == null ||
                m_roadGenerator.SpanningTree == null ||
                m_roadGenerator.SpanningTree.Segments == null ||
                m_roadGenerator.SpanningTree.Segments.Count == 0)
            {
                return;
            }

            Gizmos.color = Color.black;
            for (int i = 0; i < m_roadGenerator.SpanningTree.Segments.Count; i++)
            {
                Gizmos.DrawLine(m_roadGenerator.SpanningTree.Segments[i].PointA, m_roadGenerator.SpanningTree.Segments[i].PointB);
            }
        }

        private void DrawRoads()
        {
            if (!m_drawRoads)
            {
                return;
            }

            if (m_roadGenerator == null ||
                m_roadGenerator.Roads == null ||
                m_roadGenerator.Roads.Length == 0)
            {
                return;
            }

            Gizmos.color = Color.blue;
            for (int i = 0; i < m_roadGenerator.Roads.Length; i++)
            {
                Gizmos.DrawLine(m_roadGenerator.Roads[i].Start, m_roadGenerator.Roads[i].End);
            }
        }
    }
}