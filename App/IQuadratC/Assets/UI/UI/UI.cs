using System;
using UnityEditor;
using UnityEngine;
using Utility.Events;

namespace UI
{
    public class UI : MonoBehaviour
    {
        [SerializeField] private GameObject startPage;
        [SerializeField] private GameObject settingsPage;
        [SerializeField] private GameObject mainPage;

        [SerializeField] public GameEvent appOpenEvent;
        [SerializeField] public GameEvent appCloseEvent;

        private void Start()
        {
            appOpenEvent.Raise();
            SwitchPage(startPage);
        }

        public void SwitchPage(GameObject page)
        {
            startPage.SetActive(false);
            settingsPage.SetActive(false);
            mainPage.SetActive(false);
            
            if (page == startPage)
            {
                startPage.SetActive(true);
            }
            else if (page == settingsPage)
            {
                settingsPage.SetActive(true);
            }
            else if (page == mainPage)
            {
                mainPage.SetActive(true);
            }
        }

        public void CloseApp()
        {
            appCloseEvent.Raise();
            Application.Quit();
            
            #if UNITY_EDITOR
                if(EditorApplication.isPlaying) 
                {
                    EditorApplication.isPlaying = false;
                }
            #endif
        }

    }
}
