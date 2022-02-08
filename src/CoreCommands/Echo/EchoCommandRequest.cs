using Brighid.Commands.Sdk;

namespace Brighid.Commands.CoreCommands.Echo
{
    /// <summary>
    /// Request for a ping command.
    /// </summary>
    public class EchoCommandRequest
    {
        /// <summary>
        /// Gets or sets the message to echo.
        /// </summary>
        [Argument(0, Description = "The message to echo back to the user.")]
        public string Message { get; set; } = string.Empty;
    }
}
