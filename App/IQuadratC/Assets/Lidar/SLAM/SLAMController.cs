using System;
using Unity.Mathematics;
using UnityEngine;

namespace Lidar.SLAM
{
    public class SLAMController : MonoBehaviour
    {
        [SerializeField] private int[] mapLevels;
        private SLAMMap map;
        public float MapAcsess(int2 p, int2 p0, int2 p1, int level)
        {
            float a = (p.y - p0.y) / (p1.y - p0.y);
            float b = (p1.y - p.y) / (p1.y - p0.y);
            float c = (p.x - p0.x) / (p1.x - p0.x);
            float d = (p1.x - p.x) / (p1.x - p0.x);

            float x = a * (c * map.GetMap(p1, level) + d * map.GetMap(new int2(p0.x, p1.y), level))
                      + b * (c * map.GetMap(new int2(p1.x, p0.y), level) + d *  map.GetMap(p0, level));
            return x;
        }

        public float2 MapAcsessDirtative(int2 p, int2 p0, int2 p1, int level)
        {
            float a = (p.y - p0.y) / (p1.y - p0.y);
            float b = (p1.y - p.y) / (p1.y - p0.y);
            float c = (p.x - p0.x) / (p1.x - p0.x);
            float d = (p1.x - p.x) / (p1.x - p0.x);

            int m00 = map.GetMap(p0, level);
            int m01 = map.GetMap(new int2(p0.x, p1.y), level);
            int m10 = map.GetMap(new int2(p1.x, p0.y), level);
            int m11 = map.GetMap(p1, level);

            float2 x = new float2(
                a * (m11 - m01) + b * (m10 - m00),
                c * (m11 - m01) + d * (m10 - m00));
            return x;
        }
        
        public float2 Si(float3 t, float2 si)
        {
            float2x2 a = new float2x2(math.cos(t.z), -math.sin(t.z), math.sin(t.z), math.cos(t.z));
            float2 x = math.mul(a, si) + t.xy;
            return x;
        }
    }
}
