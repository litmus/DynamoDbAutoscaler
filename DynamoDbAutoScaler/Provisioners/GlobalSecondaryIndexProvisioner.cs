using System.Threading;
using System.Threading.Tasks;
using DynamoDbAutoScaler.Interfaces;
using DynamoDbAutoscaler.Interfaces;
using DynamoDbAutoscaler;
using Serilog;

namespace DynamoDbAutoScaler.Provisioners
{
    public class GlobalSecondaryIndexProvisioner : Provisioner
    {
        private readonly string tableName;
        private readonly string indexName;

        public GlobalSecondaryIndexProvisioner(
            string tableName,
            string indexName,
            IDynamoDbTableThroughputClient throughputClient,
            ILogger structuredLogger)
            : base(throughputClient, structuredLogger)
        {
            this.tableName = tableName;
            this.indexName = indexName;
        }

        public override async Task UpdateProvisionAsync(
            IAutoscalingConfigurationSet configuration,
            DynamoDbThroughput updated,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            await ThroughputClient.SetGlobalSecondaryIndexThroughputLevelAsync(
                tableName, indexName, updated.ReadThroughput, updated.WriteThroughput, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
