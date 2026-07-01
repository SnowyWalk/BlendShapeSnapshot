using System;
using UnityEditor;
using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    internal static class SnapshotPreviewView
    {
        public static void Draw(float contentWidth, float previewHeight, BlendShapeSnapshotViewState state, Action<Rect> renderPreview)
        {
            Rect slotRect = GUILayoutUtility.GetRect(contentWidth, previewHeight, GUILayout.Width(contentWidth), GUILayout.Height(previewHeight));
            Rect previewRect = SnapshotViewLayout.FitAspect(slotRect, SnapshotViewLayout.PreviewAspect);
            GUI.Box(slotRect.Inset(0f), GUIContent.none);

            if (state.TargetRenderer != null && Event.current.type == EventType.Repaint)
                renderPreview?.Invoke(previewRect);

            Rect labelRect = new Rect(previewRect.x, previewRect.y, previewRect.width, SnapshotViewLayout.LineHeight);
            GUI.Label(labelRect, state.PreviewLabel, EditorStyles.centeredGreyMiniLabel);
            EditorGUILayout.LabelField($"Selected: {state.SelectedIndex} {state.PreviewLabel}", EditorStyles.boldLabel);
        }
    }
}
