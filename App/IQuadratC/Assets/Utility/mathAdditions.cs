using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;

namespace Utility
{
    public static class mathAdditions
    {
        public static float GetDistancetoLine(float3 center, float3 vector, float3 testPos)
        {
            return math.length(math.cross(new float3(testPos - center), vector)) / math.length(vector);
        }
        
        public static float GetDistancetoLine(float2 center, float2 vector, float2 testPos)
        {
            return GetDistancetoLine(
                new float3(center, 0),
                new float3(vector, 0),
                new float3(testPos, 0));
        }
        
        public static float2 FindLinearLeastSquaresFit(float2[] points)
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
            
            return new float2((float)m, (float)b);
        }
        
        public static float2 FindLinearLeastSquaresFit(List<float2> points)
        {
            return FindLinearLeastSquaresFit(points.ToArray());
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

        public static float3 FindClosestPointInArray(float2 point, float2[] refference, bool itself)
        {
            float distance = float.MaxValue;
            float3 bestPoint = float3.zero;
            foreach (float2 testPoint in refference)
            {
                if(!itself && point.Equals(testPoint)) continue;
                
                float newDistance = math.distance(point, testPoint);

                if (newDistance >= distance) continue;
                distance = newDistance;
                bestPoint.xy = testPoint;
            }

            bestPoint.z = distance;
            return bestPoint;
        }

        public static float3 PointBasedMatching(List<float4> neigborList)
        {
            float x_mean = 0;
            float y_mean = 0;
            float xp_mean = 0;
            float yp_mean = 0;
            int n = neigborList.Count;

            if (n == 0)
            {
                return float3.zero;
            }

            foreach (float4 pair in neigborList)
            {
                x_mean += pair.x;
                y_mean += pair.y;
                xp_mean += pair.z;
                yp_mean += pair.w;
            }

            x_mean /= n;
            y_mean /= n;
            xp_mean /= n;
            yp_mean /= n;

            float s_x_xp = 0;
            float s_y_yp = 0;
            float s_x_yp = 0;
            float s_y_xp = 0;

            foreach (float4 pair in neigborList)
            {
                s_x_xp += (pair.x - x_mean) * (pair.z - xp_mean);
                s_y_yp += (pair.y - y_mean) * (pair.w - yp_mean);
                s_x_yp += (pair.x - x_mean) * (pair.w - yp_mean);
                s_y_xp += (pair.y - y_mean) * (pair.z - xp_mean);
            }

            float rot_angle = math.atan2(s_x_yp - s_y_xp, s_x_xp + s_y_yp);
            float translation_x = xp_mean - (x_mean * math.cos(rot_angle) - y_mean * math.sin(rot_angle));
            float translation_y = yp_mean - (x_mean * math.sin(rot_angle) + y_mean * math.cos(rot_angle));

            return new float3(translation_x, translation_y, math.degrees(rot_angle));
        }
    }
}