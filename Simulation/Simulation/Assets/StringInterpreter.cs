using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class  StringInterpreter : MonoBehaviour
{
    [FormerlySerializedAs("script")] [SerializeField]private Movment movement;
    [SerializeField]private LIDAR lidar;

    private List<String[]> Actions = new List<string[]>();
    private void FixedUpdate()
    {
        if (Actions.Count == 0)
        {
            return;
        }
        
        if (Actions[0][0] == "move")
        {
            PassMove(Actions[0][1]);
        }
        else if (Actions[0][0] == "rotate")
        {
            PassRotation(Actions[0][1]);
        }
        
        Actions.RemoveAt(0);
    }

    public void PassString(String input)
    { 
        Actions = new List<string[]>();
        
        if (input.Split(' ') [0] == "roboter")
        {
            if (input.Split(' ')[1] == "move")
            {
                Actions.Add(new  String[] {"move" , (input.Split(' ')[2])});
            }
            else if (input.Split(' ')[1] == "rotate")
            {
                Actions.Add(new  String[] {"rotate" , (input.Split(' ')[2])});
            }
            else if (input.Split(' ')[1] == "multi")
            {
                PassMulti(input.Split(' ')[2]);
            }
        }
        else if (input.Split(' ') [0] == "lidar")
        {
           lidar.LidarPunkte();
        }
    }

    private void PassMove(String input)
    {
        Vector2 pos = new Vector2(int.Parse(input.Split(',')[1]),int.Parse(input.Split(',')[0]));
        movement.Move(pos);
    }
    private void PassRotation(String input)
    {
        float rot = (int.Parse(input.Split(',')[0]));
        movement.Rotate(rot);
    }
    private void PassMulti(String input)
    {
        input = input.Replace(',', ' ');
        input = input.Replace(';', ',');
        input = input.Replace(':', ';');
        input = input.Replace('|', ':');

        for (int i = 0; i < input.Split(' ').Length - 1; i += 2)
        {
            if (input.Split(' ')[i] == "move")
            {
                Actions.Add(new  String[] {"move" , (input.Split(' ')[i + 1])});
            }
            else if (input.Split(' ')[i] == "rotate")
            {
                Actions.Add(new  String[] {"rotate" , (input.Split(' ')[i + 1])});
            }
            else if (input.Split(' ')[i] == "multi")
            {
                PassMulti(input.Split(' ')[i + 1]);
            }
        }
        
        
    }
    
}
