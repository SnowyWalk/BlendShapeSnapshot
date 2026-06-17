using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public partial class BlendShapeSnapshotEditor
    {
        private const float kDiffEpsilon = 0.001f;
        private const float kDiffRowHeight = 48f;
        private const float kDiffRowSpacing = 4f;
        private const float kDiffValueBoxWidth = 44f;
        private const float kDiffArrowWidth = 18f;
        private const float kDiffTrackLeftMargin = 15f;
        private const float kDiffTrackToValueSpacing = 8f;
        private const float kDiffMarkerRadius = 4f;
        private static readonly Color kDiffTrackColor = new Color(0f, 0f, 0f, 0.22f);
        private static readonly Color kDiffTrackFillColor = new Color(0f, 0f, 0f, 0.08f);
        private static readonly Color kDiffFromColor = new Color(0.9f, 0.2f, 0.18f, 1f);
        private static readonly Color kDiffToColor = new Color(0.2f, 0.72f, 0.32f, 1f);

        private enum DiffBasis
        {
            PreviousSnapshot,
            CurrentState,
        }

        private readonly struct BlendShapeDiffEntry
        {
            public readonly string Key;
            public readonly float From;
            public readonly float To;

            public BlendShapeDiffEntry(string key, float from, float to)
            {
                Key = key;
                From = from;
                To = to;
            }
        }

        private void DrawDiffView(DiffBasis basis, float panelWidth)
        {
            List<BlendShapeDiffEntry> entries = BuildDiffEntries(basis, out string emptyMessage);

            if (!string.IsNullOrEmpty(emptyMessage))
            {
                DrawDiffEmptyMessage(emptyMessage, panelWidth);
                return;
            }

            foreach (BlendShapeDiffEntry entry in entries)
            {
                DrawDiffEntry(entry, panelWidth);
                GUILayout.Space(kDiffRowSpacing);
            }
        }

        private List<BlendShapeDiffEntry> BuildDiffEntries(DiffBasis basis, out string emptyMessage)
        {
            emptyMessage = null;

            if (m_targetMeshRenderer == null)
            {
                emptyMessage = "대상 Mesh를 먼저 지정하세요.";
                return null;
            }

            Mesh mesh = m_targetMeshRenderer.sharedMesh;
            if (mesh == null)
            {
                emptyMessage = "대상 Mesh에 sharedMesh가 없습니다.";
                return null;
            }

            Dictionary<string, float> toValues = GetSelectedTargetValues(mesh, out string targetMessage);
            if (toValues == null)
            {
                emptyMessage = targetMessage;
                return null;
            }

            Dictionary<string, float> fromValues = GetBaselineValues(basis, mesh, out string baselineMessage);
            if (fromValues == null)
            {
                emptyMessage = baselineMessage;
                return null;
            }

            List<BlendShapeDiffEntry> entries = BuildOrderedDiffEntries(mesh, fromValues, toValues);
            if (entries.Count == 0)
                emptyMessage = "변경된 BlendShape 값이 없습니다.";

            return entries;
        }

        private Dictionary<string, float> GetSelectedTargetValues(Mesh mesh, out string message)
        {
            message = null;

            if (m_selectedListViewIndex <= 0)
                return GetCurrentBlendShapeValues(mesh);

            int snapshotIndex = GetSelectedSnapshotDatabaseIndex();
            if (!m_snapshotRepository.TryGetSnapshot(m_targetMeshRenderer, snapshotIndex, out BlendShapeSnapshotDatabase.BlendShapeSnapshot snapshot))
            {
                message = "선택한 스냅샷을 찾을 수 없습니다.";
                return null;
            }

            return GetSnapshotValues(snapshot);
        }

        private Dictionary<string, float> GetBaselineValues(DiffBasis basis, Mesh mesh, out string message)
        {
            message = null;

            if (basis == DiffBasis.CurrentState)
                return GetCurrentBlendShapeValues(mesh);

            int snapshotCount = m_snapshotRepository.GetSnapshotCount(m_targetMeshRenderer);
            if (snapshotCount <= 0)
            {
                message = "비교할 이전 스냅샷이 없습니다.";
                return null;
            }

            int previousSnapshotIndex = m_selectedListViewIndex <= 0
                ? snapshotCount - 1
                : GetSelectedSnapshotDatabaseIndex() - 1;

            if (!m_snapshotRepository.TryGetSnapshot(m_targetMeshRenderer, previousSnapshotIndex, out BlendShapeSnapshotDatabase.BlendShapeSnapshot snapshot))
            {
                message = "비교할 이전 스냅샷이 없습니다.";
                return null;
            }

            return GetSnapshotValues(snapshot);
        }

        private int GetSelectedSnapshotDatabaseIndex()
        {
            int snapshotCount = m_snapshotRepository.GetSnapshotCount(m_targetMeshRenderer);
            return snapshotCount - m_selectedListViewIndex;
        }

        private Dictionary<string, float> GetCurrentBlendShapeValues(Mesh mesh)
        {
            Dictionary<string, float> values = new Dictionary<string, float>();
            int blendShapeCount = mesh.blendShapeCount;

            for (int i = 0; i < blendShapeCount; i++)
            {
                values[mesh.GetBlendShapeName(i)] = m_targetMeshRenderer.GetBlendShapeWeight(i);
            }

            return values;
        }

        private static Dictionary<string, float> GetSnapshotValues(BlendShapeSnapshotDatabase.BlendShapeSnapshot snapshot)
        {
            Dictionary<string, float> values = new Dictionary<string, float>();
            foreach ((string blendShapeName, float value) in snapshot.BlendShapeWeights)
            {
                values[blendShapeName] = value;
            }

            return values;
        }

        private static List<BlendShapeDiffEntry> BuildOrderedDiffEntries(Mesh mesh, Dictionary<string, float> fromValues, Dictionary<string, float> toValues)
        {
            List<BlendShapeDiffEntry> entries = new List<BlendShapeDiffEntry>();
            HashSet<string> handledKeys = new HashSet<string>();

            for (int i = 0; i < mesh.blendShapeCount; i++)
            {
                string key = mesh.GetBlendShapeName(i);
                handledKeys.Add(key);
                TryAddDiffEntry(entries, key, fromValues, toValues);
            }

            foreach (string key in fromValues.Keys.Concat(toValues.Keys).Distinct().Where(key => !handledKeys.Contains(key)).OrderBy(key => key))
            {
                TryAddDiffEntry(entries, key, fromValues, toValues);
            }

            return entries;
        }

        private static void TryAddDiffEntry(List<BlendShapeDiffEntry> entries, string key, Dictionary<string, float> fromValues, Dictionary<string, float> toValues)
        {
            float from = fromValues.TryGetValue(key, out float fromValue) ? fromValue : 0f;
            float to = toValues.TryGetValue(key, out float toValue) ? toValue : 0f;

            if (Mathf.Abs(to - from) <= kDiffEpsilon)
                return;

            entries.Add(new BlendShapeDiffEntry(key, from, to));
        }

        private static void DrawDiffEmptyMessage(string message, float panelWidth)
        {
            EditorGUILayout.LabelField(message, EditorStyles.centeredGreyMiniLabel, GUILayout.Width(panelWidth));
        }

        private static void DrawDiffEntry(BlendShapeDiffEntry entry, float panelWidth)
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

            DrawDiffTrack(trackRect, entry.From, entry.To);
            DrawDiffValues(valueRect, entry.From, entry.To);
        }

        private static void DrawDiffTrack(Rect trackRect, float from, float to)
        {
            EditorGUI.DrawRect(new Rect(trackRect.x, trackRect.y - 2f, trackRect.width, trackRect.height + 4f), kDiffTrackFillColor);
            EditorGUI.DrawRect(trackRect, kDiffTrackColor);

            DrawDiffMarker(trackRect, from, kDiffFromColor, 0f);
            DrawDiffMarker(trackRect, to, kDiffToColor, Mathf.Abs(to - from) <= kDiffEpsilon ? 1.5f : 0f);
        }

        private static void DrawDiffMarker(Rect trackRect, float value, Color color, float xOffset)
        {
            float normalized = Mathf.InverseLerp(0f, 100f, Mathf.Clamp(value, 0f, 100f));
            float x = Mathf.Lerp(trackRect.x, trackRect.xMax, normalized) + xOffset;
            Handles.BeginGUI();
            Color previousColor = Handles.color;
            Handles.color = color;
            Handles.DrawSolidDisc(new Vector3(x, trackRect.center.y, 0f), Vector3.forward, kDiffMarkerRadius);
            Handles.color = previousColor;
            Handles.EndGUI();
        }

        private static void DrawDiffValues(Rect valueRect, float from, float to)
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
