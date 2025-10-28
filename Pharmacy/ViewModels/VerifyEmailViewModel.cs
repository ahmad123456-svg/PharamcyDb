using System.ComponentModel.DataAnnotations;

namespace Pharmacy.ViewModels
{
    public class VerifyEmailViewModel
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress]
       
    public string Email { get; set; } = string.Empty;
    }
}
