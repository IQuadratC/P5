using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class FindPath
{
    private Dictionary<int2, Node> grid = new Dictionary<int2, Node>();
    private Dictionary<int2,int> obstacles;
    private Heap<Node> heap;
    private int maxLoops;

    public FindPath(Dictionary<int2,int> obstacles)
    {
        this.obstacles = obstacles;
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
        maxLoops = (maxX - minX) * (maxY - minY) + 10;
        heap = new Heap<Node>(maxLoops);
    }
    public Node findPathBetweneNodes(Node start, Node end)
    {
        Node current = start;
        current.parent = current;
        current.gScore = 0;
        current.setHScore(end);
        current.completed = true;
        grid[start.pos] = start;
        heap.Add(current);
        
        int security = 0;
        while(security < maxLoops)
        {
            security++;
            current = heap.RemoveFirst();
            current.completed = true;
            if (current.pos.Equals(end.pos))
            {
                return current;
            }

            foreach (int2 neigbor in getNeigbors(current))
            {
                if (!grid[neigbor].completed && grid[neigbor].walkable)
                {
                    grid[neigbor].setGScore(current);
                    grid[neigbor].setHScore(end);
                    heap.UpdateItem(grid[neigbor]);
                }
            }
        }
        throw new Exception("didn't return inside Loop");
    }
    
    private int2[] getNeigbors(Node self)
    {
        List<int2> neigbors = new List<int2>();
        int2 pos;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (i == 0 && j == 0){continue;}
                pos.x = i;
                pos.y = j;
                pos += self.pos;
                if (!grid.ContainsKey(pos))
                {
                    grid[pos] = new Node(pos, !(obstacles.ContainsKey(pos)) || obstacles[pos] < 1);
                    if (grid[pos].walkable)
                    {
                        heap.Add(grid[pos]);
                    }
                }
                
                neigbors.Add(pos);
            }
        }

        return neigbors.ToArray();
    }

    public List<int2> findPathBetweenInt2(int2 start, int2 end)
    {
        return Node.getPath(findPathBetweneNodes(new Node(start, true),
            new Node(end, true)));
    }
}

[Serializable]
public class NoPathExists : Exception
{
    public NoPathExists () {}

    public NoPathExists (string message) : base(message) {}

    public NoPathExists (string message, Exception innerException) : base(message, innerException) {}    
}
