using Web.Services.AuthAPI.Models;

namespace Web.Services.AuthAPI.Service.IService
{
    public interface IJwtTokenGenerator
    {
        public string GenerateToken(ApplicationUser applicationUser, IEnumerable<string> roles);
    }
}
