using Unity.Mathematics;
using UnityEngine;

namespace Lidar
{
    [CreateAssetMenu(fileName = "LidarSettings", menuName = "Utility/LidarSettings")]
    public class LidarSettings : ScriptableObject
    {
        public float maxLineDistance = 2f;
        public int minLineLength = 10;
        public int minLinePoints = 10;
        
        public int interscetionBounds = 500;
        public float sameIntersectionRadius = 1f;
        
        public float maxCornerDistance = 30f;
        public float minCornerAngle = 20;
        public float maxCornerAngle = 20;
        public int minCornerAmmount = 2;

        public float minOverlayCornerAngleDiffernce = 5;
        public float overlayRayVectorMultiplyer = 100;
    }
}
