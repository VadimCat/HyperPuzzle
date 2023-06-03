using System.Collections.Generic;
using UnityEngine;

namespace Client.PuzzleCutMasksGenerator
{
    /// <summary>
    ///x = Ox + r * cos(a)
    ///y = Oy + r * sin(a)
    /// Oy - center
    ///
    /// x^2 + (ay)^2 = r^2
    /// r is "half width of ellipse"
    /// height =  r / a
    /// </summary>
    public struct EllipseGraph
    {
        public readonly int Side;
        
        public readonly List<Vector2Int> points;

        public EllipseGraph(Vector2Int center, int r, float hModifier, float vModifier, int side)
        {
            points = new List<Vector2Int>(360);

            for (int i = 0; i < 360; i++)
            {
                points.Add(new Vector2Int((int)(r * Mathf.Cos(Mathf.Deg2Rad * i) * hModifier),
                    (int)(r * Mathf.Sin(Mathf.Deg2Rad * i) * vModifier))+ center);
            }
            Side = side;
        }
    }
}