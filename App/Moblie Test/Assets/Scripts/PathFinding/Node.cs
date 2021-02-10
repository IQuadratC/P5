using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

public class Node
{
    public int2 pos;
    public bool walkable;
    public bool completed = false;
    public Node parent;
    public int gScore = 0;
    public int hScore = 0;
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
        int score = getDistanse(this, newParent);
        if (score < gScore)
        {
            gScore = score;
            parent = newParent;
        }
    }
    
    public static int getDistanse(Node a, Node b)
    {
        int x = a.pos.x - b.pos.x;
        int y = a.pos.y - b.pos.y;
        if (x < y)
        {
            return 14 * x + 10*(y - x);
        }
        return 14 * y + 10*(x - y);
    }
    
}
