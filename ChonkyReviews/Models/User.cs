using System;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace ChonkyReviews.Models
{
    public class User : BaseEntity
    {
        [JsonProperty]
        public string UserId;

        [JsonProperty]
        public string ProfileName;

        [JsonProperty]
        public string InternalId;

        [JsonIgnore]
        public string Email;

        [JsonIgnore]
        public string GoogleId;

        public override string PartitionKey { get => GoogleId; set => GoogleId = value; }
        public override string RowKey { get => Email; set => Email = value; }
    }
}
