using Microsoft.AspNetCore.Identity;

namespace Pharmacy.Models
{
    public class Users : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
    }
}
