using MrDrone.Core.Basics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MrDrone.Core.Interfaces
{
    public interface IRobotState
    {
        IPose Pose { get; set; }
        float BatteryLevel { get; set; }
        IPose CurrentTarget { get; set; }
    }
}
