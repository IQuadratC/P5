using System;
using Unity.Mathematics;
using UnityEngine;

namespace Utility.Variables
{
    [CreateAssetMenu(fileName = "Vec3Variable", menuName = "Utility/Varibles/Vec3")] 
    public class Vec3Variable : ScriptableObject, ISerializationCallbackReceiver
    {
        [NonSerialized]
        public float3 Value;
        public float3 InitialValue;
        
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            Value = InitialValue;
        }
        
        public void Set(float3 value)
        {
            Value = value;
        }
    }
}
