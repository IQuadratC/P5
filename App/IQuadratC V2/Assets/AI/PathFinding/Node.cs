using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class Node : IHeapItem<Node>
{
    public int HeapIndex { get; set; }

    public int CompareTo(Node other)
    {
        return other.FScore - this.FScore;
    }
    public int2 pos;
    public bool walkable;
    public bool completed = false;
    public Node parent;
    public int gScore = int.MaxValue/4;
    public int hScore = int.MaxValue/4;
    public int FScore => gScore + hScore;

    public Node(int2 pos, bool walkable)
    {
        this.pos = pos;
        this.walkable = walkable;
    }

    public void setHScore(Node end)
    {
        int score = getDistanse(this, end);
        if (score < hScore)
        {
            hScore = score;
        }
    }
    
    public void setGScore(Node newParent)
    {
        int score = newParent.gScore + getDistanse(this, newParent);
        if (score < gScore)
        {
            gScore = score;
            parent = newParent;
        }
    }
    
    public static int getDistanse(Node a, Node b)
    {
        int x = math.abs(a.pos.x - b.pos.x);
        int y = math.abs(a.pos.y - b.pos.y);
        if (x < y)
        {
            return 14 * x + 10*(y - x);
        }
        return 14 * y + 10*(x - y);
    }

    public static List<int2> getPath(Node end)
    {
        List<int2> path = new List<int2>();
        Node oldNode = null;
        Node newNode = end;
        while (oldNode != newNode)
        {
            path.Insert(0, newNode.pos);
            oldNode = newNode;
            newNode = newNode.parent;
        }

        return path;
    }
    
}
