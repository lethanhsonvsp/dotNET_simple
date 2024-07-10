using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlazorRosJs.Components.Pages
{
    public partial class Home
    {
        private bool connectionStatus = false;
        private double linearX, linearY, linearZ;
        private double angularX, angularY, angularZ;

        private double positionX, positionY, positionZ;
        private double orientationX, orientationY, orientationZ, orientationW;

        private string? positionXFormatted, positionYFormatted, positionZFormatted;
        private string? orientationXFormatted, orientationYFormatted, orientationZFormatted, orientationWFormatted;

        private static Home? instance;

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                instance = this;
                await InitializeBlazor();
                await ConnectToRos();
                await SubscribeToOdom();
                await MapViewer();
            }
        }

        private async Task InitializeBlazor()
        {
            await JSRuntime.InvokeVoidAsync("console.log", "Blazor initialized");
        }

        private async Task ConnectToRos()
        {
            await JSRuntime.InvokeVoidAsync("connectToRos", "ws://192.168.137.218:9090");
        }

        private async Task PublishCmdVel()
        {
            var linear = new { x = linearX, y = linearY, z = linearZ };
            var angular = new { x = angularX, y = angularY, z = angularZ };
            await JSRuntime.InvokeVoidAsync("publishTopic", "/cmd_vel", "geometry_msgs/Twist", new { linear, angular });
        }

        private async Task SubscribeToOdom()
        {
            await JSRuntime.InvokeVoidAsync("subscribeTopic", "/odom", "nav_msgs/Odometry", nameof(OnOdomReceived));
        }

        private async Task MapViewer()
        {
            await JSRuntime.InvokeVoidAsync("initMap");
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

        [JSInvokable]
        public static async Task OnOdomReceived(string message)
        {
            try
            {
                instance?.ExtractMessage(message);
                await instance?.InvokeAsync(instance.StateHasChanged);
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
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting message: {ex.Message}");
            }
        }

        private async Task PublishMessage(Twist twist)
        {
            linearX = twist.linear.x;
            linearY = twist.linear.y;
            linearZ = twist.linear.z;
            angularX = twist.angular.x;
            angularY = twist.angular.y;
            angularZ = twist.angular.z;
            await PublishCmdVel();
        }

        private async Task HandleKeyPress(KeyboardEventArgs e)
        {
            switch (e.Key)
            {
                case "i":
                    await MoveForward();
                    break;
                case "u":
                    await MoveForwardLeft();
                    break;
                case "o":
                    await MoveForwardRight();
                    break;
                case "k":
                    await Stop();
                    break;
                case "j":
                    await TurnCounterClockwise();
                    break;
                case "l":
                    await TurnClockwise();
                    break;
                case ",":
                    await MoveBackward();
                    break;
                case "m":
                    await MoveBackwardLeft();
                    break;
                case ".":
                    await MoveBackwardRight();
                    break;
            }
        }

        public float a = 0.2f;
        public float b = 0.5f;

        private async Task MoveForward()
        {
            await PublishMessage(new Twist()
            {
                linear = new Vector() { x = a, y = 0, z = 0 },
                angular = new Vector() { x = 0, y = 0, z = 0 }
            });
        }

        private async Task MoveForwardLeft()
        {
            await PublishMessage(new Twist()
            {
                linear = new Vector() { x = a, y = 0, z = 0 },
                angular = new Vector() { x = 0, y = 0, z = b }
            });
        }

        private async Task MoveForwardRight()
        {
            await PublishMessage(new Twist()
            {
                linear = new Vector() { x = a, y = 0, z = 0 },
                angular = new Vector() { x = 0, y = 0, z = -b }
            });
        }

        private async Task Stop()
        {
            await PublishMessage(new Twist()
            {
                linear = new Vector() { x = 0, y = 0, z = 0 },
                angular = new Vector() { x = 0, y = 0, z = 0 }
            });
        }

        private async Task TurnClockwise()
        {
            await PublishMessage(new Twist()
            {
                linear = new Vector() { x = 0, y = 0, z = 0 },
                angular = new Vector() { x = 0, y = 0, z = -b }
            });
        }

        private async Task TurnCounterClockwise()
        {
            await PublishMessage(new Twist()
            {
                linear = new Vector() { x = 0, y = 0, z = 0 },
                angular = new Vector() { x = 0, y = 0, z = b }
            });
        }

        private async Task MoveBackward()
        {
            await PublishMessage(new Twist()
            {
                linear = new Vector() { x = -a, y = 0, z = 0 },
                angular = new Vector() { x = 0, y = 0, z = 0 }
            });
        }

        private async Task MoveBackwardLeft()
        {
            await PublishMessage(new Twist()
            {
                linear = new Vector() { x = -a, y = 0, z = 0 },
                angular = new Vector() { x = 0, y = 0, z = b }
            });
        }

        private async Task MoveBackwardRight()
        {
            await PublishMessage(new Twist()
            {
                linear = new Vector() { x = -a, y = 0, z = 0 },
                angular = new Vector() { x = 0, y = 0, z = -b }
            });
        }

        public void Dispose()
        {
            // Dispose resources if needed
        }

        public class Twist
        {
            public Vector? linear { get; set; }
            public Vector? angular { get; set; }
        }

        public class Vector
        {
            public double x { get; set; }
            public double y { get; set; }
            public double z { get; set; }
        }
    }
}
