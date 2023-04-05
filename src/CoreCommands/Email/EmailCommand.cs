using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
        private readonly IEmailServiceFactory emailServiceFactory;
        private readonly ILogger<EmailCommand> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailCommand"/> class.
        /// </summary>
        /// <param name="emailServiceFactory">Service used for sending emails.</param>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        public EmailCommand(
            IEmailServiceFactory emailServiceFactory,
            ILogger<EmailCommand> logger
        )
        {
            this.emailServiceFactory = emailServiceFactory;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task<CommandResult> Run(CommandContext<EmailCommandRequest> context, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation("Received email request: {@to} {@message}", context.Input.To, context.Input.Message);

            var destination = new Destination { ToAddresses = new List<string> { context.Input.To } };
            var subject = new Content { Data = context.Input.Subject };
            var content = new Content { Data = context.Input.Message };
            var request = new SendEmailRequest
            {
                Source = "system@brigh.id",
                Destination = destination,
                ReplyToAddresses = new List<string> { context.Input.From },
                Message = new Message
                {
                    Subject = subject,
                    Body = new Body { Text = content },
                },
            };

            logger.LogDebug("Sending email via SES");
            var emailerRoleArn = Environment.GetEnvironmentVariable("EmailCommand__EmailerRoleArn") ?? throw new Exception("Could not find emailer role ARN.");
            var emailService = await emailServiceFactory.Create(emailerRoleArn);
            var result = await emailService.SendEmailAsync(request, cancellationToken);
            logger.LogInformation("Send email with message ID: {@messageId}", result.MessageId);

            return new CommandResult("Successfully sent the message.");
        }
    }
}
