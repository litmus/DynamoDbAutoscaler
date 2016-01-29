using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DynamoDbAutoscaler.Interfaces
{
    public interface IDynamoDbTableThroughputClient
    {
        /// <summary>
        /// Gets current throughput for table
        /// </summary>
        /// <typeparam name="TModel">Dynamo DB model</typeparam>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<DynamoDbTableThroughput> GetTableThroughputLevelAsync<TModel>(
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Gets current throughput for table
        /// </summary>
        /// <param name="tableName">Name of table to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<DynamoDbTableThroughput> GetTableThroughputLevelAsync(
            string tableName,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Sets table throughput to desired level
        /// </summary>
        /// <typeparam name="TModel">Dynamo DB model</typeparam>
        /// <param name="readLevel">Desired read level.  If null uses current level</param>
        /// <param name="writeLevel">Desired write level.  If null uses current level</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<DynamoDbTableThroughput> SetTableThroughputLevelAsync<TModel>(
            long? readLevel,
            long? writeLevel,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Sets table throughput to desired level
        /// </summary>
        /// <param name="tableName">Name of table to update</param>
        /// <param name="readLevel">Desired read level.  If null uses current level</param>
        /// <param name="writeLevel">Desired write level.  If null uses current level</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        Task<DynamoDbTableThroughput> SetTableThroughputLevelAsync(
            string tableName,
            long? readLevel,
            long? writeLevel,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Waits for table to update throughput level
        /// </summary>
        /// <typeparam name="TModel">Dynamo DB model</typeparam>
        /// <param name="cancellationToken"></param>
        Task<DynamoDbTableThroughput> WaitTableThroughputLevelUpdateAsync<TModel>(
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Waits for table to update throughput level
        /// </summary>
        /// <param name="tableName">Name of table to update</param>
        /// <param name="cancellationToken"></param>
        Task<DynamoDbTableThroughput> WaitTableThroughputLevelUpdateAsync(
            string tableName,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Gets current throughput for al table's GSI
        /// </summary>
        /// <typeparam name="TModel">Dynamo DB model</typeparam>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<IEnumerable<DynamoDbGlobalSecondaryIndexThroughput>> FindAllGlobalSecondaryIndexThroughputLevelsAsync<TModel>(
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Gets current throughput for al table's GSI
        /// </summary>
        /// <param name="tableName">Name of table to update</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<IEnumerable<DynamoDbGlobalSecondaryIndexThroughput>> FindAllGlobalSecondaryIndexThroughputLevelsAsync(
            string tableName,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Gets current throughput for GSI
        /// </summary>
        /// <param name="tableName">Name of table to update</param>
        /// <param name="indexName">Name of GSI</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<DynamoDbGlobalSecondaryIndexThroughput> GetGlobalSecondaryIndexThroughputLevelAsync(
            string tableName,
            string indexName,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Gets current throughput for GSI
        /// </summary>
        /// <typeparam name="TModel">Dynamo DB model</typeparam>
        /// <param name="indexName">Name of GSI</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<DynamoDbGlobalSecondaryIndexThroughput> GetGlobalSecondaryIndexThroughputLevelAsync<TModel>(
            string indexName,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Sets GSI throughput to desired level
        /// </summary>
        /// <param name="tableName">Name of table to update</param>
        /// <param name="indexName">Name of GSI</param>
        /// <param name="readLevel">Desired read level.  If null uses current level</param>
        /// <param name="writeLevel">Desired write level.  If null uses current level</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<DynamoDbGlobalSecondaryIndexThroughput> SetGlobalSecondaryIndexThroughputLevelAsync(
            string tableName,
            string indexName,
            long? readLevel,
            long? writeLevel,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Sets GSI throughput to desired level
        /// </summary>
        /// <typeparam name="TModel">Dynamo DB model</typeparam>
        /// <param name="indexName">Name of GSI</param>
        /// <param name="readLevel">Desired read level.  If null uses current level</param>
        /// <param name="writeLevel">Desired write level.  If null uses current level</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<DynamoDbGlobalSecondaryIndexThroughput> SetGlobalSecondaryIndexThroughputLevelAsync<TModel>(
            string indexName,
            long? readLevel,
            long? writeLevel,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Sets GSI throughput for all GSIs in table
        /// </summary>
        /// <param name="tableName">Name of table to update</param>
        /// <param name="readLevel">Desired read level.  If null uses current level</param>
        /// <param name="writeLevel">Desired write level.  If null uses current level</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<IEnumerable<DynamoDbGlobalSecondaryIndexThroughput>> SetAllGlobalSecondaryIndicesThroughputLevelAsync(
            string tableName,
            long? readLevel,
            long? writeLevel,
            CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Sets GSI throughput for all GSIs in table
        /// </summary>
        /// <typeparam name="TModel">Dynamo DB model</typeparam>
        /// <param name="readLevel">Desired read level.  If null uses current level</param>
        /// <param name="writeLevel">Desired write level.  If null uses current level</param>
        /// <param name="cancellationToken">Cancellation token</param>
        Task<IEnumerable<DynamoDbGlobalSecondaryIndexThroughput>> SetAllGlobalSecondaryIndicesThroughputLevelAsync<TModel>(
            long? readLevel,
            long? writeLevel,
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
