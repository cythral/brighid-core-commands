using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Brighid.Commands.Artifacts;
using Brighid.Commands.Cicd.Utils;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Brighid.Commands.Cicd.BuildDriver
{
    /// <inheritdoc />
    public class Host : IHost
    {
        private static readonly string ConfigFile = ProjectRootDirectoryAttribute.ThisAssemblyProjectRootDirectory + "cicd/config.yml";
        private static readonly string IntermediateOutputDirectory = ProjectRootDirectoryAttribute.ThisAssemblyProjectRootDirectory + "obj/Cicd.Driver/";
        private static readonly string CicdOutputDirectory = ProjectRootDirectoryAttribute.ThisAssemblyProjectRootDirectory + "bin/Cicd/";
        private static readonly string OutputsFile = IntermediateOutputDirectory + "cdk.outputs.json";
        private readonly CommandLineOptions options;
        private readonly IHostApplicationLifetime lifetime;

        /// <summary>
        /// Initializes a new instance of the <see cref="Host" /> class.
        /// </summary>
        /// <param name="options">Command line options.</param>
        /// <param name="lifetime">Service that controls the application lifetime.</param>
        /// <param name="serviceProvider">Object that provides access to the program's services.</param>
        public Host(
            IOptions<CommandLineOptions> options,
            IHostApplicationLifetime lifetime,
            IServiceProvider serviceProvider
        )
        {
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
            Directory.CreateDirectory(CicdOutputDirectory);

            ArtifactsStackOutputs outputs = null!;

            await Step("Restore .NET Tools", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                Directory.SetCurrentDirectory(ProjectRootDirectoryAttribute.ThisAssemblyProjectRootDirectory);

                var command = new Command("dotnet tool restore");
                await command.RunOrThrowError(
                    errorMessage: "Could not restore .NET Tools.",
                    cancellationToken: cancellationToken
                );
            });

            await Step("Deploying Artifacts Stack", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                outputs = await ArtifactsStack.Deploy(cancellationToken);
            });

            await Step("Package Template", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var command = new Command(
                    command: "dotnet pack-brighid-commands",
                    argumentsBeforeOptions: true,
                    arguments: new[] { TemplateMetadataPathAttribute.ThisAssemblyTemplateMetadataPath },
                    options: new Dictionary<string, object>
                    {
                        ["--bucket-name"] = outputs.BucketName,
                        ["--destination"] = CicdOutputDirectory + "template.yml",
                    }
                );

                await command.RunOrThrowError(
                    errorMessage: "Could not package template.",
                    cancellationToken: cancellationToken
                );
            });

            await Step("Upload Artifacts to S3", async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                var command = new Command(
                    command: "aws s3 cp",
                    options: new Dictionary<string, object>
                    {
                        ["--recursive"] = true,
                    },
                    arguments: new[]
                    {
                        CicdOutputDirectory,
                        $"s3://{outputs.BucketName}/{options.Version}",
                    }
                );

                await command.RunOrThrowError(
                    errorMessage: "Could not upload artifacts to S3.",
                    cancellationToken: cancellationToken
                );
            });

            Console.WriteLine();
            Console.WriteLine($"::set-output name=artifacts-location::s3://{outputs.BucketName}/{options.Version}");
            Console.WriteLine($"::set-output name=version::{ThisAssembly.AssemblyVersion}");

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
