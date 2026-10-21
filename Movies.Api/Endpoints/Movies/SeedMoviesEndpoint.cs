using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Auth;
using Movies.Application.DataBase;

namespace Movies.Api.Endpoints.Movies
{
    public static class SeedMoviesEndpoint
    {
        public const string Name = "SeedMovies";

        public static IEndpointRouteBuilder MapSeedMovies(this IEndpointRouteBuilder app)
        {
            app.MapPost(ApiEndpoints.Movies.Seed, async (
                DataSeeder seeder,
                IOutputCacheStore outputCacheStore,
                CancellationToken token) =>
            {
                await seeder.SeedAsync(forceMovies: true, token);
                await outputCacheStore.EvictByTagAsync("movies", token);
                return TypedResults.Ok(new { message = "Catalog and enterprise users successfully seeded." });
            })
            .WithName(Name)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .RequireAuthorization(AuthConstants.AdminUserPolicyName);

            return app;
        }
    }
}
