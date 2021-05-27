using System;
using Newtonsoft.Json;

namespace ChonkyReviews.Models
{
    public class Account
    {
        [JsonIgnore]
        public string AccountId;
    }
}
