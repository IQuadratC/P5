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
        private List<float>[] distances;
        public Dictionary<int, Vector2> positions;
        public List<List<Vector2>> lines;
        public List<Vector4> finallines;
        public List<Vector2> intersectionPoints;
        
        public LidarPoint()
        {
            distances = new List<float>[360];
            positions = new Dictionary<int, Vector2>();
            lines = new List<List<Vector2>>();
            finallines = new List<Vector4>();
            intersectionPoints = new List<Vector2>();
        }
        
        public void LoadCSVData(string name)
        {
            List<List<string>> csvData = Csv.ParseCVSFile(File.ReadAllText(Application.dataPath + "\\" + name));
            csvData.RemoveAt(csvData.Count - 1);
            
            for (int i = 0; i < distances.Length; i++)
            {
                distances[i] = new List<float>();
            }
            
            foreach (var line in csvData)
            {
                int index = (int) float.Parse(line[0]);
                int data = int.Parse(line[1]);
                if (data != 0)
                {
                    distances[index].Add((float)data / 10);
                }
            }
            
            UpdatePositions();
            UpdateLines();
            UpdateIntersectionPoints();
        }
        public void AddData(int[,] data)
        {
            for (int i = 0; i < data.GetLength(0); i++)
            {
                distances[data[i,0]].Add((float)data[i,1] / 10);
            }
            
            UpdatePositions();
            UpdateLines();
            UpdateIntersectionPoints();
        }

        private void UpdatePositions()
        {
            for (int i = 0; i < distances.Length; i++)
            {
                float sum = 0;
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
        

        private const float maxDistance = 0.5f;
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
            
            int index = 0;
            for (int k = 0; k < 100000; k++)
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
            }

            
            for (int i = 0; i < lines.Count; i++)
            {
                finallines.Add(FindLinearLeastSquaresFit(lines[i]));
            }
            
            int t = 0;
        }
        private float GetDistancetoLine(Vector2 line0Pos, Vector2 line1Pos, Vector2 testPos)
        {
            Vector2 b = line0Pos - line1Pos;
            return Vector3.Cross(testPos - line0Pos, b).magnitude / b.magnitude;
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
        private static Vector4 FindLinearLeastSquaresFit(List<Vector2> points)
        {
            double S1 = points.Count;
            double Sx = 0;
            double Sy = 0;
            double Sxx = 0;
            double Sxy = 0;
            foreach (Vector2 pt in points)
            {
                Sx += pt.x;
                Sy += pt.y;
                Sxx += pt.x * pt.x;
                Sxy += pt.x * pt.y;
            }
            
            double m = (Sxy * S1 - Sx * Sy) / (Sxx * S1 - Sx * Sx);
            double b = (Sxy * Sx - Sy * Sxx) / (Sx * Sx - S1 * Sxx);
            
            return new Vector4(0, (float)b,1,(float)m);
        }

        
        private const int bounds = 500;
        private void UpdateIntersectionPoints()
        {
            foreach (Vector4 finalline in finallines)
            {
                foreach (Vector4 testFinalline in finallines)
                {
                    Vector2 intersection = FindIntersection(
                        new Vector2(finalline.x, finalline.y), 
                        new Vector2(finalline.x + finalline.z, finalline.y + finalline.w), 
                        new Vector2(testFinalline.x, testFinalline.y), 
                        new Vector2(testFinalline.x + testFinalline.z, testFinalline.y + testFinalline.w));

                    if (!intersectionPoints.Contains(intersection) && intersection.magnitude < bounds)
                    {
                        intersectionPoints.Add(intersection);
                    }
                }
            }
        }
        private Vector2 FindIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            float dx12 = p2.x - p1.x;
            float dy12 = p2.y - p1.y;
            float dx34 = p4.x - p3.x;
            float dy34 = p4.y - p3.y;
            
            float denominator = (dy12 * dx34 - dx12 * dy34);
            float t1 = ((p1.x - p3.x) * dy34 + (p3.y - p1.y) * dx34) / denominator;
            
            if (float.IsInfinity(t1))
            {
                return  new Vector2(float.NaN, float.NaN);
            }
            
            return new Vector2(p1.x + dx12 * t1, p1.y + dy12 * t1);
        }
        
    }
}