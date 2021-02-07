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
    }
}