using UnityEditor;
using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    internal static class SnapshotSaveView
    {
        public static void Draw(float contentWidth, BlendShapeSnapshotViewState state, ref BlendShapeSnapshotViewEvents events)
        {
            float saveSectionHeight = SnapshotViewLayout.GetSaveSectionReservedHeight(contentWidth, state.CanSave);

            using (new EditorGUILayout.VerticalScope(GUILayout.Height(saveSectionHeight)))
            {
                GUILayout.Space(SnapshotViewLayout.SaveOuterTopSpacing);

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox, GUILayout.Height(saveSectionHeight - SnapshotViewLayout.SaveOuterTopSpacing - SnapshotViewLayout.SaveOuterBottomSpacing)))
                {
                    EditorGUILayout.LabelField("새 스냅샷 저장", SnapshotViewStyles.SaveHeader);

                    GUILayout.Space(SnapshotViewLayout.SaveHeaderToDescriptionSpacing);

                    const string descLabel = "설명";
                    EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(new GUIContent(descLabel)).x + 8f;
                    string nextDescription = EditorGUILayout.TextField(descLabel, state.SnapshotDescription);
                    EditorGUIUtility.labelWidth = 0f;

                    if (nextDescription != state.SnapshotDescription)
                    {
                        events.DescriptionChanged = true;
                        events.SnapshotDescription = nextDescription;
                    }

                    GUILayout.Space(SnapshotViewLayout.SaveDescriptionToButtonSpacing);

                    using (new EditorGUI.DisabledScope(!state.CanSave))
                    {
                        if (GUILayout.Button("현재 상태 스냅샷 저장", SnapshotViewStyles.SaveButton, GUILayout.ExpandWidth(true)))
                            events.SaveRequested = true;
                    }

                    if (!state.CanSave)
                    {
                        GUILayout.Space(SnapshotViewLayout.SaveHintTopSpacing);
                        float helpBoxContentWidth = Mathf.Max(0f, contentWidth - SnapshotViewLayout.HelpBoxPaddingHorizontal);
                        EditorGUILayout.LabelField(SnapshotViewLayout.SaveNoTargetHint, SnapshotViewStyles.NoWrapHintStyle, GUILayout.Width(helpBoxContentWidth), GUILayout.Height(SnapshotViewLayout.SaveHintHeight(helpBoxContentWidth)));
                    }

                    GUILayout.Space(SnapshotViewLayout.SaveBottomSpacing);
                }

                GUILayout.Space(SnapshotViewLayout.SaveOuterBottomSpacing);
            }
        }
    }
}
