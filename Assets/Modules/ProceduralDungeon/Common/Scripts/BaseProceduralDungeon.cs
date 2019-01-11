using UnityEngine;

namespace ProceduralDungeon
{
    public abstract class BaseProceduralDungeon : MonoBehaviour
    {
        public Transform Parent { get { return m_parent; } }
        protected Transform m_parent;

        private void OnDestroy()
        {
            Destroy();

            if(m_parent != null)
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

        public abstract void Destroy();
        protected abstract void StartGenerate();
        public abstract Vector3 GetRandomPosition();
    }
}
