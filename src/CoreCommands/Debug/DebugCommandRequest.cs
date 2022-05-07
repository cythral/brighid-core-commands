using Brighid.Commands.Sdk;

namespace Brighid.Commands.CoreCommands.Debug
{
    /// <summary>
    /// Request for a debug command.
    /// </summary>
    public class DebugCommandRequest
    {
        /// <summary>
        /// Gets or sets the message to echo.
        /// </summary>
        [Argument(0, Description = "Value indicating whether to turn debug mode on or off.")]
        public string Mode { get; set; } = string.Empty;
    }
}
