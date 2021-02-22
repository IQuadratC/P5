using pointmatcher.net;
using Unity.Mathematics;
using UnityEngine;
using Quaternion = System.Numerics.Quaternion;
using Vector3 = System.Numerics.Vector3;

namespace Utility.PointMatcher
{
    public static class PointMatcherWrapper
    {
        public static void perform(float3[] points1, float3[] points2, float3 gussPosition, quaternion gussRotation, out float3 outPosition, out quaternion outRotation)
        {    
            DataPoints reading = new DataPoints();
            reading.points = new DataPoint[points1.Length]; // initialize your point cloud reading here
            for (int i = 0; i < points1.Length; i++)
            {
                reading.points[i] = new DataPoint();
                reading.points[i].point = new Vector3(points1[i].x, points1[i].y, points1[i].z);
            }
            
            DataPoints reference = new DataPoints(); // initialize your reference point cloud here
            reference.points = new DataPoint[points2.Length];
            for (int i = 0; i < points2.Length; i++)
            {
                reading.points[i] = new DataPoint();
                reading.points[i].point = new Vector3(points2[i].x, points2[i].y, points2[i].z);
            }
            
            EuclideanTransform initialTransform = new EuclideanTransform(); // your initial guess at the transform from reading to reference
            initialTransform.translation = new Vector3(gussPosition.x, gussPosition.y, gussPosition.z);
            initialTransform.rotation = new Quaternion(gussRotation.value.x, gussRotation.value.y, gussRotation.value.z, gussRotation.value.w);

            ICP icp = new ICP();
            icp.ReadingDataPointsFilters = new RandomSamplingDataPointsFilter(prob: 0.1f);
            icp.ReferenceDataPointsFilters = new SamplingSurfaceNormalDataPointsFilter(SamplingMethod.RandomSampling, ratio: 0.2f);
            icp.OutlierFilter = new TrimmedDistOutlierFilter(ratio: 0.5f);
            EuclideanTransform transform = icp.Compute(reading, reference, initialTransform);
            
            outPosition = new float3(transform.translation.X, transform.translation.Y, transform.translation.Z);
            
            outRotation = new quaternion();
            outRotation.value = new float4(transform.rotation.X, transform.rotation.Y, transform.rotation.Z, transform.rotation.W);
        }
    }
}