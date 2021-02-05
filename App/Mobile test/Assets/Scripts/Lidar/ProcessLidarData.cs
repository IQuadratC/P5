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
            lidarPoint.LoadCSVData("Testdata_notMove.csv");
            
            LidarPoint lidarPoint1 = new LidarPoint();
            lidarPoint1.LoadCSVData("Testdata_Move_20cm.csv");
            
            LidarPoint lidarPoint2 = new LidarPoint();
            lidarPoint2.LoadCSVData("Testdata_Move_40cm.csv");
            
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
            
            Vector2 pos1 = OverlapIntersectionPoints(lidarPoint, lidarPoint1);
            bluePartent.transform.position = pos1;
            
            Vector2 pos2 = OverlapIntersectionPoints(lidarPoint, lidarPoint2);
            greenPartent.transform.position = pos2;
            
            /*
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

            foreach (var line in lidarPoint.finallines)
            {
                DrawLine(line, Color.red, 10000);
            }
            
            foreach (var line in lidarPoint1.finallines)
            {
                DrawLine(line + new Vector4(0,0,0,0), Color.blue, 10000);
            }
            
            foreach (var line in lidarPoint2.finallines)
            {
                DrawLine(line + new Vector4(0,0,0,0), Color.green, 10000);
            }

            foreach (Vector2 intersectionPoint in lidarPoint.intersectionPoints)
            {
                GameObject o = Instantiate(whitespherePreFab, intersectionPoint, Quaternion.identity);
                o.transform.SetParent(lightbluePartent.transform, true);
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
            
            Debug.Log(overlaps[0].pos);
            Debug.Log(overlaps[0].accuracy);
            Debug.Log(overlaps.Count);
            
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
    }
}