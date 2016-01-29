using System;
using DynamoDbAutoScaler.Interfaces;
using Serilog;

namespace DynamoDbAutoScaler
{
    public class AutoscalingCalculator : IAutoscalingCalculator
    {
        public const int SafeMinimum = 1;

        protected readonly ILogger structuredLogger;

        public AutoscalingCalculator(ILogger structuredLogger)
        {
            this.structuredLogger = structuredLogger;
        }

        public long CalculateProvisionIncrease(long currentThroughput, double increasePercent)
        {
            var increase = Convert.ToInt64(Math.Floor(currentThroughput * (increasePercent / 100)));
            var updatedThroughput = currentThroughput + increase;

            structuredLogger.Debug(
                "Calculate provision increase with percent {IncreasePercent}: {CurrentThroughput}->{UpdatedThroughput}",
                increasePercent, currentThroughput, updatedThroughput);
            return updatedThroughput;
        }

        public long CalculateProvisionDecrease(long currentThroughput, double decreasePercent)
        {
            var decrease = Convert.ToInt64(Math.Floor(currentThroughput * (decreasePercent / 100)));
            var updatedThroughput = currentThroughput - decrease;

            structuredLogger.Debug(
                "Calculate provision decrease with percent {DecreasePercent}: {CurrentThroughput}->{UpdatedThroughput}",
                decreasePercent, currentThroughput, updatedThroughput);
            return updatedThroughput;
        }

        public long EnsureProvisionInRange(long currentThroughput, long minimumThroughput, long maximumThroughput)
        {
            if (minimumThroughput < SafeMinimum)
            {
                structuredLogger.Debug("Minimum provision {MinimumThroughput} less than 1; correcting", minimumThroughput);
                minimumThroughput = SafeMinimum;
            }

            if (maximumThroughput < SafeMinimum)
            {
                structuredLogger.Debug("Maximum provision {MaximumThroughput} less than 1; correcting", maximumThroughput);
                maximumThroughput = SafeMinimum;
            }

            if ( currentThroughput < minimumThroughput)
            {
                structuredLogger.Debug("Provision {CurrentThroughput} less than minimum {MinimumThroughput}; correcting",
                    currentThroughput, minimumThroughput);
                return minimumThroughput;
            }
            else if (currentThroughput > maximumThroughput)
            {
                structuredLogger.Debug("Provision {CurrentThroughput} more than maximum {MaximumThroughput}; correcting",
                    currentThroughput, maximumThroughput);
                return maximumThroughput;
            }
            else
            {
                return currentThroughput;
            }
        }
    }
}
