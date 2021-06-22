using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChonkyReviews.Models;
using ChonkyReviews.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ChonkyReviews.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly ILogger<ReviewController> _logger;
        private readonly TableStorageService _tableStorage;

        public ReviewController(ILogger<ReviewController> logger, TableStorageService tableStorage)
        {
            _logger = logger;
            _tableStorage = tableStorage;
        }

        [HttpGet]
        [Route("all")]
        public async Task<List<Review>> GetAllReviews()
        {
            return await _tableStorage.LookupAllEntities<Review>("Reviews").ToListAsync();
        }

        [HttpPost]
        public async Task UpdateReview(string locationId, [FromBody] ReviewIn review)
        {
            await _tableStorage.MergeEntity("Reviews", new Review(locationId, BitConverter.ToString(Encoding.UTF8.GetBytes(review.Reviewer.UserId)).Replace("-", "")) {
                Reviewer = new Reviewer()
                {
                    DisplayName = review.Reviewer.DisplayName,
                    UserId = review.Reviewer.UserId
                },
                Comment = review.Comment,
                ReviewReply = new Review.Reply()
                {
                    Comment = review.ReviewReply.Comment,
                    UpdateTime = DateTimeOffset.UtcNow
                },
                UpdateTime = DateTimeOffset.UtcNow,
                CreateTime = DateTimeOffset.UtcNow,
                StarRating = review.StarRating,
            });
        }

        [HttpGet]
        [Route("forLocation")]
        public async Task<List<Review>> GetForLocation(string locationId)
        {
            return await _tableStorage.LookupEntities<Review>("Reviews", locationId).ToListAsync();
        }

        [HttpGet]
        public Task<Review> Get(string locationId, string userId)
        {
            return _tableStorage.LookupEntity<Review>("Reviews", new Review(locationId, BitConverter.ToString(Encoding.UTF8.GetBytes(userId)).Replace("-", "")));
        }
    }
}
