using MvcBase.Web.Models;

namespace MvcBase.Web.Services;

public interface IAuthService
{
    Task<bool> RegisterAsync(string email, string password);

    Task<User?> ValidateCredentialsAsync(string email, string password);

    Task SignInAsync(HttpContext httpContext, User user);

    Task SignOutAsync(HttpContext httpContext);
}
