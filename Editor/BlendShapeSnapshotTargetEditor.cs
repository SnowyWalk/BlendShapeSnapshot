using UnityEditor;
using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    [CustomEditor(typeof(BlendShapeSnapshotTarget))]
    public class BlendShapeSnapshotTargetEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            BlendShapeSnapshotTarget component = (BlendShapeSnapshotTarget)target;

            EditorGUILayout.LabelField($"Guid: {component.Guid}", EditorStyles.boldLabel);
        }
    }
}