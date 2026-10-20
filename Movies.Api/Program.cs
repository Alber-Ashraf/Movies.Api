
using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Movies.Api.Auth;
using Movies.Api.Endpoints;
using Movies.Api.Health;
using Movies.Api.Mapping;
using Movies.Api.Swagger;
using Movies.Application;
using Movies.Application.DataBase;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Movies.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var config = builder.Configuration;

            builder.Services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(x =>
            {
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = config["Jwt:Audience"],
                    ValidateIssuer = true,
                    ValidateAudience = true,
                };
            });

            builder.Services.AddAuthorization(x =>
            {
                x.AddPolicy(AuthConstants.AdminUserPolicyName,
                    p => p.AddRequirements(new AdminAuthRequirement(config["ApiKey"] ?? "")));

                x.AddPolicy(AuthConstants.TrustedMemberPolicyName,
                    p => p.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == AuthConstants.TrustedMemberClaimName &&
                                                   c.Value == "true") ||
                        context.User.HasClaim(c => c.Type == AuthConstants.AdminUserClaimName &&
                                                   c.Value == "true")));
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AngularApp", policy =>
                {
                    var allowedOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>()
                        ?? new[] { "http://localhost:4200" };
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            builder.Services.AddProblemDetails();

            builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1.0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new MediaTypeApiVersionReader("api-version");
            }).AddApiExplorer();

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddHealthChecks()
                .AddCheck<DatabaseHealthCheck>(DatabaseHealthCheck.Name);

            builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            builder.Services.AddSwaggerGen(x => x.OperationFilter<SwaggerDefaultValues>());

            builder.Services.AddOutputCache(x =>
            {
                x.AddBasePolicy(c => c.Cache());
                x.AddPolicy("MovieCache", c =>
                    c.Cache()
                    .Expire(TimeSpan.FromMinutes(1))
                    .SetVaryByQuery(new[] { "title", "year", "genre", "sortBy", "page", "pageSize" })
                    .SetVaryByHeader("Authorization")
                    .Tag("movies"));
            });

            builder.Services.AddApplication();
            builder.Services.AddDatabase(config["Database:ConnectionString"]!);

            var app = builder.Build();

            app.CreateApiVersionSet();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(x =>
                {
                    foreach (var description in app.DescribeApiVersions())
                    {
                        x.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                            description.GroupName);
                    }
                });
            }

            app.Use(async (context, next) =>
            {
                context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Append("X-Frame-Options", "DENY");
                context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
                context.Response.Headers.Append("X-XSS-Protection", "0");
                await next();
            });

            app.MapHealthChecks("_health");

            app.UseHttpsRedirection();

            app.UseCors("AngularApp");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseOutputCache();

            app.UseMiddleware<ValidationMappingMiddleware>();

            app.MapApiEndpoints();

            var dbInitializer = app.Services.GetRequiredService<DbInitializer>();
            await dbInitializer.InitializeAsync();

            app.Run();
        }
    }
}
