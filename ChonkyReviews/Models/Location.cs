using System;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace ChonkyReviews.Models
{
    public record LocationIn(string AccountId, string LocationName);

    public class Location : BaseEntity
    {
        public Location()
        {
        }

        public Location(string accountId, string locationId)
        {
            this.AccountId = accountId;
            this.LocationId = locationId;
        }

        [JsonProperty]
        public string LocationId { get; set; }

        [JsonProperty]
        public string AccountId { get; set; }

        [JsonProperty]
        public string Name => $"accounts/{AccountId}/locations/{LocationId}";

        [JsonProperty]
        public string LocationName { get; set; }

        [JsonIgnore]
        public override string PartitionKey { get => AccountId; set => AccountId = value; }

        [JsonIgnore]
        public override string RowKey { get => LocationId; set => LocationId = value; }
    }
}
