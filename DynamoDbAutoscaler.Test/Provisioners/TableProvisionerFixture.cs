using System.Threading;
using Moq;
using NUnit.Framework;
using DynamoDbAutoScaler.Provisioners;
using DynamoDbAutoscaler.Interfaces;
using DynamoDbAutoScaler.Configuration;
using Serilog;
using System;
using System.Threading.Tasks;

namespace DynamoDbAutoscaler.Test.Provisioners
{
    [TestFixture]
    public class TableProvisionerFixture
    {
        private readonly string tableName = "unittest";

        private TableProvisioner tableProvisioner;
        private Mock<IDynamoDbTableThroughputClient> throughputClientMock;
        private ILogger structuredLogger = UnitTestLogger.Logger;

        public TableProvisionerFixture()
        {
        }

        [SetUp]
        public void SetUp()
        {
            throughputClientMock = new Mock<IDynamoDbTableThroughputClient>();
            tableProvisioner = new TableProvisioner(tableName, throughputClientMock.Object, structuredLogger);
        }

        [Test]
        public async Task EnsureProvisioningAsync_ShouldCallUpdateOnClientWhenRequired()
        {
            var configurationSet = new TableAutoscalingConfigurationSet
            {
                TableName = tableName,
                DemoMode = false,
                Reads = new AutoscalingConfiguration {EnableAutoscaling = true},
                Writes = new AutoscalingConfiguration {EnableAutoscaling = true},
            };
            var provisioned = new DynamoDbTableThroughput {ReadThroughput = 500, WriteThroughput = 1000};
            var updated = new DynamoDbTableThroughput {ReadThroughput = 500, WriteThroughput = 2000};

            await tableProvisioner.ProvisionAsync(configurationSet, provisioned, updated);

            throughputClientMock.Verify(
                client => client.SetTableThroughputLevelAsync(tableName, 500, 2000, default(CancellationToken)),
                Times.Once);
        }

        [Test]
        public async Task EnsureProvisioningAsync_ShouldCallUpdateOnClientOnlyWithEnabledThroughput()
        {
            var configurationSet = new TableAutoscalingConfigurationSet
            {
                TableName = tableName,
                DemoMode = false,
                Reads = new AutoscalingConfiguration { EnableAutoscaling = false },
                Writes = new AutoscalingConfiguration { EnableAutoscaling = true },
            };
            var provisioned = new DynamoDbTableThroughput { ReadThroughput = 500, WriteThroughput = 1000 };
            var updated = new DynamoDbTableThroughput { ReadThroughput = 1000, WriteThroughput = 2000 };

            await tableProvisioner.ProvisionAsync(configurationSet, provisioned, updated);

            throughputClientMock.Verify(
                client => client.SetTableThroughputLevelAsync(tableName, 500, 2000, default(CancellationToken)),
                Times.Once);
        }

        [Test]
        public async Task EnsureProvisioningAsync_ShouldNotCallUpdateOnClientWhenValuesAreTheSame()
        {
            var readsConfiguration = new AutoscalingConfiguration {EnableAutoscaling = true};
            var writesConfiguration = new AutoscalingConfiguration {EnableAutoscaling = true};
            var configurationSet = new TableAutoscalingConfigurationSet
            {
                TableName = tableName, DemoMode = false, Reads = readsConfiguration, Writes = writesConfiguration,
            };
            var provisioned = new DynamoDbTableThroughput { ReadThroughput = 500, WriteThroughput = 1000 };
            var updated = new DynamoDbTableThroughput { ReadThroughput = 500, WriteThroughput = 1000 };

            await tableProvisioner.ProvisionAsync(configurationSet, provisioned, updated);

            throughputClientMock.Verify(
                client => client.SetTableThroughputLevelAsync(tableName, 500, 1000, default(CancellationToken)),
                Times.Never);
        }

        [Test]
        public void ShouldUpdateProvisioning_ShouldBeTrueBasedOnConditions()
        {
            var oneDayAgo = DateTime.UtcNow.AddDays(-1);

            var shouldUpdate = tableProvisioner.ShouldUpdate(tableName, "reads", 500, 450, true, false, oneDayAgo, oneDayAgo, TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(90));
            var shouldUpdateDecreaseGreater = tableProvisioner.ShouldUpdate(tableName, "reads", 500, 600, true, false, oneDayAgo, oneDayAgo, TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(90));

            Assert.That(shouldUpdate, Is.EqualTo(true));
            Assert.That(shouldUpdateDecreaseGreater, Is.EqualTo(true));
        }

        [Test]
        public void ShouldUpdateProvisioning_ShouldBeFalseBasedOnConditions()
        {
            var oneDayAgo = DateTime.UtcNow.AddDays(-1);
            var oneMinuteAgo = DateTime.UtcNow.AddMinutes(-1);

            var shouldUpdateOnDemo = tableProvisioner.ShouldUpdate(tableName, "reads", 500, 450, true, true, oneDayAgo, oneDayAgo, TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(90));
            var shouldUpdateOnDisabled = tableProvisioner.ShouldUpdate(tableName, "reads", 500, 450, false, false, oneDayAgo, oneDayAgo, TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(90));
            var shouldUpdateNoChange = tableProvisioner.ShouldUpdate(tableName, "reads", 500, 500, true, false, oneDayAgo, oneDayAgo, TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(90));
            var shouldUpdateRecentDecrease = tableProvisioner.ShouldUpdate(tableName, "reads", 500, 300, true, false, oneMinuteAgo, oneDayAgo, TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(90));
            var shouldUpdateRecentIncrease = tableProvisioner.ShouldUpdate(tableName, "reads", 500, 300, true, false, oneDayAgo, oneMinuteAgo, TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(90));
            var shouldUpdateUpRecentDecrease = tableProvisioner.ShouldUpdate(tableName, "reads", 500, 600, true, false, oneMinuteAgo, oneDayAgo, TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(90));
            var shouldUpdateUpRecentIncrease = tableProvisioner.ShouldUpdate(tableName, "reads", 500, 600, true, false, oneDayAgo , oneMinuteAgo, TimeSpan.FromMinutes(15), TimeSpan.FromMinutes(90));

            Assert.That(shouldUpdateOnDemo, Is.EqualTo(false));
            Assert.That(shouldUpdateOnDisabled, Is.EqualTo(false));
            Assert.That(shouldUpdateNoChange, Is.EqualTo(false));
            Assert.That(shouldUpdateRecentDecrease, Is.EqualTo(false));
            Assert.That(shouldUpdateRecentIncrease, Is.EqualTo(false));
            Assert.That(shouldUpdateUpRecentDecrease, Is.EqualTo(false));
            Assert.That(shouldUpdateUpRecentIncrease, Is.EqualTo(false));
        }
    }
}
