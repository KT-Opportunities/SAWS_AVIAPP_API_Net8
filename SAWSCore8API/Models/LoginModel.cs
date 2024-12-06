using System.ComponentModel.DataAnnotations;

namespace SAWSCore8API.Models;

public class LoginModel
{
    [Required(ErrorMessage = "User Name is required")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; }

    // [Display(Name = "Remember me?")]
    // public bool RememberMe { get; set; }

}
