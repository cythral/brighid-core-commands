using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Amazon.CloudFormation;
using Amazon.CloudFormation.Model;

namespace Brighid.Commands.Cicd.Utils
{
    /// <summary>
    /// Utility for deploying cloudformation stacks.
    /// </summary>
    public class StackDeployer
    {
        private readonly IAmazonCloudFormation cloudformation = new AmazonCloudFormationClient();

        /// <summary>
        /// Deploys a cloudformation stack.
        /// </summary>
        /// <param name="context">Context holder for deployment information.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>The resulting task.</returns>
        public async Task<Dictionary<string, string>> Deploy(DeployContext context, CancellationToken cancellationToken)
        {
            var (stackId, changeSetId) = await CreateChangeSet(context, cancellationToken);

            try
            {
                await WaitForChangeSetCreate(stackId, context, cancellationToken);
                await ExecuteChangeSet(stackId, context, cancellationToken);
                await WaitForChangeSetExecute(stackId, context, cancellationToken);
            }
            catch (NoUpdatesException)
            {
                Console.WriteLine("Stack already up-to-date.");
                await DeleteChangeSet(changeSetId, cancellationToken);
            }

            return await GetOutputs(stackId, cancellationToken);
        }

        private async Task<(string StackId, string ChangeSetId)> CreateChangeSet(DeployContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var parameters = context.Parameters
                .Select((entry) => new Parameter { ParameterKey = entry.Key, ParameterValue = entry.Value })
                .ToList();

            var tags = context.Tags
                .Select((entry) => new Tag { Key = entry.Key, Value = entry.Value })
                .ToList();

            var request = new CreateChangeSetRequest
            {
                StackName = context.StackName,
                ChangeSetName = context.ChangeSetName,
                ChangeSetType = await GetChangeSetType(context, cancellationToken),
                Capabilities = context.Capabilities,
                TemplateBody = context.TemplateBody,
                TemplateURL = context.TemplateURL,
                Parameters = parameters,
                Tags = tags,
            };

            var response = await cloudformation.CreateChangeSetAsync(request, cancellationToken);
            return (response.StackId, response.Id);
        }

        private async Task WaitForChangeSetCreate(string stackId, DeployContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Console.WriteLine("Waiting for changeset creation to complete...");
            var status = ChangeSetStatus.CREATE_PENDING;
            var reason = string.Empty;

            while (status != ChangeSetStatus.CREATE_COMPLETE)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (status == ChangeSetStatus.FAILED)
                {
                    var exception = reason.Contains("The submitted information didn't contain changes.") || reason.Contains("No updates are to be performed")
                        ? new NoUpdatesException()
                        : new Exception($"Change set {context.ChangeSetName} failed: {reason}");

                    throw exception;
                }

                await Task.Delay(1000, cancellationToken);
                var request = new DescribeChangeSetRequest { ChangeSetName = context.ChangeSetName, StackName = stackId };
                var response = await cloudformation.DescribeChangeSetAsync(request, cancellationToken);
                status = response.Status;
                reason = response.StatusReason;
            }
        }

        private async Task<ChangeSetType> GetChangeSetType(DeployContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var request = new DescribeStacksRequest { StackName = context.StackName };
                var response = await cloudformation.DescribeStacksAsync(request, cancellationToken);

                return response.Stacks.FirstOrDefault()?.StackStatus != StackStatus.REVIEW_IN_PROGRESS
                    ? ChangeSetType.UPDATE
                    : ChangeSetType.CREATE;
            }
            catch
            {
                return ChangeSetType.CREATE;
            }
        }

        private async Task ExecuteChangeSet(string stackId, DeployContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var request = new ExecuteChangeSetRequest { ChangeSetName = context.ChangeSetName, StackName = stackId };
            await cloudformation.ExecuteChangeSetAsync(request, cancellationToken);
        }

        private async Task WaitForChangeSetExecute(string stackId, DeployContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Console.WriteLine("Executing changeset...\n");
            var stackStatus = ResourceStatus.CREATE_IN_PROGRESS;
            var lastEventId = (string?)null;

            while (stackStatus != ResourceStatus.CREATE_COMPLETE && stackStatus != ResourceStatus.UPDATE_COMPLETE)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Delay(1000, cancellationToken);

                var events = await GetStackEvents(lastEventId, stackId, context, cancellationToken);
                foreach (var @event in events)
                {
                    var header = $"[{@event.LogicalResourceId}]: {@event.ResourceStatus}\n";
                    header += string.Join('\0', Enumerable.Repeat('-', header.Length));
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(header);
                    Console.ResetColor();
                    Console.WriteLine(@event.ResourceStatusReason);
                    Console.WriteLine();

                    lastEventId = @event.EventId;
                    if (@event.ResourceType == "AWS::CloudFormation::Stack" && @event.LogicalResourceId == context.StackName)
                    {
                        stackStatus = @event.ResourceStatus;
                    }
                }

                if (stackStatus.ToString().EndsWith("_FAILED") || stackStatus.ToString().EndsWith("ROLLBACK_COMPLETE"))
                {
                    throw new Exception("Deployment failed.");
                }
            }
        }

        private async Task DeleteChangeSet(string changeSetId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var request = new DeleteChangeSetRequest { ChangeSetName = changeSetId };
            await cloudformation.DeleteChangeSetAsync(request, cancellationToken);
        }

        private async Task<List<StackEvent>> GetStackEvents(string? lastEventId, string stackId, DeployContext context, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var request = new DescribeStackEventsRequest { StackName = stackId };
            var response = await cloudformation.DescribeStackEventsAsync(request, cancellationToken);
            var relevantEvents = new List<StackEvent>();

            foreach (var @event in response.StackEvents)
            {
                if (@event.EventId == lastEventId || @event.Timestamp < context.Timestamp)
                {
                    break;
                }

                relevantEvents.Add(@event);
            }

            relevantEvents.Reverse();
            return relevantEvents;
        }

        private async Task<Dictionary<string, string>> GetOutputs(string stackId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var request = new DescribeStacksRequest { StackName = stackId };
            var response = await cloudformation.DescribeStacksAsync(request, cancellationToken);
            var stack = response.Stacks.First();
            return stack.Outputs.ToDictionary(output => output.OutputKey, output => output.OutputValue);
        }
    }
}
