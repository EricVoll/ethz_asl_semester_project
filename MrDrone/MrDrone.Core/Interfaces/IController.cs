using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MrDrone.Core.Interfaces
{
    public interface IController
    {
        void Config(RosSharp.RosBridgeClient.RosSocket socket);
    }
}
