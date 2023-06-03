using System;
using System.Collections.Generic;
using System.Linq;
using Ji2.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Client.PuzzleCutMasksGenerator
{
    public class PuzzleCutter
    {
        private const float MIN_R = .135f;
        private const float MAX_R = .15f;

        private const float MIN_TONGUE_OFFSET_R = .45f;
        private const float MAX_TONGUE_OFFSET_R = .5f;

        private const int minSinA = 1;
        private const float maxSinPercentA = .07f;

        private const float minSinB = .33f * Mathf.PI;
        private Vector2Int baseCellSize;
        private bool[,] pointsState;
        
        public Dictionary<Vector2Int, Sprite> GetTiles(Vector2Int size, Vector2Int cutSize)
        {
            pointsState = new bool[size.x, size.y];

            baseCellSize = new Vector2Int(pointsState.GetLength(0) / cutSize.x, pointsState.GetLength(1) / cutSize.y);

            SinGraph[] verticalDividerLines = GenerateVerticalDividers(cutSize);
            SinGraph[] horizontalDividerLines = GenerateHorizontalDividers(cutSize);

            ProcessVerticalEllipses(cutSize, verticalDividerLines);
            ProcessHorizontalEllipses(cutSize, horizontalDividerLines);

            List<Vector2Int> truePoints = new List<Vector2Int>();

            for (int i = 0; i < pointsState.GetLength(0); i++)
            {
                for (int j = 0; j < pointsState.GetLength(1); j++)
                {
                    if (pointsState[i, j])
                    {
                        truePoints.Add(new Vector2Int(i, j));
                    }
                }
            }

            foreach (var point in truePoints)
            {
                foreach (var dir in Vector2IntUtils.GetVector2IntDirections())
                {
                    var pos = point + dir;
                    if(pos.x < 0 || pos.y < 0 || pos.x >= pointsState.GetLength(0) || pos.y >= pointsState.GetLength(1))
                        continue;

                    pointsState[pos.x, pos.y] = true;
                }
            }
            
            var sprites = new Dictionary<Vector2Int, Sprite>();
            for (int i = 0; i < cutSize.x; i++)
            for (int j = 0; j < cutSize.x; j++)
            {
                sprites[new Vector2Int(i, j)] = GetTile(new Vector2Int(i, j));
            }

            return sprites;
        }
        
        private Sprite GetTile(Vector2Int tilePos)
        {
            var cell = GetTilePixels(tilePos);

            Texture2D tile = new Texture2D((int)(baseCellSize.x * 1.5f), (int)(baseCellSize.y * 1.5f))
            {
                filterMode = FilterMode.Point
            };
            Vector2Int delta = new Vector2Int((int)((tilePos.x - .25f) * baseCellSize.x),
                (int)((tilePos.y - .25f) * baseCellSize.y));

            for (int i = 0; i < tile.width; i++)
            {
                for (int j = 0; j < tile.height; j++)
                {
                    tile.SetPixel(i, j, cell.Contains(new Vector2Int(i, j) + delta) ? Color.green : Color.clear);
                }
            }

            tile.Apply();

            return Sprite.Create(tile, new Rect(0, 0, tile.width, tile.height), Vector2.zero, 100);
        }
        
        private HashSet<Vector2Int> GetTilePixels(Vector2Int tile)
        {
            var startPoint = new Vector2Int(Mathf.RoundToInt((tile.x + .5f) * baseCellSize.x),
                Mathf.RoundToInt((tile.y + .5f) * baseCellSize.y));

            HashSet<Vector2Int> usedPoints = new HashSet<Vector2Int> { startPoint };
            Queue<Vector2Int> pointsToCheck = new Queue<Vector2Int>();
            pointsToCheck.Enqueue(startPoint);

            while (pointsToCheck.Count > 0)
            {
                var point = pointsToCheck.Dequeue();
                foreach (var direction in Vector2IntUtils.GetVector2IntDirections())
                {
                    var checkPoint = point + direction;
                    if (checkPoint.x >= 0 && checkPoint.x < pointsState.GetLength(0) &&
                        checkPoint.y >= 0 && checkPoint.y < pointsState.GetLength(1) &&
                        !pointsState[checkPoint.x, checkPoint.y] && usedPoints.Add(checkPoint))
                    {
                        pointsToCheck.Enqueue(checkPoint);
                    }
                }
            }

            return usedPoints;
        }
        
        private void ProcessVerticalEllipses(Vector2Int cutSIze, SinGraph[] verticalDividerLines)
        {
            EllipseGraph[] verticalEllipses = new EllipseGraph[cutSIze.y * (cutSIze.x - 1)];
            int pos = 0;
            for (int i = 0; i < cutSIze.x - 1; i++)
            {
                for (int j = 0; j < cutSIze.y; j++)
                {
                    int r = (int)(baseCellSize.y * Random.Range(MIN_R, MAX_R));
                    float vModifier = Random.Range(1, 1.2f);

                    float range = Random.Range(MIN_TONGUE_OFFSET_R, MAX_TONGUE_OFFSET_R);
                    int side = SideMultiplier();
                    Vector2Int offset = new Vector2Int((int)(r * side * range), 0);

                    Vector2Int center = new Vector2Int(baseCellSize.x * (i + 1), (int)(baseCellSize.y * (j + .5f))) +
                                        offset;

                    verticalEllipses[i * cutSIze.y + j] = new EllipseGraph(center, r, 1, vModifier, side);
                    pos++;
                }
            }

            int count = 0;
            for (int i = 0; i < cutSIze.x - 1; i++)
            {
                for (int j = 0; j < cutSIze.y; j++)
                {
                    var ellipse = verticalEllipses[count];
                    int minY = Int32.MaxValue;
                    int maxY = Int32.MinValue;

                    var line = verticalDividerLines[i];

                    foreach (var point in ellipse.points)
                    {
                        int side = point.x.CompareTo(line.GetPoint(point.y));

                        if (side == ellipse.Side)
                        {
                            pointsState[point.x, point.y] = true;
                        }
                        else
                        {
                            if (point.y < minY)
                            {
                                minY = point.y;
                            }
                            else if (point.y > maxY)
                            {
                                maxY = point.y;
                            }
                        }
                    }

                    for (int k = minY + 1; k < maxY; k++)
                    {
                        pointsState[line.GetPoint(k), k] = false;
                    }

                    count++;
                }
            }
        }

        private void ProcessHorizontalEllipses(Vector2Int cutSIze, SinGraph[] dividerLines)
        {
            EllipseGraph[] ellipses = new EllipseGraph[cutSIze.x * (cutSIze.y - 1)];
            int pos = 0;
            for (int i = 0; i < cutSIze.y - 1; i++)
            {
                for (int j = 0; j < cutSIze.x; j++)
                {
                    int r = (int)(baseCellSize.x * Random.Range(MIN_R, MAX_R));
                    float vModifier = Random.Range(1, 1.2f);

                    float range = Random.Range(MIN_TONGUE_OFFSET_R, MAX_TONGUE_OFFSET_R);
                    int side = SideMultiplier();
                    Vector2Int offset = new Vector2Int(0, (int)(r * side * range));

                    Vector2Int center = new Vector2Int((int)(baseCellSize.x * (j + .5f)), baseCellSize.y * (i + 1)) +
                                        offset;

                    ellipses[i * cutSIze.y + j] = new EllipseGraph(center, r, vModifier, 1, side);
                    pos++;
                }
            }

            int count = 0;
            for (int i = 0; i < cutSIze.y - 1; i++)
            {
                for (int j = 0; j < cutSIze.x; j++)
                {
                    var ellipse = ellipses[count];
                    int minX = Int32.MaxValue;
                    int maxX = Int32.MinValue;

                    var line = dividerLines[i];

                    foreach (var point in ellipse.points)
                    {
                        int side = point.y.CompareTo(line.GetPoint(point.x));

                        if (side == ellipse.Side)
                        {
                            pointsState[point.x, point.y] = true;
                        }
                        else
                        {
                            if (point.x < minX)
                            {
                                minX = point.x;
                            }
                            else if (point.x > maxX)
                            {
                                maxX = point.x;
                            }
                        }
                    }

                    for (int k = minX; k <= maxX; k++)
                    {
                        pointsState[k, line.GetPoint(k)] = false;
                    }

                    count++;
                }
            }
        }

        private SinGraph[] GenerateHorizontalDividers(Vector2Int cutSIze)
        {
            SinGraph[] horizontalDividerLines = new SinGraph[cutSIze.x - 1];

            for (int i = 0; i < horizontalDividerLines.Length; i++)
            {
                float a = (int)(Random.Range(0, maxSinPercentA) * baseCellSize.x);
                a = Mathf.Max(minSinA, a);

                float b = 1 / (pointsState.GetLength(1) / Random.Range(minSinB, maxSinB));

                float c = Random.Range(0, Mathf.PI);
                int offsetX = baseCellSize.x * (i + 1);
                horizontalDividerLines[i] = new SinGraph(a, b, c, offsetX);
            }

            foreach (var t in horizontalDividerLines)
                for (int x = 0; x < pointsState.GetLength(1); x++)
                {
                    int y = t.GetPoint(x);
                    pointsState[x, y] = true;
                }

            return horizontalDividerLines;
        }

        private SinGraph[] GenerateVerticalDividers(Vector2Int cutSIze)
        {
            SinGraph[] verticalDividerLines = new SinGraph[cutSIze.y - 1];

            for (int i = 0; i < verticalDividerLines.Length; i++)
            {
                float a = (int)(Random.Range(0, maxSinPercentA) * baseCellSize.y);
                a = Mathf.Max(minSinA, a);

                float b = 1 / (pointsState.GetLength(0) / Random.Range(minSinB, maxSinB));

                float c = Random.Range(0, Mathf.PI);
                int offsetY = baseCellSize.y * (i + 1);
                verticalDividerLines[i] = new SinGraph(a, b, c, offsetY);
            }

            foreach (var t in verticalDividerLines)
                for (int y = 0; y < pointsState.GetLength(1); y++)
                {
                    int x = t.GetPoint(y);
                    pointsState[x, y] = true;
                }

            return verticalDividerLines;
        }

        private const float maxSinB = 3 * Mathf.PI;

        private int SideMultiplier()
        {
            return Random.Range(0, 2) == 0 ? -1 : 1;
        }
    }
}