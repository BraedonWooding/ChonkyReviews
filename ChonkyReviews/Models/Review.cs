using System;
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
            public string Comment { get; private set; }

            // Zulu time
            public DateTimeOffset UpdateTime { get; private set; }
        }

        public string Name { get; private set; }
        public string ReviewId { get; private set; }
        public Reviewer Reviewer { get; private set; }
        public StarRatings StarRating { get; private set; }
        public string Comment { get; private set; }

        // Zulu time
        public DateTimeOffset CreateTime { get; private set; }
        public DateTimeOffset UpdateTime { get; private set; }
        public Reply ReviewReply { get; private set; }
    }
}
