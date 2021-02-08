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
            LidarPoint lidarPoint = new LidarPoint(false);
            lidarPoint.LoadCsv("Testdata_notMove.csv");
            
            LidarPoint lidarPoint1 = new LidarPoint(false);
            lidarPoint1.LoadCsv("Testdata_Move_20cm.csv");
            
            LidarPoint lidarPoint2 = new LidarPoint(false);
            lidarPoint2.LoadCsv("Testdata_Move_40cm.csv");
            
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
            
            foreach (List<float2> line in lidarPoint.lines)
            {
                GameObject lineObject = Instantiate(linePreFab, redPartent.transform);
                foreach (Vector2 point in line)
                {
                    GameObject o = Instantiate(lightbluespherePreFab, point, Quaternion.identity);
                    o.transform.SetParent(lineObject.transform, true);
                }
            }
            
            foreach (List<float2> line in lidarPoint1.lines)
            {
                GameObject lineObject = Instantiate(linePreFab, bluePartent.transform);
                foreach (Vector2 point in line)
                {
                    GameObject o = Instantiate(lightbluespherePreFab, point, Quaternion.identity);
                    o.transform.SetParent(lineObject.transform, true);
                }
            }
            
            foreach (List<float2> line in lidarPoint2.lines)
            {
                GameObject lineObject = Instantiate(linePreFab, greenPartent.transform);
                foreach (Vector2 point in line)
                {
                    GameObject o = Instantiate(lightbluespherePreFab, point, Quaternion.identity);
                    o.transform.SetParent(lineObject.transform, true);
                }
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
            
            foreach (Vector2 intersectionPoint in lidarPoint2.intersections)
            {
                GameObject o = Instantiate(whitespherePreFab, intersectionPoint, Quaternion.identity);
                o.transform.SetParent(greenPartent.transform, true);
            }
            
            //lidarPoint1.overlay = OverlayTwoLidarPoints(lidarPoint, lidarPoint1);
            bluePartent.transform.position = new Vector3(lidarPoint1.overlay.x, lidarPoint1.overlay.y, 0);
            bluePartent.transform.eulerAngles = new Vector3(0,0,lidarPoint1.overlay.z);
            
            //lidarPoint2.overlay = OverlayTwoLidarPoints(lidarPoint, lidarPoint2);
            greenPartent.transform.position = new Vector3(lidarPoint2.overlay.x, lidarPoint2.overlay.y, 0);
            greenPartent.transform.eulerAngles = new Vector3(0,0,lidarPoint2.overlay.z);
            
            foreach (List<float2> line in lidarPoint.lines)
            {
                DrawLine(new float4(line[line.Count -2], line[line.Count -1]), Color.red, 10000);
            }
            
            foreach (List<float2> line in lidarPoint1.lines)
            {
                DrawLine(new float4(
                        mathAdditions.Rotate(line[line.Count - 2], lidarPoint1.overlay.z) + lidarPoint1.overlay.xy, 
                        mathAdditions.Rotate(line[line.Count - 1], lidarPoint1.overlay.z)), 
                Color.blue, 10000);
            }
            
            foreach (List<float2> line in lidarPoint2.lines)
            {
                DrawLine(new float4(
                        mathAdditions.Rotate(line[line.Count - 2], lidarPoint2.overlay.z) + lidarPoint2.overlay.xy, 
                        mathAdditions.Rotate(line[line.Count - 1], lidarPoint2.overlay.z)), 
                    Color.green, 10000);
            }
        }
        
        private void DrawLine(float4 line, Color color, int bounds)
        {
            Vector3 pos = new Vector3(line.x - line.z * bounds, line.y - line.w * bounds, 0);
            Vector3 dir = new Vector3(line.z, line.w) * bounds * 2;
            Debug.DrawRay(pos, dir, color, 10000000000, false);
        }
        
        
    }
}