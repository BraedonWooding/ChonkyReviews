using System;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace ChonkyReviews.Models
{
    public class AggregatedMapping : BaseEntity
    {
        public AggregatedMapping() { }

        public AggregatedMapping(string partition, string row, string reference = null)
        {
            this.PartitionKey = partition;
            this.RowKey = row + (reference != null ? "_" + reference : "");
            this.Reference = reference;
        }

        [JsonProperty]
        public string Reference { get; set; }

        [JsonProperty]
        public string Partition { get; set; }

        [JsonProperty]
        public string Row { get; set; }

        [JsonProperty]
        public override string PartitionKey { get => Partition; set => Partition = value; }

        [JsonProperty]
        public override string RowKey { get => Row; set => Row = value; }
    }
}
