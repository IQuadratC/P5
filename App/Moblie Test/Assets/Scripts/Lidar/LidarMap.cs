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
        [SerializeField] private BoolVariable bigLidarPointActive;
        private bool wasBigLidarPointActive;
        private List<LidarPoint> lidarPointsProcessing;

        public List<LidarPoint> BigLidarPoints { get; private set; }
        public List<LidarPoint> SmallLidarPoints { get; private set; }
        public float3 CurrentPosition { get; private set; }
        
        [SerializeField] private float maxDistance = 0.5f;
        [SerializeField] private int minLineLength = 5;
        [SerializeField] private int bounds = 500;

        private void Awake()
        {
            BigLidarPoints = new List<LidarPoint>();
            SmallLidarPoints = new List<LidarPoint>();
            lidarPointsProcessing = new List<LidarPoint>();
        }

        public void AddLidarData(List<int2> data)
        {
            if (bigLidarPointActive.Value)
            {
                LidarPoint lidarPoint;
                if (!wasBigLidarPointActive)
                {
                    lidarPoint = new LidarPoint(true, maxDistance, minLineLength, bounds);

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
                LidarPoint lidarPoint = new LidarPoint(false, maxDistance, minLineLength, bounds);

                lidarPoint.AddData(data);
                lidarPoint.State = LidarPointState.waitingForUpdate;
                
                lidarPointsProcessing.Add(lidarPoint);
            }
        }

        private void Update()
        {
            if (simulateData)
            {
                SimulateData();
            }

            for (int i = lidarPointsProcessing.Count - 1; i >= 0; i--)
            {
                LidarPoint lidarPoint = lidarPointsProcessing[i];
                switch (lidarPoint.State)
                {
                    case LidarPointState.addingData:
                        if (lidarPoint.IsBigLidarPoint && wasBigLidarPointActive && !bigLidarPointActive.Value)
                        {
                            lidarPoint.State = LidarPointState.waitingForUpdate;
                        }
                        break;
                    
                    case LidarPointState.waitingForUpdate:

                        if (BigLidarPoints.Count > 0)
                        {
                            lidarPoint.Update(BigLidarPoints[BigLidarPoints.Count -1]);
                        }
                        else
                        {
                            if (lidarPoint.IsBigLidarPoint)
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
                        if (lidarPoint.IsBigLidarPoint)
                        {
                            BigLidarPoints.Add(lidarPoint);
                        }
                        else
                        {
                            SmallLidarPoints.Add(lidarPoint);
                            CurrentPosition = lidarPoint.Overlay.xyz;
                        }
                        ShowPoint(lidarPoint);
                        break;
                }
            }

            if (!bigLidarPointActive.Value)
            {
                wasBigLidarPointActive = false;
            }
            
        }

        private int counter;
        [SerializeField] private GameObject[] pointPreFabs;
        private void ShowPoint(LidarPoint lidarPoint)
        {
            GameObject point = new GameObject("Point " + counter +" "+ lidarPoint.IsBigLidarPoint);
            point.transform.position = new Vector3(lidarPoint.Overlay.x, lidarPoint.Overlay.y, 0);
            point.transform.eulerAngles = new Vector3(0,0,lidarPoint.Overlay.z);

            foreach (float2 lidarPointPosition in lidarPoint.Positions)
            {
                GameObject o = Instantiate(pointPreFabs[counter], 
                    new Vector3(lidarPointPosition.x, lidarPointPosition.y, 0), Quaternion.identity);
                o.transform.SetParent(point.transform, false);
            }

            counter++;
        }

        [SerializeField] private bool simulateData;
        private int frame;
        private List<List<int2>> data;
        [SerializeField] private int pushDataSpeed = 1;
        private string[] csvFiles =
        {
            "Testdata_gedreht_0.csv",
            "Testdata_gedreht_1.csv",
            "Testdata_gedreht_2.csv"
        };
        private void SimulateData()
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
                    
                        int angle = int.Parse(line[0].Split('.')[0]);
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
