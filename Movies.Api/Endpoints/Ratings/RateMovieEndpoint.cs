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
                        CancellationToken token) =>
                    {
                        var userId = context.GetUserId();
                        var result = await ratingService.RateMovieAsync(id, userId!.Value, request.Rating, token);
                        return result ? Results.Ok() : Results.NotFound();
                    })
                .WithName(Name);
            return app;
        }
    }
}
