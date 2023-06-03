using UnityEngine;

namespace Client.PuzzleCutMasksGenerator
{
    /// f(x) = a sin(bx + c) + offsetY
    public readonly struct SinGraph
    {
        private readonly float a;
        private readonly float b;
        private readonly float c;
        private readonly int offsetY;

        public SinGraph(float a, float b, float c, int offsetY)
        {
            this.a = a;
            this.b = b;
            this.c = c;
            this.offsetY = offsetY;
        }

        public int GetPoint(int x)
        {
            int y = (int)(a * Mathf.Sin(b * x + c) + offsetY);
            return y;
        }
    }
}