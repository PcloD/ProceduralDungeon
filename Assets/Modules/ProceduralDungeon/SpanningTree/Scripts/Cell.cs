using UnityEngine;

namespace ProceduralDungeon.SpanningTree
{
    public class Cell
    {
        public enum NeighborType
        {
            East,
            West,
            South,
            North,
        }

        private const string WALL_ASSET_NAME = "Cell_Wall";

        public Vector3 Center { get { return m_center; } }
        public bool IsWall { get { return m_isWall; } set { m_isWall = value; } }
        public GameObject Root { get { return m_root; } }

        private Vector3 m_center;
        private bool m_isWall;
        private GameObject m_root;
        private Cell[] m_neighborCells;

        public Cell(Transform parent, int w, int h, bool isWall)
        {
            m_center = new Vector3(w, 0, h);
            m_isWall = isWall;

            if(isWall)
            {
                m_root = GameObject.Instantiate(Resources.Load(WALL_ASSET_NAME) as GameObject);
                m_root.name = string.Format("Cell {0}, {1}, {2}", m_center.x, m_center.y, m_center.z);
                m_root.transform.SetParent(parent);
                m_root.transform.localPosition = m_center;
            }

            m_neighborCells = new Cell[4];
        }

        public void SetNeighborCell(NeighborType neighborType, Cell cell)
        {
            m_neighborCells[(int)neighborType] = cell;
        }

        public int GetNeighborRoadCount()
        {
            int count = 0;

            for(int i = 0; i < m_neighborCells.Length; i++)
            {
                if(m_neighborCells[i] == null)
                {
                    continue;
                }

                if(!m_neighborCells[i].IsWall)
                {
                    count++;
                }
            }

            return count;
        }

        public void Destroy()
        {
            m_isWall = false;

            if(m_root != null)
            {
                GameObject.Destroy(m_root);
            }
        }
    }
}