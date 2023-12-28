using Web.Services.ShoppingCartAPI.Models.Dto;

namespace Web.Services.ShoppingCartAPI.Service.IService
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetProducts();
    }
}
