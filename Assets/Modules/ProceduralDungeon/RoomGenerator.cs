using UnityEngine;
using System.Collections.Generic;
using Voronoi;
using SpanningTree;

namespace ProceduralDungeon
{
    public class RoomGenerator : MonoBehaviour
    {
        [SerializeField] private IntVector3 m_mapSize;
        [SerializeField] private int m_mainRoomCount;
        [SerializeField] private int m_totalRoomCount;
        [SerializeField] private IntVector3 m_minRoomSize;
        [SerializeField] private IntVector3 m_maxRoomSize;

        [SerializeField] private bool m_drawMap;
        [SerializeField] private bool m_drawRoom;
        [SerializeField] private bool m_drawMainRoom;
        [SerializeField] private bool m_drawMainRoomCenterBias;
        [SerializeField] private bool m_drawDelaunayTriangulation;
        [SerializeField] private bool m_drawSpanningTree;
        [SerializeField] private bool m_drawRoadCenters;
        [SerializeField] private bool m_drawOptimizedRoad;
        [SerializeField] private bool m_drawCrossedRoomRoad;

        private List<Room> m_rooms;
        private List<Room> m_mainRooms;
        private VoronoiDiagram m_voronoiDiagram;
        private BaseSpanningTree m_spanningTree;
        private List<Vector3> m_roadCenters;
        private List<Road> m_roads;

        [InspectorMethod]
        private void Generate()
        {
            GenerateRooms();
            GenerateMainRooms();
            GenerateRoads();
            GenerateRoadCenters();
            GenerateOptimizedRoads();
            RefreshOptimizedRoads();
        }

        private void GenerateRooms()
        {
            if (m_rooms == null)
            {
                m_rooms = new List<Room>();
            }
            else
            {
                m_rooms.Clear();
            }

            Room cacheRoom = null;
            for (int i = 0; i < m_totalRoomCount * m_totalRoomCount; i++)
            {
                cacheRoom = CreateRoom();
                if(cacheRoom == null)
                {
                    continue;
                }

                m_rooms.Add(cacheRoom);

                if(m_rooms.Count == m_totalRoomCount)
                {
                    break;
                }
            }
        }

        private Room CreateRoom()
        {
            Vector3 size = Vector3.zero;
            Vector3 center = Vector3.zero;

            size.x = Random.Range(m_minRoomSize.x, m_maxRoomSize.x + 1);
            size.z = Random.Range(m_minRoomSize.z, m_maxRoomSize.z + 1);
            center.x = Random.Range(1, m_mapSize.x);
            center.z = Random.Range(1, m_mapSize.z);

            if((int)size.x % 2 == 1)
            {
                int random = Random.Range(0, 2);
                if(random == 0)
                {
                    center.x -= -0.5f;
                }
                else
                {
                    center.x += 0.5f;
                }
            }

            if ((int)size.z % 2 == 1)
            {
                int random = Random.Range(0, 2);
                if (random == 0)
                {
                    center.z -= -0.5f;
                }
                else
                {
                    center.z += 0.5f;
                }
            }

            if (IsValidRoom(center, size))
            {
                return null;
            }

            Room room = new Room(center, size);
            return room;
        }

        private bool IsValidRoom(Vector3 center, Vector3 size)
        {
            if (center.x - size.x / 2 < 0 || center.x + size.x / 2 > m_mapSize.x ||
                center.z - size.z / 2 < 0 || center.z + size.z / 2 > m_mapSize.z)
            {
                return true;
            }

            Room cacheRoom;

            for(int i = 0; i < m_rooms.Count; i++)
            {
                cacheRoom = m_rooms[i];

                if(cacheRoom == null)
                {
                    continue;
                }

                if(Mathf.Abs(cacheRoom.Center.x - center.x) < (cacheRoom.Size.x + size.x) * 0.7f &&
                    Mathf.Abs(cacheRoom.Center.z - center.z) < (cacheRoom.Size.z + size.z) * 0.7f)
                {
                    return true;
                }
            }

            return false;
        }

        private void GenerateMainRooms()
        {
            m_rooms.Sort(delegate (Room x, Room y)
            {
                if(x == null)
                {
                    return 1;
                }
                else if(y == null)
                {
                    return 0;
                }
                else
                {
                    return y.Priority.CompareTo(x.Priority);
                }
            });

            if (m_mainRooms == null)
            {
                m_mainRooms = new List<Room>();
            }
            else
            {
                m_mainRooms.Clear();
            }

            for(int i = 0; i < m_rooms.Count; i++)
            {
                if(m_rooms[i] == null)
                {
                    continue;
                }

                m_mainRooms.Add(m_rooms[i]);

                if(m_mainRooms.Count == m_mainRoomCount)
                {
                    break;
                }
            }
        }

        private void GenerateRoads()
        {
            List<Vector3> mainRoomCenters = new List<Vector3>();
            for(int i = 0; i < m_mainRooms.Count; i++)
            {
                mainRoomCenters.Add(m_mainRooms[i].Center);
            }

            m_voronoiDiagram = new VoronoiDiagram(mainRoomCenters.ToArray(), new VBorder(m_mapSize.x, m_mapSize.z));

            STEdge cacheEdge = null;
            List<STEdge> stEdges = new List<STEdge>();
            for(int i = 0; i < m_voronoiDiagram.Edges.Count; i++)
            {
                cacheEdge = new STEdge();
                cacheEdge.PointA = m_voronoiDiagram.Edges[i].LeftSite;
                cacheEdge.PointB = m_voronoiDiagram.Edges[i].RightSite;

                stEdges.Add(cacheEdge);
            }
            m_spanningTree = new MinimumSpanningTree(stEdges.ToArray());
        }

        private void GenerateRoadCenters()
        {
            if (m_roadCenters == null)
            {
                m_roadCenters = new List<Vector3>();
            }
            else
            {
                m_roadCenters.Clear();
            }

            Room cacheRoomA;
            Room cacheRoomB;

            for (int i = 0; i < m_spanningTree.Segments.Count; i++)
            {
                cacheRoomA = GetRoomByPoint(m_spanningTree.Segments[i].PointA);
                cacheRoomB = GetRoomByPoint(m_spanningTree.Segments[i].PointB);

                m_roadCenters.Add(GetRoadCenter(cacheRoomA, cacheRoomB));
            }
        }

        private Room GetRoomByPoint(Vector3 point)
        {
            for (int i = 0; i < m_mainRooms.Count; i++)
            {
                if (m_mainRooms[i].InBoundary(point))
                {
                    return m_mainRooms[i];
                }
            }

            return null;
        }

        private Vector3 GetRoadCenter(Room roomA, Room roomB)
        {
            Vector3 roomCenterA = roomA.Center + roomA.CenterBias;
            Vector3 roomCenterB = roomB.Center + roomB.CenterBias;
            Vector3 direction = roomCenterB - roomCenterA;
            Vector3 roadCenter = Vector3.zero;

            if (roomCenterA.x == roomCenterB.x)
            {
                roadCenter.x = roomCenterA.x;
                roadCenter.z = (roomCenterA.z + roomCenterB.z) / 2;
            }
            else if (roomCenterA.z == roomCenterB.z)
            {
                roadCenter.x = (roomCenterA.x + roomCenterB.x) / 2;
                roadCenter.z = roomCenterA.z;
            }
            else
            {
                int random = Random.Range(0, 2);
                if (random == 0)
                {
                    roadCenter = roomCenterA + new Vector3(direction.x, 0, 0);
                }
                else
                {
                    roadCenter = roomCenterA + new Vector3(0, 0, direction.z);
                }
            }

            if (roomA.InBoundary(roadCenter))
            {
                if (roomCenterA.x == roadCenter.x)
                {
                    if (roomCenterA.x > roomCenterB.x)
                    {
                        roadCenter.x -= roomA.Size.x / 2 + roomA.CenterBias.x;
                    }
                    else if (roomCenterA.x < roomCenterB.x)
                    {
                        roadCenter.x += roomA.Size.x / 2 - roomA.CenterBias.x;
                    }
                }
                else if (roomCenterA.z == roadCenter.z)
                {
                    if (roomCenterA.z > roomCenterB.z)
                    {
                        roadCenter.z -= roomA.Size.z / 2 + roomA.CenterBias.z;
                    }
                    else if (roomCenterA.z < roomCenterB.z)
                    {
                        roadCenter.z += roomA.Size.z / 2 - roomA.CenterBias.z;
                    }
                }
            }
            else if (roomB.InBoundary(roadCenter))
            {
                if (roomCenterB.x == roadCenter.x)
                {
                    if (roomCenterB.x > roomCenterA.x)
                    {
                        roadCenter.x -= roomB.Size.x / 2 + roomB.CenterBias.x;
                    }
                    else if (roomCenterB.x < roomCenterA.x)
                    {
                        roadCenter.x += roomB.Size.x / 2 - roomB.CenterBias.x;
                    }
                }
                else if (roomCenterB.z == roadCenter.z)
                {
                    if (roomCenterB.z > roomCenterA.z)
                    {
                        roadCenter.z -= roomB.Size.z / 2 + roomB.CenterBias.z;
                    }
                    else if (roomCenterB.z < roomCenterA.z)
                    {
                        roadCenter.z += roomB.Size.z / 2 - roomB.CenterBias.z;
                    }
                }
            }

            return roadCenter;
        }

        private void GenerateOptimizedRoads()
        {
            if (m_roads == null)
            {
                m_roads = new List<Road>();
            }
            else
            {
                m_roads.Clear();
            }
            
            Room cacheRoomA;
            Room cacheRoomB;
            Vector3 roadCenter;
            Vector3 roadStartA;
            Vector3 roadStartB;

            for (int i = 0; i < m_spanningTree.Segments.Count; i++)
            {
                cacheRoomA = GetRoomByPoint(m_spanningTree.Segments[i].PointA);
                cacheRoomB = GetRoomByPoint(m_spanningTree.Segments[i].PointB);

                roadCenter = m_roadCenters[i];

                if (!cacheRoomA.InBoundary(roadCenter))
                {
                    roadStartA = cacheRoomA.Center + cacheRoomA.CenterBias;

                    if (roadStartA.x == roadCenter.x)
                    {
                        if (roadCenter.z > roadStartA.z)
                        {
                            roadStartA.z = cacheRoomA.MaxBorder.z;
                            roadStartA.x = cacheRoomA.Center.x + cacheRoomA.CenterBias.x;
                        }
                        else
                        {
                            roadStartA.z = cacheRoomA.MinBorder.z;
                            roadStartA.x = cacheRoomA.Center.x + cacheRoomA.CenterBias.x;
                        }
                    }
                    else if (roadStartA.z == roadCenter.z)
                    {
                        if (roadCenter.x > roadStartA.x)
                        {
                            roadStartA.x = cacheRoomA.MaxBorder.x;
                            roadStartA.z = cacheRoomA.Center.z + cacheRoomA.CenterBias.z;
                        }
                        else
                        {
                            roadStartA.x = cacheRoomA.MinBorder.x;
                            roadStartA.z = cacheRoomA.Center.z + cacheRoomA.CenterBias.z;
                        }
                    }

                    m_roads.Add(new Road(roadStartA, roadCenter));
                }

                if (!cacheRoomB.InBoundary(roadCenter))
                {
                    roadStartB = cacheRoomB.Center + cacheRoomB.CenterBias;

                    if (roadStartB.x == roadCenter.x)
                    {
                        if (roadCenter.z > roadStartB.z)
                        {
                            roadStartB.z = cacheRoomB.MaxBorder.z;
                            roadStartB.x = cacheRoomB.Center.x + cacheRoomB.CenterBias.x;
                        }
                        else
                        {
                            roadStartB.z = cacheRoomB.MinBorder.z;
                            roadStartB.x = cacheRoomB.Center.x + cacheRoomB.CenterBias.x;
                        }
                    }
                    else if (roadStartB.z == roadCenter.z)
                    {
                        if (roadCenter.x > roadStartB.x)
                        {
                            roadStartB.x = cacheRoomB.MaxBorder.x;
                            roadStartB.z = cacheRoomB.Center.z + cacheRoomB.CenterBias.z;
                        }
                        else
                        {
                            roadStartB.x = cacheRoomB.MaxBorder.x;
                            roadStartB.z = cacheRoomB.Center.z + cacheRoomB.CenterBias.z;
                        }
                    }

                    m_roads.Add(new Road(roadStartB, roadCenter));
                }
            }
        }

        private void RefreshOptimizedRoads()
        {
            Road cacheRoad;
            List<Road> removeRoads = new List<Road>();
            List<Road> newRoads = new List<Road>();
            List<Room> crossedRooms;
            for (int i = 0; i < m_roads.Count; i++)
            {
                cacheRoad = m_roads[i];
                crossedRooms = GetRoadCrossedRooms(m_roads[i]);
                if (crossedRooms.Count == 0)
                {
                    continue;
                }

                removeRoads.Add(cacheRoad);

                if (m_roads[i].IsVertical)
                {
                    crossedRooms.Sort(delegate (Room x, Room y)
                    {
                        return x.Center.z.CompareTo(y.Center.z);
                    });

                    if (cacheRoad.MinBorder < crossedRooms[0].MinBorder.z)
                    {
                        newRoads.Add(new Road(new Vector3(cacheRoad.Start.x, 0, cacheRoad.MinBorder), new Vector3(cacheRoad.Start.x, 0, crossedRooms[0].MinBorder.z)));
                    }

                    if (cacheRoad.MaxBorder > crossedRooms[crossedRooms.Count - 1].MaxBorder.z)
                    {
                        newRoads.Add(new Road(new Vector3(cacheRoad.Start.x, 0, crossedRooms[crossedRooms.Count - 1].MaxBorder.z), new Vector3(cacheRoad.Start.x, 0, cacheRoad.MaxBorder)));
                    }

                    for (int j = 0; j < crossedRooms.Count - 1; j++)
                    {
                        newRoads.Add(new Road(new Vector3(cacheRoad.Start.x, 0, crossedRooms[j].MaxBorder.z), new Vector3(cacheRoad.Start.x, 0, crossedRooms[j + 1].MinBorder.z)));
                    }
                }
                else
                {
                    crossedRooms.Sort(delegate (Room x, Room y)
                    {
                        return x.Center.x.CompareTo(y.Center.x);
                    });

                    if (cacheRoad.MinBorder < crossedRooms[0].MinBorder.x)
                    {
                        newRoads.Add(new Road(new Vector3(cacheRoad.MinBorder, 0, cacheRoad.Start.z), new Vector3(crossedRooms[0].MinBorder.x, 0, cacheRoad.Start.z)));
                    }

                    if (cacheRoad.MaxBorder > crossedRooms[crossedRooms.Count - 1].MaxBorder.x)
                    {
                        newRoads.Add(new Road(new Vector3(crossedRooms[crossedRooms.Count - 1].MaxBorder.x, 0, cacheRoad.Start.z), new Vector3(cacheRoad.MaxBorder, 0, cacheRoad.Start.z)));
                    }

                    for (int j = 0; j < crossedRooms.Count - 1; j++)
                    {
                        newRoads.Add(new Road(new Vector3(crossedRooms[j].MaxBorder.x, 0, cacheRoad.Start.z), new Vector3(crossedRooms[j + 1].MinBorder.x, 0, cacheRoad.Start.z)));
                    }
                }
            }

            for (int i = 0; i < removeRoads.Count; i++)
            {
                m_roads.Remove(removeRoads[i]);
            }

            m_roads.AddRange(newRoads);
        }

        private List<Room> GetRoadCrossedRooms(Road road)
        {
            List<Room> crossedRooms = new List<Room>();
            Room cacheRoom;
            for (int i = 0; i < m_mainRooms.Count; i++)
            {
                cacheRoom = m_mainRooms[i];

                if (road.IsVertical)
                {
                    if (road.Start.x <= cacheRoom.MinBorder.x ||
                        road.Start.x >= cacheRoom.MaxBorder.x)
                    {
                        continue;
                    }
                    else if (road.MinBorder >= cacheRoom.MaxBorder.z || road.MaxBorder <= cacheRoom.MinBorder.z)
                    {
                        continue;
                    }
                    else
                    {
                        crossedRooms.Add(cacheRoom);
                    }
                }
                else
                {
                    if (road.Start.z <= cacheRoom.MinBorder.z ||
                        road.Start.z >= cacheRoom.MaxBorder.z)
                    {
                        continue;
                    }
                    else if (road.MinBorder >= cacheRoom.MaxBorder.x || road.MaxBorder <= cacheRoom.MinBorder.x)
                    {
                        continue;
                    }
                    else
                    {
                        crossedRooms.Add(cacheRoom);
                    }
                }
            }

            return crossedRooms;
        }

        private void OnDrawGizmos()
        {
            DrawMap();
            DrawRoom();
            DrawMainRoom();
            DrawDelaunayTriangulation();
            DrawSpanningTree();
            DrawRoadCenter();
            DrawOptimizedRoad();
            DrawCrossedRoomRoad();
        }

        private void DrawMap()
        {
            if(!m_drawMap)
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
            if(!m_drawRoom)
            {
                return;
            }

            if (m_rooms == null || m_rooms.Count == 0)
            {
                return;
            }

            Gizmos.color = Color.red;

            Room cacheRoom;
            Vector3 leftUp;
            Vector3 rightUp;
            Vector3 leftBottom;
            Vector3 rightBottom;

            for (int i = 0; i < m_rooms.Count; i++)
            {
                cacheRoom = m_rooms[i];
                if(cacheRoom == null)
                {
                    continue;
                }

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
        }

        private void DrawMainRoom()
        {
            if (!m_drawMainRoom)
            {
                return;
            }

            if (m_mainRooms == null || m_mainRooms.Count == 0)
            {
                return;
            }

            Gizmos.color = Color.green;

            Room cacheRoom;
            Vector3 leftUp;
            Vector3 rightUp;
            Vector3 leftBottom;
            Vector3 rightBottom;

            for (int i = 0; i < m_mainRooms.Count; i++)
            {
                cacheRoom = m_mainRooms[i];
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

            if(m_drawMainRoomCenterBias)
            {
                Gizmos.color = Color.red;
                for (int i = 0; i < m_mainRooms.Count; i++)
                {
                    cacheRoom = m_mainRooms[i];
                    Gizmos.DrawSphere(cacheRoom.Center + cacheRoom.CenterBias, 0.1f);
                }
            }
        }

        private void DrawDelaunayTriangulation()
        {
            if(!m_drawDelaunayTriangulation)
            {
                return;
            }

            if (m_voronoiDiagram == null || m_voronoiDiagram.Edges == null || m_voronoiDiagram.Edges.Count == 0)
            {
                return;
            }

            Gizmos.color = Color.red;
            for (int i = 0; i < m_voronoiDiagram.Edges.Count; i++)
            {
                Gizmos.DrawLine(m_voronoiDiagram.Edges[i].LeftSite, m_voronoiDiagram.Edges[i].RightSite);
            }
        }

        private void DrawSpanningTree()
        {
            if(!m_drawSpanningTree)
            {
                return;
            }

            if(m_spanningTree == null || m_spanningTree.Segments == null || m_spanningTree.Segments.Count == 0)
            {
                return;
            }

            Gizmos.color = Color.black;
            for (int i = 0; i < m_spanningTree.Segments.Count; i++)
            {
                Gizmos.DrawLine(m_spanningTree.Segments[i].PointA, m_spanningTree.Segments[i].PointB);
            }
        }

        private void DrawRoadCenter()
        {
            if (!m_drawRoadCenters)
            {
                return;
            }

            if (m_roadCenters == null || m_roadCenters.Count == 0)
            {
                return;
            }

            Gizmos.color = Color.yellow;
            for (int i = 0; i < m_roadCenters.Count; i++)
            {
                Gizmos.DrawSphere(m_roadCenters[i], 0.1f);
            }
        }

        private void DrawOptimizedRoad()
        {
            if (!m_drawOptimizedRoad)
            {
                return;
            }

            if(m_roads == null || m_roads.Count == 0)
            {
                return;
            }

            Gizmos.color = Color.blue;
            for (int i = 0; i < m_roads.Count; i++)
            {
                Gizmos.DrawLine(m_roads[i].Start, m_roads[i].End);
            }
        }

        private void DrawCrossedRoomRoad()
        {
            if (!m_drawCrossedRoomRoad)
            {
                return;
            }

            if (m_roads == null || m_roads.Count == 0)
            {
                return;
            }

            Gizmos.color = Color.red;
            for (int i = 0; i < m_roads.Count; i++)
            {
                if(GetRoadCrossedRooms(m_roads[i]).Count != 0)
                {
                    Gizmos.DrawLine(m_roads[i].Start, m_roads[i].End);
                }
            }
        }
    }
}