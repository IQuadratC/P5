using System;
using System.Collections.Generic;
using Lidar.PreFabs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using Utility.Variables;

namespace Lidar
{
    public class LidarMap : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private Vec2Variable position;
        [SerializeField] private Int2ListVariable points;

        [SerializeField] private MeshFilter backgroundFilter;
        [SerializeField] private int backgronudSize;

        [SerializeField] private GameObject chunkPreFab;
        private Dictionary<int2, LidarMapChunk> chunks;

        private void Awake()
        {
            chunks = new Dictionary<int2, LidarMapChunk>();

            Vector3[] vertex = 
            {
                new Vector3(-backgronudSize, -backgronudSize, 2),
                new Vector3(backgronudSize, -backgronudSize, 2),
                new Vector3(-backgronudSize, backgronudSize, 2),
                new Vector3(backgronudSize, backgronudSize, 2)
            };
            int[] indices = {2, 1, 0, 3, 1, 2};
            
            Vector2[] uvoff = {Vector2.one, Vector2.one, Vector2.one, Vector2.one};
            
            Mesh mesh = new Mesh();
            mesh.vertices = vertex;
            mesh.triangles = indices;
            mesh.uv = uvoff;
            mesh.uv2 = uvoff;
            mesh.uv3 = uvoff;
            mesh.uv4 = uvoff;
            backgroundFilter.mesh = mesh;
        }

        private int index;
        [SerializeField] private int mapScale = 10;
        [SerializeField] private int chunkBounds = 50;
        public void UpdateMap()
        {
            int newIndex = points.Value.Count;

            List<LidarMapChunk> touchedChunks = new List<LidarMapChunk>();
            while (index < newIndex)
            {
                int2 scaledPos = points.Value[index] / mapScale * mapScale;
                if (points.Value[index].x < 0)
                {
                    scaledPos.x -= mapScale;
                }
                if (points.Value[index].y < 0)
                {
                    scaledPos.y -= mapScale;
                }
                
                int2 chunkPos = points.Value[index] / (mapScale * chunkBounds) * (mapScale * chunkBounds);
                if (points.Value[index].x < 0)
                {
                    chunkPos.x -= (mapScale * chunkBounds);
                }
                if (points.Value[index].y < 0)
                {
                    chunkPos.y -= (mapScale * chunkBounds);
                }

                LidarMapChunk chunk;
                if (chunks.ContainsKey(chunkPos))
                {
                    chunk = chunks[chunkPos];
                }
                else
                {
                    chunk = Instantiate(chunkPreFab,
                        new Vector3(chunkPos.x, chunkPos.y, 0),
                        Quaternion.identity).GetComponent<LidarMapChunk>();
                    chunk.ChunkBounds = chunkBounds;
                    chunk.MapScale = mapScale;
                    chunk.Points = new bool[chunkBounds, chunkBounds];
                    
                    chunk.gameObject.transform.SetParent(gameObject.transform);
                    chunks[chunkPos] = chunk;
                }

                chunk.Points[
                    (scaledPos - chunkPos).x / mapScale, 
                    (scaledPos - chunkPos).y / mapScale] = true;

                if (!touchedChunks.Contains(chunk))
                {
                    touchedChunks.Add(chunk);
                }
                
                index++;
            }

            foreach (LidarMapChunk chunk in touchedChunks)
            {
                chunk.UpdateMesh();
            }
        }
    }
}