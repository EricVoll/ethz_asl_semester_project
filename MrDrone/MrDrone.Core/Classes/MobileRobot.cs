using MrDrone.Core.Basics;
using MrDrone.Core.Interfaces;
using Newtonsoft.Json;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.Mav;
using RosSharp.RosBridgeClient.MessageTypes.Nav;
using RosSharp.RosBridgeClient.MessageTypes.Sensor;
using RosSharp.RosBridgeClient.MessageTypes.Trajectory;
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

        public virtual void CommandTrajectory(RosSharp.RosBridgeClient.MessageTypes.Trajectory.MultiDOFJointTrajectory trajectory)
        {
            RosSocket.Publish(Topics.GetPublishId(nameof(CommandTrajectory)), trajectory);
        }
        public virtual void CommandTrajectory(IPose targetPose)
        {
            MultiDOFJointTrajectory msg = new MultiDOFJointTrajectory();
            msg.joint_names = new[] { "base_link" };
            msg.points = new[]
            {
                new MultiDOFJointTrajectoryPoint()
                {
                    transforms = new[]
                    {
                        targetPose.ToTransform()
                    }
                }
            };
            CommandTrajectory(msg);
        }

        /// <summary>
        /// Configures the StateSubscriber of the mobile Robot, which udpates the robots state
        /// </summary>
        protected virtual void ConfigureStateSubscriber()
        {

        }

        protected void DigestOdometryStateMessage(Odometry odom)
        {
            State.Pose.Update(odom);
            State.PoseHasChanged = true;
        }

        protected void DigestImuStateMessage(Imu imu)
        {
            State.Imu = imu;
            State.ImuHasChanged = true;
        }

        protected void DigestJointStateMessage(JointState state)
        {
            State.JointSate = state;
            State.JointStateHasChanged = true;
        }
    }
}
