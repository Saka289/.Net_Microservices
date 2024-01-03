using Web.Service.EmailAPI.Models.Dto;
using Web.Services.EmailAPI.Message;

namespace Web.Service.EmailAPI.Services
{
    public interface IEmailService
    {
        Task EmailCartAndLog(CartDto cartDto);
        Task RegisterUserEmailAndLog(string email);
        Task LogOrderPlaced(RewardsMessage rewardsMessage);
    }
}
