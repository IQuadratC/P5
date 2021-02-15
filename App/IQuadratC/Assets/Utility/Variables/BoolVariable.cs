using System;
using UnityEngine;

namespace Utility.Variables
{
    [CreateAssetMenu(fileName = "BoolVariable", menuName = "Utility/Varibles/Bool")]
    public class BoolVariable : ScriptableObject, ISerializationCallbackReceiver
    {
        public bool Value;
        public bool InitialValue;
        
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            Value = InitialValue;
        }

        public void Set(bool value)
        {
            Value = value;
        }
    }
}
