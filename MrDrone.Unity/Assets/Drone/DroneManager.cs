using MrDrone.Core.Classes;
using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneManager : MonoBehaviour
{
    public RosConnector RosConnector;

    // Start is called before the first frame update
    void Start()
    {
        RosConnector.RosSocket.protocol.OnConnected += Protocol_OnConnected;
    }

    private void Protocol_OnConnected(object sender, System.EventArgs e)
    {
        InitializeDrone(RosConnector.RosSocket);
    }

    private void InitializeDrone(RosSocket socket)
    {
        Drone = new Drone(() => socket);
    }

    Drone Drone;

    // Update is called once per frame
    void Update()
    {
        //The drone is not initialized yet. That could be because of the RosSocket not yet being connected to the ros-bridge
        if (Drone == null) return;

        //Update the drone's position
        (var position, var rotation) = Drone.State.Pose.ToUnity();
        this.transform.position = position;
        this.transform.rotation = rotation;
    }
}
