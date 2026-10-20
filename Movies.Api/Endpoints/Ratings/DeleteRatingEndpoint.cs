using Microsoft.AspNetCore.OutputCaching;
using Movies.Api.Auth;
using Movies.Application.Services.IServices;

namespace Movies.Api.Endpoints.Ratings
{
    public static class DeleteRatingEndpoint
    {
        public const string Name = "DeleteRating";
        public static IEndpointRouteBuilder MapDeleteRating(this IEndpointRouteBuilder app)
        {
            app.MapDelete(ApiEndpoints.Movies.DeleteRating,
                    async (Guid id, HttpContext context, IRatingService ratingService,
                        IOutputCacheStore outputCacheStore,
                        CancellationToken token) =>
                    {
                        var userId = context.GetUserId();
                        if (userId is null)
                        {
                            return Results.Unauthorized();
                        }
                        var result = await ratingService.DeleteRatingAsync(id, userId.Value, token);
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
