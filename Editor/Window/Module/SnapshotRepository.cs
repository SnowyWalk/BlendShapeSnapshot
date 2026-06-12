using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public class SnapshotRepository : IEditorWindowModule
    {
        private const string m_basePath = "Assets/BlendShapeSnapshots";

        private IEditorWindowOrchestrator m_orchestrator;


        private BlendShapeSnapshotDatabase m_cachedDatabase;
        private readonly Dictionary<SkinnedMeshRenderer, string> m_cachedGuid = new Dictionary<SkinnedMeshRenderer, string>();


        void IEditorWindowModule.Initialize(IEditorWindowOrchestrator orchestrator)
        {
            m_orchestrator = orchestrator;
        }

        void IEditorWindowModule.OnEnable()
        {
            // TODO:
        }

        void IEditorWindowModule.OnDisable()
        {
            // TODO:
        }

        public void Save(SkinnedMeshRenderer smr, string description)
        {
            var snapshotTargetComponent = smr.GetComponentInChildren<BlendShapeSnapshotTarget>() ?? CreateSnapshotTarget(smr.gameObject);
            var snapshotDatabase = GetDatabaseAsset(snapshotTargetComponent.Guid);
            snapshotDatabase.AddSnapshot(smr, description);
            EditorUtility.SetDirty(snapshotDatabase);
            AssetDatabase.SaveAssets();

            m_cachedGuid[smr] = snapshotTargetComponent.Guid;
        }

        private static BlendShapeSnapshotTarget CreateSnapshotTarget(GameObject targetGameObject)
        {
            // Create Child GameObject
            string name = $"{targetGameObject.name} - BlendShapeSnapshotTarget";
            GameObject childGameObject = new GameObject(name);
            Undo.RegisterCreatedObjectUndo(childGameObject, "Create Child Object");
            childGameObject.transform.SetParent(targetGameObject.transform, false);

            // Add Component & Allocate Guid
            string newGuid = Guid.NewGuid().ToString();
            var snapshotTargetComponent = childGameObject.AddComponent<BlendShapeSnapshotTarget>();
            snapshotTargetComponent.Init(newGuid);
            EditorUtility.SetDirty(snapshotTargetComponent);

            // Make pair Database Asset
            var blendShapeSnapshotDatabase = ScriptableObject.CreateInstance<BlendShapeSnapshotDatabase>();
            blendShapeSnapshotDatabase.Init(newGuid);
            if (!AssetDatabase.IsValidFolder(m_basePath))
            {
                var regex = new Regex("(.*)/(.*)");
                AssetDatabase.CreateFolder(regex.Match(m_basePath).Groups[1].Value, regex.Match(m_basePath).Groups[2].Value);
            }
            AssetDatabase.CreateAsset(blendShapeSnapshotDatabase, $"{m_basePath}/{newGuid}.asset");
            // AssetDatabase.SaveAssets(); // 밖에서 어차피 호출할거라 생략

            return snapshotTargetComponent;
        }

        private BlendShapeSnapshotDatabase GetDatabaseAsset(string guid)
        {
            if (m_cachedDatabase != null && m_cachedDatabase.TargetGuid == guid)
                return m_cachedDatabase;

            string assetPath = $"{m_basePath}/{guid}.asset";
            return m_cachedDatabase = AssetDatabase.LoadAssetAtPath<BlendShapeSnapshotDatabase>(assetPath);
        }

        public List<string> GetSnapshotLatestOrderedNames(SkinnedMeshRenderer smr)
        {
            List<string> names = new List<string>();
            names.Add("(현재 상태)");
            string guid = GetTargetGuid(smr);
            var snapShotDatabase = GetDatabaseAsset(guid);
            for (int i = snapShotDatabase.BlendShapeSnapshots.Count - 1; i >= 0; i--)
            {
                var snapShot = snapShotDatabase.BlendShapeSnapshots[i];
                names.Add($"{i + 1}. {snapShot.SnapshotTime} · {snapShot.Description}");
            }
            return names;
        }

        private string GetTargetGuid(SkinnedMeshRenderer smr)
        {
            if (m_cachedGuid.TryGetValue(smr, out string guid))
                return guid;

            var snapShotTarget = smr.GetComponentInChildren<BlendShapeSnapshotTarget>();
            if (snapShotTarget == null)
                return null;

            return m_cachedGuid[smr] = snapShotTarget.Guid;
        }
        
        public BlendShapeSnapshotDatabase.BlendShapeSnapshot GetSnapshot(SkinnedMeshRenderer smr, int index)
        {
            string guid = GetTargetGuid(smr);
            var snapShotDatabase = GetDatabaseAsset(guid);
            return snapShotDatabase.BlendShapeSnapshots[index];
        }
    }
}