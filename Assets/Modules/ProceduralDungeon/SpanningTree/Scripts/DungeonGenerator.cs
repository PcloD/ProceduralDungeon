using UnityEngine;
using SpanningTree;

namespace ProceduralDungeon.SpanningTree
{
    public class DungeonGenerator : MonoBehaviour
    {
        [SerializeField] private Dungeon.SpanningTreeType m_spanningTreeType;
        [SerializeField] private int m_width = 10;
        [SerializeField] private int m_height = 10;
        [SerializeField] private int m_extraCullingCoefficient;
        [SerializeField] private int m_accumulationCoefficient;
        [SerializeField] private int m_erosionCoefficient;
        [SerializeField] private bool m_playAnimation = true;

        private Dungeon m_dungeon;
        private BaseSpanningTree m_spanningTree;

        [InspectorMethod]
        public void Initialize()
        {
            if(m_dungeon == null)
            {
                m_dungeon = new Dungeon();
            }
            m_dungeon.Initialize(transform, m_playAnimation, m_spanningTreeType, m_width, m_height, m_extraCullingCoefficient, m_accumulationCoefficient, m_erosionCoefficient);
        }
    }
}