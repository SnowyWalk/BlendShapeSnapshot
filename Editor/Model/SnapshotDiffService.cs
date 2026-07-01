using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public sealed class SnapshotDiffService
    {
        private const float kDiffEpsilon = 0.001f;

        private readonly SnapshotRepository m_repository;

        public SnapshotDiffService(SnapshotRepository repository)
        {
            m_repository = repository;
        }

        public SnapshotDiffResult BuildDiff(SkinnedMeshRenderer renderer, SnapshotSelection selection, SnapshotDiffBasis basis)
        {
            if (renderer == null)
                return SnapshotDiffResult.Empty("대상 Mesh를 먼저 지정하세요.");

            Mesh mesh = renderer.sharedMesh;
            if (mesh == null)
                return SnapshotDiffResult.Empty("대상 Mesh에 sharedMesh가 없습니다.");

            Dictionary<string, float> toValues = GetSelectedTargetValues(renderer, mesh, selection, out string targetMessage);
            if (toValues == null)
                return SnapshotDiffResult.Empty(targetMessage);

            Dictionary<string, float> fromValues = GetBaselineValues(renderer, mesh, selection, basis, out string baselineMessage);
            if (fromValues == null)
                return SnapshotDiffResult.Empty(baselineMessage);

            List<SnapshotDiffEntry> entries = BuildOrderedDiffEntries(mesh, fromValues, toValues);
            return entries.Count == 0
                ? SnapshotDiffResult.Empty("변경된 BlendShape 값이 없습니다.")
                : new SnapshotDiffResult(entries, null);
        }

        private Dictionary<string, float> GetSelectedTargetValues(SkinnedMeshRenderer renderer, Mesh mesh, SnapshotSelection selection, out string message)
        {
            message = null;

            if (selection.IsCurrentState || !selection.HasSavedSnapshot)
                return GetCurrentBlendShapeValues(renderer, mesh);

            if (!m_repository.TryGetSnapshot(renderer, selection.DatabaseIndex, out BlendShapeSnapshotDatabase.BlendShapeSnapshot snapshot))
            {
                message = "선택한 스냅샷을 찾을 수 없습니다.";
                return null;
            }

            return GetSnapshotValues(snapshot);
        }

        private Dictionary<string, float> GetBaselineValues(SkinnedMeshRenderer renderer, Mesh mesh, SnapshotSelection selection, SnapshotDiffBasis basis, out string message)
        {
            message = null;

            if (basis == SnapshotDiffBasis.CurrentState)
                return GetCurrentBlendShapeValues(renderer, mesh);

            int snapshotCount = m_repository.GetSnapshotCount(renderer);
            if (snapshotCount <= 0)
            {
                message = "비교할 이전 스냅샷이 없습니다.";
                return null;
            }

            int previousSnapshotIndex = selection.HasSavedSnapshot ? selection.DatabaseIndex - 1 : snapshotCount - 1;
            if (!m_repository.TryGetSnapshot(renderer, previousSnapshotIndex, out BlendShapeSnapshotDatabase.BlendShapeSnapshot snapshot))
            {
                message = "비교할 이전 스냅샷이 없습니다.";
                return null;
            }

            return GetSnapshotValues(snapshot);
        }

        private static Dictionary<string, float> GetCurrentBlendShapeValues(SkinnedMeshRenderer renderer, Mesh mesh)
        {
            Dictionary<string, float> values = new Dictionary<string, float>();
            for (int i = 0; i < mesh.blendShapeCount; i++)
                values[mesh.GetBlendShapeName(i)] = renderer.GetBlendShapeWeight(i);

            return values;
        }

        private static Dictionary<string, float> GetSnapshotValues(BlendShapeSnapshotDatabase.BlendShapeSnapshot snapshot)
        {
            Dictionary<string, float> values = new Dictionary<string, float>();
            foreach ((string blendShapeName, float value) in snapshot.BlendShapeWeights)
                values[blendShapeName] = value;

            return values;
        }

        private static List<SnapshotDiffEntry> BuildOrderedDiffEntries(Mesh mesh, Dictionary<string, float> fromValues, Dictionary<string, float> toValues)
        {
            List<SnapshotDiffEntry> entries = new List<SnapshotDiffEntry>();
            HashSet<string> handledKeys = new HashSet<string>();

            for (int i = 0; i < mesh.blendShapeCount; i++)
            {
                string key = mesh.GetBlendShapeName(i);
                handledKeys.Add(key);
                TryAddDiffEntry(entries, key, fromValues, toValues);
            }

            foreach (string key in fromValues.Keys.Concat(toValues.Keys).Distinct().Where(key => !handledKeys.Contains(key)).OrderBy(key => key))
                TryAddDiffEntry(entries, key, fromValues, toValues);

            return entries;
        }

        private static void TryAddDiffEntry(List<SnapshotDiffEntry> entries, string key, Dictionary<string, float> fromValues, Dictionary<string, float> toValues)
        {
            float from = fromValues.TryGetValue(key, out float fromValue) ? fromValue : 0f;
            float to = toValues.TryGetValue(key, out float toValue) ? toValue : 0f;

            if (Mathf.Abs(to - from) <= kDiffEpsilon)
                return;

            entries.Add(new SnapshotDiffEntry(key, from, to));
        }
    }
}
