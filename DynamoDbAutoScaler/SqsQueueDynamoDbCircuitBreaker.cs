using Amazon.SQS;
using Amazon.SQS.Model;
using DynamoDbAutoscaler.Interfaces;
using Serilog;
using System;
using System.Threading.Tasks;
namespace DynamoDbAutoscaler
{
    public class SqsQueueDynamoDbCircuitBreaker : ICircuitBreaker
    {
        private readonly ILogger structuredLogger;
        private readonly IAmazonSQS queueClient;
        private readonly int breakerTripQueueSize;
        private readonly string queueName;

        public SqsQueueDynamoDbCircuitBreaker(IAmazonSQS queueClient, string queueName, int breakerTripQueueSize, ILogger structuredLogger)
        {
            this.structuredLogger = structuredLogger;
            this.breakerTripQueueSize = breakerTripQueueSize;
            this.queueClient = queueClient;
            this.queueName = queueName;
        }

        public async Task<bool> IsTrippedAsync()
        {
            var sqsCount = await GetSqsCount();

            if (sqsCount < breakerTripQueueSize)
            {
                structuredLogger.Debug("Queue size {QueueSize} is under {TripSize}", sqsCount, breakerTripQueueSize);
                return false;
            }

            structuredLogger.Warning("SqsQueueDynamoDbCircuitBreaker is open! queue size: {QueueSize} trip size: {TripSize}",
                sqsCount, breakerTripQueueSize);
            return true;
        }

        private async Task<int> GetSqsCount()
        {
            var url = queueClient.GetQueueUrl(queueName);
            var request = new GetQueueAttributesRequest { QueueUrl = url.QueueUrl };
            request.AttributeNames.Add("ApproximateNumberOfMessages");
            request.AttributeNames.Add("ApproximateNumberOfMessagesNotVisible");

            var response = await queueClient.GetQueueAttributesAsync(request);
            return response == null ? 0 : response.ApproximateNumberOfMessages + response.ApproximateNumberOfMessagesNotVisible;
        }
    }
}