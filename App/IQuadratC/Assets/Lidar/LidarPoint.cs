using System.Collections.Generic;
using System.Threading;
using Unity.Mathematics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Utility;

namespace Lidar
{
    public enum LidarPointState
    {
        addingData = 1,
        waitingForCalculation = 2,
        readyForCalculation = 3,
        finished = 4
    }
    
    public class LidarPoint
    {
        public LidarPointState State { get; set; }
        public float[] Distances { get; private set; }
        public float2[] Positions { get; private set; }
        
        public float2[] WorldPositions { get; private set; }
        
        public int[] FiltertPositionsIds { get; private set; }
        
        private float3 overlay;
        public float3 Overlay => overlay;

        public LidarPoint(float maxDistance, int icpRounds)
        {
            this.maxDistance = maxDistance;
            this.icpRounds = icpRounds;
            
            State = LidarPointState.addingData;
            Distances = new float[360];
            overlay = new float3();
        }
        
        public void AddData(int2[] data)
        {
            foreach (int2 int2 in data)
            {
                Distances[int2.x] = (float)int2.y / 10;
            }
        }
        
        private LidarPoint otherLidarPoint;
        public void Calculate(LidarPoint otherLidarPoint)
        {
            this.otherLidarPoint = otherLidarPoint;
            CalculatePositions();
            FilterPositions();
            RunICP();
            CalculateWorldPositions();
            State = LidarPointState.finished;
        }
        
        public void Calculate()
        {
            CalculatePositions();
            FilterPositions();
            CalculateWorldPositions();
            State = LidarPointState.finished;
        }

        private void CalculatePositions()
        {
            List<float2> positionList = new List<float2>();
            for (int i = 0; i < Distances.Length; i++)
            {
                float2 position = new float2(
                    math.sin(i * math.PI / 180) * Distances[i],
                    math.cos(i * math.PI / 180) * Distances[i]);

                if (!position.Equals(float2.zero))
                {
                    positionList.Add(position);
                }
            }

            Positions = positionList.ToArray();
        }

        private float maxDistance;
        private void FilterPositions()
        {
            List<int> filtertPositionList = new List<int>();
            for (int i = 0; i < Positions.Length; i++)
            {
                float2 closestPoint = mathAdditions.FindClosestPointInArray(Positions[i], Positions, false);
                if (math.distance(Positions[i], closestPoint) < maxDistance)
                {
                    filtertPositionList.Add(i);
                }
            }

            FiltertPositionsIds = filtertPositionList.ToArray();
        }

        
        private int icpRounds = 10;

        private float2[] otherFiltertWorldPositions;
        private void RunICP()
        {
            otherFiltertWorldPositions = new float2[otherLidarPoint.FiltertPositionsIds.Length];

            for (int i = 0; i < otherLidarPoint.FiltertPositionsIds.Length; i++)
            {
                otherFiltertWorldPositions[i] = 
                    otherLidarPoint.WorldPositions[otherLidarPoint.FiltertPositionsIds[i]];
            }
            
            for (int i = 0; i < icpRounds; i++)
            {
                ICP();
                Debug.Log("ICP "+ i+ " " + overlay);
            }
        }

        //https://github.com/richardos/icp/blob/a955cc674ef8da6f3ed4460eb132c4e150e8ad1b/icp.py#L19
        private void ICP()
        {
            float2 overAllVector = new float2();
            for (int i = 0; i < FiltertPositionsIds.Length; i++)
            {
                float2 closestPoint = mathAdditions.FindClosestPointInArray(
                    ApplyOverlay(Positions[FiltertPositionsIds[i]], overlay), 
                    otherFiltertWorldPositions, true);
                
                overAllVector += math.distance(
                    ApplyOverlay(Positions[FiltertPositionsIds[i]], overlay), 
                    closestPoint);
            }

            overAllVector /= Positions.Length;
            overlay.xy = overAllVector;
           
        }

        private void CalculateWorldPositions()
        {
            WorldPositions = new float2[Positions.Length];
            for (int i = 0; i < Positions.Length; i++)
            {
                WorldPositions[i] = ApplyOverlay(Positions[i], overlay);
            }
        }
        
        public static float2 ApplyOverlay(float2 pos, float3 overlay)
        {
            return mathAdditions.Rotate(pos, overlay.z) + overlay.xy;
        }
        
    }
}