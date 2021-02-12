using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public bool measure;
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
    
    public Vec2Variable pos;
    public Vec2Variable goal;
    public Dictionary<int2, int> obstacles;
    public void updatePath()
    {
        String msg = "python multi,";
        int2 start = (int2) (pos.Value / 10);
        int2 end = (int2) (goal.Value / 10);
        FindPath finder = new FindPath(obstacles);
        List<int2> path = finder.findPathBetweenInt2(start, end);
        foreach (int2 point in path)
        {
            msg += " ";
        }
    }
}
