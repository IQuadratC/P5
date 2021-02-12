using System;
using Unity.Mathematics;
using UnityEngine;

namespace Utility.Variables
{
    [CreateAssetMenu(fileName = "Vec2Variable", menuName = "Utility/Varibles/Vec2")]
    public class Vec2Variable : ScriptableObject, ISerializationCallbackReceiver
    {
        [NonSerialized]
        public float2 Value;
        public float2 InitialValue;
        
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            Value = InitialValue;
        }
        
        public void Set(float2 value)
        {
            Value = value;
        }
    }
}
