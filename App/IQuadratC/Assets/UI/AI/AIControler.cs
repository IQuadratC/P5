using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lidar;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Utility;
using Utility.Events;
using Utility.Variables;

public class AIControler : MonoBehaviour
{
    [SerializeField]private GameEvent sendEvent;
    [SerializeField]private StringVariable sendString;
    [SerializeField]private Vec3Variable position;
    [SerializeField]private Int2ListVariable goals;
    [SerializeField]private Int2ListVariable obstaclesPoints;
    private Dictionary<int2, int> obstacles = new Dictionary<int2, int>();
    private List<int2> circle = new List<int2>();
    [SerializeField]private Int2ListVariable path;
    [SerializeField]private int distanceToWalls;
    [SerializeField]private float speed;
    public bool useRotation;
    private void Start()
    {
        circle = new List<int2>();
        for (int i = -distanceToWalls; i < distanceToWalls; i++)
        {
            for (int j = -distanceToWalls; j < distanceToWalls; j++)
            {
                if (math.sqrt(i * i + j * j) < distanceToWalls)
                {
                    circle.Add(new int2(i, j));
                }
            }
        }
    }

    public void UpdatePath()
    {
        UpdateObsticals();

        if (useRotation)
        {
            sendString.Value = WithRotation();
        }
        else
        {
            sendString.Value = NoRotation();
        }

        sendEvent.Raise();
    }

    private string WithRotation()
    {
        path.Value = new List<int2>();
        FindPath finder = new FindPath(obstacles);
        int2 start = (int2) (position.Value.xy);
        foreach (int2 goal in goals.Value)
        {
            int2 end = goal;
            path.Value.AddRange(finder.findPathBetweenInt2(start, end));
            start = end;
        }
        
        String msg = "roboter multi ";
        float2 old = position.Value.xy;
        float2 oldRotation = mathAdditions.Rotate(new float2(0,1), position.Value.z);
        float2 move;
        for (int i = 0; i < path.Value.Count; i++)
        {
            if (i >= 1 && i < (path.Value.Count - 1) &&
                ((path.Value[i - 1] + new int2(2, 0)).Equals(path.Value[i + 1]) ||
                 (path.Value[i - 1] + new int2(0, 2)).Equals(path.Value[i + 1]) ||
                 (path.Value[i - 1] + new int2(-2, 0)).Equals(path.Value[i + 1]) ||
                 (path.Value[i - 1] + new int2(0, -2)).Equals(path.Value[i + 1]) ||
                 (path.Value[i - 1] + new int2(-2, 2)).Equals(path.Value[i + 1]) ||
                 (path.Value[i - 1] + new int2(-2, -2)).Equals(path.Value[i + 1]) ||
                 (path.Value[i - 1] + new int2(2, 2)).Equals(path.Value[i + 1]) ||
                 (path.Value[i - 1] + new int2(2, -2)).Equals(path.Value[i + 1])))
            {
                continue;
            }
            move =  (path.Value[i]) - old;
            if (!move.Equals(float2.zero))
            {
                msg += "rotate," + (int)mathAdditions.Angle(oldRotation, move);
                msg += "move," + (int)math.length(move) + ";0;" + speed + ",";
                oldRotation = (path.Value[i]) - old;
            }
            old = (path.Value[i]);
        }
        move = old - (goals.Value[goals.Value.Count - 1].xy);
        if (!move.Equals(float2.zero))
        {
            msg += "move," + move.x + ";" + move.y + ",";
        }

        return msg;
    }
    private string NoRotation()
    {
        path.Value = new List<int2>();
        FindPath finder = new FindPath(obstacles);
        int2 start = (int2) (position.Value.xy);
        foreach (int2 goal in goals.Value)
        {
            int2 end = (int2) (goal);
            path.Value.AddRange(finder.findPathBetweenInt2(start, end));
            start = end;
        }
        
        String msg = "roboter multi ";
        float2 old = position.Value.xy;
        float2 move;
        for (int i = 0; i < path.Value.Count; i++)
        {
            if (i >= 1 && i < (path.Value.Count - 1) &&
                ((path.Value[i - 1] + new int2(2, 0)).Equals(path.Value[i + 1]) ||
                 (path.Value[i - 1] + new int2(0, 2)).Equals(path.Value[i + 1]) ||
                 (path.Value[i - 1] + new int2(-2, 0)).Equals(path.Value[i + 1]) ||
                 (path.Value[i - 1] + new int2(0, -2)).Equals(path.Value[i + 1]) ||
                 (path.Value[i - 1] + new int2(-2, 2)).Equals(path.Value[i + 1]) ||
                 (path.Value[i - 1] + new int2(-2, -2)).Equals(path.Value[i + 1]) ||
                 (path.Value[i - 1] + new int2(2, 2)).Equals(path.Value[i + 1]) ||
                 (path.Value[i - 1] + new int2(2, -2)).Equals(path.Value[i + 1])))
            {
                continue;
            }
            move =  (path.Value[i]) - old;
            if (!move.Equals(float2.zero))
            {
                msg += "move," + move.y + ";" + move.x + ";" + speed + ",";
            }
            old = (path.Value[i]);
        }
        move = old - (goals.Value[goals.Value.Count - 1].xy);
        if (!move.Equals(float2.zero))
        {
            msg += "move," + move.y + ";" + move.x + ",";
        }

        return msg;
    }
    private void UpdateObsticals()
    {
        obstacles = new Dictionary<int2, int>();
        foreach (int2 point in obstaclesPoints.Value)
        {
            foreach (int2 p in circle)
            {
                obstacles.Add(point + p, 1);
            }
        }

    }
    private void OnDrawGizmos()
    {
        foreach (int2 point in path.Value)
        {
            Gizmos.DrawSphere(new Vector3(point.x, point.y), 1);
        }
    }
}
