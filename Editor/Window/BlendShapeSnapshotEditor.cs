using UnityEditor;
using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public partial class BlendShapeSnapshotEditor : EditorWindow, IEditorWindowOrchestrator
    {
        // Modules
        private readonly SnapshotPreviewRenderer m_snapshotPreviewRenderer = new SnapshotPreviewRenderer();
        private readonly SnapshotRepository m_snapshotRepository = new SnapshotRepository();
        private IEditorWindowModule[] m_modules;

        // View
        private Vector2 m_windowScrollPosition;
        private SkinnedMeshRenderer m_lastTargetMeshRenderer;
        private float m_contentWidth;

        // Model
        private SkinnedMeshRenderer m_targetMeshRenderer;

        // Properties
        private bool IsPreviewing => m_targetMeshRenderer != null;

        [MenuItem("Tools/BlendShape Snapshot Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<BlendShapeSnapshotEditor>("BlendShape Snapshot Manager");
            window.minSize = new Vector2(470f, 320f);
            window.Show();
        }

        private void OnEnable()
        {
            UpdateListView();
            AllocateButtonTextures();

            m_modules = new IEditorWindowModule[] { m_snapshotPreviewRenderer, m_snapshotRepository };
            foreach (IEditorWindowModule provider in m_modules)
            {
                provider.Initialize(this);
                provider.OnEnable();
            }
        }

        private void OnDisable()
        {
            foreach (IEditorWindowModule provider in m_modules)
            {
                provider.OnDisable();
            }

            ReleaseButtonTextures();

            m_lastTargetMeshRenderer = null;
        }

        private void OnGUI()
        {
            using (var scroll = new EditorGUILayout.ScrollViewScope(m_windowScrollPosition, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, GUI.skin.scrollView))
            {
                m_windowScrollPosition = scroll.scrollPosition;
                using (new EditorGUILayout.HorizontalScope(new GUIStyle { padding = new RectOffset(11, 11, 11, 0) }))
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        m_contentWidth = position.width - 31f;

                        DrawContent();
                    }
                }
            }

            HandleDeleteKey();
        }

        private void DrawContent()
        {
            DrawSkinnedMeshRendererTarget(); // SMR 넣는 칸

            GUILayout.Space(6f);

            DrawSnapShotPreview(); // 프리뷰 영역

            GUILayout.Space(12f);

            DrawSnapShotListAndDiffViewer(); // 스냅샷 섹션 (리스트 & diff뷰어)

            GUILayout.Space(6f);

            DrawSaveField(); // Save
        }

        private void DrawSkinnedMeshRendererTarget()
        {
            const string label = "대상 Mesh";
            EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(new GUIContent(label)).x + 8f;
            m_targetMeshRenderer = EditorGUILayout.ObjectField(label, m_targetMeshRenderer, typeof(SkinnedMeshRenderer), true, GUILayout.ExpandWidth(true)) as SkinnedMeshRenderer;
            EditorGUIUtility.labelWidth = 0f;

            if (m_lastTargetMeshRenderer != m_targetMeshRenderer)
            {
                m_snapshotPreviewRenderer.CreatePreviewTarget(m_targetMeshRenderer);
                UpdateListView();
                m_lastTargetMeshRenderer = m_targetMeshRenderer;
            }
        }

        void IEditorWindowOrchestrator.Render()
        {
            Repaint();
        }
    }
}