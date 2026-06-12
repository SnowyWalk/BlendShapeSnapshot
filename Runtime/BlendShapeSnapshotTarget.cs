using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SnowyWalk.BlendShapeSnapshot
{
    public class BlendShapeSnapshotTarget : MonoBehaviour
    {
        [SerializeField] private string m_guid;
        
        public string Guid => m_guid;

        public void Init(string guid)
        {
            m_guid = guid;
        }
    }
}
