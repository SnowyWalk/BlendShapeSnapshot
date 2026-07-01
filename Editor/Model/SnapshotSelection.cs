namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public readonly struct SnapshotSelection
    {
        public static readonly SnapshotSelection None = new SnapshotSelection(-1, -1, false);

        public readonly int ListIndex;
        public readonly int DatabaseIndex;
        public readonly bool IsCurrentState;

        public bool HasSelection => ListIndex >= 0;
        public bool HasSavedSnapshot => HasSelection && !IsCurrentState && DatabaseIndex >= 0;

        private SnapshotSelection(int listIndex, int databaseIndex, bool isCurrentState)
        {
            ListIndex = listIndex;
            DatabaseIndex = databaseIndex;
            IsCurrentState = isCurrentState;
        }

        public static SnapshotSelection FromListIndex(int listIndex, int snapshotCount)
        {
            if (listIndex < 0)
                return None;

            if (listIndex == 0)
                return new SnapshotSelection(0, -1, true);

            int databaseIndex = snapshotCount - listIndex;
            if (databaseIndex < 0 || databaseIndex >= snapshotCount)
                return None;

            return new SnapshotSelection(listIndex, databaseIndex, false);
        }
    }
}
