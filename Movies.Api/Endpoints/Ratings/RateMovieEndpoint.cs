using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Auth;
using Movies.Application.Services.IServices;
using Movies.Contracts.Requests;

namespace Movies.Api.Endpoints.Ratings
{
    public static class RateMovieEndpoint
    {
        public const string Name = "RateMovie";
        public static IEndpointRouteBuilder MapRateMovie(this IEndpointRouteBuilder app)
        {
            app.MapPut(ApiEndpoints.Movies.Rate,
                    async (Guid id, RateMovieRequest request,
                        HttpContext context, IRatingService ratingService,
                        IOutputCacheStore outputCacheStore,
                        CancellationToken token) =>
                    {
                        var userId = context.GetUserId();
                        if (userId is null)
                        {
                            return Results.Unauthorized();
                        }
                        var result = await ratingService.RateMovieAsync(id, userId.Value, request.Rating, token);
                        if (!result)
                        {
                            return Results.NotFound();
                        }
                        await outputCacheStore.EvictByTagAsync("movies", token);
                        return Results.Ok();
                    })
                .WithName(Name)
                .Produces(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .Produces(StatusCodes.Status404NotFound)
                .RequireAuthorization();
            return app;
        }
    }
}
