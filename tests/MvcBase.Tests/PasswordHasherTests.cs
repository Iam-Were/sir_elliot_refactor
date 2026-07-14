using Microsoft.AspNetCore.Identity;
using MvcBase.Web.Models;
using Xunit;

namespace MvcBase.Tests;

public class PasswordHasherTests
{
    private readonly PasswordHasher<User> _hasher = new();

    [Fact]
    public void HashPassword_ThenVerify_Succeeds()
    {
        var user = new User { Email = "test@example.com" };
        var hash = _hasher.HashPassword(user, "correct-horse-battery-staple");

        var result = _hasher.VerifyHashedPassword(user, hash, "correct-horse-battery-staple");

        Assert.Equal(PasswordVerificationResult.Success, result);
    }

    [Fact]
    public void VerifyHashedPassword_WithWrongPassword_Fails()
    {
        var user = new User { Email = "test@example.com" };
        var hash = _hasher.HashPassword(user, "correct-horse-battery-staple");

        var result = _hasher.VerifyHashedPassword(user, hash, "wrong-password");

        Assert.Equal(PasswordVerificationResult.Failed, result);
    }
}
