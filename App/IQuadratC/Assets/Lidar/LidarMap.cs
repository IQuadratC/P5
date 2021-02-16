using System;
using System.Collections.Generic;
using System.IO;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Utility;
using Utility.Events;
using Utility.Variables;

namespace Lidar
{
    public class LidarMap : MonoBehaviour
    {
        [SerializeField] private Vec2Variable position;
        [SerializeField] private Int2ListVariable points;
        
        public Dictionary<int2, int> Map { get; private set; }
        [SerializeField] private int mapScale = 10;

        [SerializeField] private int meshBounds;
        private MeshFilter meshFilter;
        private Material meshMaterial;

        private void Awake()
        {
            meshFilter = GetComponent<MeshFilter>();
            meshMaterial = GetComponent<MeshRenderer>().material;
        }

        private void Update()
        {
            meshMaterial.SetVector("Position",
                new Vector4(position.Value.x, position.Value.y, 0,0));
                        
            UpdateMap();
            UpdateMapMesh();
        }

        private void UpdateMap()
        {
            Map = new Dictionary<int2, int>();
            foreach (float2 position in points.Value)
            {
                if(position.Equals(int2.zero)) continue;
               
                int2 scaledPos = ((int2)position / mapScale) * mapScale;
                    
                if (position.x < 0)
                {
                    scaledPos.x -= mapScale;
                }
                if (position.y < 0)
                {
                    scaledPos.y -= mapScale;
                }

                int value = 1;
                if (Map.ContainsKey(scaledPos))
                {
                    value = Map[scaledPos] + 1;
                }
                Map[scaledPos] = value;
            }
        }

        
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
    }
}