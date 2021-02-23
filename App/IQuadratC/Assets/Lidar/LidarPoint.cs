using System.Collections.Generic;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;
using Utility;
using Utility.PointMatcher;

namespace Lidar
{
    public enum LidarPointState
    {
        addingData = 1,
        waitingForCalculation = 2,
        readyForCalculation = 3,
        performingCalculation = 4,
        finished = 5
    }
    
    public class LidarPoint
    {
        public LidarPointState State { get; set; }
        public float[] Distances { get; private set; }
        public float2[] Positions { get; private set; }
        
        private float4 overlay;
        public float4 Overlay => overlay;
        
        public List<float4> finallines;

        public LidarPoint(float maxDistance, int minLineLength, int bounds)
        {
            State = LidarPointState.addingData;
            Distances = new float[360];
            finallines = new List<float4>();
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
            State = LidarPointState.performingCalculation;
            this.otherLidarPoint = otherLidarPoint;
            CalculatePositions();
            CalculateOverlay();
            State = LidarPointState.finished;
        }
        
        public void Calculate()
        {
            State = LidarPointState.performingCalculation;
            CalculatePositions();
            State = LidarPointState.finished;
        }

        private void CalculatePositions()
        {
            Positions = new float2[360];
            for (int i = 0; i < Distances.Length; i++)
            {
                Positions[i] = new float2(
                    math.sin(i * math.PI / 180) * Distances[i],
                    math.cos(i * math.PI / 180) * Distances[i]);
            }
        }

        private void CalculateOverlay()
        {
            float3 outPosisition = new float3();
            quaternion outQuaternion = new quaternion();
            
            PointMatcherWrapper.perform(Positions, otherLidarPoint.Positions, 
                float3.zero, quaternion.identity, out outPosisition, out outQuaternion);
            
            overlay = new float4(outPosisition.xy, 0, 0);
        }
        
        public static float2 ApplyOverlay(float2 pos, float4 overlay)
        {
            return mathAdditions.Rotate(pos, overlay.z) + overlay.xy;
        }
        
    }
}