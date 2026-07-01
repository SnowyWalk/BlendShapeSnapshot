using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace SnowyWalk.BlendShapeSnapshot
{
    public class BlendShapeSnapshotDatabase : ScriptableObject
    {
        [Serializable]
        public class BlendShapeSnapshot
        {
            [Serializable]
            private class BlendShapeWeight
            {
                public string BlendShapeName;
                public float Value;

                public BlendShapeWeight(string blendShapeName, float value)
                {
                    BlendShapeName = blendShapeName;
                    Value = value;
                }
            }

            [SerializeField] private List<BlendShapeWeight> m_blendShapeWeightList = new List<BlendShapeWeight>();
            [SerializeField] private string m_snapshotTime;
            [SerializeField] private string m_description;

            public IEnumerable<(string blendShapeName, float value)> BlendShapeWeights => m_blendShapeWeightList.Select(e => (BlendShapeKey: e.BlendShapeName, e.Value));
            public string SnapshotTime => m_snapshotTime;
            public string Description => m_description;
            
            public BlendShapeSnapshot(SkinnedMeshRenderer smr, string description = "")
            {
                m_snapshotTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                m_description = description;
                
                int blendShapeCount = smr.sharedMesh.blendShapeCount;
                for (int i = 0; i < blendShapeCount; i++)
                {
                    string name = smr.sharedMesh.GetBlendShapeName(i);
                    float value = smr.GetBlendShapeWeight(i);
                    m_blendShapeWeightList.Add(new BlendShapeWeight(name, value));
                }
            }

            public void ApplySnapshot(SkinnedMeshRenderer smr)
            {
                foreach ((string blendShapeKey, float value) in BlendShapeWeights)
                {
                    int index = smr.sharedMesh.GetBlendShapeIndex(blendShapeKey);
                    if (index == -1)
                    {
                        Debug.LogWarning($"[BlendShapeSnapshot] BlendShapeKey {blendShapeKey} not found in SkinnedMeshRenderer");
                        continue;
                    }
                    
                    smr.SetBlendShapeWeight(index, value);
                }
            }
        }

        [SerializeField] private string m_targetGuid; // 그냥 Pair 찾는 용도의 보조 필드
        [SerializeField] private List<BlendShapeSnapshot> m_blendShapeSnapshotList = new List<BlendShapeSnapshot>();

        public IReadOnlyList<BlendShapeSnapshot> BlendShapeSnapshots => m_blendShapeSnapshotList;
        public string TargetGuid => m_targetGuid;
        
        public void Init(string targetGuid)
        {
            m_targetGuid = targetGuid;
        }
        
        public void AddSnapshot(SkinnedMeshRenderer smr, string description)
        {
            BlendShapeSnapshot snapshot = new BlendShapeSnapshot(smr, description);
            m_blendShapeSnapshotList.Add(snapshot);
        }

        public void RemoveSnapshotAt(int index)
        {
            if (index < 0 || index >= m_blendShapeSnapshotList.Count)
                return;

            m_blendShapeSnapshotList.RemoveAt(index);
        }
    }
}
