using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WaypointControl.Edge.Interfaces;

public class WaypointLine : MonoBehaviour
{
    LineRenderer renderer;
    public Transform centerPiece;
    MissionController mission;
    IEdge iEdge;
    public GameObject Menu;

    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<LineRenderer>();
    }

    public void Init(MissionController mission, IEdge iedge)
    {
        this.mission = mission;
        this.iEdge = iedge;
    }

    // Update is called once per frame
    void Update()
    {
        if (transforms != null)
        {
            renderer.positionCount = transforms.Length;
            renderer.SetPositions(transforms.Select(x => x.position).ToArray());
            if(centerPiece != null)
            {
                centerPiece.position = transforms.First().position + (transforms.Last().position - transforms.First().position) / 2;
            }
        }
    }

    Transform[] transforms;

    public void SetTargets(Transform[] lineItems)
    {
        transforms = lineItems;
    }

    private bool toggle = false;
    public void Clicked()
    {
        toggle = !toggle;
        Menu.SetActive(toggle);
    }

    public void DeleteEdge()
    {
        mission.DeleteEdge(iEdge);
    }
}
