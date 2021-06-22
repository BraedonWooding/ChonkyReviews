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

namespace ChonkyReviews.Services
{
    public class TableStorageService
    {
        private readonly CloudStorageAccount _storageAccount = null;
        private readonly CloudTableClient _tableClient = null;

        public TableStorageService(IConfiguration config)
        {
            _storageAccount = CloudStorageAccount.Parse(config["TableStorageConnectionString"]);
            _tableClient = _storageAccount.CreateCloudTableClient();
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

        public async IAsyncEnumerable<T> LookupEntities<T>(string tableName, string partitionKey, [EnumeratorCancellation] CancellationToken ct = default) where T : BaseEntity, new()
        {
            var table = this[tableName];
            TableContinuationToken continuation = null;
            do
            {
                var results = await table.ExecuteQuerySegmentedAsync(new TableQuery<TableStorageEntityAdapter<T>>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey)), continuation);
                foreach (var result in results) yield return result.InnerObject;
                continuation = results.ContinuationToken;
            } while (continuation != null && !ct.IsCancellationRequested);
        }

        public Task<T> LookupEntity<T>(string tableName, T entity) where T : BaseEntity, new()
        {
            return LookupEntity<T>(tableName, entity.PartitionKey, entity.RowKey);
        }

        public async Task<T> DeleteEntity<T>(string tableName, T entity) where T : BaseEntity, new()
        {
            var table = this[tableName];

            var res = await table.ExecuteAsync(TableOperation.Delete(new TableStorageEntityAdapter<T>(await LookupEntity(tableName, entity))));
            return res.Result is TableStorageEntityAdapter<T> obj ? obj.InnerObject : null;
        }

        public async Task<T> MergeEntity<T>(string tableName, T entity) where T : BaseEntity, new()
        {
            var table = this[tableName];

            var res = await table.ExecuteAsync(TableOperation.InsertOrMerge(new TableStorageEntityAdapter<T>(entity)));
            return res.Result is TableStorageEntityAdapter<T> obj ? obj.InnerObject : null;
        }

        public async Task<T> LookupEntity<T>(string tableName, string partitionKey, string rowKey) where T : BaseEntity, new()
        {
            var table = this[tableName];

            var res = await table.ExecuteAsync(TableOperation.Retrieve<TableStorageEntityAdapter<T>>(partitionKey, rowKey));
            return res.Result is TableStorageEntityAdapter<T> obj ? obj.InnerObject : null;
        }
    }
}
