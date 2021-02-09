using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetResolution : MonoBehaviour
{
    public Camera renderCamera;
    public RenderTexture texture;
    
    void Update()
    {
        texture.height = renderCamera.pixelHeight;
        texture.width = renderCamera.pixelWidth - 400;
    }
}
