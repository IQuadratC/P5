using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Utility;

namespace Lidar
{
    public class LidarPoint
    {
        public Dictionary<int, Vector2> positions;
        
        public LidarPoint()
        {
            positions = new Dictionary<int, Vector2>();
            distances = new List<int>[360];
        }
        private List<int>[] distances;
        
        public void LoadCSVData(string name)
        {
            List<List<string>> csvData = Csv.ParseCVSFile(File.ReadAllText(Application.dataPath + "\\" + name));
            
            for (int i = 0; i < distances.Length; i++)
            {
                distances[i] = new List<int>();
            }
            
            foreach (var line in csvData)
            {
                int index = (int) float.Parse(line[0]);
                int data = int.Parse(line[1]);
                if (data != 0)
                {
                    distances[index].Add(data);
                }
            }
            
            UpdatePositions();
        }
        public void AddData(int[,] data)
        {
            for (int i = 0; i < data.GetLength(0); i++)
            {
                distances[data[i,0]].Add(data[i,1]);
            }
            
            UpdatePositions();
        }

        private void UpdatePositions()
        {
            for (int i = 0; i < distances.Length; i++)
            {
                int sum = 0;
                foreach (var distance in distances[i])
                {
                    sum += distance;
                }
                if (sum > 0)
                {
                    sum /= distances[i].Count;
                }
                else
                {
                    continue;
                }

                float x = (float)(Math.Sin(i * Math.PI / 180) * sum);
                float y = (float)(Math.Cos(i * Math.PI / 180) * sum);
                positions[i] = new Vector2(x,y);
            }
        }
    }
}