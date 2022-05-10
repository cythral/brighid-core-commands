using System;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Commands.Sdk;
using Brighid.Identity.Client;

using Microsoft.Extensions.Logging;

namespace Brighid.Commands.CoreCommands.Debug
{
    /// <summary>
    /// Ping command.
    /// </summary>
    [Command("debug", StartupType = typeof(DebugCommandStartup))]
    [RequiresScope("token")]
    public class DebugCommand : ICommand<DebugCommandRequest>
    {
        private readonly IUsersClientFactory usersClientFactory;
        private readonly ILogger<DebugCommand> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="DebugCommand"/> class.
        /// </summary>
        /// <param name="usersClientFactory">Factory class for creating clients for the Brighid Identity Users API.</param>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        public DebugCommand(
            IUsersClientFactory usersClientFactory,
            ILogger<DebugCommand> logger
        )
        {
            this.usersClientFactory = usersClientFactory;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task<CommandResult> Run(CommandContext<DebugCommandRequest> context, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation("Received command request: {@mode}", context.Input.Mode);

            try
            {
                var mode = context.Input.Mode switch
                {
                    "on" => true,
                    "off" => false,
                    _ => throw new NotSupportedException(),
                };

                var usersClient = usersClientFactory.Create(context.Token);
                var userId = Guid.Parse(context.Principal.Identity!.Name!);

                logger.LogInformation("Setting debug mode for user {@userId} to {@mode}", userId, mode);
                await usersClient.SetDebugMode(userId, mode, cancellationToken);
                return new CommandResult($"Alright, I've turned debug mode {context.Input.Mode}");
            }
            catch (NotSupportedException)
            {
                return new CommandResult($"{context.Input.Mode} is not a valid argument for this command.  Please specify either \"on\", or \"off\".");
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error setting debug mode {@mode} for user {@user}", context.Input.Mode, context.Principal.Identity!.Name);
                return new CommandResult($"Something's wrong, I wasn't able to set debug mode to {context.Input.Mode} for you.");
            }
        }
    }
}
