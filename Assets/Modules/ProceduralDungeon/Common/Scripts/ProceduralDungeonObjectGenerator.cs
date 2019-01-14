using UnityEngine;
using System.Collections.Generic;

namespace Dungeon
{
    public class ProceduralDungeonObjectGenerator
    {
        private Transform m_parent;
        private IntVector2 m_mapSize;
        private int m_gridSize;
        private ObjectReferences m_objectReferences;

        private RoomObjectGenerator m_roomObjectGenerator;
        private CorridorObjectGenerator m_corridorObjectGenerator;
        private PillarObjectGenerator m_pillarObjectGenerator;

        public ProceduralDungeonObjectGenerator(Transform parent, IntVector2 mapSize, int gridSize, ObjectReferences objectReferences)
        {
            m_parent = parent;
            m_mapSize = mapSize;
            m_gridSize = gridSize;
            m_objectReferences = objectReferences;
        }

        public void Destroy()
        {
            if (m_roomObjectGenerator != null)
            {
                m_roomObjectGenerator.Destroy();
                m_roomObjectGenerator = null;
            }

            if (m_corridorObjectGenerator != null)
            {
                m_corridorObjectGenerator.Destroy();
                m_corridorObjectGenerator = null;
            }

            if (m_pillarObjectGenerator != null)
            {
                m_pillarObjectGenerator.Destroy();
                m_pillarObjectGenerator = null;
            }
        }

        public void Generate(Room[] rooms, Corridor[] corridors)
        {
            Destroy();

            ConnectedCorridorService service = new ConnectedCorridorService(rooms, corridors);

            m_roomObjectGenerator = new RoomObjectGenerator(service.Rooms, m_objectReferences.RoomGroundRefs, m_objectReferences.RoomWallRefs);
            m_roomObjectGenerator.Parent.localScale = Vector3.one * m_gridSize;
            m_roomObjectGenerator.Parent.SetParent(m_parent);

            m_corridorObjectGenerator = new CorridorObjectGenerator(service.Roads, m_objectReferences.RoadGroundRefs, m_objectReferences.RoadWallRefs);
            m_corridorObjectGenerator.Parent.localScale = Vector3.one * m_gridSize;
            m_corridorObjectGenerator.Parent.SetParent(m_parent);

            List<Wall> walls = new List<Wall>();
            walls.AddRange(m_roomObjectGenerator.WallList);
            walls.AddRange(m_corridorObjectGenerator.WallList);
            m_pillarObjectGenerator = new PillarObjectGenerator(m_mapSize, walls.ToArray(), m_objectReferences.PillarRefs);
            m_pillarObjectGenerator.Parent.localScale = Vector3.one * m_gridSize;
            m_pillarObjectGenerator.Parent.SetParent(m_parent);

            service = null;
        }
    }
}