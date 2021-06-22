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
    public class VisitorController : ControllerBase
    {
        private readonly ILogger<VisitorController> _logger;
        private readonly TableStorageService _tableStorage;

        public VisitorController(ILogger<VisitorController> logger, TableStorageService tableStorage)
        {
            _logger = logger;
            _tableStorage = tableStorage;
        }

        [HttpGet]
        [Route("all")]
        public async Task<List<User>> GetAllVisitors()
        {
            return await _tableStorage.LookupAllEntities<User>("Visitors").ToListAsync();
        }

        [HttpPost]
        public async Task UpdateVisitor([FromBody] UserIn user)
        {
            await _tableStorage.MergeEntity("Visitors", new User(user.UserId ?? Guid.NewGuid().ToString("N")) {
                ProfileName = user.ProfileName,
                Email = user.Email
            });
        }

        [HttpGet]
        public Task<User> Get(string userId)
        {
            return _tableStorage.LookupEntity<User>("Visitors", new User(userId));
        }
    }
}
