using UnityEditor;
using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    internal interface IEditorWindowModule
    {
        public void OnEnable();
        public void OnDisable();
    }

    public class BlendShapeSnapshotEditor : EditorWindow
    {
        private readonly SnapshotPreviewRenderer m_snapshotPreviewRenderer = new SnapshotPreviewRenderer();
        private readonly SnapshotRepository m_snapshotRepository = new SnapshotRepository();

        private SkinnedMeshRenderer m_targetMeshRenderer;

        private IEditorWindowModule[] m_providers;
        
        // For Window
        private SkinnedMeshRenderer m_lastTargetMeshRenderer;
        
        private bool m_isPreviewing => m_targetMeshRenderer != null;

        [MenuItem("Tools/Blend Shape Snapshot Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<BlendShapeSnapshotEditor>("Blend Shape Snapshot Manager");
            window.minSize = new Vector2(420f, 320f);
            window.Show();
        }

        private void OnEnable()
        {
            m_providers = new IEditorWindowModule[] { m_snapshotPreviewRenderer, m_snapshotRepository };

            foreach (IEditorWindowModule provider in m_providers)
            {
                provider.OnEnable();
            }
        }

        private void OnDisable()
        {
            foreach (IEditorWindowModule provider in m_providers)
            {
                provider.OnDisable();
            }
        }

        private void OnGUI()
        {
            {
                const string label = "대상 Mesh";
                EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(new GUIContent(label)).x + 8f;
                m_targetMeshRenderer = EditorGUILayout.ObjectField(label, m_targetMeshRenderer, typeof(SkinnedMeshRenderer), true, GUILayout.ExpandWidth(true)) as SkinnedMeshRenderer;
                EditorGUIUtility.labelWidth = 0f;

                if (m_lastTargetMeshRenderer != m_targetMeshRenderer)
                {
                    if (m_targetMeshRenderer)
                        OnAllocateSkinnedMeshRenderer();
                    else
                        OnReleaseSkinnedMeshRenderer();
                    m_lastTargetMeshRenderer = m_targetMeshRenderer;
                }
            }

            if (m_isPreviewing)
            {
                Rect previewRect = GUILayoutUtility.GetAspectRect(1f, GUILayout.ExpandWidth(true));

                if (Event.current.type == EventType.Repaint)
                    m_snapshotPreviewRenderer.Render(previewRect);
            }
        }

        private void OnAllocateSkinnedMeshRenderer()
        {
            // TODO: Repository Load / Select First One
            Debug.Log("[OnAllocateSkinnedMeshRenderer]");
            m_snapshotPreviewRenderer.Init(m_targetMeshRenderer);
        }
        
        private void OnReleaseSkinnedMeshRenderer()
        {
            // TODO: Repository Unload / Preview Release
            Debug.Log("[OnReleaseSkinnedMeshRenderer]");
        }
    }
}
