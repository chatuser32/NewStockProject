using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using App.Repositories.Users;
using App.Repositories.UserRoles;
using Microsoft.EntityFrameworkCore;

namespace App.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository userRepository;

        public AuthService(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        public async Task<(bool Success, User? User, List<string> Roles, string? Error)> AuthenticateAsync(string username, string password)
        {
            var user = await userRepository
                .GetAll()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (user == null)
                return (false, null, new List<string>(), "Kullanıcı bulunamadı veya pasif.");

            if (user.PasswordHash == null || user.PasswordSalt == null)
                return (false, null, new List<string>(), "Kullanıcının parolası tanımlı değil.");

            if (!VerifyPassword(password, user.PasswordSalt, user.PasswordHash))
                return (false, null, new List<string>(), "Kullanıcı adı veya parola hatalı.");

            var roles = user.UserRoles.Select(ur => ur.Role.Name).Distinct().ToList();
            return (true, user, roles, null);
        }

        private static bool VerifyPassword(string password, byte[] salt, byte[] hash)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            var computed = pbkdf2.GetBytes(32);
            return CryptographicOperations.FixedTimeEquals(computed, hash);
        }
    }
}

