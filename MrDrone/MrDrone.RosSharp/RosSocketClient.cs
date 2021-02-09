using MrDrone.Core.Interfaces;
using RosSharp.RosBridgeClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MrDrone.RosSharp
{
    public class RosSocketClient : IRosSocket
    {
        private RosSocket rosSocket;

        public RosSocketClient(RosSocket rosSocket)
        {
            this.rosSocket = rosSocket;
        }


        public bool IsConnectedToRosCore => throw new NotImplementedException();

        public void Connect(string url)
        {
            if (!IsConnectedToRosCore)
            {
                rosSocket
            }
        }

        public void PublishActuatorEffort(string topic, double[] efforts)
        {
            throw new NotImplementedException();
        }
    }
}
