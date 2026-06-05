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

        [MenuItem("Tools/BlendShape Snapshot Manager/Save Test")]
        private static void SaveTest()
        {
            var asset = ScriptableObject.CreateInstance<BlendShapeSnapshotDatabase>();
            string assetPath = Path.Combine(m_basePath, "BlendShapeSnapshot.asset").Replace("\\", "/");

            EnsureDirectoryForAssetPath(assetPath);
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }

        private static void EnsureDirectoryForAssetPath(string assetPath)
        {
            string directoryPath = Path.GetDirectoryName(assetPath)?.Replace("\\", "/");
            if (string.IsNullOrEmpty(directoryPath) || AssetDatabase.IsValidFolder(directoryPath))
                return;

            string[] pathParts = directoryPath.Split('/');
            string currentPath = pathParts[0];
            for (int i = 1; i < pathParts.Length; i++)
            {
                string nextFolderName = pathParts[i];
                string nextPath = $"{currentPath}/{nextFolderName}";
                if (!AssetDatabase.IsValidFolder(nextPath))
                    AssetDatabase.CreateFolder(currentPath, nextFolderName);

                currentPath = nextPath;
            }
        }
    }
}
