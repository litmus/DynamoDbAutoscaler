using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DynamoDbAutoScaler.Interfaces;
using Newtonsoft.Json;

namespace DynamoDbAutoScaler.Configuration
{
    public class GlobalAutoscalingConfigurationSet : AutoscalingConfigurationSet, IAutoscalingConfigurationSet
    {
        private const string DefaultConfigurationFileName = @".\autoscaling.json";

        public GlobalAutoscalingConfigurationSet() : base()
        {
        }

        public override string EntityName { get { return "global"; } }

        public int? CheckInterval { get; set; }

        public IEnumerable<TableAutoscalingConfigurationSet> TableConfigurations { get; set; }
        public IEnumerable<GlobalSecondaryIndexAutoscalingConfigurationSet> GlobalSecondaryIndexConfigurations { get; set; }

        public void CascadeConfigurationWhenNecessary()
        {
            foreach (var configuration in TableConfigurations.Concat(GlobalSecondaryIndexConfigurations))
            {
                configuration.CascadeConfigurationWhenNecessary(this);
            }
        }

        public static async Task<GlobalAutoscalingConfigurationSet> LoadFromFileAsync(string fileName = DefaultConfigurationFileName)
        {
            using (var fileStream = File.OpenRead(fileName))
            using (var reader = new StreamReader(fileStream))
            {
                var value = await reader.ReadToEndAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<GlobalAutoscalingConfigurationSet>(value);
            }
        }
    }
}
