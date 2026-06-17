using UnityEditor;
using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public partial class BlendShapeSnapshotEditor
    {
        private void DrawSnapShotPreview(float previewHeight)
        {
            float previewWidth = m_contentWidth;
            Rect slotRect = GUILayoutUtility.GetRect(previewWidth, previewHeight, GUILayout.Width(previewWidth), GUILayout.Height(previewHeight));
            Rect previewRect = FitAspect(slotRect, 16f / 9f);
            GUI.Box(slotRect.Inset(0f), GUIContent.none);
            
            if (IsPreviewing)
            {
                if (Event.current.type == EventType.Repaint)
                    m_snapshotPreviewRenderer.Render(previewRect);
                
                var labelRect = new Rect(previewRect.x, previewRect.y, previewRect.width, EditorGUIUtility.singleLineHeight);
                GUI.Label(labelRect, GetPreviewLabel(), EditorStyles.centeredGreyMiniLabel); // TODO: selected snapshot
            }
            
            EditorGUILayout.LabelField($"Selected: {m_selectedListViewIndex} {GetPreviewLabel()}", EditorStyles.boldLabel);
        }

        private string GetPreviewLabel()
        {
            if (!IsPreviewing || m_snapshots == null || m_selectedListViewIndex < 0 || m_selectedListViewIndex >= m_snapshots.Count)
                return string.Empty;

            return m_snapshots[m_selectedListViewIndex];
        }

        private static Rect FitAspect(Rect outerRect, float aspect)
        {
            float width = outerRect.width;
            float height = width / aspect;

            if (height > outerRect.height)
            {
                height = outerRect.height;
                width = height * aspect;
            }

            float x = outerRect.x + (outerRect.width - width) * 0.5f;
            float y = outerRect.y + (outerRect.height - height) * 0.5f;
            return new Rect(x, y, width, height);
        }
    }
}
