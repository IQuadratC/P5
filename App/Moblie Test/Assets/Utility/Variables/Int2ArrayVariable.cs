using System;
using Unity.Mathematics;
using UnityEngine;

namespace Utility.Variables
{
    [CreateAssetMenu(fileName = "Int2ArrayVariable", menuName = "Utility/Varibles/Int2[]")]
    public class Int2ArrayVariable : ScriptableObject, ISerializationCallbackReceiver
    {
        [NonSerialized]
        public int2[] Value;
        public int2[] InitialValue;
        
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            Value = InitialValue;
        }
        
        public void Set(int2[] value)
        {
            Value = value;
        }
    }
}
