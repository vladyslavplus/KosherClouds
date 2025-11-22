using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;

namespace KosherClouds.ServiceDefaults.Handlers
{
    public class AuthenticationDelegatingHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthenticationDelegatingHandler> _logger;

        public AuthenticationDelegatingHandler(
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuthenticationDelegatingHandler> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext != null)
            {
                var token = httpContext.Request.Headers["Authorization"].FirstOrDefault();

                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Authorization = AuthenticationHeaderValue.Parse(token);
                    _logger.LogDebug("Forwarded authorization token to {Uri}", request.RequestUri);
                }
                else
                {
                    _logger.LogWarning("No authorization token found for request to {Uri}", request.RequestUri);
                }
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
