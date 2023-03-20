using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Client.Views.Level
{
    public class CellView : BaseCell
    {
        [SerializeField] private Button button;

        public event Action EventClick;
        
        private void Awake()
        {
            button.onClick.AddListener(FireEventClick);
        }

        public async UniTask PlayMoveAnimation(Vector3 pos)
        {
            button.interactable = false;

            await root.DOMove(pos, .5f/*animationConfig.MoveTime*/).AwaitForComplete();
            await root.DOScale(1, .5f/*animationConfig.SelectTime*/).AwaitForComplete();

            sortingCanvas.overrideSorting = false;
            button.interactable = true;

            root.localPosition = Vector3.zero;
        }
        
        public void SetSize(Vector2 size)
        {
            imageRect.sizeDelta = size;
        }
        
        public UniTask PlaySetAnimation()
        {
            button.interactable = false;
            maskImage.sprite = rootImage.sprite;
            // await disableAnimationRoot.transform.DOScale(animationConfig.SelectScale, animationConfig.SelectTime)
            //     .SetLink(gameObject)
            //     .AwaitForComplete();
            return UniTask.CompletedTask;
        }

        private void FireEventClick()
        {
            EventClick?.Invoke();
        }

        public void ResetInput()
        {
            button.onClick.RemoveAllListeners();
            EventClick = null;
        }

        public async UniTask Fade()
        {
            image.transform.SetParent(transform.parent.parent);
            Destroy(gameObject);
            await image.DOFade(0, .5f).ToUniTask();
            Destroy(image.gameObject);
        }
    }

    public abstract class BaseCell : MonoBehaviour
    {
        [SerializeField] protected RawImage image;
        [SerializeField] protected RectTransform imageRect;
        [SerializeField] protected Canvas sortingCanvas;
        [SerializeField] protected Image maskImage;
        [SerializeField] protected Image rootImage;
        [SerializeField] private Transform disableAnimationRoot;
        
        protected Transform root => sortingCanvas.transform;
        
        public void SetData(LevelViewData viewData, Vector2Int position)
        {
            image.texture = viewData.image.texture;
            float w = (float)1 / viewData.cutSize.x;
            float h = (float)1 / viewData.cutSize.y;
            float x = w * position.x;
            float y = h * position.y;
            image.uvRect = new Rect(x, y, w, h);
        }
    }
}