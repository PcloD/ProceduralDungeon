
namespace ProceduralDungeon
{
    public class Wall
    {
        public int X;
        public int Z;
        public bool IsVertical;

        public Wall(int x, int z, bool isVertical)
        {
            X = x;
            Z = z;
            IsVertical = isVertical;
        }
    }
}