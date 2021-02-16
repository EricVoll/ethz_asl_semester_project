using MrDrone.Core.Basics;
using MrDrone.Core.Classes;
using RosSharp;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.Sensor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DroneManager : MonoBehaviour
{
    public RosConnector RosConnector;

    // Start is called before the first frame update
    void Start()
    {
        if (RosConnector.IsConnected.WaitOne(0))
        {
            Protocol_OnConnected(this, EventArgs.Empty);
        }
        else
        {
            RosConnector.RosSocket.protocol.OnConnected += Protocol_OnConnected;
        }

    }

    private void Protocol_OnConnected(object sender, System.EventArgs e)
    {
        InitializeDrone(RosConnector.RosSocket);
    }

    private void InitializeDrone(RosSocket socket)
    {
        Drone = new Drone(() => socket);

        StateUpdaters = new List<Action>()
        {
            //Updates the drone's pose
            () => { if(Drone.State.PoseHasChanged) UpdateOdometry(); },
            //Updates the drone's rotors
            () => { if(Drone.State.JointStateHasChanged) UpdateJointState(); },
            //Updtaes the drone's IMU indicator
            () => { if(Drone.State.ImuHasChanged) UpdateImu(); },
            //Sends the drone to the target object
            () => { if(UseTrajectoryTarget) UpdateTrajectoryTarget(); }
        };
    }

    Drone Drone;

    List<Action> StateUpdaters;

    // Update is called once per frame
    void Update()
    {
        //The drone is not initialized yet. That could be because of the RosSocket not yet being connected to the ros-bridge
        if (Drone == null) return;

        //Checking all States if they changed saves us about 2/3 of updates, since our fps is about 3x the refresh rate of the state
        foreach (var updater in StateUpdaters)
            updater();
    }

    #region Drone Target Trajectory

    public GameObject TrajectoryTarget;
    public bool UseTrajectoryTarget = false;

    private void UpdateTrajectoryTarget()
    {
        if (!UseTrajectoryTarget) return;

        Drone.CommandTrajectory(TrajectoryTarget.transform.ToIPose<Pose6D>());
    }

    #endregion

    #region IMU

    public GameObject ImuIndicatorPrefab;
    private GameObject imuIndicator;
    private LineRenderer lineRenderer;
    /// <summary>
    /// Updates the drone's imu indicator
    /// </summary>
    private void UpdateImu()
    {
        if(ImuIndicatorPrefab == null)
        {
            Debug.LogError("Imu Indicator Prefab is null. Deactivating the IMU updater.");
            StateUpdaters.RemoveAt(2);
            return;
        }

        if(imuIndicator == null)
        {
            imuIndicator = GameObject.Instantiate(ImuIndicatorPrefab, transform, false);
            lineRenderer = imuIndicator.GetComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
        }

        lineRenderer.SetPositions(new[]
        {
            transform.position,
            transform.position + 0.1f * Drone.State.Imu.linear_acceleration.ToUnity()
        });

        Drone.State.ReportImuSynched();
    }
    #endregion

    #region Odometry
    /// <summary>
    /// Updates the Drone's odometry
    /// </summary>
    private void UpdateOdometry()
    {
        (var position, var rotation) = Drone.State.Pose.ToUnity();
        transform.position = position;
        transform.rotation = rotation;
        Drone.State.ReportPoseSynched();
    }
    #endregion

    #region JointStates
    private Dictionary<string, GameObject> Joints;
    private bool JointsAreConfigured => Joints != null;

    /// <summary>
    /// Animates the rotors according to the joint states in the Drone's state
    /// </summary>
    private void UpdateJointState()
    {
        var state = Drone.State.JointSate;

        //Ensure that all joints are references
        if (!JointsAreConfigured)
            SetupJointReferences(state.name);

        for (int i = 0; i < state.name.Length; i++)
        {
            Joints[state.name[i]].transform.rotation = Quaternion.AngleAxis(Mathf.Rad2Deg * (float)state.position[i], Vector3.up);
        }

        Drone.State.ReportJointStateSynched();
    }

    /// <summary>
    /// Configures a dictionary from joint name to GameObject to animate the joints (rotors)
    /// </summary>
    /// <param name="jointNames"></param>
    private void SetupJointReferences(string[] jointNames)
    {
        //Init
        Joints = new Dictionary<string, GameObject>();

        //Get all children
        List<GameObject> children = this.GetComponentsInChildren<Transform>().ToList().Select(x => x.gameObject).ToList();

        foreach (var name in jointNames)
        {
            if (Joints.ContainsKey(name) == false)
            {
                //find the gameobject
                string searchName = name.Substring(0, name.Length - "_joint".Length);

                GameObject joint = children.FirstOrDefault(x => x.name == searchName);

                if (joint == null)
                {
                    //We did not find the joint - skip it.
                    Debug.LogError($"Did not find the joint named {name} in this robot's GameObject Children");
                    continue;
                }

                Joints[name] = joint;
            }
        }

        if (jointNames.Length != Joints.Count)
        {
            Debug.LogError("At least one joint was not configured correctly");
        }
    }
    #endregion
}
