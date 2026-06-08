using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
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
        private ReorderableList m_listView;
        private string m_selectedListViewItem;
        private float m_listViewScrollPosition;

        // Properties
        private bool IsPreviewing => m_targetMeshRenderer != null;

        [MenuItem("Tools/BlendShape Snapshot Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<BlendShapeSnapshotEditor>("BlendShape Snapshot Manager");
            window.minSize = new Vector2(420f, 320f);
            window.Show();
        }

        private void OnEnable()
        {
            InitListView();

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
            m_lastTargetMeshRenderer = null;
        }

        private readonly List<string> items = new() {
            "BlendShape Snapshot 1",
            "BlendShape Snapshot 2",
            "BlendShape Snapshot 3"
        };

        private void OnGUI()
        {
            GUILayout.Space(11f);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(6f);
                using (new EditorGUILayout.VerticalScope())
                {
                    DrawContent();
                }
                GUILayout.Space(6f);
            }
            GUILayout.Space(11f);
        }

        private void DrawContent()
        {
            // SMR 넣는 칸
            DrawSkinnedMeshRendererTarget();

            GUILayout.Space(6f);
            
            // 프리뷰 영역
            DrawSnapShotPreview();

            GUILayout.Space(12f);
            
            // 리스트 뷰
            using (new EditorGUILayout.ScrollViewScope(new Vector2(0f, m_listViewScrollPosition)))
            {
                m_listView.DoLayoutList();
            }

            GUILayout.Space(6f);
            
            // TODO: 나머지 기능들 작성
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
                m_lastTargetMeshRenderer = m_targetMeshRenderer;
            }
        }

        private void DrawSnapShotPreview()
        {
            Rect previewRect = GUILayoutUtility.GetAspectRect(16f / 9f, GUILayout.ExpandWidth(true));
            GUI.Box(previewRect.Inset(0f), GUIContent.none);
            if (IsPreviewing)
            {
                if (Event.current.type == EventType.Repaint)
                    m_snapshotPreviewRenderer.Render(previewRect);
                GUI.Label(previewRect, "Overlay Label", EditorStyles.centeredGreyMiniLabel);
            }
            EditorGUILayout.LabelField("Selected: ", EditorStyles.boldLabel);
        }

        private void InitListView()
        {
            m_listView = new ReorderableList(items, typeof(string), false, true, false, false) {
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, "Snapshots");
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    EditorGUI.LabelField(
                        new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        items[index]
                        );
                },
                onSelectCallback = l =>
                {
                    // TODO: On Selected 구현
                },
            };
        }

        void IEditorWindowOrchestrator.Render()
        {
            Repaint();
        }
    }
}