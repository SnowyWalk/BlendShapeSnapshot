using System.Collections.Generic;
using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public sealed class BlendShapeSnapshotViewState
    {
        public SkinnedMeshRenderer TargetRenderer;
        public IReadOnlyList<string> SnapshotNames = System.Array.Empty<string>();
        public IReadOnlyList<SnapshotDiffEntry> DiffEntries = System.Array.Empty<SnapshotDiffEntry>();
        public SnapshotDiffBasis DiffBasis = SnapshotDiffBasis.PreviousSnapshot;
        public int SelectedIndex;
        public string SnapshotDescription = string.Empty;
        public string PreviewLabel = string.Empty;
        public string DiffEmptyMessage = string.Empty;
        public bool CanSave;
        public bool CanApply;
    }
}
