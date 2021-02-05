using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        
        private const float maxDistance = 5;
        private const int minLineLength = 5;
        private void UpdateLines()
        {
            int linePos0 = 0;
            int linePos1 = 0;
            
            Vector2[] positionsArray = new Vector2[positions.Count];
            positions.Values.CopyTo(positionsArray, 0);
            List<List<Vector2>> linelist = new List<List<Vector2>>();

            for (int i = 0; i < positionsArray.Length; i++)
            {
                linePos0 = i;
                linePos1 = i - 1;
                if (linePos1 < 0)
                {
                    linePos1 = positionsArray.Length - 1;
                }

                List<Vector2> line = new List<Vector2>();
                for (int j = 0; j < positionsArray.Length; j++)
                {
                    int testPos = i + j;
                    if (testPos >= positionsArray.Length)
                    {
                        testPos -= positionsArray.Length;
                    }
                    
                    float distance = GetDistancetoLine(
                        positionsArray[linePos0],
                        positionsArray[linePos1],
                        positionsArray[testPos]);

                    if (distance < maxDistance)
                    {
                        line.Add(positionsArray[testPos]);
                    }
                    else
                    {
                        break;
                    }
                }

                for (int j = 1; j < positionsArray.Length; j++)
                {
                    int testPos = i - j;
                    if (testPos < 0)
                    {
                        testPos += positionsArray.Length;
                    }
                    
                    float distance = GetDistancetoLine(
                        positionsArray[linePos0],
                        positionsArray[linePos1],
                        positionsArray[testPos]);

                    if (distance < maxDistance)
                    {
                        line.Add(positionsArray[testPos]);
                    }
                    else
                    {
                        break;
                    }
                }
                
                linelist.Add(line);
            }

            int safty = 0;
            int index = 0;
            while (true)
            {
                List<Vector2> line = linelist[index];
                lines.Add(line);
                linelist.RemoveAt(0);

                foreach (List<Vector2> testline in linelist)
                {
                    foreach (Vector2 point in line)
                    {
                        for (int j = testline.Count - 1; j >= 0; j--)
                        {
                            if (testline[j] == point)
                            {
                                testline.RemoveAt(j);
                            }
                        }
                    }
                }
                
                linelist.Sort(SortListByLenght);

                for (int i = 0; i < linelist.Count; i++)
                {
                    if (linelist[i].Count > minLineLength) continue;
                
                    linelist.RemoveRange(i, linelist.Count - i);
                    break;
                }

                if (linelist.Count <= 0)
                {
                    break;
                }

                if (safty > 100000)
                {
                    break;
                }
                safty++;
            }
            
            int k = 0;
        }

        private static int SortListByLenght<T>(List<T> x, List<T> y)
        {
            int xLenght = x.Count;
            int yLenght = y.Count;
            
            if (xLenght > yLenght)
            {
                return -1;
            }
            return xLenght < yLenght ? 1 : 0;
        }
    }
}