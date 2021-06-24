using ChonkyReviews.Models;
using ChonkyReviews.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChonkyReviews.Controllers
{
    [ApiController]
    [Route("/gmock")]
    public class GoogleMock : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly TableStorageService _tableStorage;

        public GoogleMock(ILogger<UserController> logger, TableStorageService tableStorage)
        {
            _logger = logger;
            _tableStorage = tableStorage;
        }

        public record GMock_Account(string Name, string AccountName, Account.AccountType Type);

        [Route("/api/lookupUser")]
        [HttpGet]
        public async Task<ActionResult<string>> GetUserId(string email)
        {
            var entity = await _tableStorage.LookupEntity("UserMapping", new AggregatedMapping("User", email));
            return Ok(entity?.Reference);
        }

        private string GetUserToken()
        {
            // user's have to authorize that is put their user id
            return HttpContext.Request.Headers.TryGetValue("Authorization", out var bearer)
                ? Encoding.UTF8.GetString(Convert.FromBase64String(bearer[0]))
                : null;
        }

        public record GMock_GetAccountsResult(List<GMock_Account> Accounts, string NextPageToken);

        [Route("v4/accounts")] // Deprecated but we'll still support it
        [Route("accountmanagement/v1/accounts")]
        [HttpGet]
        public async Task<ActionResult<GMock_GetAccountsResult>> GetAccounts(string pageToken, int pageSize = 20)
        {
            // we don't support 'name' nor 'filter'
            // The user should encode their 'access' token
            string userToken = GetUserToken();
            if (userToken == null) return Unauthorized();

            string lastId = null;
            List<GMock_Account> accounts = await _tableStorage.LookupEntities<Mapping<User, Account>>("UserToAccounts", userToken, pageToken, pageSize)
                    .SelectAwait(async x =>
                    {
                        var account = await _tableStorage.LookupEntity<Account>("Accounts", new Account(x.RowKey));
                        lastId = account.AccountId;
                        return new GMock_Account(account.Name, account.AccountName, account.Type);
                    })
                    .ToListAsync();

            return Ok(new GMock_GetAccountsResult(accounts, lastId));
        }

        public record GMock_Location(string Name, string LocationName);
        public record GMock_GetLocationsResult(List<GMock_Location> Locations, string NextPageToken, long TotalSize);

        [Route("v4/accounts/{accountId}/locations")]
        [HttpGet]
        public async Task<ActionResult<GMock_GetLocationsResult>> GetLocations([FromRoute] string accountId, string pageToken, int pageSize = 20)
        {
            string userToken = GetUserToken();
            if (userToken == null) return Unauthorized();

            string lastId = null;

            if (!await CanAccess(accountId))
            {
                return null;
            }

            var locs = await _tableStorage.LookupEntities<Location>("Locations", accountId, pageToken, pageSize)
                .SelectAwait(async x =>
                {
                    if (!await CanAccess(null, x.LocationId))
                    {
                        return null;
                    }

                    lastId = x.LocationId;
                    return new GMock_Location(x.Name, x.LocationName);
                })
                .Where(x => x != null)
                .ToListAsync();

            long count = (await _tableStorage.ReadLedger("Account", accountId, "Locations")).Count;
            return Ok(new GMock_GetLocationsResult(locs, lastId, count));
        }

        public record GMock_ReviewReply(string Comment, string UpdateTime);
        public record GMock_Reviewer(string ProfilePhotoUrl, string DisplayName, bool IsAnonymous);
        public record GMock_Review(string Name, string ReviewId, GMock_Reviewer Reviewer, Review.StarRatings StarRating, string Comment, string CreateTime, string UpdateTime, GMock_ReviewReply ReviewReply);
        public record GMock_ListReviewsResult(List<GMock_Review> Reviews, double AverageRating, long TotalReviewCount, string NextPageToken);

        [Route("v4/accounts/{accountId}/locations/{locationId}/reviews")]
        [HttpGet]
        public async Task<ActionResult<GMock_ListReviewsResult>> ListReviews([FromRoute] string accountId, [FromRoute] string locationId, string pageToken, int pageSize = 20, string orderBy = "updateTime desc")
        {
            if (!new string[] { "rating", "rating desc", "updateTime desc" }.Contains(orderBy))
            {
                _logger.LogError("Invalid Order By: " + orderBy);
                return BadRequest("Invalid orderby");
            }

            // validate current user has access to location / account id
            if (!await CanAccess(accountId, locationId))
            {
                return Forbid();
            }

            bool isRating = orderBy.Contains("rating");
            bool isDesc = orderBy.Contains("desc");

            string userToken = GetUserToken();
            if (userToken == null) return Unauthorized();
            
            string lastId = null;

            string table = "ReviewsUpdateTimeMappingDesc";

            if (isDesc)
            {
                if (isRating)
                {
                    table = "ReviewsRatingMappingDesc";
                }
                else
                {
                    table = "ReviewsUpdateTimeMappingDesc";
                }
            }
            else if (isRating)
            {
                table = "ReviewsRatingMappingAsc";
            }

            var locs = _tableStorage.LookupEntities<AggregatedMapping>(table, locationId, pageToken, pageSize)
                .SelectAwait(async x =>
                {
                    var review = await _tableStorage.LookupEntity<Review>("Reviews", new Review(locationId, x.Reference));
                    lastId = x.RowKey;

                    return new GMock_Review(review.Name, review.ReviewId, new GMock_Reviewer(review.Reviewer.ProfilePictureUrl, review.Reviewer.DisplayName, review.Reviewer.IsAnonymous),
                        review.StarRating, review.Comment, review.UpdateTime.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                        review.UpdateTime.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'"),
                        new GMock_ReviewReply(review.ReviewReply.Comment, review.ReviewReply.UpdateTime.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'")));
                })
                .ToListAsync();

            var ledger = await _tableStorage.ReadLedger("Location", locationId, "Reviews");
            return Ok(new GMock_ListReviewsResult(await locs, (double)ledger.Sum / ledger.Count, ledger.Count, lastId));
        }

        private async Task<bool> CanAccess(string accountId, string locationId = null)
        {
            string userId = GetUserToken();

            if ((accountId != null && await _tableStorage.LookupEntity("UsersToAccounts", new Mapping<User, Account>(new User(userId), new Account(accountId))) != null)
                 || (locationId != null && await _tableStorage.LookupEntity("UsersToLocations", new Mapping<User, Location>(new User(userId), new Location(accountId, locationId))) != null))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        [Route("v4/accounts/{accountId}/locations/{locationId}/reviews/{reviewId}")]
        [HttpGet]
        public async Task<ActionResult<GMock_Review>> GetReview([FromRoute] string accountId, [FromRoute] string locationId, [FromRoute] string reviewId)
        {
            // validate current user has access to location / account id
            if (!await CanAccess(accountId, locationId))
            {
                return Forbid();
            }

            return Ok(await _tableStorage.LookupEntity<Review>("Reviews", locationId, reviewId));
        }

        [Route("v4/accounts/{accountId}/locations/{locationId}/reviews/{reviewId}/reply")]
        [HttpDelete]
        public async Task<IActionResult> DeleteReply([FromRoute] string accountId, [FromRoute] string locationId, [FromRoute] string reviewId)
        {
            // validate current user has access to location / account id
            if (!await CanAccess(accountId, locationId))
            {
                return Forbid();
            }

            return await _tableStorage.DeleteEntity<Review>("Reviews", new Review(locationId, reviewId)) ? Ok() : NotFound();
        }

        [Route("v4/accounts/{accountId}/locations/{locationId}/reviews/{reviewId}/reply")]
        [HttpPut]
        public async Task<IActionResult> UpdateReply([FromRoute] string accountId, [FromRoute] string locationId, [FromRoute] string reviewId, [FromBody] GMock_ReviewReply reply)
        {
            // validate current user has access to location / account id
            if (!await CanAccess(accountId, locationId))
            {
                return Forbid();
            }

            var entity = await _tableStorage.LookupEntity<Review>("Reviews", new Review(locationId, reviewId));
            if (entity == null)
            {
                return NotFound();
            }

            entity.ReviewReply = new Review.Reply()
            {
                Comment = reply.Comment,
                UpdateTime = DateTimeOffset.UtcNow,
            };

            var cur = await _tableStorage.MergeEntity<Review>("Reviews", entity);
            return Ok(cur.Item1);
        }
    }
}
