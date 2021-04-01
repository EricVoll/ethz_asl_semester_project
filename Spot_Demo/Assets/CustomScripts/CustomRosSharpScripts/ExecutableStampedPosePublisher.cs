using RosSharp;
using RosSharp.RosBridgeClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The executable StampedPosePublisher listens to the Serivce "move_base_goal_publishing_service"
/// and publishes its assigned Transform every time when the service triggers it.
/// </summary>
public class ExecutableStampedPosePublisher : PoseStampedPublisher
{

    protected override void Start()
    {
        base.Start();
    }

    public bool ShouldPublishOnce = false;


    private Transform CoordinateFrame;

    public void SendPose(Transform transformToSend, Transform coordinateFrame)
    {
        PublishedTransform = transformToSend;
        CoordinateFrame = coordinateFrame;
        ShouldPublishOnce = true;
    }

    protected override void UpdateMessage()
    {
        if (ShouldPublishOnce && canPublish)
        {
            ShouldPublishOnce = false;

            Vector3 pos = new Vector3();
            Quaternion rot = new Quaternion();

            if (CoordinateFrame != null)
            {
                pos = CoordinateFrame.InverseTransformPoint(PublishedTransform.position).Unity2Ros();
                rot = (Quaternion.Inverse(CoordinateFrame.transform.rotation) * PublishedTransform.rotation).Unity2Ros();
            }
            else
            {
                pos = PublishedTransform.localPosition.Unity2Ros();
                rot = PublishedTransform.localRotation.Unity2Ros();
            }

            message.header.Update();
            GetGeometryPoint(pos, message.pose.position);
            GetGeometryQuaternion(rot, message.pose.orientation);

            Publish(message);
            Debug.Log("Published move_base_goal point!");
        }
    }
}
