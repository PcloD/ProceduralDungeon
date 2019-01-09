
namespace ProceduralDungeon
{
    public class Corridor
    {
        public IntRect Rect;
        public bool IsVertical;

        public Corridor(IntRect rect, bool isVertical)
        {
            Rect = rect;
            IsVertical = isVertical;
        }
    }
}