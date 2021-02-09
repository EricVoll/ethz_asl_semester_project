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


        public void AddTopic(string name, string topic, string publishId)
        {
            if(NameToTopicMap.ContainsKey(name) == false)
            {
                NameToTopicMap[name] = topic;
                NameToPublishId[name] = publishId;
            }
        }

        public void AddTopic<T>(string name, string topic, RosSocket socket) where T : Message, new()
        {
            string publishId = socket.Advertise<T>($"{nodeNamespace}/{topic}");
            AddTopic(name, topic, publishId);
        }

        public string GetTopic(string name)
        {
            if (NameToTopicMap.ContainsKey(name)) return $"{nodeNamespace}/{NameToTopicMap[name]}";
            else throw new Exception($"The topic called {name} was not registered in the TopicHandler");
        }
        public string GetPublishId(string name)
        {
            if (NameToPublishId.ContainsKey(name)) return NameToPublishId[name];
            else throw new Exception($"The topic called {name} was not registered in the TopicHandler");
        }
    }
}
