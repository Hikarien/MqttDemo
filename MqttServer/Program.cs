using System;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Diagnostics;
using MQTTnet.Server;

namespace MqttServer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var optionbuilder = new MqttServerOptionsBuilder()
                .WithDefaultEndpointPort(3883)
                .WithConnectionValidator((valid) =>
            {


            });

            var server_logger = new MqttNetLogger("server logger");
            server_logger.LogMessagePublished += (sender, e) =>
            {

                //Console.WriteLine(e.TraceMessage.ToString());
            };

            var mqttServer = new MqttFactory().CreateMqttServer(server_logger);

            mqttServer.StartedHandler = new MqttServerStartedHandlerDelegate((e) =>
            {
                Console.WriteLine("mqtt server started!");
            });

            mqttServer.StoppedHandler = new MqttServerStoppedHandlerDelegate((e) =>
            {
                Console.WriteLine("mqtt server stopped!");
            });

            mqttServer.UseClientConnectedHandler((eventArgs) =>
            {
                Console.WriteLine($"client Connected ClientId:{eventArgs.ClientId}");
            });

            mqttServer.UseClientDisconnectedHandler((eventArgs) =>
            {
                Console.WriteLine($"client Disconnected ClientId:{eventArgs.ClientId} Type:{eventArgs.DisconnectType}");
            });

            mqttServer.UseApplicationMessageReceivedHandler((eventArgs) =>
            {
                var msg = eventArgs.ApplicationMessage;
                var topic = msg.Topic;
                var payload = msg.ConvertPayloadToString();

                Console.WriteLine($"Topic:{topic}\r\nPayload:{payload}");
            });

            mqttServer.ClientSubscribedTopicHandler = new MqttServerClientSubscribedHandlerDelegate((e) =>
            {
                Console.WriteLine($"topic subscribed ClientId:{e.ClientId} Topic:{e.TopicFilter.ToString()}");
            });

            mqttServer.ClientUnsubscribedTopicHandler = new MqttServerClientUnsubscribedTopicHandlerDelegate((e) =>
            {
                Console.WriteLine($"topic ubsubscribed ClientId:{e.ClientId} Topic:{e.TopicFilter.ToString()}");
            });

            await mqttServer.StartAsync(optionbuilder.Build());

            Console.WriteLine("输入exit退出");
            var input = string.Empty;
            while (true)
            {
                input = Console.ReadLine().ToLower().Trim();
                if (input == "exit") break;

                switch (input)
                {
                    case "exit":
                        await mqttServer.StopAsync();
                        return;
                    case "client":
                        var clients = await mqttServer.GetClientStatusAsync();
                        if (clients.Count == 0)
                        {
                            Console.WriteLine("no client");
                            continue;
                        }

                        foreach (var client in clients)
                        {
                            Console.WriteLine($"clientid:{client.ClientId}");
                        }
                        break;
                    case "msg":
                        var msgs = await mqttServer.GetRetainedApplicationMessagesAsync();
                        if (msgs.Count == 0)
                        {
                            Console.WriteLine("no message");
                        }

                        foreach (var item in msgs)
                        {
                            Console.WriteLine($"Topic:{item.Topic}, payload:{item.ConvertPayloadToString()}");
                        }
                        break;
                    case "session":
                        var mqttSessions = await mqttServer.GetSessionStatusAsync();
                        if (mqttSessions.Count == 0)
                        {
                            Console.WriteLine("no session");
                            continue;
                        }
                        foreach (var item in mqttSessions)
                        {
                            Console.WriteLine($"ClientId:{item.ClientId},PendingMsgCnt:{item.PendingApplicationMessagesCount}");
                        }
                        break;
                    default:
                        await Task.Delay(100);
                        break;
                }
            }
        }
    }
}