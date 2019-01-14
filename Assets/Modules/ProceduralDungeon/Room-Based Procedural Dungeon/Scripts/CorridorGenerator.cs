using UnityEngine;
using System.Collections.Generic;
using Voronoi;
using SpanningTree;

namespace Dungeon
{
    public class CorridorGenerator
    { 
        public VoronoiDiagram VoronoiDiagram { get { return m_voronoiDiagram; } }
        public BaseSpanningTree SpanningTree { get { return m_spanningTree; } }
        public Corridor[] Corridors { get { return m_corridors; } }

        private IntVector2 m_mapSize;
        private Room[] m_rooms;
        private VoronoiDiagram m_voronoiDiagram;
        private BaseSpanningTree m_spanningTree;
        private List<Corridor> m_roadList;
        private Corridor[] m_corridors;

        public CorridorGenerator(IntVector2 mapSize, Room[] rooms)
        {
            m_mapSize = mapSize;
            m_rooms = rooms;

            Generate();
        }
        
        private void Generate()
        {
            GenerateSpannningTree();
            GenerateRoads();
            DividedRoads();
            UpdateRoads();
        }

        private void GenerateSpannningTree()
        {
            List<Vector3> roomCenters = new List<Vector3>();
            for (int i = 0; i < m_rooms.Length; i++)
            {
                roomCenters.Add(m_rooms[i].Center + m_rooms[i].CenterBias);
            }

            m_voronoiDiagram = new VoronoiDiagram(roomCenters.ToArray(), new VBorder(m_mapSize.x, m_mapSize.z));

            STEdge cacheEdge = null;
            List<STEdge> stEdges = new List<STEdge>();
            for (int i = 0; i < m_voronoiDiagram.Edges.Count; i++)
            {
                cacheEdge = new STEdge();
                cacheEdge.PointA = m_voronoiDiagram.Edges[i].LeftSite;
                cacheEdge.PointB = m_voronoiDiagram.Edges[i].RightSite;

                stEdges.Add(cacheEdge);
            }
            m_spanningTree = new MinimumSpanningTree(stEdges.ToArray());
        }

        private void GenerateRoads()
        {
            if (m_roadList == null)
            {
                m_roadList = new List<Corridor>();
            }
            else
            {
                m_roadList.Clear();
            }

            Room cacheRoomA;
            Room cacheRoomB;
            Vector3 roadCenter;
            Vector3 roadStartA;
            Vector3 roadStartB;
            Corridor cacheRoad;

            for (int i = 0; i < m_spanningTree.Segments.Count; i++)
            {
                cacheRoomA = GetRoomByPoint(m_spanningTree.Segments[i].PointA);
                cacheRoomB = GetRoomByPoint(m_spanningTree.Segments[i].PointB);
                roadCenter = GetRoadCenter(cacheRoomA, cacheRoomB);

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

                    cacheRoad = new Corridor(roadStartA, roadCenter);
                    m_roadList.Add(cacheRoad);
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

                    cacheRoad = new Corridor(roadStartB, roadCenter);
                    m_roadList.Add(cacheRoad);
                }
            }
        }

        private Room GetRoomByPoint(Vector3 point)
        {
            for (int i = 0; i < m_rooms.Length; i++)
            {
                if (m_rooms[i].InBoundary(point))
                {
                    return m_rooms[i];
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

        private void DividedRoads()
        {
            Corridor cacheRoad;
            List<Corridor> removeRoads = new List<Corridor>();
            List<Corridor> newRoads = new List<Corridor>();
            List<Room> crossedRooms;
            for (int i = 0; i < m_roadList.Count; i++)
            {
                cacheRoad = m_roadList[i];
                crossedRooms = GetRoadCrossedRooms(m_roadList[i]);
                if (crossedRooms.Count == 0)
                {
                    continue;
                }

                removeRoads.Add(cacheRoad);

                if (m_roadList[i].IsVertical)
                {
                    crossedRooms.Sort(delegate (Room x, Room y)
                    {
                        return x.Center.z.CompareTo(y.Center.z);
                    });

                    if (cacheRoad.MinBorder < crossedRooms[0].MinBorder.z)
                    {
                        newRoads.Add(new Corridor(new Vector3(cacheRoad.Start.x, 0, cacheRoad.MinBorder), new Vector3(cacheRoad.Start.x, 0, crossedRooms[0].MinBorder.z)));
                    }

                    if (cacheRoad.MaxBorder > crossedRooms[crossedRooms.Count - 1].MaxBorder.z)
                    {
                        newRoads.Add(new Corridor(new Vector3(cacheRoad.Start.x, 0, crossedRooms[crossedRooms.Count - 1].MaxBorder.z), new Vector3(cacheRoad.Start.x, 0, cacheRoad.MaxBorder)));
                    }

                    for (int j = 0; j < crossedRooms.Count - 1; j++)
                    {
                        newRoads.Add(new Corridor(new Vector3(cacheRoad.Start.x, 0, crossedRooms[j].MaxBorder.z), new Vector3(cacheRoad.Start.x, 0, crossedRooms[j + 1].MinBorder.z)));
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
                        newRoads.Add(new Corridor(new Vector3(cacheRoad.MinBorder, 0, cacheRoad.Start.z), new Vector3(crossedRooms[0].MinBorder.x, 0, cacheRoad.Start.z)));
                    }

                    if (cacheRoad.MaxBorder > crossedRooms[crossedRooms.Count - 1].MaxBorder.x)
                    {
                        newRoads.Add(new Corridor(new Vector3(crossedRooms[crossedRooms.Count - 1].MaxBorder.x, 0, cacheRoad.Start.z), new Vector3(cacheRoad.MaxBorder, 0, cacheRoad.Start.z)));
                    }

                    for (int j = 0; j < crossedRooms.Count - 1; j++)
                    {
                        newRoads.Add(new Corridor(new Vector3(crossedRooms[j].MaxBorder.x, 0, cacheRoad.Start.z), new Vector3(crossedRooms[j + 1].MinBorder.x, 0, cacheRoad.Start.z)));
                    }
                }
            }

            for (int i = 0; i < removeRoads.Count; i++)
            {
                m_roadList.Remove(removeRoads[i]);
            }

            m_roadList.AddRange(newRoads);
        }

        private List<Room> GetRoadCrossedRooms(Corridor road)
        {
            List<Room> crossedRooms = new List<Room>();
            Room cacheRoom;
            for (int i = 0; i < m_rooms.Length; i++)
            {
                cacheRoom = m_rooms[i];

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

        private void UpdateRoads()
        {
            m_corridors = m_roadList.ToArray();
        }
    }
}