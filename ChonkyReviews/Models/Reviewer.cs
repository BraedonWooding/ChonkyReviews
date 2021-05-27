using System;
namespace ChonkyReviews.Models
{
    public class Reviewer
    {
        public string ProfilePictureUrl { get; private set; }
        public string DisplayName { get; private set; }
        public bool IsAnonymous { get; private set; }
    }
}
