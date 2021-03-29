using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LIDAR : MonoBehaviour
{
    [SerializeField] private int messPunkte;
    

    public void LidarPunkte()
    {
        // Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1 << 8;

        // This would cast rays only against colliders in layer 8.
        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
        layerMask = ~layerMask;

        RaycastHit hit;

        for (int i = 0; i < messPunkte; i++)
        {
            if (Physics.Raycast(transform.position, transform.TransformDirection(Quaternion.Euler(0,i * 360 / messPunkte,0) * Vector3.forward), out hit, Mathf.Infinity, layerMask))
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Quaternion.Euler(0,i * 360 / messPunkte,0) * Vector3.forward) * hit.distance, Color.yellow);
                Debug.Log(hit.distance);
            }
            else
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Quaternion.Euler(0,i * 360 / messPunkte,0) * Vector3.forward) * 1000, Color.green);
                Debug.Log("Did not Hit");
            }
        }
        
    }

}

