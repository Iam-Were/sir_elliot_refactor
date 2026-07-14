using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MvcBase.Web.Data;
using MvcBase.Web.Models;

namespace MvcBase.Web.Services;

public sealed class CookieAuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher<User> _hasher = new();

    public CookieAuthService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> RegisterAsync(string email, string password)
    {
        var exists = await _db.Users.AnyAsync(u => u.Email == email);
        if (exists)
        {
            return false;
        }

        var user = new User { Email = email, Role = UserRole.User };
        user.PasswordHash = _hasher.HashPassword(user, password);

        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<User?> ValidateCredentialsAsync(string email, string password)
    {
        var user = await _db.Users.SingleOrDefaultAsync(u => u.Email == email);
        if (user is null)
        {
            return null;
        }

        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
        return result == PasswordVerificationResult.Success ? user : null;
    }

    public async Task SignInAsync(HttpContext httpContext, User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
    }

    public async Task SignOutAsync(HttpContext httpContext)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
