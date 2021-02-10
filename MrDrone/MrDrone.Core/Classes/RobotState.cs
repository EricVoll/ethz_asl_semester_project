using MrDrone.Core.Basics;
using MrDrone.Core.Interfaces;
using RosSharp.RosBridgeClient.MessageTypes.Sensor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MrDrone.Core.Classes
{
    public class RobotState : IRobotState
    {
        public IPose Pose { get; set; }
        public float BatteryLevel { get; set; }
        public IPose CurrentTarget { get; set; }
        public Imu Imu { get; set; }
        public JointState JointSate { get; set; }

        public bool ImuHasChanged { get; set; }
        public bool JointStateHasChanged { get; set; }
        public bool PoseHasChanged { get; set; }

        public void ReportImuSynched()
        {
            ImuHasChanged = false;
        }

        public void ReportJointStateSynched()
        {
            JointStateHasChanged = false;
        }

        public void ReportPoseSynched()
        {
            PoseHasChanged = false;
        }
    }
}
