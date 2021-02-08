using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Lidar
{
    public class LidarMap : MonoBehaviour
    {
        public List<LidarPoint> LidarPoints = new List<LidarPoint>();
        public float2 currentPosition = float2.zero;
        public bool moving;

        private bool wasMoving;
        public void AddLidarData(int[,] data)
        {
            LidarPoint lidarPoint;
            if (moving || wasMoving)
            {
                lidarPoint = new LidarPoint();
            }
            else
            {
                lidarPoint = LidarPoints[LidarPoints.Count - 1];
            }
            wasMoving = moving;
            
            lidarPoint.AddData(data);
            lidarPoint.Update();
        }
    }
}
