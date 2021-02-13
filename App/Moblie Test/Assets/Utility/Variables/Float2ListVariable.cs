using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Utility.Variables
{
    [CreateAssetMenu(fileName = "ListVariable", menuName = "Utility/Varibles/Float2List")] 
    public class Float2ListVariable : ScriptableObject, ISerializationCallbackReceiver
    {
        [NonSerialized]
        public List<float2> Value;
        public List<float2> InitialValue;
        
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            Value = InitialValue;
        }
        
        public void Set(List<float2> value)
        {
            Value = value;
        }
    }
}
