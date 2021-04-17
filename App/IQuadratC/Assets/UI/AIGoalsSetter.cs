using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using Utility.Variables;

public class AIGoalsSetter : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    [SerializeField] private Camera cam2D;
    [SerializeField] private CinemachineVirtualCamera cam;
    [SerializeField] private GameObject AI;
    [SerializeField] private GameObject Map;

    [SerializeField] private GameObject goalPreFab;
    private List<GameObject> goals;
    [SerializeField] private float goalClickRadius = 10;
    [SerializeField] private float actionTime = 1;

    [SerializeField] private Int2ListVariable AIGoals;

    private void Awake()
    {
        goals = new List<GameObject>();
    }
    
    float3 touchStart;
    private float3 screenPosStart;
    private bool pressed;
    private GameObject selectedGoal;
    private float startTime;
    public void OnPointerDown (PointerEventData data)
    {
        screenPosStart = Input.mousePosition;
        touchStart = cam2D.ScreenToWorldPoint(screenPosStart);
        selectedGoal = findGoal(touchStart);
        startTime = Time.time;
        pressed = true;
    }

    private GameObject findGoal(float3 pos)
    {
        GameObject match = null;
        foreach (GameObject goal in goals)
        {
            if (math.distance(((float3)goal.transform.position).xy, pos.xy) < goalClickRadius)
            {
                match = goal;
            }
        }
        return match;
    }
 
    public void OnPointerUp (PointerEventData data)
    {
        pressed = false;
    }
    
    void Update () {
        if (pressed && AI.activeSelf)
        {
            if (Time.time - startTime > actionTime)
            {
                pressed = false;
                float3 screenNow = Input.mousePosition;
                if (math.distance(screenNow, screenPosStart) > goalClickRadius) { return; }
                
                if (selectedGoal != null)
                {
                    AIGoals.Value.Remove(new int2((int) goals[0].transform.position.x,
                        (int) goals[0].transform.position.y));
                    goals.Remove(selectedGoal);
                    Destroy(selectedGoal);
                }
                else
                {
                    GameObject goal = Instantiate(goalPreFab, touchStart, Quaternion.identity);
                    goal.transform.SetParent(Map.transform);
                    goal.transform.position = new Vector3(goal.transform.position.x, goal.transform.position.y, 0);
                    goals.Add(goal);
                    AIGoals.Value.Add(new int2((int) goal.transform.position.x, (int) goal.transform.position.y));
                }
            }
        }

        if (AIGoals.Value.Count == 0 && goals.Count != 0)
        {
            GameObject goal = goals[0];
            Destroy(goal);
            goals.Remove(goal);
        }
        else if (goals.Count == 0 || !AI.activeSelf)
        {
            AIGoals.Value = new List<int2>();
        }
        else if (!AIGoals.Value[0].Equals(
            new int2((int) goals[0].transform.position.x, (int) goals[0].transform.position.y)))
        {
            GameObject goal = goals[0];
            Destroy(goal);
            goals.Remove(goal);
        }

    }
}
