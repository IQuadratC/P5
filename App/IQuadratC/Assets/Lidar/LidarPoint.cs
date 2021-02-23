using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Utility;

namespace Lidar
{
    public enum LidarPointState
    {
        addingData = 1,
        waitingForCalculation = 2,
        readyForCalculation = 3,
        processingCalculation = 4,
        finished = 5
    }

    public struct LidarLine
    {
        public List<float2> points;
        public float2 centerPoint;
        public float2 rayCenter;
        public float2 rayVector;
        public float length;
        public float2 outerPoint1;
        public float2 outerPoint2;
    }

    public struct LidarCorner
    {
        public float2 centerPoint;
        public LidarLine line1;
        public LidarLine line2;
        public float angle;
    }
    
    public class LidarPoint
    {
        public LidarPointState State { get; set; }
        public float[] Distances { get; private set; }
        public float2[] Points { get; private set; }
        
        public float2[] WorldPoints { get; private set; }
        public LidarLine[] Lines { get; private set; }
        
        public float2[] Intersections { get; private set; }
        
        public LidarCorner[] Corners { get; private set; }
        
        public bool DropLidarPoint { get; private set; }

        private float3 overlay;
        public float3 Overlay => overlay;

        private LidarSettings lidarSettings;
        public LidarPoint(LidarSettings lidarSettings)
        {
            State = LidarPointState.addingData;
            Distances = new float[360];
            overlay = new float3();
            this.lidarSettings = lidarSettings;
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
            State = LidarPointState.processingCalculation;
            this.otherLidarPoint = otherLidarPoint;
            InitalCalculation();
            State = LidarPointState.finished;
        }
        
        public void Calculate()
        {
            State = LidarPointState.processingCalculation;
            InitalCalculation();
            State = LidarPointState.finished;
        }

        private void InitalCalculation()
        {
            CalculatePoints();
            CalculateLines();
            CalculateIntersections();
            CalculateCorners();
            CheckQuality();
        }
        
        private void CalculatePoints()
        {
            List<float2> positionList = new List<float2>();
            for (int i = 0; i < Distances.Length; i++)
            {
                float2 position = new float2(
                    math.sin(i * math.PI / 180) * Distances[i],
                    math.cos(i * math.PI / 180) * Distances[i]);

                if (!position.Equals(float2.zero))
                {
                    positionList.Add(position);
                }
            }

            Points = positionList.ToArray();
        }
        
        private void CalculateLines()
        {
            int linePos0;
            int linePos1;
            
            List<LidarLine> linelist = new List<LidarLine>();
            for (int i = 0; i < Points.Length; i++)
            {
                linePos0 = i;
                linePos1 = i - 1;
                if (linePos1 < 0)
                {
                    linePos1 = Points.Length - 1;
                }

                LidarLine line = new LidarLine();
                line.points = new List<float2>();
                for (int j = 0; j < Points.Length; j++)
                {
                    int testPos = i + j;
                    if (testPos >= Points.Length)
                    {
                        testPos -= Points.Length;
                    }
                    
                    float distance = mathAdditions.GetDistancetoLine(
                        Points[linePos0],
                        Points[linePos1] - Points[linePos0],
                        Points[testPos]);

                    if (distance < lidarSettings.maxLineDistance)
                    {
                        line.points.Add(Points[testPos]);
                    }
                    else
                    {
                        break;
                    }
                }

                for (int j = 1; j < Points.Length; j++)
                {
                    int testPos = i - j;
                    if (testPos < 0)
                    {
                        testPos += Points.Length;
                    }
                    
                    float distance = mathAdditions.GetDistancetoLine(
                        Points[linePos0],
                        Points[linePos1] - Points[linePos0],
                        Points[testPos]);

                    if (distance < lidarSettings.maxLineDistance)
                    {
                        line.points.Add(Points[testPos]);
                    }
                    else
                    {
                        break;
                    }
                }

                foreach (float2 point in line.points)
                {
                    line.centerPoint += point;
                }
                line.centerPoint /= line.points.Count;
                float2 ray = mathAdditions.FindLinearLeastSquaresFit(line.points);
                
                line.rayCenter = new float2(
                    line.centerPoint.x, ray.x * line.centerPoint.x + ray.y);
                
                line.rayVector = math.normalize(
                    new float2(line.centerPoint.x + 1, ray.x * (line.centerPoint.x + 1) + ray.y) 
                    - line.rayCenter);

                linelist.Add(line);
            }
            
            int SortLidarLines(LidarLine x, LidarLine y)
            {
                int xLenght = x.points.Count;
                int yLenght = y.points.Count;
            
                if (xLenght > yLenght)
                {
                    return -1;
                }
                return xLenght < yLenght ? 1 : 0;
            }
            linelist.Sort(SortLidarLines);
            
            List<LidarLine> cuttedlist = new List<LidarLine>();
            int index = 0;
            for (int k = 0; k < 100000; k++)
            {
                LidarLine line = linelist[index];
                cuttedlist.Add(line);
                linelist.RemoveAt(0);

                foreach (LidarLine testline in linelist)
                {
                    foreach (float2 point in line.points)
                    {
                        for (int j = testline.points.Count - 1; j >= 0; j--)
                        {
                            if (testline.points[j].Equals(point))
                            {
                                testline.points.RemoveAt(j);
                            }
                        }
                    }
                }
                
                linelist.Sort(SortLidarLines);

                for (int i = 0; i < linelist.Count; i++)
                {
                    if (linelist[i].points.Count > lidarSettings.minLinePoints) continue;
                
                    linelist.RemoveRange(i, linelist.Count - i);
                    break;
                }

                if (linelist.Count <= 0)
                {
                    break;
                }
            }
            linelist = cuttedlist;
            
            cuttedlist = new List<LidarLine>();
            for (int i = 0; i < linelist.Count; i++)
            {
                LidarLine line = linelist[i];

                line.length = 0;
                foreach (float2 point in line.points)
                {
                    foreach (float2 point2 in line.points)
                    {
                        if(point.Equals(point2)) continue;

                        float distance = math.distance(point, point2);
                        
                        if(distance <= line.length) continue;

                        line.length = distance;
                        line.outerPoint1 = point;
                        line.outerPoint2 = point2;
                    }
                }

                if (line.length > lidarSettings.minLineLength)
                {
                    cuttedlist.Add(line);
                }
            }
            linelist = cuttedlist;

            Lines = linelist.ToArray();
        }
        
        private void CalculateIntersections()
        {
            List<float2> intersectionList = new List<float2>();
            foreach (LidarLine line1 in Lines)
            {
                foreach (LidarLine line2 in Lines)
                {
                    if(line1.Equals(line2)) continue;

                    float2 intersection = mathAdditions.FindIntersection(
                        line1.rayCenter,
                        line1.rayCenter + line1.rayVector,
                        line2.rayCenter,
                        line2.rayCenter + line2.rayVector);

                    bool isInList = false;
                    foreach (float2 intersectionPoint in intersectionList)
                    {
                        if (math.distance(intersectionPoint, intersection) < lidarSettings.sameIntersectionRadius)
                        {
                            isInList = true;
                        }
                    }

                    if (math.length(intersection) < lidarSettings.interscetionBounds && !isInList)
                    {
                        intersectionList.Add(intersection);
                    }
                }
            }

            Intersections = intersectionList.ToArray();
        }
        
        private void CalculateCorners()
        {
            List<LidarCorner> corners = new List<LidarCorner>();
            for (int i = 0; i < Intersections.Length; i++)
            {
                LidarCorner corner = new LidarCorner();
                corner.centerPoint = Intersections[i];

                List<LidarLine> possibleLines = new List<LidarLine>();
                List<float> possibleDistances = new List<float>();
                
                foreach (LidarLine line in Lines)
                {
                    float distance1 = math.distance(line.outerPoint1, corner.centerPoint);
                    float distance2 = math.distance(line.outerPoint2, corner.centerPoint);
                    float distance = distance1 < distance2 ? distance1 : distance2;
                    
                    if(distance >= lidarSettings.maxCornerDistance) continue;
                    
                    possibleLines.Add(line);
                    possibleDistances.Add(distance);
                }
                
                if(possibleLines.Count < 2) continue;

                if (possibleLines.Count == 2)
                {
                    corner.line1 = possibleLines[0];
                    corner.line2 = possibleLines[1];
                }
                else
                {
                    float bestDistance = float.MaxValue;
                    for (int j = 0; j < possibleLines.Count; j++)
                    {
                       if(possibleDistances[j] >= bestDistance) continue;

                       corner.line1 = possibleLines[j];
                       bestDistance = possibleDistances[j];
                    }
                    int index = possibleLines.IndexOf(corner.line1);
                    possibleLines.RemoveAt(index);
                    possibleDistances.RemoveAt(index);
                    
                    bestDistance = float.MaxValue;
                    for (int j = 0; j < possibleLines.Count; j++)
                    {
                        if(possibleDistances[j] >= bestDistance) continue;

                        corner.line2 = possibleLines[j];
                        bestDistance = possibleDistances[j];
                    }
                }

                corner.angle = mathAdditions.Angle(corner.line1.rayVector, corner.line2.rayVector);
                if(math.abs(corner.angle) < lidarSettings.minCornerAngle || 
                   math.abs(corner.angle) > lidarSettings.maxCornerAngle) continue;
                
                bool isTheSame = false;
                foreach (LidarCorner testcorner in corners)
                {
                    bool a = corner.line1.rayCenter.Equals(testcorner.line1.rayCenter);
                    bool b = corner.line1.rayCenter.Equals(testcorner.line2.rayCenter);
                    bool c = corner.line2.rayCenter.Equals(testcorner.line1.rayCenter);
                    bool d = corner.line2.rayCenter.Equals(testcorner.line2.rayCenter);

                    if ((a || b) && (c || d))
                    {
                        isTheSame = true;
                    }
                }

                if (!isTheSame)
                {
                    corners.Add(corner);
                }
            }
            
            Corners = corners.ToArray();
        }
        
        
        private void CheckQuality()
        {
            DropLidarPoint = Corners.Length < lidarSettings.minCornerAmmount;
        }
        
        private float4[] overlays;
        private int finschedCounter;
        private void CalculateOverlay()
        {
            int length = Corners.Length;
            overlays = new float4[length];
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
            
            float2 corner1 = Intersections[CornerIds[index][0]];
            float2 corner2 = float2.zero;
            float2 corner3 = float2.zero;
            float2 corner4 = float2.zero;
            float bestDelta = float.MaxValue;
            for (int i = 0; i < CornerIds.Length; i++)
            {
                corner2 = Intersections[CornerIds[i][0]];
                if(corner1.Equals(corner2)) continue;

                float distance = math.distance(corner1, corner2);
                for (int j = 0; j < otherLidarPoint.CornerIds.Length; j++)
                {
                    float2 testCorner1 = otherLidarPoint.Intersections[otherLidarPoint.CornerIds[j][0]];
                    for (int k = 0; k < otherLidarPoint.CornerIds.Length; k++)
                    {
                        float2 testCorner2 = otherLidarPoint.Intersections[otherLidarPoint.CornerIds[k][0]];
                        if(testCorner1.Equals(testCorner2)) continue;
                        
                        float testDistance = math.distance(testCorner1, testCorner2);
                        float delta = math.abs(distance - testDistance);
                        
                        if(delta >= bestDelta) continue;

                        bestDelta = delta;
                        corner3 = testCorner1;
                        corner4 = testCorner2;
                    }
                }
            }
            
            float4 overlay = new float4(0,0,0, float.MaxValue);
            void FindBestOverlay(float2 p1, float2 p2, float2 p3, float2 p4)
            {
                float3 result = mathAdditions.LayTowlinesOverEachother(p1,p2,p3,p4);

                float2 testVector = mathAdditions.Rotate(p4, result.z) + new float2(result.x, result.y);
                float testdistance = math.distance(testVector, p2);

                if (testdistance >= overlay.w) return;
                overlay.w = testdistance;
                overlay = new float4(result.x, result.y, result.z, overlay.w);
            }
                
            FindBestOverlay(corner1, corner2, corner3, corner4);
            FindBestOverlay(corner1, corner2, corner4, corner3);
            FindBestOverlay(corner2, corner1, corner3, corner4);
            FindBestOverlay(corner2, corner1, corner4, corner3);
                
            overlay.w = 0;
            for (int j = 0; j < CornerIds.Length; j++)
            {
                for (int k = 0; k < CornerIds[j].Length; k++)
                {
                    float2 testPoint = CornerIds[j][k];

                    float bestDistance = float.MaxValue;
                    for (int l = 0; l < otherLidarPoint.Points.Length; l++)
                    {
                        float distance = math.distance(testPoint, otherLidarPoint.Points[l]);
                            
                        if(distance >= bestDistance) continue;

                        bestDistance = distance;
                    }
                    overlay.w += bestDistance;
                }
            }
            overlays[index] = overlay;
            
            Interlocked.Increment(ref finschedCounter);
        }
        
        public void ParseOverlay()
        {
            if(finschedCounter < 0) return;

            float4 finalOverlay = new float4(0,0,0,float.MaxValue);
            for (int i = 0; i < overlays.Length; i++)
            {
                if(overlays[i].w >= finalOverlay.w) continue;
                finalOverlay = overlays[i];
            }

            overlay = finalOverlay.xyz;
            overlay.xyz += otherLidarPoint.overlay.xyz;
            
            State = LidarPointState.finished;
        }

        private void CalculateWorldPositions()
        {
            WorldPoints = new float2[Points.Length];
            for (int i = 0; i < Points.Length; i++)
            {
                WorldPoints[i] = ApplyOverlay(Points[i], overlay);
            }
        }
        */
        public static float2 ApplyOverlay(float2 pos, float3 overlay)
        {
            return mathAdditions.Rotate(pos, overlay.z) + overlay.xy;
        }
        
    }
}