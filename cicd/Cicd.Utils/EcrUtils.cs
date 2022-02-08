using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Amazon.ECR;
using Amazon.ECRPublic;

namespace Brighid.Commands.Cicd.Utils
{
    /// <summary>
    /// Utilities for interacting with ECR.
    /// </summary>
    public class EcrUtils
    {
        private readonly IAmazonECR ecr = new AmazonECRClient();
        private readonly IAmazonECRPublic ecrPublic = new AmazonECRPublicClient();

        /// <summary>
        /// Logs into the given ECR repository.
        /// </summary>
        /// <param name="repository">The repository to login to.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        public async Task DockerLogin(string repository, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = await ecr.GetAuthorizationTokenAsync(new(), cancellationToken);
            var token = response.AuthorizationData.ElementAt(0);
            var passwordBytes = Convert.FromBase64String(token.AuthorizationToken);
            var password = Encoding.ASCII.GetString(passwordBytes)[4..];

            await Login(repository, password, cancellationToken);
        }

        /// <summary>
        /// Logs into ECR Public.
        /// </summary>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        public async Task PublicDockerLogin(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var response = await ecrPublic.GetAuthorizationTokenAsync(new(), cancellationToken);
            var token = response.AuthorizationData;
            var passwordBytes = Convert.FromBase64String(token.AuthorizationToken);
            var password = Encoding.ASCII.GetString(passwordBytes)[4..];

            await Login("public.ecr.aws", password, cancellationToken);
        }

        private static async Task Login(string repository, string password, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var command = new Command(
                command: "docker login",
                options: new Dictionary<string, object>
                {
                    ["--username"] = "AWS",
                    ["--password-stdin"] = true,
                },
                arguments: new[] { repository }
            );

            await command.RunOrThrowError(
                errorMessage: "Failed to login to ECR.",
                input: password,
                cancellationToken: cancellationToken
            );
        }
    }
}
