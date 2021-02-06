using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetResolution : MonoBehaviour
{
    public Camera renderCamera;
    public RenderTexture texture;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        texture.height = renderCamera.pixelHeight;
        texture.width = renderCamera.pixelWidth - 400;
    }
}
