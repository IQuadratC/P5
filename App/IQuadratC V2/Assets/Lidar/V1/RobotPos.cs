using Unity.Mathematics;
using UnityEngine;
using Utility.Variables;

namespace Lidar
{
    public class RobotPos : MonoBehaviour
    {
        [SerializeField] private Vec3Variable pos;
        void Update()
        {
            transform.position = new float3(pos.Value.xy, -8);
            transform.rotation = Quaternion.AngleAxis(pos.Value.z, new Vector3(0,0,1));
        }
    }
}
