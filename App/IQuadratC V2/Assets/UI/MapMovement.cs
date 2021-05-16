// source https://pressstart.vip/tutorials/2018/07/12/44/pan--zoom.html

using System;
using Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class MapMovement : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        [SerializeField] private Camera cam2D;
        [SerializeField] private CinemachineVirtualCamera cam;
        [SerializeField] private Transform camFollow;
        
        [SerializeField] private float scrollSpeed = 10;
        [SerializeField] private float touchSpeed = 10;
        [SerializeField] private float zoomOutMin = 10;
        [SerializeField] private float zoomOutMax = 100;

        private void OnEnable()
        {
            cam.transform.position = new float3(0,0,-100);
            cam.m_Lens.OrthographicSize = 100;
        }

        float3 touchStart;
        private bool pressed;
        public void OnPointerDown (PointerEventData data)
        {
            pressed = true;
        }
 
        public void OnPointerUp (PointerEventData data)
        {
            pressed = false;
        }

        void Update () {
            if (pressed)
            {
                if(Input.GetMouseButtonDown(0)){
                    touchStart = cam2D.ScreenToWorldPoint(Input.mousePosition);
                }
                if(Input.touchCount == 2){
                    Touch touchZero = Input.GetTouch(0);
                    Touch touchOne = Input.GetTouch(1);

                    float2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                    float2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                    float prevMagnitude = math.length(touchZeroPrevPos - touchOnePrevPos);
                    float currentMagnitude = math.length(touchZero.position - touchOne.position);

                    float difference = currentMagnitude - prevMagnitude;

                    Zoom(difference * touchSpeed);
                }else if(Input.GetMouseButton(0)){
                    float3 direction = touchStart - (float3) cam2D.ScreenToWorldPoint(Input.mousePosition);
                    camFollow.position += (Vector3) direction;
                }
            }
            Zoom(Input.GetAxis("Mouse ScrollWheel") * scrollSpeed);
        }

        void Zoom(float increment){
            cam.m_Lens.OrthographicSize = math.clamp(cam.m_Lens.OrthographicSize - increment, zoomOutMin, zoomOutMax);
        }
    }
}
