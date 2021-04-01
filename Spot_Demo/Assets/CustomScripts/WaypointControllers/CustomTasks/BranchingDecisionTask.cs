using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WaypointControl.Edge.Interfaces;
using WaypointControl.Task.Interfaces;

public class BranchingDecisionTask : IBranchDecisionTask
{
    public BranchingDecisionTask(Func<List<WaypointController>> retrieveAllWaypointController)
    {
        this.RetrieveAllWaypointControllers = retrieveAllWaypointController;
    }

    Func<List<WaypointController>> RetrieveAllWaypointControllers;

    public void MakeDecision(List<IEdge> outgoingEdges, Action<IBranchDecisionResult> answer)
    {
        List<WaypointController> controllers = new List<WaypointController>();
        var allWaypoints = RetrieveAllWaypointControllers();

        foreach (var edge in outgoingEdges)
        {
            controllers.Add(allWaypoints.First(x => x.waypoint.LifetimeId == edge.Target.LifetimeId));
        }

        WaypointController sourceController = allWaypoints.First(x => x.waypoint.LifetimeId == outgoingEdges.First().Source.LifetimeId);

        GameObject.FindObjectOfType<BranchingDecider>().Decide(answer, controllers, sourceController);
    }
}
