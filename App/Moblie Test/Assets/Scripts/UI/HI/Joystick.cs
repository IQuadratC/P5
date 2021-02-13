using System;
using UnityEngine;
using System.Collections;
using Unity.Mathematics;
using UnityEngine.EventSystems;
using Utility.Events;
using Utility.Variables;

public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    
    [SerializeField]private Transform stick;

    [SerializeField]private Transform basis;

    [SerializeField]private Vec2Variable direction;
    
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
            direction.Value = Input.GetTouch(0).deltaPosition;
            Vector2 touch = Input.GetTouch(0).position;
            touch = Camera.main.ScreenToWorldPoint(touch);
            stick.position = ((Vector3)touch);
        }
        else
        {
            direction.Value = float2.zero;
            stick.position = basis.position;
        }
    }
}
