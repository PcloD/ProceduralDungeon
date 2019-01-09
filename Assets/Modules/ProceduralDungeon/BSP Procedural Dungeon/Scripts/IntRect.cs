using UnityEngine;

namespace ProceduralDungeon
{
    [System.Serializable]
    public struct IntRect
    {
        public int x;
        public int y;
        public int width;
        public int height;
        public IntVector2 minBorder;
        public IntVector2 maxBorder;
        public Vector2 center;

        public IntRect(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            minBorder = new IntVector2(x, y);
            maxBorder = new IntVector2(x + width, y + height);
            center = new Vector2(x + (float)width / 2, y + (float)height / 2);
        }

        public bool InBoundaryX(int x)
        {
            return x >= this.x && x < this.x + width;
        }

        public bool InBoundaryY(int y)
        {
            return y >= this.y && y < this.y + height;
        }
    }
}