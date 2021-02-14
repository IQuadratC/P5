using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using Utility.Events;
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
    [SerializeField]private StringVariable sendString;
    [SerializeField]private GameEvent sendEvent;
    
    // Update is called once per frame
    void Update()
    {
        float frameRotation = rotation.Value;
        float2 frameDirection = direction.Value;
        if (math.abs(frameRotation - lastRotation) > minRotationChange)
        {
            sendString.Value = "rotate " + frameRotation;
            sendEvent.Raise();
            lastRotation = frameRotation;
        }
        else if (!lastRotation.Equals(0f) && frameRotation.Equals(0f))
        {
            sendString.Value = "rotate " + frameRotation;
            sendEvent.Raise();
            lastRotation = frameRotation;
        }else if (math.abs(math.length(frameDirection - lastDirection)) > minDirectionChange)
        {
            sendString.Value = "move " + frameDirection;
            sendEvent.Raise();
            lastDirection = frameDirection;
        }else if (!lastDirection.Equals(float2.zero) && frameDirection.Equals(float2.zero))
        {
            sendString.Value = "move " + frameDirection;
            sendEvent.Raise();
            lastDirection = frameDirection;
        }
    }
}
