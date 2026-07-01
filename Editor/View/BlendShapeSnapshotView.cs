using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public sealed class BlendShapeSnapshotView
    {
        private readonly SnapshotViewLayout m_layout = new SnapshotViewLayout();
        private readonly SnapshotViewStyles m_styles = new SnapshotViewStyles();

        private ReorderableList m_listView;
        private IReadOnlyList<string> m_listSource;
        private Vector2 m_listViewScrollPosition;
        private Vector2 m_diffViewerScrollPosition;
        private int m_pendingSelectedIndex = -1;

        public Vector2 MinimumSize => m_layout.GetMinimumSize();

        public void Dispose()
        {
            m_styles.Dispose();
        }

        public BlendShapeSnapshotViewEvents Draw(Rect windowRect, BlendShapeSnapshotViewState state, Action<Rect> renderPreview)
        {
            BlendShapeSnapshotViewEvents events = new BlendShapeSnapshotViewEvents();
            float contentWidth = Mathf.Max(0f, windowRect.width - SnapshotViewLayout.RootHorizontalPadding * 2f);
            SnapshotViewLayout.LayoutBudget layoutBudget = m_layout.Calculate(windowRect, contentWidth, state.CanSave);

            using (new EditorGUILayout.HorizontalScope(new GUIStyle { padding = new RectOffset((int)SnapshotViewLayout.RootHorizontalPadding, (int)SnapshotViewLayout.RootHorizontalPadding, (int)SnapshotViewLayout.RootTopPadding, (int)SnapshotViewLayout.RootBottomPadding) }))
            {
                using (new EditorGUILayout.VerticalScope())
                {
                    SnapshotTargetFieldView.Draw(state, ref events);
                    GUILayout.Space(SnapshotViewLayout.TargetToPreviewSpacing);
                    SnapshotPreviewView.Draw(contentWidth, layoutBudget.PreviewHeight, state, renderPreview);
                    GUILayout.Space(SnapshotViewLayout.PreviewToBodySpacing);
                    DrawSnapshotBody(contentWidth, layoutBudget.SnapshotBodyHeight, state, ref events);
                    GUILayout.Space(SnapshotViewLayout.BodyToSaveSpacing);
                    SnapshotSaveView.Draw(contentWidth, state, ref events);
                }
            }

            ReadKeyboardEvents(state, ref events);
            return events;
        }

        private void DrawSnapshotBody(float contentWidth, float bodyHeight, BlendShapeSnapshotViewState state, ref BlendShapeSnapshotViewEvents events)
        {
            bodyHeight = Mathf.Max(0f, bodyHeight);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Height(bodyHeight + SnapshotViewLayout.HelpBoxFrameHeight)))
            {
                float halfWidth = Mathf.Floor((contentWidth - 16f) / 2f) - 1f;

                using (new EditorGUILayout.HorizontalScope(GUILayout.Height(bodyHeight)))
                {
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(halfWidth), GUILayout.Height(bodyHeight)))
                    {
                        using (EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(m_listViewScrollPosition, GUILayout.Height(bodyHeight)))
                        {
                            m_listViewScrollPosition = scrollView.scrollPosition;
                            EnsureListView(state);
                            m_listView?.DoLayoutList();
                            if (m_pendingSelectedIndex >= 0)
                            {
                                events.SelectionChanged = true;
                                events.SelectedIndex = m_pendingSelectedIndex;
                                m_pendingSelectedIndex = -1;
                            }
                        }
                    }

                    EditorGUILayout.Space(2f);

                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(halfWidth), GUILayout.Height(bodyHeight)))
                    {
                        DrawDiffAndApply(halfWidth, bodyHeight, state, ref events);
                    }
                }
            }
        }

        private void EnsureListView(BlendShapeSnapshotViewState state)
        {
            if (m_listView != null && ReferenceEquals(m_listSource, state.SnapshotNames))
            {
                m_listView.index = state.SelectedIndex;
                return;
            }

            m_listSource = state.SnapshotNames;
            m_listView = new ReorderableList(new List<string>(state.SnapshotNames), typeof(string), false, true, false, true)
            {
                index = state.SelectedIndex,
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Snapshots"),
                drawElementCallback = (rect, index, isActive, isFocused) =>
                {
                    if (index < 0 || index >= state.SnapshotNames.Count)
                        return;

                    EditorGUI.LabelField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), state.SnapshotNames[index]);
                },
                onSelectCallback = list =>
                {
                    m_pendingSelectedIndex = list.index;
                },
            };
        }

        private void DrawDiffAndApply(float panelWidth, float bodyHeight, BlendShapeSnapshotViewState state, ref BlendShapeSnapshotViewEvents events)
        {
            string[] tabs = { "이전 스냅샷 기준", "현재 상태 기준" };
            int currentTab = state.DiffBasis == SnapshotDiffBasis.PreviousSnapshot ? 0 : 1;
            int nextTab = GUILayout.Toolbar(currentTab, tabs, GUILayout.Height(SnapshotViewLayout.DiffToolbarHeight));
            if (nextTab != currentTab)
            {
                events.DiffBasisChanged = true;
                events.DiffBasis = nextTab == 0 ? SnapshotDiffBasis.PreviousSnapshot : SnapshotDiffBasis.CurrentState;
            }

            bool hasMeshTarget = state.TargetRenderer != null && state.TargetRenderer.sharedMesh != null;
            float applySectionReservedHeight = SnapshotViewLayout.GetApplySectionReservedHeight(panelWidth, hasMeshTarget);
            float diffScrollHeight = Mathf.Max(0f, bodyHeight - SnapshotViewLayout.DiffToolbarHeight - SnapshotViewLayout.SeparatorHeight - SnapshotViewLayout.SeparatorToApplySpacing - applySectionReservedHeight);

            using (EditorGUILayout.ScrollViewScope scrollView = new EditorGUILayout.ScrollViewScope(m_diffViewerScrollPosition, false, false, GUIStyle.none, GUI.skin.verticalScrollbar, GUI.skin.scrollView, GUILayout.Height(diffScrollHeight)))
            {
                m_diffViewerScrollPosition = scrollView.scrollPosition;
                SnapshotDiffView.Draw(state, panelWidth);
            }

            Rect separatorRect = EditorGUILayout.GetControlRect(false, SnapshotViewLayout.SeparatorHeight);
            EditorGUI.DrawRect(separatorRect, new Color(0f, 0f, 0f, 0.15f));

            GUILayout.Space(SnapshotViewLayout.SeparatorToApplySpacing);
            DrawApplySection(panelWidth, applySectionReservedHeight, state, ref events);
        }

        private void DrawApplySection(float panelWidth, float sectionHeight, BlendShapeSnapshotViewState state, ref BlendShapeSnapshotViewEvents events)
        {
            bool hasMeshTarget = state.TargetRenderer != null && state.TargetRenderer.sharedMesh != null;

            using (new EditorGUILayout.VerticalScope(GUILayout.Width(panelWidth), GUILayout.Height(sectionHeight)))
            {
                using (new EditorGUI.DisabledScope(!state.CanApply))
                {
                    GUIStyle applyStyle = state.CanApply ? m_styles.EnabledApplyButton : SnapshotViewStyles.DisabledApplyButton;
                    string buttonLabel = state.SelectedIndex >= 0 ? $"▶  \"{state.PreviewLabel}\" 적용" : "▶  스냅샷을 선택하세요";

                    if (GUILayout.Button(buttonLabel, applyStyle, GUILayout.Width(panelWidth)))
                        events.ApplyRequested = true;
                }

                if (!hasMeshTarget)
                {
                    GUILayout.Space(SnapshotViewLayout.ApplyHintTopSpacing);
                    EditorGUILayout.LabelField(SnapshotViewLayout.ApplyNoTargetHint, SnapshotViewStyles.NoWrapHintStyle, GUILayout.Width(panelWidth), GUILayout.Height(SnapshotViewLayout.ApplyHintHeight(panelWidth)));
                }

                GUILayout.Space(SnapshotViewLayout.ApplyBottomSpacing);
            }
        }

        private static void ReadKeyboardEvents(BlendShapeSnapshotViewState state, ref BlendShapeSnapshotViewEvents events)
        {
            Event current = Event.current;
            if (current.type != EventType.KeyDown)
                return;

            if (state.SelectedIndex < 0 || state.SelectedIndex >= state.SnapshotNames.Count)
                return;

            if (current.keyCode == KeyCode.Delete)
            {
                events.DeleteRequested = true;
                current.Use();
            }
            else if (current.keyCode == KeyCode.F2)
            {
                events.RenameRequested = true;
                current.Use();
            }
        }
    }
}
