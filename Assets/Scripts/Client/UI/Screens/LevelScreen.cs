using Cysharp.Threading.Tasks;
using DG.Tweening;
using Ji2.UI;
using Ji2.Utils;
using Ji2Core.UI.Screens;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Client.UI.Screens
{
    public class LevelScreen : BaseScreen
    {
        [SerializeField] private TMP_Text levelName;
        [SerializeField] private IProgressBar progressBar;

        public void SetLevelName(string name)
        {
            levelName.text = name;
        }

        // public void SetUpProgressBar(int okCount, int goodCount, int perfectCount)
        // {
        //     this.perfectCount = perfectCount;
        //     this.goodCount = goodCount;
        //     this.okCount = okCount;
        //
        //     handleImage.color = resultViewConfig.GetColor(LevelResult.Perfect);
        //     
        //     int totalCount = okCount + 2;
        //
        //     float worstAreaPercent = 2 / (float)totalCount;
        //     float okAreaPercent = (okCount - goodCount) / (float)totalCount;
        //     float goodAreaPercent = (goodCount - perfectCount) / (float)totalCount;
        //     float perfectAreaPercent = perfectCount / (float)totalCount;
        //
        //     worstArea.SetWidth(worstAreaPercent * commonProgressArea.rect.width);
        //     okArea.SetWidth(okAreaPercent * commonProgressArea.rect.width);
        //     goodArea.SetWidth(goodAreaPercent * commonProgressArea.rect.width);
        //     perfectArea.SetWidth(perfectAreaPercent * commonProgressArea.rect.width);
        //
        //     slider.maxValue = totalCount;
        //     
        //     worstArea.transform.SetLocalX(0);
        //     okArea.transform.SetLocalX(worstArea.sizeDelta.x);
        //     goodArea.transform.SetLocalX(worstArea.sizeDelta.x + okArea.sizeDelta.x);
        //     perfectArea.transform.SetLocalX(worstArea.sizeDelta.x + okArea.sizeDelta.x + goodArea.sizeDelta.x);
        // }
    }
}