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
    [SerializeField] private Transform basis;
    [SerializeField] private Vec2Variable direction;
    [SerializeField] private Camera cam;
    [SerializeField] private float maxDistance;
    
    private bool pressed;
    
    public void OnPointerDown(PointerEventData eventData){
        pressed = true;
    }
     
    public void OnPointerUp(PointerEventData eventData){
        pressed = false;
    }

    public void Update()
    {
        if (pressed)
        {
            float3 point;
            if (Input.touches.Length > 0)
            {
                point = cam.ScreenToWorldPoint(Input.GetTouch(0).position);
            }
            else
            {
                point = cam.ScreenToWorldPoint(Input.mousePosition);
            }
            
            // format point to maxDistance 
            if (math.length(point.xy - ((float3) basis.position).xy) > maxDistance)
            {
                direction.Value = new float2((math.normalize(point.xy)));
                stick.position = new float3((math.normalize(point.xy) * maxDistance), basis.position.z);
            }
            else
            {
                direction.Value = point.xy / maxDistance;
                stick.position = new float3(point.x,point.y,basis.position.z);
            }
            
        }
        else
        {
            direction.Value = float2.zero;
            stick.localPosition = float3.zero;
        }
    }
}
