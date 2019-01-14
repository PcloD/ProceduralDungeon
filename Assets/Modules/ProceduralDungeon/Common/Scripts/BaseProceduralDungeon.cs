using UnityEngine;

namespace Dungeon
{
    public abstract class BaseProceduralDungeon : MonoBehaviour
    {
        public Transform Parent { get { return m_parent; } }
        protected Transform m_parent;

        [Header("Common Settings")]
        [SerializeField] protected IntVector2 m_mapSize;
        [SerializeField] protected int m_gridSize = 1;
        [SerializeField] protected bool m_generateObjects;
        [SerializeField] private ObjectReferences m_objectRefs;

        protected ProceduralDungeonObjectGenerator m_objectGenerator;

        public void Destroy()
        {
            if (m_objectGenerator != null)
            {
                m_objectGenerator.Destroy();
                m_objectGenerator = null;
            }

            if (m_parent != null)
            {
                GameObject.Destroy(m_parent.gameObject);
            }
        }

        [InspectorMethod]
        public void Generate()
        {
            Destroy();
            m_parent = new GameObject("Procedural Dungeon").transform;
            StartGenerate();
        }

        protected abstract void StartGenerate();

        protected void GenerateObjects(Room[] rooms, Corridor[] corridors)
        {
            if (!m_generateObjects)
            {
                return;
            }

            m_objectGenerator = new ProceduralDungeonObjectGenerator(m_parent, m_mapSize, m_gridSize, m_objectRefs);
            m_objectGenerator.Generate(rooms, corridors);
        }

        public abstract Vector3 GetRandomPosition();
    }
}
