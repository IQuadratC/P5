using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Utility;

namespace Lidar
{
    public enum LidarPointState
    {
        addingData = 1,
        waitingForUpdate = 2,
        performingUpdate = 3,
        finished = 4
    }
    
    public class LidarPoint
    {
        public LidarPointState state = LidarPointState.addingData;
        public bool bigLidarPoint;
        
        private readonly List<float>[] distances;
        public readonly float2[] positions;
        public readonly List<List<float2>> lines;
        public List<float2> intersections;
        public float4 overlay;

        public LidarPoint(bool bigLidarPoint)
        {
            this.bigLidarPoint = bigLidarPoint;
            distances = new List<float>[360];
            for (int i = 0; i < distances.Length; i++)
            {
                distances[i] = new List<float>();
            }
            
            positions = new float2[360];
            lines = new List<List<float2>>();
            intersections = new List<float2>();
        }

        public void LoadCsv(string name)
        {
            string[][] csvData = Csv.ParseCVSFile(File.ReadAllText(Application.dataPath + "\\" + name));

            for (int i = 0; i < distances.Length; i++)
            {
                distances[i] = new List<float>();
            }
            
            foreach (string[] line in csvData)
            {
                if(line.Length < 2) continue;
                    
                int index = (int) float.Parse(line[0]);
                float data = int.Parse(line[1]);
                distances[index].Add(data / 10);
            }
        }
        public void AddData(List<int2> data)
        {
            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].y == 0.0f) continue;
                distances[data[i].x].Add((float)data[i].y / 10);
            }
        }

        private LidarPoint otherLidarPoint;
        public void Update(LidarPoint otherLidarPoint)
        {
            state = LidarPointState.performingUpdate;
            this.otherLidarPoint = otherLidarPoint;
            UpdatePositions();
            UpdateLines();
            UpdateIntersections();
            UpdateOverlay();
        }
        
        public void Update()
        {
            state = LidarPointState.performingUpdate;
            UpdatePositions();
            UpdateLines();
            UpdateIntersections();
            state = LidarPointState.finished;
        }
        
        private void UpdatePositions()
        {
            for (int i = 0; i < distances.Length; i++)
            {
                float sum = distances[i].Sum();
                if (sum > 0)
                {
                    sum /= distances[i].Count;
                }
                else { continue; }
                
                positions[i] = new float2(
                    math.sin(i * math.PI / 180) * sum,
                    math.cos(i * math.PI / 180) * sum);
            }
        }

        private const float maxDistance = 1f;
        private const int minLineLength = 5;
        private void UpdateLines()
        {
            List<List<float2>> lineList = new List<List<float2>>();
            for (int i = 0; i < positions.Length; i++)
            {
                if(positions[i].Equals(float2.zero)) continue;

                int linePos = i - 1;
                
                if (linePos < 0)
                {
                    linePos = positions.Length - 1;
                }

                List<float2> line = new List<float2>();
                for (int j = 0; j < positions.Length; j++)
                {
                    int testPos = i + j;
                    if (testPos >= positions.Length)
                    {
                        testPos -= positions.Length;
                    }
                    if (positions[testPos].Equals(float2.zero)) continue;
                    
                    
                    float distance = mathAdditions.GetDistancetoLine(
                        positions[i],
                        positions[linePos],
                        positions[testPos]);

                    if (distance < maxDistance)
                    {
                        line.Add(positions[testPos]);
                    }
                    else
                    {
                        break;
                    }
                }

                for (int j = 1; j < positions.Length; j++)
                {
                    int testPos = i - j;
                    if (testPos < 0)
                    {
                        testPos += positions.Length;
                    }
                    if (positions[testPos].Equals(float2.zero)) continue;
                    
                    float distance = mathAdditions.GetDistancetoLine(
                        positions[i],
                        positions[linePos],
                        positions[testPos]);

                    if (distance < maxDistance)
                    {
                        line.Add(positions[testPos]);
                    }
                    else
                    {
                        break;
                    }
                }
                
                lineList.Add(line);
            }
            
            lineList.Sort(SortListByLenght);
            
            for (int k = 0; k < 100000; k++)
            {
                List<float2> line = lineList[0];
                lines.Add(line);
                lineList.RemoveAt(0);

                foreach (List<float2> testline in lineList)
                {
                    foreach (float2 point in line)
                    {
                        for (int j = testline.Count - 1; j >= 0; j--)
                        {
                            if (testline[j].Equals(point))
                            {
                                testline.RemoveAt(j);
                            }
                        }
                    }
                }
                
                lineList.Sort(SortListByLenght);

                for (int i = 0; i < lineList.Count; i++)
                {
                    if (lineList[i].Count > minLineLength) continue;
                
                    lineList.RemoveRange(i, lineList.Count - i);
                    break;
                }

                if (lineList.Count <= 0)
                {
                    break;
                }
            }
            
            foreach (List<float2> line in lines)
            {
                float4 v = mathAdditions.FindLinearLeastSquaresFit(line);
                line.Add(new float2(v.x, v.y));
                line.Add(new float2(v.z, v.w));
            }
        }
        private static int SortListByLenght(List<float2> x, List<float2> y)
        {
            int xLenght = x.Count;
            int yLenght = y.Count;
            
            if (xLenght > yLenght)
            {
                return -1;
            }
            return xLenght < yLenght ? 1 : 0;
        }

        private const int bounds = 500;
        private void UpdateIntersections()
        {
            intersections = new List<float2>();
            foreach (List<float2> line in lines)
            {
                float4 finalLine = new float4(line[line.Count-2], line[line.Count-1]);
                foreach (List<float2> line1 in lines)
                {
                    if(line == line1) continue;
                        
                    float4 testFinalLine = new float4(line1[line1.Count-2], line1[line1.Count-1]);
                    float2 intersection = mathAdditions.FindIntersection(
                        new float2(finalLine.x, finalLine.y), 
                        new float2(finalLine.x + finalLine.z, finalLine.y + finalLine.w), 
                        new float2(testFinalLine.x, testFinalLine.y), 
                        new float2(testFinalLine.x + testFinalLine.z, testFinalLine.y + testFinalLine.w));
                    
                    bool isNotInList = true;
                    foreach (float2 intersectionPoint in intersections)
                    {
                        if (math.length(intersectionPoint - intersection) < 1.0f)
                        {
                            isNotInList = false;
                        }
                    }

                    if (math.length(intersection) < bounds && isNotInList)
                    {
                        intersections.Add(intersection);
                    }
                }
            }
        }

        private bool jobActive = false;
        private NativeArray<float2> nativeIntersections;
        private NativeArray<float2> nativeIntersections1;
        private NativeArray<float4> overlays;
        private JobHandle overlayHandle;
        private void UpdateOverlay()
        {
            int count = intersections.Count;
            int count1 = otherLidarPoint.intersections.Count;
            
            nativeIntersections = new NativeArray<float2>(count, Allocator.Persistent);
            nativeIntersections1 = new NativeArray<float2>(count1, Allocator.Persistent);

            for (int i = 0; i < count; i++) { nativeIntersections[i] = intersections[i]; }
            for (int i = 0; i < count1; i++) { nativeIntersections1[i] = otherLidarPoint.intersections[i]; }
            
            overlays = new NativeArray<float4>(count, Allocator.Persistent);

            ProcessOverlayJob processOverlayJob = new ProcessOverlayJob();
            processOverlayJob.intersections = nativeIntersections;
            processOverlayJob.intersections1 = nativeIntersections1;
            processOverlayJob.overlays = overlays;

            if (jobActive)
            {
                overlayHandle = processOverlayJob.Schedule(count, 1);
            }
            else
            {
                for (int i = 0; i < nativeIntersections.Length; i++)
                {
                    processOverlayJob.Execute(i);
                }
            }
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
                    
                    float4 overlay = new float4(0,0,0, float.MaxValue);
                    overlay = FindBestOverlay(point, point1, bestPoint, bestPoint1, overlay);
                    overlay = FindBestOverlay(point, point1, bestPoint1, bestPoint, overlay);
                    overlay = FindBestOverlay(point1, point, bestPoint1, bestPoint, overlay);
                    overlay = FindBestOverlay(point1, point, bestPoint, bestPoint1, overlay);
                    
                    overlay.w = 0;
                    for (int j = 0; j < intersections.Length; j++)
                    {
                        float2 testpoint = intersections[j];
                        float changedDistance = float.MaxValue;
                        for (int k = 0; k < intersections1.Length; k++)
                        {
                            float2 testpoint1 = intersections1[k];
                            float testChangedDistance =
                                math.length(testpoint - mathAdditions.Rotate(testpoint1, overlay.z) - overlay.xy);

                            if (testChangedDistance >= changedDistance) continue;
                            changedDistance = testChangedDistance;
                        }

                        overlay.w += changedDistance;
                    }

                    overlay.w /= intersections.Length;

                    if (overlay.w >= finalOverlay.w) continue;
                    finalOverlay = overlay;
                }
                
                overlays[index] = finalOverlay;
            } 
        }

        private static float4 FindBestOverlay(float2 p1, float2 p2, float2 p3, float2 p4, float4 overlay)
        {
            float3 result = mathAdditions.LayTowlinesOverEachother(p1,p2,p3,p4);

            float2 testVector = mathAdditions.Rotate(p4, result.z) + new float2(result.x, result.y);
            float testdistance = math.length(testVector - p2);

            if (testdistance < overlay.w)
            {
                overlay.w = testdistance;
                overlay = new float4(result.x, result.y, result.z, overlay.w);
            }

            return overlay;
        }
        
        public void ParseOverlay()
        {
            if(!overlayHandle.IsCompleted) return;
            overlayHandle.Complete();

            nativeIntersections.Dispose();
            nativeIntersections1.Dispose();

            float4 finalOverlay = new float4(0,0,0,float.MaxValue);
            for (int i = 0; i < overlays.Length; i++)
            {
                if(overlays[i].w >= finalOverlay.w) continue;
                finalOverlay = overlays[i];
            }
            overlays.Dispose();

            overlay = finalOverlay;
            overlay.xyz += otherLidarPoint.overlay.xyz;
            
            state = LidarPointState.finished;
        }
    }
}