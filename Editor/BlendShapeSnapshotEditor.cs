using UnityEditor;
using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    internal interface IEditorWindowModule
    {
        public void OnEnable();
        public void OnDisable();
    }

    public class BlendShapeSnapshotEditor : EditorWindow
    {
        private SnapshotPreviewRenderer m_snapshotPreviewRenderer = new SnapshotPreviewRenderer();
        private SnapshotRepository m_snapshotRepository = new SnapshotRepository();
        
        private SkinnedMeshRenderer m_targetMeshRenderer;

        private IEditorWindowModule[] m_providers;

        [MenuItem("Tools/Blend Shape Snapshot Manager")]
        public static void ShowWindow()
        {
            BlendShapeSnapshotEditor window = GetWindow<BlendShapeSnapshotEditor>("Blend Shape Snapshot Manager");
            window.minSize = new Vector2(420f, 320f);
            window.Show();
        }

        private void OnEnable()
        {
            m_providers = new IEditorWindowModule[] { m_snapshotPreviewRenderer, m_snapshotRepository };

            foreach (IEditorWindowModule provider in m_providers)
            {
                provider.OnEnable();
            }
        }

        private void OnDisable()
        {
            foreach (IEditorWindowModule provider in m_providers)
            {
                provider.OnDisable();
            }
        }

        private void OnGUI()
        {
            {
                const string label = "대상 Mesh";
                EditorGUIUtility.labelWidth = EditorStyles.label.CalcSize(new GUIContent(label)).x + 8f;
                m_targetMeshRenderer = EditorGUILayout.ObjectField(label, m_targetMeshRenderer, typeof(SkinnedMeshRenderer), true, GUILayout.ExpandWidth(true)) as SkinnedMeshRenderer;
                EditorGUIUtility.labelWidth = 0f;
            }
            
            
        }
    }
}