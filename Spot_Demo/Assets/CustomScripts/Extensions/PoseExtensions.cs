using RobotUtilities;
using UnityEngine;

public static class PoseExtensions
{

    public static void SetFromTransform(this RobotUtilities.IPose pose, Transform transform, bool useLocal = true)
    {
        if (useLocal)
        {
            pose.X = transform.localPosition.x;
            pose.Y = transform.localPosition.y;
            pose.Z = transform.localPosition.z;
            pose.i = transform.localRotation.x;
            pose.j = transform.localRotation.y;
            pose.k = transform.localRotation.z;
            pose.w = transform.localRotation.w;
        }
        else
        {
            pose.X = transform.position.x;
            pose.Y = transform.position.y;
            pose.Z = transform.position.z;
            pose.i = transform.rotation.x;
            pose.j = transform.rotation.y;
            pose.k = transform.rotation.z;
            pose.w = transform.rotation.w;
        }
    }

    public static RosSharp.RosBridgeClient.MessageTypes.Geometry.Pose ToRosPose(this IPose pose)
    {
        (var pos, var rot) = (pose.ToPositionVector(), pose.ToRotationQuaternion());
        pos = RosSharp.TransformExtensions.Unity2Ros(pos);
        rot = RosSharp.TransformExtensions.Unity2Ros(rot);

        RosSharp.RosBridgeClient.MessageTypes.Geometry.Pose p = new RosSharp.RosBridgeClient.MessageTypes.Geometry.Pose();
        p.position = new RosSharp.RosBridgeClient.MessageTypes.Geometry.Point()
        {
            x = pos.x,
            y = pos.y,
            z = pos.z
        };
        p.orientation = new RosSharp.RosBridgeClient.MessageTypes.Geometry.Quaternion()
        {
            x = rot.x,
            y = rot.y,
            z = rot.z,
            w = rot.w
        };
        return p;
    }

    public static (Vector3, Quaternion) Ros2Unity(RosSharp.RosBridgeClient.MessageTypes.Geometry.Point pos, RosSharp.RosBridgeClient.MessageTypes.Geometry.Quaternion quat, bool useRos2Unity = true)
    {
        Vector3 posV = new Vector3((float)pos.x, (float)pos.y, (float)pos.z);
        Quaternion quatQ = new Quaternion((float)quat.x, (float)quat.y, (float)quat.z, (float)quat.w);
        if (useRos2Unity)
        {
            posV = RosSharp.TransformExtensions.Ros2Unity(posV);
            quatQ = RosSharp.TransformExtensions.Ros2Unity(quatQ);
        }
        return (posV, quatQ);
    }

    public static void SetFromRosSharp(this RobotUtilities.IPose pose, RosSharp.RosBridgeClient.MessageTypes.Geometry.Point pos, RosSharp.RosBridgeClient.MessageTypes.Geometry.Quaternion quat, bool useRos2Unity = true)
    {
        Vector3 posV = new Vector3((float)pos.x, (float)pos.y, (float)pos.z);
        Quaternion quatQ = new Quaternion((float)quat.x, (float)quat.y, (float)quat.z, (float)quat.w);

        if (useRos2Unity)
        {
            posV = RosSharp.TransformExtensions.Ros2Unity(posV);
            quatQ = RosSharp.TransformExtensions.Ros2Unity(quatQ);
        }

        pose.X = posV.x;
        pose.Y = posV.y;
        pose.Z = posV.z;

        pose.i = quatQ.x;
        pose.j = quatQ.y;
        pose.k = quatQ.z;
        pose.w = quatQ.w;
    }

    public static void SetFromTransformPosition(this RobotUtilities.IPose pose, Transform transform)
    {
        SetFromPosition(pose, transform.position);
    }
    public static void SetFromTransformRotation(this RobotUtilities.IPose pose, Transform transform)
    {
        SetFromRotation(pose, transform.rotation);
    }

    public static void SetFromPosition(this IPose pose, Vector3 pos)
    {
        pose.X = pos.x;
        pose.Y = pos.y;
        pose.Z = pos.z;
    }
    public static void SetFromRotation(this IPose pose, Quaternion quat)
    {
        pose.i = quat.x;
        pose.j = quat.y;
        pose.k = quat.z;
        pose.w = quat.w;
    }

    public static Vector3 ToPositionVector(this RobotUtilities.IPose pose)
    {
        return new Vector3((float)pose.X, (float)pose.Y, (float)pose.Z);
    }

    public static IPose ToIPose<T>(this Vector3 vector3) where T : IPose, new()
    {
        T t = new T();
        t.X = vector3.x;
        t.Y = vector3.y;
        t.Z = vector3.z;
        return t;
    }

    public static Quaternion ToRotationQuaternion(this RobotUtilities.IPose pose)
    {
        return new Quaternion((float)pose.i, (float)pose.j, (float)pose.k, (float)pose.w);
    }
    public static RobotUtilities.Pose Translate(this IPose pose, Vector3 offset)
    {
        return new RobotUtilities.Pose(pose.X + offset.x, pose.Y + offset.y, pose.Z + offset.z, pose.i, pose.j, pose.k, pose.w);
    }
    public static RobotUtilities.IPose Translate<T>(this IPose pose, Vector3 offset) where T : IPose, new()
    {
        T t = new T();
        t.X = pose.X + offset.x;
        t.Y = pose.Y + offset.y;
        t.Z = pose.Z + offset.z;
        t.i = pose.i;
        t.j = pose.j;
        t.k = pose.k;
        t.w = pose.w;
        return t;
    }
    public static RobotUtilities.Poses.PositionOnly2DPose To2DPositionOnly(this IPose pose)
    {
        return new RobotUtilities.Poses.PositionOnly2DPose()
        {
            X = pose.X,
            Y = pose.Y,
            Z = pose.Z
        };
    }

    public static float Magnitude2D(this Vector3 v)
    {
        return Mathf.Sqrt(v.x * v.x + v.z * v.z);
    }
}
