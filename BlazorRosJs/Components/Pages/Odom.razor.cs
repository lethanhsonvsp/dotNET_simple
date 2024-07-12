using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Newtonsoft.Json.Linq;
using RosbridgeNet.RosbridgeClient.Common.Attributes;
using RosbridgeNet.RosbridgeClient.ProtocolV2.Generics;
using System.Net.WebSockets;
using System.Text;

namespace BlazorRosJs.Components.Pages
{
    public partial class Odom : IDisposable
    {
        private string? positionXFormatted, positionYFormatted, positionZFormatted;
        private string? orientationXFormatted, orientationYFormatted, orientationZFormatted, orientationWFormatted;
        private string? linearXFormatted, angularZFormatted;


        private double positionX, positionY, positionZ;
        private double orientationX, orientationY, orientationZ, orientationW;
        private double linearX, angularZ;


        private static Odom? instance;

        private ClientWebSocket rosSocket = new ClientWebSocket();

        public float a = 0.2f;
        public float b = 0.5f;

        private MovementHandler? _movementHandler;
        private KeyPressHandler? _keyPressHandler;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                instance = this;
                await InitializeBlazor();
                await ConnectToRos();
                await MapViewer();
                var publisher = new RosPublisher<Twist>(MessageDispatcher, "/cmd_vel");
                await publisher.AdvertiseAsync();

                _movementHandler = new MovementHandler(publisher, a, b);
                _keyPressHandler = new KeyPressHandler(_movementHandler);
            }
        }

        private async Task InitializeBlazor()
        {
            await JSRuntime.InvokeVoidAsync("console.log", "Blazor initialized");
        }

        private async Task ConnectToRos()
        {
            await JSRuntime.InvokeVoidAsync("connectToRos", "ws://192.168.137.109:9090");
        }

        private async Task MapViewer()
        {
            string robotImageUrl = "/rb.png";
            await JSRuntime.InvokeVoidAsync("initMap", robotImageUrl);

        }

        [JSInvokable]
        public static Task OnRosConnected()
        {
            Console.WriteLine("Connected to ROS from Blazor.");
            return Task.CompletedTask;
        }

        [JSInvokable]
        public static Task OnRosError(string errorMessage)
        {
            Console.WriteLine($"Error: {errorMessage}");
            return Task.CompletedTask;
        }

        [JSInvokable]
        public static Task OnRosClosed()
        {
            Console.WriteLine("Disconnected from ROS.");
            return Task.CompletedTask;
        }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await ConnectRosbridge();
        }

        private async Task ConnectRosbridge()
        {
            try
            {
                Uri serverUri = new Uri("ws://192.168.137.109:9090");
                await rosSocket.ConnectAsync(serverUri, CancellationToken.None);

                string subscribeOdomRequest = "{\"op\":\"subscribe\",\"topic\":\"/odom\"}";
                byte[] subscribeOdomRequestBytes = Encoding.UTF8.GetBytes(subscribeOdomRequest);
                await rosSocket.SendAsync(new ArraySegment<byte>(subscribeOdomRequestBytes), WebSocketMessageType.Text, true, CancellationToken.None);

                string subscribeCmdVelRequest = "{\"op\":\"subscribe\",\"topic\":\"/cmd_vel\"}";
                byte[] subscribeCmdVelRequestBytes = Encoding.UTF8.GetBytes(subscribeCmdVelRequest);
                await rosSocket.SendAsync(new ArraySegment<byte>(subscribeCmdVelRequestBytes), WebSocketMessageType.Text, true, CancellationToken.None);

                _ = ReceiveMessages();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to Rosbridge: {ex.Message}");
            }
        }

        private async Task ReceiveMessages()
        {
            try
            {
                while (rosSocket.State == WebSocketState.Open)
                {
                    var buffer = new byte[4096];
                    var result = await rosSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.Count > 0)
                    {
                        string receivedData = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        ExtractMessage(receivedData);
                        await InvokeAsync(StateHasChanged);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiving data: {ex.Message}");
            }
        }

        private void ExtractMessage(string jsonData)
        {
            try
            {
                var jObject = JObject.Parse(jsonData);
                var topic = jObject["topic"]?.Value<string>();
                if (topic == "/odom")
                {
                    var position = jObject["msg"]?["pose"]?["pose"]?["position"];
                    var orientation = jObject["msg"]?["pose"]?["pose"]?["orientation"];

                    if (position != null)
                    {
                        positionX = position.Value<double>("x");
                        positionY = position.Value<double>("y");
                        positionZ = position.Value<double>("z");
                        positionXFormatted = positionX.ToString("F2");
                        positionYFormatted = positionY.ToString("F2");
                        positionZFormatted = positionZ.ToString("F2");
                    }

                    if (orientation != null)
                    {
                        orientationX = orientation.Value<double>("x");
                        orientationY = orientation.Value<double>("y");
                        orientationZ = orientation.Value<double>("z");
                        orientationW = orientation.Value<double>("w");
                        orientationXFormatted = orientationX.ToString("F2");
                        orientationYFormatted = orientationY.ToString("F2");
                        orientationZFormatted = orientationZ.ToString("F2");
                        orientationWFormatted = orientationW.ToString("F2");
                    }
                }
                else if (topic == "/cmd_vel")
                {
                    var linear = jObject["msg"]?["linear"];
                    var angular = jObject["msg"]?["angular"];

                    if (linear != null && angular != null)
                    {
                        linearX = linear.Value<double>("x");
                        angularZ = angular.Value<double>("z");
                        linearXFormatted = linearX.ToString("F2");
                        angularZFormatted = angularZ.ToString("F2");
                        //Console.WriteLine($"cmd_vel - Linear: x={linear["x"]}, y={linear["y"]}, z={linear["z"]}");
                        //Console.WriteLine($"cmd_vel - Angular: x={angular["x"]}, y={angular["y"]}, z={angular["z"]}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting message: {ex.Message}");
            }
        }

        private async Task HandleKeyPress(KeyboardEventArgs e)
        {
            if (_keyPressHandler != null)
            {
                await _keyPressHandler.HandleKeyPress(e);
            }
        }

        [RosMessageType("nav_msgs/msgs/Odometry")]
        public class Pose
        {
            public VectorO? position { get; set; }
            public VectorO? orientation { get; set; }

            public override string ToString()
            {
                return $"linear: {position}, angular: {orientation}";
            }
        }

        public class VectorO
        {
            public float x { get; set; }
            public float y { get; set; }
            public float z { get; set; }
            public float w { get; set; }

            public override string ToString()
            {
                return $"x: {x}, y: {y}, z: {z}, w: {w}";
            }
        }

        [RosMessageType("geometry_msgs/Twist")]
        public class Twist
        {
            public Vector? linear { get; set; }
            public Vector? angular { get; set; }

            public override string ToString()
            {
                return $"linear: {linear}, angular: {angular}";
            }
        }

        public class Vector
        {
            public float x { get; set; }
            public float y { get; set; }
            public float z { get; set; }

            public override string ToString()
            {
                return $"x: {x}, y: {y}, z: {z}";
            }
        }

        private void Test()
        {
            Console.WriteLine("test");
        }

        public void Dispose()
        {
            // Dọn dẹp tài nguyên nếu cần
        }

        private async Task MoveForward()
        {
            if (_movementHandler != null)
            {
                await _movementHandler.MoveForward();
            }
        }

        private async Task MoveForwardLeft() => await _movementHandler?.MoveForwardLeft();
        private async Task MoveForwardRight() => await _movementHandler?.MoveForwardRight();
        private async Task Stop() => await _movementHandler?.Stop();
        private async Task TurnClockwise() => await _movementHandler?.TurnClockwise();
        private async Task TurnCounterClockwise() => await _movementHandler?.TurnCounterClockwise();
        private async Task MoveBackward() => await _movementHandler?.MoveBackward();
        private async Task MoveBackwardLeft() => await _movementHandler?.MoveBackwardLeft();
        private async Task MoveBackwardRight() => await _movementHandler?.MoveBackwardRight();

    }
}
