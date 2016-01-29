using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamoDbAutoscaler.Models
{
    public class DynamoDbThroughputMetrics
    {
        public DynamoDbThroughputMetrics()
        {
        }

        public DynamoDbThroughputMetrics(
            long provisionedCapacityUnits,
            long consumedCapacityUnits,
            long throttleEvents,
            double consumptionDirection,
            double throttleDirection)
        {
            ProvisionedCapacityUnits = provisionedCapacityUnits;
            ConsumedCapacityUnits = consumedCapacityUnits;
            ThrottleEvents = throttleEvents;
            ConsumptionDirection = consumptionDirection;
            ThrottleDirection = throttleDirection;
        }

        public long ProvisionedCapacityUnits { get; set; }
        public long ConsumedCapacityUnits { get; set; }
        public long ThrottleEvents { get; set; }
        public double ConsumptionDirection { get; set; }
        public double ThrottleDirection { get; set; }
    }
}