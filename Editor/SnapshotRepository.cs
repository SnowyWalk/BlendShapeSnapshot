using System.IO;
using UnityEditor;
using UnityEngine;
namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public class SnapshotRepository : IEditorWindowModule
    {
        private const string m_basePath = "Assets/BlendShapeSnapshots";
        
        private IEditorWindowOrchestrator m_orchestrator;
        

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

        public void Save(SkinnedMeshRenderer smr)
        {
            // TODO: Save
            // var smr = targetGameObject.GetComponent<SkinnedMeshRenderer>();
            //
            // BlendShapeSnapshotDatabase blendShapeSnapshotDatabase = ScriptableObject.CreateInstance<BlendShapeSnapshotDatabase>();
            // blendShapeSnapshotDatabase.CaptureBlendShapeKeys(smr);
            
        }

        private void SaveTest()
        {
            // TODO:
            var asset = ScriptableObject.CreateInstance<BlendShapeSnapshotDatabase>();

            AssetDatabase.CreateAsset(asset, Path.Combine(m_basePath, "BlendShapeSnapshot.asset"));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
        
    }
}
