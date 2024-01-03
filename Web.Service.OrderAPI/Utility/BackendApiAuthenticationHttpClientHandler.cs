using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

namespace Web.Services.OrderAPI.Utility
{
    public class BackendApiAuthenticationHttpClientHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _contextAccessor;

        public BackendApiAuthenticationHttpClientHandler(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            var token = await _contextAccessor.HttpContext.GetTokenAsync("access_token");
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return await base.SendAsync(requestMessage, cancellationToken);
        }
    }
}
