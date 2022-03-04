using System.Threading;
using System.Threading.Tasks;

using Brighid.Commands.Sdk;

using Microsoft.Extensions.Logging;

namespace Brighid.Commands.CoreCommands.Ping
{
    /// <summary>
    /// Ping command.
    /// </summary>
    [Command("ping")]
    public class PingCommand : ICommand<PingCommandRequest>
    {
        private readonly ILogger<PingCommand> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PingCommand"/> class.
        /// </summary>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        public PingCommand(
            ILogger<PingCommand> logger
        )
        {
            this.logger = logger;
        }

        /// <inheritdoc />
        public Task<CommandResult> Run(CommandContext<PingCommandRequest> context, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Received command request: {@context}", context);
            return Task.FromResult(new CommandResult("Pong2"));
        }
    }
}
