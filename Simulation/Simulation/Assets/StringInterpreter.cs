using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class  StringInterpreter : MonoBehaviour
{
    [SerializeField]private LIDAR lidar;

    private List<String[]> Actions = new List<string[]>();

    [SerializeField] private float speed;
    private Vector2 move = Vector2.zero;
    private void FixedUpdate()
    {
        if (Actions.Count == 0)
        {
            return;
        }
        
        if (Actions[0][0] == "move")
        {
            Vector2 goal = new Vector2(int.Parse(Actions[0][1].Split(',')[1]),int.Parse(Actions[0][1].Split(',')[0])) * 10;
            Vector2 direction = goal.normalized;
            Vector2 delta = direction * (speed * Time.deltaTime);
            
            if ((move + delta).magnitude > goal.magnitude)
            {
                Move(goal - move);
                move = Vector2.zero;
                Actions.RemoveAt(0);
                return;
            }
            
            Move(delta);
            move += delta;
        }
        else if (Actions[0][0] == "rotate")
        {
            PassRotation(Actions[0][1]);
            Actions.RemoveAt(0);
        }
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
    
    private void PassRotation(String input)
    {
        float rot = (int.Parse(input.Split(',')[0]));
        Rotate(rot);
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
                Actions.Add(new String[] {"move", (input.Split(' ')[i + 1])});
            }
            else if (input.Split(' ')[i] == "rotate")
            {
                Actions.Add(new String[] {"rotate", (input.Split(' ')[i + 1])});
            }
            else if (input.Split(' ')[i] == "multi")
            {
                PassMulti(input.Split(' ')[i + 1]);
            }
        }
        Debug.Log(Actions.Count);
    }


    [SerializeField]public Rigidbody rigidbody;
    Vector3 pos = new Vector3(0, 0.5f ,0);

    private Vector3 rot = new Vector3(0, 0, 0);
    public void Move(Vector2 move)
    {
        pos += rigidbody.rotation * new Vector3(move.x , 0 , move.y);
        rigidbody.MovePosition(pos);
    }

    public void Rotate(float rotate)
    {
        rot += new Vector3(0 , rotate , 0);
        rigidbody.MoveRotation(Quaternion.Euler(rot));
    }

}
