using Microsoft.AspNetCore.Mvc;

namespace MvcBase.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
