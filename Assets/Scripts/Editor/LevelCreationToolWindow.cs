using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class LevelCreationToolWindow : EditorWindow
    {
        private const string BaseArtPath = "Assets/Art/LevelsArt/";

        private static GameObject levelBaseGrey;
        private static GameObject levelBaseColor;
        // private static LevelsViewDataStorageBase<> storageBase;
        private static Material baseDirtMaterial;

        private static Sprite colorSprite;
        private static Sprite maskSprite;

        private static string levelName;
        
        [MenuItem("Tools/LevelCreationTool")]
        private static void ShowWindow()
        {
            SetDefaultData();

            var wind = GetWindow(typeof(LevelCreationToolWindow));
            wind.Show();
        }

        private static void SetDefaultData()
        {
            var levelDataStorage = AssetDatabase.FindAssets("t:LevelsViewDataStorage")[0];
            // storageBase = AssetDatabase.LoadAssetAtPath<LevelsViewDataStorageBase<>>(
            //     AssetDatabase.GUIDToAssetPath(levelDataStorage));

            var material = AssetDatabase.FindAssets("dirt t:Material")[0];
            baseDirtMaterial = AssetDatabase.LoadAssetAtPath<Material>(AssetDatabase.GUIDToAssetPath(material));

            var levelColor = AssetDatabase.FindAssets("LevelViewBase t:Prefab")[0];
            levelBaseGrey = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(levelColor));
            
            var levelGrey = AssetDatabase.FindAssets("LevelViewBaseColor t:Prefab")[0];
            levelBaseColor = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(levelGrey));
        }

        private void OnGUI()
        {
            DrawGUI();

            if (GUILayout.Button("Add level"))
            {
                // if (storageBase.LevelIdExists(levelName))
                // {
                //     throw new LevelExistsException(levelName);
                // }

                CreateLevel(levelName, maskSprite, colorSprite);
            }

            if (GUILayout.Button("Fill levels automaticly"))
            {
                var assets = AssetDatabase.FindAssets("t:Sprite");

                var pathes = from guid in assets
                    where AssetDatabase.GUIDToAssetPath(guid).Contains(BaseArtPath)
                    select AssetDatabase.GUIDToAssetPath(guid);

                var spritePathes = pathes.ToArray();

                Func<string, bool> greyFilter = s => s.Contains("grey");
                Func<string, bool> colorFilter = s => !s.Contains("grey");

                var greySprites = GetSpritesFromPathesByFilter(spritePathes, greyFilter);
                var colorSprites = GetSpritesFromPathesByFilter(spritePathes, colorFilter);
                foreach (var spr in greySprites)
                {
                    var sprName = spr.name.Replace("-grey", string.Empty);

                    var findIndex = colorSprites.FindIndex(sp => sp.name == sprName);
                    if (findIndex != -1)
                    {
                        CreateLevel(sprName, spr, colorSprites[findIndex]);
                    }
                }
            }
        }

        private void CreateLevel(string levelName, Sprite maskSprite, Sprite colorSprite)
        {
            var material = CreateLevelMaterial(levelName, maskSprite);
            var maskSavedPref = CreateMaskPref(levelName, material);
            var colorSavedPref = CreateColorPref(levelName, colorSprite, material);

            AddConfig(levelName, maskSavedPref, colorSavedPref, colorSprite);
        }

        private static void DrawGUI()
        {
            EditorGUILayout.LabelField("LevelsStorage");
            // storageBase = (LevelsViewDataStorageBase<>)EditorGUILayout.ObjectField(storageBase, typeof(LevelsViewDataStorageBase<>), true);
            EditorGUILayout.LabelField("Base and dirt material");
            baseDirtMaterial = (Material)EditorGUILayout.ObjectField(baseDirtMaterial, typeof(Material), true);
            EditorGUILayout.LabelField("LevelConfigBasePrefab");
            levelBaseGrey = (GameObject)EditorGUILayout.ObjectField(levelBaseGrey, typeof(GameObject), true);
            EditorGUILayout.LabelField("LevelConfigBasePrefab");
            levelBaseColor = (GameObject)EditorGUILayout.ObjectField(levelBaseColor, typeof(GameObject), true);

            EditorGUILayout.LabelField("Level name");
            levelName = EditorGUILayout.TextField(levelName);

            EditorGUILayout.LabelField("Color sprite");
            colorSprite = (Sprite)EditorGUILayout.ObjectField(colorSprite, typeof(Sprite), true);
            EditorGUILayout.LabelField("Mask sprite");
            maskSprite = (Sprite)EditorGUILayout.ObjectField(maskSprite, typeof(Sprite), true);
        }

        private static List<Sprite> GetSpritesFromPathesByFilter(string[] spritePathes, Func<string, bool> filter)
        {
            var greyAtlases = (from path in spritePathes
                where filter(path)
                select path);

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

        private static void AddConfig(string levelName, GameObject maskSavedPref, GameObject colorSavedPref,
            Sprite resultSprite)
        {
            // var levelAsset = CreateInstance<LevelViewData>();
            // levelAsset.SetData(levelName, maskSavedPref.GetComponent<ColoringLevelView>(),
            //     colorSavedPref.GetComponent<ColoringLevelView>(), resultSprite);

            var path = Path.Combine("Assets\\Configs\\Levels", $"{levelName}ViewData.asset");

            // AssetDatabase.CreateAsset(levelAsset, path);
            // EditorUtility.SetDirty(levelAsset);

            // storageBase.AddLevel(levelAsset);
            // EditorUtility.SetDirty(storageBase);

            AssetDatabase.Refresh();
        }

        private static GameObject CreateColorPref(string levelName, Sprite ColorSprite, Material material)
        {
            var path = Path.Combine("Assets\\Prefabs\\Levels", levelName, $"{levelName}Color.prefab");

            var levelPref = PrefabUtility.InstantiatePrefab(levelBaseColor) as GameObject;
            var sprites = levelPref.GetComponentsInChildren<SpriteRenderer>();
            sprites[1].sprite = ColorSprite;//HACK
            
            levelPref.GetComponentInChildren<MeshRenderer>().material = material;
            
            var colorSavedPref = PrefabUtility.SaveAsPrefabAsset(levelPref, path);
            DestroyImmediate(levelPref);
            return colorSavedPref;
        }

        private static GameObject CreateMaskPref(string levelName, Material material)
        {
            string path = Path.Combine("Assets\\Prefabs\\Levels", levelName, $"{levelName}Dirt.prefab");
            
            var levelPref = PrefabUtility.InstantiatePrefab(levelBaseGrey) as GameObject;
            var sprites = levelPref.GetComponentsInChildren<MeshRenderer>();
            sprites[1].material = material;

            var maskSavedPref = PrefabUtility.SaveAsPrefabAsset(levelPref, path);
            DestroyImmediate(levelPref);
            return maskSavedPref;
        }
        
        private Material CreateLevelMaterial(string levelName, Sprite sprite)
        {
            var path = Path.Combine("Assets\\Prefabs\\Levels", levelName);
            Material mat = new Material(baseDirtMaterial.shader);
            mat.name = levelName;
            mat.CopyPropertiesFromMaterial(baseDirtMaterial);
            mat.mainTexture = sprite.texture;
            AssetDatabase.CreateFolder("Assets\\Prefabs\\Levels", levelName);
            path = Path.Combine(path, $"{mat.name}.mat");
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private void OnFocus()
        {
            SetDefaultData();
        }
    }
}