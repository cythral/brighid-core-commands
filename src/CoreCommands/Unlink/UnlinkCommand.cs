using System.Threading;
using System.Threading.Tasks;

using Brighid.Commands.Sdk;
using Brighid.Identity.Client;

using Microsoft.Extensions.Logging;

namespace Brighid.Commands.CoreCommands.Unlink
{
    /// <summary>
    /// Ping command.
    /// </summary>
    [Command("unlink", StartupType = typeof(UnlinkCommandStartup))]
    [RequiresScope("token")]
    public class UnlinkCommand : ICommand<UnlinkCommandRequest>
    {
        private readonly ILogger<UnlinkCommand> logger;
        private readonly ILoginProvidersClientFactory loginProvidersFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnlinkCommand"/> class.
        /// </summary>
        /// <param name="loginProvidersFactory">Factory for creating login providers clients.</param>
        /// <param name="logger">Logger used to log info to some destination(s).</param>
        public UnlinkCommand(
            ILoginProvidersClientFactory loginProvidersFactory,
            ILogger<UnlinkCommand> logger
        )
        {
            this.loginProvidersFactory = loginProvidersFactory;
            this.logger = logger;
        }

        /// <inheritdoc />
        public async Task<CommandResult> Run(CommandContext<UnlinkCommandRequest> context, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            logger.LogInformation("Received command request: {@context}", context);
            logger.LogInformation("Deleting user login for provider {@provider} with ID {@id}", context.SourceSystem, context.SourceSystemUser);

            var client = loginProvidersFactory.Create(context.Token);
            await client.DeleteLogin(context.SourceSystem, context.SourceSystemUser, cancellationToken);
            return new CommandResult($"Successfully unlinked your {context.SourceSystem} account.");
        }
    }
}
