using Infrastructure.Storage.Abstractions;
using Infrastructure.Storage.Abstractions.CustomAttributes;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Storage.AzureTableStorage
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly CloudTable _cloudTable;
        private readonly PropertyInfo _rowKeyProp;
        private readonly PropertyInfo _partitionKeyProp;

        public Repository(CloudTable cloudTable)
        {
            _cloudTable = cloudTable;
            _rowKeyProp = typeof(TEntity).GetProperties().First(x => Attribute.IsDefined(x, typeof(RowKeyAttribute)));
            _partitionKeyProp = typeof(TEntity).GetProperties().FirstOrDefault(x => Attribute.IsDefined(x, typeof(PartitionKeyAttribute)));
        }

        public async Task Save(TEntity entity, CancellationToken cancellationToken)
        {
            //Flatten object of type Order) and convert it to EntityProperty Dictionary
            Dictionary<string, EntityProperty> flattenedProperties = EntityPropertyConverter.Flatten(entity, new OperationContext());
            var rowKey = GetEntityId(entity);
            var partitionKey = GetEntityPartitionKey(entity);

            // Create a DynamicTableEntity and set its PK and RK
            DynamicTableEntity dynamicTableEntity = new DynamicTableEntity(partitionKey, rowKey);
            dynamicTableEntity.Properties = flattenedProperties;

            var existingEntity = await Get(dynamicTableEntity.PartitionKey, dynamicTableEntity.RowKey, cancellationToken);

            TableOperation createOrReplaceOperation;
            if (existingEntity == null)
            {
                createOrReplaceOperation = TableOperation.Insert(dynamicTableEntity);
            }
            else
            {
                dynamicTableEntity.ETag = existingEntity.ETag;
                createOrReplaceOperation = TableOperation.Replace(dynamicTableEntity);
            }

            await _cloudTable.ExecuteAsync(createOrReplaceOperation);
        }

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task<TEntity> GetById(Guid id, CancellationToken cancellationToken)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            throw new NotSupportedException("This method is not supported for azure table storage implementation.  Please use the GetByKeys methods that use partition key and row key");
        }

        public async Task<TEntity> GetByIds(Guid partitionId, Guid rowId, CancellationToken cancellationToken)
        {
            var dynamicTableEntity = await Get(partitionId.ToString(), rowId.ToString(), cancellationToken);
            if (dynamicTableEntity == null)
                return null;

            return EntityPropertyConverter.ConvertBack<TEntity>(dynamicTableEntity.Properties, new OperationContext());

        }

        public async Task<TEntity> GetByKeys(string partitionKey, string rowKey, CancellationToken cancellationToken)
        {
            var dynamicTableEntity = await Get(partitionKey, rowKey, cancellationToken);
            if (dynamicTableEntity == null)
                return null;

            return EntityPropertyConverter.ConvertBack<TEntity>(dynamicTableEntity.Properties, new OperationContext());

        }

        private async Task<DynamicTableEntity> Get(string partitionKey, string rowKey, CancellationToken cancellationToken)
        {
            //Read the entity back from AzureTableStorage as DynamicTableEntity using the same PK and RK
            TableOperation tableOperation = TableOperation.Retrieve<DynamicTableEntity>(partitionKey, rowKey);
            TableResult tableResult = await _cloudTable.ExecuteAsync(tableOperation);
            return tableResult.Result as DynamicTableEntity;
        }

        private string GetEntityId(TEntity entity)
        {
            return _rowKeyProp.GetValue(entity).ToString();
        }

        private string GetEntityPartitionKey(TEntity entity)
        {
            var partitionKey = _partitionKeyProp != null ? _partitionKeyProp.GetValue(entity).ToString() : null;
            if(!string.IsNullOrWhiteSpace(partitionKey))
                    return partitionKey;

            return typeof(TEntity).Name; //if no partition key specified on the entity then default to entity name as partition key
        }
        
    }
}
