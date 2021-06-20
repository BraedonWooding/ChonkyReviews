using System;
namespace ChonkyReviews.Models
{
    public abstract class BaseEntity
    {
        public string ETag { get; set; }

        public abstract string PartitionKey { get; set; }
        public abstract string RowKey { get; set; }
        public DateTimeOffset Timestamp { get; set; }

        public BaseEntity()
        {
        }
    }
}
