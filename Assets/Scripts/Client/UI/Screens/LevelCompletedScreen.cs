using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Ji2.Utils;
using Ji2Core.UI.Screens;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Client.UI.Screens
{
    public class LevelCompletedScreen : BaseScreen
    {
        private const string levelNamePattern = "LEVEL {0}";
        [SerializeField] private Button nextButton;
        [SerializeField] private Image levelResult;
        [SerializeField] private TMP_Text levelName;
        [SerializeField] private Transform light0;

        public event Action ClickNextEvent;

        private void Awake()
        {
            AnimateBackLight();
            nextButton.onClick.AddListener(FireNext);
        }

        private void AnimateBackLight()
        {
            AnimateLight();

            AnimateLevelResultImage();
        }

        private void AnimateLevelResultImage()
        {
            levelResult.transform.DoPulseScale(1.06f, 1, gameObject);
            levelResult.transform.DORotate(Vector3.forward * 2.2f, 1)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.Linear)
                .SetLink(gameObject);
        }

        private void AnimateLight()
        {
            light0.transform.DoPulseScale(1.2f, 1, gameObject);
            light0.DORotate(Vector3.back * 180, 8)
                .SetLoops(-1, LoopType.Incremental)
                .SetEase(Ease.Linear)
                .SetLink(gameObject);
        }

        public void SetLevelResult(Sprite levelResult, int levelNumber)
        {
            this.levelResult.sprite = levelResult;
            this.levelName.text = string.Format(levelNamePattern, levelNumber);
        }

        private async void FireNext()
        {
            await nextButton.transform.DOScale(0.9f, 0.1f).AwaitForComplete();
            Complete();
        }

        private void Complete()
        {
            ClickNextEvent?.Invoke();
        }

        private void OnDestroy()
        {
            ClickNextEvent = null;
        }
    }
}