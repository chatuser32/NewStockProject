using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;

namespace App.Repositories
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var currentDir = new DirectoryInfo(Directory.GetCurrentDirectory());
            string appsettingsBasePath;
            if (string.Equals(currentDir.Name, "App.API", StringComparison.OrdinalIgnoreCase))
            {
                appsettingsBasePath = currentDir.FullName;
            }
            else
            {
                var dir = currentDir;
                while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "App.API")))
                {
                    dir = dir.Parent;
                }
                appsettingsBasePath = dir != null
                    ? Path.Combine(dir.FullName, "App.API")
                    : currentDir.FullName;
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(appsettingsBasePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var conn = configuration.GetSection(ConnectionStringOption.Key)
                                     .Get<ConnectionStringOption>()
                                     ?.MySqlConnection
                      ?? Environment.GetEnvironmentVariable("MYSQL_CONNECTION")
                      ?? "Server=localhost;Database=stockdb;Uid=root;Password=;";

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 34));
            optionsBuilder.UseMySql(conn, serverVersion, opts =>
            {
                opts.MigrationsAssembly(typeof(RepositoryAssembly).Assembly.FullName);
            });

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}

