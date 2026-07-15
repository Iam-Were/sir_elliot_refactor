using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MvcBase.Web.Models;

namespace MvcBase.Web.Data;

public sealed class SeedUser
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = nameof(UserRole.User);
}

public static class DbInitializer
{
    public static void Seed(AppDbContext db, IConfiguration config, ILogger logger)
    {
        db.Database.Migrate();

        SeedUserIfMissing(db, logger,
            config["Seed:AdminEmail"],
            config["Seed:AdminPassword"],
            nameof(UserRole.Admin));

        var extraUsers = config.GetSection("Seed:Users").Get<SeedUser[]>();
        if (extraUsers is not null)
        {
            foreach (var user in extraUsers)
            {
                SeedUserIfMissing(db, logger, user.Email, user.Password, user.Role);
            }
        }
    }

    private static void SeedUserIfMissing(
        AppDbContext db, ILogger logger, string? email, string? password, string roleName)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        if (db.Users.Any(u => u.Email == email))
        {
            return;
        }

        var role = Enum.TryParse<UserRole>(roleName, ignoreCase: true, out var parsed)
            ? parsed
            : UserRole.User;

        var hasher = new PasswordHasher<User>();
        var user = new User { Email = email, Role = role };
        user.PasswordHash = hasher.HashPassword(user, password);

        db.Users.Add(user);
        db.SaveChanges();
        logger.LogInformation("Seeded user {Email} ({Role}).", email, role);
    }
}
