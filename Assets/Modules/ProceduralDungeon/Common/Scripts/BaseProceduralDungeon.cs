using UnityEngine;

namespace ProceduralDungeon
{
    public abstract class BaseProceduralDungeon : MonoBehaviour
    {
        private void OnDestroy()
        {
            Destroy();
        }

        [InspectorMethod]
        public void Generate()
        {
            Destroy();
            StartGenerate();
        }

        protected abstract void Destroy();
        protected abstract void StartGenerate();
        public abstract Vector3 GetRandomPosition();
    }
}
