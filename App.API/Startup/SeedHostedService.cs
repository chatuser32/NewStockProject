using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using App.Repositories;
using App.Repositories.Roles;
using App.Repositories.UserRoles;
using App.Repositories.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Security.Cryptography;

namespace App.API.Startup
{
    public class SeedHostedService : IHostedService
    {
        private readonly IServiceProvider _services;
        public SeedHostedService(IServiceProvider services) { _services = services; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.MigrateAsync(cancellationToken);

            var roles = new[] { "Admin", "Editor", "User" };
            foreach (var r in roles)
            {
                if (!await db.Set<Role>().AnyAsync(x => x.Name == r, cancellationToken))
                    await db.Set<Role>().AddAsync(new Role { Name = r }, cancellationToken);
            }
            await db.SaveChangesAsync(cancellationToken);

            // Admin kullanıcı
            if (!await db.Set<User>().AnyAsync(u => u.Username == "admin", cancellationToken))
            {
                using var rng = RandomNumberGenerator.Create();
                var salt = new byte[16];
                rng.GetBytes(salt);
                using var pbkdf2 = new Rfc2898DeriveBytes("Admin123!", salt, 100_000, HashAlgorithmName.SHA256);
                var hash = pbkdf2.GetBytes(32);

                var admin = new User
                {
                    Username = "admin",
                    FullName = "System Administrator",
                    Email = "admin@example.com",
                    PasswordSalt = salt,
                    PasswordHash = hash,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };
                await db.Set<User>().AddAsync(admin, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);

                var adminRole = await db.Set<Role>().FirstAsync(r => r.Name == "Admin", cancellationToken);
                await db.Set<UserRole>().AddAsync(new UserRole { UserId = admin.Id, RoleId = adminRole.Id }, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

