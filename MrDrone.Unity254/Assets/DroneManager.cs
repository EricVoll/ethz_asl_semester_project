﻿using RosSharp;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.Cgal;
using RosSharp.RosBridgeClient.MessageTypes.Nav;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroneManager : MonoBehaviour
{
    public RosConnector Connector;
    public GameObject Drone;
    public UIThreadHandler UIThreadHandler;
    public MeshGenerator MeshGenerator;

    private void Awake()
    {
        Connector.OnConnectedEvent += Connector_OnConnectedEvent;
        UIThreadHandler = new UIThreadHandler();
    }

    private void Connector_OnConnectedEvent(object sender, System.EventArgs e)
    {
        // subscribe
        SubscriptionHandler<Odometry> handler = new SubscriptionHandler<Odometry>((o) => {
            UIThreadHandler.ExecuteOnMainThread(() => { UpdateDroneOdom(o); });
        });
        Connector.RosSocket.Subscribe<Odometry>("/rovio/odometry", handler);

        SubscriptionHandler<TriangleMeshStamped> meshHandler = new SubscriptionHandler<TriangleMeshStamped>((o) => {
            UIThreadHandler.ExecuteOnMainThread(() => { UpdateMesh(o); });
        });
        Connector.RosSocket.Subscribe<TriangleMeshStamped>("/mesh_publisher/mesh_out", meshHandler, 1000);
    }

    private void UpdateMesh(TriangleMeshStamped o)
    {
        MeshGenerator.ReportNewMesh(o);
    }

    void UpdateDroneOdom(Odometry odom)
    {
        Vector3 pos = odom.pose.pose.position.ToUnity();
        Quaternion quat = new Quaternion((float)odom.pose.pose.orientation.x, (float)odom.pose.pose.orientation.y, (float)odom.pose.pose.orientation.z, (float)odom.pose.pose.orientation.w);
        Quaternion rot = TransformExtensions.Ros2Unity(quat);
        Drone.transform.localPosition = pos;
        Drone.transform.localRotation = rot;
    }

    public void Update()
    {
        UIThreadHandler.ReportUpdate();
    }

}