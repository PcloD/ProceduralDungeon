using UnityEngine;
using System.Collections.Generic;

namespace Dungeon
{
    public class PillarObjectGenerator
    {
        public Transform Parent { get { return m_parent; } }

        private IntVector2 m_mapSize;
        private Wall[] m_walls;
        private GameObject[] m_pillarObjs;
        private Transform m_parent;
        private List<IntVector2> m_pillarPositions;

        public PillarObjectGenerator(IntVector2 mapSize, Wall[] walls, GameObject[] pillarObjs)
        {
            m_mapSize = mapSize;
            m_walls = walls;
            m_pillarObjs = pillarObjs;

            Generate();
        }

        public void Destroy()
        {
            if (m_parent != null)
            {
                GameObject.Destroy(m_parent.gameObject);
            }
        }

        public void Generate()
        {
            GenerateParent();
            UpdatePositions();
            GeneratePillar();
        }

        private void GenerateParent()
        {
            if (m_parent == null)
            {
                m_parent = new GameObject("Pillar Object Parent").transform;
            }
        }

        private void UpdatePositions()
        {
            m_pillarPositions = new List<IntVector2>();

            Wall previousWall = null;
            Wall cacheWall = null;
            for (int z = 0; z <= m_mapSize.z; z++)
            {
                for (int x = 0; x <= m_mapSize.x; x++)
                {
                    cacheWall = GetWall(x, z, false);

                    if ((previousWall == null && cacheWall == null) ||
                        (previousWall != null && cacheWall != null))
                    {
                        continue;
                    }

                    previousWall = cacheWall;
                    m_pillarPositions.Add(new IntVector2(x, z));
                }
            }

            previousWall = null;
            cacheWall = null;
            for (int x = 0; x <= m_mapSize.x; x++)
            {
                for (int z = 0; z <= m_mapSize.z; z++)
                {
                    cacheWall = GetWall(x, z, true);

                    if ((previousWall == null && cacheWall == null) ||
                        (previousWall != null && cacheWall != null))
                    {
                        continue;
                    }

                    previousWall = cacheWall;
                    m_pillarPositions.Add(new IntVector2(x, z));
                }
            }
        }

        private Wall GetWall(int x, int z, bool isVertical)
        {
            Wall cacheWall = null;
            for(int i = 0; i < m_walls.Length; i++)
            {
                cacheWall = m_walls[i];

                if(cacheWall.X != x || cacheWall.Z != z || cacheWall.IsVertical != isVertical)
                {
                    continue;
                }

                return cacheWall;
            }

            return null;
        }

        private void GeneratePillar()
        {
            if(m_pillarPositions.Count == 0)
            {
                return;
            }

            GameObject cacheObj = null;
            for(int i = 0; i < m_pillarPositions.Count; i++)
            {
                cacheObj = GameObject.Instantiate(m_pillarObjs[Random.Range(0, m_pillarObjs.Length)]);
                cacheObj.name = string.Format("Pillar ({0}, {1})", m_pillarPositions[i].x, m_pillarPositions[i].z);
                cacheObj.SetActive(true);
                cacheObj.transform.position = new Vector3(m_pillarPositions[i].x, 0, m_pillarPositions[i].z);
                cacheObj.transform.localScale = Vector3.one;
                cacheObj.transform.SetParent(m_parent.transform);
            }
        }
    }
}