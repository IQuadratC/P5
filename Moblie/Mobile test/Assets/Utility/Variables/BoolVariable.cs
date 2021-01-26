using System;
using UnityEngine;

namespace Utility.Variables
{
    [CreateAssetMenu(fileName = "BoolVariable", menuName = "Utility/Varibles/Bool")]
    public class BoolVariable : ScriptableObject, ISerializationCallbackReceiver
    {
        [NonSerialized]
        public float Value;
        public float InitialValue;
        
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            Value = InitialValue;
        }
    }
}
