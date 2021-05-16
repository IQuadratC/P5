using System.Collections.Generic;

using UnityEngine;

namespace Utility.Events
{
    [CreateAssetMenu(fileName = "GameEvent", menuName = "Utility/GameEvent")]
    public class GameEvent : ScriptableObject
    {
        private readonly IList<GameEventListener> events = new List<GameEventListener>();

        public void Raise()
        {
            for (var i = events.Count - 1; i >= 0; i--)
            {
                events[i].OnEventRaised();
            }
        }

        public void Register(GameEventListener listener)
        {
            events.Add(listener);
        }
        
        public void Unregister(GameEventListener listener)
        {
            events.Remove(listener);
        }
    }
}
