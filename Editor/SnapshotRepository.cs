using System.IO;
using UnityEditor;
using UnityEngine;
namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public class SnapshotRepository : IEditorWindowModule
    {
        private const string m_path = "Assets/BlendShapeSnapshots";

        void IEditorWindowModule.OnEnable()
        {
            // TODO:
        }

        void IEditorWindowModule.OnDisable()
        {
            // TODO:
        }

        public void Save(GameObject targetGameObject)
        {
            var smr = targetGameObject.GetComponent<SkinnedMeshRenderer>();

            BlendShapeSnapshotDatabase blendShapeSnapshotDatabase = ScriptableObject.CreateInstance<BlendShapeSnapshotDatabase>();
            blendShapeSnapshotDatabase.Capture(smr);
            
            // TODO:
        }

        private void Save()
        {
            // TODO:
            var asset = ScriptableObject.CreateInstance<BlendShapeSnapshotDatabase>();

            AssetDatabase.CreateAsset(asset, Path.Combine(m_path, "BlendShapeSnapshot.asset"));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
        
    }
}
