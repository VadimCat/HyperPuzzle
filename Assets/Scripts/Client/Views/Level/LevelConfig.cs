using Ji2.Models;
using UnityEngine;

namespace Client.Views.Level
{
    [CreateAssetMenu(fileName = "LevelViewConfig")]
    public class LevelConfig : ScriptableObject, ILevelView
    {
        [SerializeField] private string id;
        [SerializeField] private Sprite image;
        [SerializeField] private Vector2Int[] cutSize = { new(3, 3) };
        [SerializeField] private Sprite background;

        public Sprite Background => this.background;
        public string Id => this.id;
        public Sprite Image => image;

        public LevelViewData ViewData(int loop)
        {
            var difficulty = (Difficulty)Mathf.Clamp(loop, 0, cutSize.Length);
            var vector2Int = cutSize[Mathf.Clamp(loop, 0, cutSize.Length - 1)];
            
            return new LevelViewData()
            {
                difficulty = difficulty,
                cutSize = vector2Int,
                image = image
            };
        }

#if UNITY_EDITOR
        public void SetData(string id, Sprite image, Vector2Int cutSize, Sprite backSprite)
        {
            this.id = id;
            this.image = image;
            this.cutSize = new[] { cutSize };
            this.background = backSprite;
        }
#endif
    }

    public static class SpriteExtensions
    {
        public static float Aspect(this Sprite sprite)
        {
            return sprite.texture.width / sprite.texture.width;
        }
    }

    public class LevelViewData
    {
        public Sprite image;
        public Vector2Int cutSize;
        public Difficulty difficulty;
        public int selectionCount => 3;

        public float Aspect => this.image.Aspect();
    }
}