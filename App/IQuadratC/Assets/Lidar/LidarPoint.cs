using System.Collections.Generic;
using System.Threading;
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
        public float[] Distances { get; private set; }
        public float2[] Positions { get; private set; }
        public float2[][] Lines { get; private set; }
        public float2[] Intersections { get; private set; }
        
        private float4 overlay;
        public float4 Overlay => overlay;
        
        public List<float4> finallines;

        public LidarPoint(float maxDistance, int minLineLength, int bounds)
        {
            State = LidarPointState.addingData;
            Distances = new float[360];
            this.maxDistance = maxDistance;
            this.minLineLength = minLineLength;
            this.bounds = bounds;
            
            finallines = new List<float4>();
        }
        
        public void AddData(int2[] data)
        {
            foreach (int2 int2 in data)
            {
                Distances[int2.x] = (float)int2.y / 10;
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
        
        private float maxDistance = 2f;
        private int minLineLength = 10;
        private void CalculateLines()
        {
            int linePos0;
            int linePos1;
            
            float2[] positionsArray = new float2[Positions.Length];
            Positions.CopyTo(positionsArray, 0);
            List<List<float2>> linelist = new List<List<float2>>();

            for (int i = 0; i < positionsArray.Length; i++)
            {
                linePos0 = i;
                linePos1 = i - 1;
                if (linePos1 < 0)
                {
                    linePos1 = positionsArray.Length - 1;
                }

                List<float2> line = new List<float2>();
                for (int j = 0; j < positionsArray.Length; j++)
                {
                    int testPos = i + j;
                    if (testPos >= positionsArray.Length)
                    {
                        testPos -= positionsArray.Length;
                    }
                    
                    float distance = mathAdditions.GetDistancetoLine(
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
                    
                    float distance = mathAdditions.GetDistancetoLine(
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
            
            List<List<float2>> linelist2 = new List<List<float2>>();
            int index = 0;
            for (int k = 0; k < 100000; k++)
            {
                List<float2> line = linelist[index];
                linelist2.Add(line);
                linelist.RemoveAt(0);

                foreach (List<float2> testline in linelist)
                {
                    foreach (Vector2 point in line)
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
                
                int SortListByLenght<T>(List<T> x, List<T> y)
                {
                    int xLenght = x.Count;
                    int yLenght = y.Count;
            
                    if (xLenght > yLenght)
                    {
                        return -1;
                    }
                    return xLenght < yLenght ? 1 : 0;
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

            Lines = new float2[linelist2.Count][];
            for (int i = 0; i < linelist2.Count; i++)
            {
                finallines.Add(mathAdditions.FindLinearLeastSquaresFit(linelist2[i]));
                Lines[i] = linelist2[i].ToArray();
            }
        }
        
        private readonly int bounds;
        private void CalculateIntersections()
        {
            List<float2> intersectionList = new List<float2>();
            foreach (float4 finalLine in finallines)
            {
                foreach (float4 testFinalLine in finallines)
                {
                    if(finalLine.Equals(testFinalLine)) continue;
                    
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

        private float4[] overlaysTest;
        private int finschedCounter;
        private void CalculateOverlay()
        {
            int length = otherLidarPoint.Intersections.Length;
            overlaysTest = new float4[length];
            finschedCounter = -length;

            for (int i = 0; i < length; i++)
            {
                Threader.RunAsync(ProcessOverlay, i);
            }
        }
        private void ProcessOverlay(object indexValue)
        {
            int index = (int) indexValue;
            float4 finalOverlay = new float4(0,0,0,float.MaxValue);
                float2 point = otherLidarPoint.Intersections[index];
                for (int i = 0; i < otherLidarPoint.Intersections.Length; i++)
                {
                    float2 point1 = otherLidarPoint.Intersections[i];
                    if (point.Equals(point1)) continue;

                    float distance = math.length(point - point1);
                    float bestDistance = float.MaxValue;
                    float2 bestPoint = float2.zero;
                    float2 bestPoint1 = float2.zero;

                    for (int j = 0; j < Intersections.Length; j++)
                    {
                        float2 point2 = Intersections[j];
                        for (int k = 0; k < Intersections.Length; k++)
                        {
                            float2 point3 = Intersections[k];
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
                    for (int j = 0; j < otherLidarPoint.Intersections.Length; j++)
                    {
                        float2 testpoint = otherLidarPoint.Intersections[j];
                        float changedDistance = float.MaxValue;
                        for (int k = 0; k < Intersections.Length; k++)
                        {
                            float2 testpoint1 = Intersections[k];
                            float testChangedDistance =
                                math.length(testpoint - mathAdditions.Rotate(testpoint1, overlay.z) - overlay.xy);

                            if (testChangedDistance >= changedDistance) continue;
                            changedDistance = testChangedDistance;
                        }

                        overlay.w += changedDistance;
                    }

                    overlay.w /= otherLidarPoint.Intersections.Length;

                    if (overlay.w >= finalOverlay.w) continue;
                    finalOverlay = overlay;
                }
                
                overlaysTest[index] = finalOverlay;
                
                Interlocked.Increment(ref finschedCounter);
        }

        public void ParseOverlay()
        {
            if(finschedCounter < 0) return;

            float4 finalOverlay = new float4(0,0,0,float.MaxValue);
            for (int i = 0; i < overlaysTest.Length; i++)
            {
                if(overlaysTest[i].w >= finalOverlay.w) continue;
                finalOverlay = overlaysTest[i];
            }

            overlay = finalOverlay;
            overlay.xyz += otherLidarPoint.overlay.xyz;
            
            State = LidarPointState.finished;
        }

        public static float2 ApplyOverlay(float2 pos, float4 overlay)
        {
            return mathAdditions.Rotate(pos, overlay.z) + overlay.xy;
        }
    }
}