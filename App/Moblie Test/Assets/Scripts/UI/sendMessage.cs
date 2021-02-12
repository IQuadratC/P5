using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility.Events;
using Utility.Variables;

public class sendMessage : MonoBehaviour
{
    public StringVariable message;
    public GameEvent send;

    public void Move(int x, int y)
    {
        message.Value = "move " + x + " " + y;
        send.Raise();
    }

    public void Rotate(int r)
    {
        message.Value = "rotate " + r;
        send.Raise();
    }

    public void Send(string msg)
    {
        message.Value = msg;
        send.Raise();
    }
}
