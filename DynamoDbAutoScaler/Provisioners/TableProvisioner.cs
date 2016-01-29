using System.Threading;
using System.Threading.Tasks;
using DynamoDbAutoScaler.Interfaces;
using Serilog;
using DynamoDbAutoscaler.Interfaces;
using DynamoDbAutoscaler;

namespace DynamoDbAutoScaler.Provisioners
{
    public class TableProvisioner : Provisioner
    {
        private readonly string tableName;

        public TableProvisioner(string tableName, IDynamoDbTableThroughputClient throughputClient, ILogger structuredLogger)
            : base(throughputClient, structuredLogger)
        {
            this.tableName = tableName;
        }

        public override async Task UpdateProvisionAsync(
            IAutoscalingConfigurationSet configuration,
            DynamoDbThroughput updated,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            await ThroughputClient.SetTableThroughputLevelAsync(
                tableName, updated.ReadThroughput, updated.WriteThroughput, cancellationToken)
                .ConfigureAwait(false);
        }
    }
}
