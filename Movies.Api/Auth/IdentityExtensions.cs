using System.Security.Claims;

namespace Movies.Api.Auth
{
    public static class IdentityExtensions
    {
        public static Guid? GetUserId(this HttpContext context)
        {
            var userIdClaim = context.User.Claims.FirstOrDefault(x =>
                x.Type == "userid" ||
                x.Type == ClaimTypes.NameIdentifier ||
                x.Type == "sub");

            if (Guid.TryParse(userIdClaim?.Value, out var parsedId))
            {
                return parsedId;
            }
            return null;
        }
    }
}
