using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Lidar.SLAM
{
    public class SLAMMap
    {
        public Dictionary<int2, SLAMMapChunk> chunks;
        public int cellsPerChunk;
        public int scale;

        public SLAMMap(int cellsPerChunk, int scale)
        {
            this.cellsPerChunk = cellsPerChunk;
            this.scale = scale;
            chunks = new Dictionary<int2, SLAMMapChunk>();
        }

        public float GetMapScaled(float2 pos)
        {
            return GetMap((int2) (pos / scale));
        }
        public float GetMap(int2 pos)
        {
            SLAMMapChunk chunk = GetChunkByPos(pos);
            int2 chunkPos = pos - chunk.pos;
            return chunk.grid[chunkPos.x, chunkPos.y];
        }

        public void SetMapScaled(float2 pos, int value)
        {
            float2 unscaledPos = pos / scale;
            int2 intPos = (int2) unscaledPos;
            if (unscaledPos.x - intPos.x > 0.5)
            {
                intPos.x++;
            }
            if (unscaledPos.y - intPos.y > 0.5)
            {
                intPos.y++;
            }
            
            SetMap(intPos, value);
        }
        public void SetMap(int2 pos, int value)
        {
            SLAMMapChunk chunk = GetChunkByPos(pos);
            int2 chunkPos = pos - chunk.pos;
            chunk.grid[chunkPos.x, chunkPos.y] = value;
        }

        private SLAMMapChunk GetChunkByPos(int2 pos)
        {
            int2 chunkPos = pos / cellsPerChunk * cellsPerChunk;
            if (pos.x < 0 && pos.x != chunkPos.x)
            {
                chunkPos.x -= cellsPerChunk;
            }
            if (pos.y < 0 && pos.y != chunkPos.y)
            {
                chunkPos.y -= cellsPerChunk;
            }
            
            if (!chunks.ContainsKey(chunkPos))
            {
                chunks[chunkPos] = new SLAMMapChunk(chunkPos, cellsPerChunk);
            }
            SLAMMapChunk chunk = chunks[chunkPos];
            return chunk;
        }

        public void AddDataSet(SLAMLidarDataSet dataSet, float3 t)
        {
            foreach (float2 point in dataSet.points)
            {
                SetMapScaled(SLAMMath.Si(t, point), 1);
            }
        }
    }
    
    public class SLAMMapChunk
    {
        public int2 pos;
        public float[,] grid;

        public SLAMMapChunk(int2 pos, int cellsPerChunk)
        {
            this.pos = pos;
            grid = new float[cellsPerChunk, cellsPerChunk];
        }
    }
}