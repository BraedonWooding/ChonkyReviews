using System;
using Newtonsoft.Json;

namespace ChonkyReviews.Models
{
    public class Reviewer
    {
        [JsonProperty]
        public string ProfilePictureUrl { get; private set; }

        [JsonProperty]
        public string DisplayName { get; private set; }

        [JsonProperty]
        public bool IsAnonymous { get; private set; }
    }
}
