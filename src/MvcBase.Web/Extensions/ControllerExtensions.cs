using Microsoft.AspNetCore.Mvc;

namespace MvcBase.Web.Extensions;

public static class ControllerExtensions
{
    public static void Flash(this Controller controller, string type, string message)
    {
        controller.TempData["NotificationType"] = type;
        controller.TempData["NotificationMessage"] = message;
    }
}
