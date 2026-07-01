using System.Collections.Generic;
using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public sealed class SnapshotListModel
    {
        public const string CurrentStateLabel = "(현재 상태)";

        private readonly SnapshotRepository m_repository;

        public SnapshotListModel(SnapshotRepository repository)
        {
            m_repository = repository;
        }

        public List<string> BuildLatestOrderedNames(SkinnedMeshRenderer renderer)
        {
            List<string> names = new List<string> { CurrentStateLabel };

            if (!m_repository.TryGetSnapshotDatabase(renderer, out BlendShapeSnapshotDatabase database))
                return names;

            for (int i = database.BlendShapeSnapshots.Count - 1; i >= 0; i--)
            {
                BlendShapeSnapshotDatabase.BlendShapeSnapshot snapshot = database.BlendShapeSnapshots[i];
                names.Add($"{i + 1}. {snapshot.SnapshotTime} · {snapshot.Description}");
            }

            return names;
        }
    }
}
