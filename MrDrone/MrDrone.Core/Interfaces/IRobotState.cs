using MrDrone.Core.Basics;
using RosSharp.RosBridgeClient.MessageTypes.Sensor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MrDrone.Core.Interfaces
{
    public interface IRobotState
    {
        IPose CurrentTarget { get; set; }
        float BatteryLevel { get; set; }

        Imu Imu { get; set; }
        JointState JointSate { get; set; }
        IPose Pose { get; set; }

        bool ImuHasChanged { get; set; }
        bool JointStateHasChanged { get; set; }
        bool PoseHasChanged { get; set; }

        void ReportImuSynched();
        void ReportJointStateSynched();
        void ReportPoseSynched();
    }
}
