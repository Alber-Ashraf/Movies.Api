
using System.Text;
using System.Threading.Tasks;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.IdentityModel.Tokens;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application;
using Movies.Application.DataBase;
using Movies.Application.Repositories;
using Movies.Application.Repositories.IRepositories;

namespace Movies.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
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
                        p => p.RequireClaim(AuthConstants.AdminUserClaimName, "true"));

                x.AddPolicy(AuthConstants.TrustedMenmberPolicyName, 
                    p => p.RequireAssertion(context =>
                        context.User.HasClaim(c => c.Type == AuthConstants.TrustedMemberClaimName &&
                                                   c.Value == "true") ||
                        context.User.HasClaim(c => c.Type == AuthConstants.AdminUserClaimName &&
                                                   c.Value == "true")));
            });

            builder.Services.AddApiVersioning(
                options =>
                {
                    options.DefaultApiVersion = new ApiVersion(1.0);
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.ReportApiVersions = true;
                    options.ApiVersionReader = new MediaTypeApiVersionReader("api-version");
                }
                ).AddMvc();
            ;

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddApplication();
            builder.Services.AddDatabase(config["Database:ConnectionString"]!);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<ValidationMappingMiddleware>();

            app.MapControllers();

            // Initialize the database
            var dbInitializer = app.Services.GetRequiredService<DbInitializer>();
            await dbInitializer.InitializeAsync();  

            app.Run();
        }
    }
}
