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

    public FindPath(Dictionary<int2,int> obstacles)
    {
        this.obstacles = obstacles;
        heap = new Heap<Node>(1000);
    }
    public Node findPathBetweneNodes(Node start, Node end)
    {
        Node current = start;
        current.setGScore(current);
        current.setHScore(end);
        grid[start.pos] = start;
        heap.Add(current);
        
        int security = 0;
        while(security < 1000 && current != null)
        {
            security++;
            current = heap.RemoveFirst();
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
        return null;
    }

    private Node findLowest()
    {
        Node lowest = null;
        int fscore = int.MaxValue;
        int hscore = int.MaxValue;
        foreach (var node in grid)
        {
            if (!node.Value.completed && node.Value.FScore < fscore && node.Value.walkable)
            {
                lowest = node.Value;
                fscore = node.Value.FScore;
            }
            else if (!node.Value.completed && node.Value.FScore == fscore && node.Value.FScore < hscore && node.Value.walkable)
            {
                lowest = node.Value;
                fscore = node.Value.FScore;
                hscore = node.Value.hScore;
            }

        }
        return lowest;
    }
    
    private int2[] getNeigbors(Node self)
    {
        int2[] neigbors = new int2[8];
        int2 pos;
        int k = 0;
        for (int i = -1; i < 2; i++)
        {
            for (int j = -1; j < 2; j++)
            {
                if (i == j && j == 0){continue;}

                pos.x = i;
                pos.y = j;
                neigbors[k] = self.pos + pos;
                k++;
            }
        }

        foreach (int2 neighbor in neigbors)
        {
            if (!grid.ContainsKey(neighbor))
            {
                grid[neighbor] = new Node(neighbor, !obstacles.ContainsKey(neighbor) || obstacles[neighbor] < 1);
                heap.Add(grid[neighbor]);
            }
        }
        
        return neigbors;
    }

    public List<int2> findPathBetweenInt2(int2 start, int2 end)
    {
        return Node.getPath(findPathBetweneNodes(new Node(start, true),
            new Node(end, true)));
    }
}
