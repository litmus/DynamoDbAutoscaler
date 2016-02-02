using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.Model;
using DynamoDbAutoscaler.Interfaces;
using Serilog;
using DynamoDbAutoscaler;

namespace DynamoDbAutoscaler.Provisioners
{
    public abstract class Provisioner
    {
        private const int DefaultIncreaseInterval = 15;
        private const int DefaultDecreaseInterval = 90;

        protected readonly IDynamoDbTableThroughputClient ThroughputClient;
        protected readonly ILogger StructuredLogger;

        protected Provisioner(IDynamoDbTableThroughputClient throughputClient, ILogger structuredLogger)
        {
            StructuredLogger = structuredLogger;
            ThroughputClient = throughputClient;
        }

        public abstract Task UpdateProvisionAsync(
            IAutoscalingConfigurationSet configuration,
            DynamoDbThroughput current,
            CancellationToken cancellationToken = default(CancellationToken));

        public async Task ProvisionAsync(
            IAutoscalingConfigurationSet configuration,
            DynamoDbThroughput current,
            DynamoDbThroughput updated,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var demoMode = configuration.DemoMode.GetValueOrDefault(true);
            var increaseInterval = TimeSpan.FromMinutes(configuration.IncreaseInterval.GetValueOrDefault(DefaultIncreaseInterval));
            var decreaseInterval = TimeSpan.FromMinutes(configuration.DecreaseInterval.GetValueOrDefault(DefaultDecreaseInterval));
            var readsEnabled = configuration.Reads.EnableAutoscaling.GetValueOrDefault(true);
            var writesEnabled = configuration.Writes.EnableAutoscaling.GetValueOrDefault(true);

            var shouldUpdateReads = ShouldUpdate(configuration.EntityName, "reads", current.ReadThroughput,
                updated.ReadThroughput, readsEnabled, demoMode, current.LastDecreaseAt, current.LastIncreaseAt, increaseInterval, decreaseInterval);
            if (!shouldUpdateReads)
                updated.ReadThroughput = current.ReadThroughput;
            
            var shouldUpdateWrites = ShouldUpdate(configuration.EntityName, "writes", current.WriteThroughput,
                updated.WriteThroughput, writesEnabled, demoMode, current.LastDecreaseAt, current.LastIncreaseAt, increaseInterval, decreaseInterval);
            if (!shouldUpdateWrites)
                updated.WriteThroughput = current.WriteThroughput;

            if (shouldUpdateReads || shouldUpdateWrites)
            {
                try
                {
                    await UpdateProvisionAsync(configuration, updated, cancellationToken);

                    StructuredLogger.Warning(
                        "{EntityName}: updating reads {CurrentReadThroughput}->{UpdatedReadThroughput}, writes {CurrentWriteThroughput}->{UpdatedWriteThroughput}",
                        configuration.EntityName, current.ReadThroughput, updated.ReadThroughput, current.WriteThroughput, updated.WriteThroughput);
                }
                catch (ResourceInUseException)
                {
                    StructuredLogger.Information("{EntityName}: still updating provision", configuration.EntityName);
                }
                catch (LimitExceededException)
                {
                    StructuredLogger.Information("{EntityName}: reached maximum decreases per day", configuration.EntityName);
                }
            }
            else
            {
                StructuredLogger.Information(
                    "{EntityName}: no changes reads {CurrentReadThroughput}, writes {CurrentWriteThroughput}",
                    configuration.EntityName, current.ReadThroughput, current.WriteThroughput);
            }
        }

        internal bool ShouldUpdate(
            string entityName,
            string type,
            long provisioned,
            long updated,
            bool enabled,
            bool demoMode,
            DateTime increasedAt,
            DateTime decreaseAt,
            TimeSpan increaseInterval,
            TimeSpan decreaseInterval)
        {
            if (updated != provisioned)
            {
                var timeSinceLastDecrease = DateTime.UtcNow.Subtract(decreaseAt);
                var timeSinceLastIncrease = DateTime.UtcNow.Subtract(increasedAt);

                if (!enabled)
                {
                    StructuredLogger.Debug("{EntityName}: autoscaling {AutoscalingType} disabled!", entityName, type);
                    return false;
                }
                else if (updated < provisioned && (timeSinceLastDecrease < decreaseInterval || timeSinceLastIncrease < decreaseInterval))
                {
                    StructuredLogger.Debug("{EntityName}: decreased less than {DecreaseInterval} ago!", entityName, decreaseInterval);
                    return false;
                }
                else if (updated > provisioned && (timeSinceLastDecrease < increaseInterval || timeSinceLastIncrease < increaseInterval))
                {
                    StructuredLogger.Debug("{EntityName}: increased less than {IncreaseInterval} ago!", entityName, increaseInterval);
                    return false;
                }
                else if (demoMode)
                {
                    StructuredLogger.Debug(
                        "{EntityName}: running on demo mode! Would update {AutoscalingType} {CurrentThroughput}->{UpdatedThroughput}",
                        entityName, type, provisioned, updated);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
