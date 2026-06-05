using UnityEditor;
using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    [CustomEditor(typeof(BlendShapeSnapshotTarget))]
    public class BlendShapeSnapshotTargetEditor : UnityEditor.Editor
    {
        private BlendShapeSnapshotDatabase m_cachedDatabase;
        private bool m_isCacheFailed;

        public override void OnInspectorGUI()
        {
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
                    EditorGUILayout.HelpBox("Cache Failed", MessageType.Error);
                    if (GUILayout.Button("Refresh"))
                    {
                        m_isCacheFailed = false;
                        Repaint();
                    }
                }
            }

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