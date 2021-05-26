using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine.Rendering.Universal;

namespace Lidar.SLAM
{
    public class SLAMLidarDataSet
    {
        public float2[] points;

        public SLAMLidarDataSet()
        {
            points = new float2[0];
        }
        
        public void AddData(int2[] datas)
        {
            List<float2> newPoints = new List<float2>();
            newPoints.AddRange(points);
            
            for (int i = 0; i < datas.Length; i++)
            {
                float2 position = new float2(
                    math.sin(datas[i].x * math.PI / 180) * ((float)datas[i].y / 10),
                    math.cos(datas[i].x * math.PI / 180) * ((float)datas[i].y / 10));

                if (!position.Equals(float2.zero))
                {
                    newPoints.Add(position);
                }
            }

            points = newPoints.ToArray();
        }
    }
}