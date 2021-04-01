using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.Geometry;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SpotSharp
{
    public class Spot : MobileRobot
    {
        RosSocket rosSocket;

        Dictionary<string, string> publicationIds = new Dictionary<string, string>();

        const string topic_cmd_vel = "/spot/cmd_vel";

        public Spot()
        {

        }

        public Spot(RosSocket socket)
        {
            rosSocket = socket;

            publicationIds[topic_cmd_vel] =
                rosSocket.Advertise<RosSharp.RosBridgeClient.MessageTypes.Geometry.Twist>(topic_cmd_vel);
        }

        /// <summary>
        /// Calls the stand service
        /// </summary>
        public void Stand()
        {
            if (rosSocket == null) return;
            CallStringService("/spot/stand");
        }

        /// <summary>
        /// Calls the stand service
        /// </summary>
        public void Sit()
        {
            if (rosSocket == null) return;
            CallStringService("/spot/sit");
        }

        public void SelfRight()
        {
            if (rosSocket == null) return;
            CallStringService("/spot/self_right");
        }

        public void Claim()
        {
            if (rosSocket == null) return;
            CallStringService("/spot/claim");
        }
        public void Release()
        {
            if (rosSocket == null) return;
            CallStringService("/spot/release");
        }

        public void PowerOn()
        {
            if (rosSocket == null) return;
            CallStringService("/spot/power_on");
        }
        public void PowerOff()
        {
            if (rosSocket == null) return;
            CallStringService("/spot/power_off");
        }

        public void CommandTo(Pose pose)
        {
            if (rosSocket == null) return;
            Console.WriteLine(nameof(CommandTo));
        }

        public void CommandVelocity(Twist twist)
        {
            if (rosSocket == null) return;
            Console.WriteLine(nameof(CommandVelocity));

            rosSocket.Publish(publicationIds[topic_cmd_vel], twist);
        }


        private string CallStringService(string topic)
        {
            Console.WriteLine("Calling service " + topic);
            var t = new ServiceResponseHandler<RosSharp.RosBridgeClient.MessageTypes.Std.String>((o) => { Console.WriteLine(o.data); });

            return rosSocket.CallService<RosSharp.RosBridgeClient.MessageTypes.Std.String, RosSharp.RosBridgeClient.MessageTypes.Std.String>(topic, t, null);
        }
    }
}
