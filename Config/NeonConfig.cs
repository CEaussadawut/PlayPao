using System;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;

namespace PlayPao.Config
{
    public static class DatabaseConfig
    {
        public static string GetConnectionString(IConfiguration configuration, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                string host = Environment.GetEnvironmentVariable("NEON_HOST") ?? "";
                string database = Environment.GetEnvironmentVariable("NEON_DATABASE") ?? "";
                string username = Environment.GetEnvironmentVariable("NEON_USERNAME") ?? "";
                string password = Environment.GetEnvironmentVariable("NEON_PASSWORD") ?? "";

                var neonConnection = configuration.GetConnectionString("NeonConnection")
                    ?? throw new InvalidOperationException("Connection string 'NeonConnection' not found.");

                return string.Format(neonConnection, host, database, username, password);
            }
            else
            {
                return Environment.GetEnvironmentVariable("NEON_CONNECTION_STRING")
                    ?? configuration.GetConnectionString("NeonConnection")
                    ?? throw new InvalidOperationException("Connection string not found.");
            }
        }
    }
}
