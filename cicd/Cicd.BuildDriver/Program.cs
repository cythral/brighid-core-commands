using System.Collections.Generic;

using Brighid.Commands.Cicd.BuildDriver;
using Brighid.Commands.Cicd.Utils;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

#pragma warning disable SA1516

await Microsoft.Extensions.Hosting.Host
.CreateDefaultBuilder()
.ConfigureAppConfiguration(configure =>
{
    configure.AddCommandLine(args, new Dictionary<string, string>
    {
        ["--version"] = "CommandLineOptions:Version",
    });
})
.ConfigureServices((context, services) =>
{
    services.Configure<CommandLineOptions>(context.Configuration.GetSection("CommandLineOptions"));
    services.AddSingleton<IHost, Brighid.Commands.Cicd.BuildDriver.Host>();
    services.AddSingleton<EcrUtils>();
})
.UseConsoleLifetime()
.Build()
.RunAsync();
