using System;

using UnityEngine;

namespace Utility.Variables
{
    [CreateAssetMenu(fileName = "Vec2Variable", menuName = "Utility/Varibles/Vec2")]
    public class Vec2Variable : ScriptableObject, ISerializationCallbackReceiver
    {
        public Vector2 Value;
        public Vector2 InitialValue;
        
        public void OnBeforeSerialize() { }
        public void OnAfterDeserialize()
        {
            Value = InitialValue;
        }
        
        public void Set(Vector2 value)
        {
            Value = value;
        }
    }
}
