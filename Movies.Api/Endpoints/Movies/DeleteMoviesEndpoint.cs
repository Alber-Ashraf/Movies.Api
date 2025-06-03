using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Application.Services.IServices;
using Movies.Contracts.Requests;

namespace Movies.Api.Endpoints.Movies
{
    public static class DeleteMoviesEndpoint
    {
        public static string Name = "DeleteMovies";
        public static IEndpointRouteBuilder MapDeleteMovie(this IEndpointRouteBuilder app)
        {
            app.MapDelete(ApiEndpoints.Movies.Delete, async (
                Guid id, IMovieService movieService,
                IOutputCacheStore outputCacheStore, CancellationToken token) =>
            {
                var deleted = await movieService.DeleteAsync(id, token);
                if (!deleted)
                {
                    return Results.NotFound();
                }
                // Evict the cache for movies tag
                await outputCacheStore.EvictByTagAsync("movies", token);
                return TypedResults.Ok();
            })
                .WithName(Name)
                .RequireAuthorization(AuthConstants.TrustedMenmberPolicyName);
            return app;
        }
    }
}
