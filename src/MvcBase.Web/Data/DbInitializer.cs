using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MvcBase.Web.Models;

namespace MvcBase.Web.Data;

public static class DbInitializer
{
    public static void SeedAdmin(AppDbContext db, IConfiguration config, ILogger logger)
    {
        db.Database.Migrate();

        var adminEmail = config["Seed:AdminEmail"];
        var adminPassword = config["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
        {
            logger.LogInformation("Seed:AdminEmail / Seed:AdminPassword not configured - skipping admin seed.");
            return;
        }

        if (db.Users.Any(u => u.Email == adminEmail))
        {
            return;
        }

        var hasher = new PasswordHasher<User>();
        var admin = new User { Email = adminEmail, Role = UserRole.Admin };
        admin.PasswordHash = hasher.HashPassword(admin, adminPassword);

        db.Users.Add(admin);
        db.SaveChanges();
        logger.LogInformation("Seeded admin account {Email}.", adminEmail);
    }
}
