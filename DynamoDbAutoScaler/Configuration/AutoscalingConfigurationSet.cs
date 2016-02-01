using DynamoDbAutoscaler.Interfaces;

namespace DynamoDbAutoscaler.Configuration
{
    public abstract class AutoscalingConfigurationSet : IAutoscalingConfigurationSet
    {
        protected AutoscalingConfigurationSet()
        {
            Reads = new AutoscalingConfiguration();
            Writes = new AutoscalingConfiguration();
        }

        protected AutoscalingConfigurationSet(IAutoscalingConfigurationSet configurationSet)
        {
            DemoMode = configurationSet.DemoMode;
            IncreaseInterval = configurationSet.IncreaseInterval;
            DecreaseInterval = configurationSet.DecreaseInterval;
            Reads = new AutoscalingConfiguration(configurationSet.Reads);
            Writes = new AutoscalingConfiguration(configurationSet.Writes);
        }

        public abstract string EntityName { get; }

        public bool? DemoMode { get; set; }
        public int? IncreaseInterval { get; set; }
        public int? DecreaseInterval { get; set; }
        public AutoscalingConfiguration Reads { get; set; }
        public AutoscalingConfiguration Writes { get; set; }

        public void CascadeConfigurationWhenNecessary(IAutoscalingConfigurationSet configurationSet)
        {
            DemoMode = DemoMode.HasValue ? DemoMode : configurationSet.DemoMode;
            IncreaseInterval = IncreaseInterval.HasValue ? IncreaseInterval : configurationSet.IncreaseInterval;
            DecreaseInterval = DecreaseInterval.HasValue ? DecreaseInterval : configurationSet.DecreaseInterval;
            Reads.CascadeConfigurationWhenNecessary(configurationSet.Reads);
            Writes.CascadeConfigurationWhenNecessary(configurationSet.Writes);
        }
    }
}
