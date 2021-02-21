using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Utility.Events;
using Utility.Variables;

public class HIControler : MonoBehaviour
{
    [SerializeField]private float speedDirection;
    [SerializeField]private float minDirectionChange;
    [SerializeField]private float speedRotation;
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
            sendString.Value = "roboter rotate " + math.sign(frameRotation) * 180 + "," + (math.abs(frameRotation) * speedRotation);
            sendEvent.Raise();
            lastRotation = frameRotation;
        }
        else if (!lastRotation.Equals(0f) && frameRotation.Equals(0f))
        {
            sendString.Value = "stop";
            sendEvent.Raise();
            lastRotation = frameRotation;
            
        }
        else if (math.abs(math.length(frameDirection - lastDirection)) > minDirectionChange && !frameDirection.Equals(float2.zero))
        {
            sendString.Value = "move " + (int)(math.normalize(frameDirection).x * 1000) + "," + (int)(math.normalize(frameDirection).y * 1000) + "," + ((int)(math.length(frameDirection) * speedDirection)); 
            sendEvent.Raise(); 
            lastDirection = frameDirection;
            
        }
        else if (!lastDirection.Equals(float2.zero) && frameDirection.Equals(float2.zero))
        {
            sendString.Value = "stop";
            sendEvent.Raise();
            lastDirection = frameDirection;
        }
    }
}
