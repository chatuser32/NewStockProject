using System.Threading.Tasks;
using System.Collections.Generic;
using App.Repositories.Users;

namespace App.Services.Auth
{
    public interface IAuthService
    {
        Task<(bool Success, User? User, List<string> Roles, string? Error)> AuthenticateAsync(string username, string password);
    }
}

