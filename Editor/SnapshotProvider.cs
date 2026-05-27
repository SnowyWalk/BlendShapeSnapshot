using System.IO;
using UnityEditor;
using UnityEngine;
namespace SnowyWalk.BlendShapeSnapshot.Editor
{
    public class SnapshotProvider : IEditorWindowProvider
    {
        private const string m_path = "Assets/BlendShapeSnapshots";

        void IEditorWindowProvider.OnEnable()
        {
        }

        void IEditorWindowProvider.OnDisable()
        {
        }

        public void Save(GameObject targetGameObject)
        {
            var smr = targetGameObject.GetComponent<SkinnedMeshRenderer>();

            BlendShapeSnapshotInfo blendShapeSnapshotInfo = ScriptableObject.CreateInstance<BlendShapeSnapshotInfo>();
            int blendShapeCount = smr.sharedMesh.blendShapeCount;
            for (int i = 0; i < blendShapeCount; i++)
            {
                string name = smr.sharedMesh.GetBlendShapeName(i);
                float value = smr.GetBlendShapeWeight(i);
                blendShapeSnapshotInfo.AddBlendShapeKey(name, value);
            }
            
            // targetGameObject.get
        }

        private void Save()
        {
            var asset = ScriptableObject.CreateInstance<BlendShapeSnapshotInfo>();

            AssetDatabase.CreateAsset(asset, Path.Combine(m_path, "BlendShapeSnapshot.asset"));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
        
    }
}
