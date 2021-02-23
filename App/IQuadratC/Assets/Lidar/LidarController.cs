using System.Collections.Generic;
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

        private void OnEnable()
        {
            path = Application.dataPath;
            lidarPoints = new List<LidarPoint>();
            lidarPointsProcessing = new List<LidarPoint>();
            position.Value = new float2();
            points.Value.Clear();
            if (simulateData)
            {
                SimulateData();
            }
        }
        
        [SerializeField] private bool simulateData;
        private string path;
        private string[] files =
        {
            "Data",
            "Data_Move_1"
        };
        
        private void SimulateData()
        {
            foreach (string file in files)
            {
                TextAsset text = Resources.Load(file) as TextAsset;

                string[] commands = text.text.Split('\n');

                foreach (string command in commands)
                {
                    reciveString.Value = command;
                    ReciveData();
                }
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

                    case LidarPointState.finished:
                        lidarPointsProcessing.Remove(lidarPoint);
                        lidarPoints.Add(lidarPoint);
                        
                        position.Value = new float2(lidarPoint.Overlay.xy);

                        foreach (float2 pointPosition in lidarPoint.Positions)
                        {
                            if(pointPosition.Equals(float2.zero)) continue;
                            points.Value.Add((int2)LidarPoint.ApplyOverlay(pointPosition, lidarPoint.Overlay));
                        }
                        
                        if (showPoints)
                        {
                            ShowPoint(lidarPoint);
                        }
                        
                        newPoints.Raise();
                        break;
                }
            }
        }

        [SerializeField] private bool showPoints;
        private int counter;
        [SerializeField] private GameObject[] pointPreFabs;
        [SerializeField] private GameObject linePreFab;
        private void ShowPoint(LidarPoint lidarPoint)
        {
            GameObject point = new GameObject("Point " + counter);
            point.transform.position = new Vector3(lidarPoint.Overlay.x, lidarPoint.Overlay.y, 0);
            point.transform.eulerAngles = new Vector3(0,0,lidarPoint.Overlay.z);
            point.transform.SetParent(transform);

            foreach (float2 lidarPointPosition in lidarPoint.Positions)
            {
                GameObject o = Instantiate(pointPreFabs[counter],
                    new Vector3(lidarPointPosition.x, lidarPointPosition.y, 0), Quaternion.identity);
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
                string[] strs2 = strs[2].Split(',');
                int2[] data = new int2[strs2.Length];
                for (int i = 0; i < strs2.Length; i++)
                {
                    string[] args = strs2[i].Split(';');
                    if(args.Length < 2) continue;
                    
                    data[i].x = int.Parse(args[0]);
                    data[i].y = int.Parse(args[1]);
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