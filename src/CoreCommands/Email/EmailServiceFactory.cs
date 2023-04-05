using System.Threading.Tasks;

using Amazon.SecurityToken;
using Amazon.SecurityToken.Model;
using Amazon.SimpleEmail;

namespace Brighid.Commands.CoreCommands.Email
{
    /// <inheritdoc />
    public class EmailServiceFactory : IEmailServiceFactory
    {
        private readonly IAmazonSecurityTokenService securityTokenService;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailServiceFactory"/> class.
        /// </summary>
        /// <param name="securityTokenService">Security token service.</param>
        public EmailServiceFactory(IAmazonSecurityTokenService securityTokenService)
        {
            this.securityTokenService = securityTokenService;
        }

        /// <inheritdoc />
        public async Task<IAmazonSimpleEmailService> Create(string roleArn)
        {
            var request = new AssumeRoleRequest { RoleArn = roleArn, RoleSessionName = "BrighidEmailCommand" };
            var response = await securityTokenService.AssumeRoleAsync(request);
            return new AmazonSimpleEmailServiceClient(response.Credentials);
        }
    }
}
