﻿using DynamoDbAutoscaler.Configuration;

namespace DynamoDbAutoscaler.Interfaces
{
    public interface IAutoscalingConfigurationSet
    {
        string EntityName { get; }
        bool? DemoMode { get; set; }
        int? IncreaseInterval { get; set; }
        int? DecreaseInterval { get; set; }
        AutoscalingConfiguration Reads { get; set; }
        AutoscalingConfiguration Writes { get; set; }
    }
}
