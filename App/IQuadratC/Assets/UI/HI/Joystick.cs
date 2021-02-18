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

            stick.localPosition = new float3(point.x,point.y,0);
            direction.Value = new float2(basis.position.x - point.x, basis.position.y - point.y);
        }
        else
        {
            direction.Value = float2.zero;
            stick.localPosition = float3.zero;
        }
    }
}
