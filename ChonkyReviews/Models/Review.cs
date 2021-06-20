using System;
using Newtonsoft.Json;

namespace ChonkyReviews.Models
{
    public class Review
    {
        public enum StarRatings
        {
            STAR_RATING_UNSPECIFIED = -1,
            ONE = 1,
            TWO = 2,
            THREE = 3,
            FOUR = 4,
            FIVE = 5
        }

        public class Reply
        {
            [JsonProperty]
            public string Comment { get; private set; }

            // Zulu time
            [JsonProperty]
            public DateTimeOffset UpdateTime { get; private set; }
        }

        [JsonProperty]
        public string Name { get; private set; }

        [JsonProperty]
        public string ReviewId { get; private set; }

        [JsonProperty]
        public Reviewer Reviewer { get; private set; }

        [JsonProperty]
        public StarRatings StarRating { get; private set; }

        [JsonProperty]
        public string Comment { get; private set; }

        // Zulu time
        [JsonProperty]
        public DateTimeOffset CreateTime { get; private set; }

        [JsonProperty]
        public DateTimeOffset UpdateTime { get; private set; }

        [JsonProperty]
        public Reply ReviewReply { get; private set; }
    }
}
