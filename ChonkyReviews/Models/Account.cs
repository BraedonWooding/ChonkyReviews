using System;
using Newtonsoft.Json;

namespace ChonkyReviews.Models
{
    public record AccountIn(string AccountName, Account.AccountType Type);

    public class Account : BaseEntity
    {
        public enum AccountType
        {
            ACCOUNT_TYPE_UNSPECIFIED = -1,
            PERSONAL = 0,
            LOCATION_GROUP = 1,
            USER_GROUP = 2,
            ORGANIZATION = 3,
        }

        public Account(string accountId)
        {
            this.AccountId = accountId;
        }

        public Account()
        {
        }

        [JsonProperty]
        public string AccountId { get; set; }

        [JsonProperty]
        public string Name => $"accounts/{AccountId}";

        [JsonProperty]
        public string AccountName { get; set; }

        [JsonProperty]
        public AccountType Type { get; set; }

        public override string PartitionKey { get => "Accounts"; set { } }
        public override string RowKey { get => AccountId; set => AccountId = value; }
    }
}
