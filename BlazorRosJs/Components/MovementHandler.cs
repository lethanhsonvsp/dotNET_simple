using RosbridgeNet.RosbridgeClient.ProtocolV2.Generics;
using System.Threading.Tasks;
using static BlazorRosJs.Components.Pages.Odom;

namespace BlazorRosJs.Components
{
    public class MovementHandler
    {
        private readonly RosPublisher<Twist> _publisher;
        private readonly float _linearSpeed;
        private readonly float _angularSpeed;

        public MovementHandler(RosPublisher<Twist> publisher, float linearSpeed, float angularSpeed)
        {
            _publisher = publisher;
            _linearSpeed = linearSpeed;
            _angularSpeed = angularSpeed;
        }

        public async Task MoveForward()
        {
            await PublishMessage(new Twist()
            {
                linear = new Vector() { x = _linearSpeed, y = 0, z = 0 },
                angular = new Vector() { x = 0, y = 0, z = 0 }
            });
        }

        public async Task MoveForwardLeft()
        {
            await PublishMessage(new Twist()
            {
                linear = new Vector() { x = _linearSpeed, y = 0, z = 0 },
                angular = new Vector() { x = 0, y = 0, z = _angularSpeed }
            });
        }

        public async Task MoveForwardRight()
        {
            await PublishMessage(new Twist()
            {
                linear = new Vector() { x = _linearSpeed, y = 0, z = 0 },
                angular = new Vector() { x = 0, y = 0, z = -_angularSpeed }
            });
        }

        public async Task Stop()
        {
            await PublishMessage(new Twist()
            {
                linear = new Vector() { x = 0, y = 0, z = 0 },
                angular = new Vector() { x = 0, y = 0, z = 0 }
            });
        }

        public async Task TurnClockwise()
        {
            await PublishMessage(new Twist()
            {
                linear = new Vector() { x = 0, y = 0, z = 0 },
                angular = new Vector() { x = 0, y = 0, z = -_angularSpeed }
            });
        }

        public async Task TurnCounterClockwise()
        {
            await PublishMessage(new Twist()
            {
                linear = new Vector() { x = 0, y = 0, z = 0 },
                angular = new Vector() { x = 0, y = 0, z = _angularSpeed }
            });
        }

        public async Task MoveBackward()
        {
            await PublishMessage(new Twist()
            {
                linear = new Vector() { x = -_linearSpeed, y = 0, z = 0 },
                angular = new Vector() { x = 0, y = 0, z = 0 }
            });
        }

        public async Task MoveBackwardLeft()
        {
            await PublishMessage(new Twist()
            {
                linear = new Vector() { x = -_linearSpeed, y = 0, z = 0 },
                angular = new Vector() { x = 0, y = 0, z = _angularSpeed }
            });
        }

        public async Task MoveBackwardRight()
        {
            await PublishMessage(new Twist()
            {
                linear = new Vector() { x = -_linearSpeed, y = 0, z = 0 },
                angular = new Vector() { x = 0, y = 0, z = -_angularSpeed }
            });
        }

        private async Task PublishMessage(Twist twist)
        {
            await _publisher.PublishAsync(twist);
        }
    }
}
