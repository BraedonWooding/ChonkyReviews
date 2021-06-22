using System;
using System.ComponentModel;
using ChonkyReviews.Adapters;
using Microsoft.Azure.Cosmos.Table;
using Newtonsoft.Json;

namespace ChonkyReviews.Models
{
    public record ReviewIn(ReviewerIn Reviewer, Review.StarRatings StarRating, string Comment, Review.Reply ReviewReply);

    public class Review : BaseEntity
    {
        public enum StarRatings
        {
            STAR_RATING_UNSPECIFIED = -1,
            ONE = 1,
            TWO = 2,
            THREE = 3,
            FOUR = 4,
            FIVE = 5,
        }

        public class Reply
        {
            [JsonProperty]
            public string Comment { get; set; }

            // Zulu time
            [JsonProperty]
            public DateTimeOffset UpdateTime { get; set; }
        }

        public Review()
        {
        }

        public Review(string locationId)
        {
            this.LocationId = locationId;
        }

        public Review(string locationId, string userId)
        {
            this.ReviewId = userId;
            this.LocationId = locationId;
        }

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public string ReviewId { get; set; }

        [JsonProperty, IgnoreProperty]
        public Reviewer Reviewer { get; set; }

        [JsonProperty]
        public string ReviewerJson
        {
            get => JsonConvert.SerializeObject(Reviewer);
            set => Reviewer = JsonConvert.DeserializeObject<Reviewer>(value);
        }

        [JsonProperty]
        public StarRatings StarRating { get; set; }

        [JsonProperty]
        public string Comment { get; set; }

        // Zulu time
        [JsonProperty]
        public DateTimeOffset CreateTime { get; set; }

        [JsonProperty]
        public DateTimeOffset UpdateTime { get; set; }

        [JsonProperty, IgnoreProperty]
        public Reply ReviewReply { get; set; }

        [JsonProperty]
        public string ReviewReplyJson
        {
            get => JsonConvert.SerializeObject(ReviewReply);
            set => ReviewReply = JsonConvert.DeserializeObject<Reply>(value);
        }

        [JsonProperty]
        public string LocationId { get; set; }
        
        public override string PartitionKey { get => LocationId; set => LocationId = value; }
        public override string RowKey { get => ReviewId; set => ReviewId = value; }
    }
}
