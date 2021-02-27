using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utility.Variables;

public class SliderControler : MonoBehaviour, IPointerUpHandler
{
    [SerializeField]private Slider mainSlider;
    [SerializeField]private FloatVariable rotation;
    
    public void OnPointerUp(PointerEventData eventData){
        // resets the slider position
        mainSlider.value = 0;
        rotation.Value = 0;
    }
    
    
}
