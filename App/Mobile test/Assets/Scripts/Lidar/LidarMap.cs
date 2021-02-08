using System;
using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;
using Utility;
using Utility.Variables;

namespace Lidar
{
    public class LidarMap : MonoBehaviour
    {
        public List<LidarPoint> bigLidarPoints;
        public List<LidarPoint> smallLidarPoints;
        private List<LidarPoint> lidarPointsProcessing;
        public float3 currentPosition;
        public BoolVariable bigLidarPointActive;
        private bool wasBigLidarPointActive;
        public bool simulateData;

        private void Awake()
        {
            bigLidarPoints = new List<LidarPoint>();
            smallLidarPoints = new List<LidarPoint>();
            lidarPointsProcessing = new List<LidarPoint>();
        }

        public void AddLidarData(List<int2> data)
        {
            if (bigLidarPointActive.Value)
            {
                LidarPoint lidarPoint;
                if (!wasBigLidarPointActive)
                {
                    lidarPoint = new LidarPoint(true);

                    lidarPointsProcessing.Add(lidarPoint);
                    wasBigLidarPointActive = true;
                }
                else
                {
                    lidarPoint = lidarPointsProcessing[lidarPointsProcessing.Count - 1];
                }
                lidarPoint.AddData(data);
            }
            else
            {
                LidarPoint lidarPoint = new LidarPoint(false);

                lidarPoint.AddData(data);
                lidarPoint.state = LidarPointState.waitingForUpdate;
                
                lidarPointsProcessing.Add(lidarPoint);
            }
        }

        private void Update()
        {
            SimulateData();
            
            for (int i = lidarPointsProcessing.Count - 1; i >= 0; i--)
            {
                LidarPoint lidarPoint = lidarPointsProcessing[i];
                switch (lidarPoint.state)
                {
                    case LidarPointState.addingData:
                        if (lidarPoint.bigLidarPoint && wasBigLidarPointActive && !bigLidarPointActive.Value)
                        {
                            lidarPoint.state = LidarPointState.waitingForUpdate;
                        }
                        break;
                    
                    case LidarPointState.waitingForUpdate:

                        if (bigLidarPoints.Count > 0)
                        {
                            lidarPoint.Update(bigLidarPoints[bigLidarPoints.Count -1]);
                        }
                        else
                        {
                            if (lidarPoint.bigLidarPoint)
                            {
                                lidarPoint.Update();
                            }
                        }
                        
                        break;
                    
                    case LidarPointState.performingUpdate:
                        lidarPoint.ParseOverlay();
                        break;
                    
                    case LidarPointState.finished:
                        lidarPointsProcessing.Remove(lidarPoint);
                        if (lidarPoint.bigLidarPoint)
                        {
                            bigLidarPoints.Add(lidarPoint);
                            ShowPoint(lidarPoint);
                        }
                        else
                        {
                            smallLidarPoints.Add(lidarPoint);
                            currentPosition = lidarPoint.overlay.xyz;
                        }
                        break;
                }
            }

            if (!bigLidarPointActive.Value)
            {
                wasBigLidarPointActive = false;
            }
            
        }

        public GameObject[] pointPreFabs;
        private void ShowPoint(LidarPoint lidarPoint)
        {
            int index = bigLidarPoints.FindIndex(LidarPoint => LidarPoint == lidarPoint);
            GameObject point = new GameObject("Point" + index);
            point.transform.position = new Vector3(lidarPoint.overlay.x, lidarPoint.overlay.y, 0);
            point.transform.eulerAngles = new Vector3(0,0,lidarPoint.overlay.z);

            foreach (float2 lidarPointPosition in lidarPoint.positions)
            {
                GameObject o = Instantiate(pointPreFabs[index], 
                    new Vector3(lidarPointPosition.x, lidarPointPosition.y, 0), Quaternion.identity);
                o.transform.SetParent(point.transform, false);
            }
        }

        private int frame;
        private List<List<int2>> data;
        private int pushDataSpeed = 1;
        private string[] csvFiles =
        {
            "Testdata_gedreht_0.csv",
            "Testdata_gedreht_1.csv",
            "Testdata_gedreht_2.csv"
        };
        public void SimulateData()
        {
            if (frame == 0)
            {
                data = new List<List<int2>>();
                foreach (string csvFile in csvFiles)
                {
                    string[][] csvData = Csv.ParseCVSFile(File.ReadAllText(Application.dataPath + "\\" + csvFile));

                    
                    List<int2> dataset = new List<int2>();
                    int lastangle = -1;
                    for (int i = 0; i < csvData.Length; i++)
                    {
                        string[] line = csvData[i];
                        if(line.Length < 2) continue;
                    
                        int angle = (int) float.Parse(line[0]);
                        int distance = int.Parse(line[1]);
                        
                        if (lastangle > angle)
                        {
                            data.Add(dataset);
                            dataset = new List<int2>();
                        }
                        lastangle = angle;
                        
                        dataset.Add(new int2(angle, distance));
                    }
                    data.Add(dataset);
                }
            }
            bigLidarPointActive.Value = frame % (100 * pushDataSpeed) != 0;

            if (frame % pushDataSpeed == 0)
            {
                int index = frame / pushDataSpeed;
                if (index < data.Count)
                {
                    AddLidarData(data[index]);
                }
            }
            
            frame++;
        }
    }
}
