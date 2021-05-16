using System;
using UnityEngine;

namespace Utility.Variables
{
    [CreateAssetMenu(fileName = "Vec3Variable", menuName = "Utility/Varibles/Vec3")]
    public class Vec3Variable : ScriptableObject, ISerializationCallbackReceiver
    {
        public Vector3 Value;
        public Vector3 InitialValue;
        
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            Value = InitialValue;
        }
        
        public void Set(Vector3 value)
        {
            Value = value;
        }
    }
}
