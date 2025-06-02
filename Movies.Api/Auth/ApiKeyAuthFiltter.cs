using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Movies.Api.Auth
{
    public class ApiKeyAuthFiltter : IAuthorizationFilter
    {
        private readonly IConfiguration _configuration;
        public ApiKeyAuthFiltter(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(AuthConstants.ApiKeyHeaderName, out var extractedApiKey))
            {
                context.Result = new UnauthorizedObjectResult("ApiKey Missing");
                return;
            }
            var apiKey = _configuration["ApiKey"];
            if (apiKey != extractedApiKey)
            {
                context.Result = new UnauthorizedObjectResult("Invalid ApiKey");
            }
        }
    }
}
