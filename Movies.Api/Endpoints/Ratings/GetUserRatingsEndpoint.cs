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
                        if (userId is null)
                        {
                            return Results.Unauthorized();
                        }
                        var ratings = await ratingService.GetRatingsForUserAsync(userId.Value, token);
                        var response = ratings.MapToResponse();
                        return TypedResults.Ok(response);
                    })
                .WithName(Name)
                .Produces<IEnumerable<MovieRatingResponse>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status401Unauthorized)
                .RequireAuthorization();
            return app;
        }
    }
}
