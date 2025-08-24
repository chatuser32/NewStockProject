using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using App.Repositories.Users;
using App.Repositories.Auth;
using App.Repositories;
using App.Repositories.UserRoles;
using Microsoft.EntityFrameworkCore;

namespace App.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository userRepository;
        private readonly IGenericRepository<RefreshToken> refreshTokenRepository;

        public AuthService(IUserRepository userRepository, IGenericRepository<RefreshToken> refreshTokenRepository)
        {
            this.userRepository = userRepository;
            this.refreshTokenRepository = refreshTokenRepository;
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

        public async Task<(bool Success, int? UserId, string? Error)> RegisterAsync(string username, string fullName, string email, string password)
        {
            var exists = await userRepository.Where(u => u.Username == username || u.Email == email).AnyAsync();
            if (exists)
                return (false, null, "Kullanıcı adı veya e-posta zaten kayıtlı.");

            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[16];
            rng.GetBytes(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(32);

            var user = new User
            {
                Username = username,
                FullName = fullName,
                Email = email,
                PasswordSalt = salt,
                PasswordHash = hash,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            await userRepository.AddAsync(user);
            return (true, user.Id, null);
        }

        public async Task<(bool Success, string? RefreshToken, DateTime? ExpiresAtUtc, string? Error)> IssueRefreshTokenAsync(int userId)
        {
            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var expires = DateTime.UtcNow.AddDays(7);
            await refreshTokenRepository.AddAsync(new RefreshToken
            {
                UserId = userId,
                Token = token,
                ExpiresAtUtc = expires
            });
            return (true, token, expires, null);
        }

        public async Task<(bool Success, User? User, List<string> Roles, string? Error)> ValidateRefreshTokenAsync(string refreshToken)
        {
            var rt = await refreshTokenRepository.Where(r => r.Token == refreshToken).FirstOrDefaultAsync();
            if (rt == null || !rt.IsActive)
                return (false, null, new List<string>(), "Refresh token geçersiz veya süresi dolmuş.");

            var user = await userRepository
                .GetAll()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstAsync(u => u.Id == rt.UserId);

            var roles = user.UserRoles.Select(ur => ur.Role.Name).Distinct().ToList();

            // rotation: eski token'ı revoke et ve yeni token üretmek üst akışa bırakılabilir
            rt.RevokedAtUtc = DateTime.UtcNow;
            rt.ReplacedByToken = "rotated";
            refreshTokenRepository.Update(rt);

            return (true, user, roles, null);
        }

        public async Task<(bool Success, string? Error)> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await userRepository.GetByIdAsync(userId);
            if (user == null)
                return (false, "Kullanıcı bulunamadı.");
            if (user.PasswordHash == null || user.PasswordSalt == null)
                return (false, "Kullanıcı için parola set edilmemiş.");

            if (!VerifyPassword(currentPassword, user.PasswordSalt, user.PasswordHash))
                return (false, "Mevcut parola hatalı.");

            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[16];
            rng.GetBytes(salt);
            using var pbkdf2 = new Rfc2898DeriveBytes(newPassword, salt, 100_000, HashAlgorithmName.SHA256);
            var hash = pbkdf2.GetBytes(32);
            user.PasswordSalt = salt;
            user.PasswordHash = hash;
            userRepository.Update(user);
            return (true, null);
        }
        private static bool VerifyPassword(string password, byte[] salt, byte[] hash)
        {
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            var computed = pbkdf2.GetBytes(32);
            return CryptographicOperations.FixedTimeEquals(computed, hash);
        }
    }
}

