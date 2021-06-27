using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Utility.Events;
using Utility.Variables;

namespace Lidar.V3
{
    public class LidarControllerV3 : MonoBehaviour
    {
        [SerializeField] private StringVariable sendString;
        [SerializeField] private GameEvent sendEvent;
        [SerializeField] private StringVariable reciveString;
        
        [SerializeField] private float requestInterval = 5.0f;

        [SerializeField] private Int2ListVariable lidarPoints;
        [SerializeField] private Vec3Variable robotPos;

        [SerializeField] private GameEvent newPointsEvent;
        [SerializeField] private GameEvent reqestPointsEvent;

        private float lastRequest;

        private void Start()
        {
            lastRequest = Time.time;
        }

        private void Update()
        {
            float time = Time.time;
            if (lastRequest + requestInterval < time)
            {
                lastRequest = time;

                reqestPointsEvent.Raise();
            }
        }

        public void requestData()
        {
            sendString.Value = "roboter position";
            sendEvent.Raise();
                
            sendString.Value = "lidar mapdata";
            sendEvent.Raise();
        }

        private bool reciving;

        public void Receive()
        {
            String[] texts = reciveString.Value.Split(' ');

            if (texts[0] == "roboter")
            {
                if (texts[1] == "position")
                {
                    String[] xy = texts[2].Split(',');
                    robotPos.Value = new Vector3(float.Parse(xy[0]), float.Parse(xy[1]), float.Parse(xy[2]));
                }
            }
            
            if (texts[0] == "lidarmap")
            {
                if (texts[1] == "data")
                {
                    String[] points = texts[2].Split(',');

                    foreach (var point in points)
                    {
                        String[] xy = point.Split(';');
                        lidarPoints.Value.Add(new int2(int.Parse(xy[0]), int.Parse(xy[1])));
                    }
                
                    reciving = true;
                }
            
                if (texts[1] == "end")
                {
                    reciving = false;
                    newPointsEvent.Raise();
                }
            }
        }
    }
}