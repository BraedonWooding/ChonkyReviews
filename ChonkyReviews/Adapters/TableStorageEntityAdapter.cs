using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using ChonkyReviews.Models;
using Microsoft.Azure.Cosmos.Table;

namespace ChonkyReviews.Adapters
{
    internal class TableStorageEntityAdapter<T> : ITableEntity where T : BaseEntity, new()
    {
        private static readonly object _syncLock = new();
        private static List<AdditionalPropertyMetadata> _additionalProperties;

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

        public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            InnerObject = new T();

            TableEntity.ReadUserObject(this, properties, operationContext);
            TableEntity.ReadUserObject(InnerObject, properties, operationContext);

            var additionalMappings = GetAdditionPropertyMappings(InnerObject, operationContext);

            if (additionalMappings.Count > 0)
            {
                ReadAdditionalProperties(properties, additionalMappings);
            }

            ReadInnerObjects(properties, operationContext);
        }

        public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var properties = TableEntity.WriteUserObject(InnerObject, operationContext);

            var additionalMappings = GetAdditionPropertyMappings(InnerObject, operationContext);

            if (additionalMappings.Count > 0)
            {
                WriteAdditionalProperties(additionalMappings, properties);
            }

            WriteInnerObjects(properties, operationContext);

            return properties;
        }

        protected void ClearCache()
        {
            lock (_syncLock)
            {
                _additionalProperties = null;
            }
        }

        protected virtual TypeConverter GetTypeConverter(PropertyInfo propertyInfo)
        {
            var propertyType = propertyInfo.PropertyType;

            return TypeDescriptor.GetConverter(propertyType);
        }

        protected virtual void ReadAdditionalProperty(PropertyInfo propertyInfo, EntityProperty propertyInnerObject)
        {
            var converter = GetTypeConverter(propertyInfo);
            if (propertyInfo.GetCustomAttribute<IgnorePropertyAttribute>() != null)
            {
                return;
            }

            try
            {
                var convertedInnerObject = converter.ConvertFromInvariantString(propertyInnerObject.StringValue);

                if (propertyInfo.CanWrite)
                {
                    propertyInfo.SetValue(InnerObject, convertedInnerObject);
                }
            }
            catch (NotSupportedException ex)
            {
                const string MessageFormat = "Failed to write string value to the '{0}' property.";
                var message = string.Format(CultureInfo.CurrentCulture, MessageFormat, propertyInfo.Name);

                throw new NotSupportedException(message, ex);
            }
        }

        protected virtual void ReadInnerObjects(
            IDictionary<string, EntityProperty> properties,
            OperationContext operationContext)
        {
        }

        protected virtual void WriteAdditionalProperty(
            IDictionary<string, EntityProperty> properties,
            PropertyInfo propertyInfo,
            object propertyInnerObject)
        {
            var converter = GetTypeConverter(propertyInfo);

            if (propertyInfo.GetCustomAttribute<IgnorePropertyAttribute>() != null)
            {
                return;
            }

            try
            {
                var convertedInnerObject = converter.ConvertToInvariantString(propertyInnerObject);

                properties[propertyInfo.Name] = EntityProperty.GeneratePropertyForString(convertedInnerObject);
            }
            catch (NotSupportedException ex)
            {
                const string MessageFormat = "Failed to convert property '{0}' to a string value.";
                var message = string.Format(CultureInfo.CurrentCulture, MessageFormat, propertyInfo.Name);

                throw new NotSupportedException(message, ex);
            }
        }

        protected virtual void WriteInnerObjects(
            IDictionary<string, EntityProperty> properties,
            OperationContext operationContext)
        {
        }

        private static List<AdditionalPropertyMetadata> GetAdditionPropertyMappings(
            T value,
            OperationContext operationContext)
        {
            if (_additionalProperties != null)
            {
                return _additionalProperties;
            }

            List<AdditionalPropertyMetadata> additionalProperties;

            lock (_syncLock)
            {
                // Check the mappings again to protect against race conditions on the lock
                if (_additionalProperties != null)
                {
                    return _additionalProperties;
                }

                additionalProperties = ResolvePropertyMappings(value, operationContext);

                _additionalProperties = additionalProperties;
            }

            return additionalProperties;
        }

        private static List<AdditionalPropertyMetadata> ResolvePropertyMappings(
            T value,
            OperationContext operationContext)
        {
            var storageSupportedProperties = TableEntity.WriteUserObject(value, operationContext);
            var objectProperties = value.GetType().GetTypeInfo().GetProperties();
            var infrastructureProperties = typeof(ITableEntity).GetTypeInfo().GetProperties();
            var missingProperties =
                objectProperties.Where(
                    objectProperty => storageSupportedProperties.ContainsKey(objectProperty.Name) == false);

            var additionalProperties = missingProperties.Select(
                x => new AdditionalPropertyMetadata
                {
                    IsInfrastructureProperty = infrastructureProperties.Any(y => x.Name == y.Name),
                    PropertyMetadata = x
                });

            return additionalProperties.ToList();
        }

        private void ReadAdditionalProperties(
            IDictionary<string, EntityProperty> properties,
            IEnumerable<AdditionalPropertyMetadata> additionalMappings)
        {
            // Populate the properties missing from ReadUserObject
            foreach (var additionalMapping in additionalMappings)
            {
                var propertyType = additionalMapping.PropertyMetadata.PropertyType;

                if (additionalMapping.IsInfrastructureProperty)
                {
                    ReadInfrastructureProperty(additionalMapping, propertyType);
                }
                else if (properties.ContainsKey(additionalMapping.PropertyMetadata.Name))
                {
                    // This is a property that has an unsupport type
                    // Use a converter to resolve and apply the correct value
                    var propertyInnerObject = properties[additionalMapping.PropertyMetadata.Name];

                    ReadAdditionalProperty(additionalMapping.PropertyMetadata, propertyInnerObject);
                }

                // The else case here is that the model now contains a property that was not originally stored when the entity was last written
                // This property will assume the default value for its type
            }
        }

        private void ReadInfrastructureProperty(AdditionalPropertyMetadata additionalMapping, Type propertyType)
        {
            // We don't want to use a string conversion here
            // Explicitly map the types across
            if (additionalMapping.PropertyMetadata.Name == nameof(ITableEntity.Timestamp) &&
                propertyType == typeof(DateTimeOffset))
            {
                additionalMapping.PropertyMetadata.SetValue(InnerObject, Timestamp);
            }
            else if (additionalMapping.PropertyMetadata.Name == nameof(ITableEntity.ETag) &&
                     propertyType == typeof(string))
            {
                additionalMapping.PropertyMetadata.SetValue(InnerObject, ETag);
            }
            else if (additionalMapping.PropertyMetadata.Name == nameof(ITableEntity.PartitionKey) &&
                     propertyType == typeof(string))
            {
                additionalMapping.PropertyMetadata.SetValue(InnerObject, PartitionKey);
            }
            else if (additionalMapping.PropertyMetadata.Name == nameof(ITableEntity.RowKey) &&
                     propertyType == typeof(string))
            {
                additionalMapping.PropertyMetadata.SetValue(InnerObject, RowKey);
            }
            else
            {
                const string UnsupportedPropertyMessage =
                    "The {0} interface now defines a property {1} which is not supported by this adapter.";

                var message = string.Format(
                    CultureInfo.CurrentCulture,
                    UnsupportedPropertyMessage,
                    typeof(ITableEntity).FullName,
                    additionalMapping.PropertyMetadata.Name);

                throw new InvalidOperationException(message);
            }
        }

        private void WriteAdditionalProperties(
            IEnumerable<AdditionalPropertyMetadata> additionalMappings,
            IDictionary<string, EntityProperty> properties)
        {
            // Populate the properties missing from WriteUserObject
            foreach (var additionalMapping in additionalMappings)
            {
                if (additionalMapping.IsInfrastructureProperty)
                {
                    // We need to let the storage mechanism handle the write of the infrastructure properties
                    continue;
                }

                var propertyInnerObject = additionalMapping.PropertyMetadata.GetValue(InnerObject);

                WriteAdditionalProperty(properties, additionalMapping.PropertyMetadata, propertyInnerObject);
            }
        }

        private struct AdditionalPropertyMetadata
        {
            public bool IsInfrastructureProperty { get; set; }

            public PropertyInfo PropertyMetadata { get; set; }

        }
    }
}
