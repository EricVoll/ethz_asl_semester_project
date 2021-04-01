using Microsoft.MixedReality.Toolkit.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using WaypointControl.Task;
using WaypointControl.Task.Interfaces;
using WaypointControl.Waypoint.Interfaces;

public class BranchingDecider : MonoBehaviour
{

    [Tooltip("The gameobject used to indicate the branching directions")]
    [SerializeField]
    GameObject ArrowPrefab;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Action<IBranchDecisionResult> AnswerCallBack;

    List<GameObject> Arrows = new List<GameObject>();

    public void Decide(Action<IBranchDecisionResult> answer, List<WaypointController> possibleTargetWaypoints, WaypointController sourceWaypoint)
    {
        AnswerCallBack = answer;

        foreach (var target in possibleTargetWaypoints)
        {
            GameObject arrow = Instantiate(ArrowPrefab, this.transform);
            arrow.GetComponent<Interactable>().OnClick.AddListener(() => ArrowClicked(target.waypoint));
            Arrows.Add(arrow);

            //This double-setting of the position allows to easily rotate the arrow towards its target
            //Since this code is called in an update method from somewhere, setting it twice shouldn't impact
            //The performance too much, since neither graphics are rendered, nor physics simulated during the update method
            arrow.transform.position = sourceWaypoint.transform.position;
            arrow.transform.LookAt(target.transform);
            arrow.transform.position = sourceWaypoint.transform.position + (target.transform.position - sourceWaypoint.transform.position).normalized * 0.8f + Vector3.up * 0.2f;
        }
    }

    public void ArrowClicked(IWaypoint targetWaypoint)
    {
        IBranchDecisionResult result = new BranchDecisionResult(targetWaypoint);
        AnswerCallBack(result);
        AnswerCallBack = null;

        for (int i = Arrows.Count - 1; i >= 0; i--)
        {
            GameObject.Destroy(Arrows[i]);
        }
    }
}
