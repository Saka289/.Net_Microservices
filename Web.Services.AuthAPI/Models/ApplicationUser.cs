using Microsoft.AspNetCore.Identity;

namespace Web.Services.AuthAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
    }
}
