using UnityEngine;
using System.Collections.Generic;

namespace Dungeon
{
    public class RoomObjectGenerator
    {
        public Transform Parent { get { return m_parent; } }
        public List<Wall> WallList { get { return m_wallList; } }

        private Room[] m_rooms;
        private GameObject[] m_groundObjs;
        private GameObject[] m_wallObjs;
        private Transform m_parent;
        private List<Ground> m_groundList;
        private List<Wall> m_wallList;

        public RoomObjectGenerator(Room[] rooms, GameObject[] groundObjs, GameObject[] wallObjs)
        {
            m_rooms = rooms;
            m_groundObjs = groundObjs;
            m_wallObjs = wallObjs;

            Generate();
        }

        public void Destroy()
        {
            if(m_parent != null)
            {
                GameObject.Destroy(m_parent.gameObject);
            }
        }

        private void Generate()
        {
            m_groundList = new List<Ground>();
            m_wallList = new List<Wall>();

            GenerateParent();
            GenerateGround();
            GenerateWall();
            GenerateGroundObjects();
            GenerateWallObjects();
        }

        private void GenerateParent()
        {
            if(m_parent == null)
            {
                m_parent = new GameObject("Room Object Parent").transform;
            }
        }

        private void GenerateGround()
        {
            if(m_rooms == null || m_rooms.Length == 0)
            {
                Debug.LogError("[RoomObjectGenerator] - The room data is null or empty.");
                return;
            }

            Room cacheRoom = null;
            Ground cacheGround = null;
            for(int i = 0; i < m_rooms.Length; i++)
            {
                cacheRoom = m_rooms[i];

                for(int x = (int)cacheRoom.MinBorder.x; x <= (int)cacheRoom.MaxBorder.x - 1; x++)
                {
                    for (int z = (int)cacheRoom.MinBorder.z; z <= (int)cacheRoom.MaxBorder.z - 1; z++)
                    {
                        cacheGround = new Ground(x, z);
                        if(m_groundList.Contains(cacheGround))
                        {
                            continue;
                        }

                        m_groundList.Add(cacheGround);
                    }
                }
            }
        }

        private void GenerateWall()
        {
            if (m_rooms == null || m_rooms.Length == 0)
            {
                Debug.LogError("[RoomObjectGenerator] - The room data is null or empty.");
                return;
            }

            Room cacheRoom = null;
            Wall cacheWall = null;
            int minBorderX;
            int maxBorderX;
            int minBorderZ;
            int maxBorderZ;
            for (int i = 0; i < m_rooms.Length; i++)
            {
                cacheRoom = m_rooms[i];
                minBorderX = (int)cacheRoom.MinBorder.x;
                maxBorderX = (int)cacheRoom.MaxBorder.x;
                minBorderZ = (int)cacheRoom.MinBorder.z;
                maxBorderZ = (int)cacheRoom.MaxBorder.z;

                for(int x = minBorderX; x < maxBorderX; x++)
                {
                    cacheWall = new Wall(x, minBorderZ, false);
                    if(!cacheRoom.IsCullingWall(cacheWall))
                    {
                        m_wallList.Add(cacheWall);
                    }

                    cacheWall = new Wall(x, maxBorderZ, false);
                    if (!cacheRoom.IsCullingWall(cacheWall))
                    {
                        m_wallList.Add(cacheWall);
                    }
                }

                for (int z = minBorderZ; z < maxBorderZ; z++)
                {
                    cacheWall = new Wall(minBorderX, z, true);
                    if (!cacheRoom.IsCullingWall(cacheWall))
                    {
                        m_wallList.Add(cacheWall);
                    }

                    cacheWall = new Wall(maxBorderX, z, true);
                    if (!cacheRoom.IsCullingWall(cacheWall))
                    {
                        m_wallList.Add(cacheWall);
                    }
                }
            }
        }

        private void GenerateGroundObjects()
        {
            Ground cacheGround = null;
            GameObject cacheObj = null;
            for (int i = 0; i < m_groundList.Count; i++)
            {
                cacheGround = m_groundList[i];

                cacheObj = GameObject.Instantiate(m_groundObjs[Random.Range(0, m_groundObjs.Length)]);
                cacheObj.name = string.Format("Ground ({0}, {1})", cacheGround.X, cacheGround.Z);
                cacheObj.SetActive(true);
                cacheObj.transform.position = new Vector3(cacheGround.X + 0.5f, 0, cacheGround.Z + 0.5f);
                cacheObj.transform.localScale = Vector3.one;
                cacheObj.transform.SetParent(m_parent.transform);
            }
        }

        private void GenerateWallObjects()
        {
            Wall cacheWall = null;
            GameObject cacheObj = null;
            for (int i = 0; i < m_wallList.Count; i++)
            {
                cacheWall = m_wallList[i];

                cacheObj = GameObject.Instantiate(m_wallObjs[Random.Range(0, m_wallObjs.Length)]);
                cacheObj.name = string.Format("Wall ({0}, {1})", cacheWall.X, cacheWall.Z);
                cacheObj.SetActive(true);
                cacheObj.transform.position = new Vector3(cacheWall.IsVertical ? cacheWall.X : cacheWall.X + 0.5f, 0f, cacheWall.IsVertical ? cacheWall.Z + 0.5f : cacheWall.Z);
                cacheObj.transform.localScale = Vector3.one;
                cacheObj.transform.localEulerAngles = new Vector3(0, cacheWall.IsVertical ? 90 : 0, 0);
                cacheObj.transform.SetParent(m_parent.transform);
            }
        }
    }
}