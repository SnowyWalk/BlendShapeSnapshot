using UnityEditor;
using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    [CustomEditor(typeof(BlendShapeSnapshotTarget))]
    public class BlendShapeSnapshotTargetEditor : UnityEditor.Editor
    {
        private UnityEngine.Object m_target;
        private BlendShapeSnapshotDatabase m_cachedDatabase;
        private bool m_isCacheFailed;

        private void OnEnable()
        {
            BlendShapeSnapshotAssetWatcher.OnInvalidate += InvalidateCache;
        }

        private void OnDisable()
        {
            BlendShapeSnapshotAssetWatcher.OnInvalidate -= InvalidateCache;
        }

        public override void OnInspectorGUI()
        {
            if (m_target != target)
            {
                InvalidateCache();
                m_target = (BlendShapeSnapshotTarget)target;
            }

            BlendShapeSnapshotTarget component = (BlendShapeSnapshotTarget)target;

            EditorGUILayout.LabelField($"Guid: {component.Guid}", EditorStyles.boldLabel);

            if (m_cachedDatabase == null)
            {
                if (!m_isCacheFailed)
                {
                    bool cacheSuccess = CacheDatabase(component.Guid);
                    m_isCacheFailed = !cacheSuccess;
                }

                if (m_isCacheFailed)
                {
                    EditorGUILayout.HelpBox("대응되는 저장소를 찾지 못했음", MessageType.Error);
                }
            }

            if (m_cachedDatabase != null)
            {
                EditorGUILayout.ObjectField("Database Asset", m_cachedDatabase, typeof(BlendShapeSnapshotDatabase), false);
            }
        }

        private void InvalidateCache()
        {
            m_cachedDatabase = null;
            m_isCacheFailed = false;
        }

        private bool CacheDatabase(string guid)
        {
            string[] databases = AssetDatabase.FindAssets($"t:{nameof(BlendShapeSnapshotDatabase)}");
            foreach (string assetGuid in databases)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetGuid);
                BlendShapeSnapshotDatabase database = AssetDatabase.LoadAssetAtPath<BlendShapeSnapshotDatabase>(path);
                if (database == null || database.TargetGuid != guid)
                    continue;

                m_cachedDatabase = database;
                return true;
            }
            return false;
        }
    }
}