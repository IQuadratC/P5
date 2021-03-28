using System;
using UnityEngine;

namespace Utility.Variables
{
    [CreateAssetMenu(fileName = "FloatVariable", menuName = "Utility/Varibles/Float")]
    public class FloatVariable : ScriptableObject, ISerializationCallbackReceiver
    {
        public float Value;
        public float InitialValue;
        
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            Value = InitialValue;
        }
        
        public void Set(float value)
        {
            Value = value;
        }
    }
}
