using System;
using System.Collections;
using System.Collections.Generic;
using Lidar;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Utility.Variables;

public class AIControler : MonoBehaviour
{
    [SerializeField]private Vec3Variable position;
    [SerializeField]private Vec2Variable goal;
    [SerializeField]private Dictionary<int2, int> obstacles = new Dictionary<int2, int>();
    public void updatePath()
    {
        String msg = "python multi,";
        int2 start =  (int2)(position.Value.xy);
        int2 end = (int2) (goal.Value);
        FindPath finder = new FindPath(obstacles);
        List<int2> path = finder.findPathBetweenInt2(start, end);
        float2 old = position.Value.xy;
        float2 move;
        for (int i = 0; i < path.Count; i++)
        {
            if (i >= 1 && i < (path.Count - 1) &&
                 ((path[i - 1] + new int2(2, 0)).Equals(path[i + 1]) ||
                 (path[i - 1] + new int2(0, 2)).Equals(path[i + 1]) ||
                 (path[i - 1] + new int2(-2, 0)).Equals(path[i + 1]) ||
                 (path[i - 1] + new int2(0, -2)).Equals(path[i + 1]) ||
                 (path[i - 1] + new int2(-2, 2)).Equals(path[i + 1]) ||
                 (path[i - 1] + new int2(-2, -2)).Equals(path[i + 1]) ||
                 (path[i - 1] + new int2(2, 2)).Equals(path[i + 1]) ||
                 (path[i - 1] + new int2(2, -2)).Equals(path[i + 1])))
            {
                continue;
            }
            move =  old - (path[i]);
            msg += "move " + move.x + " " + move.y + ",";
            old = (path[i]);
        }
        move = old - (goal.Value.xy);
        msg += "move " + move.x + " " + move.y + ",";
        Debug.Log(msg);
    }
}
