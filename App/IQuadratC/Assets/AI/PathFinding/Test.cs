using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Utility.Variables;

public class Test : MonoBehaviour
{
    public int2 start;
    public int2 end;
    public void test()
    {
        Dictionary<int2, int> obstacles = new Dictionary<int2, int>();
        for (int i = 0; i < 100; i++)
        {
            for (int j = 0; j < 100; j++)
            {
                if (i % 5 > 2 && j % 5 > 2)
                {
                    obstacles.Add(new int2(i,j), 1);
                }
                else
                {
                    obstacles.Add(new int2(i,j), 0);
                }
            }
        }
        List<int2> obsticaList = new List<int2>();
        foreach (KeyValuePair<int2,int> obstical in obstacles)
        {
            if (obstical.Value > 0)
            {
                obsticaList.Add(obstical.Key);
            }
        }
        ShowList(obsticaList, obsticalsPointPrefab);

        FindPath path = new FindPath(obstacles);
        ShowList(path.findPathBetweenInt2(start, end), pointPrefab);
    }

    [SerializeField]private Int2ListVariable path;
    [SerializeField]private Vec3Variable pos;

    public void setPos()
    {
        if (path.Value.Count > 0)
        {
            pos.Value.xy = path.Value[path.Value.Count - 1];
        }
    }
    
    [SerializeField]private GameObject parent;
    [SerializeField]private GameObject pointPrefab;
    [SerializeField]private GameObject obsticalsPointPrefab;
    public void ShowList(List<int2> path, GameObject pointPrefab)
    {
        foreach (int2 int2 in path)
        {
            GameObject o = Instantiate(pointPrefab, new Vector3(int2.x,int2.y, 0), Quaternion.identity);
            o.transform.SetParent(parent.transform);
        }
    }
}
