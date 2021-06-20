using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChonkyReviews.Models;
using ChonkyReviews.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ChonkyReviews.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountsController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly TableStorageService _tableStorage;

        public AccountsController(ILogger<UserController> logger, TableStorageService tableStorage)
        {
            _logger = logger;
            _tableStorage = tableStorage;
        }

        [HttpGet]
        public Task<User> Get()
        {
            return _tableStorage.LookupEntity<User>("Users", "abc", "demo@chonky.com");
        }
    }
}
