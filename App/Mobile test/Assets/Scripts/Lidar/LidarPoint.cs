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
        
        private readonly List<float>[] distances = new List<float>[360];
        public readonly float2[] positions = new float2[360];
        public readonly List<List<float2>> lines = new List<List<float2>>();
        public List<float2> intersections = new List<float2>();
        public float4 overlay;

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
                if (data == 0.0f) continue;
                distances[index].Add(data / 10);
            }
        }
        public void AddData(int[,] data)
        {
            for (int i = 0; i < data.GetLength(0); i++)
            {
                distances[data[i,0]].Add((float)data[i,1] / 10);
            }
        }

        public void Update(LidarPoint otherLidarPoint)
        {
            UpdatePositions();
            UpdateLines();
            UpdateIntersections();
            UpdateOverlay(otherLidarPoint);
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

        private NativeArray<float2> nativeIntersections;
        private NativeArray<float2> nativeIntersections1;
        private NativeArray<float4> overlays;
        private JobHandle overlayHandle;
        private void UpdateOverlay(LidarPoint otherLidarPoint)
        {
            int count = intersections.Count;
            int count1 = otherLidarPoint.intersections.Count;
            
            nativeIntersections = new NativeArray<float2>(count, Allocator.TempJob);
            nativeIntersections1 = new NativeArray<float2>(count1, Allocator.TempJob);

            for (int i = 0; i < count; i++) { nativeIntersections[i] = intersections[i]; }
            for (int i = 0; i < count1; i++) { nativeIntersections1[i] = otherLidarPoint.intersections[i]; }
            
            overlays = new NativeArray<float4>(count, Allocator.TempJob);

            ProcessOverlayJob processOverlayJob = new ProcessOverlayJob();
            processOverlayJob.intersections = nativeIntersections;
            processOverlayJob.intersections1 = nativeIntersections1;
            processOverlayJob.overlays = overlays;

            overlayHandle = processOverlayJob.Schedule(count, 1);
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
        
        public void ParseOverlay()
        {
            if(!overlayHandle.IsCompleted) return;

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
            
            state = LidarPointState.finished;
        }
    }
}