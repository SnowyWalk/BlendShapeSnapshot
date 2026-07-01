namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public enum SnapshotDiffBasis
    {
        PreviousSnapshot,
        CurrentState,
    }

    public readonly struct SnapshotDiffEntry
    {
        public readonly string Key;
        public readonly float From;
        public readonly float To;

        public SnapshotDiffEntry(string key, float from, float to)
        {
            Key = key;
            From = from;
            To = to;
        }
    }

    public sealed class SnapshotDiffResult
    {
        public static SnapshotDiffResult Empty(string message)
        {
            return new SnapshotDiffResult(System.Array.Empty<SnapshotDiffEntry>(), message);
        }

        public readonly System.Collections.Generic.IReadOnlyList<SnapshotDiffEntry> Entries;
        public readonly string EmptyMessage;

        public bool HasEntries => Entries != null && Entries.Count > 0;

        public SnapshotDiffResult(System.Collections.Generic.IReadOnlyList<SnapshotDiffEntry> entries, string emptyMessage)
        {
            Entries = entries;
            EmptyMessage = emptyMessage;
        }
    }
}
