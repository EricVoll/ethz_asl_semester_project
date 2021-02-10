using MrDrone.Core.Basics;
using MrDrone.Core.Interfaces;
using Newtonsoft.Json;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.Mav;
using RosSharp.RosBridgeClient.MessageTypes.Nav;
using RosSharp.RosBridgeClient.MessageTypes.Sensor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MrDrone.Core.Classes
{
    public class MobileRobot : IMobileRobot
    {
        public MobileRobot(
            Func<IRobotState> StateFactory,
            Func<RosSocket> RosSocketFactory
            )
        {
            State = StateFactory();
            RosSocket = RosSocketFactory();
        }

        protected RosSocket RosSocket;
        protected TopicHandler Topics;

        protected virtual void RegisterMainTopics()
        {
            ConfigureStateSubscriber();
        }

        public IRobotState State { get; private set; }

        public virtual void CommandActuator(double[] efforts)
        {
            /*
                new RosSharp.RosBridgeClient.MessageTypes.Std.Header(
                    1, new RosSharp.RosBridgeClient.MessageTypes.Std.Time((uint)DateTime.Now.TimeOfDay.TotalSeconds, 40), "my frame")*/

            Actuators msg = new Actuators(
                new RosSharp.RosBridgeClient.MessageTypes.Std.Header(), 
                new double[0], 
                efforts,
                new double[0]);
            string json = JsonConvert.SerializeObject(msg);
            RosSocket.Publish(Topics.GetPublishId(nameof(CommandActuator)), msg);
            Console.WriteLine($"Publishing to {Topics.GetTopic(nameof(CommandActuator))}: {json}");
        }

        /// <summary>
        /// Configures the StateSubscriber of the mobile Robot, which udpates the robots state
        /// </summary>
        protected virtual void ConfigureStateSubscriber()
        {

        }

        protected void DigestOdometryStateMessage(Odometry odom)
        {
            this.State.Pose.Update(odom);
        }

        protected void DigestImuStateMessage(Imu imu)
        {
            this.State.Imu = imu;
        }
    }
}
