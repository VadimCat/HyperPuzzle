using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

namespace Client.PuzzleCutMasksGenerator
{
    public class CutterTest : MonoBehaviour
    {
        [SerializeField] private Image image;
        private Dictionary<Vector2Int, Sprite> sprites;
        private bool isStarted;

        private void Start()
        {
            var back = new BackgroundWorker();
            back.DoWork += CreateSprites;
            back.RunWorkerCompleted += ApplySprite;
            back.RunWorkerAsync();
        }

        private void CreateSprites(object sender, DoWorkEventArgs e)
        {

            PuzzleCutter cutter = new PuzzleCutter();
            sprites = cutter.GetTiles(Vector2Int.one * 500, Vector2Int.one * 3);
            image.sprite = sprites[Vector2Int.zero];

            Debug.LogError("work start");
            Debug.LogError(sprites.Count);
        }

        private void ApplySprite(object sender, RunWorkerCompletedEventArgs e)
        {
        }

        private void Update()
        {
            if (isStarted)
            {
                Debug.LogError("qwe");
            }
        }
    }
}