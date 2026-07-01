using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public struct BlendShapeSnapshotViewEvents
    {
        public bool TargetChanged;
        public SkinnedMeshRenderer TargetRenderer;

        public bool SelectionChanged;
        public int SelectedIndex;

        public bool DiffBasisChanged;
        public SnapshotDiffBasis DiffBasis;

        public bool DescriptionChanged;
        public string SnapshotDescription;

        public bool SaveRequested;
        public bool ApplyRequested;
        public bool DeleteRequested;
        public bool RenameRequested;
    }
}
