using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class LIDAR : MonoBehaviour
{
    [SerializeField] private int messPunkte;
    private List<String[]> lidar;

    public void LidarPunkte()
    {
        // Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1 << 8;

        // This would cast rays only against colliders in layer 8.
        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
        layerMask = ~layerMask;

        RaycastHit hit;
        lidar = new List<string[]>();
        for (int i = 0; i < messPunkte; i++)
        {
            if (Physics.Raycast(transform.position, transform.TransformDirection(Quaternion.Euler(0,i * 360 / messPunkte,0) * Vector3.forward), out hit, Mathf.Infinity, layerMask))
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Quaternion.Euler(0,i * 360 / messPunkte,0) * Vector3.forward) * hit.distance, Color.yellow);
                lidar.Add(new string[]{hit.distance.ToString("0.00", CultureInfo.InvariantCulture) , (i * 360 / messPunkte).ToString("0.00", CultureInfo.InvariantCulture)});
            }
            else
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Quaternion.Euler(0,i * 360 / messPunkte,0) * Vector3.forward) * 1000, Color.green);
                Debug.Log("Did not Hit");
            }
        }

        String message  = "lidarmap data ";

        for (int i = 0; i < lidar.Count; i++)
        {
            if (i > 0 && i % 50 == 0)
            {
                Debug.Log(message);
                message  = "lidarmap data ";
            }
            message += lidar[i][1] + ";"; 
            message += lidar [i][0] + ",";
        }
        Debug.Log(message);
    }

    private void OnDrawGizmos()
    {
        // Bit shift the index of the layer (8) to get a bit mask
        int layerMask = 1 << 8;

        // This would cast rays only against colliders in layer 8.
        // But instead we want to collide against everything except layer 8. The ~ operator does this, it inverts a bitmask.
        layerMask = ~layerMask;

        RaycastHit hit;
        lidar = new List<string[]>();
        for (int i = 0; i < messPunkte; i++)
        {
            if (Physics.Raycast(transform.position, transform.TransformDirection(Quaternion.Euler(0,i * 360 / messPunkte,0) * Vector3.forward), out hit, Mathf.Infinity, layerMask))
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Quaternion.Euler(0,i * 360 / messPunkte,0) * Vector3.forward) * hit.distance, Color.yellow);
            }
            else
            {
                Debug.DrawRay(transform.position, transform.TransformDirection(Quaternion.Euler(0,i * 360 / messPunkte,0) * Vector3.forward) * 1000, Color.green);
            }
        }
    }

    
}

