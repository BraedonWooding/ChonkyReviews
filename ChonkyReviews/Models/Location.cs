using System;
using Newtonsoft.Json;

namespace ChonkyReviews.Models
{
    public class Location
    {
        [JsonIgnore]
        public string LocationId;

        [JsonIgnore]
        public Account Account;

        [JsonProperty]
        public string Name => $"accounts/{Account.AccountId}/locations/{LocationId}";

        public Location()
        {

        }
    }
}
