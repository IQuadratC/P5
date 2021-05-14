using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility.Events;
using Utility.Variables;

public class TestButton : MonoBehaviour
{
    [SerializeField] private String value;
    [SerializeField] private StringVariable sendString;
    [SerializeField] private GameEvent sendEvent;
    
    public void send()
    {
        sendString.Value = value;
        sendEvent.Raise();
    }
}
