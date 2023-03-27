using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;

using Brighid.Commands.Sdk;

using Microsoft.Extensions.Logging;

namespace Brighid.Commands.CoreCommands.Email
{
    /// <summary>
    /// Ping command.
    /// </summary>
    [Command("email", StartupType = typeof(EmailCommandStartup))]
    public class EmailCommand : ICommand<EmailCommandRequest>
    {
        private readonly IAmazonSimpleEmailService emailService;
        private readonly ILogger<EmailCommand> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailCommand"/> class.
        /// </summary>
        /// <param name="emailService">Service used for sending emails.</param>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        public EmailCommand(
            IAmazonSimpleEmailService emailService,
            ILogger<EmailCommand> logger
        )
        {
            this.emailService = emailService;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task<CommandResult> Run(CommandContext<EmailCommandRequest> context, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation("Received email request: {@to} {@from} {@message}", context.Input.To, context.Input.From, context.Input.Message);

            var destination = new Destination { ToAddresses = new List<string> { context.Input.To } };
            var subject = new Content { Data = context.Input.Subject };
            var content = new Content { Data = context.Input.Message };
            var request = new SendEmailRequest
            {
                Source = context.Input.From,
                Destination = destination,
                Message = new Message
                {
                    Subject = subject,
                    Body = new Body { Text = content },
                },
            };

            logger.LogInformation("Sending email via SES");
            var result = await emailService.SendEmailAsync(request, cancellationToken);
            logger.LogInformation("Send email with message ID: {@messageId}", result.MessageId);

            return new CommandResult("Successfully sent the message.");
        }
    }
}
