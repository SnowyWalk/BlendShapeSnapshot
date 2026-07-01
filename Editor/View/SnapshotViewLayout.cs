using UnityEditor;
using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public sealed class SnapshotViewLayout
    {
        public const float WindowMinWidth = 470f;
        public const float RootHorizontalPadding = 11f;
        public const float RootTopPadding = 11f;
        public const float RootBottomPadding = 8f;
        public const float TargetToPreviewSpacing = 6f;
        public const float PreviewMinHeight = 120f;
        public const float PreviewExtraHeightRatio = 0.65f;
        public const float PreviewAspect = 16f / 9f;
        public const float PreviewToBodySpacing = 12f;
        public const float SnapshotBodyMinHeight = 250f;
        public const float BodyToSaveSpacing = 6f;

        public const float ApplyButtonHeight = 28f;
        public const float ApplyHintTopSpacing = 2f;
        public const float ApplyBottomSpacing = 6f;
        public const float SeparatorHeight = 1f;
        public const float SeparatorToApplySpacing = 6f;
        public const float SaveOuterTopSpacing = 4f;
        public const float SaveHeaderToDescriptionSpacing = 4f;
        public const float SaveDescriptionToButtonSpacing = 6f;
        public const float SaveHintTopSpacing = 2f;
        public const float SaveBottomSpacing = 16f;
        public const float SaveOuterBottomSpacing = 20f;
        public const float SaveButtonHeight = 28f;

        private const float kFallbackHelpBoxFrameHeight = 8f;
        private const float kFallbackHelpBoxPaddingHorizontal = 8f;
        private const string kApplyNoTargetHint = "대상 Mesh를 먼저 지정해야 적용할 수 있습니다.";
        private const string kSaveNoTargetHint = "대상 Mesh를 먼저 지정해야 저장할 수 있습니다.";

        public static float LineHeight => EditorGUIUtility.singleLineHeight;
        public static float PreviewLabelHeight => LineHeight;
        public static float DiffToolbarHeight => LineHeight + 2f;
        public static float HelpBoxFrameHeight => TryGetHelpBoxStyle(out GUIStyle helpBox) ? helpBox.padding.vertical + helpBox.margin.vertical : kFallbackHelpBoxFrameHeight;
        public static float HelpBoxPaddingHorizontal => TryGetHelpBoxStyle(out GUIStyle helpBox) ? helpBox.padding.horizontal : kFallbackHelpBoxPaddingHorizontal;
        public static string ApplyNoTargetHint => kApplyNoTargetHint;
        public static string SaveNoTargetHint => kSaveNoTargetHint;

        public Vector2 GetMinimumSize()
        {
            float minContentWidth = WindowMinWidth - RootHorizontalPadding * 2f;
            return new Vector2(WindowMinWidth, ComputeMinimumWindowHeight(minContentWidth));
        }

        public LayoutBudget Calculate(Rect windowRect, float contentWidth, bool canSave)
        {
            float contentHeight = Mathf.Max(0f, windowRect.height - RootTopPadding - RootBottomPadding);
            float saveSectionHeight = GetSaveSectionReservedHeight(contentWidth, canSave);
            float minimumContentHeight =
                LineHeight +
                TargetToPreviewSpacing +
                PreviewMinHeight +
                PreviewLabelHeight +
                PreviewToBodySpacing +
                SnapshotBodyMinHeight +
                HelpBoxFrameHeight +
                BodyToSaveSpacing +
                saveSectionHeight;

            float extraHeight = Mathf.Max(0f, contentHeight - minimumContentHeight);
            float desiredPreviewHeight = GetDesiredPreviewHeight(contentWidth);
            float previewExtraHeight = Mathf.Min(extraHeight * PreviewExtraHeightRatio, Mathf.Max(0f, desiredPreviewHeight - PreviewMinHeight));
            float previewHeight = PreviewMinHeight + previewExtraHeight;
            float topBlockHeight =
                LineHeight +
                TargetToPreviewSpacing +
                previewHeight +
                PreviewLabelHeight +
                PreviewToBodySpacing;

            float snapshotSectionHeight = contentHeight - topBlockHeight - BodyToSaveSpacing - saveSectionHeight;
            float snapshotBodyHeight = Mathf.Max(SnapshotBodyMinHeight, snapshotSectionHeight - HelpBoxFrameHeight);

            return new LayoutBudget(previewHeight, snapshotBodyHeight);
        }

        public static float GetApplySectionReservedHeight(float panelWidth, bool hasMeshTarget)
        {
            float hintHeight = hasMeshTarget ? 0f : ApplyHintTopSpacing + ApplyHintHeight(panelWidth);
            return ApplyButtonHeight + hintHeight + ApplyBottomSpacing;
        }

        public static float GetSaveSectionReservedHeight(float panelWidth, bool hasMeshTarget)
        {
            float helpBoxContentWidth = Mathf.Max(0f, panelWidth - HelpBoxPaddingHorizontal);
            float hintHeight = hasMeshTarget ? 0f : SaveHintTopSpacing + SaveHintHeight(helpBoxContentWidth);

            return SaveOuterTopSpacing +
                   HelpBoxFrameHeight +
                   LineHeight +
                   SaveHeaderToDescriptionSpacing +
                   LineHeight +
                   SaveDescriptionToButtonSpacing +
                   SaveButtonHeight +
                   hintHeight +
                   SaveBottomSpacing +
                   SaveOuterBottomSpacing;
        }

        public static Rect FitAspect(Rect outerRect, float aspect)
        {
            float width = outerRect.width;
            float height = width / aspect;

            if (height > outerRect.height)
            {
                height = outerRect.height;
                width = height * aspect;
            }

            float x = outerRect.x + (outerRect.width - width) * 0.5f;
            float y = outerRect.y + (outerRect.height - height) * 0.5f;
            return new Rect(x, y, width, height);
        }

        public static float ApplyHintHeight(float panelWidth)
        {
            return TryGetNoWrapHintStyle(out GUIStyle style) ? style.CalcHeight(new GUIContent(kApplyNoTargetHint), panelWidth) : LineHeight;
        }

        public static float SaveHintHeight(float panelWidth)
        {
            return TryGetNoWrapHintStyle(out GUIStyle style) ? style.CalcHeight(new GUIContent(kSaveNoTargetHint), panelWidth) : LineHeight;
        }

        private static float ComputeMinimumWindowHeight(float contentWidth)
        {
            return RootTopPadding +
                   LineHeight +
                   TargetToPreviewSpacing +
                   PreviewMinHeight +
                   PreviewLabelHeight +
                   PreviewToBodySpacing +
                   SnapshotBodyMinHeight +
                   HelpBoxFrameHeight +
                   BodyToSaveSpacing +
                   GetSaveSectionReservedHeight(contentWidth, false) +
                   RootBottomPadding;
        }

        private static float GetDesiredPreviewHeight(float contentWidth)
        {
            float aspectHeight = contentWidth / PreviewAspect;
            SceneView sceneView = SceneView.lastActiveSceneView;
            return sceneView == null ? aspectHeight : Mathf.Min(aspectHeight, sceneView.position.height);
        }

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
                style = SnapshotViewStyles.NoWrapHintStyle;
                return style != null;
            }
            catch (System.NullReferenceException)
            {
                style = null;
                return false;
            }
        }

        public readonly struct LayoutBudget
        {
            public readonly float PreviewHeight;
            public readonly float SnapshotBodyHeight;

            public LayoutBudget(float previewHeight, float snapshotBodyHeight)
            {
                PreviewHeight = previewHeight;
                SnapshotBodyHeight = snapshotBodyHeight;
            }
        }
    }
}
