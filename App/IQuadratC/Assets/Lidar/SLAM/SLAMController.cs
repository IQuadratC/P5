using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Utility.Variables;

namespace Lidar.SLAM
{
    public class SLAMController : MonoBehaviour
    {
        [SerializeField] private int[] mapScales;
        private SLAMMap[] maps;
        private List<SLAMLidarDataSet> dataSets;
        private SLAMLidarDataSet currentDataSet;
        private float3 t;
        private List<float3> ts;

        private void Awake()
        {
            maps = new SLAMMap[mapScales.Length];
            for (int i = 0; i < mapScales.Length; i++)
            {
                maps[i] = new SLAMMap(100, mapScales[i]);
            }
            
            dataSets = new List<SLAMLidarDataSet>();
            ts = new List<float3>();
        }

        private void Start()
        {
            SimulateData();
        }

        private string[] files =
        {
            "Data",
            "Data_Move_1",
            //"Data_Move_Rotate_1",
            //"Data_Rotate_1"
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
        
        private void AddLidarData(int2[] data)
        {
            if (currentDataSet == null)
            {
                currentDataSet = new SLAMLidarDataSet();
                dataSets.Add(currentDataSet);
            }
            currentDataSet.AddData(data);
        }

        private void PushLidarData()
        {
            if (dataSets.Count > 1)
            {
                for (int i = maps.Length - 1; i >= 0; i--)
                {
                    for (int j = 0; j < 100; j++)
                    {
                        float3 newT = SLAMMath.TransformDeltaDir(t, currentDataSet, maps[i]);
                        t += newT;
                        Debug.Log(t +" " + newT);
                        if (math.length(newT) < 0.3)
                        {
                            break;
                        }
                    }
                }
            }
            foreach (SLAMMap map in maps)
            {
                map.AddDataSet(currentDataSet, t);
            }
            ts.Add(t);

            currentDataSet = null;
        }

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            Gizmos.color = Color.white;
            foreach (SLAMMap map in maps)
            {
                Gizmos.color -= new Color(1.0f / mapScales.Length, 1.0f / mapScales.Length, 1.0f / mapScales.Length, 0.0f);
                foreach (KeyValuePair<int2,SLAMMapChunk> keyValuePair in map.chunks)
                {
                    SLAMMapChunk chunk = keyValuePair.Value;
                    for (int i = 0; i < map.cellsPerChunk; i++)
                    {
                        for (int j = 0; j < map.cellsPerChunk; j++)
                        {
                            if (chunk.grid[i,j] != 0)
                            {
                                Gizmos.DrawSphere(new float3((new float2(i, j) + chunk.pos) * map.scale, 0) , 1);
                            }
                        }
                    }
                }
            }
            
            Gizmos.color = Color.green;

            foreach (float3 t in ts)
            {
                float rad = t.z + 90 * math.PI / 180;
                Gizmos.DrawLine(new float3(t.xy, 0), new float3(t.xy + new float2(math.cos(rad), math.sin(rad)) * 5  , 0));
            }
        }
    }
}