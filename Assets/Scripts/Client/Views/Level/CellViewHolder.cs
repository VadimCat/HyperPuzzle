using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Client.Views.Level
{
    public class CellViewHolder : BaseCell
    {
        [SerializeField] private Image localImage;
        public Transform CellRoot => localImage.transform; 

        public async UniTask Highlight()
        {
            await localImage.DOColor(Color.green, .5f).ToUniTask();
        }
    }
}