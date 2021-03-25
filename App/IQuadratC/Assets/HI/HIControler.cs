using Unity.Mathematics;
using UnityEngine;
using Utility.Events;
using Utility.Variables;

namespace HI
{
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

        [SerializeField] private float sendIntervall;
        private float lastRotationMessage;
        private float lastMoveMessage;

        // Update is called once per frame
        void Update()
        {
            float frameRotation = rotation.Value;
            float2 frameDirection = direction.Value;
            // send new message when the rotation or direction Value has changed 
            if (math.abs(frameRotation - lastRotation) > minRotationChange)
            {
                SendRotate(frameRotation);
                lastRotation = frameRotation;
            }
            else if (!lastRotation.Equals(0f) && frameRotation.Equals(0f))
            {
                sendString.Value = "roboter stop";
                sendEvent.Raise();
                
            }
            else if (!frameRotation.Equals(0f) && Time.time - lastRotationMessage > sendIntervall)
            {
                SendRotate(frameRotation);
                lastRotationMessage = Time.time;
            }
            else if (math.abs(math.length(frameDirection - lastDirection)) > minDirectionChange && !frameDirection.Equals(float2.zero))
            {
                SendMove(frameDirection);
                lastDirection = frameDirection;
            }
            else if (!lastDirection.Equals(float2.zero) && frameDirection.Equals(float2.zero))
            {
                sendString.Value = "roboter stop";
                sendEvent.Raise();
            }
            else if (!frameDirection.Equals(float2.zero) && Time.time - lastMoveMessage > sendIntervall)
            {
                SendMove(frameRotation);
                lastMoveMessage = Time.time;
            }
            
            lastRotation = frameRotation;
            lastDirection = frameDirection;
        }
        
        private void SendRotate(float rotate)
        {
            sendString.Value = "roboter rotate " + 
                               math.sign(rotate) * 180 + "," + 
                               (int)(math.abs(rotate) * speedRotation);
            sendEvent.Raise();
        }
        
        private void SendMove(float2 frameDirection)
        {
            sendString.Value = "roboter move " + (int) (math.normalize(frameDirection).y * 1000) + "," +
                               (int) (math.normalize(frameDirection).x * 1000) + "," +
                               ((int) (math.length(frameDirection) * speedDirection));
            sendEvent.Raise();
        }
    }
}
