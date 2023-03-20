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
    public class Level : LevelBase
    {
        private Vector2Int cutSize;
        private readonly int partsPerSelection;
        private bool[,] partsState;
        private Queue<Vector2Int> partsQueue;

        private List<Vector2Int> notAssembledParts;
        private List<Vector2Int> assembledParts;
        private List<Vector2Int> allParts;
        
        public Vector2Int CutSize => cutSize;

        public bool[,] PartsState => partsState;

        public event Action<Vector2Int, Vector2Int[]> EventTurnStart;
        public event Action EventLevelCompleted;
        public event Action<Vector2Int, bool> EventPieceSelected;

        public Level(IAnalytics analytics, LevelData levelData, ISaveDataContainer saveDataContainer,
            Vector2Int cutSize, int partsPerSelection) : base(analytics, levelData, saveDataContainer)
        {
            this.cutSize = cutSize;
            this.partsPerSelection = partsPerSelection;

            GeneratePartsArray();
        }

        public void StartTurn()
        {
            Vector2Int[] selection = new Vector2Int[partsPerSelection];
            int part = Random.Range(0, partsPerSelection);

            if (partsQueue.TryPeek(out var tilePos))
            {
                var partsForSelection = new List<Vector2Int>(allParts);
                partsForSelection.Remove(tilePos);

                for (int i = 0; i < partsPerSelection; i++)
                {
                    if (i == part)
                    {
                        selection[i] = tilePos;
                    }
                    else
                    {
                        var rnd = partsForSelection[Random.Range(0, partsForSelection.Count)];
                        partsForSelection.Remove(rnd);
                        selection[i] = rnd;
                    }
                }

                EventTurnStart?.Invoke(tilePos, selection);
            }
            else
            {
                OnComplete();
            }
        }

        public void ProcessSelection(Vector2Int select)
        {
            if (select == partsQueue.Peek())
            {
                partsQueue.Dequeue();
                EventPieceSelected?.Invoke(select, true);
                StartTurn();
            }
            else
            {
                EventPieceSelected?.Invoke(select, false);
            }
        }

        private void AddToAssembled(Vector2Int pos)
        {
            assembledParts.Add(pos);
            notAssembledParts.Remove(pos);
        }

        private void GeneratePartsArray()
        {
            allParts = new List<Vector2Int>(cutSize.x * cutSize.y);
            assembledParts = new List<Vector2Int>(cutSize.x * cutSize.y);
            notAssembledParts = new List<Vector2Int>(cutSize.x * cutSize.y);

            partsState = new bool[CutSize.x, CutSize.y];

            Vector2Int randomStartPoint = new Vector2Int(Random.Range(0, CutSize.x), Random.Range(0, CutSize.y));
            partsState[randomStartPoint.x, randomStartPoint.y] = true;

            allParts.Add(randomStartPoint);
            assembledParts.Add(randomStartPoint);
            
            PartState[,] partsMap = new PartState[CutSize.x, CutSize.y];
            partsMap[randomStartPoint.x, randomStartPoint.y] = PartState.Filled;
            HashSet<Vector2Int> filledPoints = new HashSet<Vector2Int> { randomStartPoint };
            List<Vector2Int> availablePoints = new List<Vector2Int>(CutSize.x * CutSize.y);
            partsQueue = new Queue<Vector2Int>();


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
                partsQueue.Enqueue(queueItem);
                notAssembledParts.Add(queueItem);
            }
        }

        private enum PartState
        {
            Blocked,
            Available,
            Filled
        }
    }
}