using System.Threading.Tasks;

using Amazon.SimpleEmail;

namespace Brighid.Commands.CoreCommands.Email
{
    /// <summary>
    /// Email service.
    /// </summary>
    public interface IEmailServiceFactory
    {
        /// <summary>
        /// Creates an email service client with credentials of the assumed role (<paramref name="roleArn" />).
        /// </summary>
        /// <param name="roleArn">ARN of the role to assume.</param>
        /// <returns>The resulting email service.</returns>
        Task<IAmazonSimpleEmailService> Create(string roleArn);
    }
}
