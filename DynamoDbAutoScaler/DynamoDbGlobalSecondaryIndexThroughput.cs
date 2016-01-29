using Amazon.DynamoDBv2.Model;

namespace DynamoDbAutoscaler
{
    public class DynamoDbGlobalSecondaryIndexThroughput : DynamoDbTableThroughput
    {
        public DynamoDbGlobalSecondaryIndexThroughput()
        {
        }

        internal DynamoDbGlobalSecondaryIndexThroughput(
            TableDescription tableDescription,
            GlobalSecondaryIndexDescription indexDescription)
            : this(tableDescription.TableName, indexDescription)
        {
        }

        internal DynamoDbGlobalSecondaryIndexThroughput(string tableName, GlobalSecondaryIndexDescription indexDescription)
            : base(tableName, indexDescription.ProvisionedThroughput)
        {
            IndexName = indexDescription.IndexName;
        }

        public string IndexName { get; set; }
    }
}
