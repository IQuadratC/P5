using System;
using UnityEngine;

namespace Utility.Variables
{
    [CreateAssetMenu(fileName = "IntVariable", menuName = "Utility/Varibles/Int")]
    public class IntVariable : ScriptableObject, ISerializationCallbackReceiver
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
