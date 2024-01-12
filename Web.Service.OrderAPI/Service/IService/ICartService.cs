namespace Web.Services.OrderAPI.Service.IService
{
    public interface ICartService
    {
        Task<bool> UpdateCart(string userId);
    }
}
