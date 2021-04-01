using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.AsaRos;
using RosSharp.RosBridgeClient.MessageTypes.AsaRosCommander;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsaUtilities
{
    private RosSocket rosSocket;
    RosSharpUtilities utils;
    public EventHandler<AnchorFoundEventArgs> foundAnchorEvent;

    string createTfId;
    string foundTfId;

    public AsaUtilities(RosSocket rosSocket)
    {
        this.rosSocket = rosSocket;

        createTfId = rosSocket.Advertise<AsaRelPoseStamped>("/mock_anchor_created");
        foundTfId = rosSocket.Advertise<AsaRelPoseStamped>("/mock_anchor_found");

        //Subscribe to found anchors
        SubscriptionHandler<FoundAnchor> handler = new SubscriptionHandler<FoundAnchor>((foundAnchor) => {
            foundAnchorEvent?.Invoke(this, new AnchorFoundEventArgs(foundAnchor));
        });
        rosSocket.Subscribe<FoundAnchor>("asa_ros/found_anchor", handler, 1, 5);
    }

    public void AsaRosWrapperFindAnchorServiceCall(string anchorId)
    {
        var t = new ServiceResponseHandler<RosSharp.RosBridgeClient.MessageTypes.Std.String>((o) => { Debug.Log(o.data); });
        rosSocket.CallService("/asa_ros/find_anchor", t,
            new FindAnchorMsg(anchorId)
            );
    }

    /// <summary>
    /// Mocks the creation of an anchor via the asa_ros_commanders asa mocker
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="name"></param>
    public void MockCreateAnchorAt(double x, double y, double z, string name)
    {
        SendFrameRelativeToOdom(name, createTfId, x, y, z);
    }

    /// <summary>
    /// Mocks the finding of an anchor via the asa_ros_commanders asa mocker
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="name"></param>
    public void MockFindAnchorAt(double x, double y, double z, string name)
    {
        SendFrameRelativeToOdom(name, foundTfId, x, y, z);
    }

    private void SendFrameRelativeToOdom(string name, string publishId, double x)
    {
        SendFrameRelativeToOdom(name, publishId, x, 0, 0);
    }


    private void SendFrameRelativeToOdom(string name, string publishId, double x, double y, double z)
    {
        rosSocket.Publish(publishId, new AsaRelPoseStamped()
        {
            header = new RosSharp.RosBridgeClient.MessageTypes.Std.Header()
            {
                frame_id = "odom"
            },
            pose = new RosSharp.RosBridgeClient.MessageTypes.Geometry.Pose()
            {
                position = new RosSharp.RosBridgeClient.MessageTypes.Geometry.Point(x, y, z),
                orientation = new RosSharp.RosBridgeClient.MessageTypes.Geometry.Quaternion(0, 0, 0, 1)
            },
            anchor_id = name
        });
    }
}
public class FindAnchorMsg : Message
{
    public FindAnchorMsg(string id)
    {
        anchor_id = id;
    }
    public string anchor_id { get; set; }
}

public class AnchorFoundEventArgs : EventArgs
{
    public FoundAnchor FoundAnchor { get; set; }

    public AnchorFoundEventArgs(FoundAnchor foundAnchor)
    {
        FoundAnchor = foundAnchor;
    }
}