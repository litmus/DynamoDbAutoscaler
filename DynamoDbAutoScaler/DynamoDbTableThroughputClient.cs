using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using DynamoDbAutoscaler.Interfaces;
using Serilog;

namespace DynamoDbAutoscaler
{
    public class DynamoDbTableThroughputClient : IDynamoDbTableThroughputClient
    {        
        private readonly AmazonDynamoDBClient client;
        private readonly ILogger logger;

        public DynamoDbTableThroughputClient(ILogger logger) :this(new AmazonDynamoDBClient(), logger)
        {
        }

        public DynamoDbTableThroughputClient(AmazonDynamoDBClient client, ILogger logger)
        {
            this.client = client;
            this.logger = logger;
        }

        public async Task<DynamoDbTableThroughput> GetTableThroughputLevelAsync<T>(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tableName = GetTableName<T>();
            return await GetTableThroughputLevelAsync(tableName, cancellationToken);
        }

        public async Task<DynamoDbTableThroughput> GetTableThroughputLevelAsync(
            string tableName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException("tableName");

            var table = await GetTableDescriptionAsync(tableName, cancellationToken).ConfigureAwait(false);
            return new DynamoDbTableThroughput(table);
        }

        public async Task<DynamoDbTableThroughput> SetTableThroughputLevelAsync<TModel>(
            long? readLevel,
            long? writeLevel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tableName = GetTableName<TModel>();
            return await SetTableThroughputLevelAsync(tableName, readLevel, writeLevel, cancellationToken);
        }

        public async Task<DynamoDbTableThroughput> SetTableThroughputLevelAsync(
            string tableName,
            long? readLevel,
            long? writeLevel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException("tableName");
            if (readLevel < 1) throw new ArgumentOutOfRangeException("readLevel");
            if (writeLevel < 1) throw new ArgumentOutOfRangeException("writeLevel");
            if (readLevel == null && writeLevel == null) throw new ArgumentException("Must specify either or both readLevel and writeLevel");

            var tableDescription = await GetTableDescriptionAsync(tableName, cancellationToken).ConfigureAwait(false);
            var currentThroughput = tableDescription.ProvisionedThroughput;
            if (currentThroughput.ReadCapacityUnits == readLevel && currentThroughput.WriteCapacityUnits == writeLevel)
            {
                logger.Debug("Requested values same as current value for {TableName} (reads:{ReadLevel}, writes:{WriteLevel})", tableName, readLevel, writeLevel);
                return new DynamoDbTableThroughput(tableDescription);
            }

            var requestedReads = readLevel.GetValueOrDefault(currentThroughput.ReadCapacityUnits);
            var requestedWrites = writeLevel.GetValueOrDefault(currentThroughput.WriteCapacityUnits);
            logger.Debug("Updating {TableName} reads:{CurrentReadUnits}->{RequestedReadUnits}", tableName, currentThroughput.ReadCapacityUnits, requestedReads);
            logger.Debug("Updating {TableName} writes:{CurrentWriteUnits}->{RequestedWriteUnits}", tableName, currentThroughput.WriteCapacityUnits, requestedWrites);
            var requestedThroughput = new ProvisionedThroughput { ReadCapacityUnits = requestedReads, WriteCapacityUnits = requestedWrites };

            var request = new UpdateTableRequest { TableName = tableName, ProvisionedThroughput = requestedThroughput };
            var response = await client.UpdateTableAsync(request, cancellationToken).ConfigureAwait(false);
            return new DynamoDbTableThroughput(response.TableDescription);
        }

        public async Task<DynamoDbTableThroughput> WaitTableThroughputLevelUpdateAsync<TModel>(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tableName = GetTableName<TModel>();
            return await WaitTableThroughputLevelUpdateAsync(tableName, cancellationToken);
        }

        public async Task<DynamoDbTableThroughput> WaitTableThroughputLevelUpdateAsync(
            string tableName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException("tableName");

            var tableDescription = await GetTableDescriptionAsync(tableName, cancellationToken).ConfigureAwait(false);
            while (tableDescription.TableStatus.Equals(TableStatus.UPDATING))
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                tableDescription = await GetTableDescriptionAsync(tableName, cancellationToken).ConfigureAwait(false);
            }

            return new DynamoDbTableThroughput(tableDescription);
        }

        public async Task<IEnumerable<DynamoDbGlobalSecondaryIndexThroughput>> FindAllGlobalSecondaryIndexThroughputLevelsAsync<TModel>(
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tableName = GetTableName<TModel>();
            return await FindAllGlobalSecondaryIndexThroughputLevelsAsync(tableName, cancellationToken);
        }

        public async Task<IEnumerable<DynamoDbGlobalSecondaryIndexThroughput>> FindAllGlobalSecondaryIndexThroughputLevelsAsync(
            string tableName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException("tableName");

            var tableDescription = await GetTableDescriptionAsync(tableName, cancellationToken).ConfigureAwait(false);
            return tableDescription.GlobalSecondaryIndexes
                .Select(index => new DynamoDbGlobalSecondaryIndexThroughput(tableName, index));
        }

        public async Task<DynamoDbGlobalSecondaryIndexThroughput> GetGlobalSecondaryIndexThroughputLevelAsync<TModel>(
            string indexName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tableName = GetTableName<TModel>();
            return await GetGlobalSecondaryIndexThroughputLevelAsync(tableName, indexName, cancellationToken);
        }

        public async Task<DynamoDbGlobalSecondaryIndexThroughput> GetGlobalSecondaryIndexThroughputLevelAsync(
            string tableName,
            string indexName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException("tableName");
            if (string.IsNullOrEmpty(indexName)) throw new ArgumentNullException("indexName");

            var tableDescription = await GetTableDescriptionAsync(tableName, cancellationToken).ConfigureAwait(false);
            var indexDescription = tableDescription.GlobalSecondaryIndexes.FirstOrDefault(_ => _.IndexName == indexName);
            if (indexDescription == null)
            {
                throw new AutoScaleProvisioningException("Index does not exist", tableName, indexName);
            }

            return new DynamoDbGlobalSecondaryIndexThroughput(tableDescription, indexDescription);
        }

        public async Task<DynamoDbGlobalSecondaryIndexThroughput> SetGlobalSecondaryIndexThroughputLevelAsync<TModel>(
            string indexName,
            long? readLevel,
            long? writeLevel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tableName = GetTableName<TModel>();
            return await SetGlobalSecondaryIndexThroughputLevelAsync(tableName, indexName, readLevel, writeLevel, cancellationToken);
        }

        public async Task<DynamoDbGlobalSecondaryIndexThroughput> SetGlobalSecondaryIndexThroughputLevelAsync(
            string tableName,
            string indexName,
            long? readLevel,
            long? writeLevel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException("tableName");
            if (string.IsNullOrEmpty(indexName)) throw new ArgumentNullException("indexName");
            if (readLevel < 1) throw new ArgumentOutOfRangeException("readLevel");
            if (writeLevel < 1) throw new ArgumentOutOfRangeException("writeLevel");
            if (readLevel == null && writeLevel == null) throw new ArgumentException("Must specify either or both readLevel and writeLevel");

            var indexDescription = await GetGlobalSecondaryIndexDescriptionAsync(tableName, indexName, cancellationToken);
            return await SetGlobalSecondaryIndexThroughputLevelAsync(tableName, indexDescription, readLevel, writeLevel, cancellationToken);
        }

        public async Task<IEnumerable<DynamoDbGlobalSecondaryIndexThroughput>> SetAllGlobalSecondaryIndicesThroughputLevelAsync<TModel>(
            long? readLevel,
            long? writeLevel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tableName = GetTableName<TModel>();
            return await SetAllGlobalSecondaryIndicesThroughputLevelAsync(tableName, readLevel, writeLevel, cancellationToken);
        }

        public async Task<IEnumerable<DynamoDbGlobalSecondaryIndexThroughput>> SetAllGlobalSecondaryIndicesThroughputLevelAsync(
            string tableName,
            long? readLevel,
            long? writeLevel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(tableName)) throw new ArgumentNullException("tableName");
            if (readLevel < 1) throw new ArgumentOutOfRangeException("readLevel");
            if (writeLevel < 1) throw new ArgumentOutOfRangeException("writeLevel");
            if (readLevel == null && writeLevel == null) throw new ArgumentException("Must specify either or both readLevel and writeLevel");

            var tableDescription = await GetTableDescriptionAsync(tableName, cancellationToken);
            var indexDescriptions = tableDescription.GlobalSecondaryIndexes;
            var updates = indexDescriptions
                .Select(description => SetGlobalSecondaryIndexThroughputLevelAsync(tableName, description, readLevel, writeLevel, cancellationToken))
                .ToList();
            return await Task.WhenAll(updates);
        }

        internal async Task<DynamoDbGlobalSecondaryIndexThroughput> SetGlobalSecondaryIndexThroughputLevelAsync(
            string tableName,
            GlobalSecondaryIndexDescription indexDescription,
            long? readLevel,
            long? writeLevel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var indexName = indexDescription.IndexName;
            var currentThroughput = indexDescription.ProvisionedThroughput;
            if (currentThroughput.ReadCapacityUnits == readLevel && currentThroughput.WriteCapacityUnits == writeLevel)
            {
                logger.Debug("Requested values same as current value for {TableName}:{IndexName} (reads:{ReadLevle}, writes:{WriteLevel})", tableName, indexName, readLevel, writeLevel);
                return new DynamoDbGlobalSecondaryIndexThroughput(tableName, indexDescription);
            }

            var requestedReads = readLevel.GetValueOrDefault(currentThroughput.ReadCapacityUnits);
            var requestedWrites = writeLevel.GetValueOrDefault(currentThroughput.WriteCapacityUnits);
            logger.Debug("Updating {TableName}:{IndexName} reads:{CurrentReadUnits}->{RequestedReadUnits}", tableName, indexName, currentThroughput.ReadCapacityUnits, requestedReads);
            logger.Debug("Updating {TableName}:{IndexName} writes:{CurrentWriteUnits}->{RequestedWriteUnits}", tableName, indexName, currentThroughput.WriteCapacityUnits, requestedWrites);
            var requestedThroughput = new ProvisionedThroughput { ReadCapacityUnits = requestedReads, WriteCapacityUnits = requestedWrites };
            var indexUpdateAction = new UpdateGlobalSecondaryIndexAction { IndexName = indexName, ProvisionedThroughput = requestedThroughput };
            var indexUpdate = new GlobalSecondaryIndexUpdate { Update = indexUpdateAction };
            var updates = new List<GlobalSecondaryIndexUpdate> { indexUpdate };
            var request = new UpdateTableRequest { TableName = tableName, GlobalSecondaryIndexUpdates = updates };
            var response = await client.UpdateTableAsync(request, cancellationToken).ConfigureAwait(false);

            return new DynamoDbGlobalSecondaryIndexThroughput(
                tableName,
                GetGlobalSecondaryIndexDescription(response.TableDescription, indexName));
        }

        private async Task<TableDescription> GetTableDescriptionAsync(
            string tableName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var request = new DescribeTableRequest { TableName = tableName };
            var response = await client.DescribeTableAsync(request, cancellationToken).ConfigureAwait(false);
            var table = response.Table;
            return table;
        }

        private async Task<GlobalSecondaryIndexDescription> GetGlobalSecondaryIndexDescriptionAsync(
            string tableName,
            string indexName,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var tableDescription = await GetTableDescriptionAsync(tableName, cancellationToken).ConfigureAwait(false);
            var indexDescription = tableDescription.GlobalSecondaryIndexes.FirstOrDefault(_ => _.IndexName == indexName);
            if (indexDescription == null)
            {
                throw new AutoScaleProvisioningException("Index does not exist", tableName, indexName);
            }

            return indexDescription;
        }

        private GlobalSecondaryIndexDescription GetGlobalSecondaryIndexDescription(
            TableDescription tableDescription, string indexName)
        {
            var indexDescription = tableDescription.GlobalSecondaryIndexes.FirstOrDefault(_ => _.IndexName == indexName);
            if (indexDescription == null)
            {
                throw new AutoScaleProvisioningException("Index does not exist", tableDescription.TableName, indexName);
            }

            return indexDescription;
        }

        private string GetTableName<T>()
        {
            var dynamoDbTableAttribute = typeof(T).GetCustomAttributes(inherit: false)
              .FirstOrDefault(attribute => attribute is DynamoDBTableAttribute) as DynamoDBTableAttribute;
            if (dynamoDbTableAttribute == null)
            {
                throw new AutoScaleProvisioningException(string.Format("Model type {0} does not specify a DynamoDB table", typeof(T)));
            }

            return dynamoDbTableAttribute.TableName;
        }
    }
}
