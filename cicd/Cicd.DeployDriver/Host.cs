using System;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Commands.Cicd.Utils;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Brighid.Commands.Cicd.DeployDriver
{
    /// <inheritdoc />
    public class Host : IHost
    {
        private readonly StackDeployer deployer;
        private readonly CommandLineOptions options;
        private readonly IHostApplicationLifetime lifetime;

        /// <summary>
        /// Initializes a new instance of the <see cref="Host" /> class.
        /// </summary>
        /// <param name="deployer">Service for deploying cloudformation stacks.</param>
        /// <param name="options">Command line options.</param>
        /// <param name="lifetime">Service that controls the application lifetime.</param>
        /// <param name="serviceProvider">Object that provides access to the program's services.</param>
        public Host(
            StackDeployer deployer,
            IOptions<CommandLineOptions> options,
            IHostApplicationLifetime lifetime,
            IServiceProvider serviceProvider
        )
        {
            this.deployer = deployer;
            this.options = options.Value;
            this.lifetime = lifetime;
            Services = serviceProvider;
        }

        /// <inheritdoc />
        public IServiceProvider Services { get; }

        /// <inheritdoc />
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await Step($"Deploy template to {options.Environment}", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var context = new DeployContext
                {
                    StackName = "brighid-core-commands",
                    TemplateURL = $"https://{options.ArtifactsLocation!.Host}.s3.amazonaws.com{options.ArtifactsLocation!.AbsolutePath}/template.yml",
                    Capabilities = { "CAPABILITY_AUTO_EXPAND" },
                };

                await deployer.Deploy(context, cancellationToken);
            });

            lifetime.StopApplication();
        }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        private static async Task Step(string title, Func<Task> action)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\n{title} ==========\n");
            Console.ResetColor();

            await action();
        }
    }
}
