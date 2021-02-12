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
    public float posRotation;
    public float2 pos;
    public float2 goal;
    public Dictionary<int2, int> obstacles = new Dictionary<int2, int>();
    public void updatePath()
    {
        String msg = "python multi,";
        int2 start = (int2) (pos / 10);
        int2 end = (int2) (goal / 10);
        FindPath finder = new FindPath(obstacles);
        List<int2> path = finder.findPathBetweenInt2(start, end);
        float2 old = pos;
        float2 move;
        foreach (int2 point in path)
        {
            move = old - (point * 10 + new int2(5, 5));
            msg += "move " + move.x + " " + move.y + ",";
            old = (point * 10 + new int2(5, 5));
        }
        move = old - (goal * 10 + new int2(5, 5));
        msg += "move " + move.x + " " + move.y + ",";
        Debug.Log(msg);
    }
}
