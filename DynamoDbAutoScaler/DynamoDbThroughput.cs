using System;
using Amazon.DynamoDBv2.Model;

namespace DynamoDbAutoscaler
{
    public abstract class DynamoDbThroughput
    {
        protected DynamoDbThroughput()
        {
        }

        protected DynamoDbThroughput(ProvisionedThroughputDescription throughput)
        {
            LastDecreaseAt = throughput.LastDecreaseDateTime;
            LastIncreaseAt = throughput.LastIncreaseDateTime;
            ReadThroughput = throughput.ReadCapacityUnits;
            WriteThroughput = throughput.WriteCapacityUnits;
            NumberOfDecreasesToday = throughput.NumberOfDecreasesToday;
        }

        public long ReadThroughput { get; set; }
        public long WriteThroughput { get; set; }
        public long NumberOfDecreasesToday { get; set; }
        public DateTime LastIncreaseAt { get; set; }
        public DateTime LastDecreaseAt { get; set; }
    }
}
