using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Utility;
using Utility.Events;
using Utility.Variables;

namespace Lidar
{
    public class LidarController : MonoBehaviour
    {
        [SerializeField] private Vec3Variable position;
        
        private List<LidarPoint> lidarPointsProcessing;
        private List<LidarPoint> lidarPoints;
        [SerializeField] private Int2ListVariable points;

        [SerializeField] private GameEvent newPoints;

        [SerializeField] private LidarSettings lidarSettings;

        private void OnEnable()
        {
            lidarPoints = new List<LidarPoint>();
            lidarPointsProcessing = new List<LidarPoint>();
            position.Value = new float3();
            points.Value.Clear();
            pointIdcounter = 0;
            
            if (simulateData)
            {
                SimulateData();
            }
        }
        
        [SerializeField] private bool simulateData;
        private string[] files =
        {
            "Data",
            "Data_Move_1",
            "Data_Move_Rotate_1",
            "Data_Rotate_1"
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
                lidarPoint = new LidarPoint(lidarSettings);
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
                            lidarPoint.StartCalculate(lidarPoints[0]);
                        }
                        else
                        {
                            lidarPoint.StartCalculate();
                        }
                        
                        break;
                    
                    case LidarPointState.processingCalculation:
                        break;
                    
                    case LidarPointState.dropped:
                        lidarPointsProcessing.Remove(lidarPoint);

                        logString.Value = "Lidarpunkt hat zu wengig Ecken";
                        logEvent.Raise();
                        
                        break;

                    case LidarPointState.finished:
                        lidarPointsProcessing.Remove(lidarPoint);
                        lidarPoints.Add(lidarPoint);

                        position.Value = lidarPoint.Overlay;

                        foreach (float2 worldPoint in lidarPoint.WorldPoints)
                        {
                            points.Value.Add((int2)worldPoint);
                        }
                        
                        ShowPoint(lidarPoint);
                        newPoints.Raise();
                        
                        logString.Value = "Lidarpunkt eingebunden";
                        logEvent.Raise();
                        
                        break;
                }
            }
        }

        [SerializeField] private bool showPoints;
        [SerializeField] private bool showLines;
        [SerializeField] private bool showInteresctions;
        [SerializeField] private bool showConers;

        private int pointIdcounter;
        [SerializeField] private GameObject[] pointPreFabs;
        [SerializeField] private GameObject linePreFab;
        [SerializeField] private GameObject showPartent;
        private void ShowPoint(LidarPoint lidarPoint)
        {
            if (showPoints)
            {
                GameObject pointO = new GameObject("Point " + pointIdcounter);
                pointO.transform.position = new Vector3(lidarPoint.Overlay.x, lidarPoint.Overlay.y, 0);
                pointO.transform.eulerAngles = new Vector3(0,0,lidarPoint.Overlay.z);
                pointO.transform.SetParent(showPartent.transform);
                
                foreach (float2 point in lidarPoint.Points)
                {
                    GameObject o = Instantiate(
                        pointPreFabs[pointIdcounter],
                        new Vector3(point.x, point.y, 0), 
                        Quaternion.identity);
                    o.transform.SetParent(pointO.transform, false);
                }
            }
            
            if (showLines)
            {
                GameObject linesO = new GameObject("Lines " + pointIdcounter);
                linesO.transform.position = new Vector3(lidarPoint.Overlay.x, lidarPoint.Overlay.y, 0);
                linesO.transform.eulerAngles = new Vector3(0,0,lidarPoint.Overlay.z);
                linesO.transform.SetParent(showPartent.transform);

                int id = 0;
                foreach (LidarLine line in lidarPoint.Lines)
                {
                    GameObject lineO = new GameObject("Line " + id);
                    lineO.transform.SetParent(linesO.transform, false);
                    foreach (float2 point in line.points)
                    {
                        GameObject o = Instantiate(
                            pointPreFabs[pointIdcounter],
                            new Vector3(point.x, point.y, 0), 
                            Quaternion.identity);
                        o.transform.SetParent(lineO.transform, false);
                    }

                    id++;
                }
            }
            
            if (showInteresctions)
            {
                GameObject intersctionO = new GameObject("Intersctions " + pointIdcounter);
                intersctionO.transform.position = new Vector3(lidarPoint.Overlay.x, lidarPoint.Overlay.y, 0);
                intersctionO.transform.eulerAngles = new Vector3(0,0,lidarPoint.Overlay.z);
                intersctionO.transform.SetParent(showPartent.transform);
                
                foreach (float2 intersection in lidarPoint.Intersections)
                {
                    GameObject o = Instantiate(
                        pointPreFabs[pointIdcounter],
                        new Vector3(intersection.x, intersection.y, 0), 
                        Quaternion.identity);
                    o.transform.SetParent(intersctionO.transform, false);
                }
            }
            
            if (showConers)
            {
                GameObject cornersO = new GameObject("Corners " + pointIdcounter);
                cornersO.transform.position = new Vector3(lidarPoint.Overlay.x, lidarPoint.Overlay.y, 0);
                cornersO.transform.eulerAngles = new Vector3(0,0,lidarPoint.Overlay.z);
                cornersO.transform.SetParent(showPartent.transform);

                int id = 0;
                foreach (LidarCorner corner in lidarPoint.Corners)
                {
                    GameObject cornerO = new GameObject("Corner " + id);
                    cornerO.transform.SetParent(cornersO.transform, false);
                    
                    foreach (float2 point in corner.line1.points)
                    {
                        GameObject o = Instantiate(
                            pointPreFabs[pointIdcounter],
                            new Vector3(point.x, point.y, 0), 
                            Quaternion.identity);
                        o.transform.SetParent(cornerO.transform, false);
                    }
                    
                    foreach (float2 point in corner.line2.points)
                    {
                        GameObject o = Instantiate(
                            pointPreFabs[pointIdcounter],
                            new Vector3(point.x, point.y, 0), 
                            Quaternion.identity);
                        o.transform.SetParent(cornerO.transform, false);
                    }

                    id++;
                }
            }
            
            

            pointIdcounter++;
        }

        [SerializeField] private GameEvent sendEvent;
        [SerializeField] private StringVariable sendString;
        public void ReqestData()
        {
            logString.Value = "Daten werden angefragt.";
            logEvent.Raise();
            
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
                logString.Value = "Teildaten entfangen";
                logEvent.Raise();
                
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
                logString.Value = "Daten entfangen";
                logEvent.Raise();
                
                PushLidarData();
            }
        }

        private void OnDisable()
        {
            int childs = showPartent.transform.childCount;
            for (int i = childs - 1; i >= 0; i--)
            {
                Destroy(showPartent.transform.GetChild(i).gameObject);
            }
        }

        [SerializeField] private GameEvent logEvent;
        [SerializeField] private StringVariable logString;
    }
}