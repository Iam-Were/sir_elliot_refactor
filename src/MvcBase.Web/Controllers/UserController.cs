using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MvcBase.Web.Extensions;
using MvcBase.Web.Models.ViewModels;
using MvcBase.Web.Services;

namespace MvcBase.Web.Controllers;

public class UserController : Controller
{
    private readonly IAuthService _authService;

    public UserController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _authService.ValidateCredentialsAsync(model.Email, model.Password);
        if (user is null)
        {
            this.Flash("error", "Invalid email or password.");
            return View(model);
        }

        await _authService.SignInAsync(HttpContext, user);
        this.Flash("success", "Welcome back!");
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var created = await _authService.RegisterAsync(model.Email, model.Password);
        if (!created)
        {
            this.Flash("error", "Errore durante la registrazione.");
            ModelState.AddModelError(nameof(model.Email), "An account with this email already exists.");
            return View(model);
        }

        this.Flash("success", "Registrazione completata con successo!");
        return RedirectToAction(nameof(Login));
    }

    [Authorize]
    [HttpGet]
    public IActionResult Profile()
    {
        var email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
        var role = User.FindFirst(ClaimTypes.Role)?.Value ?? string.Empty;
        return View(new ProfileViewModel { Email = email, Role = role });
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _authService.SignOutAsync(HttpContext);
        return RedirectToAction(nameof(Login));
    }
}
