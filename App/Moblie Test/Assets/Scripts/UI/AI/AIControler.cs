using System;
using System.Collections;
using System.Collections.Generic;
using Lidar;
using Unity.Mathematics;
using UnityEngine;

public class AIControler : MonoBehaviour
{
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
            move =  old - (point * 10 + new int2(5, 5));
            msg += "move " + move.x + " " + move.y + ",";
            old = (point * 10 + new int2(5, 5));
        }
        move = old - (goal * 10 + new int2(5, 5));
        msg += "move " + move.x + " " + move.y + ",";
        Debug.Log(msg);
    }
}
