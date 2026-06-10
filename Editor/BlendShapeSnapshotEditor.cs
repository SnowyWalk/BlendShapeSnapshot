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
        private Vector2 m_windowScrollPosition;
        private SkinnedMeshRenderer m_lastTargetMeshRenderer;
        private ReorderableList m_listView;
        private string m_selectedListViewItem;
        private Vector2 m_listViewScrollPosition;
        private int m_diffViewerTabIndex;

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
            "BlendShape Snapshot 3",
            "BlendShape Snapshot 1",
            "BlendShape Snapshot 2",
            "BlendShape Snapshot 3",
            "BlendShape Snapshot 1",
            "BlendShape Snapshot 2",
            "BlendShape Snapshot 3",
            "BlendShape Snapshot 1",
            "BlendShape Snapshot 2",
            "BlendShape Snapshot 3",
            "BlendShape Snapshot 1",
            "BlendShape Snapshot 2",
            "BlendShape Snapshot 3",
            "BlendShape Snapshot 1",
            "BlendShape Snapshot 2",
            "BlendShape Snapshot 3",
            "BlendShape Snapshot 1",
            "BlendShape Snapshot 2",
            "BlendShape Snapshot 3",
        };

        private void OnGUI()
        {
            using (var scroll = new EditorGUILayout.ScrollViewScope(m_windowScrollPosition))
            {
                m_windowScrollPosition = scroll.scrollPosition;
                using (new EditorGUILayout.HorizontalScope(new GUIStyle { padding = new RectOffset(11, 11, 11, 11) }))
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        DrawContent();
                    }
                }
            }
        }

        private void DrawContent()
        {
            // SMR 넣는 칸
            DrawSkinnedMeshRendererTarget();

            GUILayout.Space(6f);

            // 프리뷰 영역
            DrawSnapShotPreview();

            GUILayout.Space(12f);

            // 스냅샷 섹션
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                // 실제 레이아웃 너비를 0높이 rect로 측정
                float totalWidth = EditorGUILayout.GetControlRect(GUILayout.Height(0)).width
                    - 4f; // helpBox 내부 좌우 패딩
                float halfWidth = Mathf.Floor(totalWidth / 2f);

                using (new EditorGUILayout.HorizontalScope())
                {
                    // 좌측: 리스트 뷰
                    using (var scrollView = new EditorGUILayout.ScrollViewScope(
                        m_listViewScrollPosition,
                        GUILayout.Height(250),
                        GUILayout.Width(halfWidth)))
                    {
                        m_listViewScrollPosition = scrollView.scrollPosition;
                        m_listView.DoLayoutList();
                    }

                    // 우측: 변경점 탭
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(halfWidth)))
                    {
                        m_diffViewerTabIndex = GUILayout.Toolbar(
                            m_diffViewerTabIndex,
                            new[] { "이전 스냅샷 기준", "현재 상태 기준" });
                        switch (m_diffViewerTabIndex)
                        {
                            case 0:
                                EditorGUILayout.LabelField("0번탭");
                                break;
                            case 1:
                                EditorGUILayout.LabelField("1번탭");
                                break;
                        }
                    }
                }
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
            m_listView = new ReorderableList(items, typeof(string), false, true, true, true) { // TODO: add/remove 버튼 제거
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