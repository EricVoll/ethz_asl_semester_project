using MrDrone.Core.Basics;
using RosSharp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IPoseExtensions
{
    /// <summary>
    /// Converts the pose into a Vector3 position and a Quaternion rotation in Unity's left handed coordinate system
    /// </summary>
    /// <param name="pose"></param>
    /// <param name="convertRightToLeftHanded"></param>
    /// <returns></returns>
    public static (Vector3, Quaternion) ToUnity(this IPose pose, bool convertRightToLeftHanded = true)
    {
        Vector3 pos = new Vector3()
        {
            x = (float)pose.X,
            y = (float)pose.Y,
            z = (float)pose.Z
        };
        Quaternion rotation = new Quaternion()
        {
            x = (float)pose.i,
            y = (float)pose.j,
            z = (float)pose.k,
            w = (float)pose.w,
        };

        if (convertRightToLeftHanded)
        {
            pos = TransformExtensions.Ros2Unity(pos);
            rotation = TransformExtensions.Ros2Unity(rotation);
        }

        return (pos, rotation);
    }
}
