using System;
using UnityEngine;

namespace Utility.Variables
{
    [CreateAssetMenu(fileName = "IntVariable", menuName = "Utility/Varibles/Int")]
    public class IntVariable : ScriptableObject, ISerializationCallbackReceiver
    {
        public int Value;
        public int InitialValue;
        
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            Value = InitialValue;
        }
        
        public void Set(int value)
        {
            Value = value;
        }
    }
}
