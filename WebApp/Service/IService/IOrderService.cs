using WebApp.Models;

namespace WebApp.Service.IService
{
    public interface IOrderService
    {
        Task<ResponseDto?> CreateOrder(CartDto cartDto);
    }
}
