using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility.Variables;

public class FakeTCP : MonoBehaviour
{
    [SerializeField]private StringVariable msg;
    public void sendMsg()
    {
        Debug.Log(msg.Value);
    }
}
