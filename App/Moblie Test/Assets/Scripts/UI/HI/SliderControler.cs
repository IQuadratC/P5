using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Utility.Variables;

public class SliderControler : MonoBehaviour
{
    [SerializeField]private Slider mainSlider;
    [SerializeField]private FloatVariable rotation;
    public void Update()
    {
        if (Input.touchCount == 0)
        {
            mainSlider.value = 0;
            rotation.Value = 0;
        }
    }
}
