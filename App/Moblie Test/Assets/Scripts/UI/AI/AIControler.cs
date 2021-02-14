using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lidar;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Utility.Variables;

public class AIControler : MonoBehaviour
{
    [SerializeField]private Vec3Variable position;
    [SerializeField]private Float2ListVariable goals;
    [SerializeField]private Dictionary<int2, int> obstacles = new Dictionary<int2, int>();
    [SerializeField]private Float2ListVariable pathFloat;
    List<int2> path = new List<int2>();

    public void updatePath()
    {
        FindPath finder = new FindPath(obstacles);
        int2 start = (int2) (position.Value.xy);
        foreach (float2 goal in goals.Value)
        {
            int2 end = (int2) (goal);
            path.AddRange(finder.findPathBetweenInt2(start, end));
            start = end;
        }
        
        String msg = "python multi,";
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
            move =  (path[i]) - old;
            msg += "move " + move.x + " " + move.y + ",";
            old = (path[i]);
        }
        move = old - (goals.Value[goals.Value.Count - 1].xy);
        msg += "move " + move.x + " " + move.y + ",";
        Debug.Log(msg);
    }
    private void OnDrawGizmos()
    {
        foreach (int2 point in path)
        {
            Gizmos.DrawSphere(new Vector3(point.x, point.y), 1);
        }
    }
}
