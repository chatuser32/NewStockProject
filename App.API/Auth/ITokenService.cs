using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace App.API.Auth
{
    public interface ITokenService
    {
        string CreateToken(int userId, string username, string fullName, IEnumerable<string> roles, TokenOptions options, out DateTime expiresAtUtc);
    }
}

