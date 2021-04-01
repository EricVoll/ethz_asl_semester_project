using Microsoft.Azure.SpatialAnchors.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WaypointControl.Waypoint.Interfaces;

public class EdgeCreationCursor : MonoBehaviour, IHaveColliderNotifyer
{
    LineRenderer renderer;

    MissionController missionRef;
    WaypointControl.Waypoint.Interfaces.IWaypoint source;

    public void Init(MissionController controller, WaypointControl.Waypoint.Interfaces.IWaypoint Source)
    {
        missionRef = controller;
        source = Source;
    }

    public void Start()
    {
        this.renderer = GetComponent<LineRenderer>();
    }

    public void Update()
    {
        if(renderer != null)
        {
            renderer.positionCount = 2;
            renderer.SetPositions(new Vector3[]
            {
                this.transform.position,
                this.transform.parent.position
            }) ;
        }
    }


    public void NotifyCollision(Collision collision)
    {
        StartCoroutine(AddEdge(collision));
    }

    private IEnumerator AddEdge(Collision collision)
    {
        IWaypoint target = collision.transform.parent.GetComponent<WaypointController>().waypoint;
        yield return missionRef.AddEdge(source, target);
        GameObject.Destroy(this.gameObject);
    }
}
