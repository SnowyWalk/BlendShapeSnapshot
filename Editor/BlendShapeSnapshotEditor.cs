using System;
using UnityEditor;
using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    internal interface IEditorWindowModule
    {
        public void Initialize(IEditorWindowOrchestrator orchestrator);
        public void OnEnable();
        public void OnDisable();
    }

    internal interface IEditorWindowOrchestrator
    {
        public void Render();
    }

    public class BlendShapeSnapshotEditor : EditorWindow, IEditorWindowOrchestrator
    {
        private readonly SnapshotPreviewRenderer m_snapshotPreviewRenderer = new SnapshotPreviewRenderer();
        private readonly SnapshotRepository m_snapshotRepository = new SnapshotRepository();

        private SkinnedMeshRenderer m_targetMeshRenderer;

        private IEditorWindowModule[] m_modules;
        
        // For Window
        private SkinnedMeshRenderer m_lastTargetMeshRenderer;
        
        // Properties
        private bool IsPreviewing => m_targetMeshRenderer != null;

        [MenuItem("Tools/BlendShape Snapshot Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<BlendShapeSnapshotEditor>("Blend Shape Snapshot Manager");
            window.minSize = new Vector2(420f, 320f);
            window.Show();
        }

        private void OnEnable()
        {
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
        }

        private void OnGUI()
        {
            // SMR 넣는 칸
            {
                const string label = "대상 Mesh";
                EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(new GUIContent(label)).x + 8f;
                m_targetMeshRenderer = EditorGUILayout.ObjectField(label, m_targetMeshRenderer, typeof(SkinnedMeshRenderer), true, GUILayout.ExpandWidth(true)) as SkinnedMeshRenderer;
                EditorGUIUtility.labelWidth = 0f;

                if (m_lastTargetMeshRenderer != m_targetMeshRenderer)
                {
                    HandleTargetChanged();
                    m_lastTargetMeshRenderer = m_targetMeshRenderer;
                }
            }

            // 프리뷰 영역
            if (IsPreviewing)
            {
                Rect previewRect = GUILayoutUtility.GetAspectRect(1f, GUILayout.ExpandWidth(true));

                if (Event.current.type == EventType.Repaint)
                    m_snapshotPreviewRenderer.Render(previewRect);
            }
            
            // TODO: 나머지 기능들 작성
        }
        
        private void HandleTargetChanged()
        {
            m_snapshotPreviewRenderer.CreatePreviewTarget(m_targetMeshRenderer);
        }

        void IEditorWindowOrchestrator.Render()
        {
            Repaint();
        }
    }
}
