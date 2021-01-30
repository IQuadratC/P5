using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using Utility;

namespace Lidar
{
    struct LidarPoint
    {
        public List<int>[] distances;
        public int[] averageDistances;
        public Vector2[] positions;
    }
    
    public class ProcessLidarData : MonoBehaviour
    {
        public GameObject spherePreFab;
        private void Start()
        {
            List<List<string>> csvData = Csv.ParseCVSFile(File.ReadAllText(Application.dataPath + "\\lidartestdata.csv"));
            
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

                Instantiate(spherePreFab, lidarPoint.positions[i] / 100, Quaternion.identity);
            }
        }
    }
}