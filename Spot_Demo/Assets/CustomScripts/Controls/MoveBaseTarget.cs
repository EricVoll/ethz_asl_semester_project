using Microsoft.MixedReality.Toolkit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A component which serves as a target item for the robot.
/// It serves as a communication center for the application 
/// with regards to the goal pose, and also handles the behavior 
/// the goal component should have.
/// </summary>
public class MoveBaseTarget : TriggerService
{
    /// <summary>
    /// The GameObject-Prefab used to rotate the goal pose marker object.
    /// </summary>
    [SerializeField]
    private GameObject RotationObject;

    private CoordinateFixing coordinateFixing;

    // Start is called before the first frame update
    void Start()
    {
        base.Register("move_base_goal_publishing_service");
        coordinateFixing = this.EnsureComponent<CoordinateFixing>();
    }

    void Update()
    {

    }

    /// <summary>
    /// Sends a trigger, to tell the responsible component (attached to the RosConnector)
    /// to publish the goal pose.
    /// </summary>
    public void SendGoalPoseMessage()
    {
        TriggerMessage();
    }
}
