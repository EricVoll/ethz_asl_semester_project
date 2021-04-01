using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.WindowsDevicePortal;
using RobotUtilities;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.AsaRosCommander;
using RosSharp.RosBridgeClient.MessageTypes.Nav;
using SpotSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WaypointControl.Anchor;

public class RobotHandler : MonoBehaviour, IServiceConsumer<IServiceMessage>
{
    private RosSocket rosSocket;

    public void Awake()
    {
        //Setup and configure our mobile robot with all callbacks
        robotState = new State()
        {
            NavigationPrecision = .3f,
        };
        Robot = new MobileRobot();
        Robot.Delegates.SetRobotCommandToPoseDelegate(CommandRobotToPose);
        Robot.AutoCallPositionReachedCallback = true;

        Toolkit.singleton.RegisterServiceConsumer(this, "ConnectionStateService");

        TransformHelper = Instantiate(CoordSysDummyPrefab);
        TransformHelper.transform.position = new Vector3(0, 0, 0);
    }

    /// <summary>
    /// Internal robot state used to collect changes recorded from Ros#
    /// </summary
    private State robotState;

    [SerializeField]
    private GameObject RobotTargetIndicator;

    [SerializeField]
    private GameObject CoordSysDummyPrefab;

    [SerializeField]
    private MissionController missioncontroller;

    /// <summary>
    /// this component does custom subscriptions and publications and thus needs a direct
    /// reference to the rosconnector
    /// </summary>
    [SerializeField]
    private RosConnector rosConnector;

    [SerializeField]
    private GameObject RobotBase;

    /// <summary>
    /// Needs to be public since the mission controller sends it into the mission, but the robot ahndler manages it
    /// </summary>
    public MobileRobot Robot;

    private SpatialAnchorController currentAnchorController;

    private GameObject TransformHelper;

    public void ConsumeServiceItem(IServiceMessage item, string serviceName)
    {
        //Here we listen to all ros msg and update our robot state
        if (serviceName == "ConnectionStateService")
        {
            bool newRosConnectionState = ((ConnectionStateMessage)item).ConnectionState;
            if (newRosConnectionState)
            {
                ExecuteOnUpdate(() =>
                {
                    rosSocket = rosConnector.RosSocket;
                    SetupPublisherAndSubscribers(rosSocket);
                });
            }
            else
            {
                rosSocket = null;
            }
        }
    }



    public void Update()
    {
        if (Que.Count > 0)
            lock (Que)
            {
                foreach (var action in Que)
                {
                    action();
                }
                Que.Clear();
            }
    }

    IPose targetPose;

    /// <summary>
    /// Invoked from the Waypoint controller to command the robot to this position
    /// </summary>
    /// <param name="pos"></param>
    private void CommandRobotToPose(IPose pos)
    {
        if (rosSocket == null)
        {
            Debug.Log("Did not publish goal since the rosConnector is not connected");
            return;
        }

        StartCoroutine(CommandToPose(pos));
    }

    IEnumerator CommandToPose(IPose pos)
    {
        targetPose = pos;
        Debug.Log($"{pos.X} | {pos.Y} | {pos.Z}, Sleeping for 2 secs");

        yield return new WaitForSeconds(2f);

        if(currentAnchorController == null)
        {
            Debug.Log("Current anchor controller = null");
        }

        ExecuteOnUpdate(() =>
        {
            rosSocket.Publish(anchored_goal_publishing_id, new AsaRelPoseStamped()
            {
                pose = pos.ToRosPose(),
                anchor_id = currentAnchorController.GetAnchor().Id,
                header = new RosSharp.RosBridgeClient.MessageTypes.Std.Header()
            });

            RobotTargetIndicator.transform.localPosition = currentAnchorController.transform.position + pos.ToPositionVector();
        });
    }

    /// <summary>
    /// Sets the current anchor, such that all future commands will be sent realtive to it.
    /// </summary>
    /// <param name="anchor"></param>
    public void SetCurrentAnchor(SpatialAnchorController anchor)
    {
        currentAnchorController = anchor;
    }

    /// <summary>
    /// Sets up the publishers and subscribers for the state and position commanding
    /// </summary>
    /// <param name="rosSocket"></param>
    private void SetupPublisherAndSubscribers(RosSocket rosSocket)
    {
        if (rosSocket == null) return;

        SubscriptionHandler<Odometry> subscriptionHandler = new SubscriptionHandler<Odometry>((o) => ExecuteOnUpdate(() => ReportNewAsaRelPose(o)));

        rosSocket.Subscribe<Odometry>("/odometry/filtered/asa_relative", subscriptionHandler);
        anchored_goal_publishing_id = rosSocket.Advertise<AsaRelPoseStamped>("/anchored_goal");
    }


    /// <summary>
    /// the id which is used to publish the goal poinst
    /// </summary>
    private string anchored_goal_publishing_id;



    private void ReportNewAsaRelPose(Odometry relPose)
    {
        var anchor = missioncontroller.GetAnchorControllerById(relPose.header.frame_id);

        if (anchor == null)
        {
            Debug.Log("Anchor for pose was null");
            return;
        }

        TransformHelper.transform.SetParent(anchor.transform, false);

        var position = relPose.pose.pose.position;
        var orientation = relPose.pose.pose.orientation;

        (var pos, var or) = PoseExtensions.Ros2Unity(position, orientation);

        TransformHelper.transform.localPosition = pos;
        TransformHelper.transform.localRotation = or;

        if (anchor != null)
        {
            //Calculate the relative position to the current anchor
            //pos = anchor.transform.InverseTransformPoint(TransformHelper.transform.position);
            //or = (Quaternion.Inverse(anchor.transform.rotation) * TransformHelper.transform.rotation);

            robotState.Pose.SetFromPosition(pos);
            robotState.Pose.SetFromRotation(or);
        }
        else
        {
            //we do not have a current anchor yet, so assume that the anchor is still the one at origin
            robotState.Pose.SetFromRosSharp(position, orientation);
        }

        //Animate the robot's base odometry
        if (robotModelActive)
        {
            RobotBase.transform.SetParent(anchor.transform, false);
            RobotBase.transform.localPosition = pos;
            RobotBase.transform.localRotation = or;
        }

        //if(targetPose != null)
        //{
        //    Debug.Log($"{robotState.Pose.X - targetPose.X} | {robotState.Pose.Z - targetPose.Z}");
        //}

        Robot.Delegates.UpdateRobotState(robotState);
    }

    #region UpdateThreadExecution
    List<Action> Que = new List<Action>();
    public void ExecuteOnUpdate(Action action)
    {
        lock (Que)
        {
            Que.Add(action);
        }
    }
    #endregion

    #region Robot Model disabling
    bool robotModelActive = true;
    public void ToggleRobotModel()
    {
        robotModelActive = !robotModelActive;
        RobotBase.SetActive(robotModelActive);
    }
    #endregion
}
