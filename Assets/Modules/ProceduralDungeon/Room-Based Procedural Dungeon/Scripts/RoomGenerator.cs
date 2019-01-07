using UnityEngine;
using System.Collections.Generic;

namespace ProceduralDungeon
{
    public class RoomGenerator
    {
        public Room[] Rooms { get { return m_rooms; } }
        public Room[] SelectRooms { get { return m_selectRooms; } }

        private IntVector2 m_mapSize;
        private int m_totalRoomCount;
        private int m_selectRoomCount;
        private IntVector2 m_minRoomSize;
        private IntVector2 m_maxRoomSize;

        private List<Room> m_roomList;
        private List<Room> m_selectRoomList;
        private Room[] m_rooms;
        private Room[] m_selectRooms;

        public RoomGenerator(IntVector2 mapSize, int totalRoomCount, int selectRoomCount, IntVector2 minRoomSize, IntVector2 maxRoomSize)
        {
            m_mapSize = mapSize;
            m_totalRoomCount = totalRoomCount;
            m_selectRoomCount = selectRoomCount;
            m_minRoomSize = minRoomSize;
            m_maxRoomSize = maxRoomSize;

            Generate();
        }

        private void Generate()
        {
            GenerateRooms();
            GenerateSelectRooms();
            UpdateRooms();
        }

        private void GenerateRooms()
        {
            if (m_roomList == null)
            {
                m_roomList = new List<Room>();
            }
            else
            {
                m_roomList.Clear();
            }

            Room cacheRoom = null;
            for (int i = 0; i < m_totalRoomCount * m_totalRoomCount; i++)
            {
                cacheRoom = CreateRoom();
                if(cacheRoom == null)
                {
                    continue;
                }

                m_roomList.Add(cacheRoom);

                if(m_roomList.Count == m_totalRoomCount)
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
            for(int i = 0; i < m_roomList.Count; i++)
            {
                cacheRoom = m_roomList[i];

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

        private void GenerateSelectRooms()
        {
            m_roomList.Sort(delegate (Room x, Room y)
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

            if (m_selectRoomList == null)
            {
                m_selectRoomList = new List<Room>();
            }
            else
            {
                m_selectRoomList.Clear();
            }

            for(int i = 0; i < m_roomList.Count; i++)
            {
                if(m_roomList[i] == null)
                {
                    continue;
                }

                m_selectRoomList.Add(m_roomList[i]);

                if(m_selectRoomList.Count == m_selectRoomCount)
                {
                    break;
                }
            }
        }

        private void UpdateRooms()
        {
            m_rooms = m_roomList.ToArray();
            m_selectRooms = m_selectRoomList.ToArray();
        }
    }
}