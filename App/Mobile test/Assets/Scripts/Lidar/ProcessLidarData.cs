using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using Utility;

namespace Lidar
{
    struct Overlap
    {
        public float accuracy;
        public Vector2 pos;
        public float rotation;
    }
    
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
            lidarPoint.LoadCSVData("Testdata_gedreht_0.csv");
            
            LidarPoint lidarPoint1 = new LidarPoint();
            lidarPoint1.LoadCSVData("Testdata_gedreht_1.csv");
            
            LidarPoint lidarPoint2 = new LidarPoint();
            lidarPoint2.LoadCSVData("Testdata_gedreht_2.csv");
            
            foreach (Vector2 lidarPointPosition in lidarPoint.positions.Values)
            {
                GameObject o = Instantiate(redspherePreFab, lidarPointPosition, Quaternion.identity);
                o.transform.SetParent(redPartent.transform, true);
            }
            foreach (Vector2 lidarPointPosition in lidarPoint1.positions.Values)
            {
                GameObject o = Instantiate(bluespherePreFab, lidarPointPosition, Quaternion.identity);
                o.transform.SetParent(bluePartent.transform, true);
            }
            foreach (Vector2 lidarPointPosition in lidarPoint2.positions.Values)
            {
                GameObject o = Instantiate(greenspherePreFab, lidarPointPosition, Quaternion.identity);
                o.transform.SetParent(greenPartent.transform, true);
            }
            
            foreach (List<Vector2> line in lidarPoint.lines)
            {
                GameObject lineObject = Instantiate(linePreFab, redPartent.transform);
                foreach (Vector2 point in line)
                {
                    GameObject o = Instantiate(lightbluespherePreFab, point, Quaternion.identity);
                    o.transform.SetParent(lineObject.transform, true);
                }
            }
            
            foreach (List<Vector2> line in lidarPoint1.lines)
            {
                GameObject lineObject = Instantiate(linePreFab, bluePartent.transform);
                foreach (Vector2 point in line)
                {
                    GameObject o = Instantiate(lightbluespherePreFab, point, Quaternion.identity);
                    o.transform.SetParent(lineObject.transform, true);
                }
            }
            
            foreach (List<Vector2> line in lidarPoint2.lines)
            {
                GameObject lineObject = Instantiate(linePreFab, greenPartent.transform);
                foreach (Vector2 point in line)
                {
                    GameObject o = Instantiate(lightbluespherePreFab, point, Quaternion.identity);
                    o.transform.SetParent(lineObject.transform, true);
                }
            }
            
            foreach (Vector2 intersectionPoint in lidarPoint.intersectionPoints)
            {
                GameObject o = Instantiate(whitespherePreFab, intersectionPoint, Quaternion.identity);
                o.transform.SetParent(redPartent.transform, true);
            }
            
            foreach (Vector2 intersectionPoint in lidarPoint1.intersectionPoints)
            {
                GameObject o = Instantiate(whitespherePreFab, intersectionPoint, Quaternion.identity);
                o.transform.SetParent(bluePartent.transform, true);
            }
            
            Overlap overlap1 = OverlapIntersectionPointsWithRotation(lidarPoint, lidarPoint1);
            bluePartent.transform.position = overlap1.pos;
            bluePartent.transform.eulerAngles = new Vector3(0,0,overlap1.rotation);
            
            
            /*
            foreach (var line in lidarPoint2.finallines)
            {
                DrawLine(RotateVector(line), Color.green, 10000);
            }
            */
        }
        
        private void DrawLine(Vector4 line, Color color, int bounds)
        {
            Vector3 pos = new Vector3(line.x - line.z * bounds, line.y - line.w * bounds, 0);
            Vector3 dir = new Vector3(line.z, line.w) * bounds * 2;
            Debug.DrawRay(pos, dir, color, 10000000000, false);
        }

        private List<Overlap> overlaps;
        private Vector2 OverlapIntersectionPoints(LidarPoint lidarPoint, LidarPoint lidarPoint1)
        {
            overlaps = new List<Overlap>();
            foreach (Vector2 intersectionPoint in lidarPoint.intersectionPoints)
            {
                foreach (Vector2 testIntersectionPoint in lidarPoint1.intersectionPoints)
                {
                    Overlap overlap = new Overlap();
                    overlap.pos = intersectionPoint - testIntersectionPoint;
                    overlap.accuracy = 0;

                    foreach (Vector2 testpoint in lidarPoint.intersectionPoints)
                    {
                        float distance = float.MaxValue;
                        foreach (Vector2 testpoint1 in lidarPoint1.intersectionPoints)
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
                            float testChangedDistance = (testpoint - RotateVector(testpoint1, overlap.rotation) + overlap.pos).magnitude;
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

    }
}