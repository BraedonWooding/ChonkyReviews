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
            await _tableStorage.MergeEntity("Locations", new Location(location.AccountId, "location_" + new Guid().ToString("N"))
            {
                LocationName = location.LocationName
            }); ;
        }

        [HttpGet]
        [Route("access")]
        public async Task<IActionResult> HasAccess(string accountId, string locationId, string email)
        {
            if (await _tableStorage.LookupEntity("UsersToAccounts", new Mapping<User, Account>(new User(email), new Account(accountId))) != null
                || await _tableStorage.LookupEntity("UsersToLocations", new Mapping<User, Location>(new User(email), new Location(accountId, locationId))) != null)
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
        public async Task AddAccess(string accountId, string locationId, string email)
        {
            await _tableStorage.MergeEntity("UsersToLocations", new Mapping<User, Location>(new User(email), new Location(accountId, locationId)));
        }

        [HttpDelete]
        [Route("access")]
        public async Task RemoveAccess(string accountId, string locationId, string email)
        {
            await _tableStorage.DeleteEntity("UsersToLocations", new Mapping<User, Location>(new User(email), new Location(accountId, locationId)));
        }

        [HttpGet]
        public Task<Location> Get(string accountId, string locationId)
        {
            return _tableStorage.LookupEntity("Locations", new Location(accountId, locationId));
        }

        [HttpGet]
        [Route("forUser")]
        public async Task<List<Location>> GetForUser([FromBody] UserIn user)
        {
            return await _tableStorage.LookupEntities<Mapping<User, Location>>("UsersToLocations", user.Email)
                .SelectAwait(async x => await _tableStorage.LookupEntity("Locations", new Location(x.ValueCategory, x.Value))).ToListAsync();
        }
    }
}
