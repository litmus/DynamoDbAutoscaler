using DynamoDbAutoScaler.Interfaces;

namespace DynamoDbAutoScaler.Configuration
{
    public class GlobalSecondaryIndexAutoscalingConfigurationSet : TableAutoscalingConfigurationSet, IAutoscalingConfigurationSet
    {
        public GlobalSecondaryIndexAutoscalingConfigurationSet() : base()
        {
        }

        public GlobalSecondaryIndexAutoscalingConfigurationSet(string tableName, string indexName, IAutoscalingConfigurationSet configurationSet)
            : base(tableName, configurationSet)
        {
            IndexName = indexName;
        }

        public override string EntityName { get { return string.Format("{0}:{1}", TableName, IndexName); } }
        public string IndexName { get; set; }
    }
}
