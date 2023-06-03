using UnityEngine;
using UnityEngine.UI;

namespace Client.Views.Level
{
    public abstract class BaseCell : MonoBehaviour
    {
        [SerializeField] protected RawImage image;
        [SerializeField] protected RectTransform imageRect;
        [SerializeField] protected Canvas sortingCanvas;
        [SerializeField] protected Image maskImage;
        [SerializeField] protected Image rootImage;
        [SerializeField] private Transform disableAnimationRoot;
        
        protected Transform root => sortingCanvas.transform;
        
        public void SetData(LevelViewData viewData, Vector2Int position, Sprite puzzleSprite)
        {
            maskImage.sprite = puzzleSprite;
            
            if(image != null)
            {
                float w = (float)1 / viewData.cutSize.x;
                float h = (float)1 / viewData.cutSize.y;
                float x = w * position.x;
                float y = h * position.y;
                image.texture = viewData.image.texture;
                image.uvRect = new Rect(x, y, w, h);
            }
        }
    }
}