using System;
using System.Collections.Generic;
using ChonkyReviews.Models;
using Microsoft.Azure.Cosmos.Table;

namespace ChonkyReviews.Adapters
{
    internal class TableStorageEntityAdapter<T> : ITableEntity where T : BaseEntity, new()
    {
        /// <summary>
        /// Gets or sets the entity's partition key
        /// </summary>
        public string PartitionKey
        {
            get { return InnerObject.PartitionKey; }
            set { InnerObject.PartitionKey = value; }
        }

        /// <summary>
        /// Gets or sets the entity's row key.
        /// </summary>
        public string RowKey
        {
            get { return InnerObject.RowKey; }
            set { InnerObject.RowKey = value; }
        }

        /// <summary>
        /// Gets or sets the entity's Timestamp.
        /// </summary>
        public DateTimeOffset Timestamp
        {
            get { return InnerObject.Timestamp; }
            set { InnerObject.Timestamp = value; }
        }

        /// <summary>
        /// Gets or sets the entity's current ETag.
        /// Set this value to '*' in order to blindly overwrite an entity as part of an update operation.
        /// </summary>
        public string ETag
        {
            get { return InnerObject.ETag; }
            set { InnerObject.ETag = value; }
        }

        /// <summary>
        /// Place holder for the original entity
        /// </summary>
        public T InnerObject { get; set; }

        public TableStorageEntityAdapter()
        {
            // If you would like to work with objects that do not have a default Ctor you can use (T)Activator.CreateInstance(typeof(T));
            this.InnerObject = new T();
        }

        public TableStorageEntityAdapter(T innerObject)
        {
            this.InnerObject = innerObject;
        }

        public virtual void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            TableEntity.ReadUserObject(this.InnerObject, properties, operationContext);
        }

        public virtual IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            return TableEntity.WriteUserObject(this.InnerObject, operationContext);
        }
    }
}
