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

        private void Start()
        {
            foreach (GameObject page in pages)
            {
                page.SetActive(false);
            }
            
            pages[0].SetActive(true);
            startPage.SetActive(true);
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
