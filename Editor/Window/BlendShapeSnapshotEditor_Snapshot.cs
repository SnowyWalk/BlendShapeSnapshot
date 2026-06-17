using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public partial class BlendShapeSnapshotEditor
    {
        // Model
        private int m_selectedListViewIndex;

        // Left
        private ReorderableList m_listView;
        private List<string> m_snapshots;
        private Vector2 m_listViewScrollPosition;

        // Right - Diff View
        private int m_diffViewerTabIndex;
        private Vector2 m_diffViewerScrollPosition;

        // Right - Apply
        private Texture2D m_applyBtnNormalTex;
        private Texture2D m_applyBtnHoverTex;

        // Save
        private string m_snapshotDescription;

        private const float kApplyButtonHeight = 28f;
        private const float kApplyHintTopSpacing = 2f;
        private const float kApplyBottomSpacing = 6f;
        private const float kSeparatorHeight = 1f;
        private const float kSeparatorToApplySpacing = 6f;
        private const float kSaveOuterTopSpacing = 4f;
        private const float kSaveHeaderToDescriptionSpacing = 4f;
        private const float kSaveDescriptionToButtonSpacing = 6f;
        private const float kSaveHintTopSpacing = 2f;
        private const float kSaveBottomSpacing = 16f;
        private const float kSaveOuterBottomSpacing = 20f;
        private const float kSaveButtonHeight = 28f;
        private const float kFallbackHelpBoxFrameHeight = 8f;
        private const float kFallbackHelpBoxPaddingHorizontal = 8f;
        private const string kApplyNoTargetHint = "대상 Mesh를 먼저 지정해야 적용할 수 있습니다.";
        private const string kSaveNoTargetHint = "대상 Mesh를 먼저 지정해야 저장할 수 있습니다.";

        private static float DiffToolbarHeight => EditorGUIUtility.singleLineHeight + 2f;
        private static float HelpBoxFrameHeight => TryGetHelpBoxStyle(out GUIStyle helpBox) ? helpBox.padding.vertical + helpBox.margin.vertical : kFallbackHelpBoxFrameHeight;
        private static float HelpBoxPaddingHorizontal => TryGetHelpBoxStyle(out GUIStyle helpBox) ? helpBox.padding.horizontal : kFallbackHelpBoxPaddingHorizontal;
        private static float ApplyHintHeight(float panelWidth) => TryGetNoWrapHintStyle(out GUIStyle style) ? style.CalcHeight(new GUIContent(kApplyNoTargetHint), panelWidth) : LineHeight;
        private static float SaveHintHeight(float panelWidth) => TryGetNoWrapHintStyle(out GUIStyle style) ? style.CalcHeight(new GUIContent(kSaveNoTargetHint), panelWidth) : LineHeight;
        private static GUIStyle NoWrapHintStyle => new GUIStyle(EditorStyles.centeredGreyMiniLabel) { wordWrap = false, clipping = TextClipping.Clip };
        private static bool TryGetHelpBoxStyle(out GUIStyle style)
        {
            try
            {
                style = EditorStyles.helpBox;
                return style != null;
            }
            catch (System.NullReferenceException)
            {
                style = null;
                return false;
            }
        }

        private static bool TryGetNoWrapHintStyle(out GUIStyle style)
        {
            try
            {
                style = NoWrapHintStyle;
                return style != null;
            }
            catch (System.NullReferenceException)
            {
                style = null;
                return false;
            }
        }

        private static float GetApplySectionReservedHeight(float panelWidth, bool hasMeshTarget)
        {
            float hintHeight = hasMeshTarget ? 0f : kApplyHintTopSpacing + ApplyHintHeight(panelWidth);
            return kApplyButtonHeight + hintHeight + kApplyBottomSpacing;
        }

        private static float GetSaveSectionReservedHeight(float panelWidth, bool hasMeshTarget)
        {
            float helpBoxContentWidth = Mathf.Max(0f, panelWidth - HelpBoxPaddingHorizontal);
            float hintHeight = hasMeshTarget ? 0f : kSaveHintTopSpacing + SaveHintHeight(helpBoxContentWidth);

            return kSaveOuterTopSpacing +
                   HelpBoxFrameHeight +
                   LineHeight +
                   kSaveHeaderToDescriptionSpacing +
                   LineHeight +
                   kSaveDescriptionToButtonSpacing +
                   kSaveButtonHeight +
                   hintHeight +
                   kSaveBottomSpacing +
                   kSaveOuterBottomSpacing;
        }

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

        private void DrawSnapShotListAndDiffViewer(float bodyHeight)
        {
            bodyHeight = Mathf.Max(0f, bodyHeight);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Height(bodyHeight + HelpBoxFrameHeight)))
            {
                float halfWidth = Mathf.Floor((m_contentWidth - 16f) / 2f) - 1f;

                using (new EditorGUILayout.HorizontalScope(GUILayout.Height(bodyHeight)))
                {
                    // 좌측: 리스트 뷰
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(halfWidth), GUILayout.Height(bodyHeight)))
                    {
                        using (var scrollView = new EditorGUILayout.ScrollViewScope(m_listViewScrollPosition, GUILayout.Height(bodyHeight)))
                        {
                            m_listViewScrollPosition = scrollView.scrollPosition;
                            m_listView?.DoLayoutList();
                        }
                    }

                    // Separator
                    EditorGUILayout.Space(2f);

                    // 우측: 변경점 탭
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(halfWidth), GUILayout.Height(bodyHeight)))
                    {
                        m_diffViewerTabIndex = GUILayout.Toolbar(m_diffViewerTabIndex, new[] { "이전 스냅샷 기준", "현재 상태 기준" }, GUILayout.Height(DiffToolbarHeight));
                        bool hasMeshTarget = m_targetMeshRenderer != null && m_targetMeshRenderer.sharedMesh != null;
                        float applySectionReservedHeight = GetApplySectionReservedHeight(halfWidth, hasMeshTarget);
                        float diffScrollHeight = Mathf.Max(0f, bodyHeight - DiffToolbarHeight - kSeparatorHeight - kSeparatorToApplySpacing - applySectionReservedHeight);

                        using (var scrollView = new EditorGUILayout.ScrollViewScope(m_diffViewerScrollPosition, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, GUI.skin.scrollView, GUILayout.Height(diffScrollHeight)))
                        {
                            m_diffViewerScrollPosition = scrollView.scrollPosition;
                            switch (m_diffViewerTabIndex)
                            {
                                case 0:
                                    DrawDiffView(DiffBasis.PreviousSnapshot, halfWidth);
                                    break;
                                case 1:
                                    DrawDiffView(DiffBasis.CurrentState, halfWidth);
                                    break;
                            }
                        }

                        // Apply 버튼 구분선
                        var separatorRect = EditorGUILayout.GetControlRect(false, kSeparatorHeight);
                        EditorGUI.DrawRect(separatorRect, new Color(0f, 0f, 0f, 0.15f));

                        GUILayout.Space(kSeparatorToApplySpacing);

                        DrawApplySection(halfWidth, applySectionReservedHeight);
                    }
                }
            }
        }

        private void DrawApplySection(float panelWidth, float sectionHeight)
        {
            bool hasSelection = m_listView != null && m_listView.index >= 0;
            bool hasMeshTarget = m_targetMeshRenderer != null && m_targetMeshRenderer.sharedMesh != null;
            bool canApply = hasSelection && hasMeshTarget;

            using (new EditorGUILayout.VerticalScope(GUILayout.Width(panelWidth), GUILayout.Height(sectionHeight)))
            {
                using (new EditorGUI.DisabledScope(!canApply))
                {
                    var applyStyle = new GUIStyle(GUI.skin.button) {
                        fontStyle = FontStyle.Bold,
                        fixedHeight = kApplyButtonHeight,
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
                    GUILayout.Space(kApplyHintTopSpacing);
                    EditorGUILayout.LabelField(kApplyNoTargetHint, NoWrapHintStyle, GUILayout.Width(panelWidth), GUILayout.Height(ApplyHintHeight(panelWidth)));
                }

                GUILayout.Space(kApplyBottomSpacing);
            }
        }

        private string GetSelectedSnapshotName()
        {
            // TODO: 실제 스냅샷 리스트에서 이름 반환
            return $"Snapshot_{m_listView.index:D3}";
        }

        private void ApplySnapshot()
        {
            // TODO: 실제 적용 로직
            // TODO: 현재 상태의 변경사항이 날아간다는 경고문 필요
            // 예: m_snapshots[m_listView.index].ApplyTo(m_targetMeshRenderer);
            Debug.Log($"[SnapshotViewer] Applied: {GetSelectedSnapshotName()} → {m_targetMeshRenderer.name}");
        }

        private void DrawSaveField()
        {
            bool canSave = m_targetMeshRenderer != null && m_targetMeshRenderer.sharedMesh != null;
            float saveSectionHeight = GetSaveSectionReservedHeight(m_contentWidth, canSave);

            using (new EditorGUILayout.VerticalScope(GUILayout.Height(saveSectionHeight)))
            {
                GUILayout.Space(kSaveOuterTopSpacing);

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Height(saveSectionHeight - kSaveOuterTopSpacing - kSaveOuterBottomSpacing)))
                {
                    // 헤더
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        var headerStyle = new GUIStyle(EditorStyles.boldLabel) {
                            fontSize = 11,
                        };
                        EditorGUILayout.LabelField("새 스냅샷 저장", headerStyle);
                    }

                    GUILayout.Space(kSaveHeaderToDescriptionSpacing);

                    // 설명 입력
                    const string descLabel = "설명";
                    EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(new GUIContent(descLabel)).x + 8f;
                    m_snapshotDescription = EditorGUILayout.TextField(descLabel, m_snapshotDescription);
                    EditorGUIUtility.labelWidth = 0f;

                    GUILayout.Space(kSaveDescriptionToButtonSpacing);

                    using (new EditorGUI.DisabledScope(!canSave))
                    {
                        var buttonStyle = new GUIStyle(GUI.skin.button) {
                            fontStyle = FontStyle.Bold,
                            fixedHeight = kSaveButtonHeight,
                        };

                        if (GUILayout.Button("현재 상태 스냅샷 저장", buttonStyle, GUILayout.ExpandWidth(true)))
                        {
                            SaveSnapshot();
                        }
                    }

                    // 안내 문구
                    if (!canSave)
                    {
                        GUILayout.Space(kSaveHintTopSpacing);
                        float helpBoxContentWidth = Mathf.Max(0f, m_contentWidth - HelpBoxPaddingHorizontal);
                        EditorGUILayout.LabelField(kSaveNoTargetHint, NoWrapHintStyle, GUILayout.Width(helpBoxContentWidth), GUILayout.Height(SaveHintHeight(helpBoxContentWidth)));
                    }

                    GUILayout.Space(kSaveBottomSpacing);
                }

                GUILayout.Space(kSaveOuterBottomSpacing);
            }
        }

        private void SaveSnapshot()
        {
            if (m_targetMeshRenderer == null || m_targetMeshRenderer.sharedMesh == null)
                return;
            
            // TODO: 이전 스냅샷과 변동이 없으면 안내문 띄워주기 (그래도 스냅샷 찍을까요? 네/아니오)

            m_snapshotRepository.Save(m_targetMeshRenderer, m_snapshotDescription);
            UpdateListView();
            if (IsPreviewing)
            {
                m_listView.Select(1);
                OnSelectListViewItem(1);
            }

            m_snapshotDescription = string.Empty;
            GUI.FocusControl(null);
            Repaint();
        }

        private void UpdateListView()
        {
            m_snapshots = IsPreviewing ? m_snapshotRepository.GetSnapshotLatestOrderedNames(m_targetMeshRenderer) : null;
            m_selectedListViewIndex = 0;

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
                    OnSelectListViewItem(l.index);
                },
            };
        }

        private void OnSelectListViewItem(int index)
        {
            if (!IsPreviewing)
                return;

            if (m_targetMeshRenderer == null || m_targetMeshRenderer.sharedMesh == null)
                return;

            m_selectedListViewIndex = index;

            BlendShapeSnapshotDatabase.BlendShapeSnapshot applySnapshot;
            if (m_selectedListViewIndex == 0)
            {
                applySnapshot = new BlendShapeSnapshotDatabase.BlendShapeSnapshot(m_targetMeshRenderer, m_snapshots[m_selectedListViewIndex]);
            }
            else if (!m_snapshotRepository.TryGetSnapshot(m_targetMeshRenderer, m_snapshots.Count - m_selectedListViewIndex - 1, out applySnapshot))
            {
                return;
            }

            m_snapshotPreviewRenderer.ApplySnapshot(applySnapshot);
        }

        private void HandleDeleteKey()
        {
            Event e = Event.current;

            if (e.type != EventType.KeyDown)
                return;

            if (e.keyCode != KeyCode.Delete)
                return;

            if (m_listView.index < 0 || m_listView.index >= m_snapshots.Count)
                return;

            TryDeleteSelectedItem();

            e.Use();
        }

        private void TryDeleteSelectedItem()
        {
            int index = m_listView.index;

            if (index <= 0 || index >= m_snapshots.Count) // (현재 상태) 행도 삭제못하게 index 0도 리턴처리
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

            // TODO: 데이터베이스에서도 대상 제거

            m_listView.index = Mathf.Clamp(index, 0, m_snapshots.Count - 1);

            Repaint();
        }

        private void HandleRenameKey()
        {
            Event e = Event.current;

            if (e.type != EventType.KeyDown)
                return;

            if (e.keyCode != KeyCode.F2)
                return;

            if (m_listView.index < 0 || m_listView.index >= m_snapshots.Count)
                return;

            // TODO: 이름 변경 박스 띄워서 이름 변경하기
        }
    }
}
