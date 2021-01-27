using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ButtonScript : MonoBehaviour
{
    public string message;
        int n;
        public void OnButtonPress()
        {
            n++;
            Debug.Log("Button clicked " + n + " times.");
            StartCoroutine(Upload(message));
        }
        
        IEnumerator Upload(string msg)
        {
            UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1:5000/" + msg, "");
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                Debug.Log(www.error);
            }
            else
            {
                Debug.Log("Form upload complete!");
            }
        }
}
