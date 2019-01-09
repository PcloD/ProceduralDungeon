
namespace ProceduralDungeon
{
    public class ProceduralDungeonConnectHelper
    {
        public Room[] Rooms;
        public Road[] Roads;

        public ProceduralDungeonConnectHelper(Room[] rooms, Road[] roads)
        {
            Rooms = rooms;
            Roads = roads;

            Room cacheRoom = null;
            Road cacheRoad = null;
            for (int i = 0; i < roads.Length; i++)
            {
                cacheRoad = roads[i];

                for (int j = 0; j < rooms.Length; j++)
                {
                    cacheRoom = rooms[j];
                    cacheRoom.AddConnectedRoad(cacheRoad);
                }
            }
        }
    }
}