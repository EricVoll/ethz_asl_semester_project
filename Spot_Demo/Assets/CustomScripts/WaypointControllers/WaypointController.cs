using Microsoft.MixedReality.Toolkit;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WaypointControl.Waypoint;
using WaypointControl.Waypoint.Interfaces;

public class WaypointController : MonoBehaviour
{
    MissionController missionController;

    [HideInInspector]
    public IWaypoint waypoint { get; private set; }

    [SerializeField]
    private GameObject Menu;
    [SerializeField]
    private Material CurrentWaypointMaterial;
    [SerializeField]
    private Material NonCurrentWaypointMaterial;
    [SerializeField]
    private GameObject EdgeCreationTargetPrefab;

    [SerializeField]
    private float maxDinstanceFromAnchorCenter = 10f;

    public bool isDestroyRequested = false;

    private void Awake()
    {

    }
    public void Start()
    {

    }

    public void Init(IWaypoint wp, MissionController missionController, bool addUXTasks = true)
    {
        if (wp != null)
        {
            waypoint = wp;
            this.transform.localPosition = waypoint.Pose.ToPositionVector();
        }
        if (addUXTasks)
        {
            wp.AddTargetSetTask(new ColorChangeTask(this.gameObject, CurrentWaypointMaterial));
            wp.AddExitTask(new ColorChangeTask(this.gameObject, NonCurrentWaypointMaterial));
        }

        this.missionController = missionController;
    }

    public void Update()
    {
        if (waypoint == null) return;

        waypoint.Pose.SetFromTransform(this.transform);

        //Check if current anchor is still within the heuristics
        if (Math.Sqrt(waypoint.Pose.values.Take(3).Select(x => x*x).Sum()) > maxDinstanceFromAnchorCenter)
        {
            //we have to change the anchor.
            //Look for nearby anchors
            (SpatialAnchorController closest, bool isWithinMaxDistance) = missionController.FindAnchorNearMe(this.transform, maxDinstanceFromAnchorCenter);

            SpatialAnchorController futureParentController = null;

            if (closest == null || !isWithinMaxDistance)
            {
                //if none are found -> create new one
                RobotUtilities.Pose pose = new RobotUtilities.Pose();
                Vector3 displacement = (this.transform.position - closest.transform.position).normalized * maxDinstanceFromAnchorCenter * 1.5f;
                displacement.y = 0;
                Vector3 relativePosToParent = closest.transform.InverseTransformPoint(closest.transform.position + displacement);
                futureParentController = missionController.CreateAnchor((RobotUtilities.Pose)relativePosToParent.ToIPose<RobotUtilities.Pose>(), closest.GetAnchor());
            }
            else
            {
                futureParentController = closest;
            }


            futureParentController.SetAnchorAsParent(this.gameObject, true);
            this.waypoint.Pose.SetFromTransform(transform);
            this.waypoint.SetPose(waypoint.Pose, futureParentController.GetAnchor());
        }
    }

    #region Btns

    /// <summary>
    /// Requests the missionController to insert a node after this one
    /// </summary>
    public void InsertRight()
    {
        missionController.InsertChild(this);
        StartCoroutine(CloseAfterSeconds(Menu, .1f));
    }

    /// <summary>
    /// Requests the missionController to delete this node.
    /// </summary>
    public void Delete()
    {
        //Has to be executed from ission controller, since the thread this function call is running on will die later
        //while deleting the gameobject, which results in the coroutine not finishing.
        missionController.ExecuteOnUpdate(() => StartCoroutine(missionController.RemoveWaypoint(this)));

        StartCoroutine(CloseAfterSeconds(Menu, .1f));
    }

    public void AddEdge()
    {
        var go = Instantiate(EdgeCreationTargetPrefab, transform);
        go.GetComponent<EdgeCreationCursor>().Init(missionController, waypoint);
    }

    #endregion

    #region Events for input to open/close the Menu

    float exitedFocusAt;
    float enteredFocusAt;
    float clickedAt;

    public void OnHoverExit()
    {
        CloseAfterSeconds(Menu, 10);
    }

    public void OnClick()
    {
        clickedAt = Time.realtimeSinceStartup;
        Debug.Log($"Setting Menu status to {!Menu.activeSelf}");
        Menu.SetActive(!Menu.activeSelf);
        CloseAfterSeconds(Menu, 40);
    }

    /// <summary>
    /// Waits for the given amount of time and then checks if the user is still focusing on the waypoint
    /// if yes, then the menu is opened -> avoids unwanted menu openings
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="seconds"></param>
    /// <returns></returns>
    private IEnumerator OpenAfterSeconds(GameObject obj, float seconds)
    {
        yield return new WaitForSeconds(seconds * 1.1f);

        if (Mathf.Abs(exitedFocusAt - enteredFocusAt) < seconds)
        {
            //Do not show, user already unfocused the object
        }
        else
        {
            closeTime = -1; //cancel all close operations
            Menu.SetActive(true);
            //this.gameObject.AddComponent<FadeBase>().FadeIn(Menu);
            StartCoroutine(CloseAfterSeconds(Menu, 6));
        }
    }

    float closeTime = -1;
    private IEnumerator CloseAfterSeconds(GameObject obj, float seconds)
    {
        closeTime = Time.realtimeSinceStartup;
        float myCloseTime = closeTime;
        yield return new WaitForSeconds(seconds);
        if (closeTime == myCloseTime)
        {
            Menu.SetActive(false);
            //this.gameObject.AddComponent<FadeBase>().FadeOut(Menu);
        }
    }



    #endregion



}
