using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Utility.Variables;

namespace Lidar
{
    public class LidarMap : MonoBehaviour
    {
        public List<LidarPoint> bigLidarPoints;
        public List<LidarPoint> smallLidarPoints;
        private List<LidarPoint> lidarPointsProcessing;
        public float3 currentPosition;
        public BoolVariable bigLidarPointActive;
        private bool wasBigLidarPointActive;
        public bool simulateData;

        private void Awake()
        {
            bigLidarPoints = new List<LidarPoint>();
        }

        public void AddLidarData(int[,] data)
        {
            if (bigLidarPointActive.Value)
            {
                LidarPoint lidarPoint;
                if (!wasBigLidarPointActive)
                {
                    lidarPoint = new LidarPoint
                    {
                        bigLidarPoint = true,
                    };
                    
                    lidarPointsProcessing.Add(lidarPoint);
                    wasBigLidarPointActive = true;
                }
                else
                {
                    lidarPoint = bigLidarPoints[bigLidarPoints.Count - 1];
                }
                lidarPoint.AddData(data);
            }
            else
            {
                LidarPoint lidarPoint = new LidarPoint
                {
                    bigLidarPoint = false,
                };
                
                lidarPoint.AddData(data);
                lidarPoint.state = LidarPointState.waitingForUpdate;
                
                lidarPointsProcessing.Add(lidarPoint);
            }
        }

        private void Update()
        {
            
            
            LidarPoint lastBigLidarPoint = bigLidarPoints[bigLidarPoints.Count - 1];
            for (int i = lidarPointsProcessing.Count - 1; i >= 0; i--)
            {
                LidarPoint lidarPoint = lidarPointsProcessing[i];
                switch (lidarPoint.state)
                {
                    case LidarPointState.addingData:
                        if (lidarPoint.bigLidarPoint && wasBigLidarPointActive && !bigLidarPointActive)
                        {
                            lidarPoint.state = LidarPointState.waitingForUpdate;
                        }
                        break;
                    
                    case LidarPointState.waitingForUpdate:
                        lidarPoint.Update(lastBigLidarPoint);
                        break;
                    
                    case LidarPointState.performingUpdate:
                        lidarPoint.ParseOverlay();
                        break;
                    
                    case LidarPointState.finished:
                        lidarPointsProcessing.Remove(lidarPoint);
                        if (lidarPoint.bigLidarPoint)
                        {
                            bigLidarPoints.Add(lidarPoint);
                        }
                        else
                        {
                            smallLidarPoints.Add(lidarPoint);
                            currentPosition = smallLidarPoints[smallLidarPoints.Count - 1].overlay.xyz;
                        }
                        break;
                }
            }
        }

        public void SimulateData()
        {
            
        }
    }
}
