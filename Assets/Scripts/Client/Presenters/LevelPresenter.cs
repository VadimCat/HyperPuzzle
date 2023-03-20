using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Client.UI.Screens;
using Client.Views.Level;
using Core.Compliments;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Ji2.CommonCore;
using Ji2.Presenters;
using Ji2Core.Core.Audio;
using Ji2Core.Core.ScreenNavigation;
using Ji2Core.Core.UserInput;
using Models;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Client.Presenters
{
    public class LevelPresenter : IUpdatable
    {
        public event Action EventLevelCompleted;

        private readonly LevelView view;
        private readonly Level model;
        private readonly ScreenNavigator screenNavigator;
        private readonly UpdateService updateService;
        private readonly LevelsConfig levelsViewConfig;
        private readonly LevelsLoopProgress levelsLoopProgress;
        private readonly AudioService audioService;
        private readonly ICompliments compliments;


        private readonly ModelAnimator modelAnimator = new();
        private readonly Dictionary<Vector2Int, CellViewHolder> posToCellHolder = new();

        private LevelViewData viewData;
        // private readonly Dictionary<CellView, Vector2Int> viewToPos = new();

        private LevelScreen levelScreen;
        private int tilesSet;

        private Dictionary<Vector2Int, CellView> pointToCellView = new(3);

        public Level Model => model;

        public LevelPresenter(LevelView view, Level model, ScreenNavigator screenNavigator,
            UpdateService updateService, LevelsConfig levelsViewConfig, LevelsLoopProgress levelsLoopProgress,
            AudioService audioService, ICompliments compliments, InputService inputService)
        {
            this.view = view;
            this.model = model;
            this.screenNavigator = screenNavigator;
            this.updateService = updateService;
            this.levelsViewConfig = levelsViewConfig;
            this.levelsLoopProgress = levelsLoopProgress;
            this.audioService = audioService;
            this.compliments = compliments;

            model.EventTurnStart += OnTurnStart;
            model.EventPieceSelected += OnPieceSelected;
            model.EventLevelCompleted += OnEventLevelCompleted;
        }

        public void BuildLevel()
        {
            viewData = levelsViewConfig.GetData(model.Name).ViewData((int)model.Difficulty);
            view.SetGridSizeByData(viewData);

            for (var y = 0; y < model.CutSize.y; y++)
            for (var x = 0; x < model.CutSize.x; x++)
            {
                var position = new Vector2Int(x, y);

                if (model.PartsState[x, y])
                {
                    var cellView = Object.Instantiate(levelsViewConfig.CellView, view.GridRoot);
                    cellView.SetData(viewData, position);
                }
                else
                {
                    var cellHolder = Object.Instantiate(levelsViewConfig.CellHolder, view.GridRoot);
                    posToCellHolder[new Vector2Int(x, y)] = cellHolder;
                }
            }
        }

        public void StartLevel()
        {
            model.LogAnalyticsLevelStart();
            levelScreen = (LevelScreen)screenNavigator.CurrentScreen;
            levelScreen.SetLevelName($"Level {model.LevelCount + 1}");

            model.StartTurn();

            updateService.Add(this);
        }

        public void OnUpdate()
        {
            model.AppendPlayTime(Time.deltaTime);
        }

        public Vector3 GetTilePos(Vector2Int pos)
        {
            return posToCellHolder[pos].transform.position;
        }

        private void ProcessSelection(Vector2Int point)
        {
            model.ProcessSelection(point);
        }

        private void OnTurnStart(Vector2Int highlightPos, Vector2Int[] selection)
        {
            modelAnimator.Enqueue(() =>
            {
                posToCellHolder[highlightPos].Highlight();

                Debug.LogError(pointToCellView.Count);
                
                foreach (var point in selection)
                {
                    var cell = Object.Instantiate(levelsViewConfig.CellView, view.SelectionBar);
                    cell.SetData(viewData, point);
                    cell.SetSize(view.CellSize);

                    cell.EventClick += () => ProcessSelection(point);
                    pointToCellView[point] = cell;
                }
            });
        }

        private void OnPieceSelected(Vector2Int pos, bool isRight)
        {
            modelAnimator.Enqueue(() =>
            {
                pointToCellView.Remove(pos, out var cell);

                List<UniTask> anims = new List<UniTask>(3);
                Debug.LogError(isRight);
                if (isRight)
                {
                    cell.transform.SetParent(posToCellHolder[pos].CellRoot);
                    anims.Add(cell.transform.DOLocalMove(Vector3.zero, .5f).ToUniTask());

                    foreach (var key in pointToCellView.Keys)
                    {
                        anims.Add(pointToCellView[key].Fade());
                    }
                    pointToCellView.Clear();
                }
                else
                {
                    anims.Add(cell.Fade());
                }

                return UniTask.WhenAll(anims);
            });
        }

        private async void OnEventLevelCompleted()
        {
            model.LogAnalyticsLevelFinish();
            levelsLoopProgress.IncLevel();
            updateService.Remove(this);

            modelAnimator.Enqueue(view.AnimateWin);
            await modelAnimator.AwaitAllAnimationsEnd();

            // audioService.PlaySfxAsync(AudioClipName.WinFX);
            Object.Destroy(view.gameObject);

            EventLevelCompleted?.Invoke();
        }
    }
}