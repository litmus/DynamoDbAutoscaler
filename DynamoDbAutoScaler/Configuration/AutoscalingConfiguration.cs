namespace DynamoDbAutoscaler.Configuration
{
    public class AutoscalingConfiguration
    {
        public AutoscalingConfiguration()
        {
        }

        public AutoscalingConfiguration(AutoscalingConfiguration configuration)
        {
            EnableAutoscaling = configuration.EnableAutoscaling;
            UpperThreshold = configuration.UpperThreshold;
            LowerThreshold = configuration.LowerThreshold;
            ThrottleThreshold = configuration.ThrottleThreshold;
            IncreaseWithPercent = configuration.IncreaseWithPercent;
            DecreaseWithPercent = configuration.DecreaseWithPercent;
            MinProvisioned = configuration.MinProvisioned;
            MaxProvisioned = configuration.MaxProvisioned;
        }

        public bool? EnableAutoscaling { get; set; }
        public int? UpperThreshold { get; set; }
        public int? LowerThreshold { get; set; }
        public int? ThrottleThreshold { get; set; }
        public int? IncreaseWithPercent { get; set; }
        public int? DecreaseWithPercent { get; set; }
        public int? MinProvisioned { get; set; }
        public int? MaxProvisioned { get; set; }

        public void CascadeConfigurationWhenNecessary(AutoscalingConfiguration configuration)
        {
            EnableAutoscaling = EnableAutoscaling.HasValue ? EnableAutoscaling : configuration.EnableAutoscaling;
            UpperThreshold = UpperThreshold.HasValue ? UpperThreshold : configuration.UpperThreshold;
            LowerThreshold = LowerThreshold.HasValue ? LowerThreshold : configuration.LowerThreshold;
            ThrottleThreshold = ThrottleThreshold.HasValue ? ThrottleThreshold : configuration.ThrottleThreshold;
            IncreaseWithPercent = IncreaseWithPercent.HasValue ? IncreaseWithPercent : configuration.IncreaseWithPercent;
            DecreaseWithPercent = DecreaseWithPercent.HasValue ? DecreaseWithPercent : configuration.DecreaseWithPercent;
        }
    }
}
