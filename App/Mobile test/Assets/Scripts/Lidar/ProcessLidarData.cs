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
        public GameObject redPartent;
        public GameObject bluePartent;
        public GameObject greenPartent;
        
        private void Start()
        {
            LidarPoint lidarPoint = new LidarPoint();
            lidarPoint.LoadCSVData("Testdata_notMove.csv");
            
            LidarPoint lidarPoint1 = new LidarPoint();
            lidarPoint1.LoadCSVData("Testdata_Move_40cm.csv");

            Vector2[] averagePositions = new Vector2[360];
            Vector2 offset = Vector2.zero;
            for (int i = 0; i < 360; i++)
            {
                averagePositions[i] = lidarPoint.positions[i] - lidarPoint1.positions[i];
                offset += averagePositions[i];
            }
            offset /= 360;

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
            foreach (Vector2 lidarPointPosition in averagePositions)
            {
                GameObject o = Instantiate(greenspherePreFab, lidarPointPosition / 100, Quaternion.identity);
                o.transform.SetParent(greenPartent.transform, true);
            }
            Instantiate(whitespherePreFab, offset / 100, Quaternion.identity);

            bluePartent.transform.position += new Vector3(offset.x / 100, offset.y / 100, 0);
        }
    }
}