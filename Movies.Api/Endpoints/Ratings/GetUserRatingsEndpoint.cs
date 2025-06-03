using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services.IServices;
using Movies.Contracts.Responses;

namespace Movies.Api.Endpoints.Ratings
{
    public static class GetUserRatingsEndpoint
    {
        public const string Name = "GetUserRatings";
        public static IEndpointRouteBuilder MapGetUserRatings(this IEndpointRouteBuilder app)
        {
            app.MapGet(ApiEndpoints.Ratings.GetUserRating,
                    async (HttpContext context, IRatingService ratingService,
                        CancellationToken token) =>
                    {
                        var userId = context.GetUserId();
                        var ratings = await ratingService.movieRatingsAsync(userId!.Value, token);
                        return TypedResults.Ok(ratings);
                    })
                .WithName(Name);
            return app;
        }
    }
}
