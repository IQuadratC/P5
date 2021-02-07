using System.Collections.Generic;
using Unity.Mathematics;

namespace Utility
{
    public static class mathAdditions
    {
        public static float GetDistancetoLine(float2 line0Pos, float2 line1Pos, float2 testPos)
        {
            float3 b = new float3(line0Pos - line1Pos, 0);
            return math.length(math.cross(new float3(testPos - line0Pos, 0), b)) / math.length(b);
        }
        
        public static float4 FindLinearLeastSquaresFit(float2[] points)
        {
            double s1 = points.Length;
            double sx = 0;
            double sy = 0;
            double sxx = 0;
            double sxy = 0;
            for (int i = 0; i < points.Length; i++)
            {
                float2 pt = points[i];
                sx += pt.x;
                sy += pt.y;
                sxx += pt.x * pt.x;
                sxy += pt.x * pt.y;
            }
            
            double m = (sxy * s1 - sx * sy) / (sxx * s1 - sx * sx);
            double b = (sxy * sx - sy * sxx) / (sx * sx - s1 * sxx);
            
            return new float4(0, (float)b,1,(float)m);
        }
        
        public static float4 FindLinearLeastSquaresFit(List<float2> points)
        {
            double s1 = points.Count;
            double sx = 0;
            double sy = 0;
            double sxx = 0;
            double sxy = 0;
            foreach (float2 pt in points)
            {
                sx += pt.x;
                sy += pt.y;
                sxx += pt.x * pt.x;
                sxy += pt.x * pt.y;
            }
            
            double m = (sxy * s1 - sx * sy) / (sxx * s1 - sx * sx);
            double b = (sxy * sx - sy * sxx) / (sx * sx - s1 * sxx);
            
            return new float4(0, (float)b,1,(float)m);
        }
        
        public static float2 FindIntersection(float2 p1, float2 p2, float2 p3, float2 p4)
        {
            float dx12 = p2.x - p1.x;
            float dy12 = p2.y - p1.y;
            float dx34 = p4.x - p3.x;
            float dy34 = p4.y - p3.y;
            
            float denominator = dy12 * dx34 - dx12 * dy34;
            float t1 = ((p1.x - p3.x) * dy34 + (p3.y - p1.y) * dx34) / denominator;
            
            if (float.IsInfinity(t1))
            {
                return  new float2(float.NaN, float.NaN);
            }
            
            return new float2(p1.x + dx12 * t1, p1.y + dy12 * t1);
        }

        // https://math.stackexchange.com/questions/878785/how-to-find-an-angle-in-range0-360-between-2-vectors
        public static float Angle(float2 x, float2 y)
        {
            float denominator = x.x * y.y - y.x * x.y;
            return math.degrees(math.atan2(denominator, math.dot(x, y)));
        }

        // https://stackoverflow.com/questions/22818531/how-to-rotate-2d-vector
        public static float2 Rotate(float2 v, float angle)
        {
            float radian = math.radians(angle);
            float ca = math.cos(radian);
            float sa = math.sin(radian);
            return new float2(ca*v.x - sa*v.y, sa*v.x + ca*v.y);
        }
        
        public static float3 LayTowlinesOverEachother(float2 p1, float2 p2, float2 p3, float2 p4)
        {
            float2 dir1 = p1 - p2;
            float2 dir2 = p3 - p4;
            float angle = -Angle(dir1, dir2);

            float2 p5 = p1 - Rotate(p3, angle);
            
            return new float3(p5.x, p5.y, angle);
        }
    }
}