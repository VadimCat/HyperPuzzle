using UnityEngine;
using UnityEngine.UI;

namespace Client.PuzzleCutMasksGenerator
{
    public class PuzzleCutter : MonoBehaviour
    {
        [SerializeField] private Image img;
        [SerializeField] private SpriteRenderer spr;

        private const float minR = .25f;
        private const float maxR = .35f;

        private const float minTongueOffsetR = .5f;
        private const float maxOffsetR = 1.5f;

        private const int minSinA = 1;
        private const float maxSinPercentA = .07f;

        private const float minSinB = .33f * Mathf.PI;
        private const float maxSinB = 3 * Mathf.PI;

        private void Awake()
        {
            bool[,] points = new bool[800, 800];

            Vector2Int cutSIze = new Vector2Int(10, 10);
            Vector2Int baseCellSize = new Vector2Int(points.GetLength(0) / cutSIze.x, points.GetLength(1) / cutSIze.y);

            SinGraph[] verticalDividerLines = new SinGraph[cutSIze.y - 1];

            for (int i = 0; i < verticalDividerLines.Length; i++)
            {
                float a = (int)(Random.Range(0, maxSinPercentA) * baseCellSize.y);
                a = Mathf.Max(minSinA, a);

                float b = 1 / (points.GetLength(0) / Random.Range(minSinB, maxSinB));

                float c = Random.Range(0, Mathf.PI);
                int offsetY = baseCellSize.y * (i + 1);
                verticalDividerLines[i] = new SinGraph(a, b, c, offsetY);
            }

            for (int i = 0; i < points.GetLength(0); i++)
            {
                foreach (var t in verticalDividerLines)
                {
                    int y = t.GetPoint(i);
                    points[i, y] = true;
                }
            }

            SinGraph[] horizontalDividerLines = new SinGraph[cutSIze.x - 1];

            for (int i = 0; i < horizontalDividerLines.Length; i++)
            {
                float a = (int)(Random.Range(0, maxSinPercentA) * baseCellSize.x);
                a = Mathf.Max(minSinA, a);

                float b = 1 / (points.GetLength(1) / Random.Range(minSinB, maxSinB));

                float c = Random.Range(0, Mathf.PI);
                int offsetX = baseCellSize.x * (i + 1);
                horizontalDividerLines[i] = new SinGraph(a, b, c, offsetX);
            }

            for (int i = 0; i < points.GetLength(1); i++)
            {
                foreach (var t in horizontalDividerLines)
                {
                    int x = t.GetPoint(i);
                    points[x, i] = true;
                }
            }

            EllipseGraph[] verticalEllipses = new EllipseGraph[cutSIze.y * (cutSIze.x - 1)];

            for (int i = 0; i < cutSIze.x - 1; i++)
            {
                for (int j = 0; j < cutSIze.y; j++)
                {
                    Vector2Int center = new Vector2Int((baseCellSize.x + 1) * i, (int)(baseCellSize.y * (j + .5f)));
                    int r = (int)(baseCellSize.y * Random.Range(minR, maxR));
                    int a = (int)(r / Random.Range(.8f, 1.2f));
                    verticalEllipses[i * cutSIze.y + j] = new EllipseGraph(center, r, a);
                }
            }

            EllipseGraph[] horizontalEllipses = new EllipseGraph[cutSIze.x * (cutSIze.y - 1)];

            for (int i = 0; i < cutSIze.x; i++)
            {
                for (int j = 0; j < cutSIze.y - 1; j++)
                {
                    Vector2Int center = new Vector2Int((int)((baseCellSize.x + .5f) * i), baseCellSize.y * (j + 1));
                    int r = (int)(baseCellSize.y * Random.Range(minR, maxR));
                    int a = (int)(r / Random.Range(.8f, 1.2f));
                    horizontalEllipses[i * (cutSIze.y - 1) + j] = new EllipseGraph(center, r, a);
                }
            }

            Texture2D texture2D = new Texture2D(points.GetLength(0), points.GetLength(1));
            for (var i = 0; i < points.GetLength(0); i++)
            for (var j = 0; j < points.GetLength(1); j++)
            {
                var point = points[i, j];
                texture2D.SetPixel(i, j, point ? Color.black : Color.white);
            }

            foreach (var ellipse in verticalEllipses)
            {
                int p1 = ellipse.center.x - ellipse.r;
                int p2 = ellipse.center.x + ellipse.r;
                for (int i = p1; i < p2; i++)
                {
                    if (ellipse.TryGetIntersectionPoints(i, out var intersections))
                    {
                        foreach (var p in intersections)
                        {
                            Debug.LogError(p);
                            texture2D.SetPixel(p.x, p.y, Color.black);
                        }
                    }
                }
            }
            foreach (var ellipse in horizontalEllipses)
            {
                int p1 = ellipse.center.x - ellipse.r;
                int p2 = ellipse.center.x + ellipse.r;
                for (int i = p1; i < p2; i++)
                {
                    if (ellipse.TryGetIntersectionPoints(i, out var intersections))
                    {
                        foreach (var p in intersections)
                        {
                            Debug.LogError(p);
                            texture2D.SetPixel(p.x, p.y, Color.black);
                        }
                    }
                }
            }
            
            texture2D.Apply();

            var sprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), Vector2.zero, 100);
            img.sprite = sprite;
            spr.sprite = sprite;
        }

        private int GetSideMultiplier()
        {
            return Random.Range(0, 2) == 0 ? -1 : 1;
        }
    }

    /// f(x) = a sin(bx + c) + offsetY
    public class SinGraph
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
            return (int)(a * Mathf.Sin(b * x + c) + offsetY);
        }

        public override string ToString()
        {
            return $"f(x) = {a} sin({b}x)";
        }
    }

    /// <summary>
    ///x = Ox + r * cos(a)
    ///y = Oy + r * sin(a)
    /// Oy - center
    ///
    /// x^2 + (ay)^2 = r^2
    /// r is "half width of ellipse"
    /// height =  r / a
    /// </summary>
    public class EllipseGraph
    {
        public readonly Vector2Int center;
        public readonly int r;
        private readonly int a;

        public EllipseGraph(Vector2Int center, int r, int a)
        {
            this.center = center;
            this.r = r;
            this.a = a;
        }

        public bool TryGetIntersectionPoints(int x, out Vector2Int[] points)
        {
            var normX = x - center.x;
            if (normX == r)
            {
                points = new Vector2Int[1];
                points[0] = new Vector2Int(normX, 0) + center;
                return true;
            }
            else if (normX < r)
            {
                points = new Vector2Int[2];

                int y = (int)(Mathf.Sqrt(r * r - normX * normX) / a);

                Debug.LogError(y + center.y);
                Debug.LogError(-y + center.y);
                
                points[0] = new Vector2Int(normX, y) + center;
                points[1] = new Vector2Int(normX, -y) + center;

                return true;
            }

            points = new Vector2Int[1];
            return false;
        }
    }
}