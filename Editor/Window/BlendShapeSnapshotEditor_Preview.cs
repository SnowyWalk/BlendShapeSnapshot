using UnityEditor;
using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public partial class BlendShapeSnapshotEditor
    {
        private void DrawSnapShotPreview()
        {
            float previewWidth = m_contentWidth;
            Rect previewRect = GUILayoutUtility.GetAspectRect(16f / 9f, GUILayout.Width(previewWidth));
            GUI.Box(previewRect.Inset(0f), GUIContent.none);
            
            if (IsPreviewing)
            {
                if (Event.current.type == EventType.Repaint)
                    m_snapshotPreviewRenderer.Render(previewRect);
                
                var labelRect = new Rect(previewRect.x, previewRect.y, previewRect.width, EditorGUIUtility.singleLineHeight);
                GUI.Label(labelRect, m_snapshots != null ? m_snapshots[m_selectedListViewIndex] : string.Empty , EditorStyles.centeredGreyMiniLabel); // TODO: selected snapshot
            }
            
            EditorGUILayout.LabelField("Selected: ", EditorStyles.boldLabel);
        }
    }
}
