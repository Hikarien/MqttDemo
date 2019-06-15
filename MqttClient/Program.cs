using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Subscribing;

namespace MqttClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
        var optionbuilder = new MqttClientOptionsBuilder()
            .WithClientId("client_1")
            .WithTcpServer("127.0.0.1", 3883);

            var optionbuilder2 = new MqttClientOptionsBuilder()
                .WithClientId("client_2")
                .WithTcpServer("127.0.0.1", 3883);

            var mqttClient = new MqttFactory().CreateMqttClient();
            var mqttClient2 = new MqttFactory().CreateMqttClient();

            var ret = await mqttClient.ConnectAsync(optionbuilder.Build());
            Console.WriteLine($"client_1 result : {ret.ResultCode}");

            var ret2 = await mqttClient2.ConnectAsync(optionbuilder2.Build());
            Console.WriteLine($"client_2 result : {ret2.ResultCode}");


            var subcriberet = await mqttClient2.SubscribeAsync(new MqttClientSubscribeOptions()
            {
                TopicFilters = new List<TopicFilter>
                {
                    new TopicFilter()
                    {
                        Topic = "abc"
                    }
                }
            });
            foreach (var item in subcriberet.Items)
            {
                Console.WriteLine($"Topic:{item.TopicFilter.Topic}, result:{item.ResultCode}");
            }

            mqttClient2.UseApplicationMessageReceivedHandler((e) =>
            {
                var msg = e.ApplicationMessage;
                var topic = msg.Topic;
                var payload = msg.ConvertPayloadToString();
                Console.WriteLine($"clientId:{e.ClientId}, topic:{topic}, payload:{payload}");
            });

            while (true)
            {
                var input = Console.ReadLine().ToLower().Trim();
                switch (input)
                {
                    case "exit":
                        mqttClient.Dispose();
                        mqttClient2.Dispose();
                        return;
                    default:
                        await mqttClient.PublishAsync(new MqttApplicationMessage()
                        {
                            Topic = "abc",
                            Payload = Encoding.UTF8.GetBytes(DateTime.Now.ToString())

                        }, CancellationToken.None);
                        break;
                }
            }
        }
    }
}
