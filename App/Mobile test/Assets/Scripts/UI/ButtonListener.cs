using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using Utility.Events;

public class ButtonListener : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    public GameEvent Down;
    public GameEvent Up;
    
    public void OnPointerDown(PointerEventData eventData){
        Down.Raise();
    }
     
    public void OnPointerUp(PointerEventData eventData){
        Up.Raise();
    }
}
