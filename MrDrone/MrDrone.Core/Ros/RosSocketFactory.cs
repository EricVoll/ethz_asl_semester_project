using RosSharp.RosBridgeClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MrDrone.Core.Ros
{
    public static class RosSocketFactory
    {
        public static RosSocket GetStandardRosSocket(string uri)
        {
            return GetStandardRosSocket(uri, null, null);
        }

        public static RosSocket GetStandardRosSocket(string uri, Action OnConnected, Action OnClosed)
        {
            RosSharp.RosBridgeClient.Protocols.WebSocketNetProtocol webSocketNetProtocol = new RosSharp.RosBridgeClient.Protocols.WebSocketNetProtocol(uri);
            var protocol = RosSharp.RosBridgeClient.Protocols.ProtocolInitializer.GetProtocol(RosSharp.RosBridgeClient.Protocols.Protocol.WebSocketNET, uri);
            
            if (OnConnected != null)
                protocol.OnConnected += (sender, eventArgs) => { OnConnected?.Invoke(); };
            if (OnClosed != null)
                protocol.OnClosed += (sender, eventArgs) => { OnClosed?.Invoke(); };

            var rosSocket = new RosSocket(protocol, RosSocket.SerializerEnum.Newtonsoft_JSON);
            return rosSocket;
        }
    }
}
