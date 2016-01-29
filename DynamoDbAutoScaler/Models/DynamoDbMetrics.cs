using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamoDbAutoscaler.Models
{
    public class DynamoDbMetrics
    {

        public long ProvisionedReadCapacityUnits { get; set; }
        public long ProvisionedWriteCapacityUnits { get; set; }
        public long ConsumedReadCapacityUnits { get; set; }
        public long ConsumedWriteCapacityUnits { get; set; }
        public long ThrottledRequests { get; set; }
        public long ReadThrottleEvents { get; set; }
        public long WriteThrottleEvents { get; set; }
        public double ReadConsumptionDirection { get; set; }
        public double WriteConsumptionDirection { get; set; }
        public double ReadThrottleDirection { get; set; }
        public double WriteThrottleDirection { get; set; }

        public DynamoDbThroughputMetrics ReadsThroughputMetrics
        {
            get
            {
                return new DynamoDbThroughputMetrics(
                    ProvisionedReadCapacityUnits,
                    ConsumedReadCapacityUnits,
                    ReadThrottleEvents,
                    ReadConsumptionDirection,
                    ReadThrottleDirection);
            }
        }

        public DynamoDbThroughputMetrics WritesThroughputMetrics
        {
            get
            {
                return new DynamoDbThroughputMetrics(
                    ProvisionedWriteCapacityUnits,
                    ConsumedWriteCapacityUnits,
                    WriteThrottleEvents,
                    WriteConsumptionDirection,
                    WriteThrottleDirection);
            }
        }
    }
}
