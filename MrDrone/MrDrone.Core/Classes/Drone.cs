using MrDrone.Core.Basics;
using MrDrone.Core.Interfaces;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.Mav;
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
            base.RegisterMainTopics();

            Topics = new TopicHandler("firefly");
            Topics.AddTopic<Actuators>(nameof(CommandActuator), "command/motor_speed", RosSocket);
        }


    }
}
