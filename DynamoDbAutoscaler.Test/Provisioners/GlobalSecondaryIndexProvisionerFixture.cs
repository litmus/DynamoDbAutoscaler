using System.Threading;
using Moq;
using NUnit.Framework;
using DynamoDbAutoscaler.Provisioners;
using Serilog;
using DynamoDbAutoscaler.Interfaces;
using DynamoDbAutoscaler.Configuration;
using System.Threading.Tasks;

namespace DynamoDbAutoscaler.Test.Provisioners
{
    [TestFixture]
    public class GlobalSecondaryIndexProvisionerFixture
    {
        private readonly string tableName = "ea_hits";
        private readonly string indexName = "campaign_guid";

        private GlobalSecondaryIndexProvisioner indexProvisioner;
        private Mock<IDynamoDbTableThroughputClient> throughputClientMock;
        private ILogger structuredLogger = UnitTestLogger.Logger;

        public GlobalSecondaryIndexProvisionerFixture()
        {
        }

        [SetUp]
        public void SetUp()
        {
            throughputClientMock = new Mock<IDynamoDbTableThroughputClient>();
            indexProvisioner = new GlobalSecondaryIndexProvisioner(tableName, indexName, throughputClientMock.Object, structuredLogger);
        }

        [Test]
        public async Task EnsureProvisioningAsync_ShouldCallUpdateOnClientWhenRequired()
        {
            var configurationSet = new GlobalSecondaryIndexAutoscalingConfigurationSet
            {
                TableName = tableName,
                DemoMode = false,
                Reads = new AutoscalingConfiguration { EnableAutoscaling = true },
                Writes = new AutoscalingConfiguration { EnableAutoscaling = true },
            };
            var provisioned = new DynamoDbTableThroughput { ReadThroughput = 500, WriteThroughput = 1000 };
            var updated = new DynamoDbTableThroughput { ReadThroughput = 500, WriteThroughput = 2000 };

            await indexProvisioner.ProvisionAsync(configurationSet, provisioned, updated);

            throughputClientMock.Verify(
                client => client.SetGlobalSecondaryIndexThroughputLevelAsync(tableName, indexName, 500, 2000, default(CancellationToken)),
                Times.Once);
        }

        [Test]
        public async Task EnsureProvisioningAsync_ShouldNotCallUpdateOnClientWhenValuesAreTheSame()
        {
            var readsConfiguration = new AutoscalingConfiguration { EnableAutoscaling = true };
            var writesConfiguration = new AutoscalingConfiguration { EnableAutoscaling = true };
            var configurationSet = new GlobalSecondaryIndexAutoscalingConfigurationSet
            {
                TableName = tableName,
                DemoMode = false,
                Reads = readsConfiguration,
                Writes = writesConfiguration,
            };
            var provisioned = new DynamoDbTableThroughput { ReadThroughput = 500, WriteThroughput = 1000 };
            var updated = new DynamoDbTableThroughput { ReadThroughput = 500, WriteThroughput = 1000 };

            await indexProvisioner.ProvisionAsync(configurationSet, provisioned, updated);

            throughputClientMock.Verify(
                client => client.SetGlobalSecondaryIndexThroughputLevelAsync(tableName, indexName, 500, 1000, default(CancellationToken)),
                Times.Never);
        }
    }
}
