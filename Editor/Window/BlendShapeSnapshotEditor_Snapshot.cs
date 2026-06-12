using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public partial class BlendShapeSnapshotEditor
    {
        private ReorderableList m_listView;

        private Vector2 m_listViewScrollPosition;
        private int m_diffViewerTabIndex;
        private Vector2 m_diffViewerScrollPosition;
        private string m_snapshotDescription;

        private Texture2D m_applyBtnNormalTex;
        private Texture2D m_applyBtnHoverTex;

        private List<string> m_snapshots;

        private void AllocateButtonTextures()
        {
            m_applyBtnNormalTex = GUIUtils.MakeTex(1, 1, new Color(0.2f, 0.45f, 0.75f, 1f));
            m_applyBtnHoverTex = GUIUtils.MakeTex(1, 1, new Color(0.25f, 0.52f, 0.85f, 1f));
        }

        private void ReleaseButtonTextures()
        {
            DestroyImmediate(m_applyBtnNormalTex);
            DestroyImmediate(m_applyBtnHoverTex);
        }

        private void DrawSnapShotListAndDiffViewer()
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                float halfWidth = Mathf.Floor((m_contentWidth - 16f) / 2f) - 1f;

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
                        using (var scrollView = new EditorGUILayout.ScrollViewScope(m_diffViewerScrollPosition, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, GUI.skin.scrollView, GUILayout.Height(210)))
                        {
                            m_diffViewerScrollPosition = scrollView.scrollPosition;
                            switch (m_diffViewerTabIndex)
                            {
                                case 0:
                                    for (int i = 0; i < 20; i++)
                                    {
                                        EditorGUILayout.LabelField("0번탭");
                                    }
                                    break;
                                case 1:
                                    EditorGUILayout.LabelField("1번탭");
                                    break;
                            }
                        }

                        GUILayout.FlexibleSpace();

                        // Apply 버튼 구분선
                        var separatorRect = EditorGUILayout.GetControlRect(false, 1f);
                        EditorGUI.DrawRect(separatorRect, new Color(0f, 0f, 0f, 0.15f));

                        GUILayout.Space(6f);

                        DrawApplySection(halfWidth);
                    }
                }
            }
        }

        private void DrawApplySection(float panelWidth)
        {
            bool hasSelection = m_listView.index >= 0;
            bool hasMeshTarget = m_targetMeshRenderer != null;
            bool canApply = hasSelection && hasMeshTarget;

            using (new EditorGUI.DisabledScope(!canApply))
            {
                var applyStyle = new GUIStyle(GUI.skin.button) {
                    fontStyle = FontStyle.Bold,
                    fixedHeight = 28f,
                };

                if (canApply)
                {
                    applyStyle.normal.background = m_applyBtnNormalTex;
                    applyStyle.hover.background = m_applyBtnHoverTex;
                    applyStyle.active.background = m_applyBtnNormalTex;
                    applyStyle.normal.textColor = Color.white;
                    applyStyle.hover.textColor = Color.white;
                    applyStyle.active.textColor = Color.white;
                }

                string buttonLabel = hasSelection
                    ? $"▶  \"{GetSelectedSnapshotName()}\" 적용"
                    : "▶  스냅샷을 선택하세요";

                if (GUILayout.Button(buttonLabel, applyStyle, GUILayout.Width(panelWidth)))
                {
                    ApplySnapshot();
                }
            }

            if (!hasMeshTarget)
            {
                GUILayout.Space(2f);
                var hintStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel) { wordWrap = true };
                EditorGUILayout.LabelField("대상 Mesh를 먼저 지정해야 적용할 수 있습니다.", hintStyle, GUILayout.Width(panelWidth));
            }

            GUILayout.Space(6f);
        }

        private string GetSelectedSnapshotName()
        {
            // TODO: 실제 스냅샷 리스트에서 이름 반환
            return $"Snapshot_{m_listView.index:D3}";
        }

        private void ApplySnapshot()
        {
            // TODO: 실제 적용 로직
            // 예: m_snapshots[m_listView.index].ApplyTo(m_targetMeshRenderer);
            Debug.Log($"[SnapshotViewer] Applied: {GetSelectedSnapshotName()} → {m_targetMeshRenderer.name}");
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
            m_snapshotRepository.Save(m_targetMeshRenderer, m_snapshotDescription);

            m_snapshotDescription = string.Empty;
            GUI.FocusControl(null);
            Repaint();
        }

        private void UpdateListView()
        {
            m_snapshots = IsPreviewing ? m_snapshotRepository.GetSnapshotLatestOrderedNames(m_targetMeshRenderer) : null;
            
            m_listView = new ReorderableList(m_snapshots, typeof(string), false, true, false, true) {
                drawHeaderCallback = rect =>
                {
                    EditorGUI.LabelField(rect, "Snapshots");
                },
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), m_snapshots[index]);
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

            if (m_listView.index < 0 || m_listView.index >= m_snapshots.Count)
                return;

            TryDeleteSelectedItem();

            e.Use();
        }

        private void TryDeleteSelectedItem()
        {
            int index = m_listView.index;

            if (index < 0 || index >= m_snapshots.Count)
                return;

            string itemName = m_snapshots[index];

            bool result = EditorUtility.DisplayDialog(
                "삭제 확인",
                $"'{itemName}' 항목을 삭제할까?",
                "삭제",
                "취소"
                );

            if (!result)
                return;

            m_snapshots.RemoveAt(index);

            m_listView.index = Mathf.Clamp(index, 0, m_snapshots.Count - 1);

            Repaint();
        }
    }
}