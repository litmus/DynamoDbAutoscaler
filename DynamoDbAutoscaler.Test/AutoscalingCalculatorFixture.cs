using DynamoDbAutoscaler;
using NUnit.Framework;

namespace DynamoDbAutoscaler.Test
{
    [TestFixture]
    public class AutoscalingCalculatorFixture
    {
        private AutoscalingCalculator autoscalingCalculator;

        public AutoscalingCalculatorFixture()
        {
        }

        [SetUp]
        public void SetUp()
        {
            autoscalingCalculator = new AutoscalingCalculator(UnitTestLogger.Logger);
        }

        [Test]
        public void CalculateProvisionIncrease_ShouldReturnCorrectValueBasedOnPercent()
        {
            var updated = autoscalingCalculator.CalculateProvisionIncrease(1000, 56);

            Assert.That(updated, Is.EqualTo(1560));
        }

        [Test]
        public void CalculateProvisionDecrease_ShouldReturnCorrectValueBasedOnPercent()
        {
            var updated = autoscalingCalculator.CalculateProvisionDecrease(1000, 26);

            Assert.That(updated, Is.EqualTo(740));
        }

        [Test]
        public void EnsureProvisionInRange_ShouldCorrectWhenOverOrUnderFlowing()
        {
            var inRange = autoscalingCalculator.EnsureProvisionInRange(693, 200, 800);
            var under = autoscalingCalculator.EnsureProvisionInRange(45, 200, 800);
            var over = autoscalingCalculator.EnsureProvisionInRange(7896, 100, 900);

            Assert.That(inRange, Is.EqualTo(inRange));
            Assert.That(under, Is.EqualTo(200));
            Assert.That(over, Is.EqualTo(900));
        }

        [Test]
        public void EnsureProvisionInRange_ShouldCorrectMinAndMaxWhenLessThan1()
        {
            var underMin = autoscalingCalculator.EnsureProvisionInRange(-45, -200, 800);
            var underMax = autoscalingCalculator.EnsureProvisionInRange(45, -200, -800);

            Assert.That(underMin, Is.EqualTo(1));
            Assert.That(underMax, Is.EqualTo(1));
        }
    }
}
