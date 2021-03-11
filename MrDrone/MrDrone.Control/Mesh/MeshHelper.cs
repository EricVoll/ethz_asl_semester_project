using RosSharp.RosBridgeClient;
using RosSharp.RosBridgeClient.MessageTypes.Cgal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MrDrone.Control.Mesh
{
    public class MeshHelper
    {
        public TriangleMeshStamped Mesh { get; private set;}
        private bool MeshReceived = false;
        private event EventHandler MeshReceivedEvent;


        /// <summary>
        /// Returns a Subscription handler which stores the received mesh
        /// </summary>
        /// <returns></returns>
        internal SubscriptionHandler<TriangleMeshStamped> GetSubscriptionHandler()
        {
            SubscriptionHandler<TriangleMeshStamped> subscriptionHandler = new SubscriptionHandler<TriangleMeshStamped>((o) => {
                Mesh = o;
                MeshReceived = true;
                MeshReceivedEvent?.Invoke(this, EventArgs.Empty);
            });

            return subscriptionHandler;
        }


        public void SubscribeToMeshReceivedEvent(Action<object, EventArgs> eventHandler)
        {
            if(MeshReceived)
            {
                eventHandler?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                MeshReceivedEvent += (o,e) => eventHandler(o,e);
            }
        }
    }
}
