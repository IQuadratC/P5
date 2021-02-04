using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Utility;

namespace Lidar
{
    
    public class LidarPoint
    {
        private List<int>[] distances;
        public Dictionary<int, Vector2> positions;
        public List<List<Vector2>> lines;
        
        public LidarPoint()
        {
            distances = new List<int>[360];
            positions = new Dictionary<int, Vector2>();
            lines = new List<List<Vector2>>();
        }
        
        
        public void LoadCSVData(string name)
        {
            List<List<string>> csvData = Csv.ParseCVSFile(File.ReadAllText(Application.dataPath + "\\" + name));
            csvData.RemoveAt(csvData.Count - 1);
            
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
            UpdateLines();
        }
        public void AddData(int[,] data)
        {
            for (int i = 0; i < data.GetLength(0); i++)
            {
                distances[data[i,0]].Add(data[i,1]);
            }
            
            UpdatePositions();
            UpdateLines();
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
        
        private float GetDistancetoLine(Vector2 line0Pos, Vector2 line1Pos, Vector2 testPos)
        {
            Vector2 b = line0Pos - line1Pos;
            return Vector3.Cross(testPos - line0Pos, b).magnitude / b.magnitude;
        }

        private const float maxDistance = 20;
        private const int minLineLength = 5;
        private void UpdateLines()
        {
            int lineStart = 0;
            int linePos0 = 0;
            int linePos1 = 0;
            bool lineAktive = false;
            List<Vector2> line = new List<Vector2>();
            
            Vector2[] positionsArray = new Vector2[positions.Count];
            positions.Values.CopyTo(positionsArray, 0);
            
            for (int i = 0; i < positionsArray.Length; i++)
            {
                if (!lineAktive && i >= linePos0 + line.Count + 2)
                {
                    if (line.Count > minLineLength)
                    {
                        lines.Add(line);
                    }

                    linePos0 = i - 2;
                    linePos1 = i - 1;
                    
                    line = new List<Vector2>();
                    line.Add(positionsArray[linePos0]);
                    line.Add(positionsArray[linePos1]);

                    lineAktive = true;
                }

                if (lineAktive)
                {
                    float distance = GetDistancetoLine(
                        positionsArray[linePos0],
                        positionsArray[linePos1],
                        positionsArray[i]);
                    
                    lineAktive = distance < maxDistance;

                    if (lineAktive)
                    {
                        line.Add(positionsArray[i]);
                    }
                }
            }
        }
    }
}