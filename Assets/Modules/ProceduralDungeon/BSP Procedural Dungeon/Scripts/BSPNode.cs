using UnityEngine;
using System.Collections.Generic;
using ProceduralDungeon;

namespace BinarySpacePartitioning
{
    public class BSPNode
    {
        private const float MAX_RATIO = 0.45f;

        public IntRect Rect { get { return m_rect; } }
        public Room Room { get { return m_room; } }
        public IntRect ValidRect { get { return m_validRect; } }
        public Corridor Corridor { get { return m_corridor; } }

        private int m_level;
        private IntRect m_rect;
        private BSPNode m_parentNode;
        private IntVector2 m_minBSPSize;
        private Vector2 m_minRoomSizeRatio;

        private int m_splitDice; //0: Vertical Split, 1: Horizontal Split
        private List<BSPNode> m_leafNodes;
        private Room m_room;
        private IntRect m_validRect;
        private Corridor m_corridor;

        public BSPNode(int level, IntRect rect, BSPNode parentNode, IntVector2 minBSPSize, Vector2 minRoomSizeRatio)
        {
            m_level = level;
            m_rect = rect;
            m_parentNode = parentNode;
            m_minBSPSize = minBSPSize;
            m_minRoomSizeRatio = minRoomSizeRatio;
            m_leafNodes = new List<BSPNode>();
        }

        public void Split()
        {
            if(m_leafNodes.Count == 0)
            {
                SplitCurrentNode();
            }
            else
            {
                SplitLeafNode();
            }
        }

        private void SplitCurrentNode()
        {
            if(!CanVerticalSplit() && !CanHorizontalSplit())
            {
                return;
            }

            IntRect[] splitRects = GetSplitRects();
            m_leafNodes.Add(new BSPNode(m_level + 1, splitRects[0], this, m_minBSPSize, m_minRoomSizeRatio));
            m_leafNodes.Add(new BSPNode(m_level + 1, splitRects[1], this, m_minBSPSize, m_minRoomSizeRatio));
        }

        private bool CanVerticalSplit()
        {
            if(m_rect.width < m_minBSPSize.x * 2)
            {
                return false;
            }

            if((float)m_rect.width / m_rect.height < MAX_RATIO)
            {
                return false;
            }

            return true;
        }

        private bool CanHorizontalSplit()
        {
            if (m_rect.height < m_minBSPSize.z * 2)
            {
                return false;
            }

            if ((float)m_rect.height / m_rect.width < MAX_RATIO)
            {
                return false;
            }

            return true;
        }

        private IntRect[] GetSplitRects()
        {
            IntRect[] splitRects = new IntRect[2];

            if (CanVerticalSplit() && CanHorizontalSplit())
            {
                m_splitDice = Random.Range(0, 2);
            }
            else if(CanVerticalSplit())
            {
                m_splitDice = 0;
            }
            else if(CanHorizontalSplit())
            {
                m_splitDice = 1;
            }

            if (m_splitDice == 0)
            {
                int randomWidth = (int)Random.Range(m_rect.width * 0.3f, m_rect.width * 0.7f);
                if(randomWidth < m_minBSPSize.x || m_rect.width - randomWidth < m_minBSPSize.x)
                {
                    return GetSplitRects();
                }

                splitRects[0] = new IntRect(m_rect.x, m_rect.y, randomWidth, m_rect.height);
                splitRects[1] = new IntRect(m_rect.x + randomWidth, m_rect.y, m_rect.width - randomWidth, m_rect.height);
            }
            else
            {
                int randomHeight = (int)Random.Range(m_rect.height * 0.3f, m_rect.height * 0.7f);
                if (randomHeight < m_minBSPSize.z || m_rect.height - randomHeight < m_minBSPSize.z)
                {
                    return GetSplitRects();
                }

                splitRects[0] = new IntRect(m_rect.x, m_rect.y, m_rect.width, randomHeight);
                splitRects[1] = new IntRect(m_rect.x, m_rect.y + randomHeight, m_rect.width, m_rect.height - randomHeight);
            }

            return splitRects;
        }

        private void SplitLeafNode()
        {
            for(int i = 0; i < m_leafNodes.Count; i++)
            {
                m_leafNodes[i].Split();
            }
        }

        public void GetLeafNodes(ref List<BSPNode> leafNodes)
        {
            if(!IsLeaf())
            {
                for (int i = 0; i < m_leafNodes.Count; i++)
                {
                    m_leafNodes[i].GetLeafNodes(ref leafNodes);
                }
                return;
            }

            leafNodes.Add(this);
        }

        private bool IsLeaf()
        {
            return m_leafNodes.Count == 0;
        }

        public void GenerateRoomRect()
        {
            int randomWidth = (int)Random.Range(m_minRoomSizeRatio.x * m_rect.width, m_rect.width);
            int randomHeight = (int)Random.Range(m_minRoomSizeRatio.y * m_rect.height, m_rect.height);
            int randomX = Random.Range(0, m_rect.width - randomWidth);
            int randomY = Random.Range(0, m_rect.height - randomHeight);

            IntRect roomRect = new IntRect(m_rect.x + randomX, m_rect.y + randomY, randomWidth, randomHeight);
            m_room = new Room(roomRect);
            m_validRect = roomRect;
        }

        public void GenerateCorridorRect()
        {
            if(!IsLeaf())
            {
                m_corridor = GetRandomCorridor();
                m_validRect = GetValidRect();
            }
        }

        private Corridor GetRandomCorridor()
        {
            int randomValue = GetRandomCorridorRange();
            IntRect corridorRect;
            float middleValue;
            IntRect minRect = new IntRect();
            IntRect maxRect = new IntRect();

            if(m_splitDice == 0)
            {
                middleValue = 0;
                for(int i = 0; i < m_leafNodes.Count; i++)
                {
                    middleValue += m_leafNodes[i].ValidRect.center.x;
                }
                middleValue /= m_leafNodes.Count;

                for (int i = 0; i < m_leafNodes.Count; i++)
                {
                    if(m_leafNodes[i].ValidRect.center.x < middleValue)
                    {
                        minRect = m_leafNodes[i].GetRectByY(randomValue, true);
                    }
                    else
                    {
                        maxRect = m_leafNodes[i].GetRectByY(randomValue, false);
                    }
                }

                corridorRect = new IntRect(minRect.x + minRect.width, randomValue, maxRect.x - minRect.x - minRect.width, 1);
            }
            else
            {
                middleValue = 0;
                for (int i = 0; i < m_leafNodes.Count; i++)
                {
                    middleValue += m_leafNodes[i].ValidRect.center.y;
                }
                middleValue /= m_leafNodes.Count;

                for (int i = 0; i < m_leafNodes.Count; i++)
                {
                    if (m_leafNodes[i].ValidRect.center.y < middleValue)
                    {
                        minRect = m_leafNodes[i].GetRectByX(randomValue, true);
                    }
                    else
                    {
                        maxRect = m_leafNodes[i].GetRectByX(randomValue, false);
                    }
                }

                corridorRect = new IntRect(randomValue, minRect.y + minRect.height, 1, maxRect.y - minRect.y - minRect.height);
            }

            return new Corridor(corridorRect, m_splitDice != 0);
        }

        private int GetRandomCorridorRange()
        {
            IntRect validCorridorRange = GetValidCorridorRange();

            int randomValue;

            if (m_splitDice == 0)
            {
                randomValue = Random.Range(validCorridorRange.y, validCorridorRange.y + validCorridorRange.height);
            }
            else
            {
                randomValue = Random.Range(validCorridorRange.x, validCorridorRange.x + validCorridorRange.width);
            }

            return randomValue;
        }

        private IntRect GetValidCorridorRange()
        {
            IntRect validRect = m_leafNodes[0].ValidRect;
            IntRect cacheRect;
            BSPNode cacheNode;

            if (m_splitDice == 0)
            {
                for (int i = 1; i < m_leafNodes.Count; i++)
                {
                    cacheNode = m_leafNodes[i];
                    cacheRect = validRect;

                    if (cacheNode.ValidRect.y > validRect.y)
                    {
                        cacheRect.y = cacheNode.ValidRect.y;
                        cacheRect.height -= cacheNode.ValidRect.y - validRect.y;
                    }

                    if (cacheNode.ValidRect.y + cacheNode.ValidRect.height < validRect.y + validRect.height)
                    {
                        cacheRect.height -= validRect.y + validRect.height - cacheNode.ValidRect.y - cacheNode.ValidRect.height;
                    }

                    validRect = cacheRect;
                }
            }
            else
            {
                for (int i = 1; i < m_leafNodes.Count; i++)
                {
                    cacheNode = m_leafNodes[i];
                    cacheRect = validRect;

                    if (cacheNode.ValidRect.x > validRect.x)
                    {
                        cacheRect.x = cacheNode.ValidRect.x;
                        cacheRect.width -= cacheNode.ValidRect.x - validRect.x;
                    }

                    if (cacheNode.ValidRect.x + cacheNode.ValidRect.width < validRect.x + validRect.width)
                    {
                        cacheRect.width -= validRect.x + validRect.width - cacheNode.ValidRect.x - cacheNode.ValidRect.width;
                    }

                    validRect = cacheRect;
                }
            }

            return validRect;
        }

        private IntRect GetRectByY(int y, bool isLeft)
        {
            List<IntRect> validRects = new List<IntRect>();
            GetRectsByY(ref validRects, y);
            IntRect validRect = validRects[0];

            if(validRects.Count > 1)
            {
                if(isLeft)
                {
                    for(int i = 1; i < validRects.Count; i++)
                    {
                        if(validRects[i].x + validRects[i].width > validRect.x + validRect.width)
                        {
                            validRect = validRects[i];
                        }
                    }
                }
                else
                {
                    for (int i = 1; i < validRects.Count; i++)
                    {
                        if (validRects[i].x + validRects[i].width < validRect.x + validRect.width)
                        {
                            validRect = validRects[i];
                        }
                    }
                }
            }

            return validRect;
        }

        private void GetRectsByY(ref List<IntRect> rects, int y)
        {
            if (m_room != null && m_room.Rect.InBoundaryY(y))
            {
                rects.Add(m_room.Rect);
            }

            if (m_corridor != null && m_corridor.Rect.InBoundaryY(y))
            {
                rects.Add(m_corridor.Rect);
            }

            for (int i = 0; i < m_leafNodes.Count; i++)
            {
                m_leafNodes[i].GetRectsByY(ref rects, y);
            }
        }

        private IntRect GetRectByX(int x, bool isBottom)
        {
            List<IntRect> validRects = new List<IntRect>();
            GetRectsByX(ref validRects, x);
            IntRect validRect = validRects[0];

            if (validRects.Count > 1)
            {
                if (isBottom)
                {
                    for (int i = 1; i < validRects.Count; i++)
                    {
                        if (validRects[i].y + validRects[i].height > validRect.y + validRect.height)
                        {
                            validRect = validRects[i];
                        }
                    }
                }
                else
                {
                    for (int i = 1; i < validRects.Count; i++)
                    {
                        if (validRects[i].y + validRects[i].height < validRect.y + validRect.height)
                        {
                            validRect = validRects[i];
                        }
                    }
                }
            }

            return validRect;
        }

        private void GetRectsByX(ref List<IntRect> rects, int x)
        {
            if (m_room != null && m_room.Rect.InBoundaryX(x))
            {
                rects.Add(m_room.Rect);
            }

            if (m_corridor != null && m_corridor.Rect.InBoundaryX(x))
            {
                rects.Add(m_corridor.Rect);
            }

            for (int i = 0; i < m_leafNodes.Count; i++)
            {
                m_leafNodes[i].GetRectsByX(ref rects, x);
            }
        }

        private IntRect GetValidRect()
        {
            IntRect validRect = m_leafNodes[0].ValidRect;
            IntRect cacheRect;
            BSPNode cacheNode;

            for (int i = 1; i < m_leafNodes.Count; i++)
            {
                cacheNode = m_leafNodes[i];
                cacheRect = validRect;

                if (cacheNode.ValidRect.x < validRect.x)
                {
                    cacheRect.x = cacheNode.ValidRect.x;
                    cacheRect.width = validRect.x + validRect.width - cacheNode.ValidRect.x;
                }

                if(cacheNode.ValidRect.x > validRect.x)
                {
                    cacheRect.width = cacheNode.ValidRect.x + cacheNode.ValidRect.width - validRect.x;
                }

                if(cacheNode.ValidRect.y < validRect.y)
                {
                    cacheRect.y = cacheNode.ValidRect.y;
                    cacheRect.height = validRect.y + validRect.height - cacheNode.ValidRect.y;
                }

                if (cacheNode.ValidRect.y > validRect.y)
                {
                    cacheRect.height = cacheNode.ValidRect.y + cacheNode.ValidRect.height - validRect.y;
                }

                validRect = cacheRect;
            }

            return validRect;
        }

        public void GetNodesByLevel(ref List<BSPNode> nodes, int level)
        {
            if (m_level > level)
            {
                return;
            }

            if (m_level == level)
            {
                nodes.Add(this);
            }
            else
            {
                for (int i = 0; i < m_leafNodes.Count; i++)
                {
                    m_leafNodes[i].GetNodesByLevel(ref nodes, level);
                }
            }
        }

        public void GetParentNodes(ref List<BSPNode> parentNodes)
        {
            parentNodes.Add(this);

            if(!IsLeaf())
            {
                for (int i = 0; i < m_leafNodes.Count; i++)
                {
                    m_leafNodes[i].GetParentNodes(ref parentNodes);
                }
            }
        }
    }
}