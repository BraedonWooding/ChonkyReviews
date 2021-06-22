using System;
using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ChonkyReviews.Adapters;
using ChonkyReviews.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.DataProtection;

namespace ChonkyReviews.Services
{
    public class AuthService
    {
        private const string COOKIE_NAME = "Chonky__Reviews__Auth";

        private readonly IDataProtector _protector;
        private readonly TableStorageService _tableStorage;

        public AuthService(IConfiguration config, IDataProtectionProvider provider, TableStorageService tableStorage)
        {
            _protector = provider.CreateProtector(config["PrivateKey"]);
            _tableStorage = tableStorage;
        }

        public async Task AddAuthCookie(HttpContext context, string googleId, string email)
        {
        }

        public async Task<User> LookupCurrentUser(HttpContext context)
        {
            if (context.Request.Cookies.TryGetValue(COOKIE_NAME, out var internalIdEncrypted))
            {

            }

            return null;
        }
    }
}
