using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DynamoDbAutoscaler.Configuration;
using DynamoDbAutoscaler.Interfaces;
using DynamoDbAutoscaler.Models;
using DynamoDbAutoscaler.Provisioners;
using DynamoDbAutoscaler.Interfaces;
using Serilog;
using DynamoDbAutoscaler;
using DynamoDbAutoscaler.Models;

namespace DynamoDbAutoscaler
{
    public class Autoscaler : IAutoscaler
    {
        private readonly IDynamoDbTableThroughputClient throughputClient;
        private readonly IDynamoDbTableMetricsClient metricsClient;
        private readonly IAutoscalingCalculator autoscalingCalculator;
        private readonly ICircuitBreaker circuitBreaker;
        private readonly ILogger structuredLogger;

        public Autoscaler(ILogger logger) : this(
                new DynamoDbTableThroughputClient(logger),
                new CloudWatchDynamoDbTableMetricsClient(),
                new AutoscalingCalculator(logger),
                new NullCircuitBreaker(),
                logger)
        {

        }

        public Autoscaler(
            IDynamoDbTableThroughputClient throughputClient,
            IDynamoDbTableMetricsClient metricsClient,
            IAutoscalingCalculator autoscalingCalculator,
            ICircuitBreaker circuitBreaker, 
            ILogger structuredLogger)
        {
            this.throughputClient = throughputClient;
            this.metricsClient = metricsClient;
            this.autoscalingCalculator = autoscalingCalculator;
            this.circuitBreaker = circuitBreaker;
            this.structuredLogger = structuredLogger;
        }

        public async Task EnsureProvisionAsync(
            GlobalAutoscalingConfigurationSet configurationSet,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var isCircuitBreakerTripped = await circuitBreaker.IsTrippedAsync();

            var exceptions = new List<Exception>();
            foreach (var configuration in configurationSet.TableConfigurations.Concat(configurationSet.GlobalSecondaryIndexConfigurations))
            {
                try
                {
                    await EnsureProvisionAsync(isCircuitBreakerTripped, configuration, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    structuredLogger.Error(exception, 
                        "Failed to ensure provisioning {TableName}-{EntityName} R:{Reads} W:{Writes}",
                        configuration.TableName,
                        configuration.EntityName,
                        configuration.Writes,
                        configuration.Reads);

                    exceptions.Add(exception);
                }
            }

            if (exceptions.Any())
            {
                throw exceptions.Count().Equals(1) ? exceptions.First() : new AggregateException(exceptions);
            }
        }

        internal async Task EnsureProvisionAsync(
            bool isCircuitBreakerTripped,
            AutoscalingConfigurationSet configurationSet,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (configurationSet is GlobalSecondaryIndexAutoscalingConfigurationSet)
            {
                var indexConfiguration = configurationSet as GlobalSecondaryIndexAutoscalingConfigurationSet;
                if (indexConfiguration.IndexName.Equals("*"))
                {
                    await EnsureAllGlobalSecondaryIndexProvisionAsync(isCircuitBreakerTripped, indexConfiguration, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await EnsureGlobalSecondaryIndexProvisionAsync(isCircuitBreakerTripped, indexConfiguration, cancellationToken).ConfigureAwait(false);
                }
            }
            else if (configurationSet is TableAutoscalingConfigurationSet)
            {
                var tableConfiguration = configurationSet as TableAutoscalingConfigurationSet;
                await EnsureTableProvisionAsync(isCircuitBreakerTripped, tableConfiguration, cancellationToken).ConfigureAwait(false);
            }
        }

        internal async Task EnsureTableProvisionAsync(
            bool isCircuitBreakerTripped,
            TableAutoscalingConfigurationSet configuration,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tableName = configuration.TableName;
            var provisioned = await throughputClient.GetTableThroughputLevelAsync(tableName, cancellationToken).ConfigureAwait(false);
            var metrics = await metricsClient.GetTableMetricsAsync(tableName, cancellationToken).ConfigureAwait(false);
            LogStats(configuration, provisioned, metrics);

            var updated = EnsureProvision(isCircuitBreakerTripped, provisioned, metrics, configuration);
            var provisioner = new TableProvisioner(tableName, throughputClient, structuredLogger);
            await provisioner.ProvisionAsync(configuration, provisioned, updated, cancellationToken).ConfigureAwait(false);
        }

        internal async Task EnsureAllGlobalSecondaryIndexProvisionAsync(
            bool isCircuitBreakerTripped,
            GlobalSecondaryIndexAutoscalingConfigurationSet configuration,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tableName = configuration.TableName;
            var indices = await throughputClient.FindAllGlobalSecondaryIndexThroughputLevelsAsync(tableName, cancellationToken).ConfigureAwait(false);
            foreach (var index in indices)
            {
                var indexConfiguration = new GlobalSecondaryIndexAutoscalingConfigurationSet(tableName, index.IndexName, configuration);
                await EnsureGlobalSecondaryIndexProvisionAsync(isCircuitBreakerTripped, indexConfiguration, cancellationToken).ConfigureAwait(false);
            }
        }

        internal async Task EnsureGlobalSecondaryIndexProvisionAsync(
            bool isCircuitBreakerTripped,
            GlobalSecondaryIndexAutoscalingConfigurationSet configuration,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tableName = configuration.TableName;
            var indexName = configuration.IndexName;

            var provisioned = await throughputClient.GetGlobalSecondaryIndexThroughputLevelAsync(tableName, indexName, cancellationToken).ConfigureAwait(false);
            var metrics = await metricsClient.GetGlobalSecondaryIndexMetricsAsync(tableName, indexName, cancellationToken);
            LogStats(configuration, provisioned, metrics);

            var updated = EnsureProvision(isCircuitBreakerTripped, provisioned, metrics, configuration);
            var provisioner = new GlobalSecondaryIndexProvisioner(tableName, indexName, throughputClient, structuredLogger);
            await provisioner.ProvisionAsync(configuration, provisioned, updated, cancellationToken).ConfigureAwait(false);
        }

        internal DynamoDbTableThroughput EnsureProvision(
            bool isCircuitBreakerTripped,
            DynamoDbTableThroughput provisioned,
            DynamoDbMetrics metrics,
            AutoscalingConfigurationSet configuration)
        {
            var updatedReads = EnsureProvision(isCircuitBreakerTripped, provisioned.ReadThroughput, metrics.ReadsThroughputMetrics, configuration.Reads);
            var updatedWrites = EnsureProvision(isCircuitBreakerTripped, provisioned.WriteThroughput, metrics.WritesThroughputMetrics, configuration.Writes);
            var updated = new DynamoDbTableThroughput { ReadThroughput = updatedReads, WriteThroughput = updatedWrites };
            return updated;
        }

        internal long EnsureProvision(
            bool isCircuitBreakerTripped,
            long provisioned, 
            DynamoDbThroughputMetrics metrics,
            AutoscalingConfiguration configuration)
        {
            var scaleDirection = ComputeScaleDirection(isCircuitBreakerTripped, provisioned, metrics, configuration);
            var updated = ComputeUpdatedProvisioned(provisioned, scaleDirection, metrics, configuration);
            return updated;
        }

        internal AutoscaleDirection ComputeScaleDirection(
            bool isCircuitBreakerTripped,
            long provisioned,
            DynamoDbThroughputMetrics metrics,
            AutoscalingConfiguration configuration)
        {
            var upperThreshold = configuration.UpperThreshold.GetValueOrDefault();
            var lowerThreshold = configuration.LowerThreshold.GetValueOrDefault();
            var throttleThreshold = configuration.ThrottleThreshold.GetValueOrDefault();

            return ComputeScaleDirection(
                isCircuitBreakerTripped, provisioned, metrics.ConsumedCapacityUnits, upperThreshold, lowerThreshold, throttleThreshold,
                metrics.ThrottleEvents, metrics.ConsumptionDirection, metrics.ThrottleDirection);
        }

        internal long ComputeUpdatedProvisioned(
            long provisioned,
            AutoscaleDirection autoscaleDirection,
            DynamoDbThroughputMetrics metrics,
            AutoscalingConfiguration configuration)
        {
            var increasePercent = configuration.IncreaseWithPercent.GetValueOrDefault();
            var decreasePercent = configuration.DecreaseWithPercent.GetValueOrDefault();
            var minProvisioned = configuration.MinProvisioned.GetValueOrDefault();
            var maxProvisioned = configuration.MaxProvisioned.GetValueOrDefault();

            return ComputeUpdatedProvisioned(
                autoscaleDirection, provisioned, increasePercent, decreasePercent, minProvisioned, maxProvisioned);
        }

        internal AutoscaleDirection ComputeScaleDirection(
            bool isCircuitBreakerTripped,
            long provisionedUnits,
            long consumedUnits,
            int upperThreshold,
            int lowerThreshold,
            int throttleThreshold,
            long throttleEvents,
            double consumptionDirection,
            double throttleDirection)
        {
            var consumed = consumedUnits.ToPercentage(provisionedUnits);

            // Todo: Currently ignored
            if (throttleDirection > 0)
            {
                // return ScaleDirection.UltraUp; // Scale up x3
            }

            // We only will do a scale up or stay operation when the circuit breaker is open
            // That way we do not scale down while we are having problems (like a bad deploy)
            if (throttleEvents > throttleThreshold && !isCircuitBreakerTripped)
            {
                return AutoscaleDirection.UltraUp;
            }
            else if (consumed >= upperThreshold)
            {
                return AutoscaleDirection.Up;
            }
            else if (consumptionDirection > 0)
            {
                return AutoscaleDirection.Stay;
            }
            else if (consumed <= lowerThreshold && !isCircuitBreakerTripped)
            {
                return AutoscaleDirection.Down;
            }

            return AutoscaleDirection.Stay;
        }

        internal long ComputeUpdatedProvisioned(
            AutoscaleDirection autoscaleDirection,
            long provisionedUnits,
            int increasePercent,
            int decreasePercent,
            int minProvisioned,
            int maxProvisioned)
        {
            var updatedUnits = ComputeUpdatedProvisioned(autoscaleDirection, provisionedUnits, increasePercent, decreasePercent);
            updatedUnits = autoscalingCalculator.EnsureProvisionInRange(updatedUnits, minProvisioned, maxProvisioned);
            return updatedUnits;
        }

        private long ComputeUpdatedProvisioned(
            AutoscaleDirection autoscaleDirection,
            long provisionedUnits,
            int increasePercent,
            int decreasePercent)
        {
            switch (autoscaleDirection)
            {
                case AutoscaleDirection.UltraUp:
                    return autoscalingCalculator.CalculateProvisionIncrease(provisionedUnits, increasePercent * 4);
                case AutoscaleDirection.ExtraUp:
                    return autoscalingCalculator.CalculateProvisionIncrease(provisionedUnits, increasePercent * 2);
                case AutoscaleDirection.Up:
                    return autoscalingCalculator.CalculateProvisionIncrease(provisionedUnits, increasePercent);
                case AutoscaleDirection.Down:
                    return autoscalingCalculator.CalculateProvisionDecrease(provisionedUnits, decreasePercent);
                default:
                    return provisionedUnits;
            }
        }

        private void LogStats(AutoscalingConfigurationSet configuration, DynamoDbThroughput throughput, DynamoDbMetrics metrics)
        {
            var entityName = configuration.EntityName;
            var readsProvisioned = throughput.ReadThroughput;
            var writesProvisioned = throughput.WriteThroughput;
            var readsConsumed = metrics.ConsumedReadCapacityUnits;
            var writesConsumed = metrics.ConsumedWriteCapacityUnits;
            var readsPercent = readsConsumed.ToPercentage(readsProvisioned);
            var writesPercent = writesConsumed.ToPercentage(writesProvisioned);

            structuredLogger.Information(
                "{EntityName}:reads {ReadsConsumed}/{ReadsProvisioned},{ReadsPercent}%,->{ReadConsumptionDirection},{ReadThrottleEvents}",
                entityName, readsConsumed, readsProvisioned, readsPercent, metrics.ReadConsumptionDirection.ToString("F4"), metrics.ReadThrottleEvents);

            structuredLogger.Information(
                "{EntityName}:writes {WritesConsumed}/{WritesProvisioned},{WritesPercent}%,->{WriteConsumptionDirection},{WriteThrottleEvents}",
                entityName, writesConsumed, writesProvisioned, writesPercent, metrics.WriteConsumptionDirection.ToString("F4"), metrics.WriteThrottleEvents);
        }
    }
}
