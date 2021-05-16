using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoloCam : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    
    [SerializeField] private CinemachineFreeLook freeLookComponent;
    [SerializeField] private float2 mouseSpeed;
    [SerializeField] private float2 touchSpeed;
    
    private bool pressed;
    public void OnPointerDown (PointerEventData data)
    {
        pressed = true;
    }

    public void OnPointerUp (PointerEventData data)
    {
        pressed = false;
    }

    private float2 lastPos;
    void Update () {
        
        float2 movement = float2.zero;
        if (pressed)
        {
            if (Input.touchCount > 0)
            {
                movement = -Input.touches[0].deltaPosition;
                movement *= touchSpeed;
            }
            else
            {
                float2 pos = new float2(Input.mousePosition.x, Input.mousePosition.y);
                movement = (lastPos - pos) * mouseSpeed;
                lastPos = pos;
            }
        }
        
        freeLookComponent.m_XAxis.m_InputAxisValue = movement.x;
        freeLookComponent.m_YAxis.m_InputAxisValue = movement.y;
        
    }
    
}
