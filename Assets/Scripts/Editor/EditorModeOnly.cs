using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Editor
{
    [InitializeOnLoad]
    public class EditorModeOnly
    {
        public static readonly string EditorModeOnlyTag = "EditorOnly";
        public static readonly string DeactivateOnStartTag = "DeactivateInPlaymode";

        static EditorModeOnly()
        {
            UnityEngine.Object[] asset = AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset");
            if ((asset != null) && (asset.Length > 0))
            {
                SerializedObject so = new SerializedObject(asset[0]);
                SerializedProperty tags = so.FindProperty("tags");
                bool hasEditorModeOnlyTag = false;
                bool hasDeactivateOnStartTag = false;

                for (int i = 0; i < tags.arraySize; ++i)
                {
                    SerializedProperty element = tags.GetArrayElementAtIndex(i);
                    if (element.stringValue == EditorModeOnlyTag)
                    {
                        hasEditorModeOnlyTag = true;
                    }

                    if (element.stringValue == DeactivateOnStartTag)
                    {
                        hasDeactivateOnStartTag = true;
                    }
                }

                if (!hasEditorModeOnlyTag)
                {
                    tags.InsertArrayElementAtIndex(0);
                    tags.GetArrayElementAtIndex(0).stringValue = EditorModeOnlyTag;
                    so.ApplyModifiedProperties();
                    so.Update();
                }

                if (!hasDeactivateOnStartTag)
                {
                    tags.InsertArrayElementAtIndex(0);
                    tags.GetArrayElementAtIndex(0).stringValue = DeactivateOnStartTag;
                    so.ApplyModifiedProperties();
                    so.Update();
                }
            }
        }

        static GameObject[] FindGameObjectsByTag(string tag)
        {
            List<GameObject> validTransforms = new List<GameObject>();
            Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>();

            for (int i = 0; i < objs.Length; i++)
            {
                var t = objs[i];
                if (t.hideFlags == HideFlags.None && t.gameObject.CompareTag(tag))
                {
                    validTransforms.Add(t.gameObject);
                }
            }

            return validTransforms.ToArray();
        }

        [PostProcessScene(0)]
        public static void OnPostprocessScene()
        {
            var editorModeTaggedObjects = FindGameObjectsByTag(EditorModeOnlyTag);

            foreach (GameObject go in editorModeTaggedObjects)
            {
                // Filter out gameobjects in resources folder if we are just entering playmode and not building the game
                if (!go.scene.IsValid())
                    continue;

                Object.DestroyImmediate(go, false);
            }

            var DeactivateOnStartTaggedObjects = FindGameObjectsByTag(DeactivateOnStartTag);

            foreach (GameObject go in DeactivateOnStartTaggedObjects)
            {
                // Deactivate only objects in scene
                if (!go.scene.IsValid())
                    continue;

                go.SetActive(false);
            }
        }
    }
}