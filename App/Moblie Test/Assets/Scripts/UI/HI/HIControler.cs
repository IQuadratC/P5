using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Utility.Variables;

public class HIControler : MonoBehaviour
{
    [SerializeField]private float maxSpeedDirection;
    [SerializeField]private float speedDirection;
    [SerializeField]private float minDirectionChange;
    [SerializeField]private float maxSpeedRotation;
    [SerializeField]private float minRotationChange;
    private float2 lastDirection;
    private float lastRotation;
    [SerializeField]private Vec2Variable direction;
    [SerializeField]private FloatVariable rotation;
    
    // Update is called once per frame
    void Update()
    {
        float frameRotation = rotation.Value;
        float2 frameDirection = direction.Value;
        if (math.abs(frameRotation - lastRotation) > minRotationChange)
        {
            print(frameRotation);
            lastRotation = frameRotation;
        }
        else if (!lastRotation.Equals(0f) && frameRotation.Equals(0f))
        {
            print(frameRotation);
            lastRotation = frameRotation;
        }else if (math.abs(math.length(frameDirection - lastDirection)) > minDirectionChange)
        {
            print(frameDirection);
            lastDirection = frameDirection;
        }else if (!lastDirection.Equals(float2.zero) && frameDirection.Equals(float2.zero))
        {
            print(frameDirection);
            lastDirection = frameDirection;
        }
    }
}
