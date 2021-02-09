using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utility.Events;
using Utility.Variables;

public class TestButton : MonoBehaviour
{
    public InputField InputField;
    public GameEvent Event;
    public StringVariable text;
    
    public void Send()
    {
        text.Value = InputField.text;
        Event.Raise();
    }
}
