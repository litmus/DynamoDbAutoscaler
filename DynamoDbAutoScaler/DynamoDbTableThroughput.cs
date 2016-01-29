using Amazon.DynamoDBv2.Model;

namespace DynamoDbAutoscaler
{
    public class DynamoDbTableThroughput : DynamoDbThroughput
    {
        public DynamoDbTableThroughput()
        {
        }

        internal DynamoDbTableThroughput(TableDescription tableDescription)
            : this(tableDescription.TableName, tableDescription.ProvisionedThroughput)
        {
        }

        protected DynamoDbTableThroughput(string tableName, ProvisionedThroughputDescription throughput)
            : base(throughput)
        {
            TableName = tableName;
        }

        public string TableName { get; set; }
    }
}
