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
    public class AccountController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly TableStorageService _tableStorage;

        public AccountController(ILogger<UserController> logger, TableStorageService tableStorage)
        {
            _logger = logger;
            _tableStorage = tableStorage;
        }

        [HttpGet]
        [Route("all")]
        public async Task<List<Account>> GetAllAccounts()
        {
            return await _tableStorage.LookupAllEntities<Account>("Accounts").ToListAsync();
        }

        [HttpPost]
        public async Task UpdateAccount([FromBody] AccountIn account)
        {
            await _tableStorage.MergeEntity("Accounts", new Account("account_" + new Guid().ToString("N"))
            {
                Type = account.Type,
                AccountName = account.AccountName
            });
        }

        [HttpGet]
        [Route("access")]
        public async Task<IActionResult> HasAccess(string accountId, string email)
        {
            if (await _tableStorage.LookupEntity("UsersToAccounts", new Mapping<User, Account>(new User(email), new Account(accountId))) != null)
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
        public async Task AddAccess(string accountId, string email)
        {
            await _tableStorage.MergeEntity("UsersToAccounts", new Mapping<User, Account>(new User(email), new Account(accountId)));
        }

        [HttpDelete]
        [Route("access")]
        public async Task RemoveAccess(string accountId, string email)
        {
            await _tableStorage.DeleteEntity("UsersToAccounts", new Mapping<User, Account>(new User(email), new Account(accountId)));
        }

        [HttpGet]
        public Task<Account> Get(string accountId)
        {
            return _tableStorage.LookupEntity("Accounts", new Account(accountId));
        }

        [HttpGet]
        [Route("forUser")]
        public async Task<List<Account>> GetForUser([FromBody] UserIn user)
        {
            return await _tableStorage.LookupEntities<Mapping<User, Account>>("UsersToAccounts", user.Email)
                .SelectAwait(async x => await _tableStorage.LookupEntity("Accounts", new Account(x.Value))).ToListAsync();
        }
    }
}
