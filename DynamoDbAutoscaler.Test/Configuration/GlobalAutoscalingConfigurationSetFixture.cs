﻿using System.IO;
using System.Threading.Tasks;

using DynamoDbAutoscaler.Configuration;
using NUnit.Framework;

namespace DynamoDbAutoscaler.Test.Configuration
{
    [TestFixture]
    public class GlobalAutoscalingConfigurationSetFixture
    {
        private static string GetLocalFilePath()
        {
            var basePath = TestContext.CurrentContext.TestDirectory;
            var localFilePath = Path.Combine(basePath, "autoscaling.json");

            return localFilePath;
        }

        [Test]
        public async Task LoadFromFile_ShouldLoadAndSerializeConfiguration()
        {
            var localFilePath = GetLocalFilePath();
            var configuration = await GlobalAutoscalingConfigurationSet.LoadFromFileAsync(localFilePath);

            Assert.That(configuration.CheckInterval, Is.EqualTo(60));
            Assert.That(configuration.DemoMode, Is.EqualTo(true));
            
            // reads
            Assert.That(configuration.Reads.EnableAutoscaling, Is.EqualTo(true));
            Assert.That(configuration.Reads.UpperThreshold, Is.EqualTo(50));
            Assert.That(configuration.Reads.LowerThreshold, Is.EqualTo(25));
            Assert.That(configuration.Reads.ThrottleThreshold, Is.EqualTo(100));
            Assert.That(configuration.Reads.IncreaseWithPercent, Is.EqualTo(65));
            Assert.That(configuration.Reads.DecreaseWithPercent, Is.EqualTo(45));

            // writes
            Assert.That(configuration.Writes.EnableAutoscaling, Is.EqualTo(true));
            Assert.That(configuration.Writes.UpperThreshold, Is.EqualTo(50));
            Assert.That(configuration.Writes.LowerThreshold, Is.EqualTo(25));
            Assert.That(configuration.Writes.ThrottleThreshold, Is.EqualTo(200));
            Assert.That(configuration.Writes.IncreaseWithPercent, Is.EqualTo(20));
            Assert.That(configuration.Writes.DecreaseWithPercent, Is.EqualTo(45));
        }

        [Test]
        public async Task LoadFromFile_ShouldLoadAndSerializeAllChildrenConfigurationAndSetGlobalWhenEmpty()
        {
            var localFilePath = GetLocalFilePath();
            var configuration = await GlobalAutoscalingConfigurationSet.LoadFromFileAsync(localFilePath);

            foreach (var child in configuration.TableConfigurations)
            {
                Assert.That(child.DemoMode, Is.EqualTo(true));

                // reads
                Assert.That(child.Reads.EnableAutoscaling, Is.EqualTo(true));
                Assert.That(child.Reads.MaxProvisioned, Is.GreaterThan(0));
                Assert.That(child.Reads.MinProvisioned, Is.GreaterThan(0));

                // writes
                Assert.That(child.Writes.EnableAutoscaling, Is.EqualTo(true));
                Assert.That(child.Writes.MaxProvisioned, Is.GreaterThan(0));
                Assert.That(child.Writes.MinProvisioned, Is.GreaterThan(0));
            }

            foreach (var child in configuration.GlobalSecondaryIndexConfigurations)
            {
                Assert.That(child.DemoMode, Is.EqualTo(true));

                // reads
                Assert.That(child.Reads.EnableAutoscaling, Is.EqualTo(true));
                Assert.That(child.Reads.MaxProvisioned, Is.GreaterThan(0));
                Assert.That(child.Reads.MinProvisioned, Is.GreaterThan(0));

                // writes
                Assert.That(child.Writes.EnableAutoscaling, Is.EqualTo(true));
                Assert.That(child.Writes.MaxProvisioned, Is.GreaterThan(0));
                Assert.That(child.Writes.MinProvisioned, Is.GreaterThan(0));
            }
        }
    }
}
