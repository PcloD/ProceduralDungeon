
namespace ProceduralDungeon
{
    public class ConnectedCorridorService
    {
        public Room[] Rooms;
        public Road[] Roads;

        public ConnectedCorridorService(Room[] rooms, Road[] roads)
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