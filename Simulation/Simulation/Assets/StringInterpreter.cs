using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class  StringInterpreter : MonoBehaviour
{
    [SerializeField]private Movment script;
    
    public void PassString(String input)
    {
        if (input.Split(' ') [0] == "roboter")
        {
            if (input.Split(' ')[1] == "move")
            {
                PassMove(input.Split(' ')[2]);
            }
        }
    }

    private void PassMove(String input)
    {
        Vector2 pos = new Vector2(int.Parse(input.Split(' ')[1]),int.Parse(input.Split(' ')[0]));
        script.Move(pos);
    }
}
