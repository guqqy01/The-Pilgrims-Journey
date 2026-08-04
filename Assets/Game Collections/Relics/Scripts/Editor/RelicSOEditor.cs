using UnityEngine;
using UnityEditor;
using System.Collections;

namespace Relics
{
    [CustomEditor(typeof(RelicSO))]
    public class RelicSOEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawDefaultInspector();
            serializedObject.ApplyModifiedProperties();

            RelicSO relicSO = (RelicSO)target;
            if (GUI.changed)
            {
                //UpdateIconPreview();
                string path = AssetDatabase.GetAssetPath(relicSO);
                RelicSOProjectIconDrawer.MarkAssetDirty(path);
            }
        }

        private void OnDisable()
        {
            if (target != null)
                EditorGUIUtility.SetIconForObject(target, null);
        }

        public override bool HasPreviewGUI()
        {
            RelicSO relicSO = (RelicSO)target;
            return relicSO.sprite != null;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            RelicSO relicSO = (RelicSO)target;

            if (relicSO.sprite != null)
            {
                Texture2D texture = AssetPreview.GetAssetPreview(relicSO.sprite);
                if (texture != null)
                {
                    GUI.DrawTexture(r, texture, ScaleMode.ScaleToFit);
                }
            }
        }
    }
}

