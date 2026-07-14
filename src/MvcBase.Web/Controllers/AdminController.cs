using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MvcBase.Web.Authorization;
using MvcBase.Web.Data;

namespace MvcBase.Web.Controllers;

[Authorize(Policy = PolicyNames.Admin)]
public class AdminController : Controller
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Dashboard()
    {
        var users = await _db.Users.OrderBy(u => u.Email).ToListAsync();
        return View(users);
    }
}
