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
        public async Task UpdateReview(string accountId, string locationId, [FromBody] ReviewIn review)
        {
            var (cur, old, inserted) = await _tableStorage.MergeEntity("Reviews", new Review(locationId, review.Reviewer.UserId) {
                Reviewer = new Reviewer()
                {
                    DisplayName = review.Reviewer.DisplayName,
                    UserId = review.Reviewer.UserId
                },
                AccountId = accountId,
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

            int oldValue = old == null ? 0 : Math.Max((int)old.StarRating, 0);
            int curValue = cur == null ? 0 : Math.Max((int)cur.StarRating, 0);

            if ((inserted || old.StarRating == 0) && cur.StarRating != 0)
            {
                await _tableStorage.IncrementLedger("Account", accountId, "Reviews", curValue);
                await _tableStorage.IncrementLedger("User", review.Reviewer.UserId, "Reviews", curValue);
                await _tableStorage.IncrementLedger("Location", locationId, "Reviews", curValue);
                await _tableStorage.IncrementLedger("Reviews", cur.RowKey, "__Identity__", curValue);
            }
            else if (cur.StarRating != 0)
            {
                await _tableStorage.MutateSumOfLedger("Account", accountId, "Reviews", curValue - oldValue);
                await _tableStorage.MutateSumOfLedger("User", review.Reviewer.UserId, "Reviews", curValue - oldValue);
                await _tableStorage.MutateSumOfLedger("Location", locationId, "Reviews", curValue - oldValue);
                await _tableStorage.MutateSumOfLedger("Reviews", cur.RowKey, "__Identity__", curValue - oldValue);
            }
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
            return _tableStorage.LookupEntity<Review>("Reviews", new Review(locationId, userId));
        }
    }
}
