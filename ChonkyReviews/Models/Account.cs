﻿using System;
using Newtonsoft.Json;

namespace ChonkyReviews.Models
{
    public class Account
    {
        public enum AccountType
        {
            ACCOUNT_TYPE_UNSPECIFIED = -1,
            PERSONAL = 0,
            LOCATION_GROUP = 1,
            USER_GROUP = 2,
            ORGANIZATION = 3,
        }

        [JsonIgnore]
        public string AccountId { get; private set; }

        [JsonProperty]
        public string Name => $"accounts/{AccountId}";

        [JsonProperty]
        public string AccountName { get; private set; }

        [JsonProperty]
        public AccountType Type { get; private set; }
    }
}
