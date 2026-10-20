using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Endpoints.Identity
{
    public static class TokenEndpoint
    {
        public const string Name = "GenerateToken";

        public static IEndpointRouteBuilder MapTokenEndpoint(this IEndpointRouteBuilder app)
        {
            var handler = async (TokenGenerationRequest request, IConfiguration config) =>
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(config["Jwt:Key"] ?? "ForTheLoveOfGodStoreAndLoadThisSecurely");

                var userId = request.UserId ?? Guid.NewGuid();

                var claims = new List<Claim>
                {
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new(JwtRegisteredClaimNames.Sub, userId.ToString()),
                    new(JwtRegisteredClaimNames.Email, request.Email),
                    new("userid", userId.ToString())
                };

                foreach (var (claimName, claimValue) in request.CustomClaims)
                {
                    if (claimValue is JsonElement element)
                    {
                        var valueType = element.ValueKind switch
                        {
                            JsonValueKind.True => ClaimValueTypes.Boolean,
                            JsonValueKind.False => ClaimValueTypes.Boolean,
                            JsonValueKind.Number => ClaimValueTypes.Integer,
                            _ => ClaimValueTypes.String
                        };
                        claims.Add(new Claim(claimName, element.ToString(), valueType));
                    }
                    else
                    {
                        claims.Add(new Claim(claimName, claimValue?.ToString() ?? string.Empty));
                    }
                }

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddHours(4),
                    Issuer = config["Jwt:Issuer"],
                    Audience = config["Jwt:Audience"],
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                var jwt = tokenHandler.WriteToken(token);

                return TypedResults.Ok(new TokenResponse { Token = jwt });
            };

            app.MapPost(ApiEndpoints.Identity.Token, handler)
                .WithName(Name)
                .Produces<TokenResponse>(StatusCodes.Status200OK)
                .Produces<ValidationFailureResponse>(StatusCodes.Status400BadRequest)
                .AllowAnonymous();

            // Also map directly to /token for backward compatibility with consumer apps
            app.MapPost(ApiEndpoints.Identity.TokenDirect, handler)
                .ExcludeFromDescription()
                .AllowAnonymous();

            return app;
        }
    }
}
