using DynamoDbAutoscaler.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamoDbAutoscaler.Interfaces
{
    public interface IGlobalAutoscalingConfigurationSetFactory
    {
        Task<GlobalAutoscalingConfigurationSet> LoadConfiguration();
    }
}
