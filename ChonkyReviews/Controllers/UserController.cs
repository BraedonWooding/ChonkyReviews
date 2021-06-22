using System;
using System.Collections.Generic;
using System.Linq;
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
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly TableStorageService _tableStorage;

        public UserController(ILogger<UserController> logger, TableStorageService tableStorage)
        {
            _logger = logger;
            _tableStorage = tableStorage;
        }

        [HttpGet]
        [Route("all")]
        public async Task<List<User>> GetAllUsers()
        {
            return await _tableStorage.LookupAllEntities<User>("Users").ToListAsync();
        }

        [HttpPost]
        public async Task UpdateUser([FromBody] UserIn user)
        {
            if ((await _tableStorage.MergeEntity("Users", new User(user.UserId ?? Guid.NewGuid().ToString("N")) {
                ProfileName = user.ProfileName,
                Email = user.Email
            })).Item3)
            {
                await _tableStorage.IncrementLedger("User", "__Identity__", "__Identity__");
            }
        }

        [HttpGet]
        public Task<User> Get(string id)
        {
            return _tableStorage.LookupEntity<User>("Users", new User(id));
        }
    }
}
