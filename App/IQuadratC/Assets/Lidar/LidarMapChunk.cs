using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace Lidar
{
    public class LidarMapChunk : MonoBehaviour
    {
        public bool[,] Points { get; set; }
        public int ChunkBounds { get; set; }
        public int MapScale { get; set; }

        [SerializeField] private MeshFilter meshFilter;
        [SerializeField] private MeshRenderer meshRenderer;
        public MeshRenderer MeshRenderer => meshRenderer;

        public void OnNewPoints()
        {
            Threader.RunAsync(UpdateMesh);
        }
        
        private void UpdateMesh()
        {
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

            for (int i = 0; i < ChunkBounds; i++)
            {
                for (int j = 0; j < ChunkBounds; j++)
                {
                    int x = i * MapScale;
                    int y = j * MapScale;
                    int z = Points[i, j] ? 0 : 1;

                    vertices.Add(new Vector3(x, y, z));
                    vertices.Add(new Vector3(x + MapScale, y, z));
                    vertices.Add(new Vector3(x, y + MapScale, z));
                    vertices.Add(new Vector3(x + MapScale, y + MapScale, z));
                    
                    int k = (i * ChunkBounds + j) * 4;
                    indices.AddRange(new []{k + 2, k + 1, k, k + 1, k + 2, k + 3});

                    if (Points[i, j])
                    {
                        uv.AddRange(uvoff);
                        uv1.AddRange(uvoff);
                        uv2.AddRange(uvoff);
                        uv3.AddRange(uvoff);
                    }
                    else
                    {
                        if (i > 0 && Points[i - 1, j])
                        {
                            uv.AddRange(uvon);
                        }
                        else { uv.AddRange(uvoff); }
                        
                        if (i < ChunkBounds - 1 && Points[i + 1, j])
                        {
                            uv1.AddRange(uv1on);
                        }
                        else { uv1.AddRange(uvoff); }
                        
                        if (j > 0 && Points[i, j - 1])
                        {
                            uv2.AddRange(uv2on);
                        }
                        else { uv2.AddRange(uvoff); }
                        
                        if (j < ChunkBounds - 1 && Points[i, j + 1])
                        {
                            uv3.AddRange(uv3on);
                        }
                        else { uv3.AddRange(uvoff); }
                    }

                }
            }

            void OnMain()
            {
                Mesh mesh = new Mesh();
                mesh.vertices = vertices.ToArray();
                mesh.triangles = indices.ToArray();
                mesh.uv = uv.ToArray();
                mesh.uv2 = uv1.ToArray();
                mesh.uv3 = uv2.ToArray();
                mesh.uv4 = uv3.ToArray();
                meshFilter.mesh = mesh;
                
                NewPointEffect();
            }
            Threader.RunOnMainThread(OnMain);
        }

        private static readonly int flashStartTimeId = Shader.PropertyToID("FlashStartTime");
        private void NewPointEffect()
        {
            meshRenderer.material.SetFloat(flashStartTimeId, Time.time);
        }
    }
}