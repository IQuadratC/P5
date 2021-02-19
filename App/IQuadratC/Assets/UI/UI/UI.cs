using System;
using UnityEditor;
using UnityEngine;
using Utility.Events;

namespace UI
{
    
    public class UI : MonoBehaviour
    {
        [SerializeField] private GameObject[] pages;
        [SerializeField] private GameObject[] cams;
        
        [SerializeField] public GameEvent appOpenEvent;
        [SerializeField] public GameEvent appCloseEvent;
        
        private void Start()
        {
            foreach (GameObject page in pages)
            {
                page.SetActive(false);
            }
            foreach (GameObject cam in cams)
            {
                cam.SetActive(false);
            }
            pages[0].SetActive(true);
            cams[0].SetActive(true);
            
            appOpenEvent.Raise();
        }

        private int currentPage;
        public void SwitchPage(int page)
        {
            if(currentPage == page) return;
            
            pages[currentPage].SetActive(false);
            cams[currentPage].SetActive(false);
            
            pages[page].SetActive(true);
            cams[page].SetActive(true);

            currentPage = page;
            
            
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
