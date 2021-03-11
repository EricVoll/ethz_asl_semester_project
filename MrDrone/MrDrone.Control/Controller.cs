using MrDrone.Core.Interfaces;
using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.Cgal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MrDrone.Core.Classes;
using MrDrone.Control.Mesh;

namespace MrDrone.Control
{
    public class Controller
    {
        private RosSocket socket;
        private Drone drone;
        public MeshHelper MeshHelper { get; private set; }

        public void Config(RosSocket socket, Drone drone)
        {
            this.socket = socket;
            this.drone = drone;
            MeshHelper = new MeshHelper();

            socket.Subscribe<TriangleMeshStamped>("/mesh_publisher/mesh_out", MeshHelper.GetSubscriptionHandler(), 1000);
        }


    }
}
