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
    private Dictionary<int2,int> obsticals;

    public Node findPath(Node _start, Node _end)
    {
        start = _start;
        end = _end;
        Node current = start;
        start.setGScore(start);
        start.setHScore(end);
        grid[start.pos] = start;
        int security = 0;
        while(!gotStuck && security < 1000)
        {
            security++;
            current = findLowest();
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
                }
                
            }
            
        }
        return current;
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
        if (lowest == null)
        {
            gotStuck = true;
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
                if (i == j && j == 0) {continue; }

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
                grid[neighbor] = new Node(neighbor, !obsticals.ContainsKey(neighbor) || obsticals[neighbor] < 1);
            }
        }
        
        return neigbors;
    }
    public void Test()
    {
        obsticals = new Dictionary<int2, int>();
        for (int i = 0; i < 100; i++)
        {
            for (int j = 0; j < 100; j++)
            {
                if (i % 5 > 2 && j % 5 > 2)
                {
                    obsticals.Add(new int2(i,j), 1);
                }
                else
                {
                    obsticals.Add(new int2(i,j), 0);
                }
            }
        }
        List<int2> obsticaList = new List<int2>();
        foreach (KeyValuePair<int2,int> obstical in obsticals)
        {
            if (obstical.Value > 0)
            {
                obsticaList.Add(obstical.Key);
            }
        }
        SowList(obsticaList, obsticalsPointPrefab);
        
        SowList(Node.getPath(findPath(new Node(int2.zero, true), new Node(new int2(5, 5), true))), pointPrefab);
    }

    [SerializeField]private GameObject parent;
    [SerializeField]private GameObject pointPrefab;
    [SerializeField]private GameObject obsticalsPointPrefab;
    public void SowList(List<int2> path, GameObject pointPrefab)
    {
        foreach (int2 int2 in path)
        {
            GameObject o = Instantiate(pointPrefab, new Vector3(int2.x,int2.y, 0), Quaternion.identity);
            o.transform.SetParent(parent.transform);
        }
    }
}
