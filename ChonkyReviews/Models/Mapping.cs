using System;
using System.Collections.Generic;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace ChonkyReviews.Models
{
    public class Mapping<A, B> : BaseEntity
        where A : BaseEntity
        where B : BaseEntity
    {
        public Mapping() { }

        public Mapping(A a, B b)
        {
            this.Key = a.RowKey;
            this.Value = b.RowKey;
            this.KeyCategory = a.PartitionKey;
            this.ValueCategory = b.PartitionKey;
        }

        [JsonProperty]
        public string Key { get; set; }

        [JsonProperty]
        public string Value { get; set; }

        [JsonProperty]
        public string KeyCategory { get; set; }

        [JsonProperty]
        public string ValueCategory { get; set; }

        [JsonIgnore]
        public override string PartitionKey { get => Key; set => Key = value; }

        [JsonIgnore]
        public override string RowKey { get => Value; set => Value = value; }
    }
}
