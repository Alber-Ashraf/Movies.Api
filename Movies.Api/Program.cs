
using System.Threading.Tasks;
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

            app.UseAuthorization();


            app.MapControllers();

            // Initialize the database
            var dbInitializer = app.Services.GetRequiredService<DbInitializer>();
            await dbInitializer.InitializeAsync();  

            app.Run();
        }
    }
}
