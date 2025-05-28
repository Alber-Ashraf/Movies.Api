using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Movies.Application.DataBase;
using Movies.Application.Repositories;
using Movies.Application.Repositories.IRepositories;
using Movies.Application.Services;
using Movies.Application.Services.IServices;
using Npgsql;

namespace Movies.Application
{
    public static class ApplicationServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddSingleton<IRatingRepository, RatingRepository>();
            services.AddSingleton<IRatingService, RatingService>();
            services.AddSingleton<IMovieRepository, MovieRepository>();
            services.AddSingleton<IMovieService, MovieService>();
            services.AddValidatorsFromAssemblyContaining<IApplicationMarker>(ServiceLifetime.Singleton);
            return services;
        }
        public static IServiceCollection AddDatabase(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton<IDbConnectionFactory>(_ => new DbConnectionFactory(connectionString));
            services.AddSingleton<DbInitializer>();
            return services;
        }
    }
}
