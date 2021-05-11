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
    [SerializeField]private Vec3Variable positionInput;
    [SerializeField]private Int2ListVariable goalsInput;
    [SerializeField]private Int2ListVariable obstaclesPointsInput;
    [SerializeField]private Int2ListVariable pathOutput;
    [SerializeField]private float minDistanceToGoal;
    [SerializeField]private GameEvent getLidarData;
    [SerializeField]private float updateSpeed;
    private float lastUpdate;
    
    
    // don't change these after start
    [SerializeField]private int distanceToWalls; 
    [SerializeField]private int speed;
    [SerializeField]private bool useRotation;
    [SerializeField]private bool driveCircle;
    private Dictionary<int2, int> obstacles;
    private List<int2> circle;
    [SerializeField]private StringVariable logMessage;
    [SerializeField]private GameEvent logEvent;
    
    // nedded for exploration of the world
    private List<int2> reachedPoints;
    [SerializeField]private int gridSize;
    [SerializeField]private bool explore;
    
    private void Awake()
    {
        // set some variables
        obstacles = new Dictionary<int2, int>();
        reachedPoints = new List<int2>();
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

        lastUpdate = Time.realtimeSinceStartup;

    }

    private void Update()
    {
        if (Time.realtimeSinceStartup - lastUpdate > updateSpeed)
        {
            sendString.Value = "roboter move 0,0";
            sendEvent.Raise();
            getLidarData.Raise();
            lastUpdate = Time.realtimeSinceStartup;
        }
    }

    /**
     * Starts processing thread
     */
    public void StartPath()
    {
        // set variables needed in ProsesPath thread 
        pos = positionInput.Value;
        reachedPoints.Add((int2)(pos.xy/gridSize));
        while (goalsInput.Value.Count > 0 && math.distance(goalsInput.Value[0], pos.xy) < minDistanceToGoal)
        {
            goalsInput.Value.RemoveAt(0);
        }
        if (goalsInput.Value.Count != 0)
        {
            goal = goalsInput.Value[0];
        }
        else
        {
            goal = (int2) pos.xy;
        }
        obstaclesPoints = obstaclesPointsInput.Value;
        path = new List<int2>();
        Threader.RunAsync(ProsesPath); // starts ProsesPath tread
        logMessage.Value = "Started calculation";
        logEvent.Raise();
    }

    /**
     * calculates the path
     */
    private float3 pos;
    private int2 goal;
    private List<int2> obstaclesPoints;
    private List<int2> path;
    void ProsesPath()
    {
        UpdateObsticals();
        
        if (explore)
        {
            Explore();
        }
        string msg;
        try
        {
            if (driveCircle)
            {
                msg = Circle();
            }
            else if (useRotation)
            {
                msg = WithRotation();
            }
            else
            {
                msg = NoRotation();
            }
        }
        catch (Exception e)
        {
            Threader.RunOnMainThread(NoPathFound);
            return;
        }
        
        Threader.RunOnMainThread(ParsePath);
        //sends the path
        void ParsePath()
        {
            logMessage.Value = "Finished calculation";
            logEvent.Raise();
            pathOutput.Value = path;
            sendString.Value = msg;
            sendEvent.Raise();
        }
        
        void NoPathFound()
        {
            logMessage.Value = "No path exists betwene the goals";
            logEvent.Raise();
        }
    }

    private string WithRotation()
    {
        path = new List<int2>();
        FindPath finder = new FindPath(obstacles);
        int2 start = (int2) (pos.xy);
        path.AddRange(finder.findPathBetweenInt2(start, goal));

            // set the msg string to the message to send
        String msg = "roboter multi ";
        float2 old = pos.xy;
        float2 oldRotation = mathAdditions.Rotate(new float2(0,1), pos.z);
        float2 move;
        for (int i = 0; i < path.Count; i++) // append a string to msg for each move in path
        {
            // don't add if the position is in a line with the position before and after 
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
            if (!move.Equals(float2.zero))
            {
                msg += "rotate," + (int)mathAdditions.Angle(oldRotation, move) + ",";
                msg += "move," + (int)math.length(move) + ";0;" + speed + ",";
                oldRotation = (path[i]) - old;
            }
            old = (path[i]);
        }
        move = old - (goal.xy);
        if (!move.Equals(float2.zero))
        {
            msg += "rotate," + (int)mathAdditions.Angle(oldRotation, move) + ",";
            msg += "move," + (int)math.length(move) + ";0;" + speed + ",";
        }

        return msg;
    }
    private string NoRotation()
    {
        path = new List<int2>();
        FindPath finder = new FindPath(obstacles);
        path.AddRange(finder.findPathBetweenInt2((int2) (pos.xy), goal));

            // set the msg string to the message to send
        String msg = "roboter multi ";
        float2 old = pos.xy;
        float2 move;
        for (int i = 0; i < path.Count; i++) // append a string to msg for each move in path
        {
            // don't add if the position is in a line with the position before and after 
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
            if (!move.Equals(float2.zero))
            {
                msg += "move," + (int)move.y + ";" + (int)move.x + ";" + speed + ",";
            }
            old = (path[i]);
        }
        move = old - (goal.xy);
        if (!move.Equals(float2.zero))
        {
            msg += "move," + move.y + ";" + move.x + ",";
        }

        return msg;
    }
    
    public String Circle()
    {
        float2 goal = this.goal[0];
        float2 vec = pos.xy - goal;
        float r = math.distance(goal, pos.xy);
        for (int i = 0; i < 8 * r; i++) // loop the circumference in 1cm parts 
        {
            float2 newPos = mathAdditions.Rotate(vec, 360 * i / (8 * r));
            // add the possition to the path if not alreay there
            if (!path.LastOrDefault().Equals((int2)newPos))
            {
                path.Add((int2)(newPos + pos.xy));
                
                if (obstacles.ContainsKey((int2)(newPos + pos.xy))) {path = new List<int2>(); throw new NoPathExists();}
            }
        }
    
        // set the msg string to the message to send
        String msg = "roboter multi ";
        float2 old = pos.xy;
        float2 oldRotation = mathAdditions.Rotate(new float2(0,1), pos.z);
        float2 move;
        for (int i = 0; i < path.Count; i++) // append a string to msg for each move in path
        {
            // don't add if the position is in a line with the position before and after 
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
            if (!move.Equals(float2.zero))
            {
                msg += "rotate," + (int)mathAdditions.Angle(oldRotation, move) + ",";
                msg += "move," + (int)math.length(move) + ";0;" + speed + ",";
                oldRotation = (path[i]) - old;
            }
            old = (path[i]);
        }
        move = old - (path[path.Count - 1].xy);
        if (!move.Equals(float2.zero))
        {
            msg += "rotate," + (int)mathAdditions.Angle(oldRotation, move) + ",";
            msg += "move," + (int)math.length(move) + ";0;" + speed;
        } 

        return msg;
    }

    private void Explore()
    {
        goal = new int2();
        FindPath finder = new FindPath(obstacles);
        int minX = 0;
        int minY = 0;
        int maxX = 0;
        int maxY = 0;
        foreach (int2 obstacle in obstacles.Keys)
        {
            if (obstacle.x < minX)
            {
                minX = obstacle.x;
            }
            if (obstacle.y < minY)
            {
                minY = obstacle.y;
            }
            if (obstacle.x > maxX)
            {
                maxX = obstacle.x;
            }
            if (obstacle.x > maxY)
            {
                maxY = obstacle.y;
            }
        }

        int2 pos;
        for (int r = 0; r < math.max(maxX-minX, maxY-minY) / gridSize + 2; r++)
        {
            pos = new int2(-r, r);
            while (pos.x <= r)
            {
                pos.x++;
                if (CheckPos((int2)this.pos.xy + pos * gridSize))
                {
                    goal = (int2)this.pos.xy + pos * gridSize;
                    return;
                }
            }
            while (pos.y >= -r)
            {
                pos.y--;
                if (CheckPos((int2)this.pos.xy + pos * gridSize))
                {
                    goal = (int2)this.pos.xy + pos * gridSize;
                    return;
                }
            }
            while (pos.x >= -r)
            {
                pos.x--;
                if (CheckPos((int2)this.pos.xy + pos * gridSize))
                {
                    goal = ((int2)this.pos.xy + pos * gridSize);
                    return;
                }
            }
            while (pos.y <= r)
            {
                pos.y++;
                if (CheckPos((int2)this.pos.xy + pos * gridSize))
                {
                    goal = ((int2)this.pos.xy + pos * gridSize);
                    return;
                }
            }
        }
        bool CheckPos(int2 pos)
        {

            bool b = (minX - 1) < pos.x && pos.x < (maxX + 1) &&
                     (minY - 1) < pos.y && pos.y < (maxY + 1) &&
                     !obstacles.ContainsKey(pos) &&
                     !reachedPoints.Contains(pos/gridSize);
            if (b)
            {
                try
                {
                    finder.findPathBetweenInt2((int2) this.pos.xy, pos);
                }
                catch (NoPathExists e)
                {
                    reachedPoints.Add(pos);
                    return false;
                }
            }
            return b;
        }
    }
    private void UpdateObsticals()
    {
        obstacles = new Dictionary<int2, int>();
        // add circles around all obstaclesPoints 
        foreach (int2 point in obstaclesPoints)
        {
            foreach (int2 p in circle)
            {
                if (!obstacles.ContainsKey(point + p))
                {
                    obstacles.Add(point + p, 1);
                }
            }
        }
    }
    private void OnDrawGizmos()
    {
        
        Gizmos.color = Color.green;
        foreach (int2 point in pathOutput.Value)
        {
            Gizmos.DrawSphere(new Vector3(point.x, point.y), 1);
        }
    }
}
