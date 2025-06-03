using Movies.Api.Auth;
using Movies.Api.Mapping;
using Movies.Application.Services;
using Movies.Application.Services.IServices;

namespace Movies.Api.Endpoints.Movies
{
    public static class GetMovieEndpoint
    {
        public static IEndpointRouteBuilder MapGetMovie(this IEndpointRouteBuilder app)
        {
            app.MapGet(ApiEndpoints.Movies.Get, async (
                string idOrSlug, IMovieService movieService
                , HttpContext context, CancellationToken token) =>
            {
                var userId = context.GetUserId();
                var movie = Guid.TryParse(idOrSlug, out var id)
                    ? await movieService.GetByIdAsync(id, userId, token)
                    : await movieService.GetBySlugAsync(idOrSlug, userId, token);

                if (movie == null)
                {
                    return Results.NotFound();
                }
                var response = movie.MapToResponse();
                return TypedResults.Ok(response);
            });
            return app;
        }
    }
}
