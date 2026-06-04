using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace SnowyWalk.BlendShapeSnapshot
{
    public class BlendShapeSnapshotDatabase : ScriptableObject
    {
        [Serializable]
        private class BlendShapeKeyEntry
        {
            public string BlendShapeKey;
            public float Value;
            
            public BlendShapeKeyEntry(string blendShapeKey, float value)
            {
                BlendShapeKey = blendShapeKey;
                Value = value;
            }
        }
        
        private List<BlendShapeKeyEntry> m_blendShapeKeyDataList = new List<BlendShapeKeyEntry>();
        private DateTime m_snapshotTime;
        private string m_description;

        public void Capture(SkinnedMeshRenderer smr) 
        {
            int blendShapeCount = smr.sharedMesh.blendShapeCount;
            for (int i = 0; i < blendShapeCount; i++)
            {
                string name = smr.sharedMesh.GetBlendShapeName(i);
                float value = smr.GetBlendShapeWeight(i);
                AddBlendShapeKey(name, value);
            }
        }

        public void Apply(SkinnedMeshRenderer smr)
        {
            smr.sharedMesh.ClearBlendShapes();
            foreach ((string blendShapeKey, float value) in BlendShapeKeyDataList)
            {
                smr.SetBlendShapeWeight(smr.sharedMesh.GetBlendShapeIndex(blendShapeKey), value);
            }
        }

        public string SnapshotTime => m_snapshotTime.ToString("yyyy-MM-dd HH:mm:ss");
        public string Description => m_description;
        
        public void AddBlendShapeKey(string blendShapeKey, float value)
        {
            m_blendShapeKeyDataList.Add(new BlendShapeKeyEntry(blendShapeKey, value));
        }
        
        public IEnumerable<(string blendShapeKey, float value)> BlendShapeKeyDataList => m_blendShapeKeyDataList.Select(e => (e.BlendShapeKey, e.Value));
    }
}