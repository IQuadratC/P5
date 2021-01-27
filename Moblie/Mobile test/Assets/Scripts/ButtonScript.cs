using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ButtonScript : MonoBehaviour
{
        int n;
        public void OnButtonPress()
        {
            n++;
            Debug.Log("Button clicked " + n + " times.");
            StartCoroutine(Upload());
        }
        
        IEnumerator Upload()
        {
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
            formData.Add(new MultipartFormDataSection("field1=foo&field2=bar"));

            UnityWebRequest www = UnityWebRequest.Post("http://127.0.0.1:5000/", formData);
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
