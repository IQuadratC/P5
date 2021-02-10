using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class FindPath : MonoBehaviour
{
    private Dictionary<int2, Node> grid = new Dictionary<int2, Node>();
    private Node start;
    private Node end;
    private bool gotStuck;

    public Node findPath(Node start, Node end)
    {
        this.start = start;
        this.end = end;
        Node current;
        grid[start.pos] = start;
        
        while(!gotStuck)
        {
            current = findLowestF();
            current.completed = true;
            if (current == end)
            {
                return current;
            }

            foreach (int2 neigbor in getNeigbors(current))
            {
                if (!grid[neigbor].completed || grid[neigbor].walkable)
                {
                    grid[neigbor].setGScore(current);
                    grid[neigbor].setHScore(end);
                }
                
            }
            
        }
        return null;
    }

    private Node findLowestF()
    {
        Node lowest = new Node(new int2(0,0), false);
        foreach (var node in grid)
        {
            if (!node.Value.completed && node.Value.FScore < lowest.FScore )
            {
                lowest = node.Value;
            }
        }
        if (lowest == null)
        {
            gotStuck = true;
        }
        return lowest;
    }
    
    private int2[] getNeigbors(Node self)
    {
        int2[] neigbors = new int2[]{};
        int2 pos; 
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                pos.x = i;
                pos.y = j;
                neigbors[3 * (i + 1) + j] = self.pos + pos;
            }
        }

        foreach (int2 neighbor in neigbors)
        {
            if (grid[neighbor] == null)
            {
                grid[neighbor] = new Node(neighbor, true);
            }
        }
        
        return neigbors;
    }
    public void Start()
    {
        Debug.Log(findPath(new Node(int2.zero, true), new Node(new int2(5, 5), true)));
    }
}
