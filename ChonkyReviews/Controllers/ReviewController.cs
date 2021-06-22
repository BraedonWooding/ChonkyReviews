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
            var updateTime = DateTimeOffset.UtcNow;
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
                    UpdateTime = updateTime
                },
                UpdateTime = updateTime,
                CreateTime = updateTime,
                StarRating = review.StarRating,
            });

            int oldValue = old == null ? 0 : Math.Max((int)old.StarRating, 0);
            int curValue = cur == null ? 0 : Math.Max((int)cur.StarRating, 0);

            if (old != null)
            {
                Task.WaitAll(_tableStorage.DeleteEntity("ReviewsRatingMappingAsc", new AggregatedMapping(locationId, oldValue.ToString(), cur.Reviewer.UserId)),
                    _tableStorage.DeleteEntity("ReviewsRatingMappingDesc", new AggregatedMapping(locationId, (5 - oldValue).ToString(), cur.Reviewer.UserId)),
                    _tableStorage.DeleteEntity("ReviewsUpdateTimeMappingDesc", new AggregatedMapping(locationId, (DateTimeOffset.MaxValue.UtcTicks - old.UpdateTime.UtcTicks).ToString("d19"), cur.Reviewer.UserId)));
            }

            Task.WaitAll(
             _tableStorage.MergeEntity("ReviewsRatingMappingAsc", new AggregatedMapping(locationId, curValue.ToString(), cur.Reviewer.UserId)),
             _tableStorage.MergeEntity("ReviewsRatingMappingDesc", new AggregatedMapping(locationId, (5 - curValue).ToString(), cur.Reviewer.UserId)),
             _tableStorage.MergeEntity("ReviewsUpdateTimeMappingDesc", new AggregatedMapping(locationId, (DateTimeOffset.MaxValue.UtcTicks - cur.UpdateTime.UtcTicks).ToString("d19"), cur.Reviewer.UserId)));

            if ((inserted || old.StarRating == 0) && cur.StarRating != 0)
            {
                Task.WaitAll(_tableStorage.IncrementLedger("Account", accountId, "Reviews", curValue),
                    _tableStorage.IncrementLedger("User", cur.Reviewer.UserId, "Reviews", curValue),
                    _tableStorage.IncrementLedger("Location", locationId, "Reviews", curValue),
                    _tableStorage.IncrementLedger("Reviews", "__Identity__", "__Identity__", curValue)
                );
            }
            else if (cur.StarRating != 0)
            {
                Task.WaitAll(
                _tableStorage.MutateSumOfLedger("Account", accountId, "Reviews", curValue - oldValue),
                 _tableStorage.MutateSumOfLedger("User", cur.Reviewer.UserId, "Reviews", curValue - oldValue),
                 _tableStorage.MutateSumOfLedger("Location", locationId, "Reviews", curValue - oldValue),
                 _tableStorage.MutateSumOfLedger("Reviews", "__Identity__", "__Identity__", curValue - oldValue)
                    );
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
