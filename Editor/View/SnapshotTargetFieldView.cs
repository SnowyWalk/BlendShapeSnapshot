using UnityEditor;
using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    internal static class SnapshotTargetFieldView
    {
        public static void Draw(BlendShapeSnapshotViewState state, ref BlendShapeSnapshotViewEvents events)
        {
            const string label = "대상 Mesh";
            EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(new GUIContent(label)).x + 8f;
            SkinnedMeshRenderer target = EditorGUILayout.ObjectField(label, state.TargetRenderer, typeof(SkinnedMeshRenderer), true, GUILayout.ExpandWidth(true)) as SkinnedMeshRenderer;
            EditorGUIUtility.labelWidth = 0f;

            if (target == state.TargetRenderer)
                return;

            events.TargetChanged = true;
            events.TargetRenderer = target;
        }
    }
}
