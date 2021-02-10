using MrDrone.Core.Basics;
using MrDrone.Core.Interfaces;
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
    public class Drone : MobileRobot
    {
        public Drone(Func<RosSocket> RosSocketFactory) : base(
            () => new RobotState(),
            RosSocketFactory
            )
        {
            State.Pose = new Pose6D();

            RegisterMainTopics();
        }

        protected override void RegisterMainTopics()
        {
            Topics = new TopicHandler("firefly");

            base.RegisterMainTopics();

            Topics.AddPublisher<Actuators>(nameof(CommandActuator), "command/motor_speed", RosSocket);
        }

        protected override void ConfigureStateSubscriber()
        {
            base.ConfigureStateSubscriber();

            Topics.AddSubscriber<Odometry>(nameof(DigestOdometryStateMessage), "ground_truth/odometry", RosSocket, DigestOdometryStateMessage);
            Topics.AddSubscriber<Imu>(nameof(DigestOdometryStateMessage), "imu", RosSocket, DigestImuStateMessage);
        }

    }
}
