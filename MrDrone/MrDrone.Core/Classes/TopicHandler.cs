using RosSharp.RosBridgeClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MrDrone.Core.Classes
{
    public class TopicHandler
    {
        public TopicHandler(string robotNamespace)
        {
            this.nodeNamespace = robotNamespace;
        }
        
        private string nodeNamespace;
        
        Dictionary<string, string> NameToTopicMap = new Dictionary<string, string>();
        Dictionary<string, string> NameToPublishId = new Dictionary<string, string>();
        Dictionary<string, string> NameToSubscribeId = new Dictionary<string, string>();


        private void AddTopic(string name, string topic, string publishId)
        {
            if(NameToTopicMap.ContainsKey(name) == false)
            {
                NameToTopicMap[name] = topic;
                NameToPublishId[name] = publishId;
            }
        }

        /// <summary>
        /// Advertises the Topic and Message to RosSharp and stores the topic/id in the map
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="name"></param>
        /// <param name="topic"></param>
        /// <param name="socket"></param>
        public void AddPublisher<T>(string name, string topic, RosSocket socket) where T : Message, new()
        {
            string publishId = socket.Advertise<T>($"{nodeNamespace}/{topic}");
            AddTopic(name, topic, publishId);
        }

        /// <summary>
        /// Adds a subscriber to the map
        /// </summary>
        /// <param name="name"></param>
        /// <param name="topic"></param>
        public void AddSubscriber<T>(string name, string topic, RosSocket socket, Action<T> callback) where T : Message, new()
        {
            SubscriptionHandler<T> handler = new SubscriptionHandler<T>(callback);

            if (NameToTopicMap.ContainsKey(name) == false)
            {
                NameToTopicMap[name] = topic;
            }
            string subscriptionId = socket.Subscribe<T>(GetTopic(name), handler);
            NameToSubscribeId[name] = subscriptionId;
        }

        /// <summary>
        /// Returns the stored topic of the name.
        /// </summary>
        /// <param name="name"></param>
        /// <exception cref="Exception">Thrown if the name was not configured</exception>
        /// <returns></returns>
        public string GetTopic(string name)
        {
            if (NameToTopicMap.ContainsKey(name)) return $"{nodeNamespace}/{NameToTopicMap[name]}";
            else throw new Exception($"The topic called {name} was not registered in the TopicHandler");
        }
        /// <summary>
        /// Returns the publisher id used for this name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string GetPublishId(string name)
        {
            if (NameToPublishId.ContainsKey(name)) return NameToPublishId[name];
            else throw new Exception($"The topic called {name} was not registered in the TopicHandler");
        }
    }
}
