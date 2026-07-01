using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public sealed class SnapshotRepository
    {
        private const string kBasePath = "Assets/BlendShapeSnapshots";

        private BlendShapeSnapshotDatabase m_cachedDatabase;
        private readonly Dictionary<SkinnedMeshRenderer, string> m_cachedGuid = new Dictionary<SkinnedMeshRenderer, string>();

        public void Save(SkinnedMeshRenderer renderer, string description)
        {
            if (renderer == null || renderer.sharedMesh == null)
                return;

            BlendShapeSnapshotTarget target = renderer.GetComponentInChildren<BlendShapeSnapshotTarget>() ?? CreateSnapshotTarget(renderer.gameObject);
            BlendShapeSnapshotDatabase database = GetDatabaseAsset(target.Guid) ?? CreateDatabaseAsset(target.Guid);
            database.AddSnapshot(renderer, description);
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();

            m_cachedGuid[renderer] = target.Guid;
            m_cachedDatabase = database;
        }

        public bool DeleteSnapshot(SkinnedMeshRenderer renderer, int databaseIndex)
        {
            if (!TryGetSnapshotDatabase(renderer, out BlendShapeSnapshotDatabase database))
                return false;

            if (databaseIndex < 0 || databaseIndex >= database.BlendShapeSnapshots.Count)
                return false;

            Undo.RecordObject(database, "Delete BlendShape Snapshot");
            database.RemoveSnapshotAt(databaseIndex);
            EditorUtility.SetDirty(database);
            AssetDatabase.SaveAssets();
            return true;
        }

        public bool TryGetSnapshotDatabase(SkinnedMeshRenderer renderer, out BlendShapeSnapshotDatabase database)
        {
            database = null;

            string guid = GetTargetGuid(renderer);
            if (string.IsNullOrEmpty(guid))
                return false;

            database = GetDatabaseAsset(guid);
            return database != null;
        }

        public int GetSnapshotCount(SkinnedMeshRenderer renderer)
        {
            return TryGetSnapshotDatabase(renderer, out BlendShapeSnapshotDatabase database)
                ? database.BlendShapeSnapshots.Count
                : 0;
        }

        public bool TryGetSnapshot(SkinnedMeshRenderer renderer, int index, out BlendShapeSnapshotDatabase.BlendShapeSnapshot snapshot)
        {
            snapshot = null;

            if (!TryGetSnapshotDatabase(renderer, out BlendShapeSnapshotDatabase database))
                return false;

            if (index < 0 || index >= database.BlendShapeSnapshots.Count)
                return false;

            snapshot = database.BlendShapeSnapshots[index];
            return snapshot != null;
        }

        private string GetTargetGuid(SkinnedMeshRenderer renderer)
        {
            if (renderer == null)
                return null;

            if (m_cachedGuid.TryGetValue(renderer, out string guid))
                return guid;

            BlendShapeSnapshotTarget target = renderer.GetComponentInChildren<BlendShapeSnapshotTarget>();
            if (target == null)
                return null;

            return m_cachedGuid[renderer] = target.Guid;
        }

        private BlendShapeSnapshotDatabase GetDatabaseAsset(string guid)
        {
            if (m_cachedDatabase != null && m_cachedDatabase.TargetGuid == guid)
                return m_cachedDatabase;

            string assetPath = $"{kBasePath}/{guid}.asset";
            BlendShapeSnapshotDatabase database = AssetDatabase.LoadAssetAtPath<BlendShapeSnapshotDatabase>(assetPath);
            return m_cachedDatabase = database;
        }

        private static BlendShapeSnapshotTarget CreateSnapshotTarget(GameObject targetGameObject)
        {
            string name = $"{targetGameObject.name} - BlendShapeSnapshotTarget";
            GameObject childGameObject = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(childGameObject, "Create BlendShape Snapshot Target");
            childGameObject.transform.SetParent(targetGameObject.transform, false);

            string newGuid = Guid.NewGuid().ToString();
            BlendShapeSnapshotTarget target = childGameObject.AddComponent<BlendShapeSnapshotTarget>();
            target.Init(newGuid);
            EditorUtility.SetDirty(target);

            CreateDatabaseAsset(newGuid);
            return target;
        }

        private static BlendShapeSnapshotDatabase CreateDatabaseAsset(string targetGuid)
        {
            BlendShapeSnapshotDatabase database = ScriptableObject.CreateInstance<BlendShapeSnapshotDatabase>();
            database.Init(targetGuid);

            if (!AssetDatabase.IsValidFolder(kBasePath))
            {
                Regex regex = new Regex("(.*)/(.*)");
                Match match = regex.Match(kBasePath);
                AssetDatabase.CreateFolder(match.Groups[1].Value, match.Groups[2].Value);
            }

            AssetDatabase.CreateAsset(database, $"{kBasePath}/{targetGuid}.asset");
            return database;
        }
    }
}
