using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
                DrawLine(line + new Vector4(-20,0,0,0), Color.blue, 10000);
            }
            
            foreach (var line in lidarPoint2.finallines)
            {
                DrawLine(line + new Vector4(-40,0,0,0), Color.green, 10000);
            }

            foreach (Vector2 intersectionPoint in lidarPoint.intersectionPoints)
            {
                GameObject o = Instantiate(whitespherePreFab, intersectionPoint, Quaternion.identity);
                o.transform.SetParent(lightbluePartent.transform, true);
            }
        }
        
        private void DrawLine(Vector4 line, Color color, int bounds)
        {
            Vector3 pos = new Vector3(line.x - line.z * bounds, line.y - line.w * bounds, 0);
            Vector3 dir = new Vector3(line.z, line.w) * bounds * 2;
            Debug.DrawRay(pos, dir, color, 10000000000, false);
        }

        private void OverlapIntersectionPoints()
        {
            
        }
    }
}