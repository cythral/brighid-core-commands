using Brighid.Commands.Sdk;
using Brighid.Identity.Client;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Brighid.Commands.CoreCommands.Unlink
{
    /// <inheritdoc />
    public class UnlinkCommandStartup : ICommandStartup
    {
        /// <inheritdoc />
        public void ConfigureServices(IServiceCollection services)
        {
            var configuration = new ConfigurationBuilder().AddEnvironmentVariables().Build();
            services.ConfigureBrighidIdentity<IdentityConfig>(configuration.GetSection("Identity"));
            services.UseBrighidIdentityLoginProviders();
        }
    }
}
