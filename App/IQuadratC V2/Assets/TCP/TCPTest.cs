using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility.Events;
using Utility.Variables;

public class TCPTest : MonoBehaviour
{
    [SerializeField] private StringVariable sendString;
    [SerializeField] private GameEvent sendEvent;
    [SerializeField] private StringVariable reciveString;
    
    [SerializeField] private float requestInterval = 5.0f;


    private float lastRequest;

    private void Start()
    {
        lastRequest = Time.time;
        posList = new List<Vector2>();
    }

    private void Update()
    {
        float time = Time.time;
        if (lastRequest + requestInterval < time)
        {
            lastRequest = time;

            sendString.Value = "roboter position";
            sendEvent.Raise();
            
            sendString.Value = "lidar mapdata";
            sendEvent.Raise();
        }
    }

    private bool reciving;
    private List<Vector2> posList;
    private Vector2 roboterPos;

    public void Receive()
    {
        String[] texts = reciveString.Value.Split(' ');

        if (texts[0] == "roboter")
        {
            if (texts[1] == "position")
            {
                String[] xy = texts[2].Split(',');
                roboterPos = new Vector2(float.Parse(xy[0]), float.Parse(xy[1]));
            }
        }
        
        if (texts[0] == "lidarmap")
        {
            if (texts[1] == "data")
            {
                if (!reciving)
                {
                    posList.Clear();
                }

                String[] points = texts[2].Split(',');

                foreach (var point in points)
                {
                    String[] xy = point.Split(';');
                    posList.Add(new Vector2(float.Parse(xy[0]), float.Parse(xy[1])));
                }
            
                reciving = true;
            }
        
            if (texts[1] == "end")
            {
                reciving = false;
            }
        }

        
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            return;
        }
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(roboterPos, 1);

        Gizmos.color = Color.cyan;
        foreach (Vector2 vector2 in posList)
        {
            Gizmos.DrawSphere(vector2, 1);
        }
    }
}
