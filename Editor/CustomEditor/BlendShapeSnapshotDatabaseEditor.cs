using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    [CustomEditor(typeof(BlendShapeSnapshotDatabase))]
    public class BlendShapeSnapshotDatabaseEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            BlendShapeSnapshotDatabase database = (BlendShapeSnapshotDatabase)target;
            IReadOnlyList<BlendShapeSnapshotDatabase.BlendShapeSnapshot> snapshots = database.BlendShapeSnapshots;
            EditorGUILayout.LabelField($"Target Guid: {database.TargetGuid}", EditorStyles.boldLabel); // TODO: ObjectField + 리프레쉬 버튼 적용
            EditorGUILayout.LabelField($"Snapshot List: (Total: {snapshots.Count})");

            EditorGUI.indentLevel++;
            for (int i = snapshots.Count - 1; i >= 0; i--)
            {
                BlendShapeSnapshotDatabase.BlendShapeSnapshot snapshot = snapshots[i];
                EditorGUILayout.LabelField($"{i + 1}. {snapshot.SnapshotTime}   ·   {snapshot.Description}", EditorStyles.boldLabel);

                string foldoutKey = $"BlendShapeSnapshot.{database.TargetGuid}.{snapshot.SnapshotTime}";
                bool lastIsOpen = GetFoldoutState(foldoutKey);
                bool isOpen = DrawFoldoutSection("BlendShapeKey List", lastIsOpen, () =>
                {
                    foreach (var blendShapeKeyData in snapshot.BlendShapeWeights)
                    {
                        EditorGUILayout.BeginHorizontal();
                        GUI.enabled = false;
                        EditorGUILayout.TextField(blendShapeKeyData.blendShapeName, GUILayout.ExpandWidth(true));
                        EditorGUILayout.FloatField(blendShapeKeyData.value, GUILayout.ExpandWidth(true));
                        GUI.enabled = true;
                        EditorGUILayout.EndHorizontal();
                    }
                });

                if (isOpen != lastIsOpen)
                    SetFoldoutState(foldoutKey, isOpen);

                EditorGUILayout.Space();
            }
            EditorGUI.indentLevel--;

        }

        private static bool DrawFoldoutSection(string title, bool isOpen, System.Action drawContent)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            isOpen = EditorGUILayout.Foldout(isOpen, title, true);
            if (isOpen)
            {
                EditorGUI.indentLevel++;
                drawContent?.Invoke();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();

            return isOpen;
        }

        private bool GetFoldoutState(string key)
        {
            return SessionState.GetBool(key, true);
        }

        private void SetFoldoutState(string key, bool isOpen)
        {
            SessionState.SetBool(key, isOpen);
        }
    }
}