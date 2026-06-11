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
        // Modules
        private readonly SnapshotPreviewRenderer m_snapshotPreviewRenderer = new SnapshotPreviewRenderer();
        private readonly SnapshotRepository m_snapshotRepository = new SnapshotRepository();
        private IEditorWindowModule[] m_modules;

        // View
        private Vector2 m_windowScrollPosition;
        private SkinnedMeshRenderer m_lastTargetMeshRenderer;
        private ReorderableList m_listView;
        private string m_selectedListViewItem;
        private Vector2 m_listViewScrollPosition;
        private int m_diffViewerTabIndex;
        private float m_contentWidth;
        private string m_snapshotDescription;
        
        // Model
        private SkinnedMeshRenderer m_targetMeshRenderer;
        private readonly List<string> m_items = new List<string>();

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

        private void OnGUI()
        {
            using (var scroll = new EditorGUILayout.ScrollViewScope(m_windowScrollPosition, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, GUI.skin.scrollView))
            {
                m_windowScrollPosition = scroll.scrollPosition;
                using (new EditorGUILayout.HorizontalScope(new GUIStyle { padding = new RectOffset(11, 11, 11, 0) }))
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        var contentRect = EditorGUILayout.GetControlRect(GUILayout.Height(0));
                        if (Event.current.type == EventType.Repaint)
                            m_contentWidth = contentRect.width;
                        else if (position.width < m_contentWidth)
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
                m_lastTargetMeshRenderer = m_targetMeshRenderer;
            }
        }

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
                GUI.Label(labelRect, "Overlay Label", EditorStyles.centeredGreyMiniLabel); // TODO: selected snapshot
            }
            EditorGUILayout.LabelField("Selected: ", EditorStyles.boldLabel);
        }

        private void DrawSnapShotListAndDiffViewer()
        {
            using (var vertical = new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                float halfWidth = Mathf.Floor((m_contentWidth - 16f) / 2f) - 1f;
                // Debug.Log($"halfWidth: {halfWidth} {m_contentWidth}");

                using (new EditorGUILayout.HorizontalScope())
                {
                    // 좌측: 리스트 뷰
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(halfWidth), GUILayout.Height(250)))
                    {
                        using (var scrollView = new EditorGUILayout.ScrollViewScope(m_listViewScrollPosition))
                        {
                            m_listViewScrollPosition = scrollView.scrollPosition;
                            m_listView.DoLayoutList();
                        }
                    }

                    // Separator
                    EditorGUILayout.Space(2f);

                    // 우측: 변경점 탭
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(halfWidth)))
                    {
                        m_diffViewerTabIndex = GUILayout.Toolbar(m_diffViewerTabIndex, new[] { "이전 스냅샷 기준", "현재 상태 기준" });
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
        }

        private void DrawSaveField()
        {
            GUILayout.Space(4f);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                // 헤더
                using (new EditorGUILayout.HorizontalScope())
                {
                    var headerStyle = new GUIStyle(EditorStyles.boldLabel) {
                        fontSize = 11,
                    };
                    EditorGUILayout.LabelField("새 스냅샷 저장", headerStyle);
                }

                GUILayout.Space(4f);

                // 설명 입력
                const string descLabel = "설명";
                EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(new GUIContent(descLabel)).x + 8f;
                m_snapshotDescription = EditorGUILayout.TextField(descLabel, m_snapshotDescription);
                EditorGUIUtility.labelWidth = 0f;

                GUILayout.Space(6f);

                // 저장 버튼
                bool canSave = m_targetMeshRenderer != null;

                using (new EditorGUI.DisabledScope(!canSave))
                {
                    var buttonStyle = new GUIStyle(GUI.skin.button) {
                        fontStyle = FontStyle.Bold,
                        fixedHeight = 28f,
                    };

                    if (GUILayout.Button("현재 상태 스냅샷 저장", buttonStyle, GUILayout.ExpandWidth(true)))
                    {
                        SaveSnapshot();
                    }
                }

                // 안내 문구
                if (!canSave)
                {
                    GUILayout.Space(2f);
                    var hintStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel) {
                        wordWrap = true,
                    };
                    EditorGUILayout.LabelField("대상 Mesh를 먼저 지정해야 저장할 수 있습니다.", hintStyle);
                }

                GUILayout.Space(4f);
            }
        }

        private void SaveSnapshot()
        {
            // TODO: 실제 저장 로직
            // 예: m_snapshots.Add(new Snapshot(m_targetMeshRenderer, m_snapshotDescription));
            // m_listView.list = m_snapshots;
            m_snapshotDescription = string.Empty;
            GUI.FocusControl(null);
            Repaint();
        }

        private void InitListView()
        {
            m_listView = new ReorderableList(m_items, typeof(string), false, true, true, true) { // TODO: add/remove 버튼 제거
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, "Snapshots");
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    EditorGUI.LabelField(
                        new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                        m_items[index]
                        );
                },
                onSelectCallback = l =>
                {
                    // TODO: On Selected 구현
                },
            };
        }
        
        private void HandleDeleteKey()
    {
        Event e = Event.current;

        if (e.type != EventType.KeyDown)
            return;

        if (e.keyCode != KeyCode.Delete && e.keyCode != KeyCode.Backspace)
            return;

        if (m_listView.index < 0 || m_listView.index >= m_items.Count)
            return;

        TryDeleteSelectedItem();

        e.Use();
    }

    private void TryDeleteSelectedItem()
    {
        int index = m_listView.index;

        if (index < 0 || index >= m_items.Count)
            return;

        string itemName = m_items[index];

        bool result = EditorUtility.DisplayDialog(
            "삭제 확인",
            $"'{itemName}' 항목을 삭제할까?",
            "삭제",
            "취소"
        );

        if (!result)
            return;

        m_items.RemoveAt(index);

        m_listView.index = Mathf.Clamp(index, 0, m_items.Count - 1);

        Repaint();
    }

        void IEditorWindowOrchestrator.Render()
        {
            Repaint();
        }
    }
}