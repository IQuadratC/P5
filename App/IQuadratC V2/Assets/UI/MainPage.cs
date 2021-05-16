using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class MainPage : MonoBehaviour
{
    [SerializeField] private GameObject HI;
    [SerializeField] private GameObject AI;
    [SerializeField] private GameObject sideMenue;
    [SerializeField] private GameObject sideMenueButton;

    private void OnEnable()
    {
        AI.SetActive(false);
        HI.SetActive(true);
        sideMenue.SetActive(false);
        sideMenueButton.SetActive(true);
    }

    public void setSideMenue(bool active)
    {
        sideMenue.SetActive(active);
        sideMenueButton.SetActive(!active);
    }

    [SerializeField] private Slider sliderHIAI;
    public void setHIAI()
    {
        float value = sliderHIAI.value;
        HI.SetActive(value <= 0.5f);
        AI.SetActive(value > 0.5f);
    }
}
