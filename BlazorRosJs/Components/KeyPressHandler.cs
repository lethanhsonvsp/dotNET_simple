using Microsoft.AspNetCore.Components.Web;
using System.Threading.Tasks;

namespace BlazorRosJs.Components
{
    public class KeyPressHandler
    {
        private readonly MovementHandler _movementHandler;

        public KeyPressHandler(MovementHandler movementHandler)
        {
            _movementHandler = movementHandler;
        }

        public async Task HandleKeyPress(KeyboardEventArgs e)
        {
            switch (e.Key)
            {
                case "i":
                    await _movementHandler.MoveForward();
                    break;
                case "u":
                    await _movementHandler.MoveForwardLeft();
                    break;
                case "o":
                    await _movementHandler.MoveForwardRight();
                    break;
                case "k":
                    await _movementHandler.Stop();
                    break;
                case "j":
                    await _movementHandler.TurnCounterClockwise();
                    break;
                case "l":
                    await _movementHandler.TurnClockwise();
                    break;
                case ",":
                    await _movementHandler.MoveBackward();
                    break;
                case "m":
                    await _movementHandler.MoveBackwardLeft();
                    break;
                case ".":
                    await _movementHandler.MoveBackwardRight();
                    break;
            }
        }
    }
}
