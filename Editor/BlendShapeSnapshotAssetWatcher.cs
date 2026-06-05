using System;
using UnityEditor;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    internal sealed class BlendShapeSnapshotAssetWatcher : AssetPostprocessor
    {
        public static event Action OnInvalidate;

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (ContainsSnapshotDatabaseChange(importedAssets) ||
                ContainsSnapshotDatabaseChange(deletedAssets) ||
                ContainsSnapshotDatabaseChange(movedAssets) ||
                ContainsSnapshotDatabaseChange(movedFromAssetPaths))
            {
                OnInvalidate?.Invoke();
            }
        }

        private static bool ContainsSnapshotDatabaseChange(string[] assetPaths)
        {
            foreach (string path in assetPaths)
            {
                var type = AssetDatabase.GetMainAssetTypeAtPath(path);
                if (type == typeof(BlendShapeSnapshotDatabase))
                    return true;
            }
            return false;
        }
    }
}
