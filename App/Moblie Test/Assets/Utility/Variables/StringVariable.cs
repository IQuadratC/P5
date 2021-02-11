using System;
using UnityEngine;

namespace Utility.Variables
{
    [CreateAssetMenu(fileName = "StringVariable", menuName = "Utility/Varibles/String")]
    public class StringVariable : ScriptableObject, ISerializationCallbackReceiver
    {
        [NonSerialized]
        public string Value;
        public string InitialValue;
        
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            Value = InitialValue;
        }
        
        public void Set(string value)
        {
            Value = value;
        }
    }
}
