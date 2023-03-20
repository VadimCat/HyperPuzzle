using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Ji2.Utils;
using Ji2Core.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Client.Views.Level
{
    public class LevelView : MonoBehaviour
    {
        private const float CellSizeRatio = 1;
        // private const float CellSizeRatio = 0.96F;
        // private const float CellDistanceRatio = 0.04F;

        [SerializeField] private RectTransform cellsRootTransform;
        [SerializeField] private GridLayoutGroup grid;
        [SerializeField] private Transform selectionBar;
        
        
        private readonly Dictionary<Vector2Int, CellView> cellViews = new();

        public Vector2 CellSize => grid.cellSize;
        public Transform GridRoot => grid.transform;
        public Transform SelectionBar => selectionBar;

        private Context context;

        public void Awake()
        {
            context = Context.GetInstance();
            context.Register(this);
        }

        public void OnDestroy()
        {
            context.Unregister(this);
        }

        public void SetGridSizeByData(LevelViewData levelData)
        {
            var rect = cellsRootTransform.rect;
            float cellWidth = rect.width / levelData.cutSize.x;
            float aspectHeight = rect.width / levelData.Aspect;

            float cellHeight = aspectHeight / levelData.cutSize.y;
            grid.constraintCount = levelData.cutSize.x;
            Vector2 cellSize = new Vector2(cellWidth, cellHeight);

            grid.cellSize = cellSize * CellSizeRatio;
            // grid.spacing = cellSize * CellDistanceRatio;
        }

        public async UniTask AnimateWin()
        {
            var sequence = DOTween.Sequence();
            sequence.Append(grid.DoSpacing(Vector2.zero, .2f))
                .Append(grid.transform.DOScale(.9f, .2f));
            await sequence.AwaitForComplete();
        }
    }
}