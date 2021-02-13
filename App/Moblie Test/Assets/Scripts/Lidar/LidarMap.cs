using System;
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
        private List<LidarPoint> lidarPointsProcessing;

        public List<LidarPoint> LidarPoints { get; private set; }
        [SerializeField] private Vec2Variable position;

        public Dictionary<int2, int> Map { get; private set; }
        [SerializeField] private int mapScale = 10;
        public int MapScale => mapScale;
        
        [SerializeField] private float maxDistance = 0.5f;
        [SerializeField] private int minLineLength = 5;
        [SerializeField] private int bounds = 500;

        private void Awake()
        {
            LidarPoints = new List<LidarPoint>();
            lidarPointsProcessing = new List<LidarPoint>();
            meshFilter = meshObject.GetComponent<MeshFilter>();
            meshMaterial = meshObject.GetComponent<MeshRenderer>().material;
        }

        private void Start()
        {
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
                AddLidarData(new int2[0]);
                lidarPointsProcessing[lidarPointsProcessing.Count - 1].LoadCSVData(csvFile);
                PushLidarData();
                
                /*
                string[][] csvData = Csv.ParseCVSFile(File.ReadAllText(Application.dataPath + "\\" + csvFile));

                List<int2> data = new List<int2>();
                for (int i = 0; i < csvData.Length; i++)
                {
                    string[] line = csvData[i];
                    if (line.Length < 2) continue;

                    int angle = int.Parse(line[0].Split('.')[0]);
                    int distance = int.Parse(line[1]);
                    
                    if(distance == 0) continue;

                    int index = -1;
                    for (int j = 0; j < data.Count; j++)
                    {
                        if (data[j].x == angle)
                        {
                            index = j;
                        }
                    }

                    if (index == -1)
                    {
                        data.Add(new int2(angle, distance));
                    }
                    else
                    {
                        data[index] = (new int2(angle, distance) + data[index]) / 2;
                    }
                }

                AddLidarData(data.ToArray());
                PushLidarData();*/
            }
        }

        private bool isAdding;
        private void AddLidarData(int2[] data)
        {
            LidarPoint lidarPoint;
            if (!isAdding)
            {
                isAdding = true;
                lidarPoint = new LidarPoint();
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
                        if (LidarPoints.Count > 0)
                        {
                            lidarPoint.Calculate(LidarPoints[0]);
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
                        LidarPoints.Add(lidarPoint);
                        
                        position.Value = new float2(lidarPoint.Overlay.xy);
                        meshMaterial.SetVector("Position",
                            new Vector4(position.Value.x, position.Value.y, 0,0));
                        
                        UpdateMap();
                        if (showPoints)
                        {
                            ShowPoint(lidarPoint);
                        }
                        if (showMap)
                        {
                            UpdateMapMesh();
                        }
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

            foreach (Vector2 lidarPointPosition in lidarPoint.positions.Values)
            {
                GameObject o = Instantiate(pointPreFabs[counter], 
                    new Vector3(lidarPointPosition.x, lidarPointPosition.y, 0), Quaternion.identity);
                o.transform.SetParent(point.transform, false);
            }

            foreach (List<Vector2> line in lidarPoint.lines)
            {
                foreach (float2 float2 in line)
                {
                    GameObject o = Instantiate(linePreFab, new Vector3(float2.x, float2.y, 0), Quaternion.identity);
                    o.transform.SetParent(point.transform, false);
                }
            }
            
            foreach (Vector2 intersectionPoint in lidarPoint.intersectionPoints)
            {
                GameObject o = Instantiate(linePreFab, intersectionPoint, Quaternion.identity);
                o.transform.SetParent(point.transform, false);
            }

            counter++;
        }

        private void UpdateMap()
        {
            Map = new Dictionary<int2, int>();
            foreach (LidarPoint bigLidarPoint in LidarPoints)
            {
                foreach (float2 position in bigLidarPoint.positions.Values)
                {
                    if(position.Equals(int2.zero)) continue;
                    int2 pos = new int2(LidarPoint.ApplyOverlay(position, bigLidarPoint.Overlay) / mapScale) * mapScale;

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

        [SerializeField] private int meshBounds;
        [SerializeField] private GameObject meshObject;
        private MeshFilter meshFilter;
        private Material meshMaterial;
        private void UpdateMapMesh()
        {
            bool[,] meshData = new bool[meshBounds * 2, meshBounds * 2];
            foreach (KeyValuePair<int2,int> keyValuePair in Map)
            {
                int x = keyValuePair.Key.x / mapScale;
                int y = keyValuePair.Key.y / mapScale;
                if(x < -meshBounds || x >= meshBounds || y < -meshBounds || y >= meshBounds) continue;
                
                meshData[x + meshBounds, y + meshBounds] = true;
            }
           
            
            List<Vector3> vertices = new List<Vector3>();
            List<int> indices = new List<int>();
            List<Vector2> uv = new List<Vector2>();
            List<Vector2> uv1 = new List<Vector2>();
            List<Vector2> uv2 = new List<Vector2>();
            List<Vector2> uv3 = new List<Vector2>();
            
            Vector2[] uvoff = {Vector2.one, Vector2.one, Vector2.one, Vector2.one,};
            Vector2[] uvon = {Vector2.zero, Vector2.one, Vector2.zero, Vector2.one,};
            Vector2[] uv1on = {Vector2.one, Vector2.zero, Vector2.one, Vector2.zero,};
            Vector2[] uv2on = {Vector2.zero, Vector2.zero, Vector2.one, Vector2.one,};
            Vector2[] uv3on = {Vector2.one, Vector2.one, Vector2.zero, Vector2.zero,};

            for (int i = 0; i < meshBounds * 2; i++)
            {
                for (int j = 0; j < meshBounds * 2; j++)
                {
                    int x = (i - meshBounds) * mapScale;
                    int y = (j - meshBounds) * mapScale;
                    int z = meshData[i, j] ? 1 : 0;

                    vertices.Add(new Vector3(x, y, z));
                    vertices.Add(new Vector3(x + mapScale, y, z));
                    vertices.Add(new Vector3(x, y + mapScale, z));
                    vertices.Add(new Vector3(x + mapScale, y + mapScale, z));
                    
                    int k = (i * meshBounds * 2 + j) * 4;
                    indices.AddRange(new []{k + 2, k + 1, k, k + 1, k + 2, k + 3});

                    if (meshData[i, j])
                    {
                        uv.AddRange(uvoff);
                        uv1.AddRange(uvoff);
                        uv2.AddRange(uvoff);
                        uv3.AddRange(uvoff);
                    }
                    else
                    {
                        if (i > 0 && meshData[i - 1, j])
                        {
                            uv.AddRange(uvon);
                        }
                        else { uv.AddRange(uvoff); }
                        
                        if (i < meshBounds * 2 - 1 && meshData[i + 1, j])
                        {
                            uv1.AddRange(uv1on);
                        }
                        else { uv1.AddRange(uvoff); }
                        
                        if (j > 0 && meshData[i, j - 1])
                        {
                            uv2.AddRange(uv2on);
                        }
                        else { uv2.AddRange(uvoff); }
                        
                        if (j < meshBounds * 2 - 1 && meshData[i, j + 1])
                        {
                            uv3.AddRange(uv3on);
                        }
                        else { uv3.AddRange(uvoff); }
                    }

                }
            }
            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = indices.ToArray();
            mesh.uv = uv.ToArray();
            mesh.uv2 = uv1.ToArray();
            mesh.uv3 = uv2.ToArray();
            mesh.uv4 = uv3.ToArray();
            meshFilter.mesh = mesh;
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