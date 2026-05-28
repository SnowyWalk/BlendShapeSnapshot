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
        }

        void IEditorWindowModule.OnDisable()
        {
        }

        public void Save(GameObject targetGameObject)
        {
            var smr = targetGameObject.GetComponent<SkinnedMeshRenderer>();

            BlendShapeSnapshotAsset blendShapeSnapshotAsset = ScriptableObject.CreateInstance<BlendShapeSnapshotAsset>();
            int blendShapeCount = smr.sharedMesh.blendShapeCount;
            for (int i = 0; i < blendShapeCount; i++)
            {
                string name = smr.sharedMesh.GetBlendShapeName(i);
                float value = smr.GetBlendShapeWeight(i);
                blendShapeSnapshotAsset.AddBlendShapeKey(name, value);
            }
            
            // targetGameObject.get
        }

        private void Save()
        {
            var asset = ScriptableObject.CreateInstance<BlendShapeSnapshotAsset>();

            AssetDatabase.CreateAsset(asset, Path.Combine(m_path, "BlendShapeSnapshot.asset"));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
        
    }
}
