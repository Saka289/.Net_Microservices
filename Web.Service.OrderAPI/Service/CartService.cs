using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;
using Web.Service.OrderAPI.Models.Dto;
using Web.Services.OrderAPI.Service.IService;

namespace Web.Services.ShoppingCartAPI.Service
{
    public class CartService : ICartService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CartService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }
        public async Task<bool> UpdateCart(string userId)
        {
            var client = _httpClientFactory.CreateClient("Cart");
            var jsonContent = JsonConvert.SerializeObject(userId);
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"/api/CartAPI/UpdateCart", httpContent);
            var apiContent = await response.Content.ReadAsStringAsync();
            var resp = JsonConvert.DeserializeObject<ResponseDto>(apiContent);
            if (resp.IsSuccess)
            {
                return true;
            }
            return false;
        }
    }
}
