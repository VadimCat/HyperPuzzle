using System;
using System.Collections.Generic;
using Client.PuzzleCutMasksGenerator;
using Client.UI.Screens;
using Client.Views.Level;
using Core.Compliments;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Ji2.CommonCore;
using Ji2.Presenters;
using Ji2Core.Core.Audio;
using Ji2Core.Core.ScreenNavigation;
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
        private readonly PuzzleCutter puzzleCutter = new();

        private readonly ModelAnimator modelAnimator = new();
        private readonly Dictionary<Vector2Int, CellViewHolder> posToCellHolder = new();

        private LevelViewData viewData;
        // private readonly Dictionary<CellView, Vector2Int> viewToPos = new();

        private LevelScreen levelScreen;
        private int tilesSet;

        private readonly Dictionary<Vector2Int, CellView> pointToCellView = new(3);
        private Dictionary<Vector2Int, Sprite> puzzleSprites;

        public Level Model => model;

        public LevelPresenter(LevelView view, Level model, ScreenNavigator screenNavigator,
            UpdateService updateService, LevelsConfig levelsViewConfig, LevelsLoopProgress levelsLoopProgress,
            AudioService audioService, ICompliments compliments)
        {
            this.view = view;
            this.model = model;
            this.screenNavigator = screenNavigator;
            this.updateService = updateService;
            this.levelsViewConfig = levelsViewConfig;
            this.levelsLoopProgress = levelsLoopProgress;
            this.audioService = audioService;
            this.compliments = compliments;
            
            model.Health.EventValueChanged += OnHealthChanged;
            model.EventPieceSelected += OnPieceSelected;
            model.EventTurnStart += OnTurnStart;

            model.EventLevelCompleted += OnEventLevelCompleted;
        }

        private void OnHealthChanged(int value, int prevValue)
        {
            levelScreen.UpdateHealthCount(value);
        }

        public void BuildLevel()
        {
            viewData = levelsViewConfig.GetData(model.Name).ViewData((int)model.Difficulty);
            var texture = viewData.image.texture;
            puzzleSprites = puzzleCutter.GetTiles(new Vector2Int(texture.width, texture.height), model.CutSize);

            view.SetGridSizeByData(viewData);

            for (var y = 0; y < model.CutSize.y; y++)
            for (var x = 0; x < model.CutSize.x; x++)
            {
                var position = new Vector2Int(x, y);

                if (model.PartsState[x, y])
                {
                    var cellView = Object.Instantiate(levelsViewConfig.CellView, view.GridRoot);
                    cellView.SetData(viewData, position, puzzleSprites[position] ?? default);
                }
                else
                {
                    CellViewHolder cellHolder = Object.Instantiate(levelsViewConfig.CellHolder, view.GridRoot);
                    cellHolder.SetData(viewData, position, puzzleSprites[position]);
                    posToCellHolder[position] = cellHolder;
                }
            }
        }

        public void StartLevel()
        {
            model.LogAnalyticsLevelStart();
            levelScreen = (LevelScreen)screenNavigator.CurrentScreen;
            levelScreen.SetLevelName($"Level {model.LevelCount + 1}");
            levelScreen.InitHealthCount(model.Health.Value);

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

        private void OnTurnStart(Vector2Int highlightPos, List<Vector2Int> selection)
        {
            modelAnimator.Enqueue(() =>
            {
                posToCellHolder[highlightPos].Highlight().Forget();

                foreach (var point in selection)
                {
                    var cell = Object.Instantiate(levelsViewConfig.CellView, view.SelectionBar);
                    cell.SetData(viewData, point, puzzleSprites[point]);
                    cell.SetSize(view.CellSize);

                    cell.EventClick += () => ProcessSelection(point);
                    pointToCellView[point] = cell;
                }
            }).Forget();
        }

        private void OnPieceSelected(Vector2Int pos, bool isRight)
        {
            modelAnimator.Enqueue(() =>
            {
                pointToCellView.Remove(pos, out var cell);

                List<UniTask> anims = new List<UniTask>(3);
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
            }).Forget();
        }

        private async void OnEventLevelCompleted()
        {
            model.Health.EventValueChanged += OnHealthChanged;
            model.EventTurnStart += OnTurnStart;
            model.EventPieceSelected += OnPieceSelected;
            model.EventLevelCompleted += OnEventLevelCompleted;

            foreach (var value in pointToCellView.Values)
            {
                value.ResetInput();
            }

            pointToCellView.Clear();

            model.LogAnalyticsLevelFinish();
            levelsLoopProgress.IncLevel();
            updateService.Remove(this);

            modelAnimator.Enqueue(view.AnimateWin).Forget();
            await modelAnimator.AwaitAllAnimationsEnd();

            // audioService.PlaySfxAsync(AudioClipName.WinFX);
            Object.Destroy(view.gameObject);

            EventLevelCompleted?.Invoke();
        }
    }
}