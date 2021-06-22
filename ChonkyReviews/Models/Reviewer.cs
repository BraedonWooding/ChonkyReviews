using System;
using System.ComponentModel;
using ChonkyReviews.Adapters;
using Newtonsoft.Json;

namespace ChonkyReviews.Models
{
    public record ReviewerIn(string DisplayName, string UserId);

    public class Reviewer
    {
        [JsonProperty]
        public string ProfilePictureUrl { get; set; }

        [JsonProperty]
        public string DisplayName { get; set; }

        [JsonProperty]
        public bool IsAnonymous { get; set; }

        [JsonProperty]
        public string UserId { get; set; }
    }
}
