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
        private readonly AuthService _auth;

        public UserController(ILogger<UserController> logger, TableStorageService tableStorage, AuthService auth)
        {
            _logger = logger;
            _tableStorage = tableStorage;
            _auth = auth;
        }

        [HttpGet]
        public Task<User> Get()
        {
            return _tableStorage.LookupEntity<User>("Users", "abc", "demo@chonky.com");
        }

        [HttpGet]
        public async Task Auth()
        {
            await _auth.AddAuthCookie(HttpContext, "abc", "demo@chonky.com");
        }
    }
}
