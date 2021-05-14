using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    static GameManager instance;
    private void Awake()
    {
        instance = this;
    }

    public GameObject[] views;
    public GameObject currentView;
    [SerializeField] private int startViewIndex;

    private void Start()
    {
        foreach (GameObject view in views)
        {
            view.SetActive(false);
        }
        views[startViewIndex].SetActive(true);
    }

    public void switchView(GameObject view)
    {
        currentView.gameObject.SetActive(false);
        view.gameObject.SetActive(true);
        currentView = view;
    }
}
