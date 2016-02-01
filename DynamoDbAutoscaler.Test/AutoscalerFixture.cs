
using Moq;
using NUnit.Framework;
using DynamoDbAutoscaler;
using DynamoDbAutoscaler.Interfaces;
using DynamoDbAutoscaler.Models;

namespace DynamoDbAutoscaler.Test
{
    [TestFixture]
    public class AutoscalerFixture
    {
        private Autoscaler autoscaler;
        private Mock<IDynamoDbTableThroughputClient> dynamoDbTableThroughputClientMock;
        private Mock<IDynamoDbTableMetricsClient> dynamoDbTableMetricsClientMock;
        private Mock<ICircuitBreaker> circuitBreakerMock;

        public AutoscalerFixture()
        {
        }

        [SetUp]
        public void SetUp()
        {
            dynamoDbTableThroughputClientMock = new Mock<IDynamoDbTableThroughputClient>();
            dynamoDbTableMetricsClientMock = new Mock<IDynamoDbTableMetricsClient>();
            circuitBreakerMock = new Mock<ICircuitBreaker>();

            autoscaler = new Autoscaler(
                dynamoDbTableThroughputClientMock.Object,
                dynamoDbTableMetricsClientMock.Object,
                new AutoscalingCalculator(UnitTestLogger.Logger),
                circuitBreakerMock.Object,
                UnitTestLogger.Logger);
        }

        [Test]
        public void ComputeScaleDirection_ShouldOutputCorrectScaleDirection()
        {
            var direction = autoscaler.ComputeScaleDirection(false, 1000, 10, 50, 25, 100, 9, -0.49, -0.16);

            Assert.That(direction, Is.EqualTo(AutoscaleDirection.Down));
        }

        [Test]
        public void ComputeScaleDirection_ShouldOutputStayWhenConsumptionRateIsPositive()
        {
            var direction = autoscaler.ComputeScaleDirection(false, 1000, 10, 50, 25, 100, 9, 1.23, -0.16);

            Assert.That(direction, Is.EqualTo(AutoscaleDirection.Stay));
        }

        [Test]
        public void ComputeScaleDirection_ShouldOutputExtraUpOnOverThrottling()
        {
            var direction = autoscaler.ComputeScaleDirection(false, 1000, 10, 50, 25, 100, 234, 1.89, -0.16);

            Assert.That(direction, Is.EqualTo(AutoscaleDirection.UltraUp));
        }

        [Test]
        public void ComputeUpdatedProvisioned_ShouldOutputCorrectProvision()
        {
            var updatedProvisionedUnits = autoscaler.ComputeUpdatedProvisioned(AutoscaleDirection.Up, 1000, 50, 45, 500, 2000);

            Assert.That(updatedProvisionedUnits, Is.EqualTo(1500));
        }

        [Test]
        public void ComputeUpdatedProvisioned_ShoulCorrectProvisionIntoValidRange()
        {
            var overProvisioned = autoscaler.ComputeUpdatedProvisioned(AutoscaleDirection.Up, 1000, 50, 45, 500, 1450);
            var underProvisioned = autoscaler.ComputeUpdatedProvisioned(AutoscaleDirection.Down, 1000, 50, 75, 500, 2000);
            var underMinimum = autoscaler.ComputeUpdatedProvisioned(AutoscaleDirection.Down, 1000, 50, 200, -10, 2000);

            Assert.That(overProvisioned, Is.EqualTo(1450));
            Assert.That(underProvisioned, Is.EqualTo(500));
            Assert.That(underMinimum, Is.EqualTo(1));
        }
    }
}
