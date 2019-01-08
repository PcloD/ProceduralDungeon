using UnityEngine;
using System.Collections.Generic;

namespace ProceduralDungeon
{
    public class RoadObjectGenerator
    {
        public Transform Parent { get { return m_parent; } }
        public List<Wall> WallList { get { return m_wallList; } }

        private Road[] m_roads;
        private GameObject[] m_groundObjs;
        private GameObject[] m_wallObjs;
        private Transform m_parent;
        private List<Ground> m_groundList;
        private List<Wall> m_wallList;

        public RoadObjectGenerator(Road[] roads, GameObject[] groundObjs, GameObject[] wallObjs)
        {
            m_roads = roads;
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
            RemoveCrossedWalls();
            GenerateGroundObjects();
            GenerateWallObjects();
        }

        private void GenerateParent()
        {
            if(m_parent == null)
            {
                m_parent = new GameObject("Road Object Parent").transform;
            }
        }

        private void GenerateGround()
        {
            if (m_roads == null || m_roads.Length == 0)
            {
                Debug.LogError("[RoadObjectGenerator] - The road data is null or empty.");
                return;
            }

            Road cacheRoad = null;
            int indexX;
            int indexZ;
            int minValue;
            int maxValue;
            Ground cacheGround = null;
            for (int i = 0; i < m_roads.Length; i++)
            {
                cacheRoad = m_roads[i];
                indexX = (int)cacheRoad.Start.x;
                indexZ = (int)cacheRoad.Start.z;
                minValue = (int)cacheRoad.MinBorder;
                maxValue = (int)cacheRoad.MaxBorder;
                if (maxValue != cacheRoad.MaxBorder)
                {
                    maxValue++;
                }

                if (cacheRoad.IsVertical)
                {
                    for (int z = minValue; z < maxValue; z++)
                    {
                        cacheGround = new Ground(indexX, z);
                        if (m_groundList.Contains(cacheGround))
                        {
                            continue;
                        }

                        m_groundList.Add(cacheGround);
                    }
                }
                else
                {
                    for (int x = minValue; x < maxValue; x++)
                    {
                        cacheGround = new Ground(x, indexZ);
                        if (m_groundList.Contains(cacheGround))
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
            if (m_roads == null || m_roads.Length == 0)
            {
                Debug.LogError("[RoadObjectGenerator] - The road data is null or empty.");
                return;
            }

            Road cacheRoad = null;
            int indexX;
            int indexZ;
            int minValue;
            int maxValue;
            Wall cacheWall = null;
            for (int i = 0; i < m_roads.Length; i++)
            {
                cacheRoad = m_roads[i];
                indexX = (int)cacheRoad.Start.x;
                indexZ = (int)cacheRoad.Start.z;
                minValue = (int)cacheRoad.MinBorder;
                maxValue = (int)cacheRoad.MaxBorder;
                if (maxValue != cacheRoad.MaxBorder)
                {
                    maxValue++;
                }

                if (cacheRoad.IsVertical)
                {
                    for (int z = minValue; z < maxValue; z++)
                    {
                        cacheWall = new Wall(indexX, z, cacheRoad.IsVertical);
                        if (!m_wallList.Contains(cacheWall))
                        {
                            m_wallList.Add(cacheWall);
                        }

                        cacheWall = new Wall(indexX + 1, z, cacheRoad.IsVertical);
                        if (!m_wallList.Contains(cacheWall))
                        {
                            m_wallList.Add(cacheWall);
                        }
                    }
                }
                else
                {
                    for (int x = minValue; x < maxValue; x++)
                    {
                        cacheWall = new Wall(x, indexZ, cacheRoad.IsVertical);
                        if (!m_wallList.Contains(cacheWall))
                        {
                            m_wallList.Add(cacheWall);
                        }

                        cacheWall = new Wall(x, indexZ + 1, cacheRoad.IsVertical);
                        if (!m_wallList.Contains(cacheWall))
                        {
                            m_wallList.Add(cacheWall);
                        }
                    }
                }
            }
        }

        private void RemoveCrossedWalls()
        {
            Wall cacheWall = null;
            Road cacheRoad = null;
            List<Wall> crossedWalls = new List<Wall>();
            for(int i = 0; i < m_wallList.Count; i++)
            {
                cacheWall = m_wallList[i];

                if(cacheWall.IsVertical)
                {
                    for(int j = 0; j < m_roads.Length; j++)
                    {
                        cacheRoad = m_roads[j];

                        if (cacheRoad.IsVertical)
                        {
                            continue;
                        }

                        if(cacheWall.X < cacheRoad.MinBorder || cacheWall.X > cacheRoad.MaxBorder)
                        {
                            continue;
                        }

                        if((int)cacheRoad.Start.z != cacheWall.Z)
                        {
                            continue;
                        }

                        crossedWalls.Add(cacheWall);
                        break;
                    }
                }
                else
                {
                    for (int j = 0; j < m_roads.Length; j++)
                    {
                        cacheRoad = m_roads[j];

                        if (!cacheRoad.IsVertical)
                        {
                            continue;
                        }

                        if (cacheWall.Z < cacheRoad.MinBorder || cacheWall.Z > cacheRoad.MaxBorder)
                        {
                            continue;
                        }

                        if ((int)cacheRoad.Start.x != cacheWall.X)
                        {
                            continue;
                        }

                        crossedWalls.Add(cacheWall);
                        break;
                    }
                }
            }

            for(int i = 0; i < crossedWalls.Count; i++)
            {
                m_wallList.Remove(crossedWalls[i]);
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