using UnityEngine;

namespace ProceduralDungeon
{
    [System.Serializable]
    public struct IntRect
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
        public IntVector2 MinBorder;
        public IntVector2 MaxBorder;
        public Vector2 Center;

        public IntRect(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            MinBorder = new IntVector2(x, y);
            MaxBorder = new IntVector2(x + width, y + height);
            Center = new Vector2(x + (float)width / 2, y + (float)height / 2);
        }

        public bool InBoundaryX(int x)
        {
            return x >= X && x < X + Width;
        }

        public bool InBoundaryY(int y)
        {
            return y >= Y && y < Y + Height;
        }
    }
}