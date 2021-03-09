using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utility.Events;
using Utility.Variables;

public class SliderControler : MonoBehaviour, IPointerUpHandler
{
    [SerializeField]private Slider mainSlider;
    [SerializeField]private FloatVariable rotation;
    [SerializeField] private StringVariable logMessage;
    [SerializeField] private GameEvent logEvent;
    
    public void OnPointerDown(PointerEventData eventData)
    {
        logMessage.Value = "Graped Slider";
        logEvent.Raise();
    }
    public void OnPointerUp(PointerEventData eventData){
        // resets the slider position
        mainSlider.value = 0;
        rotation.Value = 0;
        logMessage.Value = "Released Slider";
        logEvent.Raise();
    }
    
    
}
