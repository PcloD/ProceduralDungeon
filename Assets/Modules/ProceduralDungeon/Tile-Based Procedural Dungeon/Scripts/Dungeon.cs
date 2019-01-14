using UnityEngine;
using System.Collections.Generic;
using SpanningTree;

namespace Dungeon.SpanningTree
{
    public class Dungeon
    {
        public enum SpanningTreeType
        {
            Minimum,
            DepthFirst,
            BreathFirst,
        }

        public BaseSpanningTree SpanningTree { get { return m_spanningTree; } }

        private Transform m_root;
        private GameObject m_groundObj;
        private GameObject m_wallObj;
        private SpanningTreeType m_spanningTreeType;
        private int m_width;
        private int m_height;
        private int m_extraCullingCoefficient;
        private int m_accumulationCoefficient;
        private int m_erosionCoefficient;
        private Transform m_groundAsset;
        private Transform m_cellParent;
        private Cell[,] m_groundCells;
        private Cell[,] m_cells;
        private List<STEdge> m_edges;
        private BaseSpanningTree m_spanningTree;

        public void Initialize(Transform root, GameObject groundObj, GameObject wallObj, SpanningTreeType spanningTreeType, int width, int height, int extraCullingCoefficient, int accumulationCoefficient, int erosionCoefficient)
        {
            m_root = root;
            m_groundObj = groundObj;
            m_wallObj = wallObj;
            m_spanningTreeType = spanningTreeType;
            m_width = width;
            m_height = height;
            m_extraCullingCoefficient = extraCullingCoefficient;
            m_accumulationCoefficient = accumulationCoefficient;
            m_erosionCoefficient = erosionCoefficient;
            
            DestroyCells();
            SetupParentAssets();
            GenerateCells();
            UpdateNeighborCells();
            GenerateEdges();
            GenerateSpanningTree();
            ApplySpanningTree();
            ApplyExtraCulling();
            ApplyAccumulation();
            ApplyErosion();
        }

        private void DestroyCells()
        {
            if(m_cellParent != null)
            {
                GameObject.Destroy(m_cellParent.gameObject);
                m_cellParent = null;
            }
        }

        private void SetupParentAssets()
        {
            if (m_groundAsset == null)
            {
                m_groundAsset = GameObject.Instantiate(m_groundObj).transform;
                m_groundAsset.gameObject.SetActive(true);
                m_groundAsset.name = "Ground";
                m_groundAsset.SetParent(m_root);
            }

            m_groundAsset.localPosition = new Vector3((float)(m_width - 1) / 2, 0, (float)(m_height - 1) / 2);
            m_groundAsset.localScale = new Vector3(m_width, 1, m_height);

            if (m_cellParent == null)
            {
                m_cellParent = new GameObject("Cell Parent").transform;
                m_cellParent.SetParent(m_root);
                m_cellParent.localPosition = Vector3.zero;
            }
        }

        private void GenerateCells()
        {
            m_cells = new Cell[m_width, m_height];
            for (int w = 0; w < m_width; w++)
            {
                for (int h = 0; h < m_height; h++)
                {
                    m_cells[w, h] = new Cell(m_cellParent, m_wallObj, w, h, w % 2 == 1 || h % 2 == 1);
                }
            }
        }

        private void UpdateNeighborCells()
        {
            for (int w = 0; w < m_width; w++)
            {
                for (int h = 0; h < m_height; h++)
                {
                    if(w + 1 < m_width)
                    {
                        m_cells[w, h].SetNeighborCell(Cell.NeighborType.East, m_cells[w + 1, h]);
                    }

                    if (w - 1 > 0)
                    {
                        m_cells[w, h].SetNeighborCell(Cell.NeighborType.West, m_cells[w - 1, h]);
                    }

                    if (h - 1 > 0)
                    {
                        m_cells[w, h].SetNeighborCell(Cell.NeighborType.South, m_cells[w, h - 1]);
                    }

                    if (h + 1 < m_height)
                    {
                        m_cells[w, h].SetNeighborCell(Cell.NeighborType.North, m_cells[w, h + 1]);
                    }
                }
            }
        }

        private void GenerateEdges()
        {
            int cacheValue1;
            int cacheValue2;
            m_edges = new List<STEdge>();
            STEdge cacheEdge = new STEdge();
            for (int h = 0; h < m_height; h++)
            {
                cacheValue1 = 0;
                cacheValue2 = 1;

                while (!IsOutOfBoundary(cacheValue1, h) && m_cells[cacheValue1, h].IsWall)
                {
                    cacheValue1++;
                    cacheValue2 = cacheValue1 + 1;
                }

                while (cacheValue2 < m_width)
                {
                    if (m_cells[cacheValue2, h].IsWall)
                    {
                        cacheValue2++;
                        continue;
                    }

                    cacheEdge = new STEdge();
                    cacheEdge.PointA = m_cells[cacheValue1, h].Center;
                    cacheEdge.PointB = m_cells[cacheValue2, h].Center;
                    m_edges.Add(cacheEdge);

                    cacheValue1 = cacheValue2;
                    cacheValue2 = cacheValue1 + 1;
                }
            }

            for (int w = 0; w < m_width; w++)
            {
                cacheValue1 = 0;
                cacheValue2 = 1;

                while (!IsOutOfBoundary(w, cacheValue1) && m_cells[0, cacheValue1].IsWall)
                {
                    cacheValue1++;
                    cacheValue2 = cacheValue1 + 1;
                }

                while (cacheValue2 < m_height)
                {
                    if (m_cells[w, cacheValue2].IsWall)
                    {
                        cacheValue2++;
                        continue;
                    }

                    cacheEdge = new STEdge();
                    cacheEdge.PointA = m_cells[w, cacheValue1].Center;
                    cacheEdge.PointB = m_cells[w, cacheValue2].Center;
                    m_edges.Add(cacheEdge);

                    cacheValue1 = cacheValue2;
                    cacheValue2 = cacheValue1 + 1;
                }
            }
        }

        private void GenerateSpanningTree()
        {
            switch (m_spanningTreeType)
            {
                case SpanningTreeType.Minimum:
                    m_spanningTree = new MinimumSpanningTree(m_edges.ToArray());
                    break;
                case SpanningTreeType.DepthFirst:
                    m_spanningTree = new DepthFirstSpanningTree(m_edges.ToArray());
                    break;
                case SpanningTreeType.BreathFirst:
                    m_spanningTree = new BreathFirstSpanningTree(m_edges.ToArray());
                    break;
            }
        }

        private void ApplySpanningTree()
        {
            Vector3 cacheCenter;
            Cell cacheCell;

            for(int i = 0; i < m_spanningTree.Segments.Count; i++)
            {
                cacheCenter = m_spanningTree.Segments[i].GetCenter();
                cacheCell = m_cells[(int)cacheCenter.x, (int)cacheCenter.z];
                cacheCell.Destroy();
            }
        }

        private void ApplyExtraCulling()
        {
            if(m_extraCullingCoefficient == 0)
            {
                return;
            }

            int value = m_extraCullingCoefficient;
            Cell cacheCell;

            while (value > 0)
            {
                cacheCell = m_cells[Random.Range(0, m_width), Random.Range(0, m_height)];
                if(cacheCell.Root != null)
                {
                    value--;
                    cacheCell.Destroy();
                }
            }
        }

        private void ApplyAccumulation()
        {
            int value = m_accumulationCoefficient;
            List<Cell> accumulationCells = new List<Cell>();
            int validCount;
            int cacheW;
            int cacheH;

            while (value > 0)
            {
                accumulationCells.Clear();
                for (int w = 0; w < m_width; w++)
                {
                    for(int h = 0; h < m_height; h++)
                    {
                        validCount = GetRoadCount(w, h);
                        if(validCount == 1)
                        {
                            accumulationCells.Add(m_cells[w, h]);
                        }
                    }
                }

                for(int i = 0; i < accumulationCells.Count; i++)
                {
                    cacheW = (int)accumulationCells[i].Center.x;
                    cacheH = (int)accumulationCells[i].Center.z;

                    m_cells[cacheW, cacheH].Destroy();
                    m_cells[cacheW, cacheH] = new Cell(m_cellParent, m_wallObj, cacheW, cacheH, true);
                }

                value--;
            }
        }

        private int GetRoadCount(int w, int h)
        {
            int count = 0;

            if (w - 1 >= 0 && !m_cells[w - 1, h].IsWall)
            {
                count++;
            }

            if (w + 1 < m_width && !m_cells[w + 1, h].IsWall)
            {
                count++;
            }

            if (h - 1 >= 0 && !m_cells[w, h - 1].IsWall)
            {
                count++;
            }

            if (h + 1 < m_height && !m_cells[w, h + 1].IsWall)
            {
                count++;
            }

            return count;
        }

        private void ApplyErosion()
        {
            int value = m_erosionCoefficient;
            List<Cell> cellList = new List<Cell>();

            cellList.Clear();
            for (int w = 0; w < m_width; w++)
            {
                for (int h = 0; h < m_height; h++)
                {
                    cellList.Add(m_cells[w, h]);
                }
            }

            cellList.Sort(delegate (Cell x, Cell y)
            {
                return y.GetNeighborRoadCount().CompareTo(x.GetNeighborRoadCount());
            });

            while (value > 0)
            {
                cellList.Sort(delegate (Cell x, Cell y)
                {
                    return y.GetNeighborRoadCount().CompareTo(x.GetNeighborRoadCount());
                });

                int roadCount = cellList[0].GetNeighborRoadCount();
                for(int i = 0; i < cellList.Count; i++)
                {
                    if(roadCount != cellList[i].GetNeighborRoadCount())
                    {
                        continue;
                    }

                    cellList[i].Destroy();
                }

                value--;
            }
        }

        private bool IsOutOfBoundary(int w, int h)
        {
            if(w < 0 || h < 0 ||
                w >= m_width || h >= m_height)
            {
                return true;
            }

            return false;
        }
    }
}