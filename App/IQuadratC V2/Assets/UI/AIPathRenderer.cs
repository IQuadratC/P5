using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Utility.Variables;

public class AIPathRenderer : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private Int2ListVariable path;
    void Update()
    {
        lineRenderer.positionCount = path.Value.Count;
        for (int i = 0; i < path.Value.Count; i++)
        {
            lineRenderer.SetPosition(i, new float3(path.Value[i], 0));
        }
    }
}
