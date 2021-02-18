using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;
using Utility;
using Utility.Events;
using Utility.Variables;

namespace Lidar
{
    public class LidarController : MonoBehaviour
    {
        [SerializeField] private Vec2Variable position;
        
        private List<LidarPoint> lidarPointsProcessing;
        private List<LidarPoint> lidarPoints;
        [SerializeField] private Int2ListVariable points;

        [SerializeField] private float maxDistance = 2f;
        [SerializeField] private int minLineLength = 10;
        [SerializeField] private int bounds = 500;
        
        [SerializeField] private GameEvent newPoints;

        private void Awake()
        {
            lidarPoints = new List<LidarPoint>();
            lidarPointsProcessing = new List<LidarPoint>();
        }

        private void Start()
        {
            points.Value.Clear();
            if (simulateData)
            {
                SimulateData();
            }
        }
        
        [SerializeField] private bool simulateData;
        private string[] csvFiles =
        {
            "Testdata_gedreht_0.csv",
            "Testdata_gedreht_1.csv",
            "Testdata_gedreht_2.csv"
        };
        private void SimulateData()
        {
            foreach (string csvFile in csvFiles)
            {
                string[][] csvData = Csv.ParseCVSFile(
                    File.ReadAllText(Application.dataPath + "\\" + csvFile));
                csvData[csvData.Length - 1] = new []{"0.0","0"};
                
                List<int>[] distances = new List<int>[360];
                for (int i = 0; i < distances.Length; i++)
                {
                    distances[i] = new List<int>();
                }
                
                foreach (var line in csvData)
                {
                    int angle = (int) float.Parse(line[0]);
                    int distance = int.Parse(line[1]);
                    
                    if(distance == 0) continue;
                    
                    distances[angle].Add(distance);
                }
                
                List<int2> data = new List<int2>();
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
                    if(sum == 0) continue;
                    
                    data.Add(new int2(i, sum));
                }
                
                AddLidarData(data.ToArray());
                PushLidarData();
            }
        }

        private bool isAdding;
        private void AddLidarData(int2[] data)
        {
            LidarPoint lidarPoint;
            if (!isAdding)
            {
                isAdding = true;
                lidarPoint = new LidarPoint(maxDistance, minLineLength, bounds);
                lidarPointsProcessing.Add(lidarPoint);
            }
            else
            {
                lidarPoint = lidarPointsProcessing[lidarPointsProcessing.Count - 1];
            }
            lidarPoint.AddData(data);
        }

        private void PushLidarData()
        {
            isAdding = false;
            lidarPointsProcessing[lidarPointsProcessing.Count - 1].State = LidarPointState.waitingForCalculation;
        }

        private void Update()
        {
            for (int i = lidarPointsProcessing.Count - 1; i >= 0; i--)
            {
                LidarPoint lidarPoint = lidarPointsProcessing[i];
                switch (lidarPoint.State)
                {
                    case LidarPointState.addingData:
                        
                        break;
                    
                    case LidarPointState.waitingForCalculation:
                        if (lidarPoint == lidarPointsProcessing[0])
                        {
                            lidarPoint.State = LidarPointState.readyForCalculation;
                        }
                        break;
                    
                    case LidarPointState.readyForCalculation:
                        if (lidarPoints.Count > 0)
                        {
                            lidarPoint.Calculate(lidarPoints[0]);
                        }
                        else
                        {
                            lidarPoint.Calculate();
                        }
                        
                        break;
                    
                    case LidarPointState.performingCalculation:
                        lidarPoint.ParseOverlay();
                        break;
                    
                    case LidarPointState.finished:
                        lidarPointsProcessing.Remove(lidarPoint);
                        lidarPoints.Add(lidarPoint);
                        
                        position.Value = new float2(lidarPoint.Overlay.xy);

                        foreach (float2 pointPosition in lidarPoint.Positions)
                        {
                            if(pointPosition.Equals(float2.zero)) continue;
                            points.Value.Add((int2)LidarPoint.ApplyOverlay(pointPosition, lidarPoint.Overlay));
                            
                            newPoints.Raise();
                        }
                        
                        if (showPoints)
                        {
                            ShowPoint(lidarPoint);
                        }
                        break;
                }
            }
        }

        [SerializeField] private bool showPoints;
        private int counter;
        [SerializeField] private GameObject[] pointPreFabs;
        [SerializeField] private GameObject linePreFab;
        [SerializeField] private GameObject pointParent;
        private void ShowPoint(LidarPoint lidarPoint)
        {
            GameObject point = new GameObject("Point " + counter);
            point.transform.position = new Vector3(lidarPoint.Overlay.x, lidarPoint.Overlay.y, 0);
            point.transform.eulerAngles = new Vector3(0,0,lidarPoint.Overlay.z);
            point.transform.SetParent(pointParent.transform);

            foreach (float2 lidarPointPosition in lidarPoint.Positions)
            {
                GameObject o = Instantiate(pointPreFabs[counter],
                    new Vector3(lidarPointPosition.x, lidarPointPosition.y, 0), Quaternion.identity);
                o.transform.SetParent(point.transform, false);
            }

            foreach (float2[] line in lidarPoint.Lines)
            {
                foreach (float2 float2 in line)
                {
                    GameObject o = Instantiate(linePreFab, new Vector3(float2.x, float2.y, 0), Quaternion.identity);
                    o.transform.SetParent(point.transform, false);
                }
            }
            
            foreach (Vector2 intersectionPoint in lidarPoint.Intersections)
            {
                GameObject o = Instantiate(linePreFab, intersectionPoint, Quaternion.identity);
                o.transform.SetParent(point.transform, false);
            }

            counter++;
        }
        
        [SerializeField] private GameEvent sendEvent;
        [SerializeField] private StringVariable sendString;
        public void ReqestData()
        {
            sendString.Value = "lidar sumdata 50";
            sendEvent.Raise();
        }
        
        [SerializeField] private StringVariable reciveString;
        public void ReciveData()
        {
            string str = reciveString.Value;
            string[] strs = str.Split(' ');
            
            if (!strs[0].Equals("lidarmap")) return;
            
            if (strs[1].Equals("data"))
            {
                int2[] data = new int2[strs.Length -2];
                for (int i = 2; i < strs.Length; i++)
                {
                    string[] args = strs[i].Split(',');
                    if(args.Length < 2) continue;
                    
                    data[i - 2].x = int.Parse(args[0]);
                    data[i - 2].y = int.Parse(args[1]);
                }
                AddLidarData(data);
            }
            else if (strs[1].Equals("end"))
            {
                PushLidarData();
            }
        }
    }
}