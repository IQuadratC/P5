using System;
using System.Collections.Generic;

using UnityEngine;

namespace Utility.Variables
{
    [CreateAssetMenu(fileName = "Int2List", menuName = "Utility/Varibles/Int2List")] 
    public class Int2ListVariable : ScriptableObject, ISerializationCallbackReceiver
    {
        public List<Vector2Int> Value;
        public List<Vector2Int> InitialValue;
        
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            Value = InitialValue;
        }
        
        public void Set(List<Vector2Int> value)
        {
            Value = value;
        }
    }
}
