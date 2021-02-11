using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;
using Utility;
using Utility.Events;
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
        
        public Dictionary<int2, int> Map { get; private set; }
        [SerializeField] private int mapScale = 10;
        public int MapScale => mapScale;
        
        [SerializeField] private float maxDistance = 0.5f;
        [SerializeField] private int minLineLength = 5;
        [SerializeField] private int bounds = 500;

        private void Awake()
        {
            BigLidarPoints = new List<LidarPoint>();
            SmallLidarPoints = new List<LidarPoint>();
            lidarPointsProcessing = new List<LidarPoint>();
        }

        public void AddLidarData(int2[] data)
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
                        UpdateMap();

                        if (showPoints)
                        {
                            ShowPoint(lidarPoint);
                        }
                        if (showMap)
                        {
                            ShowMap();
                        }
                        break;
                }
            }

            if (!bigLidarPointActive.Value)
            {
                wasBigLidarPointActive = false;
            }
            
        }

        [SerializeField] private bool showPoints;
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

        private void UpdateMap()
        {
            Map = new Dictionary<int2, int>();
            foreach (LidarPoint bigLidarPoint in BigLidarPoints)
            {
                foreach (float2 position in bigLidarPoint.Positions)
                {
                    int2 pos = new int2(LidarPoint.ApplyOverlay(position, bigLidarPoint.Overlay) / mapScale) * mapScale;
                    if(pos.Equals(int2.zero)) continue;
                    
                    int value = 1;
                    if (Map.ContainsKey(pos))
                    {
                        value = Map[pos] + 1;
                    }
                    Map[pos] = value;
                }
            }
        }

        [SerializeField] private bool showMap;
        private List<GameObject> mapPoints;
        [SerializeField] private GameObject mapPreFab;
        [SerializeField] private GameObject mapParten;
        private void ShowMap()
        {
            if (mapPoints == null)
            {
                mapPoints = new List<GameObject>();
            }
            
            foreach (GameObject mapPoint in mapPoints)
            {
                Destroy(mapPoint);
            }
            mapPoints.Clear();
            foreach (int2 mapKey in Map.Keys)
            {
                GameObject o = Instantiate(mapPreFab, new Vector3(mapKey.x, mapKey.y, 0), Quaternion.identity);
                o.transform.SetParent(mapParten.transform);
                mapPoints.Add(o);
            }
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
                    AddLidarData(data[index].ToArray());
                }
            }
            
            frame++;
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
                bigLidarPointActive.Value = false;
            }
        }
    }
}