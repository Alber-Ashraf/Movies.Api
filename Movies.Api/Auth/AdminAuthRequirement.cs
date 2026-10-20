using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace Movies.Api.Auth
{
    public class AdminAuthRequirement : IAuthorizationHandler, IAuthorizationRequirement
    {
        private readonly string _apiKey;
        public AdminAuthRequirement(string apiKey)
        {
            _apiKey = apiKey;
        }
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            if (context.User.HasClaim(AuthConstants.AdminUserClaimName, "true"))
            {
                context.Succeed(this);
                return Task.CompletedTask;
            }

            var httpContext = context.Resource as HttpContext;
            if (httpContext is null)
            {
                return Task.CompletedTask;
            }

            if (!httpContext.Request.Headers.TryGetValue(AuthConstants.ApiKeyHeaderName, out var extractedApiKey))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var apiKeyBytes = Encoding.UTF8.GetBytes(_apiKey);
            var extractedBytes = Encoding.UTF8.GetBytes(extractedApiKey.ToString());

            if (apiKeyBytes.Length != extractedBytes.Length ||
                !CryptographicOperations.FixedTimeEquals(apiKeyBytes, extractedBytes))
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var identity = httpContext.User.Identities.FirstOrDefault();
            if (identity is null)
            {
                identity = new ClaimsIdentity("ApiKey");
                httpContext.User.AddIdentity(identity);
            }

            if (!identity.HasClaim(c => c.Type == AuthConstants.AdminUserClaimName))
            {
                identity.AddClaim(new Claim(AuthConstants.AdminUserClaimName, "true"));
            }

            context.Succeed(this);
            return Task.CompletedTask;
        }
    }
}
