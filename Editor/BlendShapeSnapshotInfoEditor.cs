using UnityEditor;
using UnityEngine;
namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    [CustomEditor(typeof(BlendShapeSnapshotAsset))]
    public class BlendShapeSnapshotInfoEditor : UnityEditor.Editor
    {
        private bool m_isBlendShapeListOpen = true;

        public override void OnInspectorGUI()
        {
            BlendShapeSnapshotAsset asset = (BlendShapeSnapshotAsset)target;
            serializedObject.Update();

            EditorGUILayout.LabelField($"Description: {asset.Description}");
            EditorGUILayout.LabelField($"Snapshot Time: {asset.SnapshotTime}");

            m_isBlendShapeListOpen = DrawFoldoutSection("BlendShapeKey List", m_isBlendShapeListOpen, () =>
            {
                foreach (var blendShapeKeyData in asset.BlendShapeKeyDataList)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.TextField(blendShapeKeyData.blendShapeKey, GUILayout.ExpandWidth(true));
                    EditorGUILayout.FloatField(blendShapeKeyData.value, GUILayout.ExpandWidth(true));
                    EditorGUILayout.EndHorizontal();
                }
            });

        }

        private static bool DrawFoldoutSection(string title, bool isOpen, System.Action drawContent)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Space(16f);
            
            isOpen = EditorGUILayout.Foldout(isOpen, title, true);
            if (isOpen)
            {
                EditorGUILayout.Space(4);
                EditorGUI.indentLevel++;
                drawContent?.Invoke();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();

            return isOpen;
        }
    }
}