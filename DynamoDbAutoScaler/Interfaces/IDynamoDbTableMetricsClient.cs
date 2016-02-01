using DynamoDbAutoscaler.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DynamoDbAutoscaler.Interfaces
{
    public interface IDynamoDbTableMetricsClient
    {
        Task<DynamoDbMetrics> GetTableMetricsAsync(string tableName, CancellationToken cancellationToken);
        Task<DynamoDbMetrics> GetGlobalSecondaryIndexMetricsAsync(string tableName, string indexName, CancellationToken cancellationToken);
    }
}
