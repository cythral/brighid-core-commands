using Brighid.Commands.Sdk;

namespace Brighid.Commands.CoreCommands.Email
{
    /// <summary>
    /// Request for a debug command.
    /// </summary>
    public class EmailCommandRequest
    {
        /// <summary>
        /// Gets or sets the email to send an email to.
        /// </summary>
        [Option(Description = "Email to send the email to.")]
        public string To { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the email of the person initiating the request.
        /// </summary>
        [Option(Description = "Email of the person initiating the request.")]
        public string From { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the subject of the message to send.
        /// </summary>
        [Option(Description = "Subject of the message to send.")]
        public string Subject { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the message to send to the to field.
        /// </summary>
        [Argument(0, Description = "Message to relay.")]
        public string Message { get; set; } = string.Empty;
    }
}
