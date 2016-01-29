using DynamoDbAutoScaler.Interfaces;

namespace DynamoDbAutoScaler.Configuration
{
    public class TableAutoscalingConfigurationSet : AutoscalingConfigurationSet, IAutoscalingConfigurationSet
    {
        public TableAutoscalingConfigurationSet() 
            : base()
        {
        }

        public TableAutoscalingConfigurationSet(string tableName, IAutoscalingConfigurationSet configurationSet)
            : base(configurationSet)
        {
            TableName = tableName;
        }

        public override string EntityName { get { return string.Format("{0}", TableName); } }
        public string TableName { get; set; }
    }
}
