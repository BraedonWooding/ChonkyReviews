using System;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace ChonkyReviews.Models
{
    public class Location : BaseEntity
    {
        [JsonIgnore, IgnoreProperty]
        public string LocationId { get; private set; }

        [JsonIgnore, IgnoreProperty]
        public string AccountId { get; private set; }

        [JsonIgnore, IgnoreProperty]
        public Account Account => null;

        [JsonProperty, IgnoreProperty]
        public string Name => $"accounts/{AccountId}/locations/{LocationId}";

        [JsonProperty]
        public string LocationName { get; private set; }

        [JsonIgnore]
        public override string PartitionKey { get => AccountId; set => AccountId = value; }

        [JsonIgnore]
        public override string RowKey { get => LocationId; set => LocationId = value; }
    }
}
