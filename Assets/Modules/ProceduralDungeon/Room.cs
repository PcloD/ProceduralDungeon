using UnityEngine;

namespace ProceduralDungeon
{
    public class Room
    {
        public Vector3 Center { get { return m_center; } }
        public Vector3 CenterBias { get { return m_centerBias; } }
        public Vector3 Size { get { return m_size; } }
        public Vector3 MinBorder { get { return m_minBorder; } }
        public Vector3 MaxBorder { get { return m_maxBorder; } }
        public int Priority { get { return m_priority; } }

        private Vector3 m_center;
        private Vector3 m_centerBias;
        private Vector3 m_size;
        private Vector3 m_minBorder;
        private Vector3 m_maxBorder;
        private int m_priority;

        public Room(Vector3 center, Vector3 size)
        {
            m_center = center;
            m_centerBias = GetCenterBias(center);
            m_size = size;
            m_priority = (int)(m_size.x * m_size.z);

            UpdateBorder(center, size);
        }

        private Vector3 GetCenterBias(Vector3 center)
        {
            Vector3 bias = Vector3.zero;
            int random = 0;
            if (center.x == (int)center.x)
            {
                random = Random.Range(0, 2);
                if(random == 0)
                {
                    bias.x = 0.5f;
                }
                else
                {
                    bias.x = -0.5f;
                }
            }

            if (center.z == (int)center.z)
            {
                random = Random.Range(0, 2);
                if (random == 0)
                {
                    bias.z = 0.5f;
                }
                else
                {
                    bias.z = -0.5f;
                }
            }

            return bias;
        }

        private void UpdateBorder(Vector3 center, Vector3 size)
        {
            m_minBorder = Vector3.zero;
            m_maxBorder = Vector3.zero;

            m_minBorder.x = center.x - size.x / 2;
            m_maxBorder.x = center.x + size.x / 2;
            m_minBorder.z = center.z - size.z / 2;
            m_maxBorder.z = center.z + size.z / 2;
        }

        public bool InBoundary(Vector3 point)
        {
            if (point.x >= m_center.x - m_size.x / 2 &&
                point.x <= m_center.x + m_size.x / 2 &&
                point.z >= m_center.z - m_size.z / 2 &&
                point.z <= m_center.z + m_size.z / 2)
            {
                return true;
            }

            return false;
        }
    }
}