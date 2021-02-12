using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Utility.Events;
using Utility.Variables;

public class MovementHandle : MonoBehaviour
{
    private enum States
    {
        stop =  0,
        dirction = 1,
        rotate = 2,
        folowpath = 3
    }

    private States State = States.stop;
    [FormerlySerializedAs("measur")]
    [FormerlySerializedAs("mesur")]
    public bool measure;
    public Int2ArrayVariable path;
    public Vec2Variable direction;
    public FloatVariable rotation;
    public GameEvent finishedMeasure;
    public GameEvent send;
    public StringVariable sendSting;

    private void FixedUpdate()
    {
        if (!measure)
        {
            switch (State)
            {
                case States.stop:
                    sendSting.Value = "python stop";
                    send.Raise();
                    break;
                case States.dirction:
                    sendSting.Value = "python move " + math.normalize(direction.Value).x + " " + math.normalize(direction.Value).y;
                    send.Raise();
                    break;
                case States.rotate:
                    sendSting.Value = "python rotate" + rotation;
                    send.Raise();
                    break;
                
            }
        }
    }
}
