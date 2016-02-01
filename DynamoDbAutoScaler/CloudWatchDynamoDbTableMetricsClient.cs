using DynamoDbAutoscaler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamoDbAutoscaler.Models;
using Amazon.CloudWatch.Model;
using Amazon.CloudWatch;
using Amazon;
using System.Threading;

namespace DynamoDbAutoscaler
{
    public class CloudWatchDynamoDbTableMetricsClient : IDynamoDbTableMetricsClient
    {
        private const string DynamoDbMetricNamespace = "AWS/DynamoDB";
        private static readonly TimeSpan MetricTimeSpan = TimeSpan.FromMinutes(15);
        private readonly IAmazonCloudWatch client;

        private const string ProvisionedReadCapacityMetricName = "ProvisionedReadCapacityUnits";
        private const string ProvisionedWriteCapacityMetricName = "ProvisionedWriteCapacityUnits";
        private const string ConsumedReadCapacityMetricName = "ConsumedReadCapacityUnits";
        private const string ConsumedWriteCapacityMetricName = "ConsumedWriteCapacityUnits";
        private const string ReadThrottleEventsMetricName = "ReadThrottleEvents";
        private const string WriteThrottleEventsMetricName = "WriteThrottleEvents";

        public CloudWatchDynamoDbTableMetricsClient(IAmazonCloudWatch client)
        {
            this.client = client;
        }

        public CloudWatchDynamoDbTableMetricsClient() : this(AWSClientFactory.CreateAmazonCloudWatchClient())
        {
        }

        public Task<DynamoDbMetrics> GetGlobalSecondaryIndexMetricsAsync(string tableName, string indexName, CancellationToken cancellationToken)
        {
            return RetrieveTableMetrics(tableName, indexName, cancellationToken);
        }

        public Task<DynamoDbMetrics> GetTableMetricsAsync(string tableName, CancellationToken cancellationToken)
        {
            return RetrieveTableMetrics(tableName, null, cancellationToken);
        }

        private async Task<DynamoDbMetrics> RetrieveTableMetrics(string tableName, string indexName, CancellationToken cancellationToken)
        {
            var provisionedReadCapacity = GetMetricResponse(tableName, indexName, ProvisionedReadCapacityMetricName, cancellationToken);
            var provisionedWriteCapacity = GetMetricResponse(tableName, indexName, ProvisionedWriteCapacityMetricName, cancellationToken);
            var consumedReadCapacity = GetMetricResponse(tableName, indexName, ConsumedReadCapacityMetricName, cancellationToken);
            var consumedWriteCapacity = GetMetricResponse(tableName, indexName, ConsumedWriteCapacityMetricName, cancellationToken);
            var readThrottledEvents = GetMetricResponse(tableName, indexName, ReadThrottleEventsMetricName, cancellationToken);
            var writeThrottledEvents = GetMetricResponse(tableName, indexName, WriteThrottleEventsMetricName, cancellationToken);

            await Task.WhenAll(provisionedReadCapacity, provisionedWriteCapacity, consumedReadCapacity, consumedWriteCapacity, readThrottledEvents, writeThrottledEvents);

            return new DynamoDbMetrics()
            {
                ProvisionedReadCapacityUnits = AverageValue(provisionedReadCapacity.Result),
                ProvisionedWriteCapacityUnits = AverageValue(provisionedWriteCapacity.Result),
                ConsumedReadCapacityUnits = ConsumedValue(consumedReadCapacity.Result),
                ConsumedWriteCapacityUnits = ConsumedValue(consumedWriteCapacity.Result),
                ReadThrottleEvents = SumValue(readThrottledEvents.Result),
                WriteThrottleEvents = SumValue(writeThrottledEvents.Result)                
            };
        }

        private long SumValue(GetMetricStatisticsResponse result)
        {
            var lastStat = result.Datapoints.OrderBy(dp => dp.Timestamp).FirstOrDefault();

            if (lastStat == null)
                return 0;

            return Convert.ToInt64(lastStat.Sum);
        }
        private long AverageValue(GetMetricStatisticsResponse result)
        {
            var lastStat = result.Datapoints.OrderBy(dp => dp.Timestamp).FirstOrDefault();

            if (lastStat == null)
                return 0;

            return Convert.ToInt64(lastStat.Average);
        }

        private long ConsumedValue(GetMetricStatisticsResponse result)
        {
            var lastStat = result.Datapoints.OrderBy(dp => dp.Timestamp).FirstOrDefault();

            if (lastStat == null)
                return 0;

            // Based on AWS documentation, Average ConsumedCapacity need to be computed based on the Sum metric over the 1 min period
            // For more information, visit http://docs.aws.amazon.com/AmazonCloudWatch/latest/DeveloperGuide/dynamo-metricscollected.html
            return Convert.ToInt64(lastStat.Sum / 60);
        }


        private async Task<GetMetricStatisticsResponse> GetMetricResponse(string tableName, string indexName, string metricName, CancellationToken cancellationToken)
        {
            var dimensions = GetDimensions(tableName, indexName);
            var completeMetricName = CalculateMetricName(tableName, indexName, metricName);

            var request = new GetMetricStatisticsRequest
            {
                Namespace = DynamoDbMetricNamespace,
                Dimensions = dimensions,
                MetricName = completeMetricName,
                Period = 60,
                StartTime = DateTime.UtcNow.Subtract(MetricTimeSpan),
                EndTime = DateTime.UtcNow,
                Statistics = new[] { "Average", "Sum" }.ToList()
            };

            return await client.GetMetricStatisticsAsync(request, cancellationToken);
        }

        private string CalculateMetricName(string tableName, string indexName, string metricName)
        {
            return string.IsNullOrEmpty(indexName) ?
                string.Format("{0}.{1}", tableName, metricName) :
                string.Format("{0}-{1}.{2}", tableName, indexName, metricName);
        }

        private List<Dimension> GetDimensions(string tableName, string indexName)
        {
            var list = new List<Dimension>();

            list.Add(new Dimension() { Name = "TableName", Value = tableName });

            if(!string.IsNullOrEmpty(indexName))
            {
                list.Add(new Dimension() { Name = "GlobalSecondaryIndexName", Value = tableName });
            }

            return list;
        }
    }
}
