using UnityEngine;

namespace ProceduralDungeon
{
    [CreateAssetMenu(menuName = "Procedural Dungeon/Object References")]
    public class ObjectReferences : ScriptableObject
    {
        public GameObject[] RoomGroundRefs;
        public GameObject[] RoomWallRefs;
        public GameObject[] RoadGroundRefs;
        public GameObject[] RoadWallRefs;
        public GameObject[] PillarRefs;
    }
}