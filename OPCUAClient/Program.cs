using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Opc.Ua;
using Opc.Ua.Client;
using Opc.Ua.Configuration;
using System.Net.WebSockets;
using System.Text.Json;

namespace OPCUAClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                var opcUaClient = new OpcUaClient("opc.tcp://localhost:4840", "http://yourorganization.org/SimpleOpcUaServer/");
                await opcUaClient.ConnectAsync();

                // Thiết lập WebSocket ROS2
                string ros2ServerUri = "ws://192.168.137.218:9090";
                var cancellationTokenSource = new CancellationTokenSource();
                using (var clientWebSocket = new ClientWebSocket())
                {
                    await clientWebSocket.ConnectAsync(new Uri(ros2ServerUri), cancellationTokenSource.Token);

                    // Đăng ký nhận dữ liệu từ topic "char"
                    string subscribeRequest = "{\"op\":\"subscribe\",\"topic\":\"/char\"}";
                    byte[] subscribeRequestBytes = Encoding.UTF8.GetBytes(subscribeRequest);
                    await clientWebSocket.SendAsync(new ArraySegment<byte>(subscribeRequestBytes), WebSocketMessageType.Text, true, CancellationToken.None);

                    // Nhận và xử lý dữ liệu từ topic "/char"
                    var receiveBuffer = new byte[1024];
                    while (clientWebSocket.State == WebSocketState.Open)
                    {
                        var result = await clientWebSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), CancellationToken.None);
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            var message = Encoding.UTF8.GetString(receiveBuffer, 0, result.Count);
                            Console.WriteLine($"Received from ROS2: {message}");

                            // Dữ liệu ROS2 gửi về là một JSON có định dạng {"op": "publish", "topic": "/char", "msg": {"data": "A"}}
                            var jsonDocument = JsonDocument.Parse(message);
                            if (jsonDocument.RootElement.TryGetProperty("msg", out JsonElement msgElement) &&
                                msgElement.TryGetProperty("data", out JsonElement dataElement) &&
                                dataElement.GetString() is string charValue && charValue.Length == 1)
                            {
                                string receivedChar = charValue;  // Convert char to string
                                await opcUaClient.WriteValueAsync("Hello", receivedChar);
                                Console.WriteLine($"Written to OPC UA Server: Hello = {receivedChar}");
                                await opcUaClient.ReadValueAsync("Hello");
                            }
                        }
                    }
                }

                opcUaClient.Disconnect();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }

            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }
    }
}