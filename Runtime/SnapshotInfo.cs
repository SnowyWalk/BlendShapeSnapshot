using System;
using System.Collections.Generic;
using UnityEngine;
namespace SnowyWalk.BlendShapeSnapshot
{
    [Serializable]
    public class BlendShapeKeyData
    {
        public string BlendShapeKey;
        public float Value;
    }
    
    public class SnapshotInfo : ScriptableObject
    {
        public List<BlendShapeKeyData> BlendShapeKeyDataList = new List<BlendShapeKeyData>();
    }
}