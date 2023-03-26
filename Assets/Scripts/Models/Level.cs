using System;
using System.Collections.Generic;
using Ji2.CommonCore.SaveDataContainer;
using Ji2.Models;
using Ji2.Models.Analytics;
using UnityEngine;
using Ji2.Utils;
using Random = UnityEngine.Random;

namespace Models
{
    public class Level : LevelBase<Level.Progress>
    {
        private Vector2Int cutSize;
        private readonly int partsPerSelection;
        private readonly int health;

        private List<Vector2Int> notAssembledParts;
        private List<Vector2Int> assembledParts;
        private List<Vector2Int> allParts;
        
        public Vector2Int CutSize => cutSize;

        public ReactiveProperty<int> Health => progress.Health;
        public bool[,] PartsState => progress.PartsState;

        private Progress progress;
        
        public event Action<Vector2Int, List<Vector2Int>> EventTurnStart;
        public event Action<Vector2Int, bool> EventPieceSelected;
        
        public Level(IAnalytics analytics, LevelData levelData, ISaveDataContainer saveDataContainer,
            Vector2Int cutSize, int partsPerSelection, int health) : base(analytics, levelData, saveDataContainer)
        {
            this.cutSize = cutSize;
            this.partsPerSelection = partsPerSelection;
            this.health = health;

            GeneratePartsArray();
        }

        public void StartTurn()
        {
            progress.Selection = new List<Vector2Int>(partsPerSelection);
            int part = Random.Range(0, partsPerSelection);

            if (progress.Queue.TryPeek(out var tilePos))
            {
                var partsForSelection = new List<Vector2Int>(allParts);
                partsForSelection.Remove(tilePos);

                for (int i = 0; i < partsPerSelection; i++)
                {
                    if (i == part)
                    {
                        progress.Selection.Add(tilePos);
                    }
                    else
                    {
                        var rnd = partsForSelection[Random.Range(0, partsForSelection.Count)];
                        partsForSelection.Remove(rnd);
                        progress.Selection.Add(rnd);
                    }
                }

                EventTurnStart?.Invoke(tilePos, progress.Selection);
            }
            else
            {
                Complete();
            }
            
            SaveProgress();
        }

        public void ProcessSelection(Vector2Int select)
        {
            if (select == progress.Queue.Peek())
            {
                progress.Queue.Dequeue();
                EventPieceSelected?.Invoke(select, true);
                StartTurn();
            }
            else
            {
                progress.Health.Value--;
                progress.Selection.Remove(select);
                if (progress.Health.Value == 0)
                {
                    Complete();
                    return;
                }
                EventPieceSelected?.Invoke(select, false);
                
                SaveProgress();
            }
        }

        private void GeneratePartsArray()
        {
            progress = new Progress
            {
                Health = new ReactiveProperty<int>(health),
                PartsState = new bool[CutSize.x, CutSize.y],
                Queue = new Queue<Vector2Int>()
            };

            allParts = new List<Vector2Int>(cutSize.x * cutSize.y);
            assembledParts = new List<Vector2Int>(cutSize.x * cutSize.y);
            notAssembledParts = new List<Vector2Int>(cutSize.x * cutSize.y);
            
            Vector2Int randomStartPoint = new Vector2Int(Random.Range(0, CutSize.x), Random.Range(0, CutSize.y));
            progress.PartsState[randomStartPoint.x, randomStartPoint.y] = true;

            allParts.Add(randomStartPoint);
            assembledParts.Add(randomStartPoint);
            
            PartState[,] partsMap = new PartState[CutSize.x, CutSize.y];
            partsMap[randomStartPoint.x, randomStartPoint.y] = PartState.Filled;
            HashSet<Vector2Int> filledPoints = new HashSet<Vector2Int> { randomStartPoint };
            List<Vector2Int> availablePoints = new List<Vector2Int>(CutSize.x * CutSize.y);

            for (int i = filledPoints.Count; i < CutSize.x * CutSize.y; i++)
            {
                availablePoints.Clear();

                foreach (var point in filledPoints)
                {
                    foreach (var direction in Vector2IntUtils.GetVector2IntDirections())
                    {
                        var pointToCheck = point + direction;

                        if (pointToCheck.x >= 0 && pointToCheck.x < CutSize.x && pointToCheck.y >= 0 &&
                            pointToCheck.y < CutSize.y && !filledPoints.Contains(pointToCheck))
                        {
                            availablePoints.Add(pointToCheck);
                        }
                    }
                }

                var queueItem = availablePoints[Random.Range(0, availablePoints.Count)];
                filledPoints.Add(queueItem);
                
                allParts.Add(queueItem);
                progress.Queue.Enqueue(queueItem);
                notAssembledParts.Add(queueItem);
            }
        }

        [Serializable]
        public class Progress : ProgressBase
        {
            public ReactiveProperty<int> Health;
            public float Time;
            public bool[,] PartsState;
            public Queue<Vector2Int> Queue;
            public List<Vector2Int> Selection;
        }
        
        public enum PartState
        {
            Blocked,
            Available,
            Filled
        }
    }
}