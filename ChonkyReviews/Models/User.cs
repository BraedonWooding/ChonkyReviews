using System;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace ChonkyReviews.Models
{
    public record UserIn(string Email, string ProfileName);

    public class User : BaseEntity
    {
        public User() { }

        public User(string email)
        {
            this.Email = email;
        }

        [JsonProperty]
        public string UserId { get; set; }

        [JsonProperty]
        public string ProfileName { get; set; }

        [JsonProperty]
        public string DisplayName { get => ProfileName; set { } }

        [JsonProperty]
        public string Email { get; set; }

        [JsonIgnore]
        public string Category => "User";

        [JsonIgnore]
        public override string PartitionKey { get => "User"; set { } }

        [JsonIgnore]
        public override string RowKey { get => Email; set => Email = value; }
    }
}
