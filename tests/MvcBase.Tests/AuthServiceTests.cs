using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using MvcBase.Web.Data;
using MvcBase.Web.Services;
using Xunit;

namespace MvcBase.Tests;

public class AuthServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly CookieAuthService _authService;

    public AuthServiceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();

        _authService = new CookieAuthService(_db);
    }

    [Fact]
    public async Task RegisterAsync_NewEmail_Succeeds()
    {
        var result = await _authService.RegisterAsync("new@example.com", "password123");

        Assert.True(result);
        Assert.Single(_db.Users);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_Fails()
    {
        await _authService.RegisterAsync("dup@example.com", "password123");

        var result = await _authService.RegisterAsync("dup@example.com", "password456");

        Assert.False(result);
        Assert.Single(_db.Users);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_CorrectPassword_ReturnsUser()
    {
        await _authService.RegisterAsync("valid@example.com", "password123");

        var user = await _authService.ValidateCredentialsAsync("valid@example.com", "password123");

        Assert.NotNull(user);
        Assert.Equal("valid@example.com", user!.Email);
    }

    [Fact]
    public async Task ValidateCredentialsAsync_WrongPassword_ReturnsNull()
    {
        await _authService.RegisterAsync("valid2@example.com", "password123");

        var user = await _authService.ValidateCredentialsAsync("valid2@example.com", "wrong");

        Assert.Null(user);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }
}
