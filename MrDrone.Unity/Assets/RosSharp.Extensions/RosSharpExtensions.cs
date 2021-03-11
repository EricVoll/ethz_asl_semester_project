using RosSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RosSharpExtensions
{
    public static Vector3 ToUnity(this RosSharp.RosBridgeClient.MessageTypes.Geometry.Vector3 vector)
    {
        Vector3 v = new Vector3((float)vector.x, (float)vector.y, (float)vector.z);
        v = TransformExtensions.Ros2Unity(v);
        return v;
    }
    public static Quaternion ToUnity(this RosSharp.RosBridgeClient.MessageTypes.Geometry.Quaternion vector)
    {
        Quaternion v = new Quaternion((float)vector.x, (float)vector.y, (float)vector.z, (float)vector.w);
        v = TransformExtensions.Ros2Unity(v);
        return v;
    }

    public static Vector3 ToUnity(this RosSharp.RosBridgeClient.MessageTypes.Geometry.Point point)
    {
        Vector3 v = new Vector3((float)point.x, (float)point.y, (float)point.z);
        return TransformExtensions.Ros2Unity(v);
    }
}
