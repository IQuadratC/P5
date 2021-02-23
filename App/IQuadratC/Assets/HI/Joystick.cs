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
    [SerializeField] private Camera cam;
    [SerializeField] private float maxDistance;
    
    private bool pressed;
    private float2 lastPos;
    
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
            float2 change;
            float2 newPos;
            if (Input.touchCount > 0)
            {
                change = Input.touches[0].deltaPosition;
                newPos = new float2(stick.localPosition.x + change.x, stick.localPosition.y + change.y);;

            }
            else
            {
                float2 pos = new float2(Input.mousePosition.x, Input.mousePosition.y);
                change = (lastPos - pos);;
                lastPos = pos;
                newPos = new float2(stick.localPosition.x - change.x, stick.localPosition.y - change.y);;

            }


            // format point to maxDistance 
            if (math.length(newPos.xy) > maxDistance)
            {
                direction.Value = math.normalize(newPos);
                stick.localPosition = new float3(math.normalize(newPos), 0) * maxDistance;
            }
            else
            {
                direction.Value = (newPos) / maxDistance;
                stick.localPosition = new float3(newPos, 0);
            }
        }
        else
        {
            direction.Value = float2.zero;
            stick.localPosition = Vector3.zero;
        }
    }
}
