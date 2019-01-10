using UnityEngine;
using System.Collections.Generic;

namespace ProceduralDungeon
{
    public class Room
    {
        public IntRect Rect { get { return m_rect; } }
        public Vector3 Center { get { return m_center; } }
        public Vector3 CenterBias { get { return m_centerBias; } }
        public Vector3 Size { get { return m_size; } }
        public Vector3 MinBorder { get { return m_minBorder; } }
        public Vector3 MaxBorder { get { return m_maxBorder; } }
        public int Priority { get { return m_priority; } }

        private IntRect m_rect;
        private Vector3 m_center;
        private Vector3 m_centerBias;
        private Vector3 m_size;
        private Vector3 m_minBorder;
        private Vector3 m_maxBorder;
        private int m_priority;
        private List<Vector3> m_cornerPositions;
        private List<Wall> m_cullingWalls;

        public Room(IntRect rect)
        {
            m_rect = rect;
            m_center = new Vector3(rect.Center.x, 0, rect.Center.y);
            m_size = new Vector3(rect.Width, 0, rect.Height);
            m_priority = (int)(m_size.x * m_size.z);

            UpdateCenterBias();
            UpdateBorder();
            UpdateCorners();
        }

        private void UpdateCenterBias()
        {
            m_centerBias = Vector3.zero;
            int random = 0;
            if (m_center.x == (int)m_center.x)
            {
                random = Random.Range(0, 2);
                if(random == 0)
                {
                    m_centerBias.x = 0.5f;
                }
                else
                {
                    m_centerBias.x = -0.5f;
                }
            }

            if (m_center.z == (int)m_center.z)
            {
                random = Random.Range(0, 2);
                if (random == 0)
                {
                    m_centerBias.z = 0.5f;
                }
                else
                {
                    m_centerBias.z = -0.5f;
                }
            }
        }

        private void UpdateBorder()
        {
            m_minBorder = new Vector3(m_rect.MinBorder.x, 0, m_rect.MinBorder.z);
            m_maxBorder = new Vector3(m_rect.MaxBorder.x, 0, m_rect.MaxBorder.z);
        }

        private void UpdateCorners()
        {
            if(m_cornerPositions == null)
            {
                m_cornerPositions = new List<Vector3>();
            }

            m_cornerPositions.Add(new Vector3(m_minBorder.x, 0, m_minBorder.z));
            m_cornerPositions.Add(new Vector3(m_maxBorder.x, 0, m_minBorder.z));
            m_cornerPositions.Add(new Vector3(m_minBorder.x, 0, m_maxBorder.z));
            m_cornerPositions.Add(new Vector3(m_maxBorder.x, 0, m_maxBorder.z));
        }

        public bool InBoundary(Vector3 point)
        {
            if (point.x >= m_minBorder.x &&
                point.x <= m_maxBorder.x &&
                point.z >= m_minBorder.z &&
                point.z <= m_maxBorder.z)
            {
                return true;
            }

            return false;
        }

        public void AddConnectedRoad(Corridor road)
        {
            if(m_cullingWalls == null)
            {
                m_cullingWalls = new List<Wall>();
            }

            Wall cacheWall = null;
            if (InBoundary(road.Start))
            {
                cacheWall = new Wall((int)road.Start.x, (int)road.Start.z, !road.IsVertical);
                if (!m_cullingWalls.Contains(cacheWall))
                {
                    m_cullingWalls.Add(cacheWall);
                }
            }
            
            if(InBoundary(road.End))
            {
                cacheWall = new Wall((int)road.End.x, (int)road.End.z, !road.IsVertical);
                if (!m_cullingWalls.Contains(cacheWall))
                {
                    m_cullingWalls.Add(cacheWall);
                }
            }
        }

        public bool IsCullingWall(Wall wall)
        {
            if(m_cullingWalls == null)
            {
                return false;
            }

            Wall cacheWall = null;
            for(int i = 0; i < m_cullingWalls.Count; i++)
            {
                cacheWall = m_cullingWalls[i];
                if(cacheWall.X != wall.X || cacheWall.Z != wall.Z || cacheWall.IsVertical != wall.IsVertical)
                {
                    continue;
                }

                return true;
            }

            return false;
        }
    }
}