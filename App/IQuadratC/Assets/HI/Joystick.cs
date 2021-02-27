using System;
using UnityEngine;
using System.Collections;
using Unity.Mathematics;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utility.Events;
using Utility.Variables;

public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    
    [SerializeField] private Transform stick;
    [SerializeField] private Vec2Variable direction;
    [SerializeField] private float maxDistance;
    
    private bool pressed;
    private float2 lastPos;
    private float2 fingerPos;
    
    public void OnPointerDown(PointerEventData eventData){
        lastPos = float2.zero;
        pressed = true;
    }
     
    public void OnPointerUp(PointerEventData eventData){
        pressed = false;
    }

    public void Update()
    {
        if (pressed)
        {
            // calculate finger position
            if (Input.touchCount > 0)
            {
                fingerPos += (float2)Input.touches[0].deltaPosition;
            }
            else
            {
                float3 pos = Input.mousePosition;
                fingerPos += (lastPos - pos.xy);;
                lastPos = pos.xy;
            }


            // format point to maxDistance 
            if (math.length(fingerPos.xy) > maxDistance)
            {
                direction.Value = math.normalize(fingerPos);
                stick.localPosition = new float3(math.normalize(fingerPos), 0) * maxDistance;
            }
            else
            {
                direction.Value = (fingerPos) / maxDistance;
                stick.localPosition = new float3(fingerPos, 0);
            }
        }
        else
        {
            // resets the stick position
            direction.Value = float2.zero;
            stick.localPosition = Vector3.zero;
            fingerPos = float2.zero;
        }
    }
}
