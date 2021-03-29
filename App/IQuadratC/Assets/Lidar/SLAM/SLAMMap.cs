using System.Collections.Generic;
using Unity.Mathematics;

namespace Lidar.SLAM
{
    public class SLAMMap
    {
        private Dictionary<int2, SLAMMapChunk>[] chunksLevels;
        private int[] levels;
        
        public SLAMMap(int[] levels)
        {
            this.levels = levels;
            
            chunksLevels = new Dictionary<int2, SLAMMapChunk>[levels.Length];
            for (int i = 0; i < chunksLevels.Length; i++)
            {
                chunksLevels[i] = new Dictionary<int2, SLAMMapChunk>();
            }
        }

        public int GetMap(float2 pos, int level)
        {
            SLAMMapChunk chunk = GetChunkByPos(pos, level);
            int2 chunkPos = (int2) pos - chunk.pos;
            return chunk.grid[chunkPos.x, chunkPos.y];
        }

        public void SetMap(float2 pos, int level, int value)
        {
            SLAMMapChunk chunk = GetChunkByPos(pos, level);
            int2 chunkPos = (int2) pos - chunk.pos;
            chunk.grid[chunkPos.x, chunkPos.y] = value;
        }

        private SLAMMapChunk GetChunkByPos(float2 pos, int level)
        {
            Dictionary<int2, SLAMMapChunk> chunks = chunksLevels[level];
            int2 chunkPos = (int2)pos / levels[level] * levels[level];
            return chunks[chunkPos];
        }
    }
    
    public class SLAMMapChunk
    {
        public int level;
        public int2 pos;
        public int[,] grid;

        public SLAMMapChunk(int level, int2 pos, int cellsPerChunk)
        {
            this.level = level;
            this.pos = pos;
            grid = new int[cellsPerChunk, cellsPerChunk];
        }
    }
}