using MrDrone.Core.Basics;
using MrDrone.Core.Interfaces;
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
    }
}
