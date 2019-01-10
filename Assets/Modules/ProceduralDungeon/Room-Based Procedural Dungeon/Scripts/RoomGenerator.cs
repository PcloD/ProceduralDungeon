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
            int width = Random.Range(m_minRoomSize.x, m_maxRoomSize.x + 1);
            int height = Random.Range(m_minRoomSize.z, m_maxRoomSize.z + 1);
            int x = Random.Range(0, m_mapSize.x - width + 1);
            int y = Random.Range(0, m_mapSize.z - height + 1);

            IntRect rect = new IntRect(x, y, width, height);
            if(!IsValidRoom(rect))
            {
                return null;
            }

            return new Room(rect);
        }

        private bool IsValidRoom(IntRect rect)
        {
            if (rect.minBorder.x < 0 || rect.maxBorder.x > m_mapSize.x ||
                rect.minBorder.z < 0 || rect.maxBorder.z > m_mapSize.z)
            {
                return false;
            }

            Room cacheRoom;
            for (int i = 0; i < m_roomList.Count; i++)
            {
                cacheRoom = m_roomList[i];

                if (cacheRoom == null)
                {
                    continue;
                }

                if (Mathf.Abs(cacheRoom.Rect.center.x - rect.center.x + 1) < (cacheRoom.Rect.width + rect.width) &&
                    Mathf.Abs(cacheRoom.Rect.center.y - rect.center.y + 1) < (cacheRoom.Rect.height + rect.height))
                {
                    return false;
                }
            }

            return true;
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