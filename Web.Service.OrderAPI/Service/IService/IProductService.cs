using Web.Service.OrderAPI.Models.Dto;

namespace Web.Services.OrderAPI.Service.IService
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetProducts();
    }
}
