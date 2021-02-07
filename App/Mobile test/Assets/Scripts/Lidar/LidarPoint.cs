using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using Utility;

namespace Lidar
{
    public class LidarPoint
    {
        public List<float2> intersections;
        public float2[] positions = new float2[360];
        
        public List<float>[] distances = new List<float>[360];
        public List<List<float2>> lines = new List<List<float2>>();
        
        public void LoadCSV(string name)
        {
            string[][] csvData = Csv.ParseCVSFile(File.ReadAllText(Application.dataPath + "\\" + name));

            for (int i = 0; i < distances.Length; i++)
            {
                distances[i] = new List<float>();
            }
            
            foreach (string[] line in csvData)
            {
                if(line.Length < 2) continue;
                    
                int index = (int) float.Parse(line[0]);
                float data = int.Parse(line[1]);
                if (data == 0.0f) continue;
                distances[index].Add(data / 10);
            }
            
            Update();
        }
        public void AddData(int[,] data)
        {
            for (int i = 0; i < data.GetLength(0); i++)
            {
                distances[data[i,0]].Add((float)data[i,1] / 10);
            }
            
            Update();
        }

        private void Update()
        {
            UpdatePositions();
            UpdateLines();
            UpdateIntersections();
        }

        
        private void UpdatePositions()
        {
            for (int i = 0; i < distances.Length; i++)
            {
                float sum = distances[i].Sum();
                if (sum > 0)
                {
                    sum /= distances[i].Count;
                }
                else { continue; }
                
                positions[i] = new float2(
                    math.sin(i * math.PI / 180) * sum,
                    math.cos(i * math.PI / 180) * sum);
            }
        }

        private const float maxDistance = 0.5f;
        private const int minLineLength = 5;
        private void UpdateLines()
        {
            List<List<float2>> lineList = new List<List<float2>>();
            for (int i = 0; i < positions.Length; i++)
            {
                if(positions[i].Equals(float2.zero)) continue;

                int linePos = i - 1;
                
                if (linePos < 0)
                {
                    linePos = positions.Length - 1;
                }

                List<float2> line = new List<float2>();
                for (int j = 0; j < positions.Length; j++)
                {
                    int testPos = i + j;
                    if (testPos >= positions.Length)
                    {
                        testPos -= positions.Length;
                    }
                    if (positions[testPos].Equals(float2.zero)) continue;
                    
                    
                    float distance = mathAdditions.GetDistancetoLine(
                        positions[i],
                        positions[linePos],
                        positions[testPos]);

                    if (distance < maxDistance)
                    {
                        line.Add(positions[testPos]);
                    }
                    else
                    {
                        break;
                    }
                }

                for (int j = 1; j < positions.Length; j++)
                {
                    int testPos = i - j;
                    if (testPos < 0)
                    {
                        testPos += positions.Length;
                    }
                    if (positions[testPos].Equals(float2.zero)) continue;
                    
                    float distance = mathAdditions.GetDistancetoLine(
                        positions[i],
                        positions[linePos],
                        positions[testPos]);

                    if (distance < maxDistance)
                    {
                        line.Add(positions[testPos]);
                    }
                    else
                    {
                        break;
                    }
                }
                
                lineList.Add(line);
            }
            
            lineList.Sort(SortListByLenght);
            
            int index = 0;
            for (int k = 0; k < 100000; k++)
            {
                List<float2> line = lineList[index];
                lines.Add(line);
                lineList.RemoveAt(0);

                foreach (List<float2> testline in lineList)
                {
                    foreach (float2 point in line)
                    {
                        for (int j = testline.Count - 1; j >= 0; j--)
                        {
                            if (testline[j].Equals(point))
                            {
                                testline.RemoveAt(j);
                            }
                        }
                    }
                }
                
                lineList.Sort(SortListByLenght);

                for (int i = 0; i < lineList.Count; i++)
                {
                    if (lineList[i].Count > minLineLength) continue;
                
                    lineList.RemoveRange(i, lineList.Count - i);
                    break;
                }

                if (lineList.Count <= 0)
                {
                    break;
                }
            }
            
            for (int i = 0; i < lines.Count; i++)
            {
                float4 v = mathAdditions.FindLinearLeastSquaresFit(lines[i]);
                lines[i].Add(new float2(v.x, v.y));
                lines[i].Add(new float2(v.z, v.w));
            }
        }
        private static int SortListByLenght(List<float2> x, List<float2> y)
        {
            int xLenght = x.Count;
            int yLenght = y.Count;
            
            if (xLenght > yLenght)
            {
                return -1;
            }
            return xLenght < yLenght ? 1 : 0;
        }

        private const int bounds = 500;
        private void UpdateIntersections()
        {
            intersections = new List<float2>();
            foreach (List<float2> line in lines)
            {
                float4 finalLine = new float4(line[line.Count-2], line[line.Count-1]);
                foreach (List<float2> line1 in lines)
                {
                    if(line == line1) continue;
                        
                    float4 testFinalLine = new float4(line1[line1.Count-2], line1[line1.Count-1]);
                    float2 intersection = mathAdditions.FindIntersection(
                        new float2(finalLine.x, finalLine.y), 
                        new float2(finalLine.x + finalLine.z, finalLine.y + finalLine.w), 
                        new float2(testFinalLine.x, testFinalLine.y), 
                        new float2(testFinalLine.x + testFinalLine.z, testFinalLine.y + testFinalLine.w));
                    
                    bool isNotinList = true;
                    foreach (float2 intersectionPoint in intersections)
                    {
                        if (math.length(intersectionPoint - intersection) < 1.0f)
                        {
                            isNotinList = false;
                        }
                    }

                    if (math.length(intersection) < bounds && isNotinList)
                    {
                        intersections.Add(intersection);
                    }
                }
            }
        }
        
        /*
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
            UpdateIntersections();
        }
        public void AddData(int[,] data)
        {
            for (int i = 0; i < data.GetLength(0); i++)
            {
                distances[data[i,0]].Add((float)data[i,1] / 10);
            }
            
            UpdatePositions();
            UpdateLines();
            UpdateIntersections();
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

            linelist.Sort(SortListByLenght);
            
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
        private void UpdateIntersections()
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
        */
    }
}