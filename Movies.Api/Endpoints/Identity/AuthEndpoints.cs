using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using Movies.Api.Auth;
using Movies.Application.Auth;
using Movies.Application.Models;
using Movies.Application.Repositories.IRepositories;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.Endpoints.Identity
{
    public static class AuthEndpoints
    {
        public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost(ApiEndpoints.Identity.Register, async (
                RegisterRequest request,
                IValidator<RegisterRequest> validator,
                IUserRepository userRepository,
                IConfiguration config,
                CancellationToken token) =>
            {
                await validator.ValidateAndThrowAsync(request, token);

                var normalizedEmail = request.Email.Trim().ToLowerInvariant();
                if (await userRepository.ExistsByEmailAsync(normalizedEmail, token))
                {
                    return Results.Problem(
                        title: "Email already in use",
                        detail: "An account with this email address already exists.",
                        statusCode: StatusCodes.Status409Conflict);
                }

                var user = new AppUser
                {
                    Id = Guid.NewGuid(),
                    Email = normalizedEmail,
                    FullName = request.FullName.Trim(),
                    PasswordHash = PasswordHasher.Hash(request.Password),
                    Role = UserRoles.Member,
                    CreatedAt = DateTime.UtcNow
                };

                await userRepository.CreateAsync(user, token);
                var authResponse = CreateAuthResponse(user, config);
                return TypedResults.Created($"/api/identity/me", authResponse);
            })
            .WithName("RegisterUser")
            .Produces<AuthResponse>(StatusCodes.Status201Created)
            .Produces<ValidationFailureResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict)
            .AllowAnonymous();

            app.MapPost(ApiEndpoints.Identity.Login, async (
                LoginRequest request,
                IValidator<LoginRequest> validator,
                IUserRepository userRepository,
                IConfiguration config,
                CancellationToken token) =>
            {
                await validator.ValidateAndThrowAsync(request, token);

                var user = await userRepository.GetByEmailAsync(request.Email.Trim(), token);
                if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
                {
                    return Results.Problem(
                        title: "Invalid credentials",
                        detail: "The email or password you entered is incorrect.",
                        statusCode: StatusCodes.Status401Unauthorized);
                }

                var authResponse = CreateAuthResponse(user, config);
                return TypedResults.Ok(authResponse);
            })
            .WithName("LoginUser")
            .Produces<AuthResponse>(StatusCodes.Status200OK)
            .Produces<ValidationFailureResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .AllowAnonymous();

            app.MapGet(ApiEndpoints.Identity.Me, async (
                HttpContext context,
                IUserRepository userRepository,
                CancellationToken token) =>
            {
                var userId = context.GetUserId();
                if (userId is null)
                {
                    return Results.Unauthorized();
                }

                var user = await userRepository.GetByIdAsync(userId.Value, token);
                if (user is null)
                {
                    var email = context.User.FindFirstValue(ClaimTypes.Email)
                                ?? context.User.FindFirstValue(JwtRegisteredClaimNames.Email)
                                ?? "user@cinemovies.io";
                    var isAdmin = context.User.HasClaim(AuthConstants.AdminUserClaimName, "true");
                    var isTrusted = context.User.HasClaim(AuthConstants.TrustedMemberClaimName, "true");
                    var role = isAdmin ? UserRoles.Admin : isTrusted ? UserRoles.TrustedMember : UserRoles.Member;

                    return TypedResults.Ok(new UserResponse
                    {
                        Id = userId.Value,
                        Email = email,
                        FullName = email.Split('@')[0],
                        Role = role
                    });
                }

                return TypedResults.Ok(new UserResponse
                {
                    Id = user.Id,
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = user.Role
                });
            })
            .WithName("GetCurrentUser")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

            return app;
        }

        private static AuthResponse CreateAuthResponse(AppUser user, IConfiguration config)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(config["Jwt:Key"]!);
            var expiresAt = DateTime.UtcNow.AddHours(8);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email),
                new(JwtRegisteredClaimNames.Name, user.FullName),
                new("userid", user.Id.ToString()),
                new("role", user.Role)
            };

            if (string.Equals(user.Role, UserRoles.Admin, StringComparison.OrdinalIgnoreCase))
            {
                claims.Add(new Claim(AuthConstants.AdminUserClaimName, "true", ClaimValueTypes.Boolean));
                claims.Add(new Claim(AuthConstants.TrustedMemberClaimName, "true", ClaimValueTypes.Boolean));
            }
            else if (string.Equals(user.Role, UserRoles.TrustedMember, StringComparison.OrdinalIgnoreCase))
            {
                claims.Add(new Claim(AuthConstants.TrustedMemberClaimName, "true", ClaimValueTypes.Boolean));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiresAt,
                Issuer = config["Jwt:Issuer"],
                Audience = config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(securityToken);

            return new AuthResponse
            {
                Token = jwt,
                UserId = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                ExpiresAt = expiresAt
            };
        }
    }
}
