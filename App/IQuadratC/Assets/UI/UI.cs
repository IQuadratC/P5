using System;
using UnityEditor;
using UnityEngine;
using Utility.Events;

namespace UI
{
    
    public class UI : MonoBehaviour
    {
        [SerializeField] private GameObject[] pages;
        [SerializeField] private GameObject startPage;
        
        [SerializeField] public GameEvent appOpenEvent;
        [SerializeField] public GameEvent appCloseEvent;
        
        private void Start()
        {
            foreach (GameObject page in pages)
            {
                page.SetActive(false);
            }
            
            pages[0].SetActive(true);
            startPage.SetActive(true);

            appOpenEvent.Raise();
        }

        private int currentPage;
        public void SwitchPage(int page)
        {
            if(currentPage == page) return;
            
            pages[currentPage].SetActive(false);

            pages[page].SetActive(true);

            currentPage = page;
            
            startPage.SetActive(currentPage < 2);
        }

        public void CloseApp()
        {
            appCloseEvent.Raise();
            
            if(Application.platform == RuntimePlatform.WebGLPlayer) return;
            
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
