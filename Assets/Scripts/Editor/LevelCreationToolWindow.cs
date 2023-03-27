using System.Collections.Generic;
using System.IO;
using System.Linq;
using Client;
using Client.Views.Level;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class LevelCreationToolWindow : EditorWindow
    {
        protected const string BaseArtPath = "Assets/Art/Levels/";
        protected static LevelsConfig storageBase;

        private static Sprite levelSprite;
        private static Sprite backSprite;
        private static string levelName;
        private static Vector2Int defaultCutSize;

        private static void DrawGUI()
        {
            EditorGUILayout.LabelField("LevelsStorage");
            storageBase = (LevelsConfig)EditorGUILayout.ObjectField(storageBase, typeof(LevelsConfig), true);
            EditorGUILayout.LabelField("Level name");
            levelName = EditorGUILayout.TextField(levelName);
            EditorGUILayout.LabelField("Level sprite");
            levelSprite = (Sprite)EditorGUILayout.ObjectField(levelSprite, typeof(Sprite), true);
            backSprite = (Sprite)EditorGUILayout.ObjectField(backSprite, typeof(Sprite), true);

            defaultCutSize = EditorGUILayout.Vector2IntField("CutSize", defaultCutSize);
        }

        [MenuItem("Tools/LevelCreationTool")]
        private static void ShowWindow()
        {
            SetDefaultData();

            var wind = GetWindow(typeof(LevelCreationToolWindow));
            wind.Show();
        }

        private static void SetDefaultData()
        {
            var levelDataStorage = AssetDatabase.FindAssets("t:LevelsConfig")[0];
            storageBase = AssetDatabase.LoadAssetAtPath<LevelsConfig>(AssetDatabase.GUIDToAssetPath(levelDataStorage));
        }

        private void OnGUI()
        {
            DrawGUI();

            if (GUILayout.Button("Add level"))
            {
                if (storageBase.LevelIdExists(levelName))
                {
                    throw new LevelExistsException(levelName);
                }

                CreateLevel(levelName, levelSprite, defaultCutSize, backSprite);
            }

            if (GUILayout.Button("Fill levels automaticly"))
            {
                var assets = AssetDatabase.FindAssets("t:Sprite");

                var pathes = from guid in assets
                    where AssetDatabase.GUIDToAssetPath(guid).Contains(BaseArtPath)
                    select AssetDatabase.GUIDToAssetPath(guid);

                var spritePathes = pathes.ToArray();

                var levelSprites = GetSpritesFromPathes(spritePathes);
                foreach (var spr in levelSprites)
                {
                    CreateLevel(spr.name, spr,defaultCutSize, backSprite);
                }
            }
        }

        private void CreateLevel(string id, Sprite sprite, Vector2Int size, Sprite backSprite)
        {
            var config = CreateInstance<LevelConfig>();
            config.name = id;
            config.SetData(id, sprite, size, backSprite);

            var path = Path.Combine("Assets\\Configs\\Levels", $"{id}_ViewData.asset");

            storageBase.AddLevel(config);
            EditorUtility.SetDirty(storageBase);
            
            AssetDatabase.CreateAsset(config, path);
            AssetDatabase.SaveAssets();
        }

        private static List<Sprite> GetSpritesFromPathes(string[] spritePathes)
        {
            var greyAtlases = from path in spritePathes select path;

            List<Sprite> sprites = new List<Sprite>();
            foreach (var atlas in greyAtlases)
            {
                var spriteObjs = AssetDatabase.LoadAllAssetsAtPath(atlas);
                foreach (var obj in spriteObjs)
                {
                    if (obj is Sprite spr)
                        sprites.Add(spr);
                }
            }

            return sprites;
        }

        private void OnFocus()
        {
            SetDefaultData();
        }
    }
}