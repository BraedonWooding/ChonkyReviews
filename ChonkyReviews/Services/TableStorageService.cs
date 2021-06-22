using System;
using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ChonkyReviews.Adapters;
using ChonkyReviews.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ChonkyReviews.Services
{
    public class Ledger : TableEntity
    {
        public Ledger(string entity, string ledgerType) : base(entity, ledgerType)
        {
        }

        public Int64 Count;
        public Int64 Sum;
    }

    public class TableStorageService
    {
        private readonly CloudStorageAccount _storageAccount = null;
        private readonly CloudTableClient _tableClient = null;
        private readonly ILogger<TableStorageService> _logger = null;

        public TableStorageService(IConfiguration config, ILogger<TableStorageService> logger)
        {
            _storageAccount = CloudStorageAccount.Parse(config["TableStorageConnectionString"]);
            _tableClient = _storageAccount.CreateCloudTableClient();
            _logger = logger;
        }

        public CloudTable this[string tableName]
        {
            get
            {
                var table = _tableClient.GetTableReference(tableName);
                // slow'ish but very convenient
                table.CreateIfNotExists();
                return table;
            }
        }

        /// <summary>
        /// Create multiple tables asynchronously given table names.
        /// </summary>
        /// <param name="tableNames">The tables to create</param>
        /// <returns>How many were created (if a table already exists it won't count it)</returns>
        public async Task<bool> CreateIfNotExistAsync(string tableName)
        {
            return await this[tableName].CreateIfNotExistsAsync();
        }

        public async Task<TableResult> InsertOrMergeAsync(string tableName, ITableEntity entity)
        {
            // This method assumes the table will be created to save the cost of checking table existance.
            TableResult result = await this[tableName].ExecuteAsync(TableOperation.InsertOrMerge(entity));
            return result;
        }

        public async IAsyncEnumerable<T> LookupAllEntities<T>(string tableName, [EnumeratorCancellation] CancellationToken ct = default) where T : BaseEntity, new()
        {
            var table = this[tableName];
            TableContinuationToken continuation = null;
            do
            {
                var results = await table.ExecuteQuerySegmentedAsync(new TableQuery<TableStorageEntityAdapter<T>>(), continuation);
                foreach (var result in results) yield return result.InnerObject;
                continuation = results.ContinuationToken;
            } while (continuation != null && !ct.IsCancellationRequested);
        }

        public async Task<Ledger> ReadLedger(string entity, string entityValue, string ledgerType = "__Identity__")
        {
            var table = this[$"{entity}Ledger"];

            var res = await table.ExecuteAsync(TableOperation.Retrieve<Ledger>(entity, ledgerType));
            return res.Result is Ledger obj ? obj : null;
        }

        private async Task MutateLedger(string entity, string entityValue, string ledgerType, Int64 sum, Int64 count)
        {
            CloudTable table = this[$"{entity}Ledger"];

            for (int i = 0; i < 100; i++)
            {
                TableOperation op;
                Ledger ledger = await ReadLedger(entity, entityValue, ledgerType);
                if (ledger == null)
                {
                    ledger = new Ledger(entityValue, ledgerType)
                    {
                        Count = 0,
                        Sum = 0,
                    };
                }

                ledger.Count += count;
                ledger.Sum += sum;

                op = TableOperation.InsertOrReplace(ledger);
                try
                {
                    TableResult res = await table.ExecuteAsync(op, null, new OperationContext()
                    {
                        UserHeaders = new Dictionary<string, string>() { { "If-Match", ledger.ETag } }
                    });
                }
                catch (StorageException ex)
                {
                    if (ex.RequestInformation.HttpStatusCode == 412)
                    {
                        _logger.LogInformation($"Optimistic Concurrency Violation on {entity}Ledger {entityValue}:{ledgerType} trying again...");
                        continue;
                    }
                    throw;
                }

                return;
            }

            _logger.LogError($"Failed to increment {entity}Ledger {entityValue}:{ledgerType} due to concurrency issues.");
        }

        public Task MutateSumOfLedger(string entity, string entityValue, string ledgerType, Int64 sumDiff = 0)
        {
            return MutateLedger(entity, entityValue, ledgerType, sumDiff, 0);
        }

        public Task IncrementLedger(string entity, string entityValue, string ledgerType, Int64 sumDiff = 0)
        {
            return MutateLedger(entity, entityValue, ledgerType, sumDiff, 1);
        }

        public Task DecrementLedger(string entity, string entityValue, string ledgerType, Int64 sumDiff = 0)
        {
            return MutateLedger(entity, entityValue, ledgerType, sumDiff, -1);
        }

        public async IAsyncEnumerable<T> LookupEntities<T>(string tableName, string partitionKey, string pageToken = null, int numberToTake = -1, [EnumeratorCancellation] CancellationToken ct = default) where T : BaseEntity, new()
        {
            var table = this[tableName];
            TableContinuationToken continuation = null;
            int count = 0;
            do
            {
                var results = await table.ExecuteQuerySegmentedAsync(new TableQuery<TableStorageEntityAdapter<T>>()
                    .Where(TableQuery.CombineFilters(
                        TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey),
                        TableOperators.And,
                        TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThan, pageToken))), continuation);
                foreach (var result in results)
                {
                    yield return result.InnerObject;
                    count++;
                    if (numberToTake > 0 && count >= numberToTake)
                    {
                        break;
                    }
                }
                continuation = results.ContinuationToken;
            } while (continuation != null && !ct.IsCancellationRequested);
        }

        public Task<T> LookupEntity<T>(string tableName, T entity) where T : BaseEntity, new()
        {
            return LookupEntity<T>(tableName, entity.PartitionKey, entity.RowKey);
        }

        public async Task<bool> DeleteEntity<T>(string tableName, T entity) where T : BaseEntity, new()
        {
            var table = this[tableName];
            var actualEntity = await LookupEntity(tableName, entity);
            if (actualEntity == null)
            {
                return false;
            }

            try
            {
                await table.ExecuteAsync(TableOperation.Delete(new TableStorageEntityAdapter<T>(actualEntity)));
            }
            catch (StorageException ex)
            {
                if (ex.RequestInformation.HttpStatusCode == 412)
                {
                    _logger.LogInformation($"Optimistic Concurrency Violation on Delete {entity.RowKey}:{entity.PartitionKey}");
                    return false;
                }
                throw;
            }
            return true;
        }

        public async Task<(T, T, bool)> MergeEntity<T>(string tableName, T entity) where T : BaseEntity, new()
        {
            var table = this[tableName];

            TableResult res;
            T exists = await LookupEntity(tableName, entity);
            if (exists != null)
            {
                res = await table.ExecuteAsync(TableOperation.Merge(new TableStorageEntityAdapter<T>(entity)));
            }
            else
            {
                res = await table.ExecuteAsync(TableOperation.Insert(new TableStorageEntityAdapter<T>(entity)));
            }

            return (res.Result is TableStorageEntityAdapter<T> obj ? obj.InnerObject : null, exists, exists == null);
        }

        public async Task<T> LookupEntity<T>(string tableName, string partitionKey, string rowKey) where T : BaseEntity, new()
        {
            var table = this[tableName];

            var res = await table.ExecuteAsync(TableOperation.Retrieve<TableStorageEntityAdapter<T>>(partitionKey, rowKey));
            return res.Result is TableStorageEntityAdapter<T> obj ? obj.InnerObject : null;
        }
    }
}
