using Amazon.SimpleEmail;

using Brighid.Commands.Sdk;

using Microsoft.Extensions.DependencyInjection;

namespace Brighid.Commands.CoreCommands.Email
{
    /// <inheritdoc />
    public class EmailCommandStartup : ICommandStartup
    {
        /// <inheritdoc />
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IAmazonSimpleEmailService, AmazonSimpleEmailServiceClient>();
        }
    }
}
