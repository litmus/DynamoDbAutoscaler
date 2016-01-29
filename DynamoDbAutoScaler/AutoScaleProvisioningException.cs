using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamoDbAutoscaler
{
    public class AutoScaleProvisioningException : Exception
    {
        public string TableName { get; private set; }
        public string IndexName { get; private set; }

        public AutoScaleProvisioningException(string message) : base(message)
        {
        }

        public AutoScaleProvisioningException(string message, string tableName) : base(message)
        {
            TableName = tableName;
        }

        public AutoScaleProvisioningException(string message, string tableName, Exception innerException) : base(message, innerException)
        {
            TableName = tableName;
        }

        public AutoScaleProvisioningException(string message, string tableName, string indexName) : this(message, tableName)
        {
            IndexName = indexName;
        }

        public AutoScaleProvisioningException(string message, string tableName, string indexName,  Exception innerException) : base(message, innerException)
        {
            IndexName = indexName;
        }

    }
}
