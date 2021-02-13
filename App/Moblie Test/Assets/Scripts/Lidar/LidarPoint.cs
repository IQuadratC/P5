using System;
using System.Collections.Generic;
using System.IO;
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
        waitingForCalculation = 2,
        readyForCalculation = 3,
        performingCalculation = 4,
        finished = 5
    }
    
    public class LidarPoint
    {
        public LidarPointState State { get; set; }
        public float4 Overlay;
        
        private List<float>[] distances;
        public Dictionary<int, Vector2> positions;
        public List<List<Vector2>> lines;
        public List<Vector4> finallines;
        public List<Vector2> intersectionPoints;
        
        public LidarPoint()
        {
            State = LidarPointState.addingData;
            
            distances = new List<float>[360];
            for (int i = 0; i < distances.Length; i++)
            {
                distances[i] = new List<float>();
            }
            
            positions = new Dictionary<int, Vector2>();
            lines = new List<List<Vector2>>();
            finallines = new List<Vector4>();
            intersectionPoints = new List<Vector2>();
        }
        
        public void LoadCSVData(string name)
        {
            string[][] csvData = Csv.ParseCVSFile(
                File.ReadAllText(Application.dataPath + "\\" + name));
            csvData[csvData.Length - 1] = new []{"0.0","0"};
            
            for (int i = 0; i < distances.Length; i++)
            {
                distances[i] = new List<float>();
            }
            
            foreach (var line in csvData)
            {
                int index = (int) float.Parse(line[0]);
                int data = int.Parse(line[1]);
                if (data != 0)
                {
                    distances[index].Add((float)data / 10);
                }
            }
        }
        
        public void AddData(int2[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                distances[data[i].x].Add((float)data[i].y / 10);
            }
        }

        public void Calculate()
        {
            UpdatePositions();
            UpdateLines();
            UpdateIntersectionPoints();
            State = LidarPointState.finished;
        }
        
        public void Calculate(LidarPoint lidarPoint)
        {
            UpdatePositions();
            UpdateLines();
            UpdateIntersectionPoints();
            CalculateOverlay(lidarPoint);
            State = LidarPointState.finished;
        }

        public void ParseOverlay()
        {
            
        }

        private void UpdatePositions()
        {
            for (int i = 0; i < distances.Length; i++)
            {
                float sum = 0;
                foreach (var distance in distances[i])
                {
                    sum += distance;
                }
                if (sum > 0)
                {
                    sum /= distances[i].Count;
                }
                else
                {
                    continue;
                }

                float x = (float)(Math.Sin(i * Math.PI / 180) * sum);
                float y = (float)(Math.Cos(i * Math.PI / 180) * sum);
                positions[i] = new Vector2(x,y);
            }
        }
        

        private const float maxDistance = 2f;
        private const int minLineLength = 10;
        private void UpdateLines()
        {
            int linePos0 = 0;
            int linePos1 = 0;
            
            Vector2[] positionsArray = new Vector2[positions.Count];
            positions.Values.CopyTo(positionsArray, 0);
            List<List<Vector2>> linelist = new List<List<Vector2>>();

            for (int i = 0; i < positionsArray.Length; i++)
            {
                linePos0 = i;
                linePos1 = i - 1;
                if (linePos1 < 0)
                {
                    linePos1 = positionsArray.Length - 1;
                }

                List<Vector2> line = new List<Vector2>();
                for (int j = 0; j < positionsArray.Length; j++)
                {
                    int testPos = i + j;
                    if (testPos >= positionsArray.Length)
                    {
                        testPos -= positionsArray.Length;
                    }
                    
                    float distance = GetDistancetoLine(
                        positionsArray[linePos0],
                        positionsArray[linePos1],
                        positionsArray[testPos]);

                    if (distance < maxDistance)
                    {
                        line.Add(positionsArray[testPos]);
                    }
                    else
                    {
                        break;
                    }
                }

                for (int j = 1; j < positionsArray.Length; j++)
                {
                    int testPos = i - j;
                    if (testPos < 0)
                    {
                        testPos += positionsArray.Length;
                    }
                    
                    float distance = GetDistancetoLine(
                        positionsArray[linePos0],
                        positionsArray[linePos1],
                        positionsArray[testPos]);

                    if (distance < maxDistance)
                    {
                        line.Add(positionsArray[testPos]);
                    }
                    else
                    {
                        break;
                    }
                }
                
                linelist.Add(line);
            }
            
            int index = 0;
            for (int k = 0; k < 100000; k++)
            {
                List<Vector2> line = linelist[index];
                lines.Add(line);
                linelist.RemoveAt(0);

                foreach (List<Vector2> testline in linelist)
                {
                    foreach (Vector2 point in line)
                    {
                        for (int j = testline.Count - 1; j >= 0; j--)
                        {
                            if (testline[j] == point)
                            {
                                testline.RemoveAt(j);
                            }
                        }
                    }
                }
                
                linelist.Sort(SortListByLenght);

                for (int i = 0; i < linelist.Count; i++)
                {
                    if (linelist[i].Count > minLineLength) continue;
                
                    linelist.RemoveRange(i, linelist.Count - i);
                    break;
                }

                if (linelist.Count <= 0)
                {
                    break;
                }
            }

            
            for (int i = 0; i < lines.Count; i++)
            {
                finallines.Add(FindLinearLeastSquaresFit(lines[i]));
            }
            
            int t = 0;
        }
        private float GetDistancetoLine(Vector2 line0Pos, Vector2 line1Pos, Vector2 testPos)
        {
            Vector2 b = line0Pos - line1Pos;
            return Vector3.Cross(testPos - line0Pos, b).magnitude / b.magnitude;
        }
        private static int SortListByLenght<T>(List<T> x, List<T> y)
        {
            int xLenght = x.Count;
            int yLenght = y.Count;
            
            if (xLenght > yLenght)
            {
                return -1;
            }
            return xLenght < yLenght ? 1 : 0;
        }
        private static Vector4 FindLinearLeastSquaresFit(List<Vector2> points)
        {
            double S1 = points.Count;
            double Sx = 0;
            double Sy = 0;
            double Sxx = 0;
            double Sxy = 0;
            foreach (Vector2 pt in points)
            {
                Sx += pt.x;
                Sy += pt.y;
                Sxx += pt.x * pt.x;
                Sxy += pt.x * pt.y;
            }
            
            double m = (Sxy * S1 - Sx * Sy) / (Sxx * S1 - Sx * Sx);
            double b = (Sxy * Sx - Sy * Sxx) / (Sx * Sx - S1 * Sxx);
            
            return new Vector4(0, (float)b,1,(float)m);
        }

        
        private const int bounds = 500;
        private void UpdateIntersectionPoints()
        {
            foreach (Vector4 finalline in finallines)
            {
                foreach (Vector4 testFinalline in finallines)
                {
                    Vector2 intersection = FindIntersection(
                        new Vector2(finalline.x, finalline.y), 
                        new Vector2(finalline.x + finalline.z, finalline.y + finalline.w), 
                        new Vector2(testFinalline.x, testFinalline.y), 
                        new Vector2(testFinalline.x + testFinalline.z, testFinalline.y + testFinalline.w));

                    if (!intersectionPoints.Contains(intersection) && intersection.magnitude < bounds)
                    {
                        intersectionPoints.Add(intersection);
                    }
                }
            }
        }
        private Vector2 FindIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            float dx12 = p2.x - p1.x;
            float dy12 = p2.y - p1.y;
            float dx34 = p4.x - p3.x;
            float dy34 = p4.y - p3.y;
            
            float denominator = (dy12 * dx34 - dx12 * dy34);
            float t1 = ((p1.x - p3.x) * dy34 + (p3.y - p1.y) * dx34) / denominator;
            
            if (float.IsInfinity(t1))
            {
                return  new Vector2(float.NaN, float.NaN);
            }
            
            return new Vector2(p1.x + dx12 * t1, p1.y + dy12 * t1);
        }

        private void CalculateOverlay(LidarPoint otherPoint)
        {
            Overlap overlap = OverlapIntersectionPointsWithRotation(otherPoint, this);
            Overlay.xy = new float2(overlap.pos) + otherPoint.Overlay.xy;
            Overlay.z = overlap.rotation + otherPoint.Overlay.z;
        }
        
        struct Overlap
        {
            public float accuracy;
            public Vector2 pos;
            public float rotation;
        }
        
        private Overlap OverlapIntersectionPointsWithRotation(LidarPoint lidarPoint, LidarPoint lidarPoint1)
        {
            List<Overlap> overlaps = new List<Overlap>();
            foreach (Vector2 point in lidarPoint.intersectionPoints)
            {
                foreach (Vector2 point1 in lidarPoint.intersectionPoints)
                {
                    if(point == point1) continue;
                    
                    float distance = (point - point1).magnitude;
                    float bestDistance = float.MaxValue;
                    Vector2 bestPoint = Vector2.zero;
                    Vector2 bestPoint1 = Vector2.zero;

                    foreach (Vector2 point2 in lidarPoint1.intersectionPoints)
                    {
                        foreach (Vector2 point3 in lidarPoint1.intersectionPoints)
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

                    foreach (Vector2 testpoint in lidarPoint.intersectionPoints)
                    {
                        
                        float changedDistance = float.MaxValue;
                        foreach (Vector2 testpoint1 in lidarPoint1.intersectionPoints)
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
        
        private static int SortListByAccuracy(Overlap x, Overlap y)
        {
            if (x.accuracy < y.accuracy)
            {
                return -1;
            }
            return x.accuracy > y.accuracy ? 1 : 0;
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
        
        /*
        public LidarPointState State { get; set; }
        public float[] Distances { get; private set; }
        public float2[] Positions { get; private set; }
        public float2[][] Lines { get; private set; }
        public float2[] Intersections { get; private set; }
        public float4 Overlay => overlay;
        
        public LidarPoint(float maxDistance, int minLineLength, int bounds)
        {
            State = LidarPointState.addingData;
            Distances = new float[360];
            this.maxDistance = maxDistance;
            this.minLineLength = minLineLength;
            this.bounds = bounds;
        }
        public void AddData(int2[] data)
        {
            foreach (int2 int2 in data)
            {
                Distances[int2.x] = int2.y / 10;
            }
        }

        private LidarPoint otherLidarPoint;
        public void Calculate(LidarPoint otherLidarPoint)
        {
            State = LidarPointState.performingCalculation;
            this.otherLidarPoint = otherLidarPoint;
            CalculatePositions();
            CalculateLines();
            CalculateIntersections();
            CalculateOverlay();
        }
        
        public void Calculate()
        {
            State = LidarPointState.performingCalculation;
            CalculatePositions();
            CalculateLines();
            CalculateIntersections();
            State = LidarPointState.finished;
        }

        private void CalculatePositions()
        {
            Positions = new float2[360];
            for (int i = 0; i < Distances.Length; i++)
            {
                Positions[i] = new float2(
                    math.sin(i * math.PI / 180) * Distances[i],
                    math.cos(i * math.PI / 180) * Distances[i]);
            }
        }

        private readonly float maxDistance;
        private readonly int minLineLength;
        private void CalculateLines()
        {
            List<List<float2>> lineList = new List<List<float2>>();
            for (int i = 0; i < Positions.Length; i++)
            {
                if(Positions[i].Equals(float2.zero)) continue;

                int linePos = i - 1;

                int k = 1;
                while (linePos < 0 || Positions[linePos].Equals(float2.zero))
                {
                    linePos = Positions.Length - k;
                    k++;
                }

                List<float2> line = new List<float2>();
                for (int j = 0; j < Positions.Length; j++)
                {
                    int testPos = i + j;
                    if (testPos >= Positions.Length)
                    {
                        testPos -= Positions.Length;
                    }
                    if (Positions[testPos].Equals(float2.zero)) continue;
                    
                    
                    float distance = mathAdditions.GetDistancetoLine(
                        Positions[i],
                        Positions[linePos],
                        Positions[testPos]);

                    if (distance < maxDistance)
                    {
                        line.Add(Positions[testPos]);
                    }
                    else
                    {
                        break;
                    }
                }

                for (int j = 1; j < Positions.Length; j++)
                {
                    int testPos = i - j;
                    if (testPos < 0)
                    {
                        testPos += Positions.Length;
                    }
                    if (Positions[testPos].Equals(float2.zero)) continue;
                    
                    float distance = mathAdditions.GetDistancetoLine(
                        Positions[i],
                        Positions[linePos],
                        Positions[testPos]);

                    if (distance < maxDistance)
                    {
                        line.Add(Positions[testPos]);
                    }
                    else
                    {
                        break;
                    }
                }
                
                lineList.Add(line);
            }
            
            int SortListByLenght(List<float2> x, List<float2> y)
            {
                int xLenght = x.Count;
                int yLenght = y.Count;
            
                if (xLenght > yLenght)
                {
                    return -1;
                }
                return xLenght < yLenght ? 1 : 0;
            }
            lineList.Sort(SortListByLenght);
            
            List<List<float2>> lineList2 = new List<List<float2>>();
            for (int k = 0; k < 100000; k++)
            {
                List<float2> line = lineList[0];
                lineList2.Add(line);
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
            
            Lines = new float2[lineList2.Count][];
            for (int i = 0; i < lineList2.Count; i++)
            {
                float4 v = mathAdditions.FindLinearLeastSquaresFit(lineList2[i]);
                lineList2[i].Add(new float2(v.x, v.y));
                lineList2[i].Add(new float2(v.z, v.w));

                Lines[i] = lineList2[i].ToArray();
            }
        }

        private readonly int bounds;
        private void CalculateIntersections()
        {
            List<float2> intersectionList = new List<float2>();
            foreach (float2[] line in Lines)
            {
                float4 finalLine = new float4(line[line.Length-2], line[line.Length-1]);
                foreach (float2[] line1 in Lines)
                {
                    if(line == line1) continue;
                        
                    float4 testFinalLine = new float4(line1[line1.Length-2], line1[line1.Length-1]);
                    float2 intersection = mathAdditions.FindIntersection(
                        new float2(finalLine.x, finalLine.y), 
                        new float2(finalLine.x + finalLine.z, finalLine.y + finalLine.w), 
                        new float2(testFinalLine.x, testFinalLine.y), 
                        new float2(testFinalLine.x + testFinalLine.z, testFinalLine.y + testFinalLine.w));
                    
                    bool isNotInList = true;
                    foreach (float2 intersectionPoint in intersectionList)
                    {
                        if (math.length(intersectionPoint - intersection) < 1.0f)
                        {
                            isNotInList = false;
                        }
                    }

                    if (math.length(intersection) < bounds && isNotInList)
                    {
                        intersectionList.Add(intersection);
                    }
                }
            }

            Intersections = intersectionList.ToArray();
        }
        private bool jobActive = true;
        private NativeArray<float2> nativeIntersections;

        private NativeArray<float2> nativeIntersections1;
        private NativeArray<float4> overlays;
        private JobHandle jobHandle;
        private void CalculateOverlay()
        {
            int length = otherLidarPoint.Intersections.Length;
            int lenght1 = Intersections.Length;
            
            nativeIntersections = new NativeArray<float2>(length, Allocator.Persistent);
            nativeIntersections1 = new NativeArray<float2>(lenght1, Allocator.Persistent);

            for (int i = 0; i < length; i++) { nativeIntersections[i] = otherLidarPoint.Intersections[i]; }
            for (int i = 0; i < lenght1; i++) { nativeIntersections1[i] = Intersections[i]; }
            
            overlays = new NativeArray<float4>(length, Allocator.Persistent);

            ProcessOverlayJob processOverlayJob = new ProcessOverlayJob();
            processOverlayJob.intersections = nativeIntersections;
            processOverlayJob.intersections1 = nativeIntersections1;
            processOverlayJob.overlays = overlays;

            if (jobActive)
            {
                jobHandle = processOverlayJob.Schedule(length, 1);
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
                    void FindBestOverlay(float2 p1, float2 p2, float2 p3, float2 p4)
                    {
                        float3 result = mathAdditions.LayTowlinesOverEachother(p1,p2,p3,p4);

                        float2 testVector = mathAdditions.Rotate(p4, result.z) + new float2(result.x, result.y);
                        float testdistance = math.length(testVector - p2);

                        if (testdistance >= overlay.w) return;
                        overlay.w = testdistance;
                        overlay = new float4(result.x, result.y, result.z, overlay.w);
                    }
                    
                    FindBestOverlay(point, point1, bestPoint, bestPoint1);
                    FindBestOverlay(point, point1, bestPoint1, bestPoint);
                    FindBestOverlay(point1, point, bestPoint1, bestPoint);
                    FindBestOverlay(point1, point, bestPoint, bestPoint1);
                    
                    overlay.w = 0;
                    for (int j = 0; j < intersections.Length; j++)
                    {
                        float2 testpoint = intersections[j];
                        float changedDistance = float.MaxValue;
                        for (int k = 0; k < intersections1.Length; k++)
                        {
                            float2 testpoint1 = intersections1[k];
                            float testChangedDistance =
                                math.length(LidarPoint.ApplyOverlay(testpoint, overlay) - testpoint1);

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

        private float4 overlay;
        public void ParseOverlay()
        {
            if(!jobHandle.IsCompleted) return;
            jobHandle.Complete();

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
            
            State = LidarPointState.finished;
        }*/

        public static float2 ApplyOverlay(float2 pos, float4 overlay)
        {
            return mathAdditions.Rotate(pos, overlay.z) + overlay.xy;
        }
    }
}