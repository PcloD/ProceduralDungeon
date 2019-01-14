
namespace Dungeon
{
    public class ConnectedCorridorService
    {
        public Room[] Rooms;
        public Corridor[] Roads;

        public ConnectedCorridorService(Room[] rooms, Corridor[] roads)
        {
            Rooms = rooms;
            Roads = roads;

            Room cacheRoom = null;
            Corridor cacheRoad = null;
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