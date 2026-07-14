using Microsoft.AspNetCore.Mvc;

namespace MvcBase.Web.Controllers;

[Route("error")]
public class ErrorController : Controller
{
    [Route("{statusCode:int}")]
    public IActionResult Handle(int statusCode)
    {
        Response.StatusCode = statusCode;
        return View(statusCode == 404 ? "Error404" : "Error500");
    }
}
