﻿using System;
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
                GameObject o = Instantiate(redspherePreFab, lidarPointPosition / 100, Quaternion.identity);
                o.transform.SetParent(redPartent.transform, true);
            }
            foreach (Vector2 lidarPointPosition in lidarPoint1.positions.Values)
            {
                GameObject o = Instantiate(bluespherePreFab, lidarPointPosition / 100, Quaternion.identity);
                o.transform.SetParent(bluePartent.transform, true);
            }
            foreach (Vector2 lidarPointPosition in lidarPoint2.positions.Values)
            {
                GameObject o = Instantiate(greenspherePreFab, lidarPointPosition / 100, Quaternion.identity);
                o.transform.SetParent(greenPartent.transform, true);
            }
            
            foreach (List<Vector2> line in lidarPoint.lines)
            {
                foreach (Vector2 point in line)
                {
                    GameObject o = Instantiate(lightbluespherePreFab, point / 100, Quaternion.identity);
                    o.transform.SetParent(lightbluePartent.transform, true);
                }
            }
            
            
        }
    }
}