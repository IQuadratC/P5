// Dieses Scipt enthält alle Funktionen zum umrechnen der LIDAR Daten von V1.
// Die Funktionen werden von anderen Klassen aufgerufen.
// Die Klasse Lidarpoint enthält alle Daten zu einer Lidarmessung.

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;
using Utility;

namespace Lidar
{
    // Die enum Flags, die angeben in welchem Schritt die Funktionenen sind. 
    // Sie werden benötigt, da diese KLasse in einem backround thread läuft.
    public enum LidarPointState
    {
        addingData = 1,
        waitingForCalculation = 2,
        readyForCalculation = 3,
        processingCalculation = 4,
        dropped = 5,
        finished = 6
    }

    // Struct für eine gefundene Linie in dem Datensatz.
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

    // Struct für eine gefundene Ecke in dem Datensatz.
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
        // Eine Liste der LIDAR Daten. Ein Wert pro Winkelgrad.
        // Sind in Polar Koordinaten angegeben.
        public float[] Distances { get; } 
        // Die Umgrechneten Punkte von Polar- zu Kartenkoordinaten.
        public float2[] Points { get; private set; } 
        
        // Absolute Position (Mit Verschiebung)
        public float2[] WorldPoints { get; private set; }

        // Alle gefundenen Linien.
        public LidarLine[] Lines { get; private set; }
        
        // Alle gefundenen Schnittpunkte von zwei Geraden.
        public float2[] Intersections { get; private set; }
        
        // Alle gefundenen Ecken.
        public LidarCorner[] Corners { get; private set; }
        
        // Ist true wenn nicht genug Ecken gefunden worden.
        public bool DropLidarPoint { get; private set; }

        // Die Verschiebung des Roboters. Z = Drehung in Radien.
        private float3 overlay;
        public float3 Overlay => overlay;

        // Einige Max und Min Werte für den Algorithmus.
        private LidarSettings lidarSettings;
        public LidarPoint(LidarSettings lidarSettings)
        {
            State = LidarPointState.addingData;
            Distances = new float[360];
            overlay = new float3();
            this.lidarSettings = lidarSettings;
        }
        
        // Zum Hinzufügen von LIDAR Daten.
        public void AddData(int2[] data)
        {
            foreach (int2 int2 in data)
            {
                Distances[int2.x] = (float)int2.y / 10;
            }
        }
        
        // Wird gecalled wenn alle Daten entfangen sind.
        private LidarPoint otherLidarPoint;
        public void StartCalculate(LidarPoint otherLidarPoint)
        {
            this.otherLidarPoint = otherLidarPoint;
            StartCalculate();
        }
        
        // Wir bein ersten Datensatz gecalled.
        public void StartCalculate()
        {
            State = LidarPointState.processingCalculation;
            Threader.RunAsync(Calculate);
        }
        
        // Hauptfunktion Sie rechnet die Daten um.
        private void Calculate()
        {
            CalculatePoints();
            CalculateLines();
            CalculateIntersections();
            CalculateCorners();
            CheckQuality();
            
            if (DropLidarPoint)
            {
                State = LidarPointState.dropped;
                return;
            }

            if (otherLidarPoint != null)
            {
                CalculateOverlay();
            }
            
            CalculateWorldPositions();
            
            State = LidarPointState.finished;
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

                corner.angle = math.abs(mathAdditions.Angle(corner.line1.rayVector, corner.line2.rayVector));
                if(corner.angle < lidarSettings.minCornerAngle || 
                   corner.angle > lidarSettings.maxCornerAngle) continue;
                
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
        
        private void CalculateOverlay()
        {
            float4[] overlays = new float4[Corners.Length];
            for (int i = 0; i < Corners.Length; i++)
            {
                LidarCorner corner1 = Corners[i];
                
                List<LidarCorner> possibleCorners = new List<LidarCorner>();
                foreach (LidarCorner testCorner in otherLidarPoint.Corners)
                {
                    if(math.abs(corner1.angle - testCorner.angle) > 
                       lidarSettings.minOverlayCornerAngleDiffernce) continue;
                    possibleCorners.Add(testCorner);
                }
                
                if (possibleCorners.Count == 0)
                {
                    overlays[i] = new float4(0,0,0, float.MaxValue);
                    continue;
                }

                LidarCorner corner2 = new LidarCorner();
                LidarCorner corner3 = new LidarCorner();
                LidarCorner corner4 = new LidarCorner();
                
                float bestDelta = float.MaxValue;
                foreach (LidarCorner testcorner2 in Corners)
                {
                    if(corner1.Equals(testcorner2)) continue;
                    
                    List<LidarCorner> possibleCorners1 = new List<LidarCorner>();
                    foreach (LidarCorner testCorner in otherLidarPoint.Corners)
                    {
                        if(math.abs(testcorner2.angle - testCorner.angle) > 
                           lidarSettings.minOverlayCornerAngleDiffernce) continue;
                        possibleCorners1.Add(testCorner);
                    }
                    
                    float distance1 = math.distance(corner1.centerPoint, testcorner2.centerPoint);
                    
                    foreach (LidarCorner testcorner3 in possibleCorners)
                    {
                        foreach (LidarCorner testcorner4 in possibleCorners1)
                        {
                            if(testcorner3.Equals(testcorner4)) continue;
                            
                            float distance2 = math.distance(testcorner3.centerPoint, testcorner4.centerPoint);
                            float delta = math.abs(distance1 - distance2);
                            
                            if(delta >= bestDelta) continue;
                            bestDelta = delta;

                            corner2 = testcorner2;
                            corner3 = testcorner3;
                            corner4 = testcorner4;
                        }
                    }
                }

                if (corner2.centerPoint.Equals(float2.zero) ||
                    corner3.centerPoint.Equals(float2.zero) ||
                    corner4.centerPoint.Equals(float2.zero))
                {
                    overlays[i] = new float4(0,0,0, float.MaxValue);
                    continue;
                }
                
                float3 overlay1 = mathAdditions.LayTowlinesOverEachother(
                    corner1.centerPoint, corner2.centerPoint, 
                    corner3.centerPoint, corner4.centerPoint);
                
                float3 overlay2 = mathAdditions.LayTowlinesOverEachother(
                    corner1.centerPoint, corner2.centerPoint, 
                    corner4.centerPoint, corner3.centerPoint);

                float2 v21 = corner2.line1.rayVector * lidarSettings.overlayRayVectorMultiplyer;
                float2 v22 = corner2.line2.rayVector * lidarSettings.overlayRayVectorMultiplyer;
                float2 v41 = corner4.line1.rayVector * lidarSettings.overlayRayVectorMultiplyer;
                float2 v42 = corner4.line2.rayVector * lidarSettings.overlayRayVectorMultiplyer;

                float2 testPos1 = ApplyOverlay(corner2.centerPoint, overlay1) + v21;
                float2 testPos2 = ApplyOverlay(corner2.centerPoint, overlay2) + v22;
                float2 refPos1 = corner4.centerPoint + v41;
                float2 refPos2 = corner4.centerPoint + v42;

                float testDistance1 = math.distance(testPos1, refPos1);
                float testDistance2 = math.distance(testPos2, refPos2);

                float3 newOverlay = testDistance1 < testDistance2 ? overlay1 : overlay2;
                
                float accuacy = 0;
                foreach (LidarCorner corner in Corners)
                {
                    float bestDistance = float.MaxValue;
                    foreach (float2 point in corner.line1.points)
                    {
                        float2 movetPoint = ApplyOverlay(point, newOverlay);
                        
                        foreach (LidarCorner otherCorner in otherLidarPoint.Corners)
                        {
                            foreach (float2 otherPoint in otherCorner.line1.points)
                            {
                                float distance = math.distance(movetPoint, otherPoint);
                                
                                if(distance >= bestDistance) continue;
                                bestDistance = distance;
                            }
                            
                            foreach (float2 otherPoint in otherCorner.line2.points)
                            {
                                float distance = math.distance(movetPoint, otherPoint);
                                
                                if(distance >= bestDistance) continue;
                                bestDistance = distance;
                            }
                            
                        }
                    }
                    accuacy += bestDistance;
                    
                    bestDistance = float.MaxValue;
                    foreach (float2 point in corner.line2.points)
                    {
                        float2 movetPoint = ApplyOverlay(point, newOverlay);
                        
                        foreach (LidarCorner otherCorner in otherLidarPoint.Corners)
                        {
                            foreach (float2 otherPoint in otherCorner.line1.points)
                            {
                                float distance = math.distance(movetPoint, otherPoint);
                                
                                if(distance >= bestDistance) continue;
                                bestDistance = distance;
                            }
                            
                            foreach (float2 otherPoint in otherCorner.line2.points)
                            {
                                float distance = math.distance(movetPoint, otherPoint);
                                
                                if(distance >= bestDistance) continue;
                                bestDistance = distance;
                            }
                            
                        }
                    }
                    accuacy += bestDistance;
                }
                
                overlays[i] = new float4(newOverlay, accuacy);
            }
            
            overlay = new float3(0,0,0);
            float bestAccuacy = float.MaxValue;
            for (int i = 0; i < overlays.Length; i++)
            {
                if(overlays[i].w >= bestAccuacy) continue;
                bestAccuacy = overlays[i].w;
                overlay = overlays[i].xyz;
            }
            
            overlay += otherLidarPoint.overlay;
        }
        
        private void CalculateWorldPositions()
        {
            WorldPoints = new float2[Points.Length];
            for (int i = 0; i < Points.Length; i++)
            {
                WorldPoints[i] = ApplyOverlay(Points[i], overlay);
            }
        }

        private static float2 ApplyOverlay(float2 pos, float3 overlay)
        {
            return mathAdditions.Rotate(pos, overlay.z) + overlay.xy;
        }
    }
}