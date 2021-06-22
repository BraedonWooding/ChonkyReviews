using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChonkyReviews.Models;
using ChonkyReviews.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChonkyReviews.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LocationController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly TableStorageService _tableStorage;

        public LocationController(ILogger<UserController> logger, TableStorageService tableStorage)
        {
            _logger = logger;
            _tableStorage = tableStorage;
        }

        [HttpGet]
        [Route("all")]
        public async Task<List<Location>> GetAllLocations()
        {
            return await _tableStorage.LookupAllEntities<Location>("Locations").ToListAsync();
        }

        [HttpPost]
        public async Task UpdateLocation([FromBody] LocationIn location)
        {
            if (string.IsNullOrEmpty(location.AccountId)) return;

            var locationId = "location_" + Guid.NewGuid().ToString("N");
            if ((await _tableStorage.MergeEntity("Locations", new Location(location.AccountId, locationId)
            {
                LocationName = location.LocationName
            })).Item3)
            {
                await _tableStorage.IncrementLedger("Location", "__Identity__", "__Identity__");
            }
        }

        [HttpGet]
        [Route("access")]
        public async Task<IActionResult> HasAccess(string accountId, string locationId, string userId)
        {
            if (await _tableStorage.LookupEntity("UsersToAccounts", new Mapping<User, Account>(new User(userId), new Account(accountId))) != null
                || await _tableStorage.LookupEntity("UsersToLocations", new Mapping<User, Location>(new User(userId), new Location(accountId, locationId))) != null)
            {
                return Ok();
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPut]
        [Route("access")]
        public async Task AddAccess(string accountId, string locationId, string userId)
        {
            if ((await _tableStorage.MergeEntity("UsersToLocations", new Mapping<User, Location>(new User(userId), new Location(accountId, locationId)))).Item3)
            {
                await _tableStorage.IncrementLedger("User", userId, "Locations");
                await _tableStorage.IncrementLedger("Location", locationId, "UsersWithAccess");
            }
        }

        [HttpDelete]
        [Route("access")]
        public async Task RemoveAccess(string accountId, string locationId, string userId)
        {
            if (await _tableStorage.DeleteEntity("UsersToLocations", new Mapping<User, Location>(new User(userId), new Location(accountId, locationId))))
            {
                await _tableStorage.DecrementLedger("User", userId, "Locations");
                await _tableStorage.DecrementLedger("Location", locationId, "UsersWithAccess");
            }
        }

        [HttpGet]
        public Task<Location> Get(string accountId, string locationId)
        {
            return _tableStorage.LookupEntity("Locations", new Location(accountId, locationId));
        }

        [HttpGet]
        [Route("forUser")]
        public async Task<List<Location>> GetForUser(string userId)
        {
            return await _tableStorage.LookupEntities<Mapping<User, Location>>("UsersToLocations", userId)
                .SelectAwait(async x => await _tableStorage.LookupEntity("Locations", new Location(x.ValueCategory, x.Value)))
                .Concat(
                    _tableStorage.LookupEntities<Mapping<User, Account>>("UsersToAccounts", userId)
                        .SelectMany(x => _tableStorage.LookupEntities<Location>("Locations", x.Value))
                )
                .ToListAsync();
        }
    }
}
