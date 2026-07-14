using System.ComponentModel.DataAnnotations;

namespace MvcBase.Web.Models.ViewModels;

public sealed class LoginViewModel
{
    [Required, EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;
}
