using System.Collections.Generic;
using DG.Tweening;
using Ji2Core.UI.Screens;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Client.UI.Screens
{
    public class LevelScreen : BaseScreen
    {
        [SerializeField] private TMP_Text levelName;
        [SerializeField] private Transform healthRoot;
        [SerializeField] private Image heartPrefab;

        private List<Image> hearts;

        public void SetLevelName(string name)
        {
            levelName.text = name;
        }

        public void InitHealthCount(int count)
        {
            hearts = new List<Image>(count);

            for (int i = 0; i < count; i++)
            {
                hearts.Add(Instantiate(heartPrefab, healthRoot));
            }
        }

        public void UpdateHealthCount(int value)
        {
            hearts[value].DOColor(Color.gray, 2f);
        }
    }
}