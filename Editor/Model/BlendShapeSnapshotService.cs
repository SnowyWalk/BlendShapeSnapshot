using UnityEditor;
using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public sealed class BlendShapeSnapshotService
    {
        public bool TryApplySnapshot(SkinnedMeshRenderer renderer, BlendShapeSnapshotDatabase.BlendShapeSnapshot snapshot)
        {
            if (renderer == null || renderer.sharedMesh == null || snapshot == null)
                return false;

            Undo.RecordObject(renderer, "Apply BlendShape Snapshot");
            snapshot.ApplySnapshot(renderer);
            EditorUtility.SetDirty(renderer);
            return true;
        }
    }
}
