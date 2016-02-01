using DynamoDbAutoscaler.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamoDbAutoscaler.Configuration;

namespace DynamoDbAutoscaler
{
    public class LocalFileGlobalAutoscalingConfigurationSetFactory : IGlobalAutoscalingConfigurationSetFactory
    {
        private readonly string localFilePath;

        public LocalFileGlobalAutoscalingConfigurationSetFactory(string localFilePath = "./autoscaling.json")
        {
            this.localFilePath = localFilePath;
        }

        public Task<GlobalAutoscalingConfigurationSet> LoadConfiguration()
        {
            return GlobalAutoscalingConfigurationSet.LoadFromFileAsync(localFilePath);
        }
    }
}
