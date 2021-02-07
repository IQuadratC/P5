using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Utility;

namespace Lidar
{
    public class ProcessLidarData : MonoBehaviour
    {
        public GameObject whitespherePreFab;
        public GameObject redspherePreFab;
        public GameObject bluespherePreFab;
        public GameObject greenspherePreFab;
        public GameObject lightbluespherePreFab;
        public GameObject redPartent;
        public GameObject bluePartent;
        public GameObject greenPartent;
        public GameObject lightbluePartent;
        public GameObject linePreFab;
        
        private void Start()
        {
            LidarPoint lidarPoint = new LidarPoint();
            lidarPoint.LoadCSV("Testdata_notMove.csv");
            
            LidarPoint lidarPoint1 = new LidarPoint();
            lidarPoint1.LoadCSV("Testdata_Move_20cm.csv");
            
            LidarPoint lidarPoint2 = new LidarPoint();
            lidarPoint2.LoadCSV("Testdata_Move_40cm.csv");
            
            foreach (Vector2 lidarPointPosition in lidarPoint.positions)
            {
                GameObject o = Instantiate(redspherePreFab, lidarPointPosition, Quaternion.identity);
                o.transform.SetParent(redPartent.transform, true);
            }
            foreach (Vector2 lidarPointPosition in lidarPoint1.positions)
            {
                GameObject o = Instantiate(bluespherePreFab, lidarPointPosition, Quaternion.identity);
                o.transform.SetParent(bluePartent.transform, true);
            }
            foreach (Vector2 lidarPointPosition in lidarPoint2.positions)
            {
                GameObject o = Instantiate(greenspherePreFab, lidarPointPosition, Quaternion.identity);
                o.transform.SetParent(greenPartent.transform, true);
            }
            
            foreach (Vector2 intersectionPoint in lidarPoint.intersections)
            {
                GameObject o = Instantiate(whitespherePreFab, intersectionPoint, Quaternion.identity);
                o.transform.SetParent(redPartent.transform, true);
            }
            
            foreach (Vector2 intersectionPoint in lidarPoint1.intersections)
            {
                GameObject o = Instantiate(whitespherePreFab, intersectionPoint, Quaternion.identity);
                o.transform.SetParent(bluePartent.transform, true);
            }
            
            float4 overlap1 = OverlayTwoLidarPoints(lidarPoint, lidarPoint1);
            bluePartent.transform.position = new Vector3(overlap1.x, overlap1.y, 0);
            bluePartent.transform.eulerAngles = new Vector3(0,0,overlap1.z);
            
            float4 overlap2 = OverlayTwoLidarPoints(lidarPoint, lidarPoint2);
            greenPartent.transform.position = new Vector3(overlap2.x, overlap2.y, 0);
            greenPartent.transform.eulerAngles = new Vector3(0,0,overlap2.z);
            
        }
        
        private void DrawLine(Vector4 line, Color color, int bounds)
        {
            Vector3 pos = new Vector3(line.x - line.z * bounds, line.y - line.w * bounds, 0);
            Vector3 dir = new Vector3(line.z, line.w) * bounds * 2;
            Debug.DrawRay(pos, dir, color, 10000000000, false);
        }

        
        private float4 OverlayTwoLidarPoints(LidarPoint lidarPoint, LidarPoint lidarPoint1)
        {
            int count = lidarPoint.intersections.Count;
            int count1 = lidarPoint1.intersections.Count;
            
            NativeArray<float2> intersections = new NativeArray<float2>(count, Allocator.TempJob);
            NativeArray<float2> intersections1 = new NativeArray<float2>(count1, Allocator.TempJob);

            for (int i = 0; i < count; i++) { intersections[i] = lidarPoint.intersections[i]; }
            for (int i = 0; i < count1; i++) { intersections1[i] = lidarPoint1.intersections[i]; }
            
            NativeArray<float4> overlays = new NativeArray<float4>(count, Allocator.TempJob);

            ProcessOverlayJob processOverlayJob = new ProcessOverlayJob();
            processOverlayJob.intersections = intersections;
            processOverlayJob.intersections1 = intersections1;
            processOverlayJob.overlays = overlays;

            processOverlayJob.Schedule(count, 1).Complete();

            /*for (int i = 0; i < count; i++)
            {
                processOverlayJob.Execute(i);
            }*/
            intersections.Dispose();
            intersections1.Dispose();

            float4 finalOverlay = new float4(0,0,0,float.MaxValue);
            for (int i = 0; i < overlays.Length; i++)
            {
                if(overlays[i].w >= finalOverlay.w) continue;
                finalOverlay = overlays[i];
            }

            overlays.Dispose();

            return finalOverlay;
        }
        
        [BurstCompile]
        private struct ProcessOverlayJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<float2> intersections;
            [ReadOnly] public NativeArray<float2> intersections1;
            public NativeArray<float4> overlays;
            public void Execute(int index)
            {
                float4 finalOverlay = new float4(0,0,0,float.MaxValue);
                float2 point = intersections[index];
                for (int i = 0; i < intersections.Length; i++)
                {
                    float2 point1 = intersections[i];
                    if (point.Equals(point1)) continue;

                    float distance = math.length(point - point1);
                    float bestDistance = float.MaxValue;
                    float2 bestPoint = float2.zero;
                    float2 bestPoint1 = float2.zero;

                    for (int j = 0; j < intersections1.Length; j++)
                    {
                        float2 point2 = intersections1[j];
                        for (int k = 0; k < intersections1.Length; k++)
                        {
                            float2 point3 = intersections1[k];
                            if (point2.Equals(point3)) continue;

                            float testDistance = math.abs(distance - math.length(point2 - point3));

                            if (testDistance >= bestDistance) continue;
                            bestDistance = testDistance;
                            bestPoint = point2;
                            bestPoint1 = point3;
                        }
                    }

                    NativeArray<float2> points = new NativeArray<float2>(4*4, Allocator.Temp);
                    points[0] = point; points[1] = point1; points[2] = bestPoint; points[3] = bestPoint1; 
                    points[4] = point; points[5] = point1; points[6] = bestPoint1; points[7] = bestPoint; 
                    points[8] = point1; points[9] = point; points[10] = bestPoint1; points[11] = bestPoint; 
                    points[12] = point1; points[13] = point; points[14] = bestPoint; points[15] = bestPoint1;

                    bestDistance = float.MaxValue;
                    float4 overlay = new float4();
                    for (int j = 0; j < 4; j++)
                    {
                        float3 result = mathAdditions.LayTowlinesOverEachother(
                            points[j * 4 + 0], points[j * 4 + 1],
                            points[j * 4 + 2], points[j * 4 + 3]);

                        float2 testVector = mathAdditions.Rotate(points[j * 4 + 3], result.z) + new float2(result.x, result.y);
                        float testdistance = math.length(testVector - points[j * 4 + 1]);

                        if (testdistance >= bestDistance) continue;
                        bestDistance = testdistance;
                        overlay = new float4(result.x, result.y, result.z, 0);
                    }
                    points.Dispose();

                    for (int j = 0; j < intersections.Length; j++)
                    {
                        float2 testpoint = intersections[j];
                        float changedDistance = float.MaxValue;
                        for (int k = 0; k < intersections.Length; k++)
                        {
                            float2 testpoint1 = intersections[k];
                            float testChangedDistance =
                                math.length(testpoint - mathAdditions.Rotate(testpoint1, overlay.z) + overlay.xy);

                            if (testChangedDistance < changedDistance)
                            {
                                changedDistance = testChangedDistance;
                            }
                        }

                        overlay.w += changedDistance;
                    }

                    if (overlay.w >= finalOverlay.w) continue;
                    finalOverlay = overlay;
                }
                
                overlays[index] = finalOverlay;
            } 
        }
        
        private static int SortListByAccuracy(float4 x, float4 y)
        {
            if (x.w < y.w)
            {
                return -1;
            }
            return x.w > y.w ? 1 : 0;
        }

        /*private List<Overlap> overlaps;
        private Vector2 OverlapIntersectionPoints(LidarPoint lidarPoint, LidarPoint lidarPoint1)
        {
            overlaps = new List<Overlap>();
            foreach (Vector2 intersectionPoint in lidarPoint.intersections)
            {
                foreach (Vector2 testIntersectionPoint in lidarPoint1.intersections)
                {
                    Overlap overlap = new Overlap();
                    overlap.pos = intersectionPoint - testIntersectionPoint;
                    overlap.accuracy = 0;

                    foreach (Vector2 testpoint in lidarPoint.intersections)
                    {
                        float distance = float.MaxValue;
                        foreach (Vector2 testpoint1 in lidarPoint1.intersections)
                        {
                            float testdistance = (testpoint - testpoint1 - overlap.pos).magnitude;
                            if (testdistance < distance)
                            {
                                distance = testdistance;
                            }
                        }
                        overlap.accuracy += distance;
                    }
                    overlaps.Add(overlap);
                }
            }
            overlaps.Sort(SortListByAccuracy);

            return overlaps[0].pos;
        }
        private static int SortListByAccuracy(Overlap x, Overlap y)
        {
            if (x.accuracy < y.accuracy)
            {
                return -1;
            }
            return x.accuracy > y.accuracy ? 1 : 0;
        }

        private Overlap OverlapIntersectionPointsWithRotation(LidarPoint lidarPoint, LidarPoint lidarPoint1)
        {
            overlaps = new List<Overlap>();
            foreach (Vector2 point in lidarPoint.intersections)
            {
                foreach (Vector2 point1 in lidarPoint.intersections)
                {
                    if(point == point1) continue;
                    
                    float distance = (point - point1).magnitude;
                    float bestDistance = float.MaxValue;
                    Vector2 bestPoint = Vector2.zero;
                    Vector2 bestPoint1 = Vector2.zero;

                    foreach (Vector2 point2 in lidarPoint1.intersections)
                    {
                        foreach (Vector2 point3 in lidarPoint1.intersections)
                        {
                            if(point2 == point3) continue;
                            
                            float testDistance = Math.Abs(distance - (point2 - point3).magnitude);

                            if (testDistance >= bestDistance) continue;
                            bestDistance = testDistance;
                            bestPoint = point2;
                            bestPoint1 = point3;
                        }
                    }

                    Vector2[,] points =
                    {
                        {point, point1, bestPoint, bestPoint1},
                        {point, point1, bestPoint1, bestPoint},
                        {point1, point, bestPoint1, bestPoint},
                        {point1, point, bestPoint, bestPoint1}
                    };

                    bestDistance = float.MaxValue;
                    Overlap overlap = new Overlap();
                    for (int i = 0; i < 4; i++)
                    {
                        Vector3 result = LayTowlinesOverEachother(
                            points[i, 0], points[i, 1], 
                            points[i, 2], points[i, 3]);
                        
                        Vector2 testVector = RotateVector(points[i, 3], result.z) + new Vector2(result.x, result.y);
                        float testdistance = (testVector - points[i, 1]).magnitude;
                        
                        if(testdistance >= bestDistance) continue;
                        bestDistance = testdistance;
                        overlap.pos = new Vector2(result.x, result.y);
                        overlap.rotation = result.z;
                    }

                    foreach (Vector2 testpoint in lidarPoint.intersections)
                    {
                        
                        float changedDistance = float.MaxValue;
                        foreach (Vector2 testpoint1 in lidarPoint1.intersections)
                        {
                            float testChangedDistance = (testpoint - RotateVector(testpoint1, overlap.rotation) - overlap.pos).magnitude;
                            if (testChangedDistance < changedDistance)
                            {
                                changedDistance = testChangedDistance;
                            }
                        }

                        overlap.accuracy += changedDistance;
                    }
                    overlaps.Add(overlap);
                }
            }
            overlaps.Sort(SortListByAccuracy);

            return overlaps[0];
        }

        private Vector3 LayTowlinesOverEachother(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            Vector2 dir1 = p1 - p2;
            Vector2 dir2 = p3 - p4;
            float angle = -Vector2.Angle(dir1, dir2);
            Vector2 p5 = p1 - RotateVector(p3, angle);
            
            return new Vector3(p5.x, p5.y, angle);
        }

        private Vector2 RotateVector(Vector2 vector, float angle)
        {
            return Quaternion.Euler(0, 0, angle) * vector;
        }
        */

    }
}