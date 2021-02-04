using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using Utility;

namespace Lidar
{
    public struct LidarPoint
    {
        public List<int>[] distances;
        public int[] averageDistances;
        public Vector2[] positions;
    }
    
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
            LidarPoint lidarPoint = LoadLidarPoint("Testdata_notMove.csv");
            LidarPoint lidarPoint1 = LoadLidarPoint("Testdata_Move_40cm.csv");
            
            Vector2[] averagePositions = new Vector2[360];
            Vector2 offset = Vector2.zero;
            for (int i = 0; i < 360; i++)
            {
                averagePositions[i] = lidarPoint.positions[i] - lidarPoint1.positions[i];
                offset += averagePositions[i];
            }
            offset /= 360;

            foreach (Vector2 lidarPointPosition in lidarPoint.positions)
            {
                GameObject o = Instantiate(redspherePreFab, lidarPointPosition / 100, Quaternion.identity);
                o.transform.SetParent(redPartent.transform, true);
            }
            foreach (Vector2 lidarPointPosition in lidarPoint1.positions)
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

        public LidarPoint LoadLidarPoint(string name)
        {
            List<List<string>> csvData = Csv.ParseCVSFile(File.ReadAllText(Application.dataPath + "\\" + name));
            
            LidarPoint lidarPoint = new LidarPoint();
            lidarPoint.distances = new List<int>[360];
            for (int i = 0; i < lidarPoint.distances.Length; i++)
            {
                lidarPoint.distances[i] = new List<int>();
            }
            
            foreach (var line in csvData)
            {
                int index = (int) float.Parse(line[0]);
                int data = int.Parse(line[1]);
                if (data != 0)
                {
                    lidarPoint.distances[index].Add(data);
                }
            }

            lidarPoint.averageDistances = new int[360];
            for (int i = 0; i < lidarPoint.distances.Length; i++)
            {
                int sum = 0;
                foreach (var distance in lidarPoint.distances[i])
                {
                    sum += distance;
                }
                if (sum != 0)
                {
                    lidarPoint.averageDistances[i] = sum / lidarPoint.distances[i].Count;
                }
            }

            lidarPoint.positions = new Vector2[360];
            for (int i = 0; i < lidarPoint.averageDistances.Length; i++)
            {
                int distance = lidarPoint.averageDistances[i];
                float x = (float)(Math.Sin(i * Math.PI / 180) * distance);
                float y = (float)(Math.Cos(i * Math.PI / 180) * distance);
                lidarPoint.positions[i] = new Vector2(x,y);
            }

            return lidarPoint;
        }
    }
}