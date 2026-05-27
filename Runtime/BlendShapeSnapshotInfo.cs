using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace SnowyWalk.BlendShapeSnapshot
{
    public class BlendShapeSnapshotInfo : ScriptableObject
    {
        [Serializable]
        private class BlendShapeKeyData
        {
            public string BlendShapeKey;
            public float Value;
            
            public BlendShapeKeyData(string blendShapeKey, float value)
            {
                BlendShapeKey = blendShapeKey;
                Value = value;
            }
        }
        
        private List<BlendShapeKeyData> m_blendShapeKeyDataList = new List<BlendShapeKeyData>();
        private DateTime m_snapshotTime;
        private string m_description;
        
        public string SnapshotTime => m_snapshotTime.ToString("yyyy-MM-dd HH:mm:ss");
        public string Description => m_description;
        
        public void AddBlendShapeKey(string blendShapeKey, float value)
        {
            m_blendShapeKeyDataList.Add(new BlendShapeKeyData(blendShapeKey, value));
        }
        
        public IEnumerable<(string blendShapeKey, float value)> BlendShapeKeyDataList => m_blendShapeKeyDataList.Select(e => (e.BlendShapeKey, e.Value));
    }
}