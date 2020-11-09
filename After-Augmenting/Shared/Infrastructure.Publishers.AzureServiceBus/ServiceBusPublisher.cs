using Infrastructure.EventStore.Abstractions;
using Infrastructure.Publishers.Abstractions;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Publishers.AzureServiceBus
{
    public class ServiceBusPublisher : IPublisher, IDisposable
    {
        private readonly string _connectionString;
        private readonly TopicClient _topicClient;

        public ServiceBusPublisher(string connectionString, string topicName)
        {
            _connectionString = connectionString;
            _topicClient = new TopicClient(_connectionString, topicName);
        }

        public void Dispose()
        {
            _topicClient.CloseAsync();
        }

        public async Task Publish(IEventData message)
        {
            if (message == null)
                return;

            var serializerSettings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None, DateParseHandling = DateParseHandling.DateTime };
            var messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject((message.Event), serializerSettings));

            var busMessage = new Message(messageBody)
            { MessageId = Guid.NewGuid().ToString() };

            busMessage.UserProperties.Add("EventType", message.EventType);
            busMessage.ContentType = "application/json";
            var test = Encoding.UTF8.GetString(busMessage.Body);
            Trace.WriteLine(test);
            await _topicClient.SendAsync(busMessage);
        }
    }
}
