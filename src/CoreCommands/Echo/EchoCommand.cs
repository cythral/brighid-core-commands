using System.Threading;
using System.Threading.Tasks;

using Brighid.Commands.Sdk;

using Microsoft.Extensions.Logging;

namespace Brighid.Commands.CoreCommands.Echo
{
    /// <summary>
    /// Ping command.
    /// </summary>
    [Command("echo")]
    public class EchoCommand : ICommand<EchoCommandRequest>
    {
        private readonly ILogger<EchoCommand> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="EchoCommand"/> class.
        /// </summary>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        public EchoCommand(
            ILogger<EchoCommand> logger
        )
        {
            this.logger = logger;
        }

        /// <inheritdoc />
        public Task<CommandResult> Run(CommandContext<EchoCommandRequest> context, CancellationToken cancellationToken = default)
        {
            logger.LogInformation("Received command request: {@context}", context);
            var result = new CommandResult(context.Input.Message);
            return Task.FromResult(result);
        }
    }
}
