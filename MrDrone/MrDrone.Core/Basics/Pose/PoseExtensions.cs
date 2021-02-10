using RosSharp.RosBridgeClient.MessageTypes.Nav;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MrDrone.Core.Basics
{
    public static class PoseExtensions
    {
        public static IPose FromOdom<T>(RosSharp.RosBridgeClient.MessageTypes.Nav.Odometry odom) where T : IPose, new()
        {
            T pose = new T();
            pose.X = odom.pose.pose.position.x;
            pose.Y = odom.pose.pose.position.y;
            pose.Z = odom.pose.pose.position.z;
            pose.i = odom.pose.pose.orientation.x;
            pose.j = odom.pose.pose.orientation.y;
            pose.k = odom.pose.pose.orientation.z;
            pose.w = odom.pose.pose.orientation.w;
            return pose;
        }

        public static void Update(this IPose pose, Odometry odom)
        {
            pose.X = odom.pose.pose.position.x;
            pose.Y = odom.pose.pose.position.y;
            pose.Z = odom.pose.pose.position.z;
            pose.i = odom.pose.pose.orientation.x;
            pose.j = odom.pose.pose.orientation.y;
            pose.k = odom.pose.pose.orientation.z;
            pose.w = odom.pose.pose.orientation.w;
        }
    }
}
