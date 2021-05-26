using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Utility.Variables;

public class Log : MonoBehaviour
{
    [SerializeField] private GameObject content;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private GameObject logItemPreFab;
    [SerializeField] private StringVariable logMessage;
    [SerializeField] private int maxMessageAmmount = 30;
    
    private List<GameObject> messages;

    private void OnEnable()
    {
        messages = new List<GameObject>();
    }

    public void PlaceLog()
    {
        GameObject gameObject = Instantiate(logItemPreFab, content.transform);
        gameObject.GetComponent<TMP_Text>().text = logMessage.Value;
        messages.Add(gameObject);
        
        Debug.Log(logMessage.Value);

        DeleteMessages(maxMessageAmmount);
    }

    private void DeleteMessages(int till)
    {
        while (messages.Count > till)
        {
            GameObject gameObject = messages[0];
            messages.RemoveAt(0);
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        scrollRect.verticalNormalizedPosition = 0;
    }

    private void OnDisable()
    {
        DeleteMessages(0);
    }
}
