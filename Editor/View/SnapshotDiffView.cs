using UnityEditor;
using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public static class SnapshotDiffView
    {
        private const float kDiffEpsilon = 0.001f;
        private const float kDiffRowHeight = 48f;
        private const float kDiffRowSpacing = 4f;
        private const float kDiffValueBoxWidth = 44f;
        private const float kDiffArrowWidth = 18f;
        private const float kDiffTrackLeftMargin = 15f;
        private const float kDiffTrackToValueSpacing = 8f;
        private const float kDiffMarkerRadius = 4f;
        private const float kDiffDeltaSegmentHeight = 7f;
        private const float kDiffDeltaSegmentMinLength = 12f;
        private const float kDiffDeltaArrowLength = 7f;
        private const float kDiffDeltaArrowHalfHeight = 5f;
        private static readonly Color kDiffTrackColor = new Color(0f, 0f, 0f, 0.22f);
        private static readonly Color kDiffTrackFillColor = new Color(0f, 0f, 0f, 0.08f);
        private static readonly Color kDiffFromColor = new Color(0.9f, 0.2f, 0.18f, 1f);
        private static readonly Color kDiffToColor = new Color(0.2f, 0.72f, 0.32f, 1f);

        public static void Draw(BlendShapeSnapshotViewState state, float panelWidth)
        {
            if (state.DiffEntries == null || state.DiffEntries.Count == 0)
            {
                DrawEmptyMessage(state.DiffEmptyMessage, panelWidth);
                return;
            }

            foreach (SnapshotDiffEntry entry in state.DiffEntries)
            {
                DrawEntry(entry, panelWidth);
                GUILayout.Space(kDiffRowSpacing);
            }
        }

        private static void DrawEmptyMessage(string message, float panelWidth)
        {
            EditorGUILayout.LabelField(message ?? string.Empty, EditorStyles.centeredGreyMiniLabel, GUILayout.Width(panelWidth));
        }

        private static void DrawEntry(SnapshotDiffEntry entry, float panelWidth)
        {
            Rect rowRect = GUILayoutUtility.GetRect(panelWidth, kDiffRowHeight, GUILayout.Width(panelWidth), GUILayout.Height(kDiffRowHeight));
            Rect labelRect = new Rect(rowRect.x, rowRect.y, rowRect.width, EditorGUIUtility.singleLineHeight);
            GUI.Label(labelRect, entry.Key, EditorStyles.miniBoldLabel);

            float valueAreaWidth = kDiffValueBoxWidth * 2f + kDiffArrowWidth;
            Rect trackRect = new Rect(
                rowRect.x + kDiffTrackLeftMargin,
                rowRect.y + EditorGUIUtility.singleLineHeight + 11f,
                Mathf.Max(24f, rowRect.width - kDiffTrackLeftMargin - valueAreaWidth - kDiffTrackToValueSpacing),
                4f);
            Rect valueRect = new Rect(
                trackRect.xMax + kDiffTrackToValueSpacing,
                rowRect.y + EditorGUIUtility.singleLineHeight + 3f,
                valueAreaWidth,
                EditorGUIUtility.singleLineHeight);

            DrawTrack(trackRect, entry.From, entry.To);
            DrawValues(valueRect, entry.From, entry.To);
        }

        private static void DrawTrack(Rect trackRect, float from, float to)
        {
            EditorGUI.DrawRect(new Rect(trackRect.x, trackRect.y - 2f, trackRect.width, trackRect.height + 4f), kDiffTrackFillColor);
            EditorGUI.DrawRect(trackRect, kDiffTrackColor);

            DrawDeltaSegment(trackRect, from, to);
            DrawMarker(trackRect, from, kDiffFromColor, 0f);
            DrawMarker(trackRect, to, kDiffToColor, Mathf.Abs(to - from) <= kDiffEpsilon ? 1.5f : 0f);
        }

        private static void DrawDeltaSegment(Rect trackRect, float from, float to)
        {
            float fromX = GetTrackX(trackRect, from);
            float toX = GetTrackX(trackRect, to);
            if (Mathf.Abs(to - from) <= kDiffEpsilon)
                return;

            float direction = to > from ? 1f : -1f;
            float startX = fromX;
            float endX = toX;
            if (Mathf.Abs(endX - startX) < 0.5f)
                EnsureVisibleOverlapDeltaSegment(trackRect, direction, ref startX, ref endX);

            Rect segmentRect = new Rect(
                Mathf.Min(startX, endX),
                trackRect.center.y - kDiffDeltaSegmentHeight * 0.5f,
                Mathf.Abs(endX - startX),
                kDiffDeltaSegmentHeight);

            DrawHorizontalGradient(segmentRect, startX <= endX);
            DrawDeltaArrowhead(trackRect.center.y, endX, direction);
        }

        private static float GetTrackX(Rect trackRect, float value)
        {
            float normalized = Mathf.InverseLerp(0f, 100f, Mathf.Clamp(value, 0f, 100f));
            return Mathf.Lerp(trackRect.x, trackRect.xMax, normalized);
        }

        private static void EnsureVisibleOverlapDeltaSegment(Rect trackRect, float direction, ref float startX, ref float endX)
        {
            endX = Mathf.Clamp(startX + direction * kDiffDeltaSegmentMinLength, trackRect.x, trackRect.xMax);
            if (Mathf.Abs(endX - startX) >= kDiffDeltaSegmentMinLength)
                return;

            startX = Mathf.Clamp(endX - direction * kDiffDeltaSegmentMinLength, trackRect.x, trackRect.xMax);
        }

        private static void DrawHorizontalGradient(Rect rect, bool leftToRight)
        {
            if (rect.width <= 0f)
                return;

            int steps = Mathf.Max(1, Mathf.CeilToInt(rect.width));
            for (int i = 0; i < steps; i++)
            {
                float t = steps == 1 ? 1f : i / (steps - 1f);
                Color color = Color.Lerp(kDiffFromColor, kDiffToColor, leftToRight ? t : 1f - t);
                float x = rect.x + rect.width * i / steps;
                float nextX = rect.x + rect.width * (i + 1) / steps;
                EditorGUI.DrawRect(new Rect(x, rect.y, nextX - x + 0.5f, rect.height), color);
            }
        }

        private static void DrawDeltaArrowhead(float centerY, float tipX, float direction)
        {
            Handles.BeginGUI();
            Color previousColor = Handles.color;
            Handles.color = kDiffToColor;
            Handles.DrawAAConvexPolygon(
                new Vector3(tipX, centerY, 0f),
                new Vector3(tipX - direction * kDiffDeltaArrowLength, centerY - kDiffDeltaArrowHalfHeight, 0f),
                new Vector3(tipX - direction * kDiffDeltaArrowLength, centerY + kDiffDeltaArrowHalfHeight, 0f));
            Handles.color = previousColor;
            Handles.EndGUI();
        }

        private static void DrawMarker(Rect trackRect, float value, Color color, float xOffset)
        {
            float x = GetTrackX(trackRect, value) + xOffset;
            Handles.BeginGUI();
            Color previousColor = Handles.color;
            Handles.color = color;
            Handles.DrawSolidDisc(new Vector3(x, trackRect.center.y, 0f), Vector3.forward, kDiffMarkerRadius);
            Handles.color = previousColor;
            Handles.EndGUI();
        }

        private static void DrawValues(Rect valueRect, float from, float to)
        {
            Rect fromRect = new Rect(valueRect.x, valueRect.y, kDiffValueBoxWidth, valueRect.height);
            Rect arrowRect = new Rect(fromRect.xMax, valueRect.y, kDiffArrowWidth, valueRect.height);
            Rect toRect = new Rect(arrowRect.xMax, valueRect.y, kDiffValueBoxWidth, valueRect.height);

            GUI.Box(fromRect, GUIContent.none);
            GUI.Label(fromRect, FormatBlendShapeValue(from), EditorStyles.centeredGreyMiniLabel);
            GUI.Label(arrowRect, "→", EditorStyles.centeredGreyMiniLabel);
            GUI.Box(toRect, GUIContent.none);
            GUI.Label(toRect, FormatBlendShapeValue(to), EditorStyles.centeredGreyMiniLabel);
        }

        private static string FormatBlendShapeValue(float value)
        {
            return value.ToString("0.##");
        }
    }
}
