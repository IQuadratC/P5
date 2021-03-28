using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class ButtonMove : MonoBehaviour
{
    [SerializeField]private Movment script;
    public void MoveRight()
    {
        script.Move(Vector2.right);
    }
}
