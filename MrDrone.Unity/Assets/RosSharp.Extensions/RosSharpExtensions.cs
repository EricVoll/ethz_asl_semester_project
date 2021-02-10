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
}
